using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBudgetDennikTests
{
    [Theory]
    [InlineData("IFOSOFT_PDF_DENNIK_BANOVCE_2023_TEST_FILE", 6729, "5620970.58")]
    [InlineData("IFOSOFT_PDF_DENNIK_BANOVCE_2024_TEST_FILE", 7405, "10903463.07")]
    [InlineData("IFOSOFT_PDF_DENNIK_BANOVCE_2025_TEST_FILE", 7785, "7452539.15")]
    public void Budget_Dennik_annual_journal_matches_printed_totals(string variable, int count, string totalText)
    {
        string? path = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing journal: " + path);
        Assert.True(IfoSoftPdfDennik1JournalImporter.TryDetect(path, out var detection));
        Assert.Equal("IfoSoft", detection.AccountingFormat);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        decimal total = decimal.Parse(totalText, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(count, result.Rows.Count);
        Assert.Equal(total, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(total, result.Rows.Sum(row => row.CreditAmount ?? 0m));
        // The printed PS opening document has no document number.
        // Ordinary documents must still retain their printed identifiers.
        Assert.All(result.Rows.Where(row => string.IsNullOrWhiteSpace(row.DocumentNumber)), row =>
        {
            Assert.Equal("PS", row.DocumentType);
            Assert.Equal(ExcelApiPoc.AccountingImport.Models.JournalRecordKind.Opening, row.RecordKind);
        });
    }
}
