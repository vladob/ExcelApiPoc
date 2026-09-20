using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesExcelGeneralLedgerImporterTests
{
    [Fact]
    public void CanImport_RecognizesOnlyIvesGeneralLedgerXlsFiles()
    {
        var importer = new IvesExcelGeneralLedgerImporter();
        string fixture = GetFixturePath();

        Assert.True(importer.CanImport(fixture, "IVES"));
        Assert.True(importer.CanImport(fixture, "ives"));
        Assert.False(importer.CanImport(fixture, "Urbis"));
        Assert.False(importer.CanImport(
            Path.ChangeExtension(fixture, ".xlsx"),
            "IVES"));
        Assert.False(importer.CanImport(
            Path.Combine(
                Path.GetDirectoryName(fixture)!,
                "U_DENNIK_00322881_2024.xls"),
            "IVES"));
    }

    [Fact]
    public void Import_MapsMetadataAndAttachesSuccessfulValidationReport()
    {
        GeneralLedgerImport result = ImportFixture();

        Assert.Equal("HL_KNIHA_00322881_2024.xls", result.SourceFileName);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("IVES", result.AccountingFormat);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Equal("12/2024", result.PeriodHeader);
        Assert.Equal(
            "8BDB81B3DCDEBB96061A8150A149295C672745B08BBC25632F055FC0ECC6BFBB",
            result.SourceFileHash);

        Assert.NotNull(result.ImportReport);
        Assert.True(result.ImportReport.IsValid);
        Assert.Empty(result.ImportReport.Diagnostics);
        Assert.Equal(1172, result.ImportReport.ValidationResults.Count);
    }

    [Fact]
    public void Import_MapsOnlyAccountRecordsAndPreservesCompositeAccountIdentity()
    {
        GeneralLedgerImport result = ImportFixture();

        Assert.Equal(320, result.Rows.Count);
        Assert.Equal(320, result.Rows.Select(row => row.AccountCode).Distinct().Count());
        Assert.DoesNotContain(result.Rows, row => row.AccountCode.Contains("="));
        Assert.DoesNotContain(result.Rows, row => row.SourceRecordNumber == 32);

        GeneralLedgerRow first = result.Rows[0];
        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(17, first.SourceRecordNumber);
        Assert.Equal("021", first.SyntheticCode);
        Assert.Equal(".1......", first.AnalyticalCode);
        Assert.Equal("021.1......", first.AccountCode);
        Assert.Equal("Pociatocny stav budov", first.AccountName);
        Assert.Equal(99417.75m, first.OpeningDebit);
        Assert.Equal(0m, first.OpeningCredit);
        Assert.Equal(0m, first.AnnualDebitTurnover);
        Assert.Equal(0m, first.AnnualCreditTurnover);
        Assert.Equal(99417.75m, first.ClosingDebit);
        Assert.Equal(0m, first.ClosingCredit);

        GeneralLedgerRow accountWithTurnover = Assert.Single(
            result.Rows,
            row => row.SourceRecordNumber == 30);
        Assert.Equal(12104.44m, accountWithTurnover.AnnualDebitTurnover);
        Assert.Equal(12104.44m, accountWithTurnover.PeriodDebitTurnover);
        Assert.Equal(0m, accountWithTurnover.AnnualCreditTurnover);
        Assert.Equal(0m, accountWithTurnover.PeriodCreditTurnover);
    }

    [Fact]
    public void Import_SplitsNegativeSignedBalancesIntoCanonicalCreditFields()
    {
        GeneralLedgerImport result = ImportFixture();
        GeneralLedgerRow account = Assert.Single(
            result.Rows,
            row => row.SourceRecordNumber == 51);

        Assert.Equal(21, account.SequenceNumber);
        Assert.Equal("042", account.SyntheticCode);
        Assert.Equal("......1.", account.AnalyticalCode);
        Assert.Equal("042......1.", account.AccountCode);
        Assert.Equal("Obstaranie", account.AccountName);
        Assert.Equal(0m, account.OpeningDebit);
        Assert.Equal(34765.97m, account.OpeningCredit);
        Assert.Equal(0m, account.ClosingDebit);
        Assert.Equal(34765.97m, account.ClosingCredit);
    }

    [Fact]
    public void Import_WideSplitRowLayout_ReconcilesAllRecordSets()
    {
        GeneralLedgerImport result = ImportWideFixture();

        Assert.Equal("00999999", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Single(result.Rows);

        Assert.NotNull(result.ImportReport);
        Assert.True(result.ImportReport.IsValid);
        Assert.Empty(result.ImportReport.Diagnostics);
        Assert.Equal(11, result.ImportReport.ValidationResults.Count);
        Assert.Equal(1, result.ImportReport.RecordCounts["Accounts"]);
        Assert.Equal(1, result.ImportReport.RecordCounts["Documents"]);
        Assert.Equal(1, result.ImportReport.RecordCounts["Activities"]);
        Assert.Equal(1, result.ImportReport.RecordCounts["SyntheticSummaries"]);
        Assert.Equal(1, result.ImportReport.RecordCounts["ReportTotals"]);
        Assert.Equal(0, result.ImportReport.RecordCounts["Unclassified"]);
    }

    [Fact]
    public void Import_WideSplitRowLayout_CombinesAccountAndAmountRows()
    {
        GeneralLedgerImport result = ImportWideFixture();

        GeneralLedgerRow first = result.Rows[0];
        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(18, first.SourceRecordNumber);
        Assert.Equal("019", first.SyntheticCode);
        Assert.Equal(".1......", first.AnalyticalCode);
        Assert.Equal("019.1......", first.AccountCode);
        Assert.Equal("Synthetic account", first.AccountName);
        Assert.Equal(100m, first.OpeningDebit);
        Assert.Equal(0m, first.OpeningCredit);
        Assert.Equal(20m, first.AnnualDebitTurnover);
        Assert.Equal(5m, first.AnnualCreditTurnover);
        Assert.Equal(115m, first.ClosingDebit);
        Assert.Equal(0m, first.ClosingCredit);
    }

    private static GeneralLedgerImport ImportFixture()
    {
        return new IvesExcelGeneralLedgerImporter().Import(GetFixturePath());
    }

    private static GeneralLedgerImport ImportWideFixture()
    {
        return new IvesExcelGeneralLedgerImporter().Import(
            GetFixturePath("HL_KNIHA_00999999_2025.xls"));
    }

    private static string GetFixturePath()
    {
        return GetFixturePath("HL_KNIHA_00322881_2024.xls");
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            fileName);
    }
}
