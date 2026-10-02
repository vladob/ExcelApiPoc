using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftVysnyHrusovLedgerTests
{
    [Fact]
    public void Vysny_hrusov_2023_ledger_preserves_period_14()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_VYSNY_HRUSOV_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Vyšný Hrušov ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Equal(2023, result.FiscalYear);
        Assert.Equal(14, result.ThroughMonth);
        Assert.Equal("14/2023", result.PeriodHeader);
        Assert.Equal(6376614.36m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(6376614.36m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
