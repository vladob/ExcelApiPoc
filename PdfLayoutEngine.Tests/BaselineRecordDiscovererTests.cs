using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Tests;

public sealed class BaselineRecordDiscovererTests
{
    [Fact]
    public void Builds_one_record_per_group_and_excludes_complete_boundary_groups()
    {
        var headerAnchor = Token(1, "header anchor", 500, 10);
        var headerCompanion = Token(1, "header companion", 500, 30);
        var first = Token(1, "first", 400, 10);
        var second = Token(1, "second", 300, 10);
        var footerAnchor = Token(1, "footer anchor", 100, 10);
        var section = Section(
            1,
            new[] { headerAnchor, headerCompanion, first, second, footerAnchor },
            Evidence("start", headerAnchor),
            Evidence("end", footerAnchor));

        var result = Discover(section);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(2, result.Records.Count);
        Assert.Same(first, Assert.Single(result.Records[0].SourceTokens));
        Assert.Same(second, Assert.Single(result.Records[1].SourceTokens));
        Assert.DoesNotContain(headerCompanion, result.Records.SelectMany(record => record.SourceTokens));
    }

    [Fact]
    public void Preserves_section_page_groups_and_source_tokens()
    {
        var left = Token(2, "left", 400, 10);
        var right = Token(2, "right", 400, 30);
        var result = Discover(Section(2, new[] { right, left }));

        var record = Assert.Single(result.Records);
        Assert.Equal("body", record.SectionId);
        Assert.Equal(2, record.StartPageNumber);
        Assert.Equal(2, record.EndPageNumber);
        Assert.False(record.CrossesPageBoundary);
        Assert.Equal("left right", record.Text);
        Assert.Equal(new[] { left, right }, record.SourceTokens);
        Assert.Single(record.Groups);
    }

    [Fact]
    public void Continuation_policy_can_join_adjacent_groups_on_same_page()
    {
        var section = Section(1, new[]
        {
            Token(1, "first", 400, 10),
            Token(1, "continued", 380, 20)
        });

        var result = Discover(section, new AlwaysContinuePolicy());

        var record = Assert.Single(result.Records);
        Assert.Equal(2, record.Groups.Count);
        Assert.Equal("first continued", record.Text);
    }

    [Fact]
    public void Continuation_policy_can_join_record_across_adjacent_pages_when_enabled()
    {
        var firstPage = Section(1, new[] { Token(1, "first", 100, 10) }, mayContinue: true);
        var secondPage = Section(2, new[] { Token(2, "continued", 500, 10) }, mayContinue: true);

        var result = Discover(new[] { firstPage, secondPage }, new AlwaysContinuePolicy());

        var record = Assert.Single(result.Records);
        Assert.True(record.CrossesPageBoundary);
        Assert.Equal(1, record.StartPageNumber);
        Assert.Equal(2, record.EndPageNumber);
        Assert.Equal(2, record.Groups.Count);
    }

    [Fact]
    public void Does_not_join_across_pages_when_section_disallows_it()
    {
        var firstPage = Section(1, new[] { Token(1, "first", 100, 10) });
        var secondPage = Section(2, new[] { Token(2, "next", 500, 10) });

        var result = Discover(new[] { firstPage, secondPage }, new AlwaysContinuePolicy());

        Assert.Equal(2, result.Records.Count);
        Assert.All(result.Records, record => Assert.False(record.CrossesPageBoundary));
    }

    [Fact]
    public void Propagates_section_diagnostics()
    {
        var diagnostic = new RecognitionDiagnostic(
            RecognitionDiagnosticKind.UnmatchedSectionBoundary,
            "footer",
            "Footer was not found.");
        var sectionResult = new SectionDiscoveryResult(
            "body",
            SectionDiscoveryStatus.Unmatched,
            Array.Empty<DiscoveredSection>(),
            new[] { diagnostic });

        var result = new BaselineRecordDiscoverer().Discover(new[] { sectionResult });

        Assert.Empty(result.Records);
        Assert.Same(diagnostic, Assert.Single(result.Diagnostics));
    }

    private static BaselineRecordDiscoveryResult Discover(
        DiscoveredSection section,
        IBaselineRecordContinuationPolicy? policy = null) =>
        Discover(new[] { section }, policy);

    private static BaselineRecordDiscoveryResult Discover(
        IEnumerable<DiscoveredSection> sections,
        IBaselineRecordContinuationPolicy? policy = null)
    {
        var result = new SectionDiscoveryResult(
            "body",
            SectionDiscoveryStatus.Discovered,
            sections,
            Array.Empty<RecognitionDiagnostic>());
        return new BaselineRecordDiscoverer().Discover(new[] { result }, policy);
    }

    private static DiscoveredSection Section(
        int page,
        IEnumerable<PdfTextToken> tokens,
        RecognitionEvidence? start = null,
        RecognitionEvidence? end = null,
        bool mayContinue = false) =>
        new DiscoveredSection("body", page, page, tokens, start, end, mayContinue);

    private static RecognitionEvidence Evidence(string ruleId, PdfTextToken token) =>
        new RecognitionEvidence(ruleId, new[] { token }, "Boundary evidence.");

    private static PdfTextToken Token(int page, string text, double baseline, double left) =>
        new PdfTextToken(page, text, left, left + 10, baseline, false, false);

    private sealed class AlwaysContinuePolicy : IBaselineRecordContinuationPolicy
    {
        public bool ContinuesRecord(BaselineRecord currentRecord, BaselineGroup nextGroup) => true;
    }
}
