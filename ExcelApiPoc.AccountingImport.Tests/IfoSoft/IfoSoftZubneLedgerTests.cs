using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftZubneLedgerTests
{
    [Fact]
    public void Zubne_2024_ledger_does_not_duplicate_numeric_suffixes()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_ZUBNE_2024_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Zubné ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Equal(1102933.61m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1106193.61m, result.Rows.Sum(row => row.AnnualCreditTurnover));
        Assert.Contains(result.Rows, row => row.AccountCode == "513100" && row.AnnualDebitTurnover == 1265.90m);
        Assert.Contains(result.Rows, row => row.AccountCode == "513500" && row.AnnualDebitTurnover == 35.47m);
    }
}
