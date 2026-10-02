using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBezovceJournalTests
{
    [Fact]
    public void Bezovce_2024_journal_separates_accounts_from_budget_sections()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_BEZOVCE_2024_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Bežovce journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        var row = Assert.Single(result.Rows.Where(item => item.SequenceNumber == 6633));
        Assert.Equal("221SLSP", row.DebitAccount);
        Assert.Equal("315OST", row.CreditAccount);
        Assert.Equal(362.04m, row.DebitAmount);
        Assert.Equal(362.04m, row.CreditAmount);
        Assert.Equal(17793243.81m, result.Rows.Sum(item => item.DebitAmount ?? 0m));
        Assert.Equal(17793243.81m, result.Rows.Sum(item => item.CreditAmount ?? 0m));
    }
}
