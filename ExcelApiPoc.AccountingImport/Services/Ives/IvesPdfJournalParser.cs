using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;
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
    internal sealed class IvesPdfJournalParser : IIvesJournalSourceParser
    {
        private const string LayoutResourceName =
            "ExcelApiPoc.AccountingImport.PdfLayouts." +
            "Ives.journal.v1.json";

        private static readonly Regex IcoPattern = new Regex(
            @"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);

        private static readonly Regex PeriodPattern = new Regex(
            @"Dátum\s+vytvorenia\s+od:\s*(?<from>\d{2}\.\d{2}\.\d{4}),?\s*" +
            @"do:\s*(?<to>\d{2}\.\d{2}\.\d{4})",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);

        public string TechnicalType => "PDF";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".pdf",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesJournalParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            LayoutDefinition layout = LoadLayout();
            LayoutRecognitionResult recognition =
                new ITextLayoutRecognizer().Recognize(fullPath, layout);

            if (recognition.Diagnostics.Count > 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF layout could not be " +
                    "recognized without diagnostics.");
            }

            RecognizedRecord[] ambiguous = recognition.Records
                .Where(record => record.Status == RuleMatchStatus.Ambiguous)
                .ToArray();

            if (ambiguous.Length > 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF contains " +
                    ambiguous.Length +
                    " ambiguously classified layout records.");
            }

            string[] pageTexts = recognition.Document.Pages
                .Select(PageText)
                .ToArray();

            string ico = ReadSingleIco(pageTexts);
            DateTime periodStart;
            DateTime periodEnd;
            ReadSinglePeriod(pageTexts, out periodStart, out periodEnd);

            if (periodStart.Year != periodEnd.Year)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF period spans more " +
                    "than one fiscal year.");
            }

            var result = new IvesJournalParseResult
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = fullPath,
                Ico = ico,
                FiscalYear = periodStart.Year,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                SourceRowCount = recognition.Records.Count
            };

            var reportTotals = new List<Tuple<int, RecognizedRecord, decimal>>();
            int sequence = 0;

            for (int index = 0; index < recognition.Records.Count; index++)
            {
                RecognizedRecord record = recognition.Records[index];

                if (record.Status != RuleMatchStatus.Matched)
                    continue;

                if (string.Equals(
                        record.RuleId,
                        "transaction-row",
                        StringComparison.Ordinal))
                {
                    IvesJournalSourceRow transaction =
                        ParseTransaction(
                            result.SourceFileName,
                            record,
                            index + 1,
                            ++sequence);

                    if (index + 1 < recognition.Records.Count)
                    {
                        RecognizedRecord next = recognition.Records[index + 1];
                        if (next.Status == RuleMatchStatus.Matched &&
                            string.Equals(
                                next.RuleId,
                                "module-row",
                                StringComparison.Ordinal))
                        {
                            transaction.Module =
                                RequiredField(next, "module");
                            transaction.RelatedSourceRowNumber = index + 2;
                        }
                    }

                    result.TransactionRows.Add(transaction);
                    continue;
                }

                if (string.Equals(
                        record.RuleId,
                        "report-total-row",
                        StringComparison.Ordinal))
                {
                    reportTotals.Add(Tuple.Create(
                        index + 1,
                        record,
                        ParseAmount(
                            RequiredField(record, "amount"),
                            "report total")));
                }
            }

            if (result.TransactionRows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF contains no " +
                    "recognized transactions.");
            }

            if (reportTotals.Count > 0)
            {
                var total = new IvesJournalSourceRow
                {
                    SequenceNumber = 1,
                    SourceRowNumber = reportTotals[0].Item1,
                    Kind = IvesJournalRowKind.ReportTotal,
                    SourceLocation = Location(
                        result.SourceFileName,
                        reportTotals[0].Item2)
                };

                total.ReportedAmounts.Add(
                    reportTotals.Sum(item => item.Item3));
                result.ReportTotalRows.Add(total);
            }

            return result;
        }

        private static IvesJournalSourceRow ParseTransaction(
            string sourceFileName,
            RecognizedRecord record,
            int sourceRowNumber,
            int sequenceNumber)
        {
            string currency = RequiredField(record, "currency");
            PdfTextToken currencyToken =
                RequiredFieldToken(record, "currency");

            return new IvesJournalSourceRow
            {
                SequenceNumber = sequenceNumber,
                SourceRowNumber = sourceRowNumber,
                Kind = IvesJournalRowKind.Transaction,
                PostingDate = ParseDate(
                    RequiredField(record, "posting-date"),
                    sourceRowNumber),
                DocumentNumber =
                    RequiredField(record, "document-number"),
                DebitCompositeAccount =
                    RequiredField(record, "debit-account"),
                CreditCompositeAccount =
                    RequiredField(record, "credit-account"),
                Amount = ParseAmount(
                    RequiredField(record, "amount"),
                    "transaction " + sequenceNumber),
                Currency = currency,
                Text = ReadTrailingText(
                    record.SourceRecord.SourceTokens,
                    currencyToken),
                SourceLocation = Location(sourceFileName, record)
            };
        }

        private static string ReadTrailingText(
            IReadOnlyList<PdfTextToken> tokens,
            PdfTextToken currencyToken)
        {
            int currencyIndex = -1;
            for (int index = 0; index < tokens.Count; index++)
            {
                if (ReferenceEquals(tokens[index], currencyToken))
                {
                    currencyIndex = index;
                    break;
                }
            }

            if (currencyIndex < 0 || currencyIndex + 1 >= tokens.Count)
                return null;

            var builder = new StringBuilder();
            PdfTextToken previous = null;

            for (int index = currencyIndex + 1; index < tokens.Count; index++)
            {
                PdfTextToken token = tokens[index];
                string rawText = token.Text ?? string.Empty;
                string text = rawText.Trim();

                if (text.Length == 0)
                    continue;

                bool sourceHasBoundaryWhitespace =
                    (previous != null &&
                     !string.IsNullOrEmpty(previous.Text) &&
                     char.IsWhiteSpace(
                         previous.Text[previous.Text.Length - 1])) ||
                    char.IsWhiteSpace(rawText[0]);

                bool hasGeometricGap =
                    previous != null &&
                    token.Left - previous.Right > 0.05d;

                if (builder.Length > 0 &&
                    (sourceHasBoundaryWhitespace || hasGeometricGap))
                {
                    builder.Append(' ');
                }

                builder.Append(text);
                previous = token;
            }

            string result = builder.ToString().Trim();
            return result.Length == 0 ? null : result;
        }

        private static string RequiredField(
            RecognizedRecord record,
            string fieldId)
        {
            RecordFieldMatchResult field = record.Fields
                .Single(item =>
                    string.Equals(
                        item.FieldId,
                        fieldId,
                        StringComparison.Ordinal));

            if (field.Status != RuleMatchStatus.Matched ||
                field.Value == null)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF record on page " +
                    record.SourceRecord.StartPageNumber +
                    " does not contain an unambiguous '" +
                    fieldId + "' field.");
            }

            return field.Value.Value == null
                ? null
                : field.Value.Value.Trim();
        }

        private static PdfTextToken RequiredFieldToken(
            RecognizedRecord record,
            string fieldId)
        {
            RecordFieldMatchResult field = record.Fields
                .Single(item =>
                    string.Equals(
                        item.FieldId,
                        fieldId,
                        StringComparison.Ordinal));

            if (field.Status != RuleMatchStatus.Matched ||
                field.Value == null ||
                field.Value.Evidence.SourceTokens.Count != 1)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF record on page " +
                    record.SourceRecord.StartPageNumber +
                    " does not contain single-token evidence for '" +
                    fieldId + "'.");
            }

            return field.Value.Evidence.SourceTokens[0];
        }

        private static DateTime ParseDate(
            string value,
            int sourceRowNumber)
        {
            DateTime parsed;
            if (DateTime.TryParseExact(
                    value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "IVES accounting-journal PDF source record " +
                sourceRowNumber +
                " contains invalid posting date '" +
                (value ?? string.Empty) + "'.");
        }

        private static decimal ParseAmount(
            string value,
            string context)
        {
            string normalized = (value ?? string.Empty)
                .Replace("\u00A0", string.Empty)
                .Replace(" ", string.Empty);

            decimal parsed;
            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number |
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"),
                    out parsed))
            {
                return parsed;
            }

            throw new InvalidDataException(
                "The IVES accounting-journal PDF " +
                context +
                " contains invalid amount '" +
                (value ?? string.Empty) + "'.");
        }

        private static string ReadSingleIco(IEnumerable<string> pageTexts)
        {
            string[] values = pageTexts
                .Select(text => IcoPattern.Match(text))
                .Where(match => match.Success)
                .Select(match => match.Groups["ico"].Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (values.Length != 1)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF must contain " +
                    "exactly one distinct IČO, but found " +
                    values.Length + ".");
            }

            return values[0];
        }

        private static void ReadSinglePeriod(
            IEnumerable<string> pageTexts,
            out DateTime start,
            out DateTime end)
        {
            Match[] matches = pageTexts
                .Select(text => PeriodPattern.Match(text))
                .Where(match => match.Success)
                .GroupBy(
                    match =>
                        match.Groups["from"].Value + "|" +
                        match.Groups["to"].Value,
                    StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();

            if (matches.Length != 1)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF must contain " +
                    "exactly one distinct accounting period, but found " +
                    matches.Length + ".");
            }

            start = ParsePeriodDate(matches[0].Groups["from"].Value);
            end = ParsePeriodDate(matches[0].Groups["to"].Value);

            if (end < start)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal PDF accounting period " +
                    "ends before it starts.");
            }
        }

        private static DateTime ParsePeriodDate(string value)
        {
            DateTime parsed;
            if (DateTime.TryParseExact(
                    value,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "The IVES accounting-journal PDF contains invalid " +
                "period date '" + value + "'.");
        }

        private static string PageText(PdfPage page)
        {
            return string.Join(
                " ",
                page.Tokens
                    .OrderByDescending(token => token.Baseline)
                    .ThenBy(token => token.Left)
                    .Select(token => token.Text == null
                        ? string.Empty
                        : token.Text.Trim())
                    .Where(text => text.Length > 0));
        }

        private static string Location(
            string sourceFileName,
            RecognizedRecord record)
        {
            return sourceFileName +
                ", PDF page " +
                record.SourceRecord.StartPageNumber +
                ", baseline " +
                record.SourceRecord.Groups[0].Baseline.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture);
        }

        private static LayoutDefinition LoadLayout()
        {
            Assembly assembly =
                typeof(IvesPdfJournalParser).Assembly;

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
                        "Embedded IVES PDF layout is invalid: " +
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
                throw new ArgumentException(
                    "A source file is required.",
                    nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES accounting-journal PDF file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES PDF journal parser accepts only .pdf files.");
            }
        }
    }
}
