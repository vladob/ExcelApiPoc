using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesGeneralLedgerImporterTests
{
    [Theory]
    [InlineData("HL_KNIHA_00325465_2025.xls")]
    [InlineData("HL_KNIHA_00325465_2025.xlsx")]
    [InlineData("HL_KNIHA_00325465_2025.csv")]
    [InlineData("HL_KNIHA_00325465_2025.xml")]
    [InlineData("HL_KNIHA_00325465_2025.pdf")]
    public void CanImport_RecognizesAllSupportedIvesLedgerFormats(
        string fileName)
    {
        var importer = new IvesGeneralLedgerImporter();

        Assert.True(importer.CanImport(fileName, "IVES"));
        Assert.True(importer.CanImport(fileName, "ives"));
        Assert.False(importer.CanImport(fileName, "Urbis"));
    }

    [Fact]
    public void CanImport_RejectsNonLedgerIvesFile()
    {
        var importer = new IvesGeneralLedgerImporter();

        Assert.False(importer.CanImport(
            "U_DENNIK_00325465_2025.xml",
            "IVES"));
    }

    [Fact]
    public void Import_XlsFixture_PreservesActivityIdentity()
    {
        string fixture = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            "HL_KNIHA_00322881_2024.xls");

        GeneralLedgerImport result =
            new IvesGeneralLedgerImporter().Import(fixture);

        Assert.All(
            result.Rows,
            row => Assert.False(
                string.IsNullOrWhiteSpace(row.ActivityName)));
    }

    [Fact]
    public void Import_XlsFixture_PreservesEstablishedCanonicalMapping()
    {
        string fixture = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            "HL_KNIHA_00322881_2024.xls");

        GeneralLedgerImport result =
            new IvesGeneralLedgerImporter().Import(fixture);

        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("IVES", result.AccountingFormat);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(320, result.Rows.Count);
        Assert.True(result.ImportReport.IsValid);
    }
}
