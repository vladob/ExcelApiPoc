using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using Xunit.Abstractions;

namespace PdfLayoutEngine.Tests;

/// <summary>
/// Opt-in corpus checks. Sample files remain outside source control.
/// Set IFOSOFT_LAYOUT_TEST_DIR to the folder containing the supplied samples.
/// </summary>
public sealed class IfoSoftRealLayoutDiagnosticTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    public static IEnumerable<object[]> Samples()
    {
        yield return new object[] { "SampleOf_DENNIK.GMX_00323748_2023_07.pdf", "ifosoft-journal-dennik", 9 };
        yield return new object[] { "SampleOf_DENNIK1.GMX_00323586_2024.oxps", "ifosoft-journal-dennik1", 93 };
        yield return new object[] { "SampleOf_DENNIK1.GMX_00323756_2022.pdf", "ifosoft-journal-dennik1", 95 };
        yield return new object[] { "SampleOf_HLKNIA4.GMX_00323586_2022_2021.pdf", "ifosoft-general-ledger-hlknia4", 9 };
        yield return new object[] { "SampleOf_HLKNIA4.GMX_00323586_2024.oxps", "ifosoft-general-ledger-hlknia4", 9 };
        yield return new object[] { "SampleOf_HLKNIA4B.GMX_00323233_2025.pdf", "ifosoft-general-ledger-hlknia4b", 17 };
        yield return new object[] { "SampleOf_HLKNIA4D.GMX_00322792_2022.oxps", "ifosoft-general-ledger-hlknia4d", 1 };
        yield return new object[] { "SampleOf_OBR_UCET.GMX _00323756_2023_b.pdf", "ifosoft-account-movements-obr-ucet", 171 };
        yield return new object[] { "SampleOf_OBR_UCET.GMX_00322792_2024.oxps", "ifosoft-account-movements-obr-ucet", 93 };
        yield return new object[] { "SampleOf_PREDVAS3.GMX_00322792_2022_2.oxps", "ifosoft-general-ledger-predvas3", 5 };
        yield return new object[] { "SampleOf_PREDVAS3.GMX_00323756_2022.pdf", "ifosoft-general-ledger-predvas3", 6 };
        yield return new object[] { "UCT_ROZVRH_00323756_2023.pdf", "ifosoft-accounting-framework", 13 };
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public void Extracts_every_page_and_detects_unique_layout(
        string fileName, string expectedLayout, int expectedPages)
    {
        var folder = Environment.GetEnvironmentVariable("IFOSOFT_LAYOUT_TEST_DIR");
        Assert.False(string.IsNullOrWhiteSpace(folder),
            "Set IFOSOFT_LAYOUT_TEST_DIR to the folder containing the IfoSoft PDF/OXPS samples.");
        var path = Path.Combine(folder!, fileName);
        Assert.True(File.Exists(path), "Missing sample: " + path);

        PdfDocument document = Path.GetExtension(path).Equals(".oxps", StringComparison.OrdinalIgnoreCase)
            ? new OpenXpsTokenExtractor().Extract(path)
            : new ITextPdfTokenExtractor().Extract(path);

        _output.WriteLine($"{fileName}: {document.Pages.Count} pages, {document.Tokens.Count} tokens");
        Assert.Equal(expectedPages, document.Pages.Count);
        Assert.All(document.Pages, page => Assert.NotEmpty(page.Tokens));

        var layoutFolder = Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft");
        var layouts = Directory.GetFiles(layoutFolder, "*.json")
            .Select(file => new LayoutDefinitionLoader().Load(File.ReadAllText(file)).Definition!)
            .ToArray();
        var matches = new LayoutSignatureDetector().Detect(document, layouts);
        _output.WriteLine("Detected: " + string.Join(", ", matches.Select(x => x.Id)));
        if (matches.Count != 1)
        {
            _output.WriteLine("First-page tokens: " + string.Join(" | ",
                document.Pages[0].Tokens.Take(40).Select(x => x.Text)));
            var expected = Assert.Single(layouts.Where(x => x.Id == expectedLayout));
            var matcher = new TokenRuleMatcher();
            foreach (var rule in expected.Rules)
                _output.WriteLine($"Rule {rule.Id}: {matcher.Match(rule, document.Tokens, expected.Defaults).Status}");
            _output.WriteLine("First-page glyph geometry: " + string.Join(" | ",
                document.Pages[0].Tokens.Take(110).Select(x =>
                    $"{x.Text.Replace(' ', '·')}@{x.Left:F1}-{x.Right:F1},y={x.Baseline:F1}")));
            var marker = document.Pages[0].Tokens
                .Select((token, index) => (token, index))
                .FirstOrDefault(x => x.token.Text == "_");
            if (marker.token != null)
                _output.WriteLine("Marker-area glyphs: " + string.Join(" | ",
                    document.Pages[0].Tokens.Skip(marker.index).Take(25).Select(x =>
                        $"{x.Text.Replace(' ', '·')}@{x.Left:F1}-{x.Right:F1},y={x.Baseline:F1}")));
        }
        Assert.Equal(expectedLayout, Assert.Single(matches).Id);
    }
}
