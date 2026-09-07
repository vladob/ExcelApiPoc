using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;

namespace ExcelApiPoc.AccountingImport.Tests;

public sealed class UrbisExcelGeneralLedgerImporterTests
{
    [Fact]
    public void Import_MapsBalancesAndIgnoresGroupRows()
    {
        GeneralLedgerImport result =
            new UrbisExcelGeneralLedgerImporter()
                .Import(GetFixturePath());

        Assert.Equal("Urbis", result.AccountingFormat);
        Assert.Equal("00325791", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ExportStage);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Equal("12/2024", result.PeriodHeader);
        Assert.Equal(4, result.Rows.Count);

        Assert.Equal(
            130m,
            result.Rows.Sum(row => row.OpeningDebit));

        Assert.Equal(
            40m,
            result.Rows.Sum(row => row.OpeningCredit));

        Assert.Equal(
            330m,
            result.Rows.Sum(row => row.AnnualDebitTurnover));

        Assert.Equal(
            205m,
            result.Rows.Sum(row => row.AnnualCreditTurnover));

        Assert.Equal(
            275m,
            result.Rows.Sum(row => row.ClosingDebit));

        Assert.Equal(
            60m,
            result.Rows.Sum(row => row.ClosingCredit));

        GeneralLedgerRow bank = result.Rows[0];

        Assert.Equal("22101", bank.AccountCode);
        Assert.Equal("221", bank.SyntheticCode);
        Assert.Equal("01", bank.AnalyticalCode);
        Assert.Equal("Bežný účet", bank.AccountName);
        Assert.Equal("B", bank.Type);

        GeneralLedgerRow correction =
            Assert.Single(
                result.Rows,
                row => row.AccountCode == "019");

        Assert.Equal(
            -30m,
            correction.AnnualDebitTurnover);

        GeneralLedgerRow slashAccount =
            Assert.Single(
                result.Rows,
                row => row.AccountCode == "431/B");

        Assert.Equal("431", slashAccount.SyntheticCode);
        Assert.Equal("/B", slashAccount.AnalyticalCode);
    }

    [Fact]
    public void CanImport_RecognizesUrbisGeneralLedger()
    {
        var importer =
            new UrbisExcelGeneralLedgerImporter();

        Assert.True(
            importer.CanImport(
                GetFixturePath(),
                "Urbis"));

        Assert.False(
            importer.CanImport(
                GetFixturePath(),
                "IfoSoft"));
    }

    private static string GetFixturePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "HL_KNIHA_00325791_202412.xls");
    }
}