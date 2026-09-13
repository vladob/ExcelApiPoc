using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesJournalValidatorTests
{
    [Theory]
    [InlineData("U_DENNIK_00322881_2022.xls", 1579, 2)]
    [InlineData("U_DENNIK_00322881_2023.xls", 1575, 3)]
    [InlineData("U_DENNIK_00322881_2024.xls", 2125, 2)]
    public void Validate_ConfirmsStructureAndFinancialRelationships(
        string fileName,
        int transactionCount,
        int validationCount)
    {
        ImportReport report = ValidateFixture(fileName);

        Assert.True(report.IsValid);
        Assert.Empty(report.Diagnostics);
        Assert.Equal(validationCount, report.ValidationResults.Count);
        Assert.All(report.ValidationResults, result => Assert.True(result.IsValid));

        Assert.Equal("IVES", report.AccountingFormat);
        Assert.Equal("Journal", report.ImportType);
        Assert.Equal(transactionCount, report.RecordCounts["Transactions"]);
        Assert.Equal(transactionCount, report.RecordCounts["Modules"]);
        Assert.Equal(1, report.RecordCounts["ReportTotals"]);
        Assert.Equal(0, report.RecordCounts["Unclassified"]);
    }

    [Fact]
    public void Validate_ReconcilesSingleFooterAgainstDisplayedAmounts()
    {
        ImportReport report = ValidateFixture("U_DENNIK_00322881_2024.xls");
        ImportValidationResult result = Assert.Single(
            report.ValidationResults,
            item => item.Code == "IVES.JOURNAL.REPORT_TOTAL.DISPLAYED");

        Assert.True(result.IsValid);
        Assert.Equal(2114932.51m, result.ExpectedAmount);
        Assert.Equal(2114932.51m, result.ActualAmount);
        Assert.Equal(4500, result.Source.SourceRowNumber);
        Assert.Equal(1, result.Source.SequenceNumber);
        Assert.Equal("ReportTotals", result.Source.RecordSet);
    }

    [Fact]
    public void Validate_ReconcilesDualFooterAgainstDebitAndCreditTotals()
    {
        ImportReport report = ValidateFixture("U_DENNIK_00322881_2023.xls");

        ImportValidationResult debit = Assert.Single(
            report.ValidationResults,
            item => item.Code == "IVES.JOURNAL.REPORT_TOTAL.DEBIT");
        ImportValidationResult credit = Assert.Single(
            report.ValidationResults,
            item => item.Code == "IVES.JOURNAL.REPORT_TOTAL.CREDIT");

        Assert.True(debit.IsValid);
        Assert.True(credit.IsValid);
        Assert.Equal(531194.94m, debit.ExpectedAmount);
        Assert.Equal(531194.94m, credit.ExpectedAmount);
        Assert.Equal(3361, debit.Source.SourceRowNumber);
    }

    [Fact]
    public void Validate_ReportsUnbalancedJournalWithSourceIndependentScope()
    {
        IvesJournalParseResult source = ParseFixture("U_DENNIK_00322881_2024.xls");
        IvesJournalSourceRow oneSidedCredit = source.TransactionRows.First(row =>
            string.IsNullOrWhiteSpace(row.DebitCompositeAccount) &&
            !string.IsNullOrWhiteSpace(row.CreditCompositeAccount));
        oneSidedCredit.Amount += 1m;

        ImportReport report = new IvesJournalValidator().Validate(source);
        ImportValidationResult failure = Assert.Single(
            report.ValidationResults,
            item => item.Code == "IVES.JOURNAL.BALANCE" && !item.IsValid);

        Assert.False(report.IsValid);
        Assert.Equal(1m, failure.Difference);
        Assert.Null(failure.Source.SourceRowNumber);
        Assert.Contains(report.Diagnostics,
            item => item.Code == "IVES.JOURNAL.BALANCE");
    }

    [Fact]
    public void Validate_ReportsFooterDifferenceWithSourceProvenance()
    {
        IvesJournalParseResult source = ParseFixture("U_DENNIK_00322881_2024.xls");
        source.ReportTotalRows[0].ReportedAmounts[0] += 1m;

        ImportReport report = new IvesJournalValidator().Validate(source);
        ImportValidationResult failure = Assert.Single(
            report.ValidationResults,
            item => item.Code == "IVES.JOURNAL.REPORT_TOTAL.DISPLAYED" &&
                    !item.IsValid);

        Assert.False(report.IsValid);
        Assert.Equal(1m, failure.Difference);
        Assert.Equal(4500, failure.Source.SourceRowNumber);
        Assert.Equal(1, failure.Source.SequenceNumber);
    }

    [Fact]
    public void Validate_ReportsCurrencyAndPeriodErrorsWithTransactionProvenance()
    {
        IvesJournalParseResult source = ParseFixture("U_DENNIK_00322881_2024.xls");
        IvesJournalSourceRow transaction = source.TransactionRows[0];
        transaction.Currency = "USD";
        transaction.PostingDate = new DateTime(2025, 1, 1);

        ImportReport report = new IvesJournalValidator().Validate(source);

        ImportDiagnostic currency = Assert.Single(report.Diagnostics,
            item => item.Code == "IVES.JOURNAL.CURRENCY.UNSUPPORTED");
        ImportDiagnostic period = Assert.Single(report.Diagnostics,
            item => item.Code == "IVES.JOURNAL.DATE.OUTSIDE_PERIOD");

        Assert.False(report.IsValid);
        Assert.Equal(transaction.SourceRowNumber, currency.Source.SourceRowNumber);
        Assert.Equal(transaction.SequenceNumber, currency.Source.SequenceNumber);
        Assert.Equal("Transactions", period.Source.RecordSet);
    }

    [Fact]
    public void Validate_ReportsBrokenTransactionModulePair()
    {
        IvesJournalParseResult source = ParseFixture("U_DENNIK_00322881_2024.xls");
        IvesJournalSourceRow transaction = source.TransactionRows[0];
        transaction.RelatedSourceRowNumber += 1;

        ImportReport report = new IvesJournalValidator().Validate(source);
        ImportDiagnostic diagnostic = Assert.Single(report.Diagnostics,
            item => item.Code == "IVES.JOURNAL.MODULE.PAIR_MISSING");

        Assert.False(report.IsValid);
        Assert.Equal(transaction.SourceRowNumber, diagnostic.Source.SourceRowNumber);
    }

    [Fact]
    public void Validate_TreatsUnclassifiedMeaningfulRowAsError()
    {
        IvesJournalParseResult source = ParseFixture("U_DENNIK_00322881_2024.xls");
        source.UnclassifiedRows.Add(new IvesJournalSourceRow
        {
            Kind = IvesJournalRowKind.Unclassified,
            SourceRowNumber = 100,
            Text = "Unexpected row"
        });

        ImportReport report = new IvesJournalValidator().Validate(source);
        ImportDiagnostic diagnostic = Assert.Single(report.Diagnostics,
            item => item.Code == "IVES.JOURNAL.ROW.UNCLASSIFIED");

        Assert.False(report.IsValid);
        Assert.Equal(100, diagnostic.Source.SourceRowNumber);
        Assert.Equal(1, report.RecordCounts["Unclassified"]);
    }

    private static ImportReport ValidateFixture(string fileName)
    {
        return new IvesJournalValidator().Validate(ParseFixture(fileName));
    }

    private static IvesJournalParseResult ParseFixture(string fileName)
    {
        return new IvesExcelJournalParser().Parse(GetFixturePath(fileName));
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
