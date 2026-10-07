using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Tests.Common;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftCsvGeneralLedgerImporterTests
{
    [Fact]
    public void Import_MapsMetadataAccountAndBalances()
    {
        using var file = CreateLedger("12/2024", "12/2024");

        GeneralLedgerImport result =
            new IfoSoftCsvGeneralLedgerImporter().Import(file.Path);

        Assert.Equal("00325791", result.Ico);
        Assert.Equal("Mesto Sobrance", result.CompanyName);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Equal("12/2024", result.PeriodHeader);

        GeneralLedgerRow row = Assert.Single(result.Rows);
        Assert.Equal("221", row.SyntheticCode);
        Assert.Equal("01", row.AnalyticalCode);
        Assert.Equal("22101", row.AccountCode);
        Assert.Equal("Zakladný bežný účet", row.AccountName);
        Assert.Equal(100.25m, row.OpeningDebit);
        Assert.Equal(0m, row.OpeningCredit);
        Assert.Equal(250.50m, row.AnnualDebitTurnover);
        Assert.Equal(75.25m, row.AnnualCreditTurnover);
        Assert.Equal(275.50m, row.ClosingDebit);
        Assert.Equal(0m, row.ClosingCredit);
        Assert.Equal(4, row.SourceRecordNumber);
    }

    [Fact]
    public void Import_ReportsInconsistentPeriodColumns()
    {
        using var file = CreateLedger("12/2024", "11/2024");

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new IfoSoftCsvGeneralLedgerImporter().Import(file.Path));

        Assert.Contains("Line 3", exception.Message);
        Assert.Contains("invalid period or closing-balance columns", exception.Message);
    }

    [Fact]
    public void Import_Kamienka_ledger_with_timestamp_header_and_filename_ico()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_CSV_KAMIENKA_LEDGER_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_CSV_KAMIENKA_LEDGER_TEST_FILE to HL_KNIHA_00323110_2024.CSV.");
        GeneralLedgerImport import = new IfoSoftCsvGeneralLedgerImporter().Import(path!);
        Assert.Equal("00323110", import.Ico);
        Assert.Equal("OBEC KAMIENKA", import.CompanyName);
        Assert.Equal(2024, import.FiscalYear);
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_CSV_ICO_FROM_FILENAME");
        Assert.NotEmpty(import.Rows);
    }

    [Fact]
    public void Import_ReportsUnterminatedFieldAtPhysicalLine13()
    {
        using var file = CreateLedger("12/2024", "12/2024");
        File.AppendAllLines(file.Path, Enumerable.Repeat("", 8).Concat(new[] { "\"unfinished" }),
            System.Text.Encoding.GetEncoding(1250));
        var error = Assert.Throws<InvalidDataException>(() => new IfoSoftCsvGeneralLedgerImporter().Import(file.Path));
        Assert.Contains(Path.GetFileName(file.Path), error.Message);
        Assert.Contains("line 13:", error.Message);
        Assert.Contains("unterminated quoted field", error.Message);
        Assert.IsType<InvalidDataException>(error.InnerException);
    }

    private static TemporaryCsvFile CreateLedger(
        string debitPeriod,
        string creditPeriod)
    {
        string header = Csv(
            "Syn", "Ana", "Typ", "P", "Odd", "Polozka", "KZdroja",
            "Program", "Stred", "Zakaz", "Nazov uctu", "Poc_M", "Poc_D",
            "Roc_M", "Roc_D", debitPeriod, creditPeriod, "Kon_M", "Kon_D",
            "Plan");

        string row = Csv(
            "221", "01", "B", "", "", "", "", "", "", "",
            "Zakladný bežný účet", "100,25", "", "250,50", "75,25",
            "250,50", "75,25", "275,50", "", "0,00");

        return new TemporaryCsvFile(
        [
            Quote("00325791 Mesto Sobrance"),
            Quote("Hlavná kniha k 12/2024"),
            header,
            row
        ]);
    }

    private static string Csv(params string[] fields)
    {
        return string.Join(";", fields.Select(Quote));
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
