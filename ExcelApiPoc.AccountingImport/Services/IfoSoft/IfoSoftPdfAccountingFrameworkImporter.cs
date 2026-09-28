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
                    string syn = Cell(line, 45, 68);
                    // Some IfoSoft PDFs render one analytical code as separate
                    // text fragments (for example, "15" and "6"). The code
                    // column contains no meaningful whitespace.
                    string ana = string.Concat(line.Where(t => t.Left >= 68 && t.Left < 92)
                        .OrderBy(t => t.Left).Select(t => t.Text.Trim()));
                    string title = Text(line, 93, 420);
                    if (syn.Length == 0 && ana.Length == 0 && title.Length == 0)
                    {
                        result.Rows.Add(new AccountingFrameworkRow { SequenceNumber = result.Rows.Count + 1,
                            SourceRecordNumber = int.Parse(number.Text.TrimEnd('.'), CultureInfo.InvariantCulture),
                            RowKind = AccountingFrameworkRowKind.Empty, AccountName = "" });
                        continue;
                    }
                    if (syn.Length > 0 && !Code.IsMatch(syn))
                        throw new InvalidDataException($"Page {page.PageNumber}, row {number.Text}: invalid synthetic account '{syn}'.");
                    if (ana.Length > 0 && ana != "****" && !Regex.IsMatch(ana, @"^[\p{L}\d]+(?:-[\p{L}\d]+)*\.?$"))
                        throw new InvalidDataException($"Page {page.PageNumber}, row {number.Text}: invalid analytical account '{ana}'.");
                    // IfoSoft permits an analytical code without a display name.
                    var kind = syn.Length == 0 ? AccountingFrameworkRowKind.Empty : ana == "****" ?
                        AccountingFrameworkRowKind.GroupHeading : ana.Length == 0 ?
                        AccountingFrameworkRowKind.SyntheticAccount : AccountingFrameworkRowKind.AnalyticalAccount;
                    result.Rows.Add(new AccountingFrameworkRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = int.Parse(number.Text.TrimEnd('.'), CultureInfo.InvariantCulture),
                        SourceSyntheticCode = syn, SourceAnalyticalCode = ana, SyntheticCode = syn, AnalyticalCode = ana,
                        AccountCode = kind == AccountingFrameworkRowKind.GroupHeading || kind == AccountingFrameworkRowKind.Empty ? "" : syn + ana,
                        AccountName = title, RowKind = kind, Type = Cell(line, 420, 440),
                        SubsidiaryFlag = Cell(line, 440, 458), TaxFlag = Cell(line, 510, 528),
                        BalanceFlag = Cell(line, 492, 510), VatFlag = Cell(line, 528, 547)
                    });
                }
            }
            if (result.Rows.Count == 0) throw new InvalidDataException("No accounting framework rows found.");
            for (int i = 0; i < result.Rows.Count; i++)
                if (result.Rows[i].SourceRecordNumber != i + 1)
                    throw new InvalidDataException("Framework row numbers are not consecutive at " + (i + 1) + ".");
            return result;
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
