using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftVelopolieLedgerTests
{
    [Fact]
    public void Velopolie_2023_compact_ledger_matches_printed_totals()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_VELOPOLIE_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Veľopolie ledger: " + path);
        var result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Contains(result.Rows, row => row.AccountCode == "042002" &&
            row.AnnualDebitTurnover == 82694.61m && row.AnnualCreditTurnover == 79694.61m &&
            row.ClosingDebit == 3000m);
        Assert.Equal(996067.29m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(996067.29m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }
}
