using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesCsvGeneralLedgerParser : IIvesGeneralLedgerSourceParser
    {
        public string TechnicalType => "CSV";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".csv",
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

            ParseRows(filePath, result);

            if (result.Activities.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger CSV file contains no recognizable activities.");
            }

            return result;
        }

        private static void ParseRows(
            string filePath,
            IvesGeneralLedgerParseResult result)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var reader = new StreamReader(
                filePath,
                DetectEncoding(filePath),
                true))
            {
                string firstLine = reader.ReadLine();
                if (firstLine == null)
                    return;

                char delimiter = DetectDelimiter(firstLine);
                int sourceRowNumber = 0;
                int documentSequence = 0;
                int accountSequence = 0;
                int syntheticSequence = 0;
                int totalSequence = 0;

                IvesGeneralLedgerActivity currentActivity = null;
                string currentActivityKey = null;

                var accountSignatures = new HashSet<string>(
                    StringComparer.Ordinal);
                var syntheticSignatures = new HashSet<string>(
                    StringComparer.Ordinal);
                var totalSignatures = new HashSet<string>(
                    StringComparer.Ordinal);

                string line = firstLine;
                while (line != null)
                {
                    sourceRowNumber++;
                    string[] fields = ParseFields(line, delimiter);

                    if (fields.Length < 198)
                    {
                        throw new InvalidDataException(
                            "IVES general-ledger CSV row " +
                            sourceRowNumber +
                            " has " + fields.Length +
                            " columns; at least 198 are required.");
                    }

                    string activityName = Text(fields, 16);
                    string currency = Text(fields, 17);

                    if (!string.IsNullOrWhiteSpace(activityName))
                    {
                        string activityKey =
                            activityName + "\u001f" + (currency ?? string.Empty);

                        if (!string.Equals(
                                currentActivityKey,
                                activityKey,
                                StringComparison.Ordinal))
                        {
                            currentActivity = new IvesGeneralLedgerActivity
                            {
                                Name = activityName,
                                Currency = currency
                            };
                            result.Activities.Add(currentActivity);
                            currentActivityKey = activityKey;

                            accountSignatures.Clear();
                            syntheticSignatures.Clear();
                            totalSignatures.Clear();
                        }
                    }

                    if (currentActivity == null)
                    {
                        throw new InvalidDataException(
                            "IVES general-ledger CSV row " +
                            sourceRowNumber +
                            " contains ledger data before the first activity.");
                    }

                    if (TryParseDocumentDate(
                            Text(fields, 18),
                            result.FiscalYear,
                            out DateTime date))
                    {
                        currentActivity.DocumentRows.Add(
                            new IvesGeneralLedgerSourceRow
                            {
                                SequenceNumber = ++documentSequence,
                                SourceRowNumber = sourceRowNumber,
                                Kind = IvesGeneralLedgerRowKind.Document,
                                DocumentDate = date,
                                DocumentNumber = Text(fields, 19),
                                AccountCode = Text(fields, 20),
                                Text = Text(fields, 21),
                                DebitTurnover = Amount(
                                    fields, 24, sourceRowNumber, "debit turnover"),
                                CreditTurnover = Amount(
                                    fields, 25, sourceRowNumber, "credit turnover")
                            });
                    }

                    if (IsAnalyticalSummary(fields))
                    {
                        string signature = Signature(fields, 60, 61, 62, 63, 64, 65);
                        if (accountSignatures.Add(signature))
                        {
                            currentActivity.AccountRows.Add(
                                new IvesGeneralLedgerSourceRow
                                {
                                    SequenceNumber = ++accountSequence,
                                    SourceRowNumber = sourceRowNumber,
                                    Kind = IvesGeneralLedgerRowKind.Account,
                                    AccountCode = Text(fields, 60),
                                    Text = Text(fields, 61),
                                    OpeningBalance = Amount(
                                        fields, 62, sourceRowNumber, "opening balance"),
                                    DebitTurnover = Amount(
                                        fields, 63, sourceRowNumber, "debit turnover"),
                                    CreditTurnover = Amount(
                                        fields, 64, sourceRowNumber, "credit turnover"),
                                    ClosingBalance = Amount(
                                        fields, 65, sourceRowNumber, "closing balance")
                                });
                        }
                    }

                    if (IsSyntheticSummary(fields))
                    {
                        string signature =
                            Signature(fields, 185, 186, 188, 189, 190, 191);

                        if (syntheticSignatures.Add(signature))
                        {
                            currentActivity.SyntheticSummaryRows.Add(
                                new IvesGeneralLedgerSourceRow
                                {
                                    SequenceNumber = ++syntheticSequence,
                                    SourceRowNumber = sourceRowNumber,
                                    Kind = IvesGeneralLedgerRowKind.SyntheticSubtotal,
                                    AccountCode = ExtractSyntheticCode(
                                        Text(fields, 185)),
                                    OpeningBalance = Amount(
                                        fields, 188, sourceRowNumber, "opening balance"),
                                    DebitTurnover = Amount(
                                        fields, 189, sourceRowNumber, "debit turnover"),
                                    CreditTurnover = Amount(
                                        fields, 190, sourceRowNumber, "credit turnover"),
                                    ClosingBalance = Amount(
                                        fields, 191, sourceRowNumber, "closing balance")
                                });
                        }
                    }

                    if (IsReportTotal(fields))
                    {
                        string signature =
                            Signature(fields, 192, 194, 195, 196, 197);

                        if (totalSignatures.Add(signature))
                        {
                            currentActivity.ReportTotalRows.Add(
                                new IvesGeneralLedgerSourceRow
                                {
                                    SequenceNumber = ++totalSequence,
                                    SourceRowNumber = sourceRowNumber,
                                    Kind = IvesGeneralLedgerRowKind.ReportTotal,
                                    OpeningBalance = Amount(
                                        fields, 194, sourceRowNumber, "opening balance"),
                                    DebitTurnover = Amount(
                                        fields, 195, sourceRowNumber, "debit turnover"),
                                    CreditTurnover = Amount(
                                        fields, 196, sourceRowNumber, "credit turnover"),
                                    ClosingBalance = Amount(
                                        fields, 197, sourceRowNumber, "closing balance")
                                });
                        }
                    }

                    line = reader.ReadLine();
                }

                result.SourceRowCount = sourceRowNumber;
            }
        }

        private static bool IsAnalyticalSummary(string[] fields)
        {
            return string.Equals(Text(fields, 58), "-", StringComparison.Ordinal) &&
                   string.Equals(Text(fields, 59), "-", StringComparison.Ordinal) &&
                   !string.IsNullOrWhiteSpace(Text(fields, 60)) &&
                   Text(fields, 60).IndexOf('=') < 0;
        }

        private static bool IsSyntheticSummary(string[] fields)
        {
            return string.Equals(Text(fields, 183), "-", StringComparison.Ordinal) &&
                   string.Equals(Text(fields, 184), "-", StringComparison.Ordinal) &&
                   !string.IsNullOrWhiteSpace(Text(fields, 185)) &&
                   Text(fields, 185).IndexOf('=') >= 0 &&
                   string.Equals(
                       Text(fields, 186),
                       "SU",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReportTotal(string[] fields)
        {
            string label = Text(fields, 192);
            if (string.IsNullOrWhiteSpace(label))
                return false;

            var builder = new StringBuilder();
            foreach (char c in label)
            {
                if (!char.IsWhiteSpace(c))
                    builder.Append(c);
            }

            return string.Equals(
                builder.ToString(),
                "Celkom",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Signature(
            string[] fields,
            params int[] indexes)
        {
            var builder = new StringBuilder();

            foreach (int index in indexes)
            {
                if (builder.Length > 0)
                    builder.Append('\u001f');

                builder.Append(Text(fields, index) ?? string.Empty);
            }

            return builder.ToString();
        }

        private static bool TryParseDocumentDate(
            string value,
            int fiscalYear,
            out DateTime date)
        {
            date = default(DateTime);

            if (string.IsNullOrWhiteSpace(value) ||
                string.Equals(value, "-", StringComparison.Ordinal))
            {
                return false;
            }

            string normalized = value.Trim();

            if (DateTime.TryParseExact(
                    normalized + fiscalYear.ToString(CultureInfo.InvariantCulture),
                    "d.M.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsed))
            {
                date = parsed.Date;
                return true;
            }

            return false;
        }

        private static decimal Amount(
            string[] fields,
            int index,
            int sourceRowNumber,
            string field)
        {
            string value = Text(fields, index);
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            string normalized = value
                .Replace("\u00a0", string.Empty)
                .Replace(" ", string.Empty)
                .Replace(",", ".");

            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                return result;
            }

            throw new InvalidDataException(
                "IVES general-ledger CSV row " +
                sourceRowNumber +
                " contains invalid " + field +
                " '" + value + "'.");
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

        private static string Text(string[] fields, int index)
        {
            if (index < 0 || index >= fields.Length)
                return null;

            string value = fields[index];
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static Encoding DetectEncoding(string filePath)
        {
            var utf8 = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true);

            try
            {
                using (var reader = new StreamReader(
                    filePath,
                    utf8,
                    detectEncodingFromByteOrderMarks: true))
                {
                    var buffer = new char[4096];
                    while (reader.Read(buffer, 0, buffer.Length) > 0)
                    {
                    }
                }

                return new UTF8Encoding(false);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.GetEncoding(1250);
            }
        }

        private static char DetectDelimiter(string line)
        {
            int commas = CountDelimiter(line, ',');
            int semicolons = CountDelimiter(line, ';');

            if (commas == 0 && semicolons == 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger CSV file does not contain a recognized delimiter.");
            }

            return commas >= semicolons ? ',' : ';';
        }

        private static int CountDelimiter(
            string line,
            char delimiter)
        {
            int count = 0;
            bool quoted = false;

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];

                if (character == '"')
                {
                    if (quoted &&
                        index + 1 < line.Length &&
                        line[index + 1] == '"')
                    {
                        index++;
                        continue;
                    }

                    quoted = !quoted;
                }
                else if (!quoted && character == delimiter)
                {
                    count++;
                }
            }

            return count;
        }

        private static string[] ParseFields(
            string line,
            char delimiter)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];

                if (character == '"')
                {
                    if (quoted &&
                        index + 1 < line.Length &&
                        line[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }

                    continue;
                }

                if (!quoted && character == delimiter)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                field.Append(character);
            }

            if (quoted)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger CSV contains an unterminated quoted field.");
            }

            fields.Add(field.ToString());
            return fields.ToArray();
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
                    "The IVES general-ledger CSV file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES CSV general-ledger parser accepts only .csv files.");
            }
        }
    }
}
