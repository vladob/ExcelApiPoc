using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfLayoutEngine.Simple;

public readonly struct PointPt
{
    public PointPt(double x, double y) { X = x; Y = y; }
    public double X { get; }
    public double Y { get; }
    public double Distance(PointPt other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}

public readonly struct RectPt
{
    public RectPt(double left, double bottom, double right, double top)
    {
        if (new[] { left, bottom, right, top }.Any(x => double.IsNaN(x) || double.IsInfinity(x)) || right < left || top < bottom)
            throw new ArgumentException("Invalid finite rectangle.");
        Left = left; Bottom = bottom; Right = right; Top = top;
    }
    public double Left { get; }
    public double Bottom { get; }
    public double Right { get; }
    public double Top { get; }
    public double Width => Right - Left;
    public double Height => Top - Bottom;
    public bool Contains(PointPt point, double padding = 0) => point.X >= Left - padding && point.X <= Right + padding && point.Y >= Bottom - padding && point.Y <= Top + padding;
    public bool Intersects(RectPt r) => Left <= r.Right && Right >= r.Left && Bottom <= r.Top && Top >= r.Bottom;
    public double[] ToArray() => new[] { Left, Bottom, Right, Top };
}

/// <summary>Affine transform: x'=A*x+C*y+E, y'=B*x+D*y+F.</summary>
public readonly struct TransformPt
{
    public TransformPt(double a, double b, double c, double d, double e, double f)
    { A = a; B = b; C = c; D = d; E = e; F = f; }
    public double A { get; } public double B { get; } public double C { get; }
    public double D { get; } public double E { get; } public double F { get; }
    public static TransformPt Identity => new TransformPt(1, 0, 0, 1, 0, 0);
    public PointPt Apply(PointPt p) => new PointPt(A * p.X + C * p.Y + E, B * p.X + D * p.Y + F);
    public RectPt Apply(RectPt r)
    {
        var points = new[] { Apply(new PointPt(r.Left, r.Bottom)), Apply(new PointPt(r.Right, r.Bottom)), Apply(new PointPt(r.Right, r.Top)), Apply(new PointPt(r.Left, r.Top)) };
        return new RectPt(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
    }
    public TransformPt Then(TransformPt t) => new TransformPt(t.A * A + t.C * B, t.B * A + t.D * B, t.A * C + t.C * D, t.B * C + t.D * D, t.A * E + t.C * F + t.E, t.B * E + t.D * F + t.F);
    public TransformPt Inverse()
    {
        var det = A * D - B * C;
        if (Math.Abs(det) < 1e-12) throw new ArgumentException("Singular transform.");
        return new TransformPt(D / det, -B / det, -C / det, A / det, (C * F - D * E) / det, (B * E - A * F) / det);
    }
    public double[] ToArray() => new[] { A, B, C, D, E, F };
}

public sealed class PositionedText
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public RectPt RawBounds { get; set; }
    public RectPt Bounds { get; set; }
    public PointPt Baseline { get; set; }
    public string Font { get; set; } = "";
    public double FontSize { get; set; }
    public int FontWeight { get; set; }
    public PointPt AdvanceEnd { get; set; }
    public string RunId { get; set; } = "";
    public bool HasGlyphGeometry { get; set; }
    public bool IsClipped { get; set; }
    public TransformPt SourceToNormalized { get; set; } = TransformPt.Identity;
}
public sealed class PositionedLine
{
    public string Id { get; set; } = "";
    public PointPt Start { get; set; }
    public PointPt End { get; set; }
}
public sealed class PositionedPage
{
    public int PhysicalPageIndex { get; set; }
    public string SourcePageId { get; set; } = "";
    public double WidthPt { get; set; }
    public double HeightPt { get; set; }
    public TransformPt SourceToNormalized { get; set; } = TransformPt.Identity;
    public List<PositionedText> Text { get; } = new List<PositionedText>();
    public List<PositionedLine> Lines { get; } = new List<PositionedLine>();
}
public sealed class PositionedDocument
{
    public string Format { get; set; } = "unknown";
    public string DocumentHash { get; set; } = "";
    public List<PositionedPage> Pages { get; } = new List<PositionedPage>();
    public List<EngineDiagnostic> Diagnostics { get; } = new List<EngineDiagnostic>();
    public bool CompleteDecode { get; set; } = true;
}
public sealed class EngineDiagnostic
{
    public string Code { get; set; } = "";
    public string Severity { get; set; } = "warning";
    public string Message { get; set; } = "";
    public List<string> EvidenceRefs { get; set; } = new List<string>();
    public List<string> RuleIds { get; set; } = new List<string>();
}
public sealed class ExtractionLimits
{
    public long FileBytes { get; set; } = 268435456;
    public int PageCount { get; set; } = 10000;
    public long DecompressedPackageBytes { get; set; } = 1073741824;
    public int MaximumElementsPerPage { get; set; } = 1000000;
    public int ExecutionTimeMs { get; set; } = 120000;
}
public interface IPageSource : IDisposable
{
    string Format { get; }
    int PageCount { get; }
    int DecodedPageCount { get; }
    PositionedDocument Document { get; }
    PositionedPage ReadPage(int index, System.Threading.CancellationToken token = default);
}
