using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftJovsaJournalTests
{
    [Fact]
    public void Jovsa_2022_journal_reads_enlarged_print_geometry()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_JOVSA_2022_TEST_FILE");
        Assert.False(string.IsNullOrWhiteSpace(path), "Set IFOSOFT_PDF_JOURNAL_JOVSA_2022_TEST_FILE.");
        Assert.True(File.Exists(path), "Missing journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        var first = result.Rows.First();
        Assert.Equal("331", first.DebitAccount);
        Assert.Equal("221BEZ", first.CreditAccount);
        Assert.Equal(-286.09m, first.DebitAmount);
        Assert.Equal(2847463.81m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(2847463.81m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
