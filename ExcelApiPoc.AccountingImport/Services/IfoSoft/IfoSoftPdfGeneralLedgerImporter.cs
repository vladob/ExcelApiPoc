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
        private static readonly Regex Account = new Regex(@"^\d{3}[\p{L}\d]*(?:-[\p{L}\d]+)*\.?$");
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
            bool analyticalCards = first.Contains("_HLKNIA4B.GMX");
            bool accountSummary = first.Contains("_HLKNIA4C.GMX");
            bool syntheticSummary = first.Contains("_HLKNIA4D.GMX");
            if ((predvas ? 1 : 0) + (detailed ? 1 : 0) + (analyticalCards ? 1 : 0) + (accountSummary ? 1 : 0) +
                (syntheticSummary ? 1 : 0) != 1 || !first.Contains("HLAVNÁ KNIHA"))
                throw new InvalidDataException("Expected one IfoSoft _PREDVAS3.GMX, _HLKNIA4.GMX, _HLKNIA4B.GMX, _HLKNIA4C.GMX, or _HLKNIA4D.GMX ledger layout.");
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
            // IfoSoft closing printouts can include accounting periods 13 and 14.
            if (through < 1 || through > 14) throw new InvalidDataException("Invalid printed ledger month.");
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
            if (syntheticSummary)
            {
                ReadSyntheticSummary(document, result);
                return result;
            }
            if (analyticalCards)
            {
                // Card names begin at x36.48 and may touch the account text.
                ReadAnalyticalCards(new ITextPdfTokenExtractor().ExtractWords(filePath,
                    new[] { 36d }), result, document);
                return result;
            }
            var allCodes = document.Pages.SelectMany(p => p.Tokens
                .Where(t => t.Left < (predvas ? 70 : 19))
                .Select(t => AccountCode(t, p.Tokens)))
                .Where(code => code != null).ToArray();
            var flaggedCodes = new HashSet<string>(StringComparer.Ordinal);
            if (predvas)
                foreach (var page in document.Pages)
                {
                    var words = page.Tokens.ToArray();
                    double coordinateScale = PredvasCoordinateScale(words);
                    foreach (var token in words.Where(t => t.Left < 70 && AccountCode(t, words) != null))
                        if (words.Any(t => Math.Abs(t.Baseline - token.Baseline) < 5.3 &&
                            t.Left * coordinateScale >= 240 && t.Left * coordinateScale < 310 && (t.Text == "M" || t.Text == "D")))
                            flaggedCodes.Add(AccountCode(token, words));
                }
            var unflaggedSeen = new HashSet<string>(StringComparer.Ordinal);
            var encounteredCodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                double coordinateScale = predvas ? PredvasCoordinateScale(tokens) : 1;
                foreach (var candidate in tokens.Where(t => t.Left < (predvas ? 70 : 19) && AccountCode(t, tokens) != null)
                         .OrderByDescending(t => t.Baseline))
                {
                    var line = tokens.Where(t => Math.Abs(t.Baseline - candidate.Baseline) < 5.3).ToArray();
                    string candidateCode = AccountCode(candidate, line);
                    // PREDVAS has analytical detail followed by a synthetic subtotal. Some
                    // detail rows omit flags; retain their first occurrence only. HLKNIA4
                    // has budget lines for the same account, all of which contribute to its total.
                    if (predvas)
                    {
                        bool flagged = line.Any(t => t.Left * coordinateScale >= 240 && t.Left * coordinateScale < 310 && (t.Text == "M" || t.Text == "D"));
                        bool childExists = allCodes.Any(codePredva => codePredva.Length > candidateCode.Length &&
                            codePredva.StartsWith(candidateCode, StringComparison.Ordinal));
                        // The first parent row can itself be a detail account (221 before 221RF).
                        // The parent subtotal follows its children and must not be imported twice.
                        bool parentDetailBeforeChildren = childExists && !encounteredCodes.Contains(candidateCode) &&
                            !encounteredCodes.Any(codePredva => codePredva.Length > candidateCode.Length &&
                                codePredva.StartsWith(candidateCode, StringComparison.Ordinal));
                        bool skip = !flagged && !parentDetailBeforeChildren &&
                            (flaggedCodes.Contains(candidateCode) || childExists || !unflaggedSeen.Add(candidateCode));
                        encounteredCodes.Add(candidateCode);
                        if (skip) continue;
                        if (!flagged) unflaggedSeen.Add(candidateCode);
                    }
                    else if (!line.Any(t => t.Left >= 20 && t.Left < 195 && t.Text == "R") &&
                        // Subtotal names start beside the account. Text farther
                        // right can be a subsidiary code (KON, MŠ, OCÚ, ŠJ).
                        line.Any(t => t.Left >= 20 && t.Left < 50 &&
                        t.Text.Length > 1 && (char.IsLetter(t.Text[0]) || t.Text.All(c => c == '?')) &&
                        t.Text != "R")) continue;
                    var cells = new decimal[8];
                    foreach (var token in line.Where(t => t.Left > (predvas ? 310 / coordinateScale : 210) && Amount.IsMatch(t.Text)))
                    {
                        // PREDVAS amounts are right-aligned; their right edge can
                        // cross a column boundary by a few points. The left edge
                        // remains inside the printed amount column.
                        int col = predvas ? Bin(token.Left * coordinateScale, new[] { 370d, 430, 490, 550, 610, 670, 730, 810 })
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
                    if (!line.Any(t => t.Left > (predvas ? 310 / coordinateScale : 210) && Amount.IsMatch(t.Text))) continue;
                    string code = candidateCode;
                    var row = new GeneralLedgerRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = page.PageNumber,
                        SyntheticCode = code.Substring(0, TwoDigitSyntheticAccount(code) ? 2 : 3),
                        AnalyticalCode = code.Substring(TwoDigitSyntheticAccount(code) ? 2 : 3), AccountCode = code,
                        AccountName = predvas ? Text(line, 85 / coordinateScale, 240 / coordinateScale) : string.Empty
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

        private static void ReadSyntheticSummary(PdfDocument document, GeneralLedgerImport result)
        {
            // HLKNIA4D has one account row per synthetic code and six right-aligned
            // amount columns. Class subtotals and the final control are not accounts.
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var candidate in tokens.Where(t => t.Left < 20 &&
                    (Account.IsMatch(t.Text) || t.Text == "75" || t.Text == "79"))
                    .OrderByDescending(t => t.Baseline))
                {
                    var line = tokens.Where(t => Math.Abs(t.Baseline - candidate.Baseline) < 3).ToArray();
                    var cells = new decimal[6];
                    bool hasAmount = false;
                    foreach (var token in line.Where(t => t.Left >= 240 && Amount.IsMatch(t.Text)))
                    {
                        int column = Bin(token.Right, new[] { 305d, 365, 420, 475, 535, 590 });
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
                    SplitNet(cells[5], out decimal closingDebit, out decimal closingCredit);
                    result.Rows.Add(new GeneralLedgerRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = page.PageNumber,
                        AccountCode = candidate.Text, SyntheticCode = candidate.Text.Length == 2 ? candidate.Text : candidate.Text.Substring(0, 3),
                        AnalyticalCode = candidate.Text.Length == 2 ? string.Empty : candidate.Text.Substring(3), AccountName = Text(line, 20, 240),
                        OpeningDebit = openingDebit, OpeningCredit = openingCredit,
                        AnnualDebitTurnover = cells[1], AnnualCreditTurnover = cells[2],
                        PeriodDebitTurnover = cells[3], PeriodCreditTurnover = cells[4],
                        ClosingDebit = closingDebit, ClosingCredit = closingCredit
                    });
                }
            }
            if (result.Rows.Count == 0)
                throw new InvalidDataException("No _HLKNIA4D.GMX account rows found.");
            ValidateAnnualTotals(document, result, false);
        }

        private static string CardAccountCode(PdfTextToken token, IEnumerable<PdfTextToken> tokens)
        {
            string compact = Regex.Replace(token.Text, @"\s+", string.Empty);
            if (Regex.IsMatch(compact, @"^(?:75|79)[\p{L}][\p{L}\d§]*$")) return compact;
            if (compact == "75" || compact == "79")
            {
                var letterSuffix = tokens.FirstOrDefault(t => t.Left >= token.Right &&
                    t.Left < 36 && t.Left - token.Right < 4 &&
                    Math.Abs(t.Baseline - token.Baseline) < 0.9 &&
                    Regex.IsMatch(t.Text, @"^[\p{L}][\p{L}\d§]*$"));
                return compact + (letterSuffix == null ? string.Empty : letterSuffix.Text);
            }
            if (!Regex.IsMatch(token.Text, @"^\d{3}[\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?$")) return null;
            var suffix = tokens.FirstOrDefault(t => t.Left >= token.Right && t.Left < 36 &&
                t.Left - token.Right < 5 && Math.Abs(t.Baseline - token.Baseline) < 0.9 &&
                Regex.IsMatch(t.Text, @"^\d{1,3}$"));
            return token.Text + (suffix == null ? string.Empty : suffix.Text);
        }

        private static void ReadAnalyticalCards(PdfDocument document, GeneralLedgerImport result, PdfDocument controlDocument)
        {
            // HLKNIA4B prints analytical cards followed by three-digit synthetic
            // subtotals. Its six amount columns share the HLKNIA4D geometry.
            var encountered = new HashSet<string>(StringComparer.Ordinal);
            var remaining = document.Pages.SelectMany(page => page.Tokens
                .Where(token => token.Left < 15)
                .Select(token => CardAccountCode(token, page.Tokens)))
                .Where(code => code != null)
                .GroupBy(code => code, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            var allCodes = remaining.Keys.ToArray();
            foreach (var page in document.Pages)
            {
                var tokens = page.Tokens.ToArray();
                foreach (var candidate in tokens.Where(t => t.Left < 15 && CardAccountCode(t, tokens) != null)
                    .OrderByDescending(t => t.Baseline))
                {
                    string code = CardAccountCode(candidate, tokens);
                    remaining[code]--;
                    bool synthetic = code.Length == 3 || code == "75" || code == "79";
                    bool subtotal = synthetic && encountered.Any(previous =>
                        previous.Length > code.Length && previous.StartsWith(code, StringComparison.Ordinal));
                    encountered.Add(code);
                    // Account 431 prints a heading before the actual card with
                    // the same code. Account 501, however, prints its own detail
                    // before analytical children, followed by a subtotal.
                    bool repeatedHeading = synthetic && remaining[code] > 0 &&
                        !allCodes.Any(other => other.Length > code.Length &&
                            other.StartsWith(code, StringComparison.Ordinal));
                    if (subtotal || repeatedHeading) continue;
                    var line = tokens.Where(t => Math.Abs(t.Baseline - candidate.Baseline) < 3).ToArray();
                    var cells = new decimal[6];
                    bool hasAmount = false;
                    foreach (var token in line.Where(t => t.Left >= 240 && Amount.IsMatch(t.Text)))
                    {
                        int column = Bin(token.Right, new[] { 305d, 365, 420, 475, 535, 590 });
                        if (column < 0 || cells[column] != 0m)
                            throw new InvalidDataException($"Page {page.PageNumber}, account {code}: ambiguous amount column.");
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
                    SplitNet(cells[5], out decimal closingDebit, out decimal closingCredit);
                    result.Rows.Add(new GeneralLedgerRow
                    {
                        SequenceNumber = result.Rows.Count + 1, SourceRecordNumber = page.PageNumber,
                        AccountCode = code, SyntheticCode = code.Substring(0, TwoDigitSyntheticAccount(code) ? 2 : 3),
                        AnalyticalCode = code.Substring(TwoDigitSyntheticAccount(code) ? 2 : 3), AccountName = Text(line, 36, 240),
                        OpeningDebit = openingDebit, OpeningCredit = openingCredit,
                        AnnualDebitTurnover = cells[1], AnnualCreditTurnover = cells[2],
                        PeriodDebitTurnover = cells[3], PeriodCreditTurnover = cells[4],
                        ClosingDebit = closingDebit, ClosingCredit = closingCredit
                    });
                }
            }
            if (result.Rows.Count == 0)
                throw new InvalidDataException("No _HLKNIA4B.GMX analytical account rows found.");
            ValidateAnnualTotals(controlDocument, result, false);
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
        private static double PredvasCoordinateScale(PdfTextToken[] tokens)
        {
            // Compact PREDVAS printouts shift both the flags and amount slots.
            // Normalize their horizontal coordinates to the existing geometry.
            var heading = tokens.FirstOrDefault(t => t.Text == "T" && t.Left > 200 && t.Left < 270 &&
                tokens.Any(other => other.Text == "MÁ" && other.Left > 290 && other.Left < 360 &&
                    Math.Abs(other.Baseline - t.Baseline) < 3));
            return heading != null && heading.Left < 240 ? 245.0 / heading.Left : 1.0;
        }

        private static bool TwoDigitSyntheticAccount(string code) =>
            Regex.IsMatch(code, @"^(?:75|79)(?:\d{1,3}|[\p{L}][\p{L}\d§]*)$");

        private static string AccountCode(PdfTextToken token, IEnumerable<PdfTextToken> pageTokens)
        {
            // Literal asterisk analytical codes also carry real journal movements.
            if (Regex.IsMatch(token.Text, @"^\d{3}\*{2,4}$")) return token.Text;
            string literal = Regex.Replace(token.Text, @"\s+", string.Empty);
            if (Regex.IsMatch(literal, @"^(?:75|79)[\p{L}][\p{L}\d§]*$")) return literal;
            if (literal.Contains("§") && Regex.IsMatch(literal, @"^\d{3}[\p{L}\d§]*(?:-[\p{L}\d§]+)*\.?$")) return literal;
            // PREDVAS can split numeric accounts ("513 100", "75 005")
            // inside the account column. Never import the suffix separately.
            if (Regex.IsMatch(token.Text, @"^(?:\d{3}\s+\d{3}|(?:75|79)\s+\d{1,3})$"))
                return Regex.Replace(token.Text, @"\s+", string.Empty);
            if (token.Left < 70 && Regex.IsMatch(token.Text, @"^(?:\d{3}|75|79)$"))
            {
                var suffix = pageTokens.FirstOrDefault(t => Regex.IsMatch(t.Text, token.Text == "75" || token.Text == "79" ? @"^\d{1,3}$" : @"^\d{3}$") &&
                    t.Left < 80 && t.Left >= token.Right && t.Left - token.Right < 4 &&
                    Math.Abs(t.Baseline - token.Baseline) < 0.9);
                if (suffix != null) return token.Text + suffix.Text;
            }
            if (Regex.IsMatch(token.Text, @"^\d{3}$") && pageTokens.Any(t =>
                t.Left < 70 && token.Left < 80 && Regex.IsMatch(t.Text, @"^(?:\d{3}|75|79)$") && token.Left >= t.Right &&
                token.Left - t.Right < 4 && Math.Abs(t.Baseline - token.Baseline) < 0.9))
                return null;
            if ((token.Text == "75" || token.Text == "79") && token.Left < 19)
            {
                var letterSuffix = pageTokens.FirstOrDefault(t =>
                    Regex.IsMatch(t.Text, @"^[\p{L}][\p{L}\d§]*$") &&
                    t.Left >= token.Right && t.Left - token.Right < 4 && t.Left < 35 &&
                    Math.Abs(t.Baseline - token.Baseline) < 0.9);
                if (letterSuffix != null) return token.Text + letterSuffix.Text;
            }
            if (Account.IsMatch(token.Text)) return token.Text;
            if ((token.Text == "75" || token.Text == "79") && token.Left < 70 &&
                pageTokens.Any(t => t.Text == "ZŠ" && t.Left > token.Left && t.Left < 80 &&
                    Math.Abs(t.Baseline - token.Baseline) < 2))
                return token.Text + "ZŠ";
            return null;
        }
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
