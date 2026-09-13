using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;

namespace PdfLayoutEngine.Tests;

public sealed class BaselineRecordRuleMatcherTests
{
    [Fact]
    public void Matches_complete_logical_record_and_retains_evidence()
    {
        var first = Group(1, "Label", 10, 30);
        var second = Group(2, "123,45", 40, 60);
        var continuation = new RecordContinuationEvidence(
            "continued",
            first,
            second,
            Array.Empty<MatchCriterionEvidence>());
        var record = new BaselineRecord("body", new[] { first, second }, new[] { continuation });
        var rule = new RecordRecognitionRuleDefinition
        {
            Id = "candidate",
            SectionId = "body",
            Text = "^label 123,45$",
            TextMatch = TextMatchMode.RegularExpression,
            IgnoreCase = true,
            MinimumGroupCount = 2,
            MaximumGroupCount = 2,
            MinimumTokenCount = 2,
            MaximumTokenCount = 2,
            LeftAtLeast = 10,
            LeftAtMost = 10,
            RightAtLeast = 60,
            RightAtMost = 60,
            CrossesPageBoundary = true
        };

        var result = new BaselineRecordRuleMatcher().Match(record, new[] { rule });

        Assert.Equal(RuleMatchStatus.Matched, result.Status);
        var evidence = Assert.Single(result.Matches);
        Assert.Equal("candidate", evidence.RuleId);
        Assert.Same(record, evidence.SourceRecord);
        Assert.Equal(11, evidence.Criteria.Count);
        Assert.All(evidence.Criteria, criterion => Assert.True(criterion.IsMatch));
        Assert.Equal(new[] { first, second }, evidence.SourceRecord.Groups);
        Assert.Equal(2, evidence.SourceRecord.SourceTokens.Count);
    }

    [Fact]
    public void Returns_unmatched_when_no_rule_satisfies_all_criteria()
    {
        var record = Record("value");
        var rule = new RecordRecognitionRuleDefinition
        {
            Id = "different",
            Text = "other"
        };

        var result = new BaselineRecordRuleMatcher().Match(record, new[] { rule });

        Assert.Equal(RuleMatchStatus.Unmatched, result.Status);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void Returns_ambiguous_instead_of_selecting_first_rule()
    {
        var record = Record("same");
        var rules = new[]
        {
            new RecordRecognitionRuleDefinition { Id = "first", Text = "same" },
            new RecordRecognitionRuleDefinition { Id = "second", Text = "same" }
        };

        var result = new BaselineRecordRuleMatcher().Match(record, rules);

        Assert.Equal(RuleMatchStatus.Ambiguous, result.Status);
        Assert.Equal(new[] { "first", "second" }, result.Matches.Select(match => match.RuleId));
        Assert.All(result.Matches, match => Assert.Same(record, match.SourceRecord));
    }

    [Fact]
    public void Normalizes_nbsp_in_rule_text_without_changing_source_token()
    {
        var token = new PdfTextToken(1, "A\u00a0B", 10, 20, 100, false, false);
        var record = new BaselineRecord("body", new[] { new BaselineGroup(1, new[] { token }) });
        var rule = new RecordRecognitionRuleDefinition { Id = "normalized", Text = "A B" };

        var result = new BaselineRecordRuleMatcher().Match(record, new[] { rule });

        Assert.Equal(RuleMatchStatus.Matched, result.Status);
        Assert.Equal("A\u00a0B", token.OriginalText);
    }

    private static BaselineRecord Record(string text) =>
        new BaselineRecord("body", new[] { Group(1, text, 10, 20) });

    private static BaselineGroup Group(int page, string text, double left, double right) =>
        new BaselineGroup(page, new[]
        {
            new PdfTextToken(page, text, left, right, 100, false, false)
        });
}
