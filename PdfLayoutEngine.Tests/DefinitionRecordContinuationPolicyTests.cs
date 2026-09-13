using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Tests;

public sealed class DefinitionRecordContinuationPolicyTests
{
    [Fact]
    public void Continues_across_page_for_matching_previous_and_next_geometry()
    {
        var policy = Policy(340, 340);
        var previous = Record(Group(8, 42.749, 125.845));
        var next = Group(9, 349.593, 770.108);

        var decision = policy.Evaluate(previous, next);

        Assert.Equal(RecordContinuationDecisionStatus.Continued, decision.Status);
        var evidence = Assert.Single(decision.Matches);
        Assert.Equal("split-values", evidence.DefinitionId);
        Assert.Same(previous.Groups[0], evidence.PreviousGroup);
        Assert.Same(next, evidence.NextGroup);
        Assert.Equal(3, evidence.Criteria.Count);
        Assert.All(evidence.Criteria, criterion => Assert.True(criterion.IsMatch));
        Assert.Equal("125.845", evidence.Criteria.Single(item => item.Criterion == "PreviousRightAtMost").Actual);
        Assert.Equal("349.593", evidence.Criteria.Single(item => item.Criterion == "NextLeftAtLeast").Actual);
    }

    [Theory]
    [InlineData(770.116, 22.501)]
    [InlineData(125.845, 22.501)]
    [InlineData(770.116, 349.593)]
    public void Does_not_continue_when_geometry_is_incomplete(double previousRight, double nextLeft)
    {
        var decision = Policy(340, 340).Evaluate(
            Record(Group(8, 42.749, previousRight)),
            Group(9, nextLeft, 770.108));

        Assert.Equal(RecordContinuationDecisionStatus.NotContinued, decision.Status);
        Assert.Empty(decision.Matches);
    }

    [Fact]
    public void Does_not_apply_page_boundary_rule_on_same_page()
    {
        var decision = Policy(340, 340).Evaluate(
            Record(Group(8, 42.749, 125.845)),
            Group(8, 349.593, 770.108));

        Assert.Equal(RecordContinuationDecisionStatus.NotContinued, decision.Status);
    }

    [Fact]
    public void Returns_ambiguous_when_two_definitions_match()
    {
        var definition = Definition("first", 340, 340);
        var policy = new DefinitionRecordContinuationPolicy(new[]
        {
            definition,
            Definition("second", 340, 340)
        });

        var decision = policy.Evaluate(
            Record(Group(8, 42.749, 125.845)),
            Group(9, 349.593, 770.108));

        Assert.Equal(RecordContinuationDecisionStatus.Ambiguous, decision.Status);
        Assert.Equal(2, decision.Matches.Count);
    }

    [Fact]
    public void Ignores_definitions_for_another_section()
    {
        var definition = Definition("other", 340, 340);
        definition.SectionId = "footer";
        var policy = new DefinitionRecordContinuationPolicy(new[] { definition });

        var decision = policy.Evaluate(
            Record(Group(8, 42.749, 125.845)),
            Group(9, 349.593, 770.108));

        Assert.Equal(RecordContinuationDecisionStatus.NotContinued, decision.Status);
    }

    [Fact]
    public void Discoverer_joins_only_the_matching_page_transition_and_retains_evidence()
    {
        var sections = new[]
        {
            Section(1, Token(1, "complete", 22.501, 770.116, 100)),
            Section(2,
                Token(2, "complete", 22.501, 770.116, 500),
                Token(2, "label", 42.749, 125.845, 100)),
            Section(3, Token(3, "values", 349.593, 770.108, 500))
        };
        var sectionResult = new SectionDiscoveryResult(
            "body",
            SectionDiscoveryStatus.Discovered,
            sections,
            Array.Empty<RecognitionDiagnostic>());

        var result = new BaselineRecordDiscoverer().Discover(
            new[] { sectionResult },
            Policy(340, 340));

        Assert.Equal(3, result.Records.Count);
        var joined = Assert.Single(result.Records, record => record.CrossesPageBoundary);
        Assert.Equal("label values", joined.Text);
        Assert.Equal(2, joined.StartPageNumber);
        Assert.Equal(3, joined.EndPageNumber);
        Assert.Equal("split-values", Assert.Single(joined.Continuations).DefinitionId);
        Assert.Empty(result.Diagnostics);
    }

    private static DefinitionRecordContinuationPolicy Policy(double rightAtMost, double leftAtLeast) =>
        new DefinitionRecordContinuationPolicy(new[] { Definition("split-values", rightAtMost, leftAtLeast) });

    private static RecordContinuationDefinition Definition(string id, double rightAtMost, double leftAtLeast) =>
        new RecordContinuationDefinition
        {
            Id = id,
            SectionId = "body",
            AcrossPageBoundaryOnly = true,
            Previous = new BaselineGroupConditionDefinition { RightAtMost = rightAtMost },
            Next = new BaselineGroupConditionDefinition { LeftAtLeast = leftAtLeast }
        };

    private static BaselineRecord Record(BaselineGroup group) =>
        new BaselineRecord("body", new[] { group });

    private static BaselineGroup Group(int page, double left, double right) =>
        new BaselineGroup(page, new[]
        {
            new PdfTextToken(page, "value", left, right, 100, false, false)
        });

    private static DiscoveredSection Section(int page, params PdfTextToken[] tokens) =>
        new DiscoveredSection("body", page, page, tokens, null, null, mayContinueOnNextPage: true);

    private static PdfTextToken Token(
        int page,
        string text,
        double left,
        double right,
        double baseline) =>
        new PdfTextToken(page, text, left, right, baseline, false, false);
}
