using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfGeneralLedgerRealFileImportTests
{
    private readonly ITestOutputHelper output;

    public IvesPdfGeneralLedgerRealFileImportTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Parse_real_pdf_from_environment_variable()
    {
        const string variable = "IVES_GENERAL_LEDGER_PDF_TEST_FILE";
        string? path = Environment.GetEnvironmentVariable(variable);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            output.WriteLine(
                variable + " is not set to an existing file. Diagnostic skipped.");
            return;
        }

        IvesGeneralLedgerParseResult result =
            new IvesPdfGeneralLedgerParser().Parse(path);

        Assert.Equal("00325465", result.Ico);
        Assert.Equal(2025, result.FiscalYear);

        output.WriteLine("Activities found:");
        for (int index = 0; index < result.Activities.Count; index++)
        {
            IvesGeneralLedgerActivity activity = result.Activities[index];
            output.WriteLine(
                "  {0}: '{1}' / '{2}' - docs {3}, accounts {4}, synthetic {5}, totals {6}",
                index + 1,
                activity.Name,
                activity.Currency,
                activity.DocumentRows.Count,
                activity.AccountRows.Count,
                activity.SyntheticSummaryRows.Count,
                activity.ReportTotalRows.Count);
        }

        Assert.Equal(2, result.Activities.Count);

        IvesGeneralLedgerActivity main = result.Activities[0];
        Assert.Equal("Hlavná činnosť", main.Name);
        Assert.Equal("EUR", main.Currency);
        Assert.Equal(12712, main.DocumentRows.Count);
        Assert.Equal(802, main.AccountRows.Count);
        Assert.Equal(77, main.SyntheticSummaryRows.Count);
        Assert.Single(main.ReportTotalRows);

        IvesGeneralLedgerActivity catering = result.Activities[1];
        Assert.Equal("Stravovanie", catering.Name);
        Assert.Equal("EUR", catering.Currency);
        Assert.Equal(2, catering.DocumentRows.Count);
        Assert.Equal(6, catering.AccountRows.Count);
        Assert.Equal(4, catering.SyntheticSummaryRows.Count);
        Assert.Single(catering.ReportTotalRows);

        IvesGeneralLedgerSourceRow firstDocument = main.DocumentRows[0];
        Assert.Equal(new DateTime(2025, 6, 30), firstDocument.DocumentDate);
        Assert.Equal("DM0000250183", firstDocument.DocumentNumber);
        Assert.Equal(87202.98m, firstDocument.DebitTurnover);
        Assert.Equal(0m, firstDocument.CreditTurnover);

        IvesGeneralLedgerSourceRow firstAccount = main.AccountRows[0];
        Assert.Equal(128008.61m, firstAccount.OpeningBalance);
        Assert.Equal(87202.98m, firstAccount.DebitTurnover);
        Assert.Equal(0m, firstAccount.CreditTurnover);
        Assert.Equal(215211.59m, firstAccount.ClosingBalance);

        IvesGeneralLedgerSourceRow firstSynthetic =
            main.SyntheticSummaryRows[0];
        Assert.Equal("021", firstSynthetic.AccountCode);
        Assert.Equal(2528448.50m, firstSynthetic.OpeningBalance);
        Assert.Equal(587524.93m, firstSynthetic.DebitTurnover);
        Assert.Equal(0m, firstSynthetic.CreditTurnover);
        Assert.Equal(3115973.43m, firstSynthetic.ClosingBalance);

        IvesGeneralLedgerSourceRow mainTotal =
            Assert.Single(main.ReportTotalRows);
        Assert.Equal(10322.93m, mainTotal.OpeningBalance);
        Assert.Equal(14035906.83m, mainTotal.DebitTurnover);
        Assert.Equal(14035906.83m, mainTotal.CreditTurnover);
        Assert.Equal(10322.93m, mainTotal.ClosingBalance);

        ImportReport validation =
            new IvesGeneralLedgerValidator().Validate(result);

        Assert.True(
            validation.IsValid,
            string.Join(
                Environment.NewLine,
                validation.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Code + ": " + diagnostic.Message)));
        Assert.Empty(validation.Diagnostics);

        output.WriteLine("IVES PDF general-ledger parser verified.");
        output.WriteLine("Physical groups      : " + result.SourceRowCount);
        output.WriteLine("Activities           : " + result.Activities.Count);
        output.WriteLine(
            "Documents            : " +
            result.Activities.Sum(activity => activity.DocumentRows.Count));
        output.WriteLine(
            "Analytical accounts  : " +
            result.Activities.Sum(activity => activity.AccountRows.Count));
        output.WriteLine(
            "Synthetic summaries  : " +
            result.Activities.Sum(
                activity => activity.SyntheticSummaryRows.Count));
        output.WriteLine(
            "Report totals        : " +
            result.Activities.Sum(
                activity => activity.ReportTotalRows.Count));
    }
}
