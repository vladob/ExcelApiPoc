using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services;

namespace PdfLayoutEngine.SimpleTests;

public sealed class AccountingFrameworkYearValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2024)]
    public void Absent_or_matching_year_is_accepted_silently(int sourceYear)
    {
        var framework = new AccountingFrameworkImport { FiscalYear = sourceYear };
        AccountingFrameworkYearValidator.Validate(framework, 2024);
        Assert.Equal(sourceYear, framework.FiscalYear);
        Assert.Null(framework.ImportReport);
    }

    [Fact]
    public void Different_year_warns_and_preserves_source_and_existing_diagnostics()
    {
        var framework = new AccountingFrameworkImport { FiscalYear = 2023, ImportReport = new ImportReport() };
        framework.ImportReport.Diagnostics.Add(new ImportDiagnostic { Code = "STAGED_LAYOUT" });
        AccountingFrameworkYearValidator.Validate(framework, 2024);
        AccountingFrameworkYearValidator.Validate(framework, 2024);
        Assert.Equal(2023, framework.FiscalYear);
        Assert.Contains(framework.ImportReport.Diagnostics, d => d.Code == "STAGED_LAYOUT");
        var warning = Assert.Single(framework.ImportReport.Diagnostics, d => d.Code == "AF_FISCAL_YEAR_MISMATCH");
        Assert.Equal(ImportDiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("2023", warning.Message);
        Assert.Contains("2024", warning.Message);
    }

    [Fact]
    public void Different_year_without_report_creates_source_warning()
    {
        var framework = new AccountingFrameworkImport { FiscalYear = 2023, SourceFileName = "framework.pdf" };
        AccountingFrameworkYearValidator.Validate(framework, 2024);
        Assert.Equal("framework.pdf", framework.ImportReport.SourceFileName);
        Assert.Single(framework.ImportReport.Diagnostics);
    }
}
