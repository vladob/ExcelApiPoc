using ExcelApiPoc.AccountingImport.Services.Common;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

public sealed class AccountingFileNameMetadataTests
{
    [Theory]
    [InlineData(
        "U_DENNIK_00325791_202412.csv",
        AccountingSourceDocumentKind.AccountingJournal,
        "00325791",
        2024,
        12)]
    [InlineData(
        "UCT_ROZVRH_00325791_202414.csv",
        AccountingSourceDocumentKind.AccountingFramework,
        "00325791",
        2024,
        14)]
    [InlineData(
        "HL_KNIHA_00325791_202499.xls",
        AccountingSourceDocumentKind.GeneralLedger,
        "00325791",
        2024,
        99)]
    [InlineData(
        "U_DENNIK_00325791_2024.xlsx",
        AccountingSourceDocumentKind.AccountingJournal,
        "00325791",
        2024,
        null)]
    public void TryParse_ExtractsDocumentMetadata(
        string fileName,
        AccountingSourceDocumentKind expectedKind,
        string expectedIco,
        int expectedYear,
        int? expectedStage)
    {
        bool parsed = AccountingFileNameMetadataParser.TryParse(
            fileName,
            out AccountingFileNameMetadata metadata);

        Assert.True(parsed);
        Assert.Equal(expectedKind, metadata.DocumentKind);
        Assert.Equal(expectedIco, metadata.Ico);
        Assert.Equal(expectedYear, metadata.FiscalYear);
        Assert.Equal(expectedStage, metadata.ExportStage);
    }

    [Fact]
    public void Resolve_PrefersMatchingContentMetadata()
    {
        AccountingFileNameMetadataParser.TryParse(
            "U_DENNIK_00325791_202412.csv",
            out AccountingFileNameMetadata metadata);

        string ico = AccountingFileNameMetadataParser.ResolveIco(
            metadata.FileName,
            "00325791",
            metadata);
        int year = AccountingFileNameMetadataParser.ResolveFiscalYear(
            metadata.FileName,
            2024,
            metadata);

        Assert.Equal("00325791", ico);
        Assert.Equal(2024, year);
        Assert.Equal(12, metadata.ExportStage);
    }

    [Fact]
    public void Resolve_UsesFilenameAsFallback()
    {
        AccountingFileNameMetadataParser.TryParse(
            "HL_KNIHA_00325791_202414.csv",
            out AccountingFileNameMetadata metadata);

        string ico = AccountingFileNameMetadataParser.ResolveIco(
            metadata.FileName,
            null,
            metadata);
        int year = AccountingFileNameMetadataParser.ResolveFiscalYear(
            metadata.FileName,
            0,
            metadata);

        Assert.Equal("00325791", ico);
        Assert.Equal(2024, year);
        Assert.Equal(14, metadata.ExportStage);
    }

    [Fact]
    public void Resolve_ReportsIcoConflict()
    {
        AccountingFileNameMetadataParser.TryParse(
            "U_DENNIK_00325791_202412.csv",
            out AccountingFileNameMetadata metadata);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => AccountingFileNameMetadataParser.ResolveIco(
                metadata.FileName,
                "00323233",
                metadata));

        Assert.Contains("00325791", exception.Message);
        Assert.Contains("00323233", exception.Message);
        Assert.Contains("file contents", exception.Message);
    }

    [Fact]
    public void Resolve_ReportsFiscalYearConflict()
    {
        AccountingFileNameMetadataParser.TryParse(
            "HL_KNIHA_00325791_202412.csv",
            out AccountingFileNameMetadata metadata);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => AccountingFileNameMetadataParser.ResolveFiscalYear(
                metadata.FileName,
                2023,
                metadata));

        Assert.Contains("2024", exception.Message);
        Assert.Contains("2023", exception.Message);
        Assert.Contains("file contents", exception.Message);
    }
}
