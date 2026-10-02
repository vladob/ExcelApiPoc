using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftFekisovceJournalTests
{
    [Fact]
    public void Fekisovce_2022_journal_preserves_two_digit_letter_account()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_FEKISOVCE_2022_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Fekišovce journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        Assert.Contains(result.Rows, row => row.DebitAccount == "701" &&
            row.CreditAccount == "79OBEC" && row.CreditAmount == 6229.42m);
        Assert.Equal(1080529.05m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1080529.05m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
