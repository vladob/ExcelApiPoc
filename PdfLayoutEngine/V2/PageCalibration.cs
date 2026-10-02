using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace PdfLayoutEngine.V2;
public sealed class PageCalibration
{
    public string State { get; set; } = "unresolved";
    public double Scale { get; set; } = 1;
    public double Tx { get; set; }
    public double Ty { get; set; }
    public double ResidualPt { get; set; }
    public List<string> Inliers { get; set; } = new List<string>();
    public List<string> RejectedAnchors { get; set; } = new List<string>();
    public int LabelMatchCount { get; set; }
    public int ExpectedLabelCount { get; set; }
    public double LabelCoverage => ExpectedLabelCount == 0 ? 0 : (double)LabelMatchCount / ExpectedLabelCount;
    public bool HypothesisLimitReached { get; set; }
    public TransformPt Transform => new TransformPt(Scale, 0, 0, Scale, Tx, Ty);
}
public static class CalibrationFitter
{
    public static PageCalibration Fit(PositionedPage page, JsonElement variant, CancellationToken cancellationToken = default)
    {
        var config = variant.GetProperty("calibration");
        var anchors = variant.A("anchors").Where(a => config.Strings("anchorRefs").Contains(a.S("id"))).ToArray();
        var nominal = anchors.Select(a => a.GetProperty("pointPt").Point()).ToArray();
        var lines = page.Lines.Where(l => l.Start.Distance(l.End) > 1).ToArray();
        var observed = lines.SelectMany(l => new[] { l.Start, l.End }).GroupBy(p => (Math.Round(p.X, 3), Math.Round(p.Y, 3))).Select(g => g.First()).ToArray();
        var scaleBounds = config.A("scaleRange"); var tolerance = config.N("maximumResidualPt"); var max = (int)config.N("maximumHypotheses");
        var hypotheses = new List<PageCalibration>(); var seen = new HashSet<string>(); var attempted = 0;
        // Anchor pairs are limited to a shared source ruling; ordering by span gives
        // a stable broad baseline without treating page dimensions as scale evidence.
        var pairs = (from a in Enumerable.Range(0, anchors.Length) from b in Enumerable.Range(a + 1, anchors.Length - a - 1)
                     where anchors[a].S("lineRef") == anchors[b].S("lineRef") && nominal[a].Distance(nominal[b]) >= config.N("minimumAnchorSeparationPt")
                     orderby nominal[a].Distance(nominal[b]) descending select (a, b)).ToArray();
        foreach (var pair in pairs)
        foreach (var line in lines.OrderByDescending(l => l.Start.Distance(l.End)))
        foreach (var reverse in new[] { false, true })
        {
            cancellationToken.ThrowIfCancellationRequested();
            var n1 = nominal[pair.a]; var n2 = nominal[pair.b]; var p1 = reverse ? line.End : line.Start; var p2 = reverse ? line.Start : line.End;
            var dx = n2.X - n1.X; var dy = n2.Y - n1.Y; var length2 = dx * dx + dy * dy;
            var scale = ((p2.X - p1.X) * dx + (p2.Y - p1.Y) * dy) / length2;
            if (scale < scaleBounds[0].GetDouble() || scale > scaleBounds[1].GetDouble()) continue;
            var tx = p1.X - scale * n1.X; var ty = p1.Y - scale * n1.Y;
            if (new PointPt(scale * n2.X + tx, scale * n2.Y + ty).Distance(p2) > tolerance) continue;
            var key = $"{Math.Round(scale, 5)}:{Math.Round(tx, 2)}:{Math.Round(ty, 2)}";
            if (!seen.Add(key)) continue;
            if (++attempted > max) goto Finished;
            var transform = new TransformPt(scale, 0, 0, scale, tx, ty); var matches = new List<(int index, PointPt observed, double residual)>(); var used = new HashSet<int>();
            for (var i = 0; i < nominal.Length; i++)
            {
                int nearest = -1; double distance = double.MaxValue; var predicted = transform.Apply(nominal[i]);
                for (int j = 0; j < observed.Length; j++)
                {
                    if (used.Contains(j)) continue;
                    double d = predicted.Distance(observed[j]);
                    if (d < distance) { distance = d; nearest = j; }
                }
                if (nearest < 0 || distance > tolerance) continue;
                used.Add(nearest); matches.Add((i, observed[nearest], distance));
            }
            if (matches.Count < config.N("minimumInliers")) continue;
            var span = config.A("minimumSpanPt");
            if (matches.Max(x => nominal[x.index].X) - matches.Min(x => nominal[x.index].X) < span[0].GetDouble() || matches.Max(x => nominal[x.index].Y) - matches.Min(x => nominal[x.index].Y) < span[1].GetDouble()) continue;
            // Least-squares refinement over all distinct inlier correspondences.
            var nx = matches.Average(x => nominal[x.index].X); var ny = matches.Average(x => nominal[x.index].Y); var ox = matches.Average(x => x.observed.X); var oy = matches.Average(x => x.observed.Y);
            var denominator = matches.Sum(x => Math.Pow(nominal[x.index].X - nx, 2) + Math.Pow(nominal[x.index].Y - ny, 2));
            scale = matches.Sum(x => (nominal[x.index].X - nx) * (x.observed.X - ox) + (nominal[x.index].Y - ny) * (x.observed.Y - oy)) / denominator;
            tx = ox - scale * nx; ty = oy - scale * ny; transform = new TransformPt(scale, 0, 0, scale, tx, ty);
            var residual = matches.Max(x => transform.Apply(nominal[x.index]).Distance(x.observed));
            if (residual > tolerance || scale < scaleBounds[0].GetDouble() || scale > scaleBounds[1].GetDouble()) continue;
            var header = variant.A("blocks").First(b => b.S("kind") == "pageHeader");
            var labels = header.A("labels").Where(l => l.S("text").Trim().Length > 3).ToArray();
            var labelMatches = labels.Count(l => TextMatching.Compact(TextMatching.Assemble(page.Text.Where(t => transform.Apply(l.GetProperty("rectPt").Rect()).Contains(t.Baseline, 1.5))))
                .Contains(TextMatching.Compact(l.S("text").Replace("\\n", " "))));
            if (labels.Length > 0 && labelMatches < Math.Max(2, Math.Ceiling(labels.Length * .65))) continue;
            hypotheses.Add(new PageCalibration { State = "resolved", LabelMatchCount = labelMatches, ExpectedLabelCount = labels.Length, Scale = scale, Tx = tx, Ty = ty, ResidualPt = residual, Inliers = matches.Select(x => anchors[x.index].S("id")).ToList(), RejectedAnchors = anchors.Where((x, i) => matches.All(m => m.index != i)).Select(x => x.S("id")).ToList() });
        }
        Finished:
        var ordered = hypotheses.OrderByDescending(x => x.Inliers.Count).ThenBy(x => x.ResidualPt).ToArray();
        if (ordered.Length == 0) return new PageCalibration { HypothesisLimitReached = attempted > max };
        var best = ordered[0]; best.HypothesisLimitReached = attempted > max;
        if (best.HypothesisLimitReached || ordered.Skip(1).Any(x => x.Inliers.Count == best.Inliers.Count && Math.Abs(x.ResidualPt - best.ResidualPt) < 0.1 && (Math.Abs(x.Tx - best.Tx) > tolerance || Math.Abs(x.Ty - best.Ty) > tolerance || Math.Abs(x.Scale - best.Scale) > 0.005))) best.State = "ambiguous";
        return best;
    }
}
