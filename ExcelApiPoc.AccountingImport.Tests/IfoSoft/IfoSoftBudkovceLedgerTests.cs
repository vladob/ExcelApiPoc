using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBudkovceLedgerTests
{
    [Fact]
    public void Budkovce_2025_ledger_preserves_short_off_balance_account_suffixes()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_BUDKOVCE_2025_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Budkovce ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Contains(result.Rows, row => row.AccountCode == "7902" &&
            row.SyntheticCode == "79" && row.AnalyticalCode == "02" &&
            row.AnnualCreditTurnover == 2317.96m);
        Assert.Contains(result.Rows, row => row.AccountCode == "75771" &&
            row.SyntheticCode == "75" && row.AnalyticalCode == "771" &&
            row.AnnualDebitTurnover == 2317.96m);
        Assert.Equal(8542400.29m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(8542400.29m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
