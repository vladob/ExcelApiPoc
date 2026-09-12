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
        private const int DateColumn = 1;
        private const int DocumentColumn = 3;
        private const int AccountColumn = 8;
        private const int TextColumn = 12;
        private const int ReportOpeningColumn = 13;
        private const int AccountOpeningColumn = 15;
        private const int DocumentDebitColumn = 18;
        private const int AccountDebitColumn = 19;
        private const int CreditColumn = 23;
        private const int ClosingColumn = 26;

        private static readonly Regex IcoPattern = new Regex(
            @"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex PeriodPattern = new Regex(
            @"Dátum\s+od\s*:\s*(?<from>\d{1,2}\.\d{1,2}\.\d{4})\s*,\s*" +
            @"Dátum\s+do\s*:\s*(?<to>\d{1,2}\.\d{1,2}\.\d{4})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

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
            int sourceRowNumber = 0;
            int lastMeaningfulSourceRowNumber = 0;
            var sequences = new Dictionary<IvesGeneralLedgerRowKind, int>();

            while (reader.Read())
            {
                sourceRowNumber++;

                if (!IsEntireRowBlank(reader))
                {
                    lastMeaningfulSourceRowNumber = sourceRowNumber;
                }

                ReadMetadata(reader, result);
                IvesGeneralLedgerRowKind kind = Classify(reader, sourceRowNumber, currentSyntheticAccount);
                IvesGeneralLedgerSourceRow row = CreateRow(reader, sourceRowNumber, kind);

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

                AddRow(result, row);
            }

            result.SourceRowCount = lastMeaningfulSourceRowNumber;

            result.StructuralRows.RemoveAll(
                row => row.Kind == IvesGeneralLedgerRowKind.Blank &&
                       row.SourceRowNumber > lastMeaningfulSourceRowNumber);
            ValidateMetadata(result);
        }

        private static IvesGeneralLedgerRowKind Classify(
            IExcelDataReader reader, int sourceRowNumber, string currentSyntheticAccount)
        {
            if (sourceRowNumber <= 11) return IvesGeneralLedgerRowKind.Header;
            if (IsEntireRowBlank(reader)) return IvesGeneralLedgerRowKind.Blank;

            string dateText = Text(reader, DateColumn);
            string document = Text(reader, DocumentColumn);
            string account = Text(reader, AccountColumn);
            string description = Text(reader, TextColumn);

            if (sourceRowNumber == 12 && Contains(dateText, "Dátum") && Contains(document, "Doklad"))
                return IvesGeneralLedgerRowKind.Title;
            if (IsReportTotalLabel(dateText))
                return IvesGeneralLedgerRowKind.ReportTotal;
            if (IsDocumentSummaryLabel(dateText) || IsDocumentSummaryLabel(document) || IsDocumentSummaryLabel(description))
                return IvesGeneralLedgerRowKind.DocumentSummary;
            if (IsSyntheticAccount(account, description))
                return IvesGeneralLedgerRowKind.SyntheticAccount;
            if (!string.IsNullOrWhiteSpace(currentSyntheticAccount) &&
                string.IsNullOrWhiteSpace(account) && HasAnyAmount(reader))
                return IvesGeneralLedgerRowKind.SyntheticSubtotal;
            if (dateText == "-" && document == "-" &&
                !string.IsNullOrWhiteSpace(account) && account.IndexOf('=') < 0)
                return IvesGeneralLedgerRowKind.Account;
            if (TryGetDocumentDate(Value(reader, DateColumn), out _) &&
                !string.IsNullOrWhiteSpace(document) && document != "-" &&
                !string.IsNullOrWhiteSpace(account))
                return IvesGeneralLedgerRowKind.Document;
            if (Contains(dateText, "Hlavná činnosť"))
                return IvesGeneralLedgerRowKind.SectionTitle;

            return IvesGeneralLedgerRowKind.Unclassified;
        }

        private static IvesGeneralLedgerSourceRow CreateRow(
            IExcelDataReader reader, int sourceRowNumber, IvesGeneralLedgerRowKind kind)
        {
            var row = new IvesGeneralLedgerSourceRow
            {
                SourceRowNumber = sourceRowNumber,
                Kind = kind,
                DocumentNumber = Text(reader, DocumentColumn),
                AccountCode = Text(reader, AccountColumn),
                Text = Text(reader, TextColumn)
            };

            if (kind == IvesGeneralLedgerRowKind.Document)
            {
                if (TryGetDocumentDate(Value(reader, DateColumn), out DateTime date)) row.DocumentDate = date;
                row.DebitTurnover = Amount(reader, DocumentDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, CreditColumn, sourceRowNumber);
            }
            else if (kind == IvesGeneralLedgerRowKind.Account || kind == IvesGeneralLedgerRowKind.SyntheticSubtotal)
            {
                row.OpeningBalance = Amount(reader, AccountOpeningColumn, sourceRowNumber);
                row.DebitTurnover = Amount(reader,
                    kind == IvesGeneralLedgerRowKind.Account ? AccountDebitColumn : DocumentDebitColumn,
                    sourceRowNumber);
                row.CreditTurnover = Amount(reader, CreditColumn, sourceRowNumber);
                row.ClosingBalance = Amount(reader, ClosingColumn, sourceRowNumber);
            }
            else if (kind == IvesGeneralLedgerRowKind.ReportTotal)
            {
                row.OpeningBalance = Amount(reader, ReportOpeningColumn, sourceRowNumber);
                row.DebitTurnover = Amount(reader, AccountDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, CreditColumn, sourceRowNumber);
                row.ClosingBalance = Amount(reader, ClosingColumn, sourceRowNumber);
            }
            else if (kind == IvesGeneralLedgerRowKind.DocumentSummary)
            {
                // Exact amount-column validation awaits an original XLS containing this row kind.
                row.DebitTurnover = Amount(reader, DocumentDebitColumn, sourceRowNumber);
                row.CreditTurnover = Amount(reader, CreditColumn, sourceRowNumber);
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
            if (decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out decimal parsed) ||
                decimal.TryParse(text.Trim(), NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"), out parsed))
                return parsed;

            throw new InvalidDataException("IVES source row " + row + ", column " +
                (column + 1) + " contains invalid amount '" + text + "'.");
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
            return !string.IsNullOrWhiteSpace(account) && account.IndexOf('=') >= 0 &&
                string.Equals(description, "SU", StringComparison.OrdinalIgnoreCase);
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
            return !string.IsNullOrWhiteSpace(value) &&
                value.Trim().StartsWith("Spolu za zákazku", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasAnyAmount(IExcelDataReader reader)
        {
            return HasValue(reader, AccountOpeningColumn) || HasValue(reader, DocumentDebitColumn) ||
                HasValue(reader, AccountDebitColumn) || HasValue(reader, CreditColumn) ||
                HasValue(reader, ClosingColumn);
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
            return value != null && value != DBNull.Value &&
                !(value is string text && string.IsNullOrWhiteSpace(text));
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
            return !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static DateTime ParseDate(string value)
        {
            if (DateTime.TryParseExact(value, "d.M.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime result)) return result;
            throw new InvalidDataException("Invalid IVES period date '" + value + "'.");
        }

        private static int NextSequence(
            IDictionary<IvesGeneralLedgerRowKind, int> sequences, IvesGeneralLedgerRowKind kind)
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
    }
}
