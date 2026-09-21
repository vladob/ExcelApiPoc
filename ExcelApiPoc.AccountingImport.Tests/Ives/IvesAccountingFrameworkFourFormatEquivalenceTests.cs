using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesAccountingFrameworkFourFormatEquivalenceTests
{
    private readonly ITestOutputHelper output;

    public IvesAccountingFrameworkFourFormatEquivalenceTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Real_xml_xlsx_csv_and_pdf_are_semantically_equivalent()
    {
        string? xmlPath = PathFromEnvironment(
            "IVES_ACCOUNTING_FRAMEWORK_XML_TEST_FILE");
        string? xlsxPath = PathFromEnvironment(
            "IVES_ACCOUNTING_FRAMEWORK_XLSX_TEST_FILE");
        string? csvPath = PathFromEnvironment(
            "IVES_ACCOUNTING_FRAMEWORK_CSV_TEST_FILE");
        string? pdfPath = PathFromEnvironment(
            "IVES_ACCOUNTING_FRAMEWORK_PDF_TEST_FILE");

        if (xmlPath == null ||
            xlsxPath == null ||
            csvPath == null ||
            pdfPath == null)
        {
            output.WriteLine(
                "All four IVES accounting-framework real-file " +
                "environment variables must be set for the equivalence test.");
            return;
        }

        var sources = new[]
        {
            new Snapshot(
                "XML",
                new IvesXmlAccountingFrameworkParser().Parse(xmlPath)),
            new Snapshot(
                "XLSX",
                new IvesXlsxAccountingFrameworkParser().Parse(xlsxPath)),
            new Snapshot(
                "CSV",
                new IvesCsvAccountingFrameworkParser().Parse(csvPath)),
            new Snapshot(
                "PDF",
                new IvesPdfAccountingFrameworkParser().Parse(pdfPath))
        };

        Snapshot expected = sources[0];
        var mismatches = new List<string>();

        foreach (Snapshot actual in sources.Skip(1))
            Compare(expected, actual, mismatches);

        output.WriteLine(
            "IVES AF four-format equivalence comparison completed.");
        output.WriteLine(
            "Rows       : " + expected.Parsed.Rows.Count);
        output.WriteLine(
            "Mismatches : " + mismatches.Count);
        output.WriteLine(
            "PDF descriptions: excluded from equivalence because " +
            "the report may wrap or clip account names.");

        foreach (string mismatch in mismatches.Take(40))
            output.WriteLine("  " + mismatch);

        Assert.True(
            mismatches.Count == 0,
            "IVES AF semantic equivalence failed with " +
            mismatches.Count +
            " mismatch(es). See test output for samples.");
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
            expected.Parsed.Rows.Count,
            actual.Parsed.Rows.Count,
            actual.Name + ".Rows.Count",
            mismatches);

        int count = Math.Min(
            expected.Parsed.Rows.Count,
            actual.Parsed.Rows.Count);

        bool compareDescriptions =
            !string.Equals(
                actual.Name,
                "PDF",
                StringComparison.Ordinal);

        for (int index = 0; index < count; index++)
        {
            IvesAccountingFrameworkSourceRow expectedRow =
                expected.Parsed.Rows[index];
            IvesAccountingFrameworkSourceRow actualRow =
                actual.Parsed.Rows[index];

            string scope =
                actual.Name + ".Rows[" + index + "]";

            Equal(
                expectedRow.SequenceNumber,
                actualRow.SequenceNumber,
                scope + ".SequenceNumber",
                mismatches);
            Equal(
                NormalizeAccount(expectedRow.SourceAccountCode),
                NormalizeAccount(actualRow.SourceAccountCode),
                scope + ".SourceAccountCode",
                mismatches);
            Equal(
                expectedRow.AccountCode,
                actualRow.AccountCode,
                scope + ".AccountCode",
                mismatches);

            if (compareDescriptions)
            {
                Equal(
                    NormalizeText(expectedRow.AccountName),
                    NormalizeText(actualRow.AccountName),
                    scope + ".AccountName",
                    mismatches);
            }

            Equal(
                NormalizeText(expectedRow.ActivityCode),
                NormalizeText(actualRow.ActivityCode),
                scope + ".ActivityCode",
                mismatches);
            Equal(
                NormalizeText(expectedRow.Type),
                NormalizeText(actualRow.Type),
                scope + ".Type",
                mismatches);
            Equal(
                NormalizeText(expectedRow.PsFlag),
                NormalizeText(actualRow.PsFlag),
                scope + ".PsFlag",
                mismatches);
            Equal(
                NormalizeText(expectedRow.BuFlag),
                NormalizeText(actualRow.BuFlag),
                scope + ".BuFlag",
                mismatches);
            Equal(
                NormalizeText(expectedRow.PlFlag),
                NormalizeText(actualRow.PlFlag),
                scope + ".PlFlag",
                mismatches);
            Equal(
                NormalizeText(expectedRow.RuFlag),
                NormalizeText(actualRow.RuFlag),
                scope + ".RuFlag",
                mismatches);
            Equal(
                NormalizeText(expectedRow.Currency),
                NormalizeText(actualRow.Currency),
                scope + ".Currency",
                mismatches);
            Equal(
                expectedRow.ValidFrom,
                actualRow.ValidFrom,
                scope + ".ValidFrom",
                mismatches);
            Equal(
                expectedRow.ValidTo,
                actualRow.ValidTo,
                scope + ".ValidTo",
                mismatches);
        }
    }

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
            IvesAccountingFrameworkParseResult parsed)
        {
            Name = name;
            Parsed = parsed;
        }

        public string Name { get; }
        public IvesAccountingFrameworkParseResult Parsed { get; }
    }
}
