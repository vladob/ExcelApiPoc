using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftBudkovceFrameworkTests
{
    [Fact]
    public void Budkovce_2025_framework_retains_three_asterisk_group_heading()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_BUDKOVCE_2025_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Budkovce framework: " + path);
        var result = new IfoSoftPdfAccountingFrameworkImporter().Import(path);
        var heading = Assert.Single(result.Rows.Where(row => row.SourceRecordNumber == 247));
        Assert.Equal("357", heading.SourceSyntheticCode);
        Assert.Equal("***", heading.SourceAnalyticalCode);
        Assert.Equal(AccountingFrameworkRowKind.GroupHeading, heading.RowKind);
        Assert.Equal(string.Empty, heading.AccountCode);
        var secondHeading = Assert.Single(result.Rows.Where(row => row.SourceRecordNumber == 729));
        Assert.Equal("693", secondHeading.SourceSyntheticCode);
        Assert.Equal("**", secondHeading.SourceAnalyticalCode);
        Assert.Equal(AccountingFrameworkRowKind.GroupHeading, secondHeading.RowKind);
        Assert.Equal(string.Empty, secondHeading.AccountCode);
        Assert.Contains(result.Rows, row => row.AccountCode == "35701" &&
            row.RowKind == AccountingFrameworkRowKind.AnalyticalAccount);
    }
}
