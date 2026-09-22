using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.MkSoft;

namespace ExcelApiPoc.AccountingImport.Tests.MkSoft;

public sealed class MkSoftExcelGeneralLedgerImporterTests
{
    [Fact]
    public void Imports_real_mksoft_general_ledger_when_configured()
    {
        string? path = Environment.GetEnvironmentVariable(
            "MKSOFT_GL_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
            return;

        var importer = new MkSoftExcelGeneralLedgerImporter();

        Assert.True(importer.CanImport(path, "MkSoft"));
        Assert.False(importer.CanImport(path, "Urbis"));

        GeneralLedgerImport result = importer.Import(path);

        Assert.Equal("MkSoft", result.AccountingFormat);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("35581638", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Null(result.ExportStage);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Equal("12/2024", result.PeriodHeader);
        Assert.Equal(191, result.Rows.Count);
        Assert.Equal(64, result.SourceFileHash.Length);

        Assert.Equal(
            23272870.30m,
            result.Rows.Sum(row => row.OpeningDebit));

        Assert.Equal(
            23272870.30m,
            result.Rows.Sum(row => row.OpeningCredit));

        Assert.Equal(
            10642153.79m,
            result.Rows.Sum(row => row.AnnualDebitTurnover));

        Assert.Equal(
            10642153.79m,
            result.Rows.Sum(row => row.AnnualCreditTurnover));

        Assert.Equal(
            10642153.79m,
            result.Rows.Sum(row => row.PeriodDebitTurnover));

        Assert.Equal(
            10642153.79m,
            result.Rows.Sum(row => row.PeriodCreditTurnover));

        Assert.Equal(
            15301260.03m,
            result.Rows.Sum(row => row.ClosingDebit));

        Assert.Equal(
            15301260.03m,
            result.Rows.Sum(row => row.ClosingCredit));

        GeneralLedgerRow software = Assert.Single(
            result.Rows,
            row => row.AccountCode == "01310");

        Assert.Equal("013", software.SyntheticCode);
        Assert.Equal("10", software.AnalyticalCode);
        Assert.Equal("Softvér", software.AccountName);
        Assert.Equal(828m, software.OpeningDebit);
        Assert.Equal(0m, software.OpeningCredit);
        Assert.Equal(828m, software.ClosingDebit);
        Assert.Equal(0m, software.ClosingCredit);

        GeneralLedgerRow construction = Assert.Single(
            result.Rows,
            row => row.AccountCode == "04218");

        Assert.Equal(
            construction.ClosingDebit,
            0m);
        Assert.Equal(
            construction.ClosingCredit,
            0m);

        Assert.Equal(
            result.Rows.Sum(row => row.OpeningDebit) +
            result.Rows.Sum(row => row.AnnualDebitTurnover),
            33915024.09m);

        Assert.Equal(
            result.Rows.Sum(row => row.OpeningCredit) +
            result.Rows.Sum(row => row.AnnualCreditTurnover),
            33915024.09m);
    }

    [Fact]
    public void Rejects_xlsx_even_for_mksoft_filename()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "MkSoftGeneralLedgerImporterTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "HL_KNIHA_35581638_2024.xlsx");

        try
        {
            File.WriteAllText(path, "not an xls");

            var importer = new MkSoftExcelGeneralLedgerImporter();

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
