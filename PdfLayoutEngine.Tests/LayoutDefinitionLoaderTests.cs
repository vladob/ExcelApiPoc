using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Validation;

namespace PdfLayoutEngine.Tests;

public sealed class LayoutDefinitionLoaderTests
{
    [Fact]
    public void Loads_valid_v1_definition_and_applies_default_tolerances()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "sections": [{ "id": "body", "mayContinueOnNextPage": true }],
          "rules": [
            {
              "id": "record",
              "sectionId": "body",
              "horizontal": { "anchor": "Right", "position": 770 },
              "pageScope": "Repeated"
            }
          ]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Definition);
        Assert.Equal(5d, result.Definition.Defaults.HorizontalTolerance);
        Assert.Equal(4d, result.Definition.Defaults.BaselineTolerance);
        Assert.Equal(HorizontalAnchor.Right, result.Definition.Rules[0].Horizontal!.Anchor);
    }

    [Fact]
    public void Reports_unsupported_version_duplicate_ids_and_unknown_references()
    {
        const string json = """
        {
          "schemaVersion": 2,
          "id": "sample",
          "sections": [{ "id": "body", "startRuleId": "missing" }],
          "rules": [
            { "id": "same", "sectionId": "unknown", "text": "A" },
            { "id": "same", "text": "B" }
          ]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, message => message.Path == "$.schemaVersion");
        Assert.Contains(result.Messages, message => message.Message.Contains("Duplicate rule id"));
        Assert.Contains(result.Messages, message => message.Message.Contains("Unknown section id"));
        Assert.Contains(result.Messages, message => message.Message.Contains("Unknown rule id"));
    }

    [Fact]
    public void Reports_malformed_json_without_throwing()
    {
        var result = new LayoutDefinitionLoader().Load("{ not-json }");

        Assert.False(result.IsValid);
        Assert.Null(result.Definition);
        Assert.Single(result.Messages);
        Assert.Equal(ValidationSeverity.Error, result.Messages[0].Severity);
    }

    [Fact]
    public void Reports_invalid_regular_expression()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "rules": [{ "id": "pattern", "text": "[", "textMatch": "RegularExpression" }]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, message => message.Message.Contains("Invalid regular expression"));
    }

    [Fact]
    public void Loads_per_page_section_scope()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "sections": [{ "id": "header", "scope": "PerPage", "startRuleId": "header" }],
          "rules": [{ "id": "header", "text": "Header", "pageScope": "Repeated" }]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.True(result.IsValid);
        Assert.Equal(SectionScope.PerPage, result.Definition!.Sections[0].Scope);
    }

    [Fact]
    public void Allows_records_in_per_page_section_to_continue_on_next_page()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "sections": [{ "id": "header", "scope": "PerPage", "mayContinueOnNextPage": true }]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.True(result.IsValid);
        Assert.True(result.Definition!.Sections[0].MayContinueOnNextPage);
    }

    [Fact]
    public void Loads_record_continuation_geometry()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "sections": [{ "id": "body", "scope": "PerPage", "mayContinueOnNextPage": true }],
          "recordContinuations": [
            {
              "id": "split-values",
              "sectionId": "body",
              "acrossPageBoundaryOnly": true,
              "previous": { "rightAtMost": 340 },
              "next": { "leftAtLeast": 340 }
            }
          ]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.True(result.IsValid);
        var continuation = Assert.Single(result.Definition!.RecordContinuations);
        Assert.True(continuation.AcrossPageBoundaryOnly);
        Assert.Equal(340d, continuation.Previous.RightAtMost);
        Assert.Equal(340d, continuation.Next.LeftAtLeast);
    }

    [Fact]
    public void Reports_invalid_record_continuation_definitions()
    {
        const string json = """
        {
          "schemaVersion": 1,
          "id": "sample",
          "sections": [{ "id": "body" }],
          "recordContinuations": [
            { "id": "same", "sectionId": "missing" },
            {
              "id": "same",
              "sectionId": "body",
              "previous": { "rightAtLeast": 400, "rightAtMost": 300 }
            }
          ]
        }
        """;

        var result = new LayoutDefinitionLoader().Load(json);

        Assert.False(result.IsValid);
        Assert.Contains(result.Messages, message => message.Message.Contains("Duplicate record continuation id"));
        Assert.Contains(result.Messages, message => message.Message.Contains("Unknown section id"));
        Assert.Contains(result.Messages, message => message.Message.Contains("At least one baseline-group condition"));
        Assert.Contains(result.Messages, message => message.Message.Contains("rightAtLeast cannot exceed rightAtMost"));
    }
}
