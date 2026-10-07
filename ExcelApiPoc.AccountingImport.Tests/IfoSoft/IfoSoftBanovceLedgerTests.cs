using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBanovceLedgerTests
{
    [Fact]
    public void Banovce_2024_cards_preserve_account_column_and_control_totals()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_BANOVCE_2024_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Equal(2143826.86m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(2143826.86m, result.Rows.Sum(row => row.AnnualCreditTurnover));
        Assert.Contains(result.Rows, row => row.AccountCode == "524REGO" && row.AnnualDebitTurnover == 21.21m);
        Assert.Contains(result.Rows, row => row.AccountCode == "518REGO" && row.AnnualDebitTurnover == 13.30m);
        Assert.Contains(result.Rows, row => row.AccountCode == "521REGO" && row.AnnualDebitTurnover == 220m);
        Assert.Contains(result.Rows, row => row.AccountCode == "501OE1" && row.AnnualDebitTurnover == 230.05m);
        Assert.Contains(result.Rows, row => row.AccountCode == "428§80");
    }
}
