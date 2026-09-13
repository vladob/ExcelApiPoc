using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Tests;

public sealed class TokenRuleMatcherTests
{
    [Fact]
    public void Matches_normalized_text_position_baseline_and_style_with_evidence()
    {
        var token = Token(1, "A\u00a0B", 10.25, 20.75, 100.5, bold: true);
        var rule = new RecognitionRuleDefinition
        {
            Id = "field",
            Text = "a b",
            IgnoreCase = true,
            Horizontal = new HorizontalMatchDefinition { Anchor = HorizontalAnchor.Right, Position = 21 },
            Baseline = 101,
            IsBold = true
        };

        var result = new TokenRuleMatcher().Match(rule, new[] { token });

        Assert.Equal(RuleMatchStatus.Matched, result.Status);
        var evidence = Assert.Single(result.Matches);
        Assert.Same(token, Assert.Single(evidence.SourceTokens));
        Assert.Equal(4, evidence.Criteria.Count);
        Assert.All(evidence.Criteria, criterion => Assert.True(criterion.IsMatch));
        Assert.Equal(5d, evidence.Criteria.Single(criterion => criterion.Criterion == "Right").Tolerance);
        Assert.Equal(4d, evidence.Criteria.Single(criterion => criterion.Criterion == "Baseline").Tolerance);
    }

    [Theory]
    [InlineData(HorizontalAnchor.Left, 10)]
    [InlineData(HorizontalAnchor.Right, 20)]
    [InlineData(HorizontalAnchor.Center, 15)]
    public void Supports_each_horizontal_anchor(HorizontalAnchor anchor, double position)
    {
        var rule = new RecognitionRuleDefinition
        {
            Id = "position",
            Horizontal = new HorizontalMatchDefinition { Anchor = anchor, Position = position, Tolerance = 0 }
        };

        var result = new TokenRuleMatcher().Match(rule, new[] { Token(1, "value", 10, 20, 100) });

        Assert.Equal(RuleMatchStatus.Matched, result.Status);
    }

    [Fact]
    public void Returns_unmatched_when_no_candidate_satisfies_all_criteria()
    {
        var rule = new RecognitionRuleDefinition { Id = "bold", IsBold = true };

        var result = new TokenRuleMatcher().Match(rule, new[] { Token(1, "value", 10, 20, 100) });

        Assert.Equal(RuleMatchStatus.Unmatched, result.Status);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void Returns_ambiguous_instead_of_selecting_first_candidate()
    {
        var rule = new RecognitionRuleDefinition { Id = "value", Text = "same" };
        var tokens = new[]
        {
            Token(1, "same", 10, 20, 100),
            Token(1, "same", 30, 40, 90)
        };

        var result = new TokenRuleMatcher().Match(rule, tokens);

        Assert.Equal(RuleMatchStatus.Ambiguous, result.Status);
        Assert.Equal(2, result.Matches.Count);
    }

    [Fact]
    public void Allows_explicit_multiple_matches()
    {
        var rule = new RecognitionRuleDefinition { Id = "value", Text = "same", AllowMultipleMatches = true };

        var result = new TokenRuleMatcher().Match(rule, new[]
        {
            Token(1, "same", 10, 20, 100),
            Token(2, "same", 10, 20, 100)
        });

        Assert.Equal(RuleMatchStatus.Matched, result.Status);
        Assert.Equal(2, result.Matches.Count);
    }

    [Fact]
    public void Applies_first_and_last_page_scope()
    {
        var tokens = new[]
        {
            Token(1, "same", 10, 20, 100),
            Token(2, "same", 10, 20, 100),
            Token(3, "same", 10, 20, 100)
        };
        var matcher = new TokenRuleMatcher();
        var first = new RecognitionRuleDefinition { Id = "first", Text = "same", PageScope = PageScope.First };
        var last = new RecognitionRuleDefinition { Id = "last", Text = "same", PageScope = PageScope.Last };

        var firstResult = matcher.Match(first, tokens);
        var lastResult = matcher.Match(last, tokens);

        Assert.Equal(1, Assert.Single(firstResult.Matches).SourceTokens[0].PageNumber);
        Assert.Equal(3, Assert.Single(lastResult.Matches).SourceTokens[0].PageNumber);
    }

    private static PdfTextToken Token(
        int page,
        string text,
        double left,
        double right,
        double baseline,
        bool bold = false,
        bool italic = false) =>
        new PdfTextToken(page, text, left, right, baseline, bold, italic);
}
