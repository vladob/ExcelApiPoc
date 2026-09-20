using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesGeneralLedgerFourFormatEquivalenceTests
{
    private readonly ITestOutputHelper output;

    public IvesGeneralLedgerFourFormatEquivalenceTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Real_xml_xlsx_csv_and_pdf_are_semantically_equivalent()
    {
        string? xmlPath = PathFromEnvironment(
            "IVES_GENERAL_LEDGER_XML_TEST_FILE");
        string? xlsxPath = PathFromEnvironment(
            "IVES_GENERAL_LEDGER_XLSX_TEST_FILE");
        string? csvPath = PathFromEnvironment(
            "IVES_GENERAL_LEDGER_CSV_TEST_FILE");
        string? pdfPath = PathFromEnvironment(
            "IVES_GENERAL_LEDGER_PDF_TEST_FILE");

        if (xmlPath == null ||
            xlsxPath == null ||
            csvPath == null ||
            pdfPath == null)
        {
            output.WriteLine(
                "All four IVES general-ledger real-file environment " +
                "variables must be set for the equivalence test.");
            return;
        }

        var sources = new[]
        {
            Load(
                "XML",
                xmlPath,
                path => new IvesXmlGeneralLedgerParser().Parse(path)),
            Load(
                "XLSX",
                xlsxPath,
                path => new IvesXlsxGeneralLedgerParser().Parse(path)),
            Load(
                "CSV",
                csvPath,
                path => new IvesCsvGeneralLedgerParser().Parse(path)),
            Load(
                "PDF",
                pdfPath,
                path => new IvesPdfGeneralLedgerParser().Parse(path))
        };

        Snapshot expected = sources[0];
        var mismatches = new List<string>();

        foreach (Snapshot actual in sources.Skip(1))
            Compare(expected, actual, mismatches);

        output.WriteLine(
            "IVES GL four-format equivalence comparison completed.");
        output.WriteLine("Activities          : " + expected.Parsed.Activities.Count);
        output.WriteLine(
            "Documents           : " +
            expected.Parsed.Activities.Sum(activity => activity.DocumentRows.Count));
        output.WriteLine(
            "Analytical accounts : " +
            expected.Parsed.Activities.Sum(activity => activity.AccountRows.Count));
        output.WriteLine(
            "Synthetic summaries : " +
            expected.Parsed.Activities.Sum(
                activity => activity.SyntheticSummaryRows.Count));
        output.WriteLine(
            "Report totals       : " +
            expected.Parsed.Activities.Sum(
                activity => activity.ReportTotalRows.Count));
        output.WriteLine("Mismatches          : " + mismatches.Count);

        if (mismatches.Count > 0)
        {
            output.WriteLine("Mismatch categories :");

            foreach (var category in mismatches
                .GroupBy(MismatchCategory)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal))
            {
                output.WriteLine(
                    "  {0,-44} {1,5}",
                    category.Key,
                    category.Count());
            }

            output.WriteLine("Mismatch samples    :");
        }

        foreach (string mismatch in mismatches.Take(40))
            output.WriteLine("  " + mismatch);

        Assert.True(
            mismatches.Count == 0,
            "IVES GL semantic equivalence failed with " +
            mismatches.Count +
            " mismatch(es). See test output for samples.");
    }

    private static Snapshot Load(
        string name,
        string path,
        Func<string, IvesGeneralLedgerParseResult> parse)
    {
        IvesGeneralLedgerParseResult parsed = parse(path);
        ImportReport validation =
            new IvesGeneralLedgerValidator().Validate(parsed);

        Assert.True(
            validation.IsValid,
            name + " validation failed: " +
            string.Join(
                Environment.NewLine,
                validation.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Code + ": " + diagnostic.Message)));

        Assert.Empty(validation.Diagnostics);

        return new Snapshot(name, parsed);
    }

    private static void Compare(
        Snapshot expected,
        Snapshot actual,
        ICollection<string> mismatches)
    {
        Equal(
            expected.Parsed.Ico,
            actual.Parsed.Ico,
            actual.Name + ".Ico",
            mismatches);
        Equal(
            expected.Parsed.FiscalYear,
            actual.Parsed.FiscalYear,
            actual.Name + ".FiscalYear",
            mismatches);
        Equal(
            expected.Parsed.PeriodStart,
            actual.Parsed.PeriodStart,
            actual.Name + ".PeriodStart",
            mismatches);
        Equal(
            expected.Parsed.PeriodEnd,
            actual.Parsed.PeriodEnd,
            actual.Name + ".PeriodEnd",
            mismatches);
        Equal(
            expected.Parsed.Activities.Count,
            actual.Parsed.Activities.Count,
            actual.Name + ".Activities.Count",
            mismatches);

        int activityCount = Math.Min(
            expected.Parsed.Activities.Count,
            actual.Parsed.Activities.Count);

        for (int activityIndex = 0;
             activityIndex < activityCount;
             activityIndex++)
        {
            IvesGeneralLedgerActivity expectedActivity =
                expected.Parsed.Activities[activityIndex];
            IvesGeneralLedgerActivity actualActivity =
                actual.Parsed.Activities[activityIndex];

            string scope =
                actual.Name + ".Activity[" + activityIndex + "]";

            Equal(
                NormalizeText(expectedActivity.Name),
                NormalizeText(actualActivity.Name),
                scope + ".Name",
                mismatches);
            Equal(
                NormalizeText(expectedActivity.Currency),
                NormalizeText(actualActivity.Currency),
                scope + ".Currency",
                mismatches);

            CompareRows(
                expectedActivity.DocumentRows,
                actualActivity.DocumentRows,
                scope + ".Documents",
                CompareDocument,
                mismatches);

            CompareRows(
                expectedActivity.AccountRows,
                actualActivity.AccountRows,
                scope + ".Accounts",
                CompareAccount,
                mismatches);

            CompareRows(
                expectedActivity.SyntheticSummaryRows,
                actualActivity.SyntheticSummaryRows,
                scope + ".Synthetic",
                CompareSynthetic,
                mismatches);

            CompareRows(
                expectedActivity.ReportTotalRows,
                actualActivity.ReportTotalRows,
                scope + ".Totals",
                CompareTotal,
                mismatches);
        }
    }

    private static void CompareRows(
        IReadOnlyList<IvesGeneralLedgerSourceRow> expected,
        IReadOnlyList<IvesGeneralLedgerSourceRow> actual,
        string scope,
        Action<
            IvesGeneralLedgerSourceRow,
            IvesGeneralLedgerSourceRow,
            string,
            ICollection<string>> compare,
        ICollection<string> mismatches)
    {
        Equal(
            expected.Count,
            actual.Count,
            scope + ".Count",
            mismatches);

        int count = Math.Min(expected.Count, actual.Count);

        for (int index = 0; index < count; index++)
        {
            compare(
                expected[index],
                actual[index],
                scope + "[" + index + "]",
                mismatches);
        }
    }

    private static void CompareDocument(
        IvesGeneralLedgerSourceRow expected,
        IvesGeneralLedgerSourceRow actual,
        string scope,
        ICollection<string> mismatches)
    {
        Equal(expected.SequenceNumber, actual.SequenceNumber,
            scope + ".SequenceNumber", mismatches);
        Equal(expected.DocumentDate, actual.DocumentDate,
            scope + ".DocumentDate", mismatches);
        Equal(NormalizeText(expected.DocumentNumber),
            NormalizeText(actual.DocumentNumber),
            scope + ".DocumentNumber", mismatches);
        Equal(NormalizeAccount(expected.AccountCode),
            NormalizeAccount(actual.AccountCode),
            scope + ".AccountCode", mismatches);
        Equal(NormalizeText(expected.Text),
            NormalizeText(actual.Text),
            scope + ".Text", mismatches);
        Equal(Value(expected.DebitTurnover),
            Value(actual.DebitTurnover),
            scope + ".Debit", mismatches);
        Equal(Value(expected.CreditTurnover),
            Value(actual.CreditTurnover),
            scope + ".Credit", mismatches);
    }

    private static void CompareAccount(
        IvesGeneralLedgerSourceRow expected,
        IvesGeneralLedgerSourceRow actual,
        string scope,
        ICollection<string> mismatches)
    {
        Equal(expected.SequenceNumber, actual.SequenceNumber,
            scope + ".SequenceNumber", mismatches);
        Equal(NormalizeAccount(expected.AccountCode),
            NormalizeAccount(actual.AccountCode),
            scope + ".AccountCode", mismatches);
        Equal(NormalizeText(expected.Text),
            NormalizeText(actual.Text),
            scope + ".Text", mismatches);
        Equal(Value(expected.OpeningBalance),
            Value(actual.OpeningBalance),
            scope + ".Opening", mismatches);
        Equal(Value(expected.DebitTurnover),
            Value(actual.DebitTurnover),
            scope + ".Debit", mismatches);
        Equal(Value(expected.CreditTurnover),
            Value(actual.CreditTurnover),
            scope + ".Credit", mismatches);
        Equal(Value(expected.ClosingBalance),
            Value(actual.ClosingBalance),
            scope + ".Closing", mismatches);
    }

    private static void CompareSynthetic(
        IvesGeneralLedgerSourceRow expected,
        IvesGeneralLedgerSourceRow actual,
        string scope,
        ICollection<string> mismatches)
    {
        Equal(expected.SequenceNumber, actual.SequenceNumber,
            scope + ".SequenceNumber", mismatches);
        Equal(NormalizeAccount(expected.AccountCode),
            NormalizeAccount(actual.AccountCode),
            scope + ".AccountCode", mismatches);
        Equal(Value(expected.OpeningBalance),
            Value(actual.OpeningBalance),
            scope + ".Opening", mismatches);
        Equal(Value(expected.DebitTurnover),
            Value(actual.DebitTurnover),
            scope + ".Debit", mismatches);
        Equal(Value(expected.CreditTurnover),
            Value(actual.CreditTurnover),
            scope + ".Credit", mismatches);
        Equal(Value(expected.ClosingBalance),
            Value(actual.ClosingBalance),
            scope + ".Closing", mismatches);
    }

    private static void CompareTotal(
        IvesGeneralLedgerSourceRow expected,
        IvesGeneralLedgerSourceRow actual,
        string scope,
        ICollection<string> mismatches)
    {
        Equal(expected.SequenceNumber, actual.SequenceNumber,
            scope + ".SequenceNumber", mismatches);
        Equal(Value(expected.OpeningBalance),
            Value(actual.OpeningBalance),
            scope + ".Opening", mismatches);
        Equal(Value(expected.DebitTurnover),
            Value(actual.DebitTurnover),
            scope + ".Debit", mismatches);
        Equal(Value(expected.CreditTurnover),
            Value(actual.CreditTurnover),
            scope + ".Credit", mismatches);
        Equal(Value(expected.ClosingBalance),
            Value(actual.ClosingBalance),
            scope + ".Closing", mismatches);
    }

    private static decimal Value(decimal? value) =>
        value.GetValueOrDefault();

    private static string NormalizeAccount(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Concat(
            value.Where(character =>
                !char.IsWhiteSpace(character) &&
                character != '\u00A0'));
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new System.Text.StringBuilder();
        bool pendingSpace = false;

        foreach (char character in value.Trim())
        {
            if (char.IsWhiteSpace(character) ||
                character == '\u00A0')
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string MismatchCategory(string mismatch)
    {
        int colon = mismatch.IndexOf(':');
        string path = colon < 0
            ? mismatch
            : mismatch.Substring(0, colon);

        var builder = new System.Text.StringBuilder(path.Length);
        bool insideIndex = false;

        foreach (char character in path)
        {
            if (character == '[')
            {
                insideIndex = true;
                builder.Append("[]");
                continue;
            }

            if (insideIndex)
            {
                if (character == ']')
                    insideIndex = false;

                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static void Equal<T>(
        T expected,
        T actual,
        string scope,
        ICollection<string> mismatches)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual))
            return;

        mismatches.Add(
            scope +
            ": expected '" + expected +
            "', actual '" + actual + "'.");
    }

    private string? PathFromEnvironment(string variableName)
    {
        string? path =
            Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(variableName + " is not set.");
            return null;
        }

        Assert.True(
            File.Exists(path),
            variableName +
            " does not point to an existing file: " +
            path);

        return path;
    }

    private sealed class Snapshot
    {
        public Snapshot(
            string name,
            IvesGeneralLedgerParseResult parsed)
        {
            Name = name;
            Parsed = parsed;
        }

        public string Name { get; }
        public IvesGeneralLedgerParseResult Parsed { get; }
    }
}
