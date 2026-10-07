using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftFalkusovceJournalTests
{
    [Theory]
    [InlineData(2023)]
    [InlineData(2024)]
    [InlineData(2025)]
    public void Falkusovce_journal_detects_and_imports_closing_periods(int year)
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_FALKUSOVCE_" + year + "_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Falkušovce journal: " + path);
        Assert.True(IfoSoftPdfDennik1JournalImporter.TryDetect(path, out var detection));
        Assert.Equal(year, detection.FiscalYear);
        var result = new IfoSoftPdfDennik1JournalImporter().Import(path);
        Assert.NotEmpty(result.Rows);
        Assert.Equal(year, result.FiscalYear);
        if (year == 2025)
            Assert.Contains(result.ImportReport.Diagnostics, item =>
                item.Code == "IFOSOFT_PDF_OPENING_PERIOD_NOT_INCLUDED");
    }
}
