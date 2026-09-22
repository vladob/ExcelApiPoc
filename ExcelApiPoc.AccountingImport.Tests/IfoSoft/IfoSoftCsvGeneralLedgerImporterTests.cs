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
    public void Import_RecoversMalformedTrailingQuoteInAccountName()
    {
        string header = Csv(
            "Syn", "Ana", "Typ", "P", "Odd", "Polozka", "KZdroja",
            "Program", "Stred", "Zakaz", "Nazov uctu", "Poc_M", "Poc_D",
            "Roc_M", "Roc_D", "12/2025", "12/2025", "Kon_M", "Kon_D",
            "Plan");

        string malformedRow =
            "\"021\";\"915\";\"M\";\"N\";\"\";\"\";\"\";\"\";\"\";\"\";" +
            "\"Zníženie energ.náročnosti Telocvičňa ZŚ\"\";" +
            "62087,00;;;;;;62087,00;;";

        using var file = new TemporaryCsvFile(
        [
            ";;\"00325937 Obec Úbrež\";",
            ";;\"Hlavna kniha k 12/2025\";",
            header,
            malformedRow
        ]);

        GeneralLedgerImport result =
            new IfoSoftCsvGeneralLedgerImporter().Import(file.Path);

        GeneralLedgerRow row = Assert.Single(result.Rows);

        Assert.Equal("021915", row.AccountCode);
        Assert.Equal(
            "Zníženie energ.náročnosti Telocvičňa ZŚ\"",
            row.AccountName);
        Assert.Equal(62087m, row.OpeningDebit);
        Assert.Equal(62087m, row.ClosingDebit);
    }


    [Fact]
    public void Import_ReportsFileNameAndLineForUnrecoverableMalformedQuote()
    {
        using var file = new TemporaryCsvFile(
        [
            Quote("00325791 Mesto Sobrance"),
            Quote("Hlavná kniha k 12/2024"),
            Csv(
                "Syn", "Ana", "Typ", "P", "Odd", "Polozka", "KZdroja",
                "Program", "Stred", "Zakaz", "Nazov uctu", "Poc_M",
                "Poc_D", "Roc_M", "Roc_D", "12/2024", "12/2024",
                "Kon_M", "Kon_D", "Plan"),
            "\"221\";\"01\";\"B\";\"broken"
        ]);

        InvalidDataException exception =
            Assert.Throws<InvalidDataException>(
                () =>
                    new IfoSoftCsvGeneralLedgerImporter()
                        .Import(file.Path));

        Assert.Contains(
            Path.GetFileName(file.Path),
            exception.Message);

        Assert.Contains(
            "line 4",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "malformed quoted field",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
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
