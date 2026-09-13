using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PdfLayoutEngine.Definitions;

public sealed class LayoutDefinition
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; }
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public MatchingDefaults Defaults { get; set; } = new MatchingDefaults();
    public List<SectionDefinition> Sections { get; set; } = new List<SectionDefinition>();
    public List<RecognitionRuleDefinition> Rules { get; set; } = new List<RecognitionRuleDefinition>();
    public List<RecordContinuationDefinition> RecordContinuations { get; set; } = new List<RecordContinuationDefinition>();
    public List<RecordRecognitionRuleDefinition> RecordRules { get; set; } = new List<RecordRecognitionRuleDefinition>();
}

public sealed class MatchingDefaults
{
    public const double StandardHorizontalTolerance = 5d;
    public const double StandardBaselineTolerance = 4d;

    public double HorizontalTolerance { get; set; } = StandardHorizontalTolerance;
    public double BaselineTolerance { get; set; } = StandardBaselineTolerance;
}

public sealed class SectionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string? StartRuleId { get; set; }
    public string? EndRuleId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SectionScope Scope { get; set; } = SectionScope.Document;

    public bool MayContinueOnNextPage { get; set; }
}

public enum SectionScope
{
    Document,
    PerPage
}

public sealed class RecordContinuationDefinition
{
    public string Id { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public bool AcrossPageBoundaryOnly { get; set; }
    public BaselineGroupConditionDefinition Previous { get; set; } = new BaselineGroupConditionDefinition();
    public BaselineGroupConditionDefinition Next { get; set; } = new BaselineGroupConditionDefinition();
}

public sealed class BaselineGroupConditionDefinition
{
    public double? LeftAtLeast { get; set; }
    public double? LeftAtMost { get; set; }
    public double? RightAtLeast { get; set; }
    public double? RightAtMost { get; set; }
}

public sealed class RecordRecognitionRuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string? SectionId { get; set; }
    public string? Text { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TextMatchMode TextMatch { get; set; } = TextMatchMode.Equals;

    public bool IgnoreCase { get; set; }
    public int? MinimumGroupCount { get; set; }
    public int? MaximumGroupCount { get; set; }
    public int? MinimumTokenCount { get; set; }
    public int? MaximumTokenCount { get; set; }
    public double? LeftAtLeast { get; set; }
    public double? LeftAtMost { get; set; }
    public double? RightAtLeast { get; set; }
    public double? RightAtMost { get; set; }
    public bool? CrossesPageBoundary { get; set; }
}

public sealed class RecognitionRuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string? SectionId { get; set; }
    public string? Text { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TextMatchMode TextMatch { get; set; } = TextMatchMode.Equals;

    public bool IgnoreCase { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PageScope PageScope { get; set; } = PageScope.Any;

    public HorizontalMatchDefinition? Horizontal { get; set; }
    public double? Baseline { get; set; }
    public double? BaselineTolerance { get; set; }
    public bool? IsBold { get; set; }
    public bool? IsItalic { get; set; }
    public bool AllowMultipleMatches { get; set; }
}

public sealed class HorizontalMatchDefinition
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HorizontalAnchor Anchor { get; set; }

    public double Position { get; set; }
    public double? Tolerance { get; set; }
}

public enum HorizontalAnchor
{
    Left,
    Right,
    Center
}

public enum TextMatchMode
{
    Equals,
    Contains,
    RegularExpression
}

public enum PageScope
{
    Any,
    First,
    Last,
    Repeated
}
