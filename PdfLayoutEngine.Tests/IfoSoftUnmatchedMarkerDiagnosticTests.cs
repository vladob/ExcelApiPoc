using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Recognition;
using Xunit.Abstractions;

namespace PdfLayoutEngine.Tests;

/// <summary>
/// Opt-in diagnostics for reports whose visible GMX marker already has a layout definition.
/// Set IFOSOFT_FOCUSED_TEST_DIR to the folder containing copies of these PDFs.
/// </summary>
public sealed class IfoSoftUnmatchedMarkerDiagnosticTests(ITestOutputHelper output)
{
    public static IEnumerable<object[]> Samples()
    {
        yield return new object[] { "HL_KNIHA_00325015_2023_2024.pdf", "ifosoft-general-ledger-hlknia4b" };
        yield return new object[] { "HL_KNIHA_00325015_2023_a.pdf", "ifosoft-general-ledger-hlknia4b" };
        yield return new object[] { "HL_KNIHA_00325015_2023_b.pdf", "ifosoft-general-ledger-hlknia4b" };
        yield return new object[] { "HL_KNIHA_00325015_2024.pdf", "ifosoft-general-ledger-hlknia4b" };
        yield return new object[] { "HL_KNIHA_00325015_2025.pdf", "ifosoft-general-ledger-hlknia4b" };
        yield return new object[] { "HL_KNIHA_00325287_2022_MD D.pdf", "ifosoft-general-ledger-hlknia4" };
        yield return new object[] { "HL_KNIHA_00325899_2024_b.pdf", "ifosoft-general-ledger-hlknia4" };
    }

    [Theory]
    [MemberData(nameof(Samples))]
    public void Explain_unmatched_marker(string fileName, string expectedLayout)
    {
        var folder = Environment.GetEnvironmentVariable("IFOSOFT_FOCUSED_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder),
            "Set IFOSOFT_FOCUSED_TEST_DIR to the folder containing the 31 unmatched PDFs.");
        var path = Path.Combine(folder!, fileName);
        Assert.True(File.Exists(path), "Missing sample: " + path);

        var document = new ITextPdfTokenExtractor().Extract(path);
        var layoutFolder = Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft");
        var layouts = Directory.GetFiles(layoutFolder, "*.json")
            .Select(file => new LayoutDefinitionLoader().Load(File.ReadAllText(file)).Definition!)
            .ToArray();
        var expected = Assert.Single(layouts, x => x.Id == expectedLayout);
        var matches = new LayoutSignatureDetector().Detect(document, layouts);
        output.WriteLine($"{fileName}: {document.Pages.Count} pages, {document.Tokens.Count} tokens");
        output.WriteLine("Detected: " + (matches.Count == 0 ? "(none)" : string.Join(", ", matches.Select(x => x.Id))));

        var matcher = new TokenRuleMatcher();
        foreach (var rule in expected.Rules)
        {
            var result = matcher.Match(rule, document.Tokens, expected.Defaults);
            output.WriteLine($"Rule {rule.Id}: {result.Status} ({result.Matches.Count} matching tokens)");
            foreach (var evidence in result.Matches.Take(3))
                foreach (var token in evidence.SourceTokens)
                    output.WriteLine("  Match: " + Describe(token));
        }

        // Print only first-page header tokens. The renderer can split a visible
        // title or GMX marker into several text operations that never match one rule.
        output.WriteLine("First-page header tokens:");
        foreach (var token in document.Pages[0].Tokens.Where(x =>
                     x.Text.IndexOf("HLAV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.Text.IndexOf("KNIHA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.Text.IndexOf("GMX", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.Text.IndexOf("HLKNIA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.Text == "_").Take(25))
            output.WriteLine("  " + Describe(token));
        output.WriteLine("First 45 tokens: " + string.Join(" | ",
            document.Pages[0].Tokens.Take(45).Select(Describe)));
        Assert.Equal(expectedLayout, Assert.Single(matches).Id);
    }

    private static string Describe(PdfLayoutEngine.Models.PdfTextToken token) =>
        $"'{token.Text.Replace(' ', '·')}'@{token.Left:F1}-{token.Right:F1},y={token.Baseline:F1}";
}
