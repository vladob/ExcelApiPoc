using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftSectionSignFrameworkTests
{
    [Fact]
    public void Banovce_2023_framework_preserves_section_sign_account()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_BANOVCE_2023_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing framework: " + path);
        var result = new IfoSoftPdfAccountingFrameworkImporter().Import(path);
        Assert.Contains(result.Rows, row => row.AccountCode == "428§80" && row.AnalyticalCode == "§80");
    }
}
