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
        public static VerificationPackage Read(string producer, string journalPath, string ledgerPath, string ico, int year)
        {
            var result = new VerificationPackage { Ico = ico, FiscalYear = year, Source = "Súbory", RetrievedAtUtc = DateTime.UtcNow };
            if (string.IsNullOrWhiteSpace(journalPath))
            {
                if (!File.Exists(ledgerPath)) throw new InvalidOperationException("Vyberte hlavnú knihu alebo účtovný denník.");
                var ledger = StagedImportRuntime.Import<GeneralLedgerImport>(ledgerPath, producer, "GL", year);
                if (ledger.FiscalYear != 0 && ledger.FiscalYear != year || !string.IsNullOrWhiteSpace(ledger.Ico) && ledger.Ico != ico)
                    throw new InvalidOperationException("IČO alebo rok hlavnej knihy nesúhlasí.");
                result.EntityName = ledger.CompanyName;
                // Preserve imported net balances when the source does not expose gross sides.
                foreach (var x in ledger.Rows) result.Rows.Add(new LedgerRow {
                    AccountNo = x.AccountCode, AccountName = x.AccountName,
                    OpeningDebit = x.SourceOpeningNet.HasValue ? Math.Max(x.SourceOpeningNet.Value, 0) : x.OpeningDebit,
                    OpeningCredit = x.SourceOpeningNet.HasValue ? Math.Max(-x.SourceOpeningNet.Value, 0) : x.OpeningCredit,
                    MovementsDebit = x.AnnualDebitTurnover, MovementsCredit = x.AnnualCreditTurnover,
                    ClosingDebit = x.SourceClosingNet.HasValue ? Math.Max(x.SourceClosingNet.Value, 0) : x.ClosingDebit,
                    ClosingCredit = x.SourceClosingNet.HasValue ? Math.Max(-x.SourceClosingNet.Value, 0) : x.ClosingCredit
                });
                if (ledger.ImportReport != null) result.Warnings.AddRange(ledger.ImportReport.Diagnostics.Where(d => d.Severity != ExcelApiPoc.AccountingImport.Models.Reporting.ImportDiagnosticSeverity.Information).Select(d => d.Message));
            }
            else
            {
                var package = AccountingImportCoordinator.CreateDefault().Import(new AccountingImportRequest {
                    AccountingFormat = producer, JournalFilePath = journalPath, GeneralLedgerFilePath = ledgerPath,
                    ExpectedIco = ico, ExpectedFiscalYear = year
                });
                result.EntityName = package.Journal.CompanyName;
                if (package.Journal.Rows.Any(x => x.RecordKind == JournalRecordKind.Normal && x.EffectivePostingDate.Year != year))
                    throw new InvalidOperationException("Denník obsahuje pohyby mimo zvoleného roka. Skontrolujte vstupný súbor.");
                if (package.GeneralLedger == null && !package.CalculatedGeneralLedger.JournalContainsOpeningRecords)
                    throw new InvalidOperationException("Denník neobsahuje počiatočné stavy. Pridajte hlavnú knihu.");
                if (package.JournalLedgerReconciliation != null)
                {
                    var reconciliation = package.JournalLedgerReconciliation;
                    if (!reconciliation.IsReconciled || reconciliation.JournalOnlyAccountCount != 0 || reconciliation.LedgerOnlyAccountCount != 0)
                        throw new InvalidOperationException("Denník a hlavná kniha nesúhlasia. Rozdiely vyriešte pred vytvorením zošita.");
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
            result.Source = string.Join("; ", new[] { journalPath, ledgerPath }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Path.GetFileName));
            return result;
        }
    }
}
