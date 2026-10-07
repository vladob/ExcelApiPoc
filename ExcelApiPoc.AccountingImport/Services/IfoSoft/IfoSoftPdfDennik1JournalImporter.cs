using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    /// <summary>
    /// Imports the full-year, three-line IfoSoft _DENNIK1.GMX PDF printout.
    /// Also reads the annual, budget-detail _DENNIK.GMX printout.
    /// </summary>
    public sealed class IfoSoftPdfDennik1JournalImporter : IJournalImporter
    {
        private const string LayoutResource =
            "ExcelApiPoc.AccountingImport.PdfLayouts.IfoSoft.journal-dennik1.v1.json";
        private static readonly Regex DatePattern = new Regex(@"^\d{2}\.\d{2}\.\d{2}$");
        private static readonly Regex AccountPattern = new Regex(@"^(?:\d{3}[\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?|(?:75|79)[\p{L}][\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?|\d{3}\*{2,4})$");
        private static readonly Regex AmountPattern = new Regex(@"^-?\d+(?:,\d{2}|,-)$");
        private static readonly Regex PeriodPattern = new Regex(
            @"(?<!\d)(?:00|01)\s*/\s*(?<year>\d{4})\s*-\s*(?:12|13|14)\s*/\s*\k<year>(?!\d)");
        private static readonly Regex NamePattern = new Regex(
            @"^U_DENNIK_(?<ico>\d{8})_(?<year>\d{4})(?:_?[A-Za-z])?\.pdf$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public bool CanImport(string filePath, string accountingFormat) =>
            string.Equals(accountingFormat, "IfoSoft", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(filePath) &&
            NamePattern.IsMatch(Path.GetFileName(filePath));

        public static bool TryDetect(string filePath, out JournalDetectionResult detection)
        {
            detection = null;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) ||
                !NamePattern.IsMatch(Path.GetFileName(filePath))) return false;
            try
            {
                var document = new ITextPdfTokenExtractor().Extract(filePath);
                if (!(HasLayout(document) || HasBudgetDennikLayout(document)) || !TryReadPeriod(document, out int year)) return false;
                var name = NamePattern.Match(Path.GetFileName(filePath));
                if (year.ToString(CultureInfo.InvariantCulture) != name.Groups["year"].Value)
                    return false;
                detection = new JournalDetectionResult
                {
                    TechnicalType = "PDF", AccountingFormat = "IfoSoft",
                    Ico = name.Groups["ico"].Value, FiscalYear = year
                };
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException ||
                                       ex is ArgumentException)
            {
                return false;
            }
        }

        public JournalImport Import(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A journal PDF is required.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The journal PDF was not found.", filePath);
            var name = NamePattern.Match(Path.GetFileName(filePath));
            if (!name.Success)
                throw new InvalidDataException("Expected U_DENNIK_<IČO>_<year>.pdf (optional letter suffix).");

            var document = new ITextPdfTokenExtractor().Extract(filePath);
            if (!(HasLayout(document) || HasBudgetDennikLayout(document)))
                throw new InvalidDataException("Expected IfoSoft _DENNIK1.GMX or the budget-detail _DENNIK.GMX journal layout.");
            if (!TryReadPeriod(document, out int year) ||
                name.Groups["year"].Value != year.ToString(CultureInfo.InvariantCulture))
                throw new InvalidDataException("The printed journal must begin at period 00 or 01, end at 12, 13 or 14, and agree with its filename.");

            if (!HasBudgetDennikLayout(document)) document = NormalizeJournalScale(document);

            var result = new JournalImport
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = Sha256(filePath),
                TechnicalType = "PDF", AccountingFormat = "IfoSoft",
                Ico = name.Groups["ico"].Value, FiscalYear = year,
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = new ImportReport
                {
                    AccountingFormat = "IfoSoft", ImportType = "AccountingJournal",
                    SourceFileName = Path.GetFileName(filePath)
                }
            };
            result.ImportReport.Diagnostics.Add(new ImportDiagnostic
            {
                Code = "IFOSOFT_PDF_ICO_FROM_FILENAME",
                Severity = ImportDiagnosticSeverity.Warning,
                Message = "The PDF does not print an IČO; the filename supplies '" + result.Ico +
                          "'. Verify that it belongs to the printed entity."
            });

            var periodHeader = string.Join(" ", document.Pages[0].Tokens
                .Where(t => t.Baseline > document.Pages[0].Tokens.Max(item => item.Baseline) - 60)
                .OrderByDescending(t => t.Baseline).ThenBy(t => t.Left).Select(t => t.Text));
            if (Regex.IsMatch(periodHeader, @"(?<!\d)01\s*/\s*" + year + @"\s*-"))
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_PDF_OPENING_PERIOD_NOT_INCLUDED",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = "The printed journal starts at period 01; opening period 00 is not included in its header. Verify opening balances."
                });

            int nextYearEntries = 0;
            int missingDateEntries = 0;
            if (HasBudgetDennikLayout(document))
                ReadBudgetDennik(document, result, ref nextYearEntries, ref missingDateEntries);
            else foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                // The budget section can begin just inside the legacy account
                // range. Read its printed boundary before interpreting accounts.
                var budgetHeader = tokens.FirstOrDefault(t => t.Left >= 330 && t.Left < 380 &&
                    t.Text.TrimStart().StartsWith("OdSTP", StringComparison.Ordinal));
                double debitAccountRight = budgetHeader == null ? 350 : budgetHeader.Left - 0.5;
                var creditHeader = tokens.FirstOrDefault(t => t.Left >= 400 && t.Left < 440 &&
                    t.Text.Trim() == "Účet");
                double creditAccountLeft = creditHeader == null ? 415 : creditHeader.Left - 0.5;
                var creditBudgetHeader = tokens.FirstOrDefault(t => t.Left >= 440 && t.Left < 480 &&
                    t.Text.TrimStart().StartsWith("OdSTP", StringComparison.Ordinal));
                double creditAccountRight = creditBudgetHeader == null ? 455 : creditBudgetHeader.Left - 0.5;
                var datedHeads = tokens.Where(t => t.Left >= 15 && t.Left < 35 &&
                    DatePattern.IsMatch(t.Text)).ToArray();
                // Some printouts contain a complete account/amount row without a date.
                // Use the account row as its anchor so it cannot disappear silently.
                var undatedHeads = tokens.Where(t =>
                    ((t.Left >= 297 && t.Left < debitAccountRight) || (t.Left >= creditAccountLeft && t.Left < creditAccountRight)) &&
                    AccountPattern.IsMatch(t.Text) &&
                    !datedHeads.Any(date => Math.Abs(date.Baseline - t.Baseline) <= 0.9) &&
                    AmountPattern.IsMatch(JournalAmount(AtBaseline(tokens, t.Baseline - 5.3, 1.3))))
                    .GroupBy(t => Math.Round(t.Baseline, 1)).Select(group => group.First());
                foreach (var anchor in datedHeads.Concat(undatedHeads).OrderByDescending(t => t.Baseline))
                {
                    int sourceNumber = result.Rows.Count + 1;
                    bool missingDate = !DatePattern.IsMatch(anchor.Text);
                    var head = AtBaseline(tokens, anchor.Baseline, 0.9);
                    var amountLine = AtBaseline(tokens, anchor.Baseline - 5.3, 1.3);
                    var details = AtBaseline(tokens, anchor.Baseline - 10.4, 1.8);
                    string debit = Account(head, 297, debitAccountRight, page.PageNumber, sourceNumber, true);
                    string credit = Account(head, creditAccountLeft, creditAccountRight, page.PageNumber, sourceNumber);
                    string amountText = JournalAmount(amountLine);
                    if (!AmountPattern.IsMatch(amountText) ||
                        (!AccountPattern.IsMatch(debit) && !AccountPattern.IsMatch(credit)) ||
                        (debit.Length > 0 && !AccountPattern.IsMatch(debit)) ||
                        (credit.Length > 0 && !AccountPattern.IsMatch(credit)))
                        throw new InvalidDataException($"Page {page.PageNumber}, entry {sourceNumber}: incomplete amount or account columns.");

                    decimal amount = decimal.Parse(amountText.Replace(",-", ",00"),
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.GetCultureInfo("sk-SK"));
                    DateTime date = missingDate ? DateTime.MinValue :
                        DateTime.ParseExact(anchor.Text, "dd.MM.yy", CultureInfo.InvariantCulture);
                    if (missingDate) missingDateEntries++;
                    if (date.Year == year + 1 && date.Month == 1) nextYearEntries++;
                    string description = Column(head, 54, 296, true);
                    string documentType = Column(head, 54, 91, true);
                    string documentNumber = Column(details, 25, 54, true);
                    var row = new JournalRow
                    {
                        SequenceNumber = sourceNumber, SourceRecordNumber = sourceNumber,
                        SourceLocation = $"Page {page.PageNumber}, entry {sourceNumber}" +
                            (missingDate ? " (source date missing)" : string.Empty),
                        PostingDate = date, DocumentType = documentType,
                        DateExceptionResolution = missingDate ? JournalDateExceptionResolution.Excluded :
                            (JournalDateExceptionResolution?)null,
                        DocumentNumber = documentNumber, Description = description,
                        DebitAccount = debit.Length == 0 ? null : debit,
                        CreditAccount = credit.Length == 0 ? null : credit,
                        DebitAmount = debit.Length == 0 ? (decimal?)null : amount,
                        CreditAmount = credit.Length == 0 ? (decimal?)null : amount,
                        RecordKind = date.Month == 1 && date.Day == 1 &&
                            (debit == "701" || credit == "701")
                                ? JournalRecordKind.Opening
                                : date.Month == 12 && date.Day == 31 &&
                                  (debit == "702" || credit == "702" ||
                                   debit == "710" || credit == "710")
                                    ? JournalRecordKind.Closing : JournalRecordKind.Normal
                    };
                    result.Rows.Add(row);
                }
            }
            if (result.Rows.Count == 0 || result.Rows.Count > 500_000)
                throw new InvalidDataException("The PDF contains no journal entries or exceeds 500,000 entries.");

            var printed = ReadPrintedTotals(document.Pages.Last().Tokens);
            decimal debitTotal = result.Rows.Sum(row => row.DebitAmount ?? 0m);
            decimal creditTotal = result.Rows.Sum(row => row.CreditAmount ?? 0m);
            if (debitTotal != printed.Item1 || creditTotal != printed.Item2)
                throw new InvalidDataException($"Journal totals disagree with the printed CELKOM line: " +
                    $"MD {debitTotal} versus {printed.Item1}; DAL {creditTotal} versus {printed.Item2}.");
            if (nextYearEntries > 0)
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_PDF_NEXT_YEAR_POSTINGS",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = nextYearEntries + " entries in the " + year +
                        " journal have January " + (year + 1) + " posting dates; their printed dates were preserved."
                });
            if (missingDateEntries > 0)
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_PDF_MISSING_POSTING_DATE",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = missingDateEntries + " PDF journal entry/entries have no printed posting date. " +
                        "Their amounts were preserved, but they are excluded from report calculation until reviewed."
                });
            result.ImportReport.RecordCounts["JournalRows"] = result.Rows.Count;
            return result;
        }

        private static IReadOnlyList<PdfTextToken> AtBaseline(PdfTextToken[] tokens, double baseline, double tolerance) =>
            tokens.Where(t => Math.Abs(t.Baseline - baseline) <= tolerance).ToArray();

        private static string Column(IEnumerable<PdfTextToken> tokens, double left, double right, bool spaced) =>
            string.Join(spaced ? " " : "", tokens.Where(t => t.Left >= left && t.Left < right)
                .OrderBy(t => t.Left).Select(t => t.Text.Trim()).Where(t => t.Length > 0)).Trim();

        private static PdfDocument NormalizeJournalScale(PdfDocument document)
        {
            return new PdfDocument(document.Pages.Select(page =>
            {
                var headers = page.Tokens.Where(t => t.Text.Trim() == "Účet" &&
                    t.Left >= 280 && t.Left < 470).OrderBy(t => t.Left).ToArray();
                if (headers.Length != 2 || Math.Abs(headers[0].Baseline - headers[1].Baseline) > 2)
                    return page;
                double distance = headers[1].Left - headers[0].Left;
                double scale = 116.04 / distance;
                // Preserve normal print geometry; normalize only a materially
                // different scale, anchored to the two printed account headers.
                if (scale < 0.8 || scale > 1.2 || Math.Abs(scale - 1) < 0.02) return page;
                double offset = 297.84 - headers[0].Left * scale;
                return new PdfPage(page.PageNumber, page.Tokens.Select(t => new PdfTextToken(
                    t.PageNumber, t.OriginalText, t.Left * scale + offset,
                    t.Right * scale + offset, t.Baseline * scale, t.IsBold, t.IsItalic)));
            }));
        }

        private static string JournalAmount(IEnumerable<PdfTextToken> tokens) =>
            // Leading padding may start before the amount column. Its right edge
            // identifies the printed amount without collecting adjacent fields.
            string.Concat(tokens.Where(t => t.Right > 239 && t.Right < 301)
                .OrderBy(t => t.Left).Select(t => t.Text))
                .Replace(" ", string.Empty).Replace("\u00a0", string.Empty);

        private static string Account(IEnumerable<PdfTextToken> tokens, double left, double right,
            int pageNumber, int entryNumber, bool allowSplitAnalytical = false)
        {
            var fragments = tokens.Where(t => t.Left >= left && t.Left < right)
                .OrderBy(t => t.Left).Select(t => t.Text.Trim())
                // A print token can contain a space between the synthetic code
                // and its letter analytical suffix, e.g. "318  KO".
                .Select(text => Regex.IsMatch(text, @"^\d{3}\s+[\p{L}][\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?$")
                    || Regex.IsMatch(text, @"^\d{3}(?:\s+\d+)+$")
                    ? Regex.Replace(text, @"\s+", string.Empty) : text)
                .Where(t => t.Length > 0).ToArray();
            // Two-digit asset-register accounts can have spaced letter suffixes,
            // such as "75 ZŠ", "79 ZŠ", or "79 OBEC".
            string compact = Regex.Replace(string.Concat(fragments), @"\s+", string.Empty);
            if (Regex.IsMatch(compact, @"^\d{3}\*{2,4}$")) return compact;
            if (Regex.IsMatch(compact, @"^(?:75|79)[\p{L}][\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?$"))
                return compact;
            // The Kamienka print driver spaces numeric account codes into separate
            // runs ("75 2020", "75 1 1", "461 001") without changing columns.
            // Rejoin only wholly numeric fragments inside one account column.
            if (fragments.Length > 0 && fragments.All(t => Regex.IsMatch(t, @"^\d+(?:\s+\d+)*$")))
            {
                string numericCode = Regex.Replace(string.Concat(fragments), @"\s+", string.Empty);
                if (AccountPattern.IsMatch(numericCode)) return numericCode;
            }
            if (allowSplitAnalytical && fragments.Length == 2 &&
                AccountPattern.IsMatch(fragments[0]) &&
                Regex.IsMatch(fragments[1], @"^\d{1,3}$") &&
                (fragments[1].Length == 1 || fragments[0].Any(char.IsLetter)))
                return fragments[0] + fragments[1];
            if (allowSplitAnalytical && fragments.Length == 1 &&
                Regex.IsMatch(fragments[0], @"^\d{3}[\p{L}\d§]*(?:-[\p{L}\d§]+)*\s+\d{1,3}$"))
                return Regex.Replace(fragments[0], @"\s+", string.Empty);
            var candidates = fragments.Where(t => AccountPattern.IsMatch(t)).ToArray();
            if (candidates.Length > 1)
                throw new InvalidDataException($"Page {pageNumber}, entry {entryNumber}: ambiguous account column.");
            return candidates.Length == 0 ? string.Empty : candidates[0];
        }

        private static Tuple<decimal, decimal> ReadPrintedTotals(IReadOnlyList<PdfTextToken> tokens)
        {
            var label = tokens.LastOrDefault(t => t.Text.IndexOf("CELKOM:", StringComparison.OrdinalIgnoreCase) >= 0);
            if (label == null)
                label = tokens.LastOrDefault(candidate => candidate.Left < 200 &&
                    Regex.Replace(string.Join("", tokens.Where(t => t.Left < 200 &&
                        Math.Abs(t.Baseline - candidate.Baseline) < 1.5)
                        .OrderBy(t => t.Left).Select(t => t.Text)), @"\s+", string.Empty)
                        .IndexOf("CELKOM:", StringComparison.OrdinalIgnoreCase) >= 0);
            if (label == null) throw new InvalidDataException("The PDF has no final CELKOM line.");
            var line = tokens.Where(t => Math.Abs(t.Baseline - label.Baseline) < 1.5).ToArray();
            // Printed totals also include leading padding; select by the right
            // edge so a token starting just before its column is retained.
            string debit = string.Concat(line.Where(t => t.Right > 300 && t.Right < 400)
                .OrderBy(t => t.Left).Select(t => t.Text));
            string credit = string.Concat(line.Where(t => t.Right > 420 && t.Right < 510)
                .OrderBy(t => t.Left).Select(t => t.Text));
            return Tuple.Create(ParsePrintedTotal(debit), ParsePrintedTotal(credit));
        }

        private static decimal ParsePrintedTotal(string text)
        {
            string normalized = text.Replace(" ", "").Replace("\u00a0", "").Replace('.', ',');
            if (!decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"), out decimal amount))
                throw new InvalidDataException("The printed CELKOM amount could not be read: '" + text + "'.");
            return amount;
        }

        private static bool TryReadPeriod(PdfDocument document, out int year)
        {
            var firstPageTokens = document.Pages[0].Tokens;
            double headerBottom = firstPageTokens.Count == 0 ? 0 :
                firstPageTokens.Max(t => t.Baseline) - 60;
            string header = string.Join(" ", firstPageTokens
                .Where(t => t.Baseline > headerBottom).OrderByDescending(t => t.Baseline)
                .ThenBy(t => t.Left).Select(t => t.Text));
            var match = HasBudgetDennikLayout(document)
                ? Regex.Match(header, @"(?<!\d)00\s*/\s*(?<year>\d{4})\s*-\s*(?:12|13|14)\s*/\s*\k<year>(?!\d)")
                : PeriodPattern.Match(header);
            year = match.Success ? int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture) : 0;
            return year >= 1900 && year <= 9999;
        }

        private static bool HasBudgetDennikLayout(PdfDocument document)
        {
            var tokens = document.Pages[0].Tokens;
            double bottom = tokens.Max(t => t.Baseline) - 75;
            string header = string.Join(" ", tokens.Where(t => t.Baseline > bottom)
                .Select(t => t.Text));
            return header.Contains("_DENNIK.GMX") && header.Contains("DUÚP") &&
                header.Contains("OdSTP") && header.Contains("Položka") &&
                header.Contains("Rozdiel") && header.Contains("DAL");
        }

        private static void ReadBudgetDennik(PdfDocument document, JournalImport result,
            ref int nextYearEntries, ref int missingDateEntries)
        {
            // GMX version 1: account/description, amount, then posting date.
            // Calibrate the GMX coordinates from the two printed account headers.
            var locations = new List<Tuple<JournalRow, int, double>>();
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                double top = tokens.Max(t => t.Baseline);
                var headers = tokens.Where(t => t.Baseline > top - 75 &&
                    t.Text.Trim() == "Účet").OrderBy(t => t.Left).ToArray();
                if (headers.Length != 2)
                    throw new InvalidDataException($"Page {page.PageNumber}: missing DENNIK account headers.");
                double scale = (headers[1].Left - headers[0].Left) / 440.0;
                double offset = headers[0].Left - 1130 * scale;
                Func<double, double> x = value => offset + value * scale;
                double step = 20 * scale;
                var anchors = tokens.Where(t => t.Baseline < headers[0].Baseline - 2 &&
                    ((t.Left >= x(1130) - 1 && t.Left < x(1270) - 1) ||
                     (t.Left >= x(1570) - 1 && t.Left < x(1710) - 1)) &&
                    AccountPattern.IsMatch(Regex.Replace(t.Text.Trim(), @"\s+", string.Empty)) &&
                    // Subsidiary/department numbers also occur in the account
                    // columns on the lower detail line. Only an entry heading
                    // shares a baseline with the printed per-document ordinal.
                    tokens.Any(ordinal => ordinal.Left >= x(110) - 1 && ordinal.Left < x(230) - 1 &&
                        Math.Abs(ordinal.Baseline - t.Baseline) < 0.9 &&
                        Regex.IsMatch(ordinal.Text.Trim(), @"^\d+$")))
                    .OrderByDescending(t => t.Baseline).ToArray();
                double previous = double.MaxValue;
                foreach (var anchor in anchors)
                {
                    if (Math.Abs(anchor.Baseline - previous) < 0.9) continue;
                    previous = anchor.Baseline;
                    var amountLine = AtBaseline(tokens, anchor.Baseline - step, 1.3);
                    // Right-aligned PDF runs can include leading padding before
                    // the amount column. Select by the terminal edge.
                    string amountText = string.Join("", amountLine
                        .Where(t => t.Right >= x(920) - 1 && t.Right < x(1120) - 1)
                        .OrderBy(t => t.Left).Select(t => t.Text.Trim()))
                        .Replace(" ", "").Replace("\u00a0", "");
                    if (!AmountPattern.IsMatch(amountText))
                        throw new InvalidDataException($"Page {page.PageNumber}: DENNIK entry has no readable amount. " +
                            $"Anchor '{anchor.Text}' at ({anchor.Left:F2}, {anchor.Baseline:F2}); " +
                            $"scale {scale:F4}, amount column {x(920) - 1:F2}..{x(1120) - 1:F2}, read '{amountText}'. Nearby: " +
                            string.Join(" | ", tokens.Where(t => t.Left >= x(870) && t.Left < x(1270) &&
                                Math.Abs(t.Baseline - anchor.Baseline) < 15)
                                .Select(t => $"'{t.Text}' x={t.Left:F2} y={t.Baseline:F2}")));
                    int number = result.Rows.Count + 1;
                    var head = AtBaseline(tokens, anchor.Baseline, 0.9);
                    string debit = Account(head, x(1130) - 1, x(1270) - 1, page.PageNumber, number, true);
                    string credit = Account(head, x(1570) - 1, x(1710) - 1, page.PageNumber, number, true);
                    if (debit.Length == 0 && credit.Length == 0)
                        throw new InvalidDataException($"Page {page.PageNumber}: DENNIK entry has no readable account.");
                    var details = AtBaseline(tokens, anchor.Baseline - 2 * step, 1.3);
                    string dateText = Column(details, x(110) - 1, x(230) - 1, false);
                    bool missing = dateText.Length == 0;
                    if (!missing && !DatePattern.IsMatch(dateText))
                        throw new InvalidDataException($"Page {page.PageNumber}: invalid posting date '{dateText}'.");
                    DateTime date = DateTime.MinValue;
                    bool invalidDate = !missing && !DateTime.TryParseExact(dateText, "dd.MM.yy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                    if (invalidDate)
                    {
                        date = DateTime.MinValue;
                        result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                        {
                            Code = "IFOSOFT_PDF_INVALID_POSTING_DATE",
                            Severity = ImportDiagnosticSeverity.Warning,
                            Message = $"Page {page.PageNumber}, entry {number}: printed posting date '{dateText}' " +
                                "is invalid. Amounts were preserved; the entry is excluded from report calculation until reviewed."
                        });
                    }
                    decimal amount = decimal.Parse(amountText.Replace(",-", ",00"),
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("sk-SK"));
                    if (missing) missingDateEntries++;
                    if (date.Year == result.FiscalYear + 1 && date.Month == 1) nextYearEntries++;
                    result.Rows.Add(new JournalRow
                    {
                        SequenceNumber = number, SourceRecordNumber = number,
                        SourceLocation = $"Page {page.PageNumber}, entry {number}" +
                            (invalidDate ? $"; printed posting date: {dateText}" : string.Empty),
                        PostingDate = date,
                        DateExceptionResolution = missing || invalidDate ? JournalDateExceptionResolution.Excluded :
                            (JournalDateExceptionResolution?)null,
                        // The per-document ordinal is not the document identifier.
                        DocumentNumber = null,
                        Description = Column(head, x(240) - 1, x(920) - 1, true),
                        DebitAccount = debit.Length == 0 ? null : debit,
                        CreditAccount = credit.Length == 0 ? null : credit,
                        DebitAmount = debit.Length == 0 ? (decimal?)null : amount,
                        CreditAmount = credit.Length == 0 ? (decimal?)null : amount,
                        RecordKind = date.Month == 1 && date.Day == 1 && (debit == "701" || credit == "701")
                            ? JournalRecordKind.Opening : date.Month == 12 && date.Day == 31 &&
                            (debit == "702" || credit == "702" || debit == "710" || credit == "710")
                            ? JournalRecordKind.Closing : JournalRecordKind.Normal
                    });
                    locations.Add(Tuple.Create(result.Rows.Last(), page.PageNumber, anchor.Baseline));
                }
            }
            // The document identifier is printed after its entries, possibly on
            // the next page. Keep entry dates; the footer date is a document date.
            int assigned = 0;
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                var headers = tokens.Where(t => t.Text.Trim() == "Účet")
                    .OrderBy(t => t.Left).ToArray();
                double scale = (headers[1].Left - headers[0].Left) / 440.0;
                double offset = headers[0].Left - 1130 * scale;
                foreach (var footer in tokens.Where(t => t.Text.Contains("SPOLU"))
                    .OrderByDescending(t => t.Baseline))
                {
                    var line = AtBaseline(tokens, footer.Baseline, 1.3);
                    string doc = Column(line, offset + 470 * scale - 1, offset + 590 * scale - 1, false);
                    string type = Column(line, offset + 610 * scale - 1, offset + 690 * scale - 1, true);
                    while (assigned < locations.Count &&
                        (locations[assigned].Item2 < page.PageNumber ||
                         (locations[assigned].Item2 == page.PageNumber && locations[assigned].Item3 > footer.Baseline)))
                    {
                        locations[assigned].Item1.DocumentNumber = doc;
                        locations[assigned].Item1.DocumentType = type;
                        assigned++;
                    }
                }
            }
        }

        private static bool HasLayout(PdfDocument document)
        {
            var assembly = typeof(IfoSoftPdfDennik1JournalImporter).Assembly;
            using (var stream = assembly.GetManifestResourceStream(LayoutResource))
            {
                if (stream == null) throw new InvalidOperationException("Missing embedded IfoSoft journal layout.");
                var layout = new LayoutDefinitionLoader().Load(stream);
                if (!layout.IsValid) throw new InvalidDataException("Invalid embedded IfoSoft journal layout.");
                return new LayoutSignatureDetector().Detect(document, new[] { layout.Definition }).Count == 1;
            }
        }

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
