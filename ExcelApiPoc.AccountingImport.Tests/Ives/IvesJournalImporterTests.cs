using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesJournalImporterTests
{
    [Fact]
    public void CanImport_RecognizesSupportedIvesJournalFormats()
    {
        var importer = new IvesJournalImporter();
        string fixture = GetFixturePath("U_DENNIK_00322881_2024.xls");

        Assert.True(importer.CanImport(fixture, "IVES"));
        Assert.True(importer.CanImport(fixture, "ives"));
        Assert.False(importer.CanImport(fixture, "Urbis"));
        Assert.True(importer.CanImport(
            Path.ChangeExtension(fixture, ".xlsx"),
            "IVES"));
        Assert.True(importer.CanImport(
            Path.ChangeExtension(fixture, ".csv"),
            "IVES"));
        Assert.True(importer.CanImport(
            Path.ChangeExtension(fixture, ".xml"),
            "IVES"));
        Assert.False(importer.CanImport(
            Path.ChangeExtension(fixture, ".pdf"),
            "IVES"));
        Assert.False(importer.CanImport(
            Path.Combine(
                Path.GetDirectoryName(fixture)!,
                "HL_KNIHA_00322881_2024.xls"),
            "IVES"));
    }

    [Theory]
    [InlineData(
        "U_DENNIK_00322881_2022.xls",
        2022,
        1579,
        "50C8F48A2E1A5B327C18026095FD31AC33A7C7898A8B4E2DF0785D4D4699F2CB")]
    [InlineData(
        "U_DENNIK_00322881_2023.xls",
        2023,
        1575,
        "DA0F7ED30F36A4CB80A0780BE9FC8855961702A20E167B01A823B111B73226F4")]
    [InlineData(
        "U_DENNIK_00322881_2024.xls",
        2024,
        2125,
        "EFE9B982E7B4BF5828E78B35C097BE81904D351BC6222E4ADACA434E404F072A")]
    public void Import_MapsMetadataRowsAndSuccessfulValidationReport(
        string fileName,
        int fiscalYear,
        int rowCount,
        string expectedHash)
    {
        JournalImport result = ImportFixture(fileName);

        Assert.Equal(fileName, result.SourceFileName);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("IVES", result.AccountingFormat);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(fiscalYear, result.FiscalYear);
        Assert.Equal(expectedHash, result.SourceFileHash);
        Assert.Equal(rowCount, result.Rows.Count);

        Assert.NotNull(result.ImportReport);
        Assert.True(result.ImportReport.IsValid);
        Assert.Empty(result.ImportReport.Diagnostics);
        Assert.Equal(rowCount, result.ImportReport.RecordCounts["Transactions"]);
    }

    [Fact]
    public void Import_PreservesOptionalExportStageFromFilename()
    {
        string copiedPath = CopyFixtureWithName(
            "U_DENNIK_00322881_202414.xls");

        try
        {
            JournalImport result = new IvesJournalImporter().Import(copiedPath);

            Assert.Equal("00322881", result.Ico);
            Assert.Equal(2024, result.FiscalYear);
            Assert.Equal(14, result.ExportStage);
        }
        finally
        {
            DeleteCopiedFixture(copiedPath);
        }
    }

    [Fact]
    public void Import_ReportsIcoConflictBetweenContentsAndFilename()
    {
        string copiedPath = CopyFixtureWithName(
            "U_DENNIK_00325791_2024.xls");

        try
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new IvesJournalImporter().Import(copiedPath));

            Assert.Contains("00325791", exception.Message);
            Assert.Contains("00322881", exception.Message);
            Assert.Contains("file contents", exception.Message);
        }
        finally
        {
            DeleteCopiedFixture(copiedPath);
        }
    }

    [Fact]
    public void Import_ReportsFiscalYearConflictBetweenContentsAndFilename()
    {
        string copiedPath = CopyFixtureWithName(
            "U_DENNIK_00322881_2023.xls");

        try
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => new IvesJournalImporter().Import(copiedPath));

            Assert.Contains("2023", exception.Message);
            Assert.Contains("2024", exception.Message);
            Assert.Contains("file contents", exception.Message);
        }
        finally
        {
            DeleteCopiedFixture(copiedPath);
        }
    }

    [Fact]
    public void Import_PreservesCompositeAccountsAmountsAndTwoRowProvenance()
    {
        JournalImport result = ImportFixture("U_DENNIK_00322881_2024.xls");
        JournalRow first = result.Rows[0];

        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(20, first.SourceRecordNumber);
        Assert.Equal(20, first.SourceStartLineNumber);
        Assert.Equal(21, first.SourceEndLineNumber);
        Assert.Equal(
            "U_DENNIK_00322881_2024.xls, worksheet Sheet1, rows 20-21",
            first.SourceLocation);
        Assert.Null(first.DocumentType);
        Assert.Equal("ZZ0000000015", first.DocumentNumber);
        Assert.Equal(new DateTime(2024, 1, 2), first.PostingDate);
        Assert.Equal("562.1......", first.DebitAccount);
        Assert.Equal("221.0170.651002.41..R..", first.CreditAccount);
        Assert.Equal(66.24m, first.DebitAmount);
        Assert.Equal(66.24m, first.CreditAmount);
        Assert.Equal(JournalRecordKind.Normal, first.RecordKind);
        Assert.True(first.UsedForReportCalculation);
    }

    [Fact]
    public void Import_AssignsAmountOnlyToEachPopulatedSide()
    {
        JournalImport result = ImportFixture("U_DENNIK_00322881_2024.xls");

        Assert.Equal(722, result.Rows.Count(row => row.DebitAccount == null));
        Assert.Equal(146, result.Rows.Count(row => row.CreditAccount == null));
        Assert.DoesNotContain(result.Rows, row =>
            row.DebitAccount == null && row.CreditAccount == null);

        Assert.All(result.Rows, row =>
        {
            Assert.Equal(row.DebitAccount == null, row.DebitAmount == null);
            Assert.Equal(row.CreditAccount == null, row.CreditAmount == null);
        });
    }

    [Fact]
    public void Import_PreservesNegativeSignedAmounts()
    {
        JournalImport result = ImportFixture("U_DENNIK_00322881_2023.xls");
        JournalRow[] negativeRows = result.Rows
            .Where(row => row.DebitAmount < 0m || row.CreditAmount < 0m)
            .ToArray();

        Assert.Equal(5, negativeRows.Length);
        Assert.All(negativeRows, row =>
        {
            if (row.DebitAccount != null) Assert.True(row.DebitAmount < 0m);
            if (row.CreditAccount != null) Assert.True(row.CreditAmount < 0m);
        });
    }

    [Fact]
    public void Import_DoesNotPrematurelyClassifyOpeningOrClosingRecords()
    {
        JournalImport result = ImportFixture("U_DENNIK_00322881_2024.xls");

        Assert.All(result.Rows, row =>
        {
            Assert.Equal(JournalRecordKind.Normal, row.RecordKind);
            Assert.Null(row.DocumentType);
        });
    }

    private static JournalImport ImportFixture(string fileName)
    {
        return new IvesJournalImporter().Import(GetFixturePath(fileName));
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            fileName);
    }

    private static string CopyFixtureWithName(string fileName)
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        string copiedPath = Path.Combine(directory, fileName);
        File.Copy(
            GetFixturePath("U_DENNIK_00322881_2024.xls"),
            copiedPath);
        return copiedPath;
    }

    private static void DeleteCopiedFixture(string copiedPath)
    {
        string? directory = Path.GetDirectoryName(copiedPath);

        if (File.Exists(copiedPath))
            File.Delete(copiedPath);

        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            Directory.Delete(directory);
    }
}
