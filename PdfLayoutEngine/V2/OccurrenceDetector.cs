using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PdfLayoutEngine.V2;
internal sealed class BlockOccurrence
{
    public JsonElement Block { get; set; }
    public TransformPt Transform { get; set; }
    public double OriginY { get; set; }
    public double Score { get; set; }
    public List<string> Tokens { get; set; } = new List<string>();
    public bool Ambiguous { get; set; }
}
internal static class OccurrenceDetector
{
    public static List<BlockOccurrence> Detect(PositionedPage page, JsonElement variant, PageCalibration calibration, int maximum, CancellationToken token)
    {
        var occurrences = new List<BlockOccurrence>(); var header = variant.A("blocks").First(b => b.S("kind") == "pageHeader");
        occurrences.Add(new BlockOccurrence { Block = header, Transform = calibration.Transform, OriginY = page.HeightPt, Tokens = Covered(page, header, calibration.Transform).ToList() });
        var headerIds = new HashSet<string>(occurrences[0].Tokens);
        var headerBottom = header.A("lines").SelectMany(l => l.A("pointsPt")).Select(p => calibration.Transform.Apply(p.Point()).Y).DefaultIfEmpty(page.HeightPt * .85).Min();
        var available = page.Text.Where(t => !headerIds.Contains(t.Id) && t.Baseline.Y < headerBottom + 1 && !string.IsNullOrWhiteSpace(t.Text)).ToArray();
        var rows = TextMatching.Rows(available).ToArray(); var candidates = new List<BlockOccurrence>();
        foreach (var block in variant.A("blocks").Where(b => b.S("frame") == "blockLocal" && !b.B("emptyTemplate")))
        {
            token.ThrowIfCancellationRequested(); var origins = new List<double>(); var scale = calibration.Scale; var padding = block.GetProperty("fieldAssignment").N("paddingPt") * scale;
            var templates = block.A("lines").Select(l => l.A("pointsPt").Select(p => p.Point()).ToArray()).Where(p => p.Length == 2).ToArray();
            // Every ruling-derived origin is independent of previous rows and nominal advance.
            foreach (var line in page.Lines.Where(l => Math.Max(l.Start.Y, l.End.Y) < headerBottom + 2))
            foreach (var expected in templates)
            {
                var dx = (expected[1].X - expected[0].X) * scale; var dy = (expected[1].Y - expected[0].Y) * scale;
                foreach (var reverse in new[] { false, true })
                {
                    var a = reverse ? line.End : line.Start; var b = reverse ? line.Start : line.End;
                    if (Math.Abs(a.X - (expected[0].X * scale + calibration.Tx)) > 1.5 || Math.Abs(b.X - a.X - dx) > 1.5 || Math.Abs(b.Y - a.Y - dy) > 1.5) continue;
                    origins.Add(a.Y - expected[0].Y * scale);
                }
            }
            // Text candidates form a union with ruling candidates. They survive invalid values.
            foreach (var row in rows)
            foreach (var field in block.A("fields").Concat(block.A("labels")))
            {
                var r = field.GetProperty("rectPt").Rect();
                if (row.Any(t => t.Baseline.X >= r.Left * scale + calibration.Tx - padding && t.Baseline.X <= r.Right * scale + calibration.Tx + padding))
                    origins.Add(row.Average(t => t.Baseline.Y) - (r.Bottom + r.Height * .22) * scale);
            }
            foreach (var origin in origins.OrderByDescending(y => y).Aggregate(new List<double>(), (list, y) => { if (list.All(v => Math.Abs(v - y) > 1.2)) list.Add(y); return list; }))
            {
                token.ThrowIfCancellationRequested(); if (candidates.Count > maximum) throw new InvalidOperationException("Candidate budget exceeded.");
                if (origin > headerBottom + 2 || origin < 0) continue;
                var transform = new TransformPt(scale, 0, 0, scale, calibration.Tx, origin);
                var covered = Covered(page, block, transform).Where(id => !headerIds.Contains(id)).ToList();
                if (covered.Count == 0) continue;
                int labels = 0, expectedLabels = 0;
                foreach (var label in block.A("labels").Where(l => TextMatching.Compact(l.S("text")).Length > 1))
                {
                    expectedLabels++; var r = transform.Apply(label.GetProperty("rectPt").Rect());
                    var text = TextMatching.Assemble(page.Text.Where(t => r.Contains(new PointPt((t.Bounds.Left + t.Bounds.Right) / 2, t.Baseline.Y), padding)));
                    if (TextMatching.Compact(text).Contains(TextMatching.Compact(label.S("text").Replace("\\n", " ")))) labels++;
                }
                // A footer's fixed labels distinguish it from data occupying the same columns.
                if (expectedLabels > 0 && labels == 0 && block.S("kind") != "repeatingRecord") continue;
                int matchedLines = templates.Count(expected => page.Lines.Any(actual => EndpointsMatch(transform.Apply(expected[0]), transform.Apply(expected[1]), actual)));
                var bounds = block.A("fields").Concat(block.A("labels")).Select(f => transform.Apply(f.GetProperty("rectPt").Rect())).ToArray();
                if (bounds.Length == 0) continue;
                double top = bounds.Max(r => r.Top), bottom = bounds.Min(r => r.Bottom);
                var band = available.Where(t => t.Baseline.Y <= top + padding && t.Baseline.Y >= bottom - padding).ToArray();
                int missed = band.Count(t => !covered.Contains(t.Id));
                var fieldRects = block.A("fields").Select(f => transform.Apply(f.GetProperty("rectPt").Rect())).ToArray();
                int splitWords = 0;
                foreach (var row in TextMatching.Rows(band))
                for (int i = 1; i < row.Length; i++)
                {
                    var a = row[i - 1]; var b = row[i];
                    if (string.IsNullOrWhiteSpace(a.Text) || string.IsNullOrWhiteSpace(b.Text) || b.Bounds.Left - a.Bounds.Right > 1.2) continue;
                    int first = Array.FindIndex(fieldRects, r => r.Contains(PositionedLayoutEngine.Center(a)));
                    int second = Array.FindIndex(fieldRects, r => r.Contains(PositionedLayoutEngine.Center(b)));
                    if (first >= 0 && second >= 0 && first != second) splitWords++;
                }
                double score = -splitWords * 5 + labels * 8 + (templates.Length == 0 ? 0 : 5.0 * matchedLines / templates.Length) + Math.Min(4, covered.Count / 10.0) - missed * .15;
                // Require actual text in at least one field or a matched fixed label.
                candidates.Add(new BlockOccurrence { Block = block, Transform = transform, OriginY = origin, Score = score, Tokens = covered });
            }
        }
        var used = new HashSet<string>(headerIds);
        foreach (var candidate in candidates.OrderByDescending(c => c.Score).ThenByDescending(c => c.OriginY))
        {
            if (candidate.Tokens.Count == 0 || candidate.Tokens.Count(t => used.Contains(t)) > candidate.Tokens.Count * .25) continue;
            var alternatives = candidates.Where(c => c != candidate && c.Block.S("id") != candidate.Block.S("id") && Math.Abs(c.Score - candidate.Score) < .5 && c.Tokens.Intersect(candidate.Tokens).Count() > candidate.Tokens.Count * .75).ToArray();
            candidate.Ambiguous = alternatives.Length > 0;
            occurrences.Add(candidate); foreach (var id in candidate.Tokens) used.Add(id);
        }
        return occurrences.OrderByDescending(c => c.OriginY).ToList();
    }
    private static bool EndpointsMatch(PointPt a, PointPt b, PositionedLine line) => a.Distance(line.Start) < 1.5 && b.Distance(line.End) < 1.5 || a.Distance(line.End) < 1.5 && b.Distance(line.Start) < 1.5;
    private static IEnumerable<string> Covered(PositionedPage page, JsonElement block, TransformPt transform)
    {
        double padding = block.GetProperty("fieldAssignment").N("paddingPt");
        var rects = block.A("fields").Concat(block.A("labels")).Select(f => transform.Apply(f.GetProperty("rectPt").Rect())).ToArray();
        if (rects.Length == 0) return Array.Empty<string>();
        double bottom = rects.Min(r => r.Bottom) - padding, top = rects.Max(r => r.Top) + padding;
        return page.Text.Where(t => t.Baseline.Y >= bottom && t.Baseline.Y <= top && !string.IsNullOrWhiteSpace(t.Text) && rects.Any(r => r.Contains(new PointPt((t.Bounds.Left + t.Bounds.Right) / 2, t.Baseline.Y), padding))).Select(t => t.Id);
    }
}
