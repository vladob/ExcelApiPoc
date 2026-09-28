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
    /// A monthly _DENNIK.GMX report is deliberately not accepted as an annual journal.
    /// </summary>
    public sealed class IfoSoftPdfDennik1JournalImporter : IJournalImporter
    {
        private const string LayoutResource =
            "ExcelApiPoc.AccountingImport.PdfLayouts.IfoSoft.journal-dennik1.v1.json";
        private static readonly Regex DatePattern = new Regex(@"^\d{2}\.\d{2}\.\d{2}$");
        private static readonly Regex AccountPattern = new Regex(@"^\d{3}[\p{L}\d]*(?:-[\p{L}\d]+)*$");
        private static readonly Regex AmountPattern = new Regex(@"^-?\d+(?:,\d{2}|,-)$");
        private static readonly Regex PeriodPattern = new Regex(
            @"(?<!\d)00/(?<year>\d{4})\s*-\s*12/\k<year>(?!\d)");
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
                if (!HasLayout(document) || !TryReadPeriod(document, out int year)) return false;
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
            if (!HasLayout(document))
                throw new InvalidDataException("Expected the unique IfoSoft _DENNIK1.GMX journal layout.");
            if (!TryReadPeriod(document, out int year) ||
                name.Groups["year"].Value != year.ToString(CultureInfo.InvariantCulture))
                throw new InvalidDataException("The printed journal must cover 00/year through 12/year and agree with its filename.");

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

            int nextYearEntries = 0;
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var dateToken in tokens.Where(t => t.Left >= 15 && t.Left < 35 &&
                             DatePattern.IsMatch(t.Text)).OrderByDescending(t => t.Baseline))
                {
                    int sourceNumber = result.Rows.Count + 1;
                    var head = AtBaseline(tokens, dateToken.Baseline, 0.9);
                    var amountLine = AtBaseline(tokens, dateToken.Baseline - 5.3, 1.3);
                    var details = AtBaseline(tokens, dateToken.Baseline - 10.4, 1.8);
                    string debit = Account(head, 297, 350, page.PageNumber, sourceNumber, true);
                    string credit = Account(head, 415, 455, page.PageNumber, sourceNumber);
                    string amountText = Column(amountLine, 239, 301, false)
                        .Replace(" ", string.Empty).Replace("\u00a0", string.Empty);
                    if (!AmountPattern.IsMatch(amountText) ||
                        (!AccountPattern.IsMatch(debit) && !AccountPattern.IsMatch(credit)) ||
                        (debit.Length > 0 && !AccountPattern.IsMatch(debit)) ||
                        (credit.Length > 0 && !AccountPattern.IsMatch(credit)))
                        throw new InvalidDataException($"Page {page.PageNumber}, entry {sourceNumber}: incomplete amount or account columns.");

                    decimal amount = decimal.Parse(amountText.Replace(",-", ",00"),
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.GetCultureInfo("sk-SK"));
                    DateTime date = DateTime.ParseExact(dateToken.Text, "dd.MM.yy",
                        CultureInfo.InvariantCulture);
                    if (date.Year != year && !(date.Year == year + 1 && date.Month == 1))
                        throw new InvalidDataException($"Page {page.PageNumber}, entry {sourceNumber}: posting date is outside {year}.");
                    if (date.Year != year) nextYearEntries++;
                    string description = Column(head, 54, 296, true);
                    string documentType = Column(head, 54, 91, true);
                    string documentNumber = Column(details, 25, 54, true);
                    var row = new JournalRow
                    {
                        SequenceNumber = sourceNumber, SourceRecordNumber = sourceNumber,
                        SourceLocation = $"Page {page.PageNumber}, entry {sourceNumber}",
                        PostingDate = date, DocumentType = documentType,
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
            result.ImportReport.RecordCounts["JournalRows"] = result.Rows.Count;
            return result;
        }

        private static IReadOnlyList<PdfTextToken> AtBaseline(PdfTextToken[] tokens, double baseline, double tolerance) =>
            tokens.Where(t => Math.Abs(t.Baseline - baseline) <= tolerance).ToArray();

        private static string Column(IEnumerable<PdfTextToken> tokens, double left, double right, bool spaced) =>
            string.Join(spaced ? " " : "", tokens.Where(t => t.Left >= left && t.Left < right)
                .OrderBy(t => t.Left).Select(t => t.Text.Trim()).Where(t => t.Length > 0)).Trim();

        private static string Account(IEnumerable<PdfTextToken> tokens, double left, double right,
            int pageNumber, int entryNumber, bool allowSplitAnalytical = false)
        {
            var fragments = tokens.Where(t => t.Left >= left && t.Left < right)
                .OrderBy(t => t.Left).Select(t => t.Text.Trim()).Where(t => t.Length > 0).ToArray();
            if (allowSplitAnalytical && fragments.Length == 2 &&
                AccountPattern.IsMatch(fragments[0]) && Regex.IsMatch(fragments[1], @"^\d$"))
                return fragments[0] + fragments[1];
            if (allowSplitAnalytical && fragments.Length == 1 &&
                Regex.IsMatch(fragments[0], @"^\d{3}[\p{L}\d]*\s+\d$"))
                return Regex.Replace(fragments[0], @"\s+", string.Empty);
            var candidates = fragments.Where(t => AccountPattern.IsMatch(t)).ToArray();
            if (candidates.Length > 1)
                throw new InvalidDataException($"Page {pageNumber}, entry {entryNumber}: ambiguous account column.");
            return candidates.Length == 0 ? string.Empty : candidates[0];
        }

        private static Tuple<decimal, decimal> ReadPrintedTotals(IReadOnlyList<PdfTextToken> tokens)
        {
            var label = tokens.LastOrDefault(t => t.Text.IndexOf("CELKOM:", StringComparison.OrdinalIgnoreCase) >= 0);
            if (label == null) throw new InvalidDataException("The PDF has no final CELKOM line.");
            var line = tokens.Where(t => Math.Abs(t.Baseline - label.Baseline) < 1.5).ToArray();
            return Tuple.Create(ParsePrintedTotal(Column(line, 300, 400, false)),
                ParsePrintedTotal(Column(line, 420, 510, false)));
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
            string header = string.Join(" ", document.Pages[0].Tokens
                .Where(t => t.Baseline > 785).OrderByDescending(t => t.Baseline)
                .ThenBy(t => t.Left).Select(t => t.Text));
            var match = PeriodPattern.Match(header);
            year = match.Success ? int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture) : 0;
            return year >= 1900 && year <= 9999;
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
