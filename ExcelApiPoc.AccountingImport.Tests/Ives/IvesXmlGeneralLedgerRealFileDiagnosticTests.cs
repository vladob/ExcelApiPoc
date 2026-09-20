using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXmlGeneralLedgerRealFileDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesXmlGeneralLedgerRealFileDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Parse_real_xml_from_environment_variable()
    {
        const string variable = "IVES_GENERAL_LEDGER_XML_TEST_FILE";
        string? path = Environment.GetEnvironmentVariable(variable);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            output.WriteLine(
                variable + " is not set to an existing file. Diagnostic skipped.");
            return;
        }

        IvesGeneralLedgerParseResult result =
            new IvesXmlGeneralLedgerParser().Parse(path);

        Assert.Equal("00325465", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(new DateTime(2025, 1, 1), result.PeriodStart);
        Assert.Equal(new DateTime(2025, 12, 31), result.PeriodEnd);

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

        Assert.Equal(
            12714,
            result.Activities.Sum(activity => activity.DocumentRows.Count));
        Assert.Equal(
            808,
            result.Activities.Sum(activity => activity.AccountRows.Count));
        Assert.Equal(
            81,
            result.Activities.Sum(
                activity => activity.SyntheticSummaryRows.Count));
        Assert.Equal(
            2,
            result.Activities.Sum(
                activity => activity.ReportTotalRows.Count));

        IvesGeneralLedgerSourceRow mainTotal =
            Assert.Single(main.ReportTotalRows);
        Assert.Equal(10322.93m, mainTotal.OpeningBalance);
        Assert.Equal(14035906.83m, mainTotal.DebitTurnover);
        Assert.Equal(14035906.83m, mainTotal.CreditTurnover);
        Assert.Equal(10322.93m, mainTotal.ClosingBalance);

        IvesGeneralLedgerSourceRow cateringTotal =
            Assert.Single(catering.ReportTotalRows);
        Assert.Equal(0m, cateringTotal.OpeningBalance);
        Assert.Equal(2460.17m, cateringTotal.DebitTurnover);
        Assert.Equal(2460.17m, cateringTotal.CreditTurnover);
        Assert.Equal(0m, cateringTotal.ClosingBalance);

        ImportReport validation =
            new IvesGeneralLedgerValidator().Validate(result);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Diagnostics);

        output.WriteLine("IVES XML general-ledger diagnostic verified.");
        output.WriteLine("Activities         : " + result.Activities.Count);
        output.WriteLine(
            "Documents          : " +
            result.Activities.Sum(activity => activity.DocumentRows.Count));
        output.WriteLine(
            "Analytical accounts: " +
            result.Activities.Sum(activity => activity.AccountRows.Count));
        output.WriteLine(
            "Synthetic summaries: " +
            result.Activities.Sum(
                activity => activity.SyntheticSummaryRows.Count));
        output.WriteLine(
            "Report totals      : " +
            result.Activities.Sum(
                activity => activity.ReportTotalRows.Count));
    }
}
