using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Globalization;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXlsxAccountingFrameworkParser : IIvesAccountingFrameworkSourceParser
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
                       "UCT_ROZVRH_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesAccountingFrameworkParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            AccountingFileNameMetadata metadata;
            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out metadata) ||
                metadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingFramework)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' does not identify an IVES accounting framework with IČO and fiscal year.");
            }

            var result = new IvesAccountingFrameworkParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear
            };

            using (IExcelDataReader reader =
                   ExcelWorkbookReader.Open(filePath))
            {
                ValidateSingleWorksheet(reader, fileName);
                ParseWorksheet(reader, result);
            }

            return result;
        }

        private static void ParseWorksheet(
            IExcelDataReader reader,
            IvesAccountingFrameworkParseResult result)
        {
            int sourceRowNumber = 0;
            int sequenceNumber = 0;

            while (reader.Read())
            {
                sourceRowNumber++;

                if (IsEntireRowBlank(reader))
                    continue;

                if (sourceRowNumber == 1)
                {
                    ValidateHeader(reader);
                    continue;
                }

                string sourceAccountCode = Text(reader, 0);
                if (string.IsNullOrWhiteSpace(sourceAccountCode))
                {
                    throw new InvalidDataException(
                        "IVES accounting-framework XLSX row " +
                        sourceRowNumber +
                        " contains no account code.");
                }

                result.Rows.Add(new IvesAccountingFrameworkSourceRow
                {
                    SequenceNumber = ++sequenceNumber,
                    SourceRowNumber = sourceRowNumber,
                    SourceAccountCode = sourceAccountCode,
                    AccountCode =
                        AccountCodeNormalizer.Normalize(sourceAccountCode),
                    AccountName = Text(reader, 1),
                    ActivityCode = Text(reader, 2),
                    Type = Text(reader, 3),
                    PsFlag = Text(reader, 4),
                    BuFlag = Text(reader, 5),
                    PlFlag = Text(reader, 6),
                    RuFlag = Text(reader, 7),
                    Currency = Text(reader, 8),
                    ValidFrom = Date(
                        reader,
                        9,
                        sourceRowNumber,
                        "valid-from date"),
                    ValidTo = Date(
                        reader,
                        10,
                        sourceRowNumber,
                        "valid-to date")
                });
            }

            result.SourceRowCount = sourceRowNumber;

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework XLSX file contains no account rows.");
            }
        }

        private static void ValidateHeader(IExcelDataReader reader)
        {
            if (!Contains(Text(reader, 0), "SU") ||
                !Contains(Text(reader, 1), "Názov účtu") ||
                !Contains(Text(reader, 2), "Činn") ||
                !Contains(Text(reader, 3), "Typ") ||
                !Contains(Text(reader, 4), "PS") ||
                !Contains(Text(reader, 5), "BU") ||
                !Contains(Text(reader, 6), "PL") ||
                !Contains(Text(reader, 7), "RU") ||
                !Contains(Text(reader, 8), "Mena") ||
                !Contains(Text(reader, 9), "Platný"))
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework XLSX header is not recognized.");
            }
        }

        private static DateTime? Date(
            IExcelDataReader reader,
            int column,
            int sourceRowNumber,
            string fieldName)
        {
            object value = Value(reader, column);

            if (value == null ||
                value == DBNull.Value ||
                (value is string blank &&
                 string.IsNullOrWhiteSpace(blank)))
            {
                return null;
            }

            if (value is DateTime typedDate)
                return typedDate.Date;

            if (value is double serial)
            {
                try
                {
                    return DateTime.FromOADate(serial).Date;
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
                    out DateTime parsed) ||
                DateTime.TryParse(
                    text,
                    CultureInfo.GetCultureInfo("sk-SK"),
                    DateTimeStyles.AllowWhiteSpaces,
                    out parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "IVES accounting-framework XLSX row " +
                sourceRowNumber +
                " contains invalid " + fieldName +
                " '" + text + "'.");
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

            string value =
                ExcelWorkbookReader.GetText(reader, column);

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static bool IsEntireRowBlank(
            IExcelDataReader reader)
        {
            for (int column = 0;
                 column < reader.FieldCount;
                 column++)
            {
                object value = reader.GetValue(column);

                if (value != null &&
                    value != DBNull.Value &&
                    !(value is string text &&
                      string.IsNullOrWhiteSpace(text)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool Contains(
            string value,
            string expected)
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
                    "The IVES accounting-framework XLSX file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES XLSX accounting-framework parser accepts only .xlsx files.");
            }
        }
    }
}
