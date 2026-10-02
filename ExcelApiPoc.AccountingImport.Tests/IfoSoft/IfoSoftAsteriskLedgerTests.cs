using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftAsteriskLedgerTests
{
    [Fact]
    public void Ledger_2025_preserves_literal_asterisk_account()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_00325155_2025_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_GL_00325155_2025_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        var account = Assert.Single(result.Rows.Where(row => row.AccountCode == "357***"));
        Assert.Equal("357", account.SyntheticCode);
        Assert.Equal("***", account.AnalyticalCode);
        Assert.Equal(1264.04m, account.AnnualCreditTurnover);
        Assert.Equal(2246060.80m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(2246060.80m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
