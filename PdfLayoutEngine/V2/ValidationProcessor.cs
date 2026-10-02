using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PdfLayoutEngine.V2;
internal static class ValidationProcessor
{
    public static void Apply(DefinitionCatalog catalog, JsonElement layout, EngineResult result, string level, CancellationToken token)
    {
        var validation = layout.A("validationRefs").Select(catalog.Resolve).ToArray();
        foreach (var definition in validation)
        foreach (var control in definition.A("printedControls"))
        foreach (var record in result.Records.Where(r => r.VariantRef == control.S("variantRef") && r.BlockRef == control.S("blockRef")))
        {
            var block = layout.A("variants").Single(v => v.S("id") == record.VariantRef).A("blocks").Single(b => b.S("id") == record.BlockRef);
            var name = block.A("fields").Single(f => f.S("id") == control.S("fieldRef")).S("semanticName");
            if (record.Fields.TryGetValue(name, out var value)) result.PrintedControls[control.S("id") + "@" + record.Id] = value;
        }
        if (level == "none") return;
        foreach (var rule in layout.A("completeness").Where(r => r.B("enabled")))
        {
            token.ThrowIfCancellationRequested();
            switch (rule.S("op"))
            {
                case "requireFields": case "requireAny":
                    foreach (var record in result.Records.Where(r => PositionedLayoutEngine.Select(rule.GetProperty("selector"), r)))
                    {
                        var fields = rule.Strings("fieldRefs"); var present = fields.Select(f => record.Fields.TryGetValue(f, out var value) && value.State == rule.S("requiredState")).ToArray();
                        bool pass = rule.S("op") == "requireFields" ? present.All(v => v) : present.Any(v => v);
                        result.ValidationResults.Add(new RuleResult { RuleId = rule.S("id"), ScopeOccurrenceId = record.Id, State = pass ? "pass" : "inconclusive", Reason = pass ? null : "Required fields are not unambiguously present.", EvidenceRefs = record.EvidenceRefs.ToList() });
                        if (!pass && record.ExportEligibility == "eligible") record.ExportEligibility = "review";
                    }
                    break;
                case "accountForCandidates": Add(rule, result.Coverage.UnresolvedCandidateCount == 0 ? "pass" : "inconclusive", "Candidate classification coverage."); break;
                case "accountForTokens": Add(rule, result.Coverage.OrphanTokenCount == 0 ? "pass" : "inconclusive", "Text assignment coverage."); break;
                case "inspectCoverage":
                    var printed = result.Records.Where(r => r.SemanticClass == "pageHeader" && r.Fields.TryGetValue(rule.S("printedPageField"), out var f) && f.State == "present").OrderBy(r => r.PageIndex).Select(r => long.TryParse(r.Fields[rule.S("printedPageField")].Value, out var n) ? n : -1).ToArray();
                    if (printed.Length > 0 && (printed[0] > 1 || printed.Zip(printed.Skip(1), (a, b) => b != a + 1).Any(v => v))) result.Coverage.OriginalInputCompleteness = "partial";
                    Add(rule, "inconclusive", "Original input completeness is " + result.Coverage.OriginalInputCompleteness + "; contiguous pages alone do not prove completeness."); break;
                case "sequence":
                    var selectedRows = result.Records.Where(r => r.Fields.ContainsKey(rule.S("fieldRef"))).OrderBy(r => r.PageIndex).ThenByDescending(r => r.OriginY).ToArray();
                    var numbers = selectedRows.Select(r => r.Fields.TryGetValue(rule.S("fieldRef"), out var value) && value.State == "present" && long.TryParse(value.Value, out var number) ? (long?)number : null).ToArray();
                    bool validSequence = numbers.Length > 0 && numbers.All(n => n.HasValue) && numbers.Zip(numbers.Skip(1), (a, b) => b == a + (long)rule.N("increment")).All(v => v);
                    Add(rule, validSequence ? "pass" : "inconclusive", validSequence ? "Observed sequence is contiguous." : "Sequence is missing, duplicated or discontinuous."); break;
                case "checkSpacing":
                    bool gap = false;
                    foreach (var page in result.Records.GroupBy(r => r.PageIndex))
                    {
                        var rows = page.Where(r => r.ExportEligibility != "excluded").OrderByDescending(r => r.OriginY).ToArray();
                        for (int i = 1; i < rows.Length; i++)
                        {
                            var previous = rows[i - 1]; var current = rows[i];
                            var block = layout.A("variants").Single(v => v.S("id") == previous.VariantRef).A("blocks").Single(b => b.S("id") == previous.BlockRef);
                            double advance = block.N("nominalAdvancePt");
                            if (advance > 1 && previous.OriginY - current.OriginY > advance * rule.N("maximumAdvanceMultiplier") && !page.Any(r => r != previous && r != current && r.OriginY < previous.OriginY && r.OriginY > current.OriginY)) gap = true;
                        }
                    }
                    Add(rule, gap ? "inconclusive" : "pass", gap ? "Spacing suggests an unexplained gap; no record was fabricated." : "No unexplained spacing gap detected."); break;
                default: throw new NotSupportedException("Unsupported completeness operation: " + rule.S("op"));
            }
        }
        if (level != "internalConsistency") return;
        foreach (var definition in validation)
        {
            var calculations = definition.A("calculations").ToDictionary(c => c.S("id"));
            var cache = new Dictionary<string, CalculationResult>();
            CalculationResult Calculate(string id, ExtractedRecord? record)
            {
                token.ThrowIfCancellationRequested(); string scope = record?.Id ?? "document", key = id + "@" + scope;
                if (cache.TryGetValue(key, out var prior)) return prior;
                var spec = calculations[id]; var output = new CalculationResult { RuleId = id, ScopeOccurrenceId = scope }; cache.Add(key, output); result.Calculations.Add(output);
                if (!spec.B("enabled")) return output;
                if (spec.GetProperty("scope").S("kind") != "record") { output.State = "inconclusive"; return output; }
                if (record == null || record.AmbiguousPlacement || record.SemanticClass == "ambiguous" || record.SemanticClass == "unrecognizedCandidate") { output.State = "inconclusive"; return output; }
                var values = new List<decimal>();
                foreach (var input in spec.A("inputRefs"))
                {
                    string? value = null;
                    switch (input.S("kind"))
                    {
                        case "literal": value = input.S("decimal"); output.InputRefs.Add("literal:" + value); break;
                        case "field":
                            output.InputRefs.Add(record.Id + ":" + input.S("semanticName"));
                            if (record.Fields.TryGetValue(input.S("semanticName"), out var f) && f.State == "present") value = f.Value;
                            break;
                        case "calculation": var dependency = Calculate(input.S("ruleRef"), record); output.InputRefs.Add(dependency.RuleId + "@" + scope); if (dependency.State == "pass") value = dependency.Value; break;
                    }
                    if (!decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)) { output.State = "inconclusive"; return output; } values.Add(amount);
                }
                try
                {
                    decimal total;
                    switch (spec.S("op"))
                    { case "add": case "sum": total = values.Sum(); break; case "subtract": total = values.Skip(1).Aggregate(values[0], (a, b) => a - b); break; default: throw new NotSupportedException("Unsupported calculation: " + spec.S("op")); }
                    output.Value = total.ToString("G29", CultureInfo.InvariantCulture); output.State = "pass";
                }
                catch (OverflowException) { output.State = "inconclusive"; }
                return output;
            }
            foreach (var calculation in calculations.Values)
            {
                if (calculation.GetProperty("scope").S("kind") == "record") foreach (var record in result.Records.Where(r => PositionedLayoutEngine.Select(calculation.GetProperty("memberSelector"), r))) Calculate(calculation.S("id"), record);
                else Calculate(calculation.S("id"), null);
            }
            foreach (var rule in definition.A("rules"))
            {
                if (!rule.B("enabled")) { result.ValidationResults.Add(new RuleResult { RuleId = rule.S("id"), Reason = "Disabled in the definition pending semantic reconciliation." }); continue; }
                if (rule.S("op") != "compare") throw new NotSupportedException("Unsupported validation operation: " + rule.S("op"));
                var lefts = result.Calculations.Where(c => c.RuleId == rule.S("calculatedRef")).ToArray();
                foreach (var left in lefts)
                {
                    var output = new RuleResult { RuleId = rule.S("id"), ScopeOccurrenceId = left.ScopeOccurrenceId, State = "inconclusive", CalculatedValue = left.Value };
                    var right = rule.Has("otherCalculatedRef") ? result.Calculations.SingleOrDefault(c => c.RuleId == rule.S("otherCalculatedRef") && c.ScopeOccurrenceId == left.ScopeOccurrenceId) : null;
                    output.PrintedValue = right?.Value;
                    if (left.State == "pass" && right?.State == "pass")
                    {
                        decimal difference = decimal.Parse(left.Value!, CultureInfo.InvariantCulture) - decimal.Parse(right.Value!, CultureInfo.InvariantCulture);
                        output.Difference = difference.ToString("G29", CultureInfo.InvariantCulture); output.State = Math.Abs(difference) <= decimal.Parse(rule.S("absoluteTolerance"), CultureInfo.InvariantCulture) ? "pass" : "fail";
                    }
                    else output.Reason = "Operands or scope coverage are unresolved; blank values are not zero.";
                    result.ValidationResults.Add(output);
                }
            }
        }
        void Add(JsonElement rule, string state, string reason) => result.ValidationResults.Add(new RuleResult { RuleId = rule.S("id"), State = state, Reason = reason });
    }
}
