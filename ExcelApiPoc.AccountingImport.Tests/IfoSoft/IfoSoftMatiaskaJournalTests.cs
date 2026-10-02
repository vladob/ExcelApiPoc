using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftMatiaskaJournalTests
{
    [Fact]
    public void Matiaska_2023_pdf_preserves_spaced_letter_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_MATIASKA_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Matiaška journal: " + path);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        Assert.Equal(1738637.13m, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1738637.13m, result.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Equal(200m, result.Rows.Where(row => row.DebitAccount == "318KO")
            .Sum(row => row.DebitAmount ?? 0m));
		Assert.Equal(5131.33m, result.Rows.Where(row => row.CreditAccount == "318KO")
			.Sum(row => row.CreditAmount ?? 0m));
    }
}
