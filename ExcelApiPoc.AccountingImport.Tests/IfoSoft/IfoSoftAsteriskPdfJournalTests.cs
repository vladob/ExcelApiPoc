using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftAsteriskPdfJournalTests
{
    [Fact]
    public void Pdf_2025_preserves_asterisk_account_opening_and_correction()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_00325155_2025_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_JOURNAL_00325155_2025_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Contains(result.Rows, row => row.DebitAccount == "357***" &&
            row.CreditAccount == "701" && row.DebitAmount == 1264.04m);
        Assert.Contains(result.Rows, row => row.DebitAccount == "4282024" &&
            row.CreditAccount == "357***" && row.CreditAmount == 1264.04m);
        Assert.Equal(7482952.58m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(7482952.58m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
