using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfLayoutEngine.V2;
public sealed class EngineOptions
{
    public string Extraction { get; set; } = "records";
    public string Validation { get; set; } = "internalConsistency";
    public int MaximumCandidatesPerPage { get; set; } = 10000;
    public int ExecutionTimeMs { get; set; } = 120000;
}
public sealed class ProcessingLevels
{
    public string Extraction { get; set; } = "records";
    public string Validation { get; set; } = "internalConsistency";
}
public sealed class RecognitionCandidate
{
    public string Id { get; set; } = "";
    public double Score { get; set; }
    public List<string> EvidenceRefs { get; set; } = new List<string>();
}
public sealed class RecognitionDecision
{
    public string Target { get; set; } = "";
    public string State { get; set; } = "unknown";
    public List<RecognitionCandidate> Candidates { get; set; } = new List<RecognitionCandidate>();
}
public sealed class ExtractedRecord
{
    public string Id { get; set; } = "";
    public string VariantRef { get; set; } = "";
    public string BlockRef { get; set; } = "";
    public string SemanticClass { get; set; } = "unrecognizedCandidate";
    public string ExportEligibility { get; set; } = "review";
    public List<string> AggregationMemberships { get; set; } = new List<string>();
    public Dictionary<string, FieldValue> Fields { get; set; } = new Dictionary<string, FieldValue>();
    public List<string> EvidenceRefs { get; set; } = new List<string>();
    [JsonIgnore] public int PageIndex { get; set; }
    [JsonIgnore] public double OriginY { get; set; }
    [JsonIgnore] public string Level { get; set; } = "any";
    [JsonIgnore] public Dictionary<string, string> GroupOccurrences { get; } = new Dictionary<string, string>();
    [JsonIgnore] public bool AmbiguousPlacement { get; set; }
}
public sealed class ResultCoverage
{
    public List<int> InspectedPages { get; set; } = new List<int>();
    public int AvailablePageCount { get; set; }
    public string OriginalInputCompleteness { get; set; } = "unknown";
    public int UnresolvedCandidateCount { get; set; }
    public int OrphanTokenCount { get; set; }
}
public sealed class CalculationResult
{
    public string RuleId { get; set; } = "";
    public string ScopeOccurrenceId { get; set; } = "document";
    public string State { get; set; } = "notEvaluated";
    public string? Value { get; set; }
    public List<string> InputRefs { get; set; } = new List<string>();
}
public sealed class RuleResult
{
    public string RuleId { get; set; } = "";
    public string ScopeOccurrenceId { get; set; } = "document";
    public string State { get; set; } = "notEvaluated";
    public string? CalculatedValue { get; set; }
    public string? PrintedValue { get; set; }
    public string? Difference { get; set; }
    public string? Reason { get; set; }
    public List<string> EvidenceRefs { get; set; } = new List<string>();
}
public sealed class EvidenceItem
{
    public string Id { get; set; } = "";
    public int PhysicalPageIndex { get; set; }
    public string SourcePageId { get; set; } = "";
    public List<string> SourceElementRefs { get; set; } = new List<string>();
    public string RawText { get; set; } = "";
    public List<double[]> RawGeometry { get; set; } = new List<double[]>();
    public List<double[]> NormalizedGeometry { get; set; } = new List<double[]>();
    public List<string> TransformRefs { get; set; } = new List<string>();
    public List<string> RuleIds { get; set; } = new List<string>();
    public string? BlockOccurrenceId { get; set; }
    public List<string> InheritedFrom { get; set; } = new List<string>();
}
public sealed class DefinitionProvenance
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Sha256 { get; set; } = "";
}
public sealed class TransformProvenance
{
    public string Id { get; set; } = "";
    public double[] SourceToNormalized { get; set; } = TransformPt.Identity.ToArray();
    public double[] NormalizedToSource { get; set; } = TransformPt.Identity.ToArray();
}
public sealed class ResultProvenance
{
    public string DocumentHash { get; set; } = "";
    public string EngineVersion { get; set; } = "positioned-v2-step3.1";
    public List<DefinitionProvenance> Definitions { get; set; } = new List<DefinitionProvenance>();
    public List<EvidenceItem> Evidence { get; set; } = new List<EvidenceItem>();
    public List<TransformProvenance> Transforms { get; set; } = new List<TransformProvenance>();
}
public sealed class EngineResult
{
    public int SchemaVersion { get; set; } = 2;
    public string Format { get; set; } = "unknown";
    public string ProcessingStatus { get; set; } = "incomplete";
    public ProcessingLevels RequestedLevels { get; set; } = new ProcessingLevels();
    public ProcessingLevels EffectiveLevels { get; set; } = new ProcessingLevels();
    public List<RecognitionDecision> RecognitionDecisions { get; set; } = new List<RecognitionDecision>();
    public Dictionary<string, FieldValue> Metadata { get; set; } = new Dictionary<string, FieldValue>();
    public List<ExtractedRecord> Records { get; set; } = new List<ExtractedRecord>();
    public Dictionary<string, FieldValue> PrintedControls { get; set; } = new Dictionary<string, FieldValue>();
    public List<CalculationResult> Calculations { get; set; } = new List<CalculationResult>();
    public List<RuleResult> ValidationResults { get; set; } = new List<RuleResult>();
    public ResultCoverage Coverage { get; set; } = new ResultCoverage();
    public List<EngineDiagnostic> Diagnostics { get; set; } = new List<EngineDiagnostic>();
    public ResultProvenance Provenance { get; set; } = new ResultProvenance();
    public string ToJson(bool indented = true) => JsonSerializer.Serialize(this, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = indented });
}
