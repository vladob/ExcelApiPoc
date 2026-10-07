using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftDubravkaCsvLedgerDiagnosticTests
{
    [Fact]
    public void Dubravka_2025_corrected_ledger_preserves_rows_and_balances()
    {
        // Keep the old environment variable as an alias for existing local settings.
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_CSV_GL_DUBRAVKA_2025_TEST_FILE")
            ?? Environment.GetEnvironmentVariable("IFOSOFT_CSV_GL_DUBRAVKA_2025_MALFORMED_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing corrected Dubravka ledger: " + path);
        foreach (var ledger in new[] {
            new IfoSoftCsvGeneralLedgerImporter().Import(path),
            StagedImportRuntime.Import<GeneralLedgerImport>(path, "IfoSoft", "GL", 2025) })
        {
            Assert.Equal("00325112", ledger.Ico);
            Assert.Equal(2025, ledger.FiscalYear);
            Assert.Equal(714, ledger.Rows.Count);
            Assert.Equal(3620877.68m, ledger.Rows.Sum(r => r.OpeningDebit));
            Assert.Equal(3620877.68m, ledger.Rows.Sum(r => r.OpeningCredit));
            Assert.Equal(6858777.62m, ledger.Rows.Sum(r => r.AnnualDebitTurnover));
            Assert.Equal(6858777.62m, ledger.Rows.Sum(r => r.AnnualCreditTurnover));
            Assert.Equal(2824191.24m, ledger.Rows.Sum(r => r.ClosingDebit));
            Assert.Equal(2824191.24m, ledger.Rows.Sum(r => r.ClosingCredit));
            var row = Assert.Single(ledger.Rows, r => r.AccountCode == "042006" && r.AnnualDebitTurnover == 9922.64m && r.AnnualCreditTurnover == 518056.52m);
            Assert.Equal("Obstaranie- Zateplenie a stavebné úprav", row.AccountName);
            Assert.Equal(9922.64m, row.AnnualDebitTurnover);
            Assert.Equal(518056.52m, row.AnnualCreditTurnover);
        }
    }
}
