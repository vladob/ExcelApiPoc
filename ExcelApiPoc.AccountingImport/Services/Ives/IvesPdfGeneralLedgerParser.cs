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
    internal sealed class IvesPdfGeneralLedgerParser : IIvesGeneralLedgerSourceParser
    {
        private const string LayoutResourceName =
            "ExcelApiPoc.AccountingImport.PdfLayouts.Ives.general-ledger.v1.json";

        private static readonly Regex DocumentDatePattern = new Regex(
            @"^\d{2}\.\d{2}\.",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string TechnicalType => "PDF";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".pdf",
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
                            OpeningBalance = ReadAmount(record, 363.4),
                            DebitTurnover = ReadAmount(record, 434.7),
                            CreditTurnover = ReadAmount(record, 502.2),
                            ClosingBalance = ReadAmount(record, 570.1)
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
                            OpeningBalance = ReadAmount(record, 363.4),
                            DebitTurnover = ReadAmount(record, 434.7),
                            CreditTurnover = ReadAmount(record, 502.2),
                            ClosingBalance = ReadAmount(record, 570.1)
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
                            OpeningBalance = ReadAmount(record, 363.4),
                            DebitTurnover = ReadAmount(record, 434.7),
                            CreditTurnover = ReadAmount(record, 502.2),
                            ClosingBalance = ReadAmount(record, 570.1)
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
                            DebitTurnover = ReadAmount(record, 434.7),
                            CreditTurnover = ReadAmount(record, 502.2)
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

            // An IVES activity header is a short standalone row near the left
            // margin. Do not classify ordinary account/document text that
            // merely contains an activity name.
            if (record.Left > 35.0 || record.Right > 170.0)
                return false;

            if (string.Equals(
                    compact,
                    "HlavnáčinnosťEUR",
                    StringComparison.OrdinalIgnoreCase))
            {
                name = "Hlavná činnosť";
                currency = "EUR";
                return true;
            }

            if (string.Equals(
                    compact,
                    "StravovanieEUR",
                    StringComparison.OrdinalIgnoreCase))
            {
                name = "Stravovanie";
                currency = "EUR";
                return true;
            }

            return false;
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
            // In this PDF the S and U glyphs can be interleaved with the
            // underline '=' tokens, so "SU" is not reliably contiguous.
            // The repeated '=' run is the stable discriminator from an
            // analytical account summary.
            return compact.StartsWith("--", StringComparison.Ordinal) &&
                   compact.IndexOf("====", StringComparison.Ordinal) >= 0;
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
            double anchor)
        {
            decimal tokenAmount;
            if (TryReadGroupedAmount(record, anchor, out tokenAmount))
                return tokenAmount;

            const double commaRightOffset = 8.30;
            const double decimalAdvance = 4.104;
            const double commaToUnitsRight = 2.052;
            const double thousandsGap = 2.04;
            const double slotTolerance = 0.50;
            const int maximumIntegerDigits = 12;

            List<AmountGlyph> glyphs = record.SourceTokens
                .Select(TryCreateAmountGlyph)
                .Where(glyph => glyph != null)
                .ToList();

            AmountGlyph comma = FindNearestGlyph(
                glyphs,
                anchor - commaRightOffset,
                slotTolerance,
                glyph => glyph.Character == ',');

            if (comma == null)
            {
                string nearby = string.Join(" ", record.SourceTokens
                    .Where(token => token.Right >= anchor - 65.0 && token.Left <= anchor + 12.0)
                    .OrderBy(token => token.Left)
                    .Take(90)
                    .Select(token => "'" + token.Text + "'@" +
                        token.Right.ToString("F2", CultureInfo.InvariantCulture)));
                throw new InvalidDataException(
                    "IVES general-ledger PDF contains no amount decimal separator near " +
                    anchor.ToString("F1", CultureInfo.InvariantCulture) +
                    " on page " + record.StartPageNumber +
                    ". Record: '" + record.Text + "'. Nearby tokens: " + nearby + ".");
            }

            AmountGlyph decimal1 = FindNearestGlyph(
                glyphs,
                comma.Right + decimalAdvance,
                slotTolerance,
                glyph => char.IsDigit(glyph.Character));

            AmountGlyph decimal2 = FindNearestGlyph(
                glyphs,
                comma.Right + (2.0 * decimalAdvance),
                slotTolerance,
                glyph => char.IsDigit(glyph.Character));

            if (decimal1 == null || decimal2 == null)
            {
                throw new InvalidDataException(
                    "IVES general-ledger PDF contains an incomplete decimal amount near " +
                    anchor.ToString("F1", CultureInfo.InvariantCulture) + ".");
            }

            var integerDigits = new List<AmountGlyph>();

            for (int digitIndex = 0;
                 digitIndex < maximumIntegerDigits;
                 digitIndex++)
            {
                double expectedRight =
                    comma.Right -
                    commaToUnitsRight -
                    (decimalAdvance * digitIndex) -
                    (thousandsGap * (digitIndex / 3));

                AmountGlyph digit = FindNearestGlyph(
                    glyphs,
                    expectedRight,
                    slotTolerance,
                    glyph => char.IsDigit(glyph.Character));

                if (digit == null)
                    break;

                integerDigits.Add(digit);
            }

            if (integerDigits.Count == 0)
            {
                throw new InvalidDataException(
                    "IVES general-ledger PDF contains no integer digits near amount anchor " +
                    anchor.ToString("F1", CultureInfo.InvariantCulture) + ".");
            }

            AmountGlyph mostSignificant =
                integerDigits[integerDigits.Count - 1];

            AmountGlyph sign = glyphs
                .Where(glyph => glyph.Character == '-')
                .Where(glyph =>
                {
                    double gap = mostSignificant.Left - glyph.Right;
                    return gap >= -0.20 && gap <= 0.60;
                })
                .OrderBy(glyph =>
                    Math.Abs(mostSignificant.Left - glyph.Right))
                .FirstOrDefault();

            integerDigits.Reverse();

            string amountText =
                (sign == null ? string.Empty : "-") +
                new string(
                    integerDigits
                        .Select(glyph => glyph.Character)
                        .ToArray()) +
                "," +
                decimal1.Character +
                decimal2.Character;

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
                amountText + "'.");
        }

        private static bool TryReadGroupedAmount(BaselineRecord record, double anchor, out decimal amount)
        {
            amount = 0m;
            var tokens = record.SourceTokens
                .Where(token => token.Right >= anchor - 70.0 && token.Left <= anchor + 1.0)
                .OrderBy(token => token.Left)
                .ToArray();
            var final = tokens
                .Where(token => Math.Abs(token.Right - anchor) <= 0.75 &&
                    Regex.IsMatch(Trim(token.Text), @"^-?(?:\d{1,12}|\d{1,3}(?:[ \u00a0]\d{3})+),\d{2}$"))
                .OrderBy(token => Math.Abs(token.Right - anchor))
                .FirstOrDefault();
            if (final == null) return false;

            string value = Trim(final.Text).Replace(" ", string.Empty).Replace("\u00a0", string.Empty);
            double left = final.Left;
            for (int i = Array.IndexOf(tokens, final) - 1; i >= 0; i--)
            {
                var previous = tokens[i];
                string part = Trim(previous.Text);
                double gap = left - previous.Right;
                if (Math.Abs(previous.Baseline - final.Baseline) > 1.5 ||
                    gap < -0.3 || gap > 4.0) break;
                string digits = part.Replace(" ", string.Empty).Replace("\u00a0", string.Empty);
                if (Regex.IsMatch(part, @"^-?\d{1,3}(?:[ \u00a0]\d{3})*$") &&
                    value.TrimStart('-').Split(',')[0].Length + digits.TrimStart('-').Length <= 12)
                {
                    value = digits + value;
                    left = previous.Left;
                    if (part.StartsWith("-", StringComparison.Ordinal)) break;
                }
                else if (part == "-" && !value.StartsWith("-", StringComparison.Ordinal))
                {
                    value = "-" + value;
                    break;
                }
                else break;
            }
            return decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.GetCultureInfo("sk-SK"), out amount);
        }

        private static AmountGlyph TryCreateAmountGlyph(
            PdfTextToken token)
        {
            string text = Trim(token.Text);

            if (text.Length != 1)
                return null;

            char character = text[0];

            if (!char.IsDigit(character) &&
                character != ',' &&
                character != '-')
            {
                return null;
            }

            return new AmountGlyph(
                character,
                token.Left,
                token.Right);
        }

        private static AmountGlyph FindNearestGlyph(
            IEnumerable<AmountGlyph> glyphs,
            double expectedRight,
            double tolerance,
            Func<AmountGlyph, bool> predicate)
        {
            return glyphs
                .Where(predicate)
                .Select(glyph => new
                {
                    Glyph = glyph,
                    Distance = Math.Abs(glyph.Right - expectedRight)
                })
                .Where(item => item.Distance <= tolerance)
                .OrderBy(item => item.Distance)
                .ThenBy(item => item.Glyph.Left)
                .Select(item => item.Glyph)
                .FirstOrDefault();
        }

        private sealed class AmountGlyph
        {
            public AmountGlyph(
                char character,
                double left,
                double right)
            {
                Character = character;
                Left = left;
                Right = right;
            }

            public char Character { get; private set; }
            public double Left { get; private set; }
            public double Right { get; private set; }
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
