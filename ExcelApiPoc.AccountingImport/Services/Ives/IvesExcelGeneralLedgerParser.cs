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
    internal sealed class IvesExcelGeneralLedgerParser
    {
        private static readonly Regex IcoPattern = new Regex(@"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex PeriodPattern = new Regex(@"Dátum\s+od\s*:\s*(?<from>\d{1,2}\.\d{1,2}\.\d{4})\s*,\s*" + @"Dátum\s+do\s*:\s*(?<to>\d{1,2}\.\d{1,2}\.\d{4})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public IvesGeneralLedgerParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            var result = new IvesGeneralLedgerParseResult
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath)
            };

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                ValidateSingleWorksheet(reader, result.SourceFileName);
                result.WorksheetName = reader.Name;
                ParseWorksheet(reader, result);
            }

            return result;
        }

        private static void ParseWorksheet(IExcelDataReader reader, IvesGeneralLedgerParseResult result)
        {
            string currentSyntheticAccount = null;
            IvesGeneralLedgerSourceRow pendingAmountTarget = null;
            ColumnLayout layout = null;
            bool compactLayoutResolved = false;
            int sourceRowNumber = 0;
            int lastMeaningfulSourceRowNumber = 0;
            var sequences = new Dictionary<IvesGeneralLedgerRowKind, int>();

            while (reader.Read())
            {
                sourceRowNumber++;

                if (layout == null)
                {
                    layout = DiscoverLayout(reader, result.SourceFileName);
                    compactLayoutResolved = reader.FieldCount != 28;
                }

                if (!compactLayoutResolved && reader.FieldCount == 28)
                {
                    ColumnLayout discoveredLayout = DiscoverCompactLayout(reader);

                    if (discoveredLayout != null)
                    {
                        layout = discoveredLayout;
                        compactLayoutResolved = true;
                    }
                }

                if (!IsEntireRowBlank(reader))
                {
                    lastMeaningfulSourceRowNumber = sourceRowNumber;
                }

                ReadMetadata(reader, result);
                IvesGeneralLedgerRowKind kind = Classify(reader, sourceRowNumber, currentSyntheticAccount, pendingAmountTarget, layout);
                IvesGeneralLedgerSourceRow row = CreateRow(reader, sourceRowNumber, kind, layout);

                if (kind == IvesGeneralLedgerRowKind.AmountContinuation)
                {
                    if (pendingAmountTarget.Kind == IvesGeneralLedgerRowKind.Account)
                    {
                        MergeAccountAmounts(pendingAmountTarget, reader, sourceRowNumber, layout);
                    }
                    else if (pendingAmountTarget.Kind == IvesGeneralLedgerRowKind.Document)
                    {
                        PopulateDocumentAmounts(pendingAmountTarget, reader, sourceRowNumber, layout);
                    }

                    string continuationText = Text(reader, layout.TextColumn);

                    if (!string.IsNullOrWhiteSpace(continuationText))
                    {
                        pendingAmountTarget.Text = continuationText;
                    }

                    pendingAmountTarget = null;
                }

                if (IsSequencedRecord(kind))
                {
                    row.SequenceNumber = NextSequence(sequences, kind);
                }

                if (kind == IvesGeneralLedgerRowKind.SyntheticAccount)
                {
                    currentSyntheticAccount = ExtractSyntheticCode(row.AccountCode);
                }
                else if (kind == IvesGeneralLedgerRowKind.SyntheticSubtotal)
                {
                    row.AccountCode = currentSyntheticAccount;
                    currentSyntheticAccount = null;
                }
                else if (kind != IvesGeneralLedgerRowKind.Blank)
                {
                    currentSyntheticAccount = null;
                }

                if (kind == IvesGeneralLedgerRowKind.Account && (layout.UsesAmountContinuation || !HasAccountAmounts(reader, layout)))
                {
                    pendingAmountTarget = row;
                }
                else if (kind == IvesGeneralLedgerRowKind.Document && !HasDocumentAmounts(reader, layout))
                {
                    pendingAmountTarget = row;
                }
                else if (kind != IvesGeneralLedgerRowKind.Blank && kind != IvesGeneralLedgerRowKind.AmountContinuation)
                {
                    pendingAmountTarget = null;
                }

                AddRow(result, row);
            }

            result.SourceRowCount = lastMeaningfulSourceRowNumber;

            result.StructuralRows.RemoveAll(row => row.Kind == IvesGeneralLedgerRowKind.Blank && row.SourceRowNumber > lastMeaningfulSourceRowNumber);
            ValidateMetadata(result);
        }

        private static IvesGeneralLedgerRowKind Classify(IExcelDataReader reader, int sourceRowNumber, string currentSyntheticAccount, IvesGeneralLedgerSourceRow pendingAmountTarget, ColumnLayout layout)
        {
            if (sourceRowNumber <= 11)
                return IvesGeneralLedgerRowKind.Header;

            if (IsEntireRowBlank(reader))
                return IvesGeneralLedgerRowKind.Blank;

            string dateText = Text(reader, layout.DateColumn);
            string document = Text(reader, layout.DocumentColumn);
            string account = Text(reader, layout.AccountColumn);
            string description = Text(reader, layout.TextColumn);

            if (Contains(dateText, "Dátum") && IsDocumentHeader(document))
                return IvesGeneralLedgerRowKind.Title;

            if (IsReportTotalLabel(dateText))
                return IvesGeneralLedgerRowKind.ReportTotal;

            if (IsDocumentSummaryLabel(dateText) || IsDocumentSummaryLabel(document) || IsDocumentSummaryLabel(description))
            {
                return IvesGeneralLedgerRowKind.DocumentSummary;
            }

            if (IsSyntheticAccount(account, description))
                return IvesGeneralLedgerRowKind.SyntheticAccount;

            if (!string.IsNullOrWhiteSpace(currentSyntheticAccount) && string.IsNullOrWhiteSpace(account) && HasAnyAmount(reader, layout))
            {
                return IvesGeneralLedgerRowKind.SyntheticSubtotal;
            }

            if (pendingAmountTarget != null && string.IsNullOrWhiteSpace(account) && HasAnyAmount(reader, layout))
            {
                return IvesGeneralLedgerRowKind.AmountContinuation;
            }

            if (dateText == "-" && !string.IsNullOrWhiteSpace(account) && account.IndexOf('=') < 0)
            {
                return IvesGeneralLedgerRowKind.Account;
            }

            if (TryGetDocumentDate(Value(reader, layout.DateColumn), out _) && !string.IsNullOrWhiteSpace(account) && !string.Equals(document, "-", StringComparison.Ordinal))
            {
                return IvesGeneralLedgerRowKind.Document;
            }

            if (Contains(dateText, "Hlavná činnosť"))
                return IvesGeneralLedgerRowKind.SectionTitle;

            return IvesGeneralLedgerRowKind.Unclassified;
        }

        private static IvesGeneralLedgerSourceRow CreateRow(IExcelDataReader reader, int sourceRowNumber, IvesGeneralLedgerRowKind kind, ColumnLayout layout)
        {
            var row = new IvesGeneralLedgerSourceRow
            {
                SourceRowNumber = sourceRowNumber,
                Kind = kind,
                DocumentNumber = Text(reader, layout.DocumentColumn),
                AccountCode = Text(reader, layout.AccountColumn),
                Text = Text(reader, layout.TextColumn)
            };

            if (kind == IvesGeneralLedgerRowKind.Document)
            {
                if (TryGetDocumentDate(Value(reader, layout.DateColumn), out DateTime date))
                {
                    row.DocumentDate = date;
                }
                row.DebitTurnover = Amount(reader, layout.DocumentDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, layout.CreditColumn, sourceRowNumber);
            }
            else if (kind == IvesGeneralLedgerRowKind.Account)
            {
                PopulateAccountAmounts(row, reader, sourceRowNumber, layout);
            }
            else if (kind == IvesGeneralLedgerRowKind.SyntheticSubtotal)
            {
                PopulateAmounts(row, reader, sourceRowNumber, layout, layout.SyntheticSubtotalDebitColumn);
            }
            else if (kind == IvesGeneralLedgerRowKind.ReportTotal)
            {
                row.OpeningBalance = Amount(reader, layout.ReportOpeningColumn, sourceRowNumber);
                row.DebitTurnover = Amount(reader, layout.ReportDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, layout.ReportCreditColumn, sourceRowNumber);
                row.ClosingBalance = Amount(reader, layout.ClosingColumn, sourceRowNumber);
            }
            else if (kind == IvesGeneralLedgerRowKind.DocumentSummary)
            {
                // Exact amount-column validation awaits an original XLS containing this row kind.
                row.DebitTurnover = Amount(reader, layout.DocumentDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, layout.CreditColumn, sourceRowNumber);
            }

            return row;
        }

        private static void ReadMetadata(IExcelDataReader reader, IvesGeneralLedgerParseResult result)
        {
            for (int column = 0; column < reader.FieldCount; column++)
            {
                string value = Text(reader, column);
                if (string.IsNullOrWhiteSpace(value)) continue;

                if (string.IsNullOrWhiteSpace(result.Ico))
                {
                    Match match = IcoPattern.Match(value);
                    if (match.Success) result.Ico = match.Groups["ico"].Value;
                }

                if (!result.PeriodStart.HasValue)
                {
                    Match match = PeriodPattern.Match(value);
                    if (match.Success)
                    {
                        result.PeriodStart = ParseDate(match.Groups["from"].Value);
                        result.PeriodEnd = ParseDate(match.Groups["to"].Value);
                    }
                }
            }
        }

        private static decimal? Amount(IExcelDataReader reader, int column, int row)
        {
            object value = Value(reader, column);
            if (value == null || value == DBNull.Value) return null;
            if (value is decimal d) return d;
            if (value is double db) return Convert.ToDecimal(db);
            if (value is float f) return Convert.ToDecimal(f);
            if (value is int i) return i;
            if (value is long l) return l;

            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal parsed) || decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.GetCultureInfo("sk-SK"), out parsed))
                return parsed;

            throw new InvalidDataException("IVES source row " + row + ", column " + (column + 1) + " contains invalid amount '" + text + "'.");
        }

        private static bool TryGetDocumentDate(object value, out DateTime date)
        {
            if (value is DateTime typedDate) { date = typedDate.Date; return true; }
            if (value is double serial && serial >= 1 && serial <= 2958465)
            {
                try { date = DateTime.FromOADate(serial).Date; return true; }
                catch (ArgumentException) { }
            }
            date = default;
            return false;
        }

        private static bool IsSyntheticAccount(string account, string description)
        {
            return !string.IsNullOrWhiteSpace(account) && account.IndexOf('=') >= 0 && (string.Equals(description, "SU", StringComparison.OrdinalIgnoreCase) || IsSyntheticSubtotalLabel(description));
        }

        private static bool IsSyntheticSubtotalLabel(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith( "Spolu za SU", StringComparison.OrdinalIgnoreCase);
        }

        private static string ExtractSyntheticCode(string account)
        {
            if (string.IsNullOrWhiteSpace(account)) return null;
            int end = account.IndexOf('.');
            if (end < 0) end = account.IndexOf('=');
            if (end < 0) end = account.Length;
            return account.Substring(0, end).Trim();
        }

        private static bool IsReportTotalLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var compact = new StringBuilder();
            foreach (char c in value) if (!char.IsWhiteSpace(c)) compact.Append(c);
            return string.Equals(compact.ToString(), "Celkom", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDocumentSummaryLabel(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith("Spolu za zákazku", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasAnyAmount(IExcelDataReader reader, ColumnLayout layout)
        {
            return HasValue(reader, layout.AccountOpeningColumn) || HasValue(reader, layout.DocumentDebitColumn) || HasValue(reader, layout.AccountDebitColumn) || HasValue(reader, layout.CreditColumn) || HasValue(reader, layout.ClosingColumn);
        }

        private static void PopulateAccountAmounts(IvesGeneralLedgerSourceRow row, IExcelDataReader reader, int sourceRowNumber, ColumnLayout layout)
        {
            PopulateAmounts(row, reader, sourceRowNumber, layout, layout.AccountDebitColumn);
        }

        private static void MergeAccountAmounts(IvesGeneralLedgerSourceRow row, IExcelDataReader reader, int sourceRowNumber, ColumnLayout layout)
        {
            decimal? opening = Amount(reader, layout.AccountOpeningColumn, sourceRowNumber);

            decimal? debit = Amount(reader, layout.AccountDebitColumn, sourceRowNumber);

            decimal? credit = Amount(reader, layout.CreditColumn, sourceRowNumber);

            decimal? closing = Amount(reader, layout.ClosingColumn, sourceRowNumber);

            if (opening.HasValue)
                row.OpeningBalance = opening;

            if (debit.HasValue)
                row.DebitTurnover = debit;

            if (credit.HasValue)
                row.CreditTurnover = credit;

            if (closing.HasValue)
                row.ClosingBalance = closing;
        }

        private static void PopulateAmounts(IvesGeneralLedgerSourceRow row, IExcelDataReader reader, int sourceRowNumber, ColumnLayout layout, int debitColumn)
        {
            row.OpeningBalance = Amount(reader, layout.AccountOpeningColumn, sourceRowNumber);
            row.DebitTurnover = Amount(reader, debitColumn, sourceRowNumber);
            row.CreditTurnover = Amount(reader, layout.CreditColumn, sourceRowNumber);
            row.ClosingBalance = Amount(reader, layout.ClosingColumn, sourceRowNumber);
        }

        private static ColumnLayout DiscoverLayout(IExcelDataReader reader, string sourceFileName)
        {
            if (reader.FieldCount == 28)
            {
                //return DiscoverCompactLayout(reader);
                return ColumnLayout.Compact;
            }

            if (reader.FieldCount == 37)
            {
                return ColumnLayout.Wide;
            }

            throw new InvalidDataException("IVES general-ledger column layout could not be discovered " + "for '" + sourceFileName + "'. Expected 28 or 37 columns, " + "found " + reader.FieldCount + ".");
        }

        private static bool IsEntireRowBlank(IExcelDataReader reader)
        {
            for (int column = 0; column < reader.FieldCount; column++)
                if (HasValue(reader, column)) return false;
            return true;
        }

        private static bool HasValue(IExcelDataReader reader, int column)
        {
            object value = Value(reader, column);
            return value != null && value != DBNull.Value && !(value is string text && string.IsNullOrWhiteSpace(text));
        }

        private static object Value(IExcelDataReader reader, int column)
        {
            return column < reader.FieldCount ? reader.GetValue(column) : null;
        }

        private static string Text(IExcelDataReader reader, int column)
        {
            if (column >= reader.FieldCount) return null;
            string value = ExcelWorkbookReader.GetText(reader, column);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool Contains(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static DateTime ParseDate(string value)
        {
            if (DateTime.TryParseExact(value, "d.M.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)) return result;
            throw new InvalidDataException("Invalid IVES period date '" + value + "'.");
        }

        private static int NextSequence(IDictionary<IvesGeneralLedgerRowKind, int> sequences, IvesGeneralLedgerRowKind kind)
        {
            sequences.TryGetValue(kind, out int value);
            sequences[kind] = ++value;
            return value;
        }

        private static bool IsSequencedRecord(IvesGeneralLedgerRowKind kind)
        {
            return kind == IvesGeneralLedgerRowKind.Account ||
                kind == IvesGeneralLedgerRowKind.Document ||
                kind == IvesGeneralLedgerRowKind.DocumentSummary ||
                kind == IvesGeneralLedgerRowKind.SyntheticAccount ||
                kind == IvesGeneralLedgerRowKind.SyntheticSubtotal ||
                kind == IvesGeneralLedgerRowKind.ReportTotal;
        }

        private static void AddRow(IvesGeneralLedgerParseResult result, IvesGeneralLedgerSourceRow row)
        {
            switch (row.Kind)
            {
                case IvesGeneralLedgerRowKind.Account: result.AccountRows.Add(row); break;
                case IvesGeneralLedgerRowKind.Document: result.DocumentRows.Add(row); break;
                case IvesGeneralLedgerRowKind.DocumentSummary: result.DocumentSummaryRows.Add(row); break;
                case IvesGeneralLedgerRowKind.SyntheticAccount: result.SyntheticAccountRows.Add(row); break;
                case IvesGeneralLedgerRowKind.SyntheticSubtotal: result.SyntheticSubtotalRows.Add(row); break;
                case IvesGeneralLedgerRowKind.ReportTotal: result.ReportTotalRows.Add(row); break;
                case IvesGeneralLedgerRowKind.Unclassified: result.UnclassifiedRows.Add(row); break;
                default: result.StructuralRows.Add(row); break;
            }
        }

        private sealed class ColumnLayout
        {
            public static readonly ColumnLayout Compact = new ColumnLayout
            {
                DateColumn = 1,
                DocumentColumn = 3,
                AccountColumn = 8,
                TextColumn = 12,
                ReportOpeningColumn = 13,
                AccountOpeningColumn = 15,
                DocumentCreditColumn = 23,
                DocumentDebitColumn = 18,
                AccountDebitColumn = 19,
                SyntheticSubtotalDebitColumn = 18,
                ReportDebitColumn = 19,
                CreditColumn = 23,
                ClosingColumn = 26,
                UsesAmountContinuation = false
            };

            public static readonly ColumnLayout CompactShifted = new ColumnLayout
            {
                DateColumn = 1,
                DocumentColumn = 3,
                AccountColumn = 7,
                TextColumn = 12,
                ReportOpeningColumn = 15,
                AccountOpeningColumn = 15,
                DocumentCreditColumn = 22,
                DocumentDebitColumn = 19,
                AccountDebitColumn = 19,
                SyntheticSubtotalDebitColumn = 19,
                ReportDebitColumn = 20,
                ReportCreditColumn = 23,
                CreditColumn = 22,
                ClosingColumn = 25,
                UsesAmountContinuation = true
            };

            public static readonly ColumnLayout Wide = new ColumnLayout
            {
                DateColumn = 1,
                DocumentColumn = 2,
                AccountColumn = 7,
                TextColumn = 11,
                ReportOpeningColumn = 16,
                AccountOpeningColumn = 16,
                DocumentCreditColumn = 31,
                DocumentDebitColumn = 26,
                AccountDebitColumn = 26,
                SyntheticSubtotalDebitColumn = 26,
                ReportDebitColumn = 27,
                CreditColumn = 31,
                ReportCreditColumn = 31,
                ClosingColumn = 35,
                UsesAmountContinuation = true
            };

            public int DateColumn { get; private set; }
            public int DocumentColumn { get; private set; }
            public int AccountColumn { get; private set; }
            public int TextColumn { get; private set; }
            public int ReportOpeningColumn { get; private set; }
            public int AccountOpeningColumn { get; private set; }
            public int DocumentDebitColumn { get; private set; }
            public int AccountDebitColumn { get; private set; }
            public int SyntheticSubtotalDebitColumn { get; private set; }
            public int ReportDebitColumn { get; private set; }
            public int CreditColumn { get; private set; }
            public int ClosingColumn { get; private set; }
            public bool UsesAmountContinuation { get; private set; }
            public int ReportCreditColumn { get; private set; }
            public int DocumentCreditColumn { get; private set; }
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("An IVES general-ledger source file is required.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The IVES general-ledger source file was not found.", filePath);
            if (!string.Equals(Path.GetExtension(filePath), ".xls", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The IVES general-ledger parser requires an original .xls file.");
        }

        private static void ValidateMetadata(IvesGeneralLedgerParseResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Ico))
                throw new InvalidDataException("The IVES workbook does not contain a recognizable IČO.");
            if (!result.PeriodStart.HasValue || !result.PeriodEnd.HasValue)
                throw new InvalidDataException("The IVES workbook does not contain a recognizable fiscal period.");
            if (result.PeriodStart.Value.Year != result.PeriodEnd.Value.Year)
                throw new InvalidDataException("The IVES general-ledger period spans more than one fiscal year.");
            result.FiscalYear = result.PeriodEnd.Value.Year;
        }

        private static void ValidateSingleWorksheet(IExcelDataReader reader, string fileName)
        {
            if (reader.ResultsCount == 1) return;
            var names = new List<string>();
            do { names.Add(reader.Name ?? "(unnamed)"); } while (reader.NextResult());
            throw new InvalidDataException("IVES workbook '" + fileName + "' contains " +
                reader.ResultsCount + " worksheets: " + string.Join(", ", names) +
                ". Exactly one worksheet is required.");
        }

        private static bool IsDocumentHeader(string value)
        {
            return Contains(value, "Doklad") || Contains(value, "Dokl.");
        }

        private static void PopulateDocumentAmounts(IvesGeneralLedgerSourceRow row, IExcelDataReader reader, int sourceRowNumber, ColumnLayout layout)
        {
            row.DebitTurnover = Amount(reader, layout.DocumentDebitColumn, sourceRowNumber);
            row.CreditTurnover = Amount(reader, layout.DocumentCreditColumn, sourceRowNumber);
        }

        private static bool HasAccountAmounts(IExcelDataReader reader, ColumnLayout layout)
        {
            return
                HasValue(reader, layout.AccountOpeningColumn) ||
                HasValue(reader, layout.AccountDebitColumn) ||
                HasValue(reader, layout.CreditColumn) ||
                HasValue(reader, layout.ClosingColumn);
        }

        private static bool HasDocumentAmounts(IExcelDataReader reader, ColumnLayout layout)
        {
            return
                HasValue(reader, layout.DocumentDebitColumn) ||
                HasValue(reader, layout.CreditColumn);
        }

        private static ColumnLayout DiscoverCompactLayout(IExcelDataReader reader)
        {
            int dateColumn = -1;
            int documentColumn = -1;
            int accountColumn = -1;

            for (int column = 0; column < reader.FieldCount; column++)
            {
                string value = Text(reader, column);

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (Contains(value, "Dátum"))
                {
                    dateColumn = column;
                    continue;
                }

                if (IsDocumentHeader(value))
                {
                    documentColumn = column;
                    continue;
                }

                string normalized = value.Trim();

                if (normalized.StartsWith("SU", StringComparison.OrdinalIgnoreCase) && normalized.IndexOf( "1...4", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    accountColumn = column;
                }
            }

            // Do not infer a layout from ordinary data rows.
            if (dateColumn < 0 || documentColumn < 0 || accountColumn < 0)
            {
                return null;
            }

            if (accountColumn == ColumnLayout.Compact.AccountColumn)
            {
                return ColumnLayout.Compact;
            }

            if (accountColumn == ColumnLayout.CompactShifted.AccountColumn)
            {
                return ColumnLayout.CompactShifted;
            }

            throw new InvalidDataException("IVES compact general-ledger layout could not be " + "recognized. The account header was found in column " + (accountColumn + 1).ToString(CultureInfo.InvariantCulture) + ".");
        }



    }
}
