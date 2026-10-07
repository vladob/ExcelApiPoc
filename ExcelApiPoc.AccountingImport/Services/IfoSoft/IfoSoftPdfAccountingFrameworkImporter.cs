using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    /// <summary>Imports the numbered IfoSoft ÚČTOVÝ ROZVRH ANALYTICKÝCH ÚČTOV PDF.</summary>
    public sealed class IfoSoftPdfAccountingFrameworkImporter
    {
        private static readonly Regex Name = new Regex(@"^UCT_ROZVRH_(?<ico>\d{8})_(?<year>\d{4})(?:[_\(].*)?\.pdf$", RegexOptions.IgnoreCase);
        private static readonly Regex Number = new Regex(@"^\d+\.$");
        private static readonly Regex Code = new Regex(@"^\d{2,3}$");
        public AccountingFrameworkImport Import(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Accounting framework PDF not found.", filePath);
            var name = Name.Match(Path.GetFileName(filePath));
            if (!name.Success) throw new InvalidDataException("Expected UCT_ROZVRH_<IČO>_<year>.pdf.");
            var doc = new ITextPdfTokenExtractor().ExtractWords(filePath);
            string header = string.Join(" ", doc.Pages[0].Tokens.OrderByDescending(t => t.Baseline).ThenBy(t => t.Left).Select(t => t.Text));
            if (!header.Contains("ÚČTOVÝ") || !header.Contains("ROZVRH") ||
                !header.Contains("ANALYTICKÝCH") || !header.Contains("ÚČTOV"))
                throw new InvalidDataException("Expected the IfoSoft analytical accounting framework layout. First tokens: " +
                    string.Join(" | ", doc.Pages[0].Tokens.Take(30).Select(t => t.Text)));
            // Column positions vary between IfoSoft printouts. Read the
            // analytical and name boundaries from the printed header.
            var analyticalHeader = doc.Pages[0].Tokens.FirstOrDefault(t => t.Text == "ANA");
            var nameHeader = doc.Pages[0].Tokens.FirstOrDefault(t => t.Text == "Názov" ||
                t.Text.StartsWith("Názov účtu", StringComparison.Ordinal));
            double analyticalLeft = analyticalHeader == null ? 68 : analyticalHeader.Left - 1;
            double nameLeft = nameHeader == null ? 93 : nameHeader.Left - 1;
            var columnHeaders = analyticalHeader == null ? doc.Pages[0].Tokens.ToArray() :
                doc.Pages[0].Tokens.Where(t => Math.Abs(t.Baseline - analyticalHeader.Baseline) < 1).ToArray();
            double syntheticLeft = HeaderBoundary(columnHeaders, "SYN", 45);
            double typeLeft = HeaderBoundary(columnHeaders, "TYP", 420);
            double subsidiaryLeft = HeaderBoundary(columnHeaders, "STR", 440);
            double departmentLeft = HeaderBoundary(columnHeaders, "ODD", 458);
            double balanceLeft = HeaderBoundary(columnHeaders, "SAL", 492);
            double taxLeft = HeaderBoundary(columnHeaders, "DAŇ", 510);
            double vatLeft = HeaderBoundary(columnHeaders, "DPH", 528);
            double businessLeft = HeaderBoundary(columnHeaders, "POD", 547);
            var result = new AccountingFrameworkImport
            {
                SourceFileName = Path.GetFileName(filePath), SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = Hash(filePath), TechnicalType = "PDF", AccountingFormat = "IfoSoft",
                Ico = name.Groups["ico"].Value, FiscalYear = int.Parse(name.Groups["year"].Value, CultureInfo.InvariantCulture),
                ImportedAtUtc = DateTime.UtcNow
            };
            foreach (var page in doc.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var number in tokens.Where(t => t.Left >= 20 && t.Left < 44 && Number.IsMatch(t.Text))
                    .OrderByDescending(t => t.Baseline))
                {
                    var line = tokens.Where(t => Math.Abs(t.Baseline - number.Baseline) < 3.0).ToArray();
                    string syn = Cell(line, syntheticLeft, analyticalLeft);
                    // Some IfoSoft PDFs render one analytical code as separate
                    // text fragments (for example, "15" and "6"). The code
                    // column contains no meaningful whitespace.
                    string ana = string.Concat(line.Where(t => t.Left >= analyticalLeft && t.Left < nameLeft)
                        .OrderBy(t => t.Left).Select(t => t.Text.Trim()));
                    string title = Text(line, nameLeft, typeLeft);
                    // Some exports print an unlabelled, unflagged placeholder such
                    // as "108 /23". Retain its numbered source fields for review,
                    // but never expose an invalid value as a usable account code.
                    if (Code.IsMatch(syn) && ana.StartsWith("/", StringComparison.Ordinal) &&
                        title.Length == 0 && Text(line, typeLeft, 590).Length == 0)
                    {
                        result.Rows.Add(new AccountingFrameworkRow
                        {
                            SequenceNumber = result.Rows.Count + 1,
                            SourceRecordNumber = int.Parse(number.Text.TrimEnd('.'), CultureInfo.InvariantCulture),
                            SourceSyntheticCode = syn, SourceAnalyticalCode = ana,
                            RowKind = AccountingFrameworkRowKind.Empty, AccountCode = "", AccountName = ""
                        });
                        continue;
                    }
                    if (syn.Length == 0 && ana.Length == 0 && title.Length == 0)
                    {
                        result.Rows.Add(new AccountingFrameworkRow { SequenceNumber = result.Rows.Count + 1,
                            SourceRecordNumber = int.Parse(number.Text.TrimEnd('.'), CultureInfo.InvariantCulture),
                            RowKind = AccountingFrameworkRowKind.Empty, AccountName = "" });
                        continue;
                    }
                    if (syn.Length > 0 && !Code.IsMatch(syn))
                        throw new InvalidDataException($"Page {page.PageNumber}, row {number.Text}: invalid synthetic account '{syn}'.");
                    bool groupHeading = Regex.IsMatch(ana, @"^\*{2,4}$");
                    if (ana.Length > 0 && !groupHeading &&
                        !Regex.IsMatch(ana, @"^-?[\p{L}\d§]+(?:-[\p{L}\d§]+)*\.?$") &&
                        !Regex.IsMatch(ana, @"^\d+%$"))
                        throw new InvalidDataException($"Page {page.PageNumber}, row {number.Text}: invalid analytical account '{ana}'.");
                    // IfoSoft permits an analytical code without a display name.
                    var kind = syn.Length == 0 ? AccountingFrameworkRowKind.Empty : groupHeading ?
                        AccountingFrameworkRowKind.GroupHeading : ana.Length == 0 ?
                        AccountingFrameworkRowKind.SyntheticAccount : AccountingFrameworkRowKind.AnalyticalAccount;
                    result.Rows.Add(new AccountingFrameworkRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = int.Parse(number.Text.TrimEnd('.'), CultureInfo.InvariantCulture),
                        SourceSyntheticCode = syn, SourceAnalyticalCode = ana, SyntheticCode = syn, AnalyticalCode = ana,
                        AccountCode = kind == AccountingFrameworkRowKind.GroupHeading || kind == AccountingFrameworkRowKind.Empty ? "" : syn + ana,
                        AccountName = title, RowKind = kind, Type = Cell(line, typeLeft, subsidiaryLeft),
                        SubsidiaryFlag = Cell(line, subsidiaryLeft, departmentLeft), TaxFlag = Cell(line, taxLeft, vatLeft),
                        BalanceFlag = Cell(line, balanceLeft, taxLeft), VatFlag = Cell(line, vatLeft, businessLeft)
                    });
                }
            }
            if (result.Rows.Count == 0) throw new InvalidDataException("No accounting framework rows found.");
            // A newly added account can be printed next to its parent with the
            // next available number (e.g. row 705 between rows 424 and 425).
            // Preserve print order, while still requiring every number exactly once.
            var numbers = result.Rows.Select(row => row.SourceRecordNumber).OrderBy(n => n).ToArray();
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] != i + 1)
                    throw new InvalidDataException("Framework row numbers are missing or duplicated at " + (i + 1) + ".");
            return result;
        }
        private static double HeaderBoundary(PdfTextToken[] headers, string label, double fallback)
        {
            // Adjacent short headings can be emitted as one token (STRODD,
            // POLSAL, DPHPOD). Their boundaries lie in the gap between flags.
            var token = headers.FirstOrDefault(t => t.Text.Contains(label));
            if (token == null) return fallback;
            int offset = token.Text.IndexOf(label, StringComparison.Ordinal);
            return token.Left + (token.Right - token.Left) * offset / token.Text.Length - 1;
        }
        private static string Cell(PdfTextToken[] line, double left, double right) => Text(line, left, right).Trim();
        private static string Text(PdfTextToken[] line, double left, double right) =>
            string.Join(" ", line.Where(t => t.Left >= left && t.Left < right).OrderBy(t => t.Left).Select(t => t.Text)).Trim();
        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
