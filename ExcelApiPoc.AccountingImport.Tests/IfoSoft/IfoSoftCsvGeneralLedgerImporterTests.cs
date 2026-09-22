using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Tests.Common;
using System.Text;

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
    public void Import_AcceptsMedzilaborceCompactLedgerVariant()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "IfoSoftCompactLedgerTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);

        string path = Path.Combine(
            directory,
            "HL_KNIHA_00323233_202512.csv");

        try
        {
            File.WriteAllLines(
                path,
                new[]
                {
                    "Mesto Medzilaborce – Hlavná kniha 2025;;;;;;;;",
                    "Zdroj: HK2025.pdf • obdobie 00/2025 – 12/2025 • všetkých 16 strán v pôvodnom poradí;;;;;;;;",
                    "\"Zachované sú aj medzisúčty a kontrolný súčet z PDF; nesčítavajte ich spolu s analytickými účtami.\";;;;;;;;",
                    ";;;;;;;;",
                    "Účet;Názov účtu;Počiatočný stav (MD ? DAL);Obrat od začiatku roka – MD;Obrat od začiatku roka – DAL;Obrat za posledný mesiac – MD;Obrat za posledný mesiac – DAL;Zostatok účtu (MD ? DAL);Strana PDF",
                    "021101;Stavby - MsÚ;15 640 744,63;214 292,68;5 405,46;;;15 849 631,85;1",
                    "021;Stavby;15 640 744,63;214 292,68;5 405,46;;;15 849 631,85;1",
                    "6**;MEDZISÚČET ZA TRIEDU;;250,00;6 155 070,73;;557 316,73;-6 154 820,73;16",
                    "***;KONTROLNÝ SÚČET;;41 469 921,86;41 469 921,86;5 672 760,86;5 672 760,86;;16"
                },
                Encoding.GetEncoding(1250));

            GeneralLedgerImport result =
                new IfoSoftCsvGeneralLedgerImporter().Import(path);

            Assert.Equal("00323233", result.Ico);
            Assert.Equal("Mesto Medzilaborce", result.CompanyName);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Equal(12, result.ExportStage);
            Assert.Equal(12, result.ThroughMonth);
            Assert.Equal("12/2025", result.PeriodHeader);

            GeneralLedgerRow row = Assert.Single(result.Rows);

            Assert.Equal("021", row.SyntheticCode);
            Assert.Equal("101", row.AnalyticalCode);
            Assert.Equal("021101", row.AccountCode);
            Assert.Equal("Stavby - MsÚ", row.AccountName);
            Assert.Equal(15640744.63m, row.OpeningDebit);
            Assert.Equal(0m, row.OpeningCredit);
            Assert.Equal(214292.68m, row.AnnualDebitTurnover);
            Assert.Equal(5405.46m, row.AnnualCreditTurnover);
            Assert.Equal(15849631.85m, row.ClosingDebit);
            Assert.Equal(0m, row.ClosingCredit);
            Assert.Equal(6, row.SourceRecordNumber);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);

            if (Directory.Exists(directory))
                Directory.Delete(directory);
        }
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
