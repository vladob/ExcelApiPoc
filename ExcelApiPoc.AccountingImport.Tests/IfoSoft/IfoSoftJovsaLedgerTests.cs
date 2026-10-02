using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftJovsaLedgerTests
{
    [Fact]
    public void Jovsa_2022_ledger_preserves_letter_off_balance_and_section_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_JOVSA_2022_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_GL_JOVSA_2022_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        Assert.Contains(result.Rows, row => row.AccountCode == "75MS" && row.SyntheticCode == "75" &&
            row.AnalyticalCode == "MS" && row.AnnualDebitTurnover == 4243.48m);
        Assert.Contains(result.Rows, row => row.AccountCode == "79OCU" && row.SyntheticCode == "79" &&
            row.AnnualDebitTurnover == 34929.50m);
        Assert.Contains(result.Rows, row => row.AccountCode == "357\u00a754");
        Assert.Equal(9267190.18m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(9267190.18m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
