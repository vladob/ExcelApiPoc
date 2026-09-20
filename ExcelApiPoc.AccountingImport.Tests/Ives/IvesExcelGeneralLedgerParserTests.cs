using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesExcelGeneralLedgerParserTests
{
    [Fact]
    public void Parse_ExtractsMetadataAndAllEstablishedRowSets()
    {
        IvesGeneralLedgerParseResult result = ParseFixture();

        Assert.Equal("HL_KNIHA_00322881_2024.xls", result.SourceFileName);
        Assert.Equal("Sheet1", result.WorksheetName);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(new DateTime(2024, 1, 1), result.PeriodStart);
        Assert.Equal(new DateTime(2024, 12, 31), result.PeriodEnd);
        Assert.Equal(3824, result.SourceRowCount);

        IvesGeneralLedgerActivity activity = Assert.Single(result.Activities);
        Assert.Equal(320, activity.AccountRows.Count);
        Assert.Equal(3382, activity.DocumentRows.Count);
        Assert.Empty(activity.DocumentSummaryRows);
        Assert.Equal(52, activity.SyntheticSummaryRows.Count);
        Assert.Single(activity.ReportTotalRows);
        Assert.Empty(result.UnclassifiedRows);
    }

    [Fact]
    public void Parse_PreservesSourceRowsAndSequencesEachRecordSetIndependently()
    {
        IvesGeneralLedgerParseResult result = ParseFixture();

        IvesGeneralLedgerActivity activity = Assert.Single(result.Activities);

        Assert.Equal((1, 17),
            (activity.AccountRows[0].SequenceNumber,
             activity.AccountRows[0].SourceRowNumber));
        Assert.Equal((1, 29),
            (activity.DocumentRows[0].SequenceNumber,
             activity.DocumentRows[0].SourceRowNumber));
        Assert.Equal((1, 32),
            (activity.SyntheticSummaryRows[0].SequenceNumber,
             activity.SyntheticSummaryRows[0].SourceRowNumber));
        Assert.Equal((1, 3824),
            (activity.ReportTotalRows[0].SequenceNumber,
             activity.ReportTotalRows[0].SourceRowNumber));

        Assert.Equal(320, activity.AccountRows[^1].SequenceNumber);
        Assert.Equal(3382, activity.DocumentRows[^1].SequenceNumber);
        Assert.Equal(52, activity.SyntheticSummaryRows[^1].SequenceNumber);
    }

    [Fact]
    public void Parse_ExtractsRepresentativeTypedValuesWithoutNormalizingAccountLayout()
    {
        IvesGeneralLedgerParseResult result = ParseFixture();
        IvesGeneralLedgerActivity activity = Assert.Single(result.Activities);
        IvesGeneralLedgerSourceRow firstAccount = activity.AccountRows[0];
        IvesGeneralLedgerSourceRow firstDocument = activity.DocumentRows[0];
        IvesGeneralLedgerSourceRow firstSyntheticSummary =
            activity.SyntheticSummaryRows[0];
        IvesGeneralLedgerSourceRow total = activity.ReportTotalRows[0];

        Assert.Equal("021.1    .      .    .       .   . .", firstAccount.AccountCode);
        Assert.Equal("Pociatocny stav budov", firstAccount.Text);
        Assert.Equal(99417.75m, firstAccount.OpeningBalance);
        Assert.Equal(99417.75m, firstAccount.ClosingBalance);

        Assert.Equal(new DateTime(2024, 12, 27), firstDocument.DocumentDate);
        Assert.Equal("ZZ0000000280", firstDocument.DocumentNumber);
        Assert.Equal("021.30   .      .    .       .   . .", firstDocument.AccountCode);
        Assert.Equal(12104.44m, firstDocument.DebitTurnover);
        Assert.Equal(0m, firstDocument.CreditTurnover);

        Assert.Equal("021", firstSyntheticSummary.AccountCode);
        Assert.Equal(391637.48m, firstSyntheticSummary.OpeningBalance);
        Assert.Equal(12104.44m, firstSyntheticSummary.DebitTurnover);
        Assert.Equal(0m, firstSyntheticSummary.CreditTurnover);
        Assert.Equal(403741.92m, firstSyntheticSummary.ClosingBalance);

        Assert.Equal(0m, total.OpeningBalance);
        Assert.Equal(1997652.32m, total.DebitTurnover);
        Assert.Equal(1997652.32m, total.CreditTurnover);
        Assert.Equal(0m, total.ClosingBalance);
    }

    [Fact]
    public void Parse_RejectsNonXlsInputBeforeOpeningWorkbook()
    {
        var parser = new IvesExcelGeneralLedgerParser();
        string path = Path.ChangeExtension(GetFixturePath(), ".xlsx");
        File.Copy(GetFixturePath(), path, true);

        try
        {
            InvalidDataException exception = Assert.Throws<InvalidDataException>(
                () => parser.Parse(path));

            Assert.Contains("original .xls", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static IvesGeneralLedgerParseResult ParseFixture()
    {
        return new IvesExcelGeneralLedgerParser().Parse(GetFixturePath());
    }

    private static string GetFixturePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            "HL_KNIHA_00322881_2024.xls");
    }
}
