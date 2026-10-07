using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftVelopolieFrameworkTests
{
    [Fact]
    public void Velopolie_2022_framework_reads_shifted_columns()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_VELOPOLIE_2022_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Veľopolie framework: " + path);
        var result = new IfoSoftPdfAccountingFrameworkImporter().Import(path);
        Assert.Contains(result.Rows, row => row.SourceRecordNumber == 2 &&
            row.SyntheticCode == "01" && row.AnalyticalCode == "****" &&
            row.RowKind == AccountingFrameworkRowKind.GroupHeading && row.AccountCode == "" &&
            row.AccountName == "DLHODOBÝ NEHMOTNÝ MAJETOK");
        Assert.Contains(result.Rows, row => row.AccountCode == "021001" &&
            row.AccountName == "stavby - správa OcÚ" && row.Type == "M" &&
            row.SubsidiaryFlag == "N" && row.TaxFlag == "N");
    }
}
