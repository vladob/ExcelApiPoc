using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.IText.Extraction;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfGeneralLedgerStructureDiagnosticTests
{
    private const string EnvironmentVariable =
        "IVES_GENERAL_LEDGER_PDF_TEST_FILE";

    private readonly ITestOutputHelper output;

    public IvesPdfGeneralLedgerStructureDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Inspect_real_pdf_structure_and_geometry()
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

        output.WriteLine("IVES GL PDF structure diagnostic");
        output.WriteLine("--------------------------------");
        output.WriteLine("File            : " + Path.GetFileName(path));
        output.WriteLine("Pages           : " + document.Pages.Count);
        output.WriteLine("Tokens          : " + document.Tokens.Count);
        output.WriteLine("Baseline groups : " + groups.Count);

        WritePageSummary(document, groups);
        WriteGroups(
            "First page groups",
            groups.Where(group => group.PageNumber == 1),
            80);

        WriteGroups(
            "Transaction-like samples",
            groups.Where(IsTransactionLike),
            16);

        WriteGroups(
            "Analytical-summary-like samples",
            groups.Where(IsAnalyticalSummaryLike),
            16);

        WriteGroups(
            "Synthetic SU samples",
            groups.Where(group =>
                group.Text.IndexOf("SU", StringComparison.OrdinalIgnoreCase) >= 0 ||
                group.Text.IndexOf("====", StringComparison.Ordinal) >= 0),
            16);

        WriteGroups(
            "Activity/title samples",
            groups.Where(group =>
                group.Text.IndexOf(
                    "Hlavná činnosť",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                group.Text.IndexOf(
                    "Stravovanie",
                    StringComparison.OrdinalIgnoreCase) >= 0),
            16);

        WriteGroups(
            "Celkom samples",
            groups.Where(group =>
                Regex.IsMatch(
                    group.Text,
                    @"C\s*e\s*l\s*k\s*o\s*m",
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant)),
            16);

        WritePageBoundarySamples(groups, document.Pages.Count);

        Assert.NotEmpty(document.Pages);
        Assert.All(
            document.Pages,
            page => Assert.NotEmpty(page.Tokens));

        Assert.Contains(groups, IsTransactionLike);
        Assert.Contains(
            groups,
            group => group.Text.IndexOf(
                "Hlavná činnosť",
                StringComparison.OrdinalIgnoreCase) >= 0);
        Assert.Contains(
            groups,
            group => group.Text.IndexOf(
                "Stravovanie",
                StringComparison.OrdinalIgnoreCase) >= 0);
        Assert.Contains(
            groups,
            group => Regex.IsMatch(
                group.Text,
                @"C\s*e\s*l\s*k\s*o\s*m",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant));
    }

    private void WritePageSummary(
        PdfLayoutEngine.Models.PdfDocument document,
        IReadOnlyList<BaselineGroup> groups)
    {
        output.WriteLine("");
        output.WriteLine("Page summary:");

        foreach (var page in document.Pages.Take(6)
                     .Concat(document.Pages.TakeLast(3)))
        {
            int groupCount = groups.Count(
                group => group.PageNumber == page.PageNumber);

            output.WriteLine(
                "  Page {0}: {1} tokens, {2} groups",
                page.PageNumber,
                page.Tokens.Count,
                groupCount);
        }
    }

    private void WriteGroups(
        string heading,
        IEnumerable<BaselineGroup> groups,
        int take)
    {
        output.WriteLine("");
        output.WriteLine(heading + ":");

        foreach (BaselineGroup group in groups.Take(take))
        {
            WriteGroup(group);
        }
    }

    private void WritePageBoundarySamples(
        IReadOnlyList<BaselineGroup> groups,
        int pageCount)
    {
        output.WriteLine("");
        output.WriteLine("Page-boundary samples:");

        int[] pages = new[] { 1, 2, 3, pageCount - 2, pageCount - 1, pageCount }
            .Where(page => page >= 1 && page <= pageCount)
            .Distinct()
            .OrderBy(page => page)
            .ToArray();

        foreach (int page in pages)
        {
            BaselineGroup[] pageGroups = groups
                .Where(group => group.PageNumber == page)
                .OrderByDescending(group => group.Baseline)
                .ToArray();

            if (pageGroups.Length == 0)
                continue;

            output.WriteLine("  Page " + page + " first groups:");
            foreach (BaselineGroup group in pageGroups.Take(5))
                WriteGroup(group, "    ");

            output.WriteLine("  Page " + page + " last groups:");
            foreach (BaselineGroup group in pageGroups.TakeLast(5))
                WriteGroup(group, "    ");
        }
    }

    private void WriteGroup(
        BaselineGroup group,
        string indent = "  ")
    {
        output.WriteLine(
            indent +
            "page {0}, baseline {1:F3}, left {2:F3}, right {3:F3}, tokens {4}: {5}",
            group.PageNumber,
            group.Baseline,
            group.Left,
            group.Right,
            group.Tokens.Count,
            group.Text);

        foreach (var token in group.Tokens)
        {
            output.WriteLine(
                indent +
                "  [{0:F3}-{1:F3}] {2}",
                token.Left,
                token.Right,
                token.Text);
        }
    }

    private static bool IsTransactionLike(BaselineGroup group)
    {
        return Regex.IsMatch(
            group.Text,
            @"^\s*\d{2}\.\d{2}\.",
            RegexOptions.CultureInvariant);
    }

    private static bool IsAnalyticalSummaryLike(
        BaselineGroup group)
    {
        return Regex.IsMatch(
                   group.Text,
                   @"^\s*-\s+\d{3}\.",
                   RegexOptions.CultureInvariant) &&
               group.Text.IndexOf(
                   "====",
                   StringComparison.Ordinal) < 0;
    }
}
