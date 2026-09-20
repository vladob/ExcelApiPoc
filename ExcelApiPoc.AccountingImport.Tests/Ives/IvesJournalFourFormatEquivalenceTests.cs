using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesJournalFourFormatEquivalenceTests
{
    private readonly ITestOutputHelper output;

    public IvesJournalFourFormatEquivalenceTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Real_xml_csv_xlsx_and_pdf_are_semantically_equivalent()
    {
        string? xmlPath = PathFromEnvironment(
            "IVES_JOURNAL_XML_TEST_FILE");
        string? csvPath = PathFromEnvironment(
            "IVES_JOURNAL_CSV_TEST_FILE");
        string? xlsxPath = PathFromEnvironment(
            "IVES_JOURNAL_XLSX_TEST_FILE");
        string? pdfPath = PathFromEnvironment(
            "IVES_JOURNAL_PDF_TEST_FILE");

        if (xmlPath == null ||
            csvPath == null ||
            xlsxPath == null ||
            pdfPath == null)
        {
            output.WriteLine(
                "All four IVES journal real-file environment variables " +
                "must be set for the equivalence test.");
            return;
        }

        var sources = new[]
        {
            Load(
                "XML",
                xmlPath,
                path => new IvesXmlJournalParser().Parse(path)),
            Load(
                "CSV",
                csvPath,
                path => new IvesCsvJournalParser().Parse(path)),
            Load(
                "XLSX",
                xlsxPath,
                path => new IvesXlsxJournalParser().Parse(path)),
            Load(
                "PDF",
                pdfPath,
                path => new IvesPdfJournalParser().Parse(path))
        };

        FormatSnapshot expected = sources[0];

        foreach (FormatSnapshot actual in sources.Skip(1))
        {
            Assert.Equal(expected.Parsed.Ico, actual.Parsed.Ico);
            Assert.Equal(
                expected.Parsed.FiscalYear,
                actual.Parsed.FiscalYear);
            Assert.Equal(
                expected.Parsed.PeriodStart,
                actual.Parsed.PeriodStart);
            Assert.Equal(
                expected.Parsed.PeriodEnd,
                actual.Parsed.PeriodEnd);
            Assert.Equal(
                expected.Imported.Ico,
                actual.Imported.Ico);
            Assert.Equal(
                expected.Imported.FiscalYear,
                actual.Imported.FiscalYear);
            Assert.Equal(
                expected.Imported.Rows.Count,
                actual.Imported.Rows.Count);
            Assert.Equal(
                expected.Parsed.TransactionRows.Count,
                actual.Parsed.TransactionRows.Count);

            Assert.Equal(
                ReportTotal(expected.Parsed),
                ReportTotal(actual.Parsed));

            for (int index = 0;
                 index < expected.Imported.Rows.Count;
                 index++)
            {
                JournalRow expectedRow =
                    expected.Imported.Rows[index];
                JournalRow actualRow =
                    actual.Imported.Rows[index];

                Assert.Equal(
                    expectedRow.SequenceNumber,
                    actualRow.SequenceNumber);
                Assert.Equal(
                    expectedRow.PostingDate,
                    actualRow.PostingDate);
                Assert.Equal(
                    expectedRow.DocumentNumber,
                    actualRow.DocumentNumber);
                Assert.Equal(
                    expectedRow.DebitAccount,
                    actualRow.DebitAccount);
                Assert.Equal(
                    expectedRow.CreditAccount,
                    actualRow.CreditAccount);
                Assert.Equal(
                    expectedRow.DebitAmount,
                    actualRow.DebitAmount);
                Assert.Equal(
                    expectedRow.CreditAmount,
                    actualRow.CreditAmount);
                Assert.Equal(
                    expectedRow.Description,
                    actualRow.Description);

                Assert.Equal(
                    expected.Parsed.TransactionRows[index].Module,
                    actual.Parsed.TransactionRows[index].Module);
            }
        }

        output.WriteLine(
            "IVES four-format semantic equivalence verified.");
        output.WriteLine(
            "Transactions     : {0}",
            expected.Imported.Rows.Count);
        output.WriteLine(
            "IČO              : {0}",
            expected.Imported.Ico);
        output.WriteLine(
            "Fiscal year      : {0}",
            expected.Imported.FiscalYear);
        output.WriteLine(
            "Reported total   : {0:N2}",
            ReportTotal(expected.Parsed));

        foreach (FormatSnapshot source in sources)
        {
            output.WriteLine(
                "{0,-4}: {1} rows, SHA-256 {2}",
                source.Name,
                source.Imported.Rows.Count,
                source.Imported.SourceFileHash);
        }
    }

    private static FormatSnapshot Load(
        string name,
        string path,
        Func<string, IvesJournalParseResult> parse)
    {
        IvesJournalParseResult parsed = parse(path);
        JournalImport imported =
            new IvesJournalImporter().Import(path);

        Assert.True(imported.ImportReport.IsValid);
        Assert.Empty(imported.ImportReport.Diagnostics);

        return new FormatSnapshot(
            name,
            parsed,
            imported);
    }

    private static decimal ReportTotal(
        IvesJournalParseResult parsed)
    {
        IvesJournalSourceRow total =
            Assert.Single(parsed.ReportTotalRows);
        return Assert.Single(total.ReportedAmounts);
    }

    private string? PathFromEnvironment(
        string variableName)
    {
        string? path =
            Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(
                variableName + " is not set.");
            return null;
        }

        Assert.True(
            File.Exists(path),
            variableName +
            " does not point to an existing file: " +
            path);

        return path;
    }

    private sealed class FormatSnapshot
    {
        public FormatSnapshot(
            string name,
            IvesJournalParseResult parsed,
            JournalImport imported)
        {
            Name = name;
            Parsed = parsed;
            Imported = imported;
        }

        public string Name { get; }
        public IvesJournalParseResult Parsed { get; }
        public JournalImport Imported { get; }
    }
}
