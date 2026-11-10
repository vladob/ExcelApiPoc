using System;
using System.IO;
using System.Linq;
using AccountBalanceVerification.Core;
using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.Layouts;
namespace AccountBalanceVerification.Import
{
    public static class FileSource
    {
        public static VerificationPackage Read(string producer, string journalPath, string ledgerPath, string ico, int year, string chartPath = null)
        {
            var result = new VerificationPackage { Ico = ico, FiscalYear = year, Source = "Súbory", RetrievedAtUtc = DateTime.UtcNow };
            if (string.IsNullOrWhiteSpace(journalPath))
            {
                if (!File.Exists(ledgerPath)) throw new InvalidOperationException("Vyberte hlavnú knihu alebo účtovný denník.");
                var ledger = StagedImportRuntime.Import<GeneralLedgerImport>(ledgerPath, producer, "GL", year);
                if (ledger.FiscalYear != 0 && ledger.FiscalYear != year || !string.IsNullOrWhiteSpace(ledger.Ico) && ledger.Ico != ico)
                    throw new InvalidOperationException("IČO alebo rok hlavnej knihy nesúhlasí.");
                result.EntityName = ledger.CompanyName;
                AddNames(result, ledger.SyntheticAccountNames);
                // Preserve imported net balances when the source does not expose gross sides.
                foreach (var x in ledger.Rows) result.Rows.Add(new LedgerRow {
                    AccountNo = x.AccountCode, AccountName = x.AccountName,
                    OpeningDebit = x.SourceOpeningNet.HasValue ? Math.Max(x.SourceOpeningNet.Value, 0) : x.OpeningDebit,
                    OpeningCredit = x.SourceOpeningNet.HasValue ? Math.Max(-x.SourceOpeningNet.Value, 0) : x.OpeningCredit,
                    MovementsDebit = x.AnnualDebitTurnover, MovementsCredit = x.AnnualCreditTurnover,
                    ClosingDebit = x.SourceClosingNet.HasValue ? Math.Max(x.SourceClosingNet.Value, 0) : x.ClosingDebit,
                    ClosingCredit = x.SourceClosingNet.HasValue ? Math.Max(-x.SourceClosingNet.Value, 0) : x.ClosingCredit
                });
                // IfoSoft CSV partitions an analytical account by budget dimensions.
                // Inventory account sheets need one additive balance per account.
                if (string.Equals(producer, "IfoSoft", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Path.GetExtension(ledgerPath), ".csv", StringComparison.OrdinalIgnoreCase))
                    result.Rows = result.Rows.GroupBy(x => x.AccountNo, StringComparer.Ordinal).Select(g => new LedgerRow {
                        AccountNo = g.Key, AccountName = g.Select(x => x.AccountName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "",
                        OpeningDebit = g.Sum(x => x.OpeningDebit), OpeningCredit = g.Sum(x => x.OpeningCredit),
                        MovementsDebit = g.Sum(x => x.MovementsDebit), MovementsCredit = g.Sum(x => x.MovementsCredit),
                        ClosingDebit = g.Sum(x => x.ClosingDebit), ClosingCredit = g.Sum(x => x.ClosingCredit)
                    }).ToList();
                if (ledger.ImportReport != null) result.Warnings.AddRange(ledger.ImportReport.Diagnostics.Where(d => d.Severity != ExcelApiPoc.AccountingImport.Models.Reporting.ImportDiagnosticSeverity.Information).Select(d => d.Message));
            }
            else
            {
                var package = AccountingImportCoordinator.CreateDefault().Import(new AccountingImportRequest {
                    AccountingFormat = producer, JournalFilePath = journalPath, GeneralLedgerFilePath = ledgerPath,
                    ExpectedIco = ico, ExpectedFiscalYear = year
                });
                result.EntityName = package.Journal.CompanyName;
                if (package.GeneralLedger != null) AddNames(result, package.GeneralLedger.SyntheticAccountNames);
                if (package.Journal.Rows.Any(x => x.RecordKind == JournalRecordKind.Normal && x.EffectivePostingDate.Year != year))
                    throw new InvalidOperationException("Denník obsahuje pohyby mimo zvoleného roka. Skontrolujte vstupný súbor.");
                if (package.GeneralLedger == null && !package.CalculatedGeneralLedger.JournalContainsOpeningRecords)
                    throw new InvalidOperationException("Denník neobsahuje počiatočné stavy. Pridajte hlavnú knihu.");
                if (package.JournalLedgerReconciliation != null)
                {
                    var reconciliation = package.JournalLedgerReconciliation;
                    var differences = reconciliation.Accounts.Where(x =>
                        (!x.IsReconciled || !x.HasJournalActivity || !x.HasGeneralLedgerAccount) &&
                        !(x.JournalOpeningDebit == 0m && x.JournalOpeningCredit == 0m &&
                          x.JournalDebitTurnover == 0m && x.JournalCreditTurnover == 0m &&
                          x.LedgerOpeningDebit == 0m && x.LedgerOpeningCredit == 0m &&
                          x.LedgerDebitTurnover == 0m && x.LedgerCreditTurnover == 0m &&
                          x.LedgerClosingDebit == 0m && x.LedgerClosingCredit == 0m)).ToList();
                    if (differences.Count != 0)
                        throw new InvalidOperationException("Denník a hlavná kniha nesúhlasia. Účty: " +
                            string.Join(", ", differences.Take(20).Select(x => x.AccountCode)) +
                            (differences.Count > 20 ? ", …" : "") +
                            ". Rozdiely vyriešte pred vytvorením zošita.");
                    foreach (var x in reconciliation.Accounts) result.Rows.Add(new LedgerRow {
                        AccountNo = x.AccountCode, AccountName = x.GeneralLedgerAccountName,
                        OpeningDebit = x.EffectiveOpeningDebit, OpeningCredit = x.EffectiveOpeningCredit,
                        MovementsDebit = x.JournalDebitTurnover, MovementsCredit = x.JournalCreditTurnover,
                        ClosingDebit = x.CalculatedClosingDebit, ClosingCredit = x.CalculatedClosingCredit
                    });
                }
                else foreach (var x in package.CalculatedGeneralLedger.Rows) result.Rows.Add(new LedgerRow {
                    AccountNo = x.AccountCode, OpeningDebit = x.OpeningDebit, OpeningCredit = x.OpeningCredit,
                    MovementsDebit = x.DebitTurnover, MovementsCredit = x.CreditTurnover,
                    ClosingDebit = x.ClosingDebit ?? throw new InvalidOperationException("Missing closing balance."),
                    ClosingCredit = x.ClosingCredit ?? throw new InvalidOperationException("Missing closing balance.")
                });
                if (package.Journal.ImportReport != null) result.Warnings.AddRange(package.Journal.ImportReport.Diagnostics.Where(d => d.Severity != ExcelApiPoc.AccountingImport.Models.Reporting.ImportDiagnosticSeverity.Information).Select(d => d.Message));
            }
            result.Source = string.Join("; ", new[] { journalPath, chartPath, ledgerPath }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Path.GetFileName));
            // Only an explicit three-digit GL account supplies a synthetic name.
            foreach (var row in result.Rows.Where(x => x.AccountNo.Length == 3 && !string.IsNullOrWhiteSpace(x.AccountName)))
                if (!result.OfficialAccounts.Any(x => x.AccountCode == row.AccountNo)) result.OfficialAccounts.Add(new OfficialAccount { AccountCode = row.AccountNo, AccountName = row.AccountName, AccountLevel = 3 });
            if (!string.IsNullOrWhiteSpace(chartPath)) ApplyChart(result, producer, chartPath, ico, year);
            return result;
        }
        private static void AddNames(VerificationPackage result, System.Collections.Generic.IDictionary<string, string> names)
        {
            foreach (var item in names.Where(x => WorkbookPlan.InScope(x.Key)))
            {
                result.OfficialAccounts.RemoveAll(x => x.AccountCode == item.Key);
                result.OfficialAccounts.Add(new OfficialAccount { AccountCode = item.Key, AccountName = item.Value, AccountLevel = 3 });
            }
        }
        public static void ApplyChart(VerificationPackage result, string producer, string path, string ico, int year)
        {
            var chart = StagedImportRuntime.Import<AccountingFrameworkImport>(path, producer, "AF", year);
            if ((chart.FiscalYear != 0 && chart.FiscalYear != year) ||
                (!string.IsNullOrWhiteSpace(chart.Ico) && chart.Ico != ico))
                throw new InvalidOperationException("IČO alebo rok účtového rozvrhu nesúhlasí.");
            if (string.IsNullOrWhiteSpace(result.EntityName)) result.EntityName = chart.CompanyName;
            var names = chart.Rows.Where(x => !string.IsNullOrWhiteSpace(x.AccountCode) && !string.IsNullOrWhiteSpace(x.AccountName))
                .GroupBy(x => x.AccountCode.Trim()).ToDictionary(g => g.Key, g => g.First().AccountName.Trim());
            foreach (var row in result.Rows)
                if (names.TryGetValue(row.AccountNo, out var name)) row.AccountName = name;
            AddNames(result, chart.SyntheticAccountNames);
            // AF names take precedence over GL names; balances remain unchanged.
            foreach (var item in names.Where(x => x.Key.Length == 3 && WorkbookPlan.InScope(x.Key)))
            {
                result.OfficialAccounts.RemoveAll(x => x.AccountCode == item.Key);
                result.OfficialAccounts.Add(new OfficialAccount { AccountCode = item.Key, AccountName = item.Value, AccountLevel = 3 });
            }
            if (chart.ImportReport != null) result.Warnings.AddRange(chart.ImportReport.Diagnostics
                .Where(d => d.Severity != ExcelApiPoc.AccountingImport.Models.Reporting.ImportDiagnosticSeverity.Information).Select(d => d.Message));
        }
    }
}
