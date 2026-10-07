using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftDubravkaJournalTests
{
    [Fact]
    public void Dubravka_2023_journal_does_not_treat_detail_codes_as_entries()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_DUBRAVKA_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Dúbravka journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        var row = Assert.Single(result.Rows.Where(item => item.SequenceNumber == 153));
        Assert.Equal("521", row.DebitAccount);
        Assert.Equal("3311", row.CreditAccount);
        Assert.Equal(6659.50m, row.DebitAmount);
        Assert.Equal(6659.50m, row.CreditAmount);
        Assert.Equal(new DateTime(2023, 1, 31), row.PostingDate);
    }
}
