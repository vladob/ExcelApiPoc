using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftFalkusovceLedgerTests
{
    [Fact]
    public void Falkusovce_2023_detailed_ledger_keeps_subsidiary_rows()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_FALKUSOVCE_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Falkušovce detailed ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        foreach (decimal amount in new[] { 35.04m, 444.29m, 783.60m, 113.44m })
            Assert.Contains(result.Rows, row => row.AccountCode == "527" &&
                row.AnnualDebitTurnover == amount && row.AnnualCreditTurnover == 0m);
        Assert.Equal(1376.37m, result.Rows.Where(row => row.AccountCode.StartsWith("472"))
            .Sum(row => row.AnnualCreditTurnover));
        Assert.Equal(1226455.04m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1226455.04m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
