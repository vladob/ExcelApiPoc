using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXmlJournalRealFileDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesXmlJournalRealFileDiagnosticTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Import_real_xml_from_environment_variable()
    {
        string? path = Environment.GetEnvironmentVariable(
            "IVES_JOURNAL_XML_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(
                "IVES_JOURNAL_XML_TEST_FILE is not set. Real-file diagnostic was not run.");
            return;
        }

        Assert.True(
            File.Exists(path),
            "IVES_JOURNAL_XML_TEST_FILE does not point to an existing file: " + path);

        IvesJournalParseResult parsed =
            new IvesXmlJournalParser().Parse(path);

        JournalImport imported =
            new IvesJournalImporter().Import(path);

        Assert.NotEmpty(parsed.TransactionRows);
        Assert.NotEmpty(imported.Rows);
        Assert.True(imported.ImportReport.IsValid);
        Assert.Empty(imported.ImportReport.Diagnostics);
        Assert.Equal(parsed.TransactionRows.Count, imported.Rows.Count);

        decimal displayedTotal = parsed.TransactionRows.Sum(
            row => row.Amount.GetValueOrDefault());

        decimal debitTotal = parsed.TransactionRows
            .Where(row => !string.IsNullOrWhiteSpace(row.DebitCompositeAccount))
            .Sum(row => row.Amount.GetValueOrDefault());

        decimal creditTotal = parsed.TransactionRows
            .Where(row => !string.IsNullOrWhiteSpace(row.CreditCompositeAccount))
            .Sum(row => row.Amount.GetValueOrDefault());

        decimal? reportedTotal = parsed.ReportTotalRows.Count == 1 &&
                                 parsed.ReportTotalRows[0].ReportedAmounts.Count == 1
            ? parsed.ReportTotalRows[0].ReportedAmounts[0]
            : null;

        IvesJournalSourceRow first = parsed.TransactionRows.First();
        IvesJournalSourceRow last = parsed.TransactionRows.Last();

        output.WriteLine("IVES real XML diagnostic");
        output.WriteLine("------------------------");
        output.WriteLine("File             : {0}", Path.GetFileName(path));
        output.WriteLine("Technical type   : {0}", imported.TechnicalType);
        output.WriteLine("IČO              : {0}", imported.Ico);
        output.WriteLine("Fiscal year      : {0}", imported.FiscalYear);
        output.WriteLine(
            "Period           : {0:yyyy-MM-dd} .. {1:yyyy-MM-dd}",
            parsed.PeriodStart,
            parsed.PeriodEnd);
        output.WriteLine("Transactions     : {0}", parsed.TransactionRows.Count);
        output.WriteLine("Canonical rows   : {0}", imported.Rows.Count);
        output.WriteLine("Module evidence  : {0}", parsed.ModuleRows.Count);
        output.WriteLine("Report totals    : {0}", parsed.ReportTotalRows.Count);
        output.WriteLine("Displayed total  : {0:N2}", displayedTotal);
        output.WriteLine("Debit total      : {0:N2}", debitTotal);
        output.WriteLine("Credit total     : {0:N2}", creditTotal);
        output.WriteLine(
            "Reported total   : {0}",
            reportedTotal.HasValue
                ? reportedTotal.Value.ToString("N2")
                : "(not exactly one aggregate total)");
        output.WriteLine(
            "Normalized fields: {0}",
            imported.NormalizedTextFieldCount);
        output.WriteLine("SHA-256          : {0}", imported.SourceFileHash);
        output.WriteLine("");
        output.WriteLine(
            "First row        : #{0}, {1:yyyy-MM-dd}, {2}, {3} -> {4}, {5:N2}, {6}",
            first.SequenceNumber,
            first.PostingDate,
            first.DocumentNumber,
            first.DebitCompositeAccount ?? "(null)",
            first.CreditCompositeAccount ?? "(null)",
            first.Amount,
            first.Module);
        output.WriteLine(
            "First location   : {0}",
            first.SourceLocation);
        output.WriteLine(
            "Last row         : #{0}, {1:yyyy-MM-dd}, {2}, {3} -> {4}, {5:N2}, {6}",
            last.SequenceNumber,
            last.PostingDate,
            last.DocumentNumber,
            last.DebitCompositeAccount ?? "(null)",
            last.CreditCompositeAccount ?? "(null)",
            last.Amount,
            last.Module);
        output.WriteLine(
            "Last location    : {0}",
            last.SourceLocation);

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
