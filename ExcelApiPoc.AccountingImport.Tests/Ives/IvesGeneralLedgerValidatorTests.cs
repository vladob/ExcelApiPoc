using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesGeneralLedgerValidatorTests
{
    [Fact]
    public void Validate_ConfirmsAllFinancialRelationshipsInFixture()
    {
        ImportReport report = ValidateFixture();

        Assert.True(report.IsValid);
        Assert.Empty(report.Diagnostics);
        Assert.Equal(1172, report.ValidationResults.Count);
        Assert.All(report.ValidationResults, validation => Assert.True(validation.IsValid));

        Assert.Equal(320, report.RecordCounts["Accounts"]);
        Assert.Equal(3382, report.RecordCounts["Documents"]);
        Assert.Equal(0, report.RecordCounts["DocumentSummaries"]);
        Assert.Equal(1, report.RecordCounts["Activities"]);
        Assert.Equal(52, report.RecordCounts["SyntheticSummaries"]);
        Assert.Equal(1, report.RecordCounts["ReportTotals"]);
        Assert.Equal(0, report.RecordCounts["Unclassified"]);
    }

    [Fact]
    public void Validate_ReportsAccountBalanceDifferenceWithSourceProvenance()
    {
        IvesGeneralLedgerParseResult source = ParseFixture();
        source.Activities[0].AccountRows[0].ClosingBalance += 1m;

        ImportReport report = new IvesGeneralLedgerValidator().Validate(source);
        ImportValidationResult failure = Assert.Single(report.ValidationResults,
            result => result.Code == "IVES.ACCOUNT.BALANCE" && !result.IsValid);

        Assert.False(report.IsValid);
        Assert.Equal("021.1    .      .    .       .   . .", failure.Scope);
        Assert.Equal(17, failure.Source.SourceRowNumber);
        Assert.Equal(1, failure.Source.SequenceNumber);
        Assert.Equal(1m, failure.Difference);
        Assert.Contains(report.Diagnostics,
            diagnostic => diagnostic.Code == "IVES.ACCOUNT.BALANCE");
    }

    [Fact]
    public void Validate_ReportsDocumentToAccountTurnoverDifference()
    {
        IvesGeneralLedgerParseResult source = ParseFixture();
        source.Activities[0].DocumentRows[0].DebitTurnover += 1m;

        ImportReport report = new IvesGeneralLedgerValidator().Validate(source);

        Assert.False(report.IsValid);
        Assert.Contains(report.ValidationResults,
            result => result.Code == "IVES.DOCUMENTS.DEBIT" &&
                      result.Scope == source.Activities[0].DocumentRows[0].AccountCode &&
                      !result.IsValid && result.Difference == -1m);
    }

    [Fact]
    public void Validate_ReportsAccountToSyntheticDifference()
    {
        IvesGeneralLedgerParseResult source = ParseFixture();
        source.Activities[0].AccountRows[0].OpeningBalance += 1m;

        ImportReport report = new IvesGeneralLedgerValidator().Validate(source);

        Assert.False(report.IsValid);
        Assert.Contains(report.ValidationResults,
            result => result.Code == "IVES.SYNTHETIC.OPENING" &&
                      result.Scope == "021" && !result.IsValid &&
                      result.Difference == -1m);
    }

    [Fact]
    public void Validate_ReportsSyntheticToReportTotalDifference()
    {
        IvesGeneralLedgerParseResult source = ParseFixture();
        source.Activities[0].ReportTotalRows[0].DebitTurnover += 1m;

        ImportReport report = new IvesGeneralLedgerValidator().Validate(source);

        Assert.False(report.IsValid);
        ImportValidationResult failure = Assert.Single(report.ValidationResults,
            result => result.Code == "IVES.REPORT_TOTAL.DEBIT" && !result.IsValid);
        Assert.Equal(3824, failure.Source.SourceRowNumber);
        Assert.Equal(1m, failure.Difference);
    }

    [Fact]
    public void Validate_TreatsUnclassifiedMeaningfulRowAsError()
    {
        IvesGeneralLedgerParseResult source = ParseFixture();
        source.UnclassifiedRows.Add(new IvesGeneralLedgerSourceRow
        {
            Kind = IvesGeneralLedgerRowKind.Unclassified,
            SourceRowNumber = 100,
            Text = "Unexpected row"
        });

        ImportReport report = new IvesGeneralLedgerValidator().Validate(source);

        Assert.False(report.IsValid);
        ImportDiagnostic diagnostic = Assert.Single(report.Diagnostics,
            item => item.Code == "IVES.ROW.UNCLASSIFIED");
        Assert.Equal(100, diagnostic.Source.SourceRowNumber);
    }

    private static ImportReport ValidateFixture()
    {
        return new IvesGeneralLedgerValidator().Validate(ParseFixture());
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
