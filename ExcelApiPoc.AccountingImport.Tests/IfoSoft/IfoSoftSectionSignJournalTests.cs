using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftSectionSignJournalTests
{
    [Fact]
    public void Pdf_journal_preserves_section_sign_account()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_SECTION_SIGN_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        Assert.Equal(5620970.58m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(5620970.58m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(result.Rows, row => row.CreditAccount == "428§80" &&
            row.CreditAmount == 489831.72m);
    }
}
