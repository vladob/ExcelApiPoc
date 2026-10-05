using PdfLayoutEngine.V2;
using System.Globalization;
using System.Text.Json;

namespace PdfLayoutEngine.Runner;
public sealed class ManifestResult
{
    public int ManifestRowNumber { get; set; }
    public ExpectedInput Expected { get; set; } = null!;
    public ActualInput Actual { get; set; } = new();
    public string ProcessingStatus { get; set; } = "failed";
    public string RecognitionStatus { get; set; } = "unknown";
    public string ExpectationStatus { get; set; } = "notEvaluated";
    public string ValidationStatus { get; set; } = "notEvaluated";
    public int RecordCount { get; set; }
    public ResultCoverage Coverage { get; set; } = new();
    public List<StandardTotal> StandardTotals { get; set; } = new();
    public List<string> Explanations { get; set; } = new();
    public string? DetailResultPath { get; set; }
}
public sealed class ActualInput
{
    public string? Category { get; set; }
    public string? EntityCin { get; set; }
    public string? EntityName { get; set; }
    public int? FiscalYear { get; set; }
}
public sealed record StandardTotal(string Name, string ScopeOccurrenceId, string? Value, string State);
public sealed record FileDetail(int ManifestRowNumber, string? ResolvedPath, long ElapsedMilliseconds, string? EngineResultPath, string? ErrorType, string? ErrorMessage, string? ErrorDetail, ManifestResult Summary);
public static class Results
{
    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    public static ManifestResult Evaluate(ManifestEntry entry, EngineResult engine)
    {
        string? Value(string name) => engine.Metadata.TryGetValue(name, out var v) && v.State == "present" ? v.Value : null;
        var category = engine.RecognitionDecisions.FirstOrDefault(d => d.Target == "category");
        var actualCategory = category?.State == "confirmed" ? category.Candidates.FirstOrDefault()?.Id : null;
        var result = new ManifestResult { ManifestRowNumber = entry.RowNumber, Expected = entry.Expected, ProcessingStatus = engine.ProcessingStatus, Coverage = engine.Coverage,
            Actual = new ActualInput { Category = actualCategory, EntityCin = Value("reportingEntity.cin"), EntityName = Value("reportingEntity.name"), FiscalYear = int.TryParse(Value("fiscalYear"), NumberStyles.None, CultureInfo.InvariantCulture, out var year) && year is >= 1000 and <= 9999 ? year : null },
            // Structural headers, controls and subtotals are not exported detail records.
            RecordCount = engine.Records.Count(r => r.ExportEligibility == "eligible"),
            RecognitionStatus = engine.RecognitionDecisions.Any(d => d.State == "ambiguous") ? "ambiguous" : engine.RecognitionDecisions.Count == 3 && engine.RecognitionDecisions.All(d => d.State == "confirmed") ? "confirmed" : engine.RecognitionDecisions.Any(d => d.State != "unknown") ? "tentative" : "unknown" };
        var states = engine.ValidationResults.Select(r => r.State).ToArray();
        result.ValidationStatus = states.Contains("fail") ? "fail" : states.Contains("inconclusive") ? "inconclusive" : states.Contains("pass") ? "pass" : "notEvaluated";
        var recordIds = engine.Records.Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
        result.StandardTotals = engine.Calculations.Where(c => !recordIds.Contains(c.ScopeOccurrenceId)).Select(c => new StandardTotal(c.RuleId, c.ScopeOccurrenceId, c.Value, c.State)).ToList();
        bool fail = false, unresolved = false;
        void Compare(string name, string? expected, string? actual)
        {
            if (string.IsNullOrEmpty(expected)) return;
            if (actual == null) { unresolved = true; result.Explanations.Add(name + ": actual value is unresolved."); }
            else if (!string.Equals(expected, actual, StringComparison.Ordinal)) { fail = true; result.Explanations.Add($"{name}: expected '{expected}', actual '{actual}'."); }
        }
        var map = new Dictionary<string, string> { ["HL_KNIHA"] = "GL", ["U_DENNIK"] = "AJ", ["UCT_ROZVRH"] = "AF" };
        Compare("category", map[entry.Expected.PerceivedCategory], result.Actual.Category);
        Compare("fiscalYear", entry.Expected.ExpectedYear, result.Actual.FiscalYear?.ToString(CultureInfo.InvariantCulture));
        var entity = entry.Expected.AccountingEntity.Trim();
        string? cin = entity.Length >= 8 && entity.Take(8).All(char.IsAsciiDigit) && (entity.Length == 8 || char.IsWhiteSpace(entity[8]) || entity[8] == '-') ? entity[..8] : null;
        if (cin != null) Compare("entityCin", cin, result.Actual.EntityCin);
        else if (entity.Length > 0) { unresolved = true; result.Explanations.Add("AccountingEntity has no leading eight-digit CIN; name equivalence requires review."); }
        result.ExpectationStatus = fail ? "fail" : unresolved ? "inconclusive" : "pass";
        result.Explanations.AddRange(engine.Diagnostics.GroupBy(d => d.Code).Select(g => g.Key + " (" + g.Count() + "): " + g.First().Message));
        return result;
    }
}
