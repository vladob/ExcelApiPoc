using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBudkovceSyntheticLedgerTests
{
    [Fact]
    public void Budkovce_2025_synthetic_ledger_includes_two_digit_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_BUDKOVCE_2025_SYNTHETIC_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Budkovce synthetic ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Contains(result.Rows, row => row.AccountCode == "75" &&
            row.SyntheticCode == "75" && row.AnalyticalCode == string.Empty &&
            row.AnnualDebitTurnover == 2317.96m);
        Assert.Contains(result.Rows, row => row.AccountCode == "79" &&
            row.SyntheticCode == "79" && row.AnalyticalCode == string.Empty &&
            row.AnnualCreditTurnover == 2317.96m);
        Assert.Equal(8542400.29m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(8542400.29m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
