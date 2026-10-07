using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftHnojneJournalTests
{
    [Fact]
    public void Hnojne_2025_journal_reads_shifted_credit_column()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_HNOJNE_2025_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_JOURNAL_HNOJNE_2025_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        var first = result.Rows.First();
        Assert.Equal("021", first.DebitAccount);
        Assert.Equal("701", first.CreditAccount);
        Assert.Equal(191807.20m, first.DebitAmount);
        Assert.Equal(1446551.89m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1446551.89m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
