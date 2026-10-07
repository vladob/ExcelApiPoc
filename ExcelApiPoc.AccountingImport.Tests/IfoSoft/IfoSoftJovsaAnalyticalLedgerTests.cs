using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftJovsaAnalyticalLedgerTests
{
    [Fact]
    public void Jovsa_2022_analytical_cards_preserve_off_balance_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_JOVSA_2022_ANALYTICAL_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_GL_JOVSA_2022_ANALYTICAL_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        var ms = Assert.Single(result.Rows.Where(row => row.AccountCode == "75MS"));
        Assert.Equal("75", ms.SyntheticCode);
        Assert.Equal("MS", ms.AnalyticalCode);
        Assert.Equal(4243.48m, ms.AnnualDebitTurnover);
        Assert.Equal(56.61m, ms.AnnualCreditTurnover);
        var ocu = Assert.Single(result.Rows.Where(row => row.AccountCode == "79OCU"));
        Assert.Equal("79", ocu.SyntheticCode);
        Assert.Equal("OCU", ocu.AnalyticalCode);
        Assert.Equal(329m, ocu.AnnualDebitTurnover);
        Assert.Equal(1330.19m, ocu.AnnualCreditTurnover);
        Assert.Contains(result.Rows, row => row.AccountCode == "75OCU");
        Assert.Contains(result.Rows, row => row.AccountCode == "79MS");
        Assert.DoesNotContain(result.Rows, row => row.AccountCode == "75" || row.AccountCode == "79");
        Assert.Equal(2847463.81m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(2847463.81m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
