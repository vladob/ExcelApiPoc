using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfGeneralLedgerAmountGeometryDiagnosticTests
{
    private const string EnvironmentVariable =
        "IVES_GENERAL_LEDGER_PDF_TEST_FILE";

    private static readonly string[] TargetAccounts =
    {
        "021.1",
        "022.2....04",
        "023.61....6",
        "081.1",
        "211......1",
        "221......3",
        "461.....18.1.1"
    };

    private static readonly ColumnWindow[] Columns =
    {
        new("Opening", 300.0, 375.0, 363.4),
        new("Debit", 375.0, 445.0, 434.7),
        new("Credit", 445.0, 513.0, 502.2),
        new("Closing", 513.0, 580.0, 570.1)
    };

    private readonly ITestOutputHelper output;

    public IvesPdfGeneralLedgerAmountGeometryDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Inspect_problematic_amount_token_geometry()
    {
        string? path =
            Environment.GetEnvironmentVariable(EnvironmentVariable);

        Assert.False(
            string.IsNullOrWhiteSpace(path),
            "Set the " + EnvironmentVariable + " environment variable.");

        Assert.True(
            File.Exists(path),
            "PDF file does not exist: " + path);

        var document = new ITextPdfTokenExtractor().Extract(path);
        IReadOnlyList<BaselineGroup> groups =
            new BaselineGroupBuilder().Build(document);

        output.WriteLine("IVES GL PDF amount geometry diagnostic");
        output.WriteLine("--------------------------------------");
        output.WriteLine("File: " + Path.GetFileName(path));

        foreach (string account in TargetAccounts)
        {
            BaselineGroup[] matches = groups
                .Where(IsAnalyticalSummary)
                .Where(group => CompactText(group).StartsWith(
                    "--" + account,
                    StringComparison.Ordinal))
                .ToArray();

            output.WriteLine("");
            output.WriteLine(
                "Account target '{0}': {1} matching analytical row(s)",
                account,
                matches.Length);

            Assert.NotEmpty(matches);

            foreach (BaselineGroup group in matches.Take(3))
                WriteGeometry(group);
        }
    }

    private void WriteGeometry(BaselineGroup group)
    {
        output.WriteLine(
            "  Page {0}, baseline {1:F3}, left {2:F3}, right {3:F3}",
            group.PageNumber,
            group.Baseline,
            group.Left,
            group.Right);

        output.WriteLine("  Compact: " + CompactText(group));

        foreach (ColumnWindow column in Columns)
        {
            PdfTextToken[] tokens = group.Tokens
                .Where(token =>
                    token.Right >= column.Left &&
                    token.Left <= column.Right)
                .OrderBy(token => token.Left)
                .ThenBy(token => token.Right)
                .ToArray();

            output.WriteLine(
                "    {0} [{1:F1}, {2:F1}], expected right {3:F1}:",
                column.Name,
                column.Left,
                column.Right,
                column.ExpectedRight);

            PdfTextToken? previous = null;
            foreach (PdfTextToken token in tokens)
            {
                double? gap = previous == null
                    ? null
                    : token.Left - previous.Right;

                output.WriteLine(
                    "      [{0:F3}-{1:F3}] gap {2,8} numeric {3,-5} text='{4}'",
                    token.Left,
                    token.Right,
                    gap.HasValue ? gap.Value.ToString("F3") : "-",
                    IsAmountCompatible(token.Text) ? "yes" : "no",
                    Escape(token.Text));

                previous = token;
            }

            WriteCandidateRuns(tokens, column.ExpectedRight, 1.0);
            WriteCandidateRuns(tokens, column.ExpectedRight, 2.0);
            WriteCandidateRuns(tokens, column.ExpectedRight, 3.0);
            WriteCandidateRuns(tokens, column.ExpectedRight, 4.0);
        }
    }

    private void WriteCandidateRuns(
        IReadOnlyList<PdfTextToken> tokens,
        double expectedRight,
        double maximumGap)
    {
        var runs = new List<List<PdfTextToken>>();
        List<PdfTextToken>? current = null;
        PdfTextToken? previous = null;

        foreach (PdfTextToken token in tokens)
        {
            if (!IsAmountCompatible(token.Text))
            {
                current = null;
                previous = null;
                continue;
            }

            double gap = previous == null
                ? double.PositiveInfinity
                : token.Left - previous.Right;

            if (current == null || gap > maximumGap)
            {
                current = new List<PdfTextToken>();
                runs.Add(current);
            }

            current.Add(token);
            previous = token;
        }

        output.WriteLine(
            "      runs gap<={0:F1}: {1}",
            maximumGap,
            runs.Count == 0
                ? "(none)"
                : string.Join(
                    " | ",
                    runs.Select(run =>
                    {
                        string text = string.Concat(
                            run.Select(token => token.Text ?? string.Empty))
                            .Replace("\u00A0", " ");
                        double right = run.Max(token => token.Right);
                        return "'" + text + "' right=" +
                               right.ToString("F3") +
                               " delta=" +
                               Math.Abs(right - expectedRight).ToString("F3");
                    })));
    }

    private static bool IsAnalyticalSummary(BaselineGroup group)
    {
        string compact = CompactText(group);

        return compact.StartsWith("--", StringComparison.Ordinal) &&
               compact.IndexOf("====", StringComparison.Ordinal) < 0;
    }

    private static bool IsAmountCompatible(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        foreach (char character in value)
        {
            if (!char.IsDigit(character) &&
                character != ',' &&
                character != '.' &&
                character != '-' &&
                !char.IsWhiteSpace(character) &&
                character != '\u00A0')
            {
                return false;
            }
        }

        return true;
    }

    private static string CompactText(BaselineGroup group)
    {
        return string.Concat(
            group.Tokens
                .Select(token => token.Text ?? string.Empty)
                .Where(text => !string.IsNullOrWhiteSpace(text)))
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty);
    }

    private static string Escape(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\u00A0", "<NBSP>")
            .Replace("\t", "<TAB>")
            .Replace("\r", "<CR>")
            .Replace("\n", "<LF>");
    }

    private sealed record ColumnWindow(
        string Name,
        double Left,
        double Right,
        double ExpectedRight);
}
