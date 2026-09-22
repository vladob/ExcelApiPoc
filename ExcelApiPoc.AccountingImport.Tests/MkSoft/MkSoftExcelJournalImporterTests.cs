using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.MkSoft;

namespace ExcelApiPoc.AccountingImport.Tests.MkSoft;

public sealed class MkSoftExcelJournalImporterTests
{
    [Fact]
    public void Imports_real_mksoft_accounting_journal_when_configured()
    {
        string? path = Environment.GetEnvironmentVariable(
            "MKSOFT_AJ_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
            return;

        var importer = new MkSoftExcelJournalImporter();

        Assert.True(importer.CanImport(path, "MkSoft"));
        Assert.False(importer.CanImport(path, "Urbis"));

        JournalImport result = importer.Import(path);

        Assert.Equal("MkSoft", result.AccountingFormat);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("35581638", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Null(result.ExportStage);
        Assert.Equal(5654, result.Rows.Count);
        Assert.Equal(64, result.SourceFileHash.Length);

        Assert.Equal(
            5397,
            result.Rows.Count(
                row => row.RecordKind == JournalRecordKind.Normal));

        Assert.Equal(
            91,
            result.Rows.Count(
                row => row.RecordKind == JournalRecordKind.Opening));

        Assert.Equal(
            166,
            result.Rows.Count(
                row => row.RecordKind == JournalRecordKind.Closing));

        Assert.Equal(
            23272870.30m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Opening)
                .Sum(row => row.DebitAmount ?? 0m));

        Assert.Equal(
            23272870.30m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Opening)
                .Sum(row => row.CreditAmount ?? 0m));

        Assert.Equal(
            10642153.79m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Normal)
                .Sum(row => row.DebitAmount ?? 0m));

        Assert.Equal(
            10642153.79m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Normal)
                .Sum(row => row.CreditAmount ?? 0m));

        Assert.Equal(
            28598830.28m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Closing)
                .Sum(row => row.DebitAmount ?? 0m));

        Assert.Equal(
            28598830.28m,
            result.Rows
                .Where(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Closing)
                .Sum(row => row.CreditAmount ?? 0m));

        Assert.Equal(
            172,
            result.Rows.Count(
                row =>
                    row.DebitAmount.HasValue !=
                    row.CreditAmount.HasValue));

        Assert.Contains(
            result.Rows,
            row =>
                row.DebitAccount == "33621" &&
                row.CreditAccount == "22111" &&
                row.DebitAmount == 97.24m &&
                !row.CreditAmount.HasValue);

        Assert.Contains(
            result.Rows,
            row =>
                row.RecordKind == JournalRecordKind.Opening &&
                row.Description == "Začiatočný doklad súvahový");

        Assert.Contains(
            result.Rows,
            row =>
                row.RecordKind == JournalRecordKind.Closing &&
                row.Description == "Konečný doklad súvahový");
    }

    [Fact]
    public void Rejects_xlsx_even_for_mksoft_journal_filename()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "MkSoftJournalImporterTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "U_DENNIK_35581638_2024.xlsx");

        try
        {
            File.WriteAllText(path, "not an xls");

            var importer = new MkSoftExcelJournalImporter();

            Assert.False(importer.CanImport(path, "MkSoft"));

            InvalidDataException exception =
                Assert.Throws<InvalidDataException>(
                    () => importer.Import(path));

            Assert.Contains(
                "original .xls",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
