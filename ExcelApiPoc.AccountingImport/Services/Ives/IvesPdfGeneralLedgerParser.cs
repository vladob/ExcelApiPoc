using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesPdfGeneralLedgerParser
    {
        private const string LayoutResourceName =
            "ExcelApiPoc.AccountingImport.PdfLayouts.Ives.general-ledger.v1.json";

        private static readonly Regex DocumentDatePattern = new Regex(
            @"^\d{2}\.\d{2}\.",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public IvesGeneralLedgerParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            AccountingFileNameMetadata metadata;
            if (!AccountingFileNameMetadataParser.TryParse(fileName, out metadata) ||
                metadata.DocumentKind != AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' does not identify an IVES general ledger with IČO and fiscal year.");
            }

            LayoutRecognitionResult recognition =
                new ITextLayoutRecognizer().Recognize(fullPath, LoadLayout());

            if (recognition.Diagnostics.Count > 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger PDF layout could not be recognized without diagnostics.");
            }

            var result = new IvesGeneralLedgerParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                PeriodStart = new DateTime(metadata.FiscalYear, 1, 1),
                PeriodEnd = new DateTime(metadata.FiscalYear, 12, 31),
                SourceRowCount = recognition.RecordDiscovery.Records.Count
            };

            ParseRecords(recognition.RecordDiscovery.Records, result);

            if (result.Activities.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger PDF contains no recognizable activities.");
            }

            return result;
        }

        private static void ParseRecords(
            IReadOnlyList<BaselineRecord> records,
            IvesGeneralLedgerParseResult result)
        {
            IvesGeneralLedgerActivity currentActivity = null;
            int documentSequence = 0;
            int accountSequence = 0;
            int syntheticSequence = 0;
            int totalSequence = 0;

            for (int index = 0; index < records.Count; index++)
            {
                BaselineRecord record = records[index];
                string compact = CompactText(record);

                string activityName;
                string activityCurrency;
                if (TryReadActivity(
                        record,
                        compact,
                        out activityName,
                        out activityCurrency))
                {
                    currentActivity = result.Activities.FirstOrDefault(
                        activity =>
                            string.Equals(
                                activity.Name,
                                activityName,
                                StringComparison.Ordinal) &&
                            string.Equals(
                                activity.Currency,
                                activityCurrency,
                                StringComparison.Ordinal));

                    if (currentActivity == null)
                    {
                        currentActivity = new IvesGeneralLedgerActivity
                        {
                            Name = activityName,
                            Currency = activityCurrency
                        };
                        result.Activities.Add(currentActivity);
                    }

                    continue;
                }

                if (currentActivity == null)
                    continue;

                if (compact.StartsWith(
                        "Celkom",
                        StringComparison.OrdinalIgnoreCase))
                {
                    currentActivity.ReportTotalRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++totalSequence,
                            SourceRowNumber = index + 1,
                            Kind = IvesGeneralLedgerRowKind.ReportTotal,
                            OpeningBalance = ReadAmount(record, 319.0, 364.0),
                            DebitTurnover = ReadAmount(record, 386.0, 435.0),
                            CreditTurnover = ReadAmount(record, 454.0, 503.0),
                            ClosingBalance = ReadAmount(record, 526.0, 571.0)
                        });
                    continue;
                }

                if (IsSyntheticSummary(compact))
                {
                    currentActivity.SyntheticSummaryRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++syntheticSequence,
                            SourceRowNumber = index + 1,
                            Kind = IvesGeneralLedgerRowKind.SyntheticSubtotal,
                            AccountCode = ReadSyntheticCode(record),
                            OpeningBalance = ReadAmount(record, 319.0, 364.0),
                            DebitTurnover = ReadAmount(record, 386.0, 435.0),
                            CreditTurnover = ReadAmount(record, 454.0, 503.0),
                            ClosingBalance = ReadAmount(record, 526.0, 571.0)
                        });
                    continue;
                }

                if (IsAnalyticalSummary(compact))
                {
                    currentActivity.AccountRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++accountSequence,
                            SourceRowNumber = index + 1,
                            Kind = IvesGeneralLedgerRowKind.Account,
                            AccountCode = ReadCompact(record, 103.0, 221.0),
                            Text = ReadText(record, 228.0, 321.0),
                            OpeningBalance = ReadAmount(record, 319.0, 364.0),
                            DebitTurnover = ReadAmount(record, 386.0, 435.0),
                            CreditTurnover = ReadAmount(record, 454.0, 503.0),
                            ClosingBalance = ReadAmount(record, 526.0, 571.0)
                        });
                    continue;
                }

                if (DocumentDatePattern.IsMatch(compact))
                {
                    currentActivity.DocumentRows.Add(
                        new IvesGeneralLedgerSourceRow
                        {
                            SequenceNumber = ++documentSequence,
                            SourceRowNumber = index + 1,
                            Kind = IvesGeneralLedgerRowKind.Document,
                            DocumentDate = ReadDocumentDate(
                                record,
                                result.FiscalYear,
                                index + 1),
                            DocumentNumber = ReadCompact(record, 47.0, 103.0),
                            AccountCode = ReadCompact(record, 103.0, 221.0),
                            Text = ReadText(record, 228.0, 321.0),
                            DebitTurnover = ReadAmount(record, 386.0, 435.0),
                            CreditTurnover = ReadAmount(record, 454.0, 503.0)
                        });
                }
            }
        }

        private static bool TryReadActivity(
            BaselineRecord record,
            string compact,
            out string name,
            out string currency)
        {
            name = null;
            currency = null;

            if (compact.IndexOf(
                    "Hlavnáčinnosť",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                name = "Hlavná činnosť";
            }
            else if (compact.IndexOf(
                         "Stravovanie",
                         StringComparison.OrdinalIgnoreCase) >= 0)
            {
                name = "Stravovanie";
            }
            else
            {
                return false;
            }

            currency = ReadCompact(record, 130.0, 160.0);
            return true;
        }

        private static bool IsAnalyticalSummary(string compact)
        {
            return compact.StartsWith("--", StringComparison.Ordinal) &&
                   compact.Length >= 5 &&
                   char.IsDigit(compact[2]) &&
                   char.IsDigit(compact[3]) &&
                   char.IsDigit(compact[4]) &&
                   compact.IndexOf("====", StringComparison.Ordinal) < 0;
        }

        private static bool IsSyntheticSummary(string compact)
        {
            return compact.StartsWith("--", StringComparison.Ordinal) &&
                   compact.IndexOf("====", StringComparison.Ordinal) >= 0 &&
                   compact.IndexOf(
                       "SU",
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ReadSyntheticCode(BaselineRecord record)
        {
            string value = ReadCompact(record, 103.0, 118.5);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var digits = new string(
                value.Where(char.IsDigit).Take(3).ToArray());

            return digits.Length == 3 ? digits : null;
        }

        private static DateTime ReadDocumentDate(
            BaselineRecord record,
            int fiscalYear,
            int sourceRowNumber)
        {
            string value = ReadCompact(record, 27.0, 47.0);

            DateTime parsed;
            if (DateTime.TryParseExact(
                    value + fiscalYear.ToString(CultureInfo.InvariantCulture),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "IVES general-ledger PDF source record " +
                sourceRowNumber +
                " contains invalid document date '" +
                (value ?? string.Empty) + "'.");
        }

        private static decimal ReadAmount(
            BaselineRecord record,
            double left,
            double right)
        {
            string value = string.Concat(
                record.SourceTokens
                    .Where(token =>
                        token.Left >= left &&
                        token.Left < right)
                    .OrderBy(token => token.Left)
                    .ThenBy(token => token.Right)
                    .Select(token => Trim(token.Text))
                    .Where(IsAmountToken))
                .Replace("\u00A0", string.Empty)
                .Replace(" ", string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            MatchCollection amountMatches = Regex.Matches(
                value,
                @"-?\d+(?:,\d{2})?",
                RegexOptions.CultureInvariant);

            string amountText = amountMatches.Count > 0
                ? amountMatches[amountMatches.Count - 1].Value
                : value;

            decimal parsed;
            if (decimal.TryParse(
                    amountText,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"),
                    out parsed))
            {
                return parsed;
            }

            throw new InvalidDataException(
                "IVES general-ledger PDF contains invalid amount '" +
                value + "'.");
        }

        private static bool IsAmountToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (char character in text)
            {
                if (!char.IsDigit(character) &&
                    character != ',' &&
                    character != '.' &&
                    character != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static string ReadCompact(
            BaselineRecord record,
            double left,
            double right)
        {
            return string.Concat(
                record.SourceTokens
                    .Where(token =>
                        token.Left >= left &&
                        token.Left < right)
                    .OrderBy(token => token.Left)
                    .ThenBy(token => token.Right)
                    .Select(token => Trim(token.Text))
                    .Where(text => text.Length > 0))
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);
        }

        private static string ReadText(
            BaselineRecord record,
            double left,
            double right)
        {
            var builder = new StringBuilder();

            foreach (PdfTextToken token in record.SourceTokens
                         .Where(token =>
                             token.Left >= left &&
                             token.Left < right)
                         .OrderBy(token => token.Left)
                         .ThenBy(token => token.Right))
            {
                builder.Append(token.Text ?? string.Empty);
            }

            return NormalizeWhitespace(builder.ToString());
        }

        private static string CompactText(BaselineRecord record)
        {
            return string.Concat(
                record.SourceTokens
                    .OrderBy(token => token.Left)
                    .ThenBy(token => token.Right)
                    .Select(token => token.Text ?? string.Empty))
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);
        }

        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var builder = new StringBuilder();
            bool pendingSpace = false;

            foreach (char character in value.Trim())
            {
                if (char.IsWhiteSpace(character))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }

        private static string Trim(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        private static LayoutDefinition LoadLayout()
        {
            Assembly assembly =
                typeof(IvesPdfGeneralLedgerParser).Assembly;

            using (Stream stream =
                   assembly.GetManifestResourceStream(LayoutResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        "Embedded IVES PDF layout '" +
                        LayoutResourceName +
                        "' was not found.");
                }

                LayoutLoadResult result =
                    new LayoutDefinitionLoader().Load(stream);

                if (!result.IsValid)
                {
                    throw new InvalidDataException(
                        "Embedded IVES general-ledger PDF layout is invalid: " +
                        string.Join(
                            "; ",
                            result.Messages.Select(
                                message => message.Message)));
                }

                return result.Definition;
            }
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
                    "The IVES general-ledger PDF file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES PDF general-ledger parser accepts only .pdf files.");
            }
        }
    }
}
