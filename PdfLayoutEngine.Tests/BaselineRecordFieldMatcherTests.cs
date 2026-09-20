using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;

namespace PdfLayoutEngine.Tests;

public sealed class BaselineRecordFieldMatcherTests
{
    [Fact]
    public void Matches_all_fields_with_fractional_coordinates_and_evidence()
    {
        var tokens = new[]
        {
            Token("first", 371.833),
            Token("second", 452.098),
            Token("third", 531.583)
        };
        var record = Record(tokens);
        var fieldSet = new RecordFieldSetDefinition
        {
            RecordRuleId = "record",
            Fields = new List<RecordFieldDefinition>
            {
                Field("value-1", 372),
                Field("value-2", 452),
                Field("value-3", 532)
            }
        };

        var results = new BaselineRecordFieldMatcher().Match(record, fieldSet);

        Assert.Equal(3, results.Count);
        Assert.All(results, result => Assert.Equal(RuleMatchStatus.Matched, result.Status));
        Assert.Equal(new[] { "first", "second", "third" }, results.Select(result => result.Value!.Value));
        Assert.Equal(tokens, results.Select(result => Assert.Single(result.Value!.Evidence.SourceTokens)));
        Assert.All(results, result => Assert.Equal(5d, Assert.Single(result.Value!.Evidence.Criteria).Tolerance));
    }

    [Fact]
    public void Returns_unmatched_when_field_is_absent()
    {
        var result = new BaselineRecordFieldMatcher().Match(
            Record(new[] { Token("value", 100) }),
            Field("missing", 770));

        Assert.Equal(RuleMatchStatus.Unmatched, result.Status);
        Assert.Null(result.Value);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void Returns_ambiguous_and_retains_candidate_evidence()
    {
        var first = Token("first", 769);
        var second = Token("second", 771);

        var result = new BaselineRecordFieldMatcher().Match(
            Record(new[] { first, second }),
            Field("ambiguous", 770));

        Assert.Equal(RuleMatchStatus.Ambiguous, result.Status);
        Assert.Null(result.Value);
        Assert.Equal(2, result.Matches.Count);
        Assert.Equal(
            new[] { first, second },
            result.Matches.Select(match => Assert.Single(match.SourceTokens)));
    }

    private static RecordFieldDefinition Field(string id, double right) =>
        new RecordFieldDefinition
        {
            Id = id,
            Horizontal = new HorizontalMatchDefinition
            {
                Anchor = HorizontalAnchor.Right,
                Position = right
            }
        };

    private static BaselineRecord Record(IEnumerable<PdfTextToken> tokens) =>
        new BaselineRecord("body", new[] { new BaselineGroup(1, tokens) });

    private static PdfTextToken Token(string text, double right) =>
        new PdfTextToken(1, text, right - 20, right, 100, false, false);
}
