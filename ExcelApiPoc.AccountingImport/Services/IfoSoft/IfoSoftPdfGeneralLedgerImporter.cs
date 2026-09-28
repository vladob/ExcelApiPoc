using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    /// <summary>Reads the analytical detail of _PREDVAS3.GMX and _HLKNIA4.GMX printouts.</summary>
    public sealed class IfoSoftPdfGeneralLedgerImporter : IGeneralLedgerImporter
    {
        private static readonly Regex FileName = new Regex(@"^HL_KNIHA_(?<ico>\d{8})_(?<year>\d{4})(?:[_\(].*)?\.pdf$", RegexOptions.IgnoreCase);
        private static readonly Regex Period = new Regex(@"00\s*/\s*(?<year>\d{4})\s*-?\s*(?<month>\d{1,2})\s*/\s*\k<year>");
        private static readonly Regex Account = new Regex(@"^\d{3}[\p{L}\d]*(?:-[\p{L}\d]+)*$");
        private static readonly Regex Amount = new Regex(@"^-?[\d.]+(?:,\d{2}|,-)$");
        private static readonly CultureInfo Sk = CultureInfo.GetCultureInfo("sk-SK");

        public bool CanImport(string filePath, string accountingFormat) =>
            string.Equals(accountingFormat, "IfoSoft", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(filePath) && FileName.IsMatch(Path.GetFileName(filePath));

        public GeneralLedgerImport Import(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Ledger PDF not found.", filePath);
            var file = FileName.Match(Path.GetFileName(filePath));
            if (!file.Success) throw new InvalidDataException("Expected HL_KNIHA_<IČO>_<year>.pdf.");
            var document = new ITextPdfTokenExtractor().ExtractWords(filePath);
            string first = string.Join(" ", document.Pages[0].Tokens.OrderByDescending(t => t.Baseline).ThenBy(t => t.Left).Select(t => t.Text));
            bool predvas = first.Contains("_PREDVAS3.GMX");
            bool detailed = first.Contains("_HLKNIA4.GMX");
            bool accountSummary = first.Contains("_HLKNIA4C.GMX");
            if ((predvas ? 1 : 0) + (detailed ? 1 : 0) + (accountSummary ? 1 : 0) != 1 ||
                !first.Contains("HLAVNÁ KNIHA"))
                throw new InvalidDataException("Expected one IfoSoft _PREDVAS3.GMX, _HLKNIA4.GMX, or _HLKNIA4C.GMX ledger layout.");
            var periodLine = document.Pages[0].Tokens
                .FirstOrDefault(t => t.Text.StartsWith("00/", StringComparison.Ordinal));
            string printedPeriod = periodLine == null ? string.Empty : string.Join(" ",
                document.Pages[0].Tokens.Where(t => Math.Abs(t.Baseline - periodLine.Baseline) < 2)
                    .OrderBy(t => t.Left).Select(t => t.Text));
            var period = Period.Match(printedPeriod);
            if (!period.Success)
            {
                // Older print drivers place period fragments on different baselines.
                printedPeriod = string.Join(" ", document.Pages[0].Tokens
                    .Where(t => t.Left >= 400 && t.Left < 550)
                    .OrderByDescending(t => t.Baseline).ThenBy(t => t.Left)
                    .Take(35).Select(t => t.Text));
                period = Period.Match(printedPeriod);
            }
            if (!period.Success || period.Groups["year"].Value != file.Groups["year"].Value)
                throw new InvalidDataException("Printed accounting period does not agree with ledger filename. Header: " + printedPeriod);
            int through = int.Parse(period.Groups["month"].Value, CultureInfo.InvariantCulture);
            if (through < 1 || through > 13) throw new InvalidDataException("Invalid printed ledger month.");
            var result = new GeneralLedgerImport
            {
                SourceFileName = Path.GetFileName(filePath), SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = Hash(filePath), TechnicalType = "PDF", AccountingFormat = "IfoSoft",
                Ico = file.Groups["ico"].Value, FiscalYear = int.Parse(file.Groups["year"].Value, CultureInfo.InvariantCulture),
                ThroughMonth = through, PeriodHeader = through.ToString("00", CultureInfo.InvariantCulture) + "/" + file.Groups["year"].Value,
                ImportedAtUtc = DateTime.UtcNow
            };
            if (accountSummary)
            {
                ReadAccountSummary(document, result);
                return result;
            }
            var allCodes = document.Pages.SelectMany(p => p.Tokens)
                .Where(t => t.Left < (predvas ? 70 : 19) && Account.IsMatch(t.Text))
                .Select(t => t.Text).ToArray();
            var flaggedCodes = new HashSet<string>(StringComparer.Ordinal);
            if (predvas)
                foreach (var page in document.Pages)
                {
                    var words = page.Tokens.ToArray();
                    foreach (var token in words.Where(t => t.Left < 70 && Account.IsMatch(t.Text)))
                        if (words.Any(t => Math.Abs(t.Baseline - token.Baseline) < 5.3 &&
                            t.Left >= 240 && t.Left < 310 && (t.Text == "M" || t.Text == "D")))
                            flaggedCodes.Add(token.Text);
                }
            var unflaggedSeen = new HashSet<string>(StringComparer.Ordinal);
            var encounteredCodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var candidate in tokens.Where(t => t.Left < (predvas ? 70 : 19) && Account.IsMatch(t.Text))
                         .OrderByDescending(t => t.Baseline))
                {
                    var line = tokens.Where(t => Math.Abs(t.Baseline - candidate.Baseline) < 5.3).ToArray();
                    // PREDVAS has analytical detail followed by a synthetic subtotal. Some
                    // detail rows omit flags; retain their first occurrence only. HLKNIA4
                    // has budget lines for the same account, all of which contribute to its total.
                    if (predvas)
                    {
                        bool flagged = line.Any(t => t.Left >= 240 && t.Left < 310 && (t.Text == "M" || t.Text == "D"));
                        bool childExists = allCodes.Any(codePredva => codePredva.Length > candidate.Text.Length &&
                            codePredva.StartsWith(candidate.Text, StringComparison.Ordinal));
                        // The first parent row can itself be a detail account (221 before 221RF).
                        // The parent subtotal follows its children and must not be imported twice.
                        bool parentDetailBeforeChildren = childExists && !encounteredCodes.Contains(candidate.Text) &&
                            !encounteredCodes.Any(codePredva => codePredva.Length > candidate.Text.Length &&
                                codePredva.StartsWith(candidate.Text, StringComparison.Ordinal));
                        bool skip = !flagged && !parentDetailBeforeChildren &&
                            (flaggedCodes.Contains(candidate.Text) || childExists || !unflaggedSeen.Add(candidate.Text));
                        encounteredCodes.Add(candidate.Text);
                        if (skip) continue;
                        if (!flagged) unflaggedSeen.Add(candidate.Text);
                    }
                    else if (line.Any(t => t.Left >= 20 && t.Left < 195 &&
                        t.Text.Length > 1 && char.IsLetter(t.Text[0]) && t.Text != "R")) continue;
                    var cells = new decimal[8];
                    foreach (var token in line.Where(t => t.Left > (predvas ? 310 : 210) && Amount.IsMatch(t.Text)))
                    {
                        int col = predvas ? Bin(token.Right, new[] { 370d, 430, 490, 550, 610, 670, 730, 810 })
                                          : Bin(token.Right, new[] { 275d, 335, 395, 455, 515, 580 });
                        if (col < 0) throw new InvalidDataException($"Page {page.PageNumber}: amount outside the known ledger columns.");
                        if (cells[col] != 0m) throw new InvalidDataException($"Page {page.PageNumber}, account {candidate.Text}: ambiguous amount column.");
                        decimal value = Parse(token.Text);
                        if (value >= 0 && line.Any(sign => sign.Text == "-" &&
                            Math.Abs(sign.Baseline - token.Baseline) < 1 &&
                            sign.Right <= token.Left && token.Left - sign.Right < 4))
                            value = -value;
                        cells[col] = value;
                    }
                    if (!line.Any(t => t.Left > (predvas ? 310 : 210) && Amount.IsMatch(t.Text))) continue;
                    string code = candidate.Text;
                    var row = new GeneralLedgerRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = page.PageNumber,
                        SyntheticCode = code.Substring(0, 3), AnalyticalCode = code.Substring(3), AccountCode = code,
                        AccountName = predvas ? Text(line, 85, 240) : string.Empty
                    };
                    if (predvas)
                    {
                        row.OpeningDebit = cells[0]; row.OpeningCredit = cells[1];
                        row.AnnualDebitTurnover = cells[2]; row.AnnualCreditTurnover = cells[3];
                        row.PeriodDebitTurnover = cells[4]; row.PeriodCreditTurnover = cells[5];
                        row.ClosingDebit = cells[6]; row.ClosingCredit = cells[7];
                    }
                    else
                    {
                        SplitNet(cells[0], out decimal od, out decimal oc);
                        SplitNet(cells[5], out decimal cd, out decimal cc);
                        row.OpeningDebit = od; row.OpeningCredit = oc;
                        row.AnnualDebitTurnover = cells[1]; row.AnnualCreditTurnover = cells[2];
                        row.PeriodDebitTurnover = cells[3]; row.PeriodCreditTurnover = cells[4];
                        row.ClosingDebit = cd; row.ClosingCredit = cc;
                    }
                    result.Rows.Add(row);
                }
            }
            if (result.Rows.Count == 0) throw new InvalidDataException("No analytical ledger rows found. First tokens: " +
                    string.Join(" | ", document.Pages[0].Tokens.Take(30).Select(t =>
                        t.Text + "@" + t.Left.ToString("F1", CultureInfo.InvariantCulture))));
            ValidateAnnualTotals(document, result, predvas);
            return result;
        }

        private static void ReadAccountSummary(PdfDocument document, GeneralLedgerImport result)
        {
            // HLKNIA4C prints four columns: opening net, annual MD, annual DAL,
            // closing net. Some account lines have no M/D type flag. The repeated
            // synthetic totals carry "****" beside the account code.
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var candidate in tokens.Where(t => t.Left >= 40 && t.Left < 75 && Account.IsMatch(t.Text))
                    .OrderByDescending(t => t.Baseline))
                {
                    var line = tokens.Where(t => Math.Abs(t.Baseline - candidate.Baseline) < 3).ToArray();
                    if (line.Any(t => t.Text == "****"))
                        continue;
                    var cells = new decimal[4];
                    bool hasAmount = false;
                    foreach (var token in line.Where(t => t.Left >= 300 && Amount.IsMatch(t.Text)))
                    {
                        int column = Bin(token.Right, new[] { 375d, 440, 505, 575 });
                        if (column < 0 || cells[column] != 0m)
                            throw new InvalidDataException($"Page {page.PageNumber}, account {candidate.Text}: ambiguous amount column.");
                        decimal value = Parse(token.Text);
                        if (value >= 0 && line.Any(sign => sign.Text == "-" &&
                            Math.Abs(sign.Baseline - token.Baseline) < 1 &&
                            sign.Right <= token.Left && token.Left - sign.Right < 4))
                            value = -value;
                        cells[column] = value;
                        hasAmount = true;
                    }
                    if (!hasAmount) continue;
                    SplitNet(cells[0], out decimal openingDebit, out decimal openingCredit);
                    SplitNet(cells[3], out decimal closingDebit, out decimal closingCredit);
                    result.Rows.Add(new GeneralLedgerRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = page.PageNumber,
                        AccountCode = candidate.Text, SyntheticCode = candidate.Text.Substring(0, 3),
                        AnalyticalCode = candidate.Text.Substring(3), AccountName = Text(line, 94, 300),
                        OpeningDebit = openingDebit, OpeningCredit = openingCredit,
                        AnnualDebitTurnover = cells[1], AnnualCreditTurnover = cells[2],
                        ClosingDebit = closingDebit, ClosingCredit = closingCredit
                    });
                }
            }
            if (result.Rows.Count == 0)
                throw new InvalidDataException("No _HLKNIA4C.GMX account rows found.");
            var last = document.Pages.Last().Tokens;
            var label = last.LastOrDefault(t => t.Text.Contains("KONTROLNÝ"));
            if (label == null) throw new InvalidDataException("Ledger has no final KONTROLNÝ SÚČET.");
            var control = last.Where(t => Math.Abs(t.Baseline - label.Baseline) < 5 &&
                t.Left >= 375 && Amount.IsMatch(t.Text)).OrderBy(t => t.Left).ToArray();
            if (control.Length != 2 || result.Rows.Sum(r => r.AnnualDebitTurnover) != Parse(control[0].Text) ||
                result.Rows.Sum(r => r.AnnualCreditTurnover) != Parse(control[1].Text))
                throw new InvalidDataException("_HLKNIA4C.GMX annual turnover does not match the printed KONTROLNÝ SÚČET.");
        }

        private static void ValidateAnnualTotals(PdfDocument document, GeneralLedgerImport result, bool predvas)
        {
            var page = document.Pages.Last();
            var label = page.Tokens.LastOrDefault(t => t.Text.Contains("KONTROLNÝ"));
            if (label == null) throw new InvalidDataException("Ledger has no final KONTROLNÝ SÚČET.");
            var line = page.Tokens.Where(t => Math.Abs(t.Baseline - label.Baseline) < 5).ToArray();
            decimal[] amounts = line.Where(t => Amount.IsMatch(t.Text)).OrderBy(t => t.Left)
                .Select(t => Parse(t.Text)).ToArray();
            int debit = predvas ? 2 : 0, credit = predvas ? 3 : 1;
            if (amounts.Length <= credit) throw new InvalidDataException("Incomplete ledger control totals.");
            decimal actualDebit = result.Rows.Sum(r => r.AnnualDebitTurnover);
            decimal actualCredit = result.Rows.Sum(r => r.AnnualCreditTurnover);
            if (actualDebit != amounts[debit] || actualCredit != amounts[credit])
                throw new InvalidDataException($"Ledger annual turnover does not match the printed KONTROLNÝ SÚČET: " +
                    $"MD {actualDebit} / {amounts[debit]}, DAL {actualCredit} / {amounts[credit]}.");
        }

        private static string Text(IEnumerable<PdfTextToken> line, double left, double right) =>
            string.Join(" ", line.Where(t => t.Left >= left && t.Left < right).OrderBy(t => t.Left).Select(t => t.Text));
        private static int Bin(double right, double[] ends)
        {
            for (int i = 0; i < ends.Length; i++) if (right < ends[i]) return i;
            return -1;
        }
        private static void SplitNet(decimal net, out decimal debit, out decimal credit)
        {
            debit = Math.Max(net, 0m); credit = Math.Max(-net, 0m);
        }
        private static decimal Parse(string text) => decimal.Parse(text.Replace(".", "").Replace(",-", ",00"),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, Sk);
        private static string Hash(string filePath)
        {
            using (var stream = File.OpenRead(filePath)) using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
