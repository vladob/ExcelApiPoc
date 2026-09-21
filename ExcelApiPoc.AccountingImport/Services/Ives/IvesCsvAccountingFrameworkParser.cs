using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesCsvAccountingFrameworkParser : IIvesAccountingFrameworkSourceParser
    {
        private const int MinimumFieldCount = 29;

        public string TechnicalType => "CSV";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".csv",
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

            ParseRecords(filePath, result);

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework CSV file contains no account rows.");
            }

            return result;
        }

        private static void ParseRecords(
            string filePath,
            IvesAccountingFrameworkParseResult result)
        {
            int sourceRecordNumber = 0;
            int sequenceNumber = 0;

            foreach (string[] fields in ReadRecords(filePath))
            {
                sourceRecordNumber++;

                if (fields.Length < MinimumFieldCount)
                {
                    throw new InvalidDataException(
                        "IVES accounting-framework CSV record " +
                        sourceRecordNumber +
                        " has " + fields.Length +
                        " fields; at least " +
                        MinimumFieldCount + " are required.");
                }

                ValidateRepeatedHeader(
                    fields,
                    sourceRecordNumber);

                string contentIco =
                    ExtractIco(Text(fields, 3));

                if (!string.Equals(
                        contentIco,
                        result.Ico,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        "IVES accounting-framework CSV record " +
                        sourceRecordNumber +
                        " identifies IČO '" +
                        contentIco +
                        "', but the filename identifies IČO '" +
                        result.Ico + "'.");
                }

                string sourceAccountCode = Text(fields, 18);

                if (string.IsNullOrWhiteSpace(sourceAccountCode))
                {
                    throw new InvalidDataException(
                        "IVES accounting-framework CSV record " +
                        sourceRecordNumber +
                        " contains no account code.");
                }

                result.Rows.Add(new IvesAccountingFrameworkSourceRow
                {
                    SequenceNumber = ++sequenceNumber,
                    SourceRowNumber = sourceRecordNumber,
                    SourceAccountCode = sourceAccountCode,
                    AccountCode =
                        AccountCodeNormalizer.Normalize(sourceAccountCode),
                    AccountName = Text(fields, 19),
                    ActivityCode = Text(fields, 20),
                    Type = Text(fields, 21),
                    PsFlag = Text(fields, 22),
                    BuFlag = Text(fields, 23),
                    PlFlag = Text(fields, 24),
                    RuFlag = Text(fields, 25),
                    Currency = Text(fields, 26),
                    ValidFrom = ParseOptionalDate(
                        Text(fields, 27),
                        sourceRecordNumber,
                        "valid-from date"),
                    ValidTo = ParseOptionalDate(
                        Text(fields, 28),
                        sourceRecordNumber,
                        "valid-to date")
                });
            }

            result.SourceRowCount = sourceRecordNumber;
        }

        private static void ValidateRepeatedHeader(
            string[] fields,
            int sourceRecordNumber)
        {
            if (!Contains(Text(fields, 4), "Účtový rozvrh") ||
                !Contains(Text(fields, 8), "SU") ||
                !Contains(Text(fields, 9), "Názov účtu") ||
                !Contains(Text(fields, 10), "Činn") ||
                !Contains(Text(fields, 11), "Typ") ||
                !Contains(Text(fields, 12), "PS") ||
                !Contains(Text(fields, 13), "BU") ||
                !Contains(Text(fields, 14), "PL") ||
                !Contains(Text(fields, 15), "RU") ||
                !Contains(Text(fields, 16), "Mena") ||
                !Contains(Text(fields, 17), "Platný"))
            {
                throw new InvalidDataException(
                    "IVES accounting-framework CSV record " +
                    sourceRecordNumber +
                    " does not contain the expected repeated report header.");
            }
        }

        private static string ExtractIco(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            const string prefix = "IČO:";
            string trimmed = value.Trim();

            if (!trimmed.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return trimmed.Substring(prefix.Length).Trim();
        }

        private static DateTime? ParseOptionalDate(
            string value,
            int sourceRecordNumber,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParseExact(
                    value.Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsed) ||
                DateTime.TryParseExact(
                    value.Trim(),
                    "d.M.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "IVES accounting-framework CSV record " +
                sourceRecordNumber +
                " contains invalid " + fieldName +
                " '" + value + "'.");
        }

        private static IEnumerable<string[]> ReadRecords(
            string filePath)
        {
            using (var reader = new StreamReader(
                filePath,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false,
                    throwOnInvalidBytes: true),
                detectEncodingFromByteOrderMarks: true))
            {
                var fields = new List<string>();
                var field = new StringBuilder();
                bool quoted = false;

                while (true)
                {
                    int raw = reader.Read();

                    if (raw < 0)
                    {
                        if (quoted)
                        {
                            throw new InvalidDataException(
                                "The IVES accounting-framework CSV file contains an unterminated quoted field.");
                        }

                        if (field.Length > 0 ||
                            fields.Count > 0)
                        {
                            fields.Add(field.ToString());
                            yield return fields.ToArray();
                        }

                        yield break;
                    }

                    char character = (char)raw;

                    if (character == '"')
                    {
                        if (quoted && reader.Peek() == '"')
                        {
                            reader.Read();
                            field.Append('"');
                        }
                        else
                        {
                            quoted = !quoted;
                        }

                        continue;
                    }

                    if (!quoted && character == ',')
                    {
                        fields.Add(field.ToString());
                        field.Clear();
                        continue;
                    }

                    if (!quoted &&
                        (character == '\r' ||
                         character == '\n'))
                    {
                        if (character == '\r' &&
                            reader.Peek() == '\n')
                        {
                            reader.Read();
                        }

                        fields.Add(field.ToString());
                        field.Clear();

                        if (fields.Count > 1 ||
                            !string.IsNullOrWhiteSpace(fields[0]))
                        {
                            yield return fields.ToArray();
                        }

                        fields.Clear();
                        continue;
                    }

                    field.Append(character);
                }
            }
        }

        private static string Text(
            string[] fields,
            int index)
        {
            if (index < 0 || index >= fields.Length)
                return null;

            string value = fields[index];

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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
                    "The IVES accounting-framework CSV file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES CSV accounting-framework parser accepts only .csv files.");
            }
        }
    }
}
