using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesExcelJournalParser : IIvesJournalSourceParser
    {
        public string TechnicalType => "Excel";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".xls",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static readonly Regex IcoPattern = new Regex(
            @"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex PeriodPattern = new Regex(
            @"od\s*:\s*(?<from>\d{1,2}\.\d{1,2}\.\d{4})\s*,?\s*" +
            @"do\s*:\s*(?<to>\d{1,2}\.\d{1,2}\.\d{4})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public IvesJournalParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            var result = new IvesJournalParseResult
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath)
            };

            var rows = new List<object[]>();
            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                ValidateSingleWorksheet(reader, result.SourceFileName);
                result.WorksheetName = reader.Name;

                while (reader.Read())
                {
                    var values = new object[reader.FieldCount];
                    for (int column = 0; column < reader.FieldCount; column++)
                    {
                        values[column] = reader.GetValue(column);
                    }
                    rows.Add(values);
                }
            }

            ParseRows(rows, result);
            return result;
        }

        private static void ParseRows(
            List<object[]> rows,
            IvesJournalParseResult result)
        {
            int lastMeaningfulIndex = FindLastMeaningfulIndex(rows);
            if (lastMeaningfulIndex < 0)
            {
                throw new InvalidDataException("IVES journal workbook is empty.");
            }

            result.SourceRowCount = lastMeaningfulIndex + 1;
            ReadMetadata(rows, lastMeaningfulIndex, result);
            ValidateMetadata(result);

            IvesJournalColumnLayout layout = DiscoverLayout(
                rows,
                lastMeaningfulIndex,
                result.PeriodStart.Value,
                result.PeriodEnd.Value);
            result.Layout = layout;

            var transactionIndices = FindTransactionIndices(
                rows,
                layout,
                result.PeriodStart.Value,
                result.PeriodEnd.Value,
                requireAmount: true);

            if (transactionIndices.Count == 0)
            {
                throw new InvalidDataException(
                    "IVES journal contains no transaction rows matching the discovered layout.");
            }

            var transactionSet = new HashSet<int>(transactionIndices);
            var moduleSet = new HashSet<int>();
            int transactionSequence = 0;
            int moduleSequence = 0;
            int totalSequence = 0;

            foreach (int transactionIndex in transactionIndices)
            {
                int moduleIndex = transactionIndex + 1;
                if (moduleIndex > lastMeaningfulIndex || transactionSet.Contains(moduleIndex))
                {
                    throw PairingError(transactionIndex);
                }

                string module = JoinValues(rows[moduleIndex], layout.TextColumn);
                if (!IsModuleRow(rows[moduleIndex], layout, module))
                {
                    throw PairingError(transactionIndex);
                }

                moduleSet.Add(moduleIndex);
                result.TransactionRows.Add(CreateTransactionRow(
                    rows[transactionIndex],
                    transactionIndex,
                    moduleIndex,
                    ++transactionSequence,
                    module,
                    layout));
                result.ModuleRows.Add(new IvesJournalSourceRow
                {
                    SequenceNumber = ++moduleSequence,
                    SourceRowNumber = moduleIndex + 1,
                    RelatedSourceRowNumber = transactionIndex + 1,
                    Kind = IvesJournalRowKind.Module,
                    Text = module,
                    Module = module
                });
            }

            for (int index = 0; index <= lastMeaningfulIndex; index++)
            {
                if (transactionSet.Contains(index) || moduleSet.Contains(index))
                {
                    continue;
                }

                object[] values = rows[index];
                IvesJournalRowKind kind = ClassifyRemainingRow(values, index, layout);
                var row = new IvesJournalSourceRow
                {
                    SourceRowNumber = index + 1,
                    Kind = kind,
                    Text = JoinValues(values, 0)
                };

                if (kind == IvesJournalRowKind.ReportTotal)
                {
                    row.SequenceNumber = ++totalSequence;
                    AddReportedAmounts(row, values);
                    result.ReportTotalRows.Add(row);
                }
                else if (kind == IvesJournalRowKind.Unclassified)
                {
                    result.UnclassifiedRows.Add(row);
                }
                else
                {
                    result.StructuralRows.Add(row);
                }
            }
        }

        private static IvesJournalColumnLayout DiscoverLayout(
            List<object[]> rows,
            int lastMeaningfulIndex,
            DateTime periodStart,
            DateTime periodEnd)
        {
            for (int rowIndex = 0; rowIndex <= lastMeaningfulIndex; rowIndex++)
            {
                object[] row = rows[rowIndex];
                int dateColumn = FindColumn(row, value => EqualsLabel(value, "Dátum"));
                int documentColumn = FindColumn(row, value => ContainsLabel(value, "dokladu"));
                int currencyColumn = FindColumn(row, value => EqualsLabel(value, "Mena"));
                int textColumn = FindColumn(row, value => EqualsLabel(value, "Text"));
                List<int> accountColumns = FindColumns(row, IsAccountHeader);

                if (dateColumn < 0 || documentColumn < 0 || currencyColumn < 0 ||
                    textColumn < 0 || accountColumns.Count != 2)
                {
                    continue;
                }

                int debitColumn = accountColumns[0];
                int creditColumn = accountColumns[1];
                if (!(dateColumn < documentColumn && documentColumn < debitColumn &&
                      debitColumn < creditColumn && creditColumn < currencyColumn &&
                      currencyColumn < textColumn))
                {
                    continue;
                }

                var provisional = new IvesJournalColumnLayout
                {
                    ColumnCount = row.Length,
                    HeaderRowNumber = rowIndex + 1,
                    DateColumn = dateColumn,
                    DocumentNumberColumn = documentColumn,
                    DebitAccountColumn = debitColumn,
                    CreditAccountColumn = creditColumn,
                    CurrencyColumn = currencyColumn,
                    TextColumn = textColumn
                };

                List<int> candidates = FindTransactionIndices(
                    rows, provisional, periodStart, periodEnd, requireAmount: false);
                provisional.AmountColumn = DiscoverAmountColumn(rows, provisional, candidates);
                return provisional;
            }

            throw new InvalidDataException(
                "IVES journal column layout could not be discovered from the report header.");
        }

        private static int DiscoverAmountColumn(
            List<object[]> rows,
            IvesJournalColumnLayout layout,
            List<int> candidateRows)
        {
            if (candidateRows.Count == 0)
            {
                throw new InvalidDataException(
                    "IVES journal contains no candidate transaction rows for amount-column discovery.");
            }

            var amountColumns = new List<int>();
            for (int column = layout.CreditAccountColumn + 1;
                 column < layout.CurrencyColumn;
                 column++)
            {
                bool populatedOnEveryTransaction = true;
                foreach (int rowIndex in candidateRows)
                {
                    if (!TryGetDecimal(Value(rows[rowIndex], column), out _))
                    {
                        populatedOnEveryTransaction = false;
                        break;
                    }
                }

                if (populatedOnEveryTransaction)
                {
                    amountColumns.Add(column);
                }
            }

            if (amountColumns.Count != 1)
            {
                throw new InvalidDataException(
                    "IVES journal amount column is ambiguous. Expected one numeric column " +
                    "between the credit account and currency columns, found " +
                    amountColumns.Count + ".");
            }

            return amountColumns[0];
        }

        private static List<int> FindTransactionIndices(
            List<object[]> rows,
            IvesJournalColumnLayout layout,
            DateTime periodStart,
            DateTime periodEnd,
            bool requireAmount)
        {
            var indices = new List<int>();
            for (int index = layout.HeaderRowNumber; index < rows.Count; index++)
            {
                object[] row = rows[index];
                if (!TryGetDate(Value(row, layout.DateColumn), out DateTime date) ||
                    date < periodStart || date > periodEnd ||
                    string.IsNullOrWhiteSpace(Text(row, layout.DocumentNumberColumn)))
                {
                    continue;
                }

                string debit = Text(row, layout.DebitAccountColumn);
                string credit = Text(row, layout.CreditAccountColumn);
                if (string.IsNullOrWhiteSpace(debit) && string.IsNullOrWhiteSpace(credit))
                {
                    continue;
                }

                if (!IsCompositeAccount(debit) || !IsCompositeAccount(credit))
                {
                    throw new InvalidDataException(
                        "IVES journal source row " + (index + 1) +
                        " contains an invalid composite account.");
                }

                if (requireAmount &&
                    !TryGetDecimal(Value(row, layout.AmountColumn), out _))
                {
                    throw new InvalidDataException(
                        "IVES journal source row " + (index + 1) +
                        " contains no valid amount in discovered column " +
                        (layout.AmountColumn + 1) + ".");
                }

                indices.Add(index);
            }
            return indices;
        }

        private static IvesJournalSourceRow CreateTransactionRow(
            object[] values,
            int transactionIndex,
            int moduleIndex,
            int sequence,
            string module,
            IvesJournalColumnLayout layout)
        {
            TryGetDate(Value(values, layout.DateColumn), out DateTime date);
            TryGetDecimal(Value(values, layout.AmountColumn), out decimal amount);

            return new IvesJournalSourceRow
            {
                SequenceNumber = sequence,
                SourceRowNumber = transactionIndex + 1,
                RelatedSourceRowNumber = moduleIndex + 1,
                Kind = IvesJournalRowKind.Transaction,
                PostingDate = date,
                DocumentNumber = Text(values, layout.DocumentNumberColumn),
                DebitCompositeAccount = Text(values, layout.DebitAccountColumn),
                CreditCompositeAccount = Text(values, layout.CreditAccountColumn),
                Amount = amount,
                Currency = Text(values, layout.CurrencyColumn),
                Text = Text(values, layout.TextColumn),
                Module = module
            };
        }

        private static bool IsModuleRow(
            object[] row,
            IvesJournalColumnLayout layout,
            string module)
        {
            if (string.IsNullOrWhiteSpace(module) ||
                TryGetDate(Value(row, layout.DateColumn), out _) ||
                !string.IsNullOrWhiteSpace(Text(row, layout.DocumentNumberColumn)) ||
                !string.IsNullOrWhiteSpace(Text(row, layout.DebitAccountColumn)) ||
                !string.IsNullOrWhiteSpace(Text(row, layout.CreditAccountColumn)) ||
                !string.IsNullOrWhiteSpace(Text(row, layout.CurrencyColumn)))
            {
                return false;
            }

            for (int column = 0; column < layout.TextColumn; column++)
            {
                if (!IsBlank(Value(row, column))) return false;
            }
            return true;
        }

        private static IvesJournalRowKind ClassifyRemainingRow(
            object[] row,
            int rowIndex,
            IvesJournalColumnLayout layout)
        {
            if (IsBlankRow(row)) return IvesJournalRowKind.Blank;
            string dateText = Text(row, layout.DateColumn);
            if (ContainsLabel(dateText, "Spolu")) return IvesJournalRowKind.ReportTotal;
            if (rowIndex + 1 == layout.HeaderRowNumber) return IvesJournalRowKind.Header;
            if (ContainsAny(row, "Stredisko") || ContainsAny(row, "Zákazka") ||
                ContainsAny(row, "Modul")) return IvesJournalRowKind.Header;
            if (ContainsAny(row, "Účtovný denník")) return IvesJournalRowKind.Title;
            if (ContainsLabel(dateText, "Hlavná činnosť")) return IvesJournalRowKind.SectionTitle;
            if (rowIndex < layout.HeaderRowNumber) return IvesJournalRowKind.Header;
            return IvesJournalRowKind.Unclassified;
        }

        private static void ReadMetadata(
            List<object[]> rows,
            int lastMeaningfulIndex,
            IvesJournalParseResult result)
        {
            for (int rowIndex = 0; rowIndex <= lastMeaningfulIndex; rowIndex++)
            {
                foreach (object value in rows[rowIndex])
                {
                    string text = Text(value);
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    if (string.IsNullOrWhiteSpace(result.Ico))
                    {
                        Match icoMatch = IcoPattern.Match(text);
                        if (icoMatch.Success) result.Ico = icoMatch.Groups["ico"].Value;
                    }

                    if (!result.PeriodStart.HasValue)
                    {
                        Match periodMatch = PeriodPattern.Match(text);
                        if (periodMatch.Success)
                        {
                            result.PeriodStart = ParseDate(periodMatch.Groups["from"].Value);
                            result.PeriodEnd = ParseDate(periodMatch.Groups["to"].Value);
                        }
                    }
                }
            }
        }

        private static void ValidateMetadata(IvesJournalParseResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Ico))
                throw new InvalidDataException("IVES journal IČO could not be identified.");
            if (!result.PeriodStart.HasValue || !result.PeriodEnd.HasValue)
                throw new InvalidDataException("IVES journal fiscal period could not be identified.");
            if (result.PeriodStart.Value > result.PeriodEnd.Value)
                throw new InvalidDataException("IVES journal fiscal period is invalid.");
            if (result.PeriodStart.Value.Year != result.PeriodEnd.Value.Year)
                throw new InvalidDataException("IVES journal fiscal period spans multiple years.");
            result.FiscalYear = result.PeriodStart.Value.Year;
        }

        private static void AddReportedAmounts(IvesJournalSourceRow row, object[] values)
        {
            foreach (object value in values)
            {
                if (TryGetDecimal(value, out decimal amount)) row.ReportedAmounts.Add(amount);
            }
        }

        private static bool IsCompositeAccount(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            string[] parts = value.Trim().Split('.');
            if (parts.Length != 8 || parts[0].Length != 3) return false;
            foreach (char character in parts[0])
            {
                if (!char.IsDigit(character)) return false;
            }
            return true;
        }

        private static int FindLastMeaningfulIndex(List<object[]> rows)
        {
            for (int index = rows.Count - 1; index >= 0; index--)
            {
                if (!IsBlankRow(rows[index])) return index;
            }
            return -1;
        }

        private static string JoinValues(object[] row, int startColumn)
        {
            var builder = new StringBuilder();
            for (int column = startColumn; column < row.Length; column++)
            {
                string value = Text(row, column);
                if (string.IsNullOrWhiteSpace(value)) continue;
                if (builder.Length > 0) builder.Append(' ');
                builder.Append(value.Trim());
            }
            return builder.Length == 0 ? null : builder.ToString();
        }

        private static int FindColumn(object[] row, Func<string, bool> predicate)
        {
            for (int column = 0; column < row.Length; column++)
            {
                if (predicate(Text(row, column))) return column;
            }
            return -1;
        }

        private static List<int> FindColumns(object[] row, Func<string, bool> predicate)
        {
            var result = new List<int>();
            for (int column = 0; column < row.Length; column++)
            {
                if (predicate(Text(row, column))) result.Add(column);
            }
            return result;
        }

        private static bool IsAccountHeader(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string trimmed = value.TrimStart();
            return trimmed.Length > 2 &&
                trimmed.StartsWith("SU", StringComparison.OrdinalIgnoreCase) &&
                char.IsWhiteSpace(trimmed[2]);
        }

        private static bool ContainsAny(object[] row, string value)
        {
            foreach (object cell in row)
            {
                if (ContainsLabel(Text(cell), value)) return true;
            }
            return false;
        }

        private static bool EqualsLabel(string actual, string expected)
        {
            return string.Equals(
                actual?.Trim(),
                expected,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsLabel(string actual, string expected)
        {
            return actual != null &&
                actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsBlankRow(object[] row)
        {
            foreach (object value in row)
            {
                if (!IsBlank(value)) return false;
            }
            return true;
        }

        private static bool IsBlank(object value)
        {
            if (value == null || value == DBNull.Value) return true;
            return value is string text && string.IsNullOrWhiteSpace(text);
        }

        private static string Text(object[] row, int column)
        {
            return Text(Value(row, column));
        }

        private static string Text(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (value is string text) return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            if (value is DateTime date) return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static object Value(object[] row, int column)
        {
            return column >= 0 && column < row.Length ? row[column] : null;
        }

        private static bool TryGetDate(object value, out DateTime date)
        {
            if (value is DateTime typedDate)
            {
                date = typedDate.Date;
                return true;
            }
            if (value is double serial && serial >= 1 && serial <= 2958465)
            {
                try
                {
                    date = DateTime.FromOADate(serial).Date;
                    return true;
                }
                catch (ArgumentException) { }
            }
            if (value is string text && DateTime.TryParse(
                text.Trim(), CultureInfo.GetCultureInfo("sk-SK"),
                DateTimeStyles.None, out DateTime parsed))
            {
                date = parsed.Date;
                return true;
            }
            date = default;
            return false;
        }

        private static bool TryGetDecimal(object value, out decimal amount)
        {
            if (value is decimal decimalValue) { amount = decimalValue; return true; }
            if (value is double doubleValue) { amount = Convert.ToDecimal(doubleValue); return true; }
            if (value is float floatValue) { amount = Convert.ToDecimal(floatValue); return true; }
            if (value is int intValue) { amount = intValue; return true; }
            if (value is long longValue) { amount = longValue; return true; }
            if (value is string text)
            {
                return decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                           CultureInfo.InvariantCulture, out amount) ||
                       decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                           CultureInfo.GetCultureInfo("sk-SK"), out amount);
            }
            amount = 0m;
            return false;
        }

        private static DateTime ParseDate(string value)
        {
            return DateTime.ParseExact(
                value.Trim(), "d.M.yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None);
        }

        private static InvalidDataException PairingError(int transactionIndex)
        {
            return new InvalidDataException(
                "IVES journal transaction on source row " + (transactionIndex + 1) +
                " is not followed by a recognizable module row.");
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Source file path is required.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("IVES journal source file was not found.", filePath);
            if (!string.Equals(Path.GetExtension(filePath), ".xls", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("IVES journal parser supports original .xls files only.");
        }

        private static void ValidateSingleWorksheet(IExcelDataReader reader, string sourceFileName)
        {
            if (reader.ResultsCount == 1) return;
            var names = new List<string>();
            do { names.Add(reader.Name ?? "(unnamed)"); }
            while (reader.NextResult());
            throw new InvalidDataException(
                "IVES journal workbook '" + sourceFileName + "' contains " +
                reader.ResultsCount + " worksheets: " + string.Join(", ", names) +
                ". Exactly one worksheet is required.");
        }
    }
}
