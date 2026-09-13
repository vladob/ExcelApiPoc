using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Tests;

public sealed class SectionDiscovererTests
{
    [Fact]
    public void Discovers_document_section_across_page_boundary_with_evidence()
    {
        var start = Token(1, "START", 500);
        var firstBody = Token(1, "first", 400);
        var secondBody = Token(2, "second", 500);
        var end = Token(2, "END", 400);
        var document = Document(start, firstBody, secondBody, end);
        var definition = new SectionDefinition
        {
            Id = "body",
            StartRuleId = "start",
            EndRuleId = "end",
            MayContinueOnNextPage = true
        };

        var result = new SectionDiscoverer().Discover(definition, Rules(), document);

        Assert.Equal(SectionDiscoveryStatus.Discovered, result.Status);
        var section = Assert.Single(result.Sections);
        Assert.Equal(1, section.StartPageNumber);
        Assert.Equal(2, section.EndPageNumber);
        Assert.Equal(new[] { start, firstBody, secondBody, end }, section.Tokens);
        Assert.Same(start, Assert.Single(section.StartEvidence!.SourceTokens));
        Assert.Same(end, Assert.Single(section.EndEvidence!.SourceTokens));
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Discovers_one_section_per_page_from_repeated_boundaries()
    {
        var document = Document(
            Token(1, "HEADER", 500), Token(1, "columns", 490), Token(1, "body", 400),
            Token(2, "HEADER", 500), Token(2, "columns", 490), Token(2, "body", 400));
        var definition = new SectionDefinition
        {
            Id = "header",
            StartRuleId = "header-start",
            EndRuleId = "header-end",
            Scope = SectionScope.PerPage
        };
        var rules = new[]
        {
            Rule("header-start", "HEADER", repeated: true),
            Rule("header-end", "columns", repeated: true)
        };

        var result = new SectionDiscoverer().Discover(definition, rules, document);

        Assert.Equal(SectionDiscoveryStatus.Discovered, result.Status);
        Assert.Equal(2, result.Sections.Count);
        Assert.All(result.Sections, section => Assert.Equal(section.StartPageNumber, section.EndPageNumber));
        Assert.All(result.Sections, section => Assert.Equal(2, section.Tokens.Count));
    }

    [Fact]
    public void Reports_missing_boundary_without_guessing()
    {
        var definition = new SectionDefinition { Id = "body", StartRuleId = "start", EndRuleId = "end" };
        var document = Document(Token(1, "START", 500), Token(1, "body", 400));

        var result = new SectionDiscoverer().Discover(definition, Rules(), document);

        Assert.Equal(SectionDiscoveryStatus.Unmatched, result.Status);
        Assert.Empty(result.Sections);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(RecognitionDiagnosticKind.UnmatchedSectionBoundary, diagnostic.Kind);
        Assert.Equal("end", diagnostic.RuleId);
    }

    [Fact]
    public void Reports_ambiguous_boundary_and_retains_candidate_tokens()
    {
        var first = Token(1, "START", 500);
        var second = Token(1, "START", 450);
        var definition = new SectionDefinition { Id = "body", StartRuleId = "start", EndRuleId = "end" };
        var document = Document(first, second, Token(1, "END", 400));

        var result = new SectionDiscoverer().Discover(definition, Rules(), document);

        Assert.Equal(SectionDiscoveryStatus.Ambiguous, result.Status);
        Assert.Empty(result.Sections);
        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(RecognitionDiagnosticKind.AmbiguousSectionBoundary, diagnostic.Kind);
        Assert.Equal(new[] { first, second }, diagnostic.CandidateTokens);
    }

    [Fact]
    public void Reports_boundaries_in_reverse_logical_order()
    {
        var definition = new SectionDefinition { Id = "body", StartRuleId = "start", EndRuleId = "end" };
        var document = Document(Token(1, "END", 500), Token(1, "START", 400));

        var result = new SectionDiscoverer().Discover(definition, Rules(), document);

        Assert.Equal(SectionDiscoveryStatus.Ambiguous, result.Status);
        Assert.Empty(result.Sections);
        Assert.Equal(RecognitionDiagnosticKind.InvalidSectionBoundaryOrder, Assert.Single(result.Diagnostics).Kind);
    }

    private static RecognitionRuleDefinition[] Rules() =>
        new[] { Rule("start", "START"), Rule("end", "END") };

    private static RecognitionRuleDefinition Rule(string id, string text, bool repeated = false) =>
        new RecognitionRuleDefinition
        {
            Id = id,
            Text = text,
            PageScope = repeated ? PageScope.Repeated : PageScope.Any
        };

    private static PdfDocument Document(params PdfTextToken[] tokens) =>
        new PdfDocument(tokens.GroupBy(token => token.PageNumber).Select(group => new PdfPage(group.Key, group)));

    private static PdfTextToken Token(int page, string text, double baseline) =>
        new PdfTextToken(page, text, 10, 20, baseline, false, false);
}
