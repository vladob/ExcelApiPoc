using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Urbis;

namespace ExcelApiPoc.AccountingImport.Tests.Urbis;

public sealed class UrbisExcelJournalImporterTests
{
    [Fact]
    public void Import_MapsTwoSegmentsAndOneSidedMovements()
    {
        string path = GetFixturePath("U_DENNIK_00325791_202412.xlsx");

        JournalImport result =
            new UrbisExcelJournalImporter().Import(path);

        Assert.Equal("Urbis", result.AccountingFormat);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("00325791", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ExportStage);
        Assert.Equal(6, result.Rows.Count);

        Assert.Equal(12m, result.Rows.Sum(row => row.DebitAmount ?? 0m));

        Assert.Equal(12m, result.Rows.Sum(row => row.CreditAmount ?? 0m));

        JournalRow standard = result.Rows[0];

        Assert.Equal("B2", standard.DocumentType);
        Assert.Equal("B2/1", standard.DocumentNumber);
        Assert.Equal("22101", standard.DebitAccount);
        Assert.Equal(5m, standard.DebitAmount);
        Assert.Equal("3192", standard.CreditAccount);
        Assert.Equal(5m, standard.CreditAmount);
        Assert.Equal(3, standard.SourceStartLineNumber);

        JournalRow creditCorrection = result.Rows[1];

        Assert.Null(creditCorrection.DebitAccount);
        Assert.Null(creditCorrection.DebitAmount);
        Assert.Equal("3953", creditCorrection.CreditAccount);
        Assert.Equal(-10m, creditCorrection.CreditAmount);

        JournalRow debitCorrection = result.Rows[3];

        Assert.Equal("3951", debitCorrection.DebitAccount);
        Assert.Equal(-20m, debitCorrection.DebitAmount);
        Assert.Null(debitCorrection.CreditAccount);
        Assert.Null(debitCorrection.CreditAmount);

        Assert.All(
            result.Rows,
            row => Assert.Equal(JournalRecordKind.Normal, row.RecordKind));
    }

    [Fact]
    public void CanImport_RecognizesUrbisJournal()
    {
        var importer = new UrbisExcelJournalImporter();

        Assert.True(importer.CanImport(GetFixturePath("U_DENNIK_00325791_202412.xlsx"), "Urbis"));
        Assert.False(importer.CanImport(GetFixturePath("U_DENNIK_00325791_202412.xlsx"), "IfoSoft"));
    }

    [Fact]
    public void Import_AllowsFilenameWithoutExportStage()
    {
        string copiedPath = CopyFixtureWithName("U_DENNIK_00325791_2024.xlsx");

        try
        {
            JournalImport result = new UrbisExcelJournalImporter() .Import(copiedPath);

            Assert.Null(result.ExportStage);
            Assert.Equal("00325791", result.Ico);
            Assert.Equal(2024, result.FiscalYear);
            Assert.Equal(6, result.Rows.Count);
        }
        finally
        {
            DeleteCopiedFixture(copiedPath);
        }
    }

    [Fact]
    public void Import_PreservesOptionalExportStage()
    {
        string copiedPath = CopyFixtureWithName(
            "U_DENNIK_00325791_202414.xlsx");

        try
        {
            JournalImport result =
                new UrbisExcelJournalImporter()
                    .Import(copiedPath);

            Assert.Equal(14, result.ExportStage);
            Assert.Equal(6, result.Rows.Count);
        }
        finally
        {
            DeleteCopiedFixture(copiedPath);
        }
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Urbis",
            fileName);
    }

    private static string CopyFixtureWithName(
    string fileName)
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc-" +
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string copiedPath =
            Path.Combine(directory, fileName);

        File.Copy(GetFixturePath("U_DENNIK_00325791_202412.xlsx"), copiedPath);

        return copiedPath;
    }

    private static void DeleteCopiedFixture(
        string copiedPath)
    {
        string directory =
            Path.GetDirectoryName(copiedPath);

        if (File.Exists(copiedPath))
        {
            File.Delete(copiedPath);
        }

        if (!string.IsNullOrWhiteSpace(directory) &&
            Directory.Exists(directory))
        {
            Directory.Delete(directory);
        }
    }
}