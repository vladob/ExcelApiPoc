using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXlsxGeneralLedgerRealFileDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesXlsxGeneralLedgerRealFileDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Parse_real_xlsx_from_environment_variable()
    {
        const string variable = "IVES_GENERAL_LEDGER_XLSX_TEST_FILE";
        string? path = Environment.GetEnvironmentVariable(variable);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            output.WriteLine(
                variable + " is not set to an existing file. Diagnostic skipped.");
            return;
        }

        IvesGeneralLedgerParseResult result =
            new IvesXlsxGeneralLedgerParser().Parse(path);

        Assert.Equal("00325465", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
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

        Assert.Empty(result.UnclassifiedRows);

        IvesGeneralLedgerSourceRow firstDocument = main.DocumentRows[0];
        Assert.Equal(new DateTime(2025, 6, 30), firstDocument.DocumentDate);
        Assert.Equal("DM0000250183", firstDocument.DocumentNumber);
        Assert.Equal(
            "021.1    .      .    .       .   .š.",
            firstDocument.AccountCode);
        Assert.Equal(87202.98m, firstDocument.DebitTurnover);
        Assert.Equal(0m, firstDocument.CreditTurnover);

        IvesGeneralLedgerSourceRow firstAccount = main.AccountRows[0];
        Assert.Equal(
            "021.1    .      .    .       .   .š.",
            firstAccount.AccountCode);
        Assert.Equal("Budovy", firstAccount.Text);
        Assert.Equal(128008.61m, firstAccount.OpeningBalance);
        Assert.Equal(87202.98m, firstAccount.DebitTurnover);
        Assert.Equal(0m, firstAccount.CreditTurnover);
        Assert.Equal(215211.59m, firstAccount.ClosingBalance);

        IvesGeneralLedgerSourceRow mainTotal =
            Assert.Single(main.ReportTotalRows);
        Assert.Equal(10322.93m, mainTotal.OpeningBalance);
        Assert.Equal(14035906.83m, mainTotal.DebitTurnover);
        Assert.Equal(14035906.83m, mainTotal.CreditTurnover);
        Assert.Equal(10322.93m, mainTotal.ClosingBalance);

        ImportReport validation =
            new IvesGeneralLedgerValidator().Validate(result);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Diagnostics);

        output.WriteLine("IVES XLSX general-ledger diagnostic verified.");
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
