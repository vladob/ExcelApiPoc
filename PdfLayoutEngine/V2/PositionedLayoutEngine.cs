using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PdfLayoutEngine.V2;
/// <summary>Common, deterministic v2 pipeline; the caller supplies the format adapter.</summary>
public sealed class PositionedLayoutEngine
{
    private readonly DefinitionCatalog catalog;
    public PositionedLayoutEngine(DefinitionCatalog catalog) { this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); }
    public EngineResult Process(string path, IPositionedDocumentAdapter adapter, EngineOptions? options = null, ExtractionLimits? limits = null, CancellationToken cancellationToken = default)
        => Process(adapter.Extract(path, limits, cancellationToken), options, cancellationToken);
    public EngineResult Process(PositionedDocument document, EngineOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new EngineOptions();
        if (!new[] { "recognition", "metadata", "records" }.Contains(options.Extraction) || !new[] { "none", "completeness", "internalConsistency" }.Contains(options.Validation)) throw new ArgumentException("Unknown processing level.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); timeout.CancelAfter(options.ExecutionTimeMs);
        var effectiveExtraction = options.Validation == "none" ? options.Extraction : "records";
        var result = new EngineResult { Format = document.Format, RequestedLevels = new ProcessingLevels { Extraction = options.Extraction, Validation = options.Validation }, EffectiveLevels = new ProcessingLevels { Extraction = effectiveExtraction, Validation = options.Validation }, Coverage = new ResultCoverage { AvailablePageCount = document.Pages.Count }, Provenance = new ResultProvenance { DocumentHash = document.DocumentHash } };
        result.Diagnostics.AddRange(document.Diagnostics);
        foreach (var item in catalog.Catalog.A("definitions")) result.Provenance.Definitions.Add(new DefinitionProvenance { Id = item.GetProperty("ref").S("id"), Version = item.GetProperty("ref").S("version"), Sha256 = item.S("sha256") });
        AddEvidence(document, result);
        var recognition = new Recognition(catalog); result.RecognitionDecisions = recognition.Run(document, result, timeout.Token);
        var layoutDecision = result.RecognitionDecisions.Single(d => d.Target == "layout");
        if (effectiveExtraction == "recognition") { result.ProcessingStatus = document.CompleteDecode && result.RecognitionDecisions.All(d => d.State == "confirmed") ? "success" : "incomplete"; return result; }
        if (layoutDecision.State != "confirmed") { Diagnose(result, "layoutUnresolved", "No unique layout satisfies recognition prerequisites."); return result; }
        var layout = catalog.Resolve(layoutDecision.Candidates[0].Id);
        if (!layout.Strings("formats").Contains(document.Format)) { result.ProcessingStatus = "unsupported"; Diagnose(result, "formatUnsupported", "Selected layout does not support this format."); return result; }
        var parsers = layout.A("parserRefs").SelectMany(r => catalog.Resolve(r).A("profiles")).ToDictionary(p => p.S("id"));
        var occurrences = new List<(ExtractedRecord record, JsonElement block, TransformPt transform, PositionedPage page)>();
        var assigned = new HashSet<string>();
        foreach (var page in document.Pages)
        {
            timeout.Token.ThrowIfCancellationRequested();
            if (!result.Coverage.InspectedPages.Contains(page.PhysicalPageIndex + 1)) result.Coverage.InspectedPages.Add(page.PhysicalPageIndex + 1);
            var fits = layout.A("variants").Select(v => (variant: v, fit: recognition.Fit(page, layout, v, timeout.Token))).Where(x => x.fit.State == "resolved").OrderByDescending(x => x.fit.LabelCoverage).ThenByDescending(x => x.fit.LabelMatchCount).ThenByDescending(x => x.fit.Inliers.Count).ThenBy(x => x.fit.ResidualPt).ToArray();
            if (fits.Length == 0) { Diagnose(result, "pageCalibrationUnresolved", $"Physical page {page.PhysicalPageIndex + 1} could not be calibrated."); continue; }
            if (fits.Length > 1 && Math.Abs(fits[0].fit.LabelCoverage - fits[1].fit.LabelCoverage) < .02 && fits[0].fit.LabelMatchCount == fits[1].fit.LabelMatchCount && fits[0].fit.Inliers.Count == fits[1].fit.Inliers.Count && Math.Abs(fits[0].fit.ResidualPt - fits[1].fit.ResidualPt) < .1)
            { Diagnose(result, "variantAmbiguous", $"Physical page {page.PhysicalPageIndex + 1} fits multiple variants."); continue; }
            var selected = fits[0]; string transformId = "calibration-p" + page.PhysicalPageIndex;
            AddTransform(result, transformId, selected.fit.Transform);
            Diagnose(result, "pageCalibration", $"Page {page.PhysicalPageIndex + 1}: variant {selected.variant.S("id")}, scale {selected.fit.Scale.ToString("G6", CultureInfo.InvariantCulture)}, residual {selected.fit.ResidualPt.ToString("G4", CultureInfo.InvariantCulture)} pt, {selected.fit.Inliers.Count} inliers.", "info");
            var blocks = effectiveExtraction == "records" ? OccurrenceDetector.Detect(page, selected.variant, selected.fit, options.MaximumCandidatesPerPage, timeout.Token) : new List<BlockOccurrence> { new BlockOccurrence { Block = selected.variant.A("blocks").Single(b => b.S("kind") == "pageHeader"), Transform = selected.fit.Transform, OriginY = page.HeightPt } };
            foreach (var occurrence in blocks)
            {
                var record = new ExtractedRecord { Id = $"p{page.PhysicalPageIndex}:b{occurrences.Count}", VariantRef = selected.variant.S("id"), BlockRef = occurrence.Block.S("id"), PageIndex = page.PhysicalPageIndex, OriginY = occurrence.OriginY, AmbiguousPlacement = occurrence.Ambiguous };
                var fields = occurrence.Block.A("fields"); var rectangles = fields.Select(f => occurrence.Transform.Apply(f.GetProperty("rectPt").Rect())).ToArray();
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i]; var value = ReadField(page, rectangles[i], parsers[field.S("parserRef")], occurrence.Block.GetProperty("fieldAssignment").N("paddingPt"), rectangles.Where((r, index) => index != i).ToArray());
                    Merge(record.Fields, field.S("semanticName"), value);
                }
                foreach (var label in occurrence.Block.A("labels"))
                {
                    var rect = occurrence.Transform.Apply(label.GetProperty("rectPt").Rect());
                    var tokens = page.Text.Where(t => rect.Contains(Center(t), 1.5)).ToArray();
                    if (!TextMatching.Compact(TextMatching.Assemble(tokens)).Contains(TextMatching.Compact(label.S("text").Replace("\\n", " ")))) continue;
                    foreach (var t in tokens) assigned.Add(t.Id);
                    MarkEvidence(result, tokens.Select(t => t.Id), record.Id, label.S("id"));
                }
                record.EvidenceRefs = record.Fields.Values.SelectMany(f => f.EvidenceRefs).Distinct().ToList();
                foreach (var id in record.EvidenceRefs) assigned.Add(id);
                MarkEvidence(result, record.EvidenceRefs, record.Id, occurrence.Block.GetProperty("fieldAssignment").S("id"));
                Classify(layout, record);
                if (occurrence.Ambiguous) { record.SemanticClass = "ambiguous"; record.ExportEligibility = "review"; record.AggregationMemberships.Clear(); Diagnose(result, "blockPlacementAmbiguous", record.Id + " has competing block templates."); }
                occurrences.Add((record, occurrence.Block, occurrence.Transform, page)); result.Records.Add(record);
            }
            ExtractMetadata(layout, selected.variant, page, selected.fit, occurrences.Where(o => o.page == page).ToList(), parsers, result);
        }
        if (!result.Metadata.ContainsKey("reportingEntity.cin")) result.Metadata.Add("reportingEntity.cin", new FieldValue());
        foreach (var metadata in result.Metadata.Where(m => m.Value.State == "present" && m.Key.StartsWith("period.", StringComparison.Ordinal)))
        {
            var value = metadata.Value.Value!; var code = value.Split('/')[0];
            var periodParser = parsers.Values.FirstOrDefault(p => p.S("op") == "accountingPeriod");
            if (periodParser.ValueKind != JsonValueKind.Undefined && !periodParser.Strings("observedCodes").Contains(code)) Diagnose(result, "unknownPeriodCode", metadata.Key + " retains unrecognized printed period code " + code + ".");
        }
        DeriveMetadata(layout, result);
        int? year = result.Metadata.TryGetValue("fiscalYear", out var fiscalYear) && fiscalYear.State == "present" && int.TryParse(fiscalYear.Value, out var y) ? y : (int?)null;
        if (year.HasValue)
        foreach (var occurrence in occurrences)
        foreach (var field in occurrence.block.A("fields").Where(f => parsers[f.S("parserRef")].S("op") == "exactDate"))
        {
            var value = occurrence.record.Fields[field.S("semanticName")];
            if (value.State != "unresolved" || value.RawText.Length == 0) continue;
            var parsed = ValueParser.Parse(value.RawText, parsers[field.S("parserRef")], true, year); parsed.EvidenceRefs = value.EvidenceRefs; parsed.DerivationInputs.Add("metadata:fiscalYear"); occurrence.record.Fields[field.S("semanticName")] = parsed;
        }
        if (effectiveExtraction == "records")
        {
            GroupProcessor.Apply(layout, result);
            result.Coverage.OrphanTokenCount = document.Pages.SelectMany(p => p.Text).Count(t => !string.IsNullOrWhiteSpace(t.Text) && !assigned.Contains(t.Id));
            result.Coverage.UnresolvedCandidateCount = result.Records.Count(r => r.SemanticClass == "unrecognizedCandidate" || r.SemanticClass == "ambiguous");
            if (result.Coverage.OrphanTokenCount > 0) Diagnose(result, "orphanTokens", $"{result.Coverage.OrphanTokenCount} text elements remain unassigned; their provenance is retained.");
            ValidationProcessor.Apply(catalog, layout, result, options.Validation, timeout.Token);
        }
        else result.Records.Clear();
        if (layout.S("status") != "approved") Diagnose(result, "reviewDefinitions", "Review definitions were explicitly enabled; results require review.", "info");
        result.Coverage.InspectedPages.Sort();
        result.ProcessingStatus = document.CompleteDecode && !result.Diagnostics.Any(d => d.Severity != "info") && result.ValidationResults.All(v => v.State != "fail" && v.State != "inconclusive") && result.Records.All(r => r.Fields.Values.All(f => f.State != "invalid" && f.State != "ambiguous")) ? "success" : "incomplete";
        return result;
    }
    internal static PointPt Center(PositionedText t) => new PointPt((t.Bounds.Left + t.Bounds.Right) / 2, t.Baseline.Y);
    internal static FieldValue ReadField(PositionedPage page, RectPt rect, JsonElement parser, double padding, RectPt[]? otherFields = null)
    {
        var tokens = page.Text.Where(t => rect.Contains(Center(t), padding) && (rect.Contains(Center(t)) || otherFields == null || !otherFields.Any(r => r.Contains(Center(t))))).ToArray();
        var value = ValueParser.Parse(TextMatching.Assemble(tokens), parser); value.EvidenceRefs = tokens.Select(t => t.Id).ToList();
        bool ambiguous = tokens.Any(t => t.IsClipped || (!t.HasGlyphGeometry || t.Text.Length > 1) && (t.Bounds.Left < rect.Left - padding || t.Bounds.Right > rect.Right + padding) || otherFields != null && otherFields.Any(r => r.Contains(Center(t), 0) && rect.Contains(Center(t), 0)));
        if (ambiguous) { if (value.Value != null) value.Alternatives.Add(value.Value); value.State = "ambiguous"; value.Value = null; }
        return value;
    }
    internal static void Merge(Dictionary<string, FieldValue> target, string name, FieldValue incoming)
    {
        if (!target.TryGetValue(name, out var current) || current.State == "unresolved" && current.RawText.Length == 0 || current.State == "blank" && incoming.State == "present") { target[name] = incoming; return; }
        if (incoming.State == "blank" || incoming.State == "unresolved" && incoming.RawText.Length == 0) return;
        current.EvidenceRefs = current.EvidenceRefs.Concat(incoming.EvidenceRefs).Distinct().ToList();
        if (current.State == incoming.State && current.Value == incoming.Value) return;
        current.Alternatives = current.Alternatives.Concat(incoming.Alternatives).Concat(new[] { current.Value, incoming.Value }.Where(v => v != null).Select(v => v!)).Distinct().ToList(); current.State = "ambiguous"; current.Value = null;
    }
    internal static bool Predicate(JsonElement predicate, ExtractedRecord record)
    {
        switch (predicate.S("op"))
        {
            case "all": return predicate.A("conditions").All(p => Predicate(p, record));
            case "any": return predicate.A("conditions").Any(p => Predicate(p, record));
            case "not": return !Predicate(predicate.GetProperty("condition"), record);
            case "blockKind": return predicate.Strings("blockRefs").Contains(record.BlockRef);
            case "fieldState": return predicate.Strings("states").Contains(record.Fields.TryGetValue(predicate.S("fieldRef"), out var field) ? field.State : "unresolved");
            case "textMatch": return record.Fields.TryGetValue(predicate.S("fieldRef"), out var text) && TextMatching.Matches(text.RawText, predicate.GetProperty("match"));
            case "groupContext": throw new NotSupportedException("Group-context classification requires a resolved group phase and is not enabled in the migrated definitions.");
            default: throw new NotSupportedException("Unknown predicate operation.");
        }
    }
    internal static bool Select(JsonElement selector, ExtractedRecord record) => (selector.Strings("semanticClasses").Length == 0 || selector.Strings("semanticClasses").Contains(record.SemanticClass)) && (selector.Strings("aggregationMemberships").Length == 0 || selector.Strings("aggregationMemberships").Intersect(record.AggregationMemberships).Any()) && (selector.S("level") == "any" || selector.S("level") == record.Level);
    private static void Classify(JsonElement layout, ExtractedRecord record)
    {
        var matches = layout.A("classification").Where(r => r.B("enabled") && Predicate(r.GetProperty("when"), record)).ToArray();
        if (matches.Length == 0) return;
        if (matches.Select(r => r.S("semanticClass") + ":" + r.S("exportEligibility") + ":" + r.S("level")).Distinct().Count() > 1) { record.SemanticClass = "ambiguous"; return; }
        record.SemanticClass = matches[0].S("semanticClass"); record.ExportEligibility = matches[0].S("exportEligibility"); record.Level = matches[0].S("level"); record.AggregationMemberships = matches.SelectMany(m => m.Strings("aggregationMemberships")).Distinct().ToList();
    }
    private static void ExtractMetadata(JsonElement layout, JsonElement variant, PositionedPage page, PageCalibration calibration, List<(ExtractedRecord record, JsonElement block, TransformPt transform, PositionedPage page)> occurrences, Dictionary<string, JsonElement> parsers, EngineResult result)
    {
        foreach (var definition in layout.A("metadata"))
        {
            var locator = definition.GetProperty("locator"); var name = definition.S("semanticName");
            if (!result.Metadata.ContainsKey(name)) result.Metadata.Add(name, new FieldValue());
            if (locator.S("kind") == "unresolved") continue;
            if (locator.Has("variantRef") && locator.S("variantRef") != variant.S("id") || !Recognition.SelectPage(locator.GetProperty("pages"), page.PhysicalPageIndex, result.Coverage.AvailablePageCount)) continue;
            var values = new List<FieldValue>();
            if (locator.S("kind") == "blockField")
            {
                foreach (var occurrence in occurrences.Where(o => o.block.S("id") == locator.S("blockRef")))
                {
                    var field = occurrence.block.A("fields").Single(f => f.S("id") == locator.S("fieldRef"));
                    values.Add(ReadField(page, occurrence.transform.Apply(field.GetProperty("rectPt").Rect()), parsers[definition.S("parserRef")], 1.5));
                }
            }
            else
            {
                RectPt rect;
                switch (locator.S("kind"))
                {
                    case "calibratedRegion": rect = calibration.Transform.Apply(locator.GetProperty("rectPt").Rect()); break;
                    case "normalizedPage": var r = locator.GetProperty("regionFraction").Rect(); rect = new RectPt(r.Left * page.WidthPt, r.Bottom * page.HeightPt, r.Right * page.WidthPt, r.Top * page.HeightPt); break;
                    case "relativeToAnchor": var anchor = variant.A("anchors").Single(a => a.S("id") == locator.S("anchorRef")).GetProperty("pointPt").Point(); rect = new TransformPt(1, 0, 0, 1, anchor.X, anchor.Y).Then(calibration.Transform).Apply(locator.GetProperty("offsetRectPt").Rect()); break;
                    default: throw new NotSupportedException("Unsupported metadata locator: " + locator.S("kind"));
                }
                values.Add(ReadField(page, rect, parsers[definition.S("parserRef")], locator.N("paddingPt")));
            }
            // Printed page labels are occurrence-specific, not conflicting document metadata.
            if (name == "printedPageLabel")
            { foreach (var value in values) Merge(result.Metadata, name + ".p" + (page.PhysicalPageIndex + 1), value); continue; }
            foreach (var value in values) { Merge(result.Metadata, name, value); MarkEvidence(result, value.EvidenceRefs, null, definition.S("id")); }
        }
    }
    private static void DeriveMetadata(JsonElement layout, EngineResult result)
    {
        foreach (var rule in layout.A("metadataDerivations").Where(r => r.B("enabled")))
        {
            var inputs = rule.Strings("inputMetadataNames");
            var values = inputs.Select(name => result.Metadata.TryGetValue(name, out var v) && v.State == "present" ? v.Value : null).ToArray();
            var years = values.Select(v => v != null && v.Contains("/") && int.TryParse(v.Substring(v.LastIndexOf('/') + 1), out var year) ? (int?)year : null).ToArray();
            result.Metadata[rule.S("outputMetadataName")] = years.All(y => y.HasValue) && years.Distinct().Count() == 1 ? FieldValue.Derived(years[0]!.Value.ToString(CultureInfo.InvariantCulture), inputs.Select(n => "metadata:" + n).ToArray()) : new FieldValue();
        }
    }
    internal static void Diagnose(EngineResult result, string code, string message, string severity = "warning") => result.Diagnostics.Add(new EngineDiagnostic { Code = code, Message = message, Severity = severity });
    private static void AddTransform(EngineResult result, string id, TransformPt transform) => result.Provenance.Transforms.Add(new TransformProvenance { Id = id, SourceToNormalized = transform.ToArray(), NormalizedToSource = transform.Inverse().ToArray() });
    private static void AddEvidence(PositionedDocument document, EngineResult result)
    {
        foreach (var page in document.Pages)
        {
            var transforms = new Dictionary<string, string>();
            foreach (var text in page.Text)
            {
                var key = string.Join(",", text.SourceToNormalized.ToArray().Select(n => n.ToString("R", CultureInfo.InvariantCulture)));
                if (!transforms.TryGetValue(key, out var id)) { id = "p" + page.PhysicalPageIndex + ":transform" + transforms.Count; transforms.Add(key, id); AddTransform(result, id, text.SourceToNormalized); }
                result.Provenance.Evidence.Add(new EvidenceItem { Id = text.Id, PhysicalPageIndex = page.PhysicalPageIndex + 1, SourcePageId = page.SourcePageId, SourceElementRefs = new List<string> { text.Id }, RawText = text.Text, RawGeometry = new List<double[]> { text.RawBounds.ToArray() }, NormalizedGeometry = new List<double[]> { text.Bounds.ToArray() }, TransformRefs = new List<string> { id } });
            }
        }
    }
    private static void MarkEvidence(EngineResult result, IEnumerable<string> ids, string? block, string rule)
    {
        var set = new HashSet<string>(ids);
        foreach (var evidence in result.Provenance.Evidence.Where(e => set.Contains(e.Id))) { evidence.RuleIds.Add(rule); if (block != null) evidence.BlockOccurrenceId = block; }
    }
}
