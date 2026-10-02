using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PdfLayoutEngine.V2;
internal sealed class Recognition
{
    private readonly DefinitionCatalog catalog;
    private readonly Dictionary<string, PageCalibration> fits = new Dictionary<string, PageCalibration>();
    public Recognition(DefinitionCatalog catalog) { this.catalog = catalog; }
    public PageCalibration Fit(PositionedPage page, JsonElement layout, JsonElement variant, CancellationToken token)
    {
        var key = page.PhysicalPageIndex + ":" + layout.S("id") + ":" + variant.S("id");
        if (!fits.TryGetValue(key, out var fit)) fits.Add(key, fit = CalibrationFitter.Fit(page, variant, token));
        return fit;
    }
    public List<RecognitionDecision> Run(PositionedDocument document, EngineResult result, CancellationToken token)
    {
        var definitions = catalog.Identification.ToArray();
        var initial = definitions.Max(d => (int)d.GetProperty("scan").GetProperty("initialPages").N("maximumPages"));
        var maximum = definitions.Max(d => (int)d.GetProperty("scan").GetProperty("additionalPages").N("maximumPages"));
        var decisions = Evaluate(Math.Min(initial, document.Pages.Count));
        if (decisions.Any(d => d.State != "confirmed") && document.Pages.Count > initial) decisions = Evaluate(Math.Min(maximum, document.Pages.Count));
        return decisions;
        List<RecognitionDecision> Evaluate(int budget)
        {
            var rows = new List<(JsonElement definition, JsonElement decision, RecognitionCandidate candidate, HashSet<string> matched)>();
            foreach (var definition in definitions)
            {
                var hits = new Dictionary<string, List<string>>();
                foreach (var signal in definition.A("signals").Where(s => s.B("enabled")))
                {
                    token.ThrowIfCancellationRequested(); var evidence = new List<string>();
                    foreach (var page in document.Pages.Take(budget))
                    {
                        if (!result.Coverage.InspectedPages.Contains(page.PhysicalPageIndex + 1)) result.Coverage.InspectedPages.Add(page.PhysicalPageIndex + 1);
                        var locator = signal.GetProperty("locator"); if (!SelectPage(locator.GetProperty("pages"), page.PhysicalPageIndex, document.Pages.Count)) continue;
                        var tokens = page.Text.AsEnumerable();
                        if (locator.S("kind") == "normalizedPage")
                        { var region = locator.GetProperty("regionFraction").Rect(); var rect = new RectPt(region.Left * page.WidthPt, region.Bottom * page.HeightPt, region.Right * page.WidthPt, region.Top * page.HeightPt); tokens = tokens.Where(t => rect.Contains(t.Baseline)); }
                        var selected = tokens.ToArray(); bool match;
                        switch (signal.S("op"))
                        {
                            case "textMatch": case "markerMatch": match = TextMatching.Matches(TextMatching.Assemble(selected), signal.GetProperty("match")); break;
                            case "orderedLabels": match = Ordered(selected, signal); break;
                            case "geometryFit":
                                var layout = catalog.Resolve(signal.GetProperty("layoutRef"));
                                match = layout.A("variants").Where(v => signal.Strings("variantRefs").Contains(v.S("id"))).Any(v => Fit(page, layout, v, token).State == "resolved"); break;
                            default: throw new NotSupportedException("Unsupported recognition operation: " + signal.S("op"));
                        }
                        if (match) { evidence.AddRange(selected.Where(t => !string.IsNullOrWhiteSpace(t.Text)).Select(t => t.Id)); break; }
                    }
                    if (evidence.Count > 0) hits.Add(signal.S("id"), evidence);
                }
                foreach (var decision in definition.A("decisions"))
                {
                    var signals = definition.A("signals").Where(s => s.S("target") == decision.S("target") && s.S("candidate") == decision.S("candidate") && hits.ContainsKey(s.S("id"))).ToArray();
                    double score = signals.GroupBy(s => s.S("evidenceGroup")).Sum(g => Math.Min(definition.A("evidenceGroups").Single(e => e.S("id") == g.Key).N("scoreCap"), g.Sum(s => s.N("weight") * (s.S("polarity") == "oppose" ? -1 : 1))));
                    rows.Add((definition, decision, new RecognitionCandidate { Id = decision.S("candidate"), Score = score, EvidenceRefs = signals.SelectMany(s => hits[s.S("id")]).Distinct().ToList() }, new HashSet<string>(hits.Keys)));
                }
            }
            var decisions = new List<RecognitionDecision>();
            foreach (var target in new[] { "category", "producer", "layout" })
            {
                // Multiple identification documents can describe the same category/producer.
                // Use the strongest complete evidence bundle, never sum duplicates across definitions.
                var candidates = rows.Where(r => r.decision.S("target") == target).GroupBy(r => r.candidate.Id).Select(g => g.OrderByDescending(r => r.candidate.Score).First()).OrderByDescending(r => r.candidate.Score).ThenBy(r => r.candidate.Id, StringComparer.Ordinal).ToArray();
                var output = new RecognitionDecision { Target = target, Candidates = candidates.Where(c => c.candidate.Score > 0).Select(c => c.candidate).ToList() };
                if (candidates.Length > 0 && candidates[0].candidate.Score > 0)
                {
                    var best = candidates[0]; var rule = best.decision;
                    bool prerequisites = rule.Strings("requiredSignals").All(best.matched.Contains) && !rule.Strings("vetoSignals").Any(best.matched.Contains) && rule.A("requiredDecisions").All(r => decisions.Any(d => d.Target == r.S("target") && d.State == "confirmed" && d.Candidates[0].Id == r.S("candidate")));
                    bool threshold = best.candidate.Score >= rule.N("minimumScore");
                    bool lead = best.candidate.Score - (candidates.Length > 1 ? candidates[1].candidate.Score : 0) >= rule.N("minimumLead");
                    output.State = threshold && prerequisites && lead ? "confirmed" : !lead ? "ambiguous" : "tentative";
                }
                decisions.Add(output);
            }
            return decisions;
        }
    }
    internal static bool SelectPage(JsonElement spec, int index, int count)
    {
        if (index >= spec.N("maximumPages") && spec.S("selection") != "indices" && spec.S("selection") != "last") return false;
        switch (spec.S("selection")) { case "all": case "first": return true; case "last": return index >= count - spec.N("maximumPages"); case "indices": return spec.A("indices").Any(i => i.GetInt32() == index + 1); default: return false; }
    }
    private static bool Ordered(PositionedText[] text, JsonElement signal)
    {
        var found = new List<PointPt[]>();
        foreach (var label in signal.A("labels"))
        {
            var positions = new List<PointPt>();
            foreach (var row in TextMatching.Rows(text))
            {
                var compact = string.Concat(row.Select(t => TextMatching.Compact(t.Text)));
                var needle = TextMatching.Compact(label.S("value"));
                int offset = compact.IndexOf(needle, StringComparison.Ordinal);
                while (offset >= 0)
                {
                    int n = 0, start = 0, end = 0;
                    for (int j = 0; j < row.Length; j++)
                    {
                        int next = n + TextMatching.Compact(row[j].Text).Length;
                        if (n <= offset && next > offset) start = j;
                        if (n < offset + needle.Length && next >= offset + needle.Length) { end = j; break; }
                        n = next;
                    }
                    if (TextMatching.Matches(TextMatching.Assemble(row.Skip(start).Take(end - start + 1)), label)) positions.Add(row[start].Baseline);
                    offset = compact.IndexOf(needle, offset + 1, StringComparison.Ordinal);
                }
            }
            found.Add(positions.ToArray());
        }
        bool Follow(int i, PointPt previous, PointPt first)
        {
            if (i == found.Count) return true;
            return found[i].Any(p => (signal.S("axis") == "x" ? p.X > previous.X && Math.Abs(p.Y - first.Y) <= signal.N("maximumOrthogonalGapPt") : p.Y < previous.Y && Math.Abs(p.X - first.X) <= signal.N("maximumOrthogonalGapPt")) && Follow(i + 1, p, first));
        }
        return found.Count > 0 && found[0].Any(p => Follow(1, p, p));
    }
}
