using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfJournalRealFileImportTests
{
    private readonly ITestOutputHelper output;

    public IvesPdfJournalRealFileImportTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Import_real_pdf_from_environment_variable()
    {
        string? path =
            Environment.GetEnvironmentVariable(
                "IVES_JOURNAL_PDF_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(
                "IVES_JOURNAL_PDF_TEST_FILE is not set. " +
                "Real-file diagnostic was not run.");
            return;
        }

        Assert.True(File.Exists(path));

        IvesJournalParseResult parsed =
            new IvesPdfJournalParser().Parse(path);
        JournalImport imported =
            new IvesJournalImporter().Import(path);

        Assert.Equal("PDF", imported.TechnicalType);
        Assert.Equal("00325465", imported.Ico);
        Assert.Equal(2025, imported.FiscalYear);
        Assert.Equal(6391, parsed.TransactionRows.Count);
        Assert.Equal(6391, imported.Rows.Count);
        Assert.Single(parsed.ReportTotalRows);
        Assert.Single(parsed.ReportTotalRows[0].ReportedAmounts);
        Assert.Equal(
            14038367.00m,
            parsed.ReportTotalRows[0].ReportedAmounts[0]);
        Assert.True(imported.ImportReport.IsValid);
        Assert.Empty(imported.ImportReport.Diagnostics);

        decimal displayedTotal = parsed.TransactionRows.Sum(
            row => row.Amount.GetValueOrDefault());

        IvesJournalSourceRow first = parsed.TransactionRows.First();
        IvesJournalSourceRow last = parsed.TransactionRows.Last();

        output.WriteLine("IVES real PDF import diagnostic");
        output.WriteLine("-------------------------------");
        output.WriteLine("File             : {0}", Path.GetFileName(path));
        output.WriteLine("Technical type   : {0}", imported.TechnicalType);
        output.WriteLine("IČO              : {0}", imported.Ico);
        output.WriteLine("Fiscal year      : {0}", imported.FiscalYear);
        output.WriteLine("Transactions     : {0}", parsed.TransactionRows.Count);
        output.WriteLine("Canonical rows   : {0}", imported.Rows.Count);
        output.WriteLine("Displayed total  : {0:N2}", displayedTotal);
        output.WriteLine(
            "Reported total   : {0:N2}",
            parsed.ReportTotalRows[0].ReportedAmounts[0]);
        output.WriteLine(
            "First row        : #{0}, {1:yyyy-MM-dd}, {2}, {3} -> {4}, {5:N2}, {6}, {7}",
            first.SequenceNumber,
            first.PostingDate,
            first.DocumentNumber,
            first.DebitCompositeAccount ?? "(null)",
            first.CreditCompositeAccount ?? "(null)",
            first.Amount,
            first.Module,
            first.Text ?? "(null)");
        output.WriteLine(
            "Last row         : #{0}, {1:yyyy-MM-dd}, {2}, {3} -> {4}, {5:N2}, {6}, {7}",
            last.SequenceNumber,
            last.PostingDate,
            last.DocumentNumber,
            last.DebitCompositeAccount ?? "(null)",
            last.CreditCompositeAccount ?? "(null)",
            last.Amount,
            last.Module,
            last.Text ?? "(null)");

        foreach (var validation in imported.ImportReport.ValidationResults)
        {
            output.WriteLine(
                "Validation       : {0} = {1}; expected {2:N2}, actual {3:N2}, diff {4:N2}",
                validation.Code,
                validation.IsValid ? "OK" : "FAILED",
                validation.ExpectedAmount,
                validation.ActualAmount,
                validation.Difference);
        }
    }
}
