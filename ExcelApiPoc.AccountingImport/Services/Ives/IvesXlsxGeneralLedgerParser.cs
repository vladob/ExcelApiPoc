using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Globalization;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXlsxGeneralLedgerParser : IIvesGeneralLedgerSourceParser
    {
        public string TechnicalType => "Excel";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".xlsx",
                       StringComparison.OrdinalIgnoreCase) &&
                   Path.GetFileName(filePath).StartsWith(
                       "HL_KNIHA_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesGeneralLedgerParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            AccountingFileNameMetadata metadata;
            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out metadata) ||
                metadata.DocumentKind !=
                    AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' does not identify an IVES general ledger with IČO and fiscal year.");
            }

            var result = new IvesGeneralLedgerParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                PeriodStart = new DateTime(metadata.FiscalYear, 1, 1),
                PeriodEnd = new DateTime(metadata.FiscalYear, 12, 31)
            };

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                ValidateSingleWorksheet(reader, fileName);
                result.WorksheetName = reader.Name;
                ParseWorksheet(reader, result);
            }

            return result;
        }

        private static void ParseWorksheet(
            IExcelDataReader reader,
            IvesGeneralLedgerParseResult result)
        {
            IvesGeneralLedgerActivity currentActivity = null;
            int sourceRowNumber = 0;
            int documentSequence = 0;
            int accountSequence = 0;
            int syntheticSequence = 0;
            int totalSequence = 0;

            while (reader.Read())
            {
                sourceRowNumber++;

                if (IsEntireRowBlank(reader))
                    continue;

                if (sourceRowNumber == 1 && IsHeaderRow(reader))
                    continue;

                if (IsActivityRow(reader))
                {
                    currentActivity = new IvesGeneralLedgerActivity
                    {
                        Name = Text(reader, 0),
                        Currency = Text(reader, 1)
                    };
                    result.Activities.Add(currentActivity);
                    continue;
                }

                if (currentActivity == null)
                {
                    throw new InvalidDataException(
                        "IVES general-ledger XLSX row " +
                        sourceRowNumber +
                        " contains ledger data before the first activity.");
                }

                if (IsReportTotalRow(reader))
                {
                    currentActivity.ReportTotalRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++totalSequence,
                            SourceRowNumber = sourceRowNumber,
                            Kind = IvesGeneralLedgerRowKind.ReportTotal,
                            OpeningBalance = Amount(
                                reader, 2, sourceRowNumber, "opening balance"),
                            DebitTurnover = Amount(
                                reader, 3, sourceRowNumber, "debit turnover"),
                            CreditTurnover = Amount(
                                reader, 4, sourceRowNumber, "credit turnover"),
                            ClosingBalance = Amount(
                                reader, 5, sourceRowNumber, "closing balance")
                        });
                    continue;
                }

                if (IsSyntheticSummaryRow(reader))
                {
                    currentActivity.SyntheticSummaryRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++syntheticSequence,
                            SourceRowNumber = sourceRowNumber,
                            Kind = IvesGeneralLedgerRowKind.SyntheticSubtotal,
                            AccountCode = ExtractSyntheticCode(Text(reader, 2)),
                            OpeningBalance = Amount(
                                reader, 5, sourceRowNumber, "opening balance"),
                            DebitTurnover = Amount(
                                reader, 6, sourceRowNumber, "debit turnover"),
                            CreditTurnover = Amount(
                                reader, 7, sourceRowNumber, "credit turnover"),
                            ClosingBalance = Amount(
                                reader, 8, sourceRowNumber, "closing balance")
                        });
                    continue;
                }

                if (TryGetDocumentDate(Value(reader, 0), out DateTime date))
                {
                    currentActivity.DocumentRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++documentSequence,
                            SourceRowNumber = sourceRowNumber,
                            Kind = IvesGeneralLedgerRowKind.Document,
                            DocumentDate = date,
                            DocumentNumber = Text(reader, 1),
                            AccountCode = Text(reader, 2),
                            Text = Text(reader, 3),
                            DebitTurnover = Amount(
                                reader, 6, sourceRowNumber, "debit turnover"),
                            CreditTurnover = Amount(
                                reader, 7, sourceRowNumber, "credit turnover")
                        });
                    continue;
                }

                if (IsAnalyticalSummaryRow(reader))
                {
                    currentActivity.AccountRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++accountSequence,
                            SourceRowNumber = sourceRowNumber,
                            Kind = IvesGeneralLedgerRowKind.Account,
                            AccountCode = Text(reader, 2),
                            Text = Text(reader, 3),
                            OpeningBalance = Amount(
                                reader, 4, sourceRowNumber, "opening balance"),
                            DebitTurnover = Amount(
                                reader, 5, sourceRowNumber, "debit turnover"),
                            CreditTurnover = Amount(
                                reader, 6, sourceRowNumber, "credit turnover"),
                            ClosingBalance = Amount(
                                reader, 7, sourceRowNumber, "closing balance")
                        });
                    continue;
                }

                result.UnclassifiedRows.Add(
                    new IvesGeneralLedgerSourceRow
                    {
                        SourceRowNumber = sourceRowNumber,
                        Kind = IvesGeneralLedgerRowKind.Unclassified,
                        AccountCode = Text(reader, 2),
                        Text = Text(reader, 3)
                    });
            }

            result.SourceRowCount = sourceRowNumber;

            if (result.Activities.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger XLSX file contains no recognizable activities.");
            }
        }

        private static bool IsHeaderRow(IExcelDataReader reader)
        {
            return Contains(Text(reader, 0), "Dátum") &&
                Contains(Text(reader, 1), "Doklad") &&
                Contains(Text(reader, 2), "SU");
        }

        private static bool IsActivityRow(IExcelDataReader reader)
        {
            string first = Text(reader, 0);
            string currency = Text(reader, 1);

            if (string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(currency) ||
                string.Equals(first, "-", StringComparison.Ordinal) ||
                IsReportTotalLabel(first))
            {
                return false;
            }

            if (TryGetDocumentDate(Value(reader, 0), out _))
                return false;

            for (int column = 2; column < reader.FieldCount; column++)
            {
                if (HasValue(reader, column))
                    return false;
            }

            return true;
        }

        private static bool IsAnalyticalSummaryRow(IExcelDataReader reader)
        {
            return string.Equals(
                       Text(reader, 0),
                       "-",
                       StringComparison.Ordinal) &&
                   string.Equals(
                       Text(reader, 1),
                       "-",
                       StringComparison.Ordinal) &&
                   !string.IsNullOrWhiteSpace(Text(reader, 2)) &&
                   Text(reader, 2).IndexOf('=') < 0;
        }

        private static bool IsSyntheticSummaryRow(IExcelDataReader reader)
        {
            return string.Equals(
                       Text(reader, 0),
                       "-",
                       StringComparison.Ordinal) &&
                   string.Equals(
                       Text(reader, 1),
                       "-",
                       StringComparison.Ordinal) &&
                   !string.IsNullOrWhiteSpace(Text(reader, 2)) &&
                   Text(reader, 2).IndexOf('=') >= 0 &&
                   string.Equals(
                       Text(reader, 3),
                       "SU",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReportTotalRow(IExcelDataReader reader)
        {
            return IsReportTotalLabel(Text(reader, 0));
        }

        private static bool IsReportTotalLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return string.Equals(
                RemoveWhiteSpace(value),
                "Celkom",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string RemoveWhiteSpace(string value)
        {
            var chars = new char[value.Length];
            int length = 0;

            foreach (char c in value)
            {
                if (!char.IsWhiteSpace(c))
                    chars[length++] = c;
            }

            return new string(chars, 0, length);
        }

        private static decimal Amount(
            IExcelDataReader reader,
            int column,
            int sourceRowNumber,
            string field)
        {
            object value = Value(reader, column);

            if (value == null || value == DBNull.Value)
                return 0m;

            if (value is decimal decimalValue)
                return decimalValue;
            if (value is double doubleValue)
                return Convert.ToDecimal(doubleValue);
            if (value is float floatValue)
                return Convert.ToDecimal(floatValue);
            if (value is int intValue)
                return intValue;
            if (value is long longValue)
                return longValue;

            string text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(text))
                return 0m;

            if (decimal.TryParse(
                    text.Trim(),
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal parsed) ||
                decimal.TryParse(
                    text.Trim(),
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"),
                    out parsed))
            {
                return parsed;
            }

            throw new InvalidDataException(
                "IVES general-ledger XLSX row " +
                sourceRowNumber +
                " contains invalid " + field +
                " '" + text + "'.");
        }

        private static bool TryGetDocumentDate(
            object value,
            out DateTime date)
        {
            if (value is DateTime typedDate)
            {
                date = typedDate.Date;
                return true;
            }

            if (value is double serial &&
                serial >= 1 &&
                serial <= 2958465)
            {
                try
                {
                    date = DateTime.FromOADate(serial).Date;
                    return true;
                }
                catch (ArgumentException)
                {
                }
            }

            string text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture);

            if (DateTime.TryParse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTime parsed))
            {
                date = parsed.Date;
                return true;
            }

            date = default(DateTime);
            return false;
        }

        private static string ExtractSyntheticCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            int separator = value.IndexOf('.');
            if (separator < 0)
                separator = value.IndexOf('=');
            if (separator < 0)
                separator = value.Length;

            return value.Substring(0, separator).Trim();
        }

        private static object Value(
            IExcelDataReader reader,
            int column)
        {
            return column < reader.FieldCount
                ? reader.GetValue(column)
                : null;
        }

        private static string Text(
            IExcelDataReader reader,
            int column)
        {
            if (column >= reader.FieldCount)
                return null;

            string value = ExcelWorkbookReader.GetText(reader, column);
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool HasValue(
            IExcelDataReader reader,
            int column)
        {
            object value = Value(reader, column);
            return value != null &&
                value != DBNull.Value &&
                !(value is string text &&
                  string.IsNullOrWhiteSpace(text));
        }

        private static bool IsEntireRowBlank(IExcelDataReader reader)
        {
            for (int column = 0; column < reader.FieldCount; column++)
            {
                if (HasValue(reader, column))
                    return false;
            }

            return true;
        }

        private static bool Contains(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(
                    expected,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ValidateSingleWorksheet(
            IExcelDataReader reader,
            string fileName)
        {
            if (reader.ResultsCount == 1)
                return;

            throw new InvalidDataException(
                "IVES workbook '" + fileName + "' contains " +
                reader.ResultsCount +
                " worksheets. Exactly one worksheet is required.");
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A source file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES general-ledger XLSX file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES XLSX general-ledger parser accepts only .xlsx files.");
            }
        }
    }
}
