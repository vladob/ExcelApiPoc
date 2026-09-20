using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Tests;

public sealed class LayoutRecognizerTests
{
    [Fact]
    public void Orchestrates_sections_records_classification_and_fields_with_evidence()
    {
        var key = Token("A", 10, 20);
        var value = Token("value", 80, 100);
        var document = Document(key, value);
        var layout = Layout(
            new[]
            {
                Rule("record", "A value")
            },
            new[]
            {
                new RecordFieldSetDefinition
                {
                    RecordRuleId = "record",
                    Fields = new List<RecordFieldDefinition>
                    {
                        Field("key", HorizontalAnchor.Left, 10),
                        Field("value", HorizontalAnchor.Right, 100)
                    }
                }
            });

        var result = new LayoutRecognizer().Recognize(document, layout);

        Assert.Same(document, result.Document);
        Assert.Single(result.Sections);
        Assert.Empty(result.Diagnostics);
        Assert.Single(result.RecordDiscovery.Records);
        var record = Assert.Single(result.Records);
        Assert.Equal(RuleMatchStatus.Matched, record.Status);
        Assert.Equal("record", record.RuleId);
        Assert.Same(record.SourceRecord, Assert.Single(record.Classification.Matches).SourceRecord);
        Assert.Equal(new[] { "key", "value" }, record.Fields.Select(field => field.FieldId));
        Assert.Equal(new[] { "A", "value" }, record.Fields.Select(field => field.Value!.Value));
        Assert.Same(key, Assert.Single(record.Fields[0].Value!.Evidence.SourceTokens));
        Assert.Same(value, Assert.Single(record.Fields[1].Value!.Evidence.SourceTokens));
    }

    [Fact]
    public void Retains_explicit_unmatched_and_ambiguous_classifications_without_fields()
    {
        var document = Document(Token("unknown", 10, 20));
        var unmatched = new LayoutRecognizer().Recognize(
            document,
            Layout(new[] { Rule("other", "other") }));
        var ambiguous = new LayoutRecognizer().Recognize(
            document,
            Layout(new[]
            {
                CountRule("first"),
                CountRule("second")
            }));

        var unmatchedRecord = Assert.Single(unmatched.Records);
        Assert.Equal(RuleMatchStatus.Unmatched, unmatchedRecord.Status);
        Assert.Null(unmatchedRecord.RuleId);
        Assert.Empty(unmatchedRecord.Fields);

        var ambiguousRecord = Assert.Single(ambiguous.Records);
        Assert.Equal(RuleMatchStatus.Ambiguous, ambiguousRecord.Status);
        Assert.Null(ambiguousRecord.RuleId);
        Assert.Equal(2, ambiguousRecord.Classification.Matches.Count);
        Assert.Empty(ambiguousRecord.Fields);
    }

    private static LayoutDefinition Layout(
        IEnumerable<RecordRecognitionRuleDefinition> recordRules,
        IEnumerable<RecordFieldSetDefinition>? recordFields = null) =>
        new LayoutDefinition
        {
            Sections = new List<SectionDefinition>
            {
                new SectionDefinition { Id = "body" }
            },
            RecordRules = recordRules.ToList(),
            RecordFields = recordFields?.ToList() ?? new List<RecordFieldSetDefinition>()
        };

    private static RecordRecognitionRuleDefinition Rule(string id, string text) =>
        new RecordRecognitionRuleDefinition
        {
            Id = id,
            SectionId = "body",
            Text = text
        };

    private static RecordRecognitionRuleDefinition CountRule(string id) =>
        new RecordRecognitionRuleDefinition
        {
            Id = id,
            SectionId = "body",
            MinimumTokenCount = 1
        };

    private static RecordFieldDefinition Field(
        string id,
        HorizontalAnchor anchor,
        double position) =>
        new RecordFieldDefinition
        {
            Id = id,
            Horizontal = new HorizontalMatchDefinition
            {
                Anchor = anchor,
                Position = position
            }
        };

    private static PdfDocument Document(params PdfTextToken[] tokens) =>
        new PdfDocument(new[] { new PdfPage(1, tokens) });

    private static PdfTextToken Token(string text, double left, double right) =>
        new PdfTextToken(1, text, left, right, 100, false, false);
}
