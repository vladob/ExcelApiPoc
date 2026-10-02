using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PdfLayoutEngine.V2;

namespace PdfLayoutEngine.IText.V2;

/// <summary>PDF glyphs and stroked rulings in upright, crop-local points.</summary>
public sealed class ITextPositionedAdapter : IPositionedDocumentAdapter
{
    public PositionedDocument Extract(string filePath, ExtractionLimits? limits = null, CancellationToken cancellationToken = default)
    {
        limits ??= new ExtractionLimits();
        if (new FileInfo(filePath).Length > limits.FileBytes) throw new InvalidDataException("PDF exceeds byte budget.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(limits.ExecutionTimeMs);
        var result = new PositionedDocument { Format = "PDF", DocumentHash = DefinitionCatalog.Hash(File.ReadAllBytes(filePath)) };
        using var reader = new PdfReader(filePath);
        using var pdf = new PdfDocument(reader);
        if (pdf.GetNumberOfPages() > limits.PageCount) throw new InvalidDataException("PDF exceeds page budget.");
        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            timeout.Token.ThrowIfCancellationRequested();
            var source = pdf.GetPage(i); var crop = source.GetCropBox();
            double unit = source.GetPdfObject().GetAsNumber(PdfName.UserUnit)?.DoubleValue() ?? 1;
            if (unit <= 0 || double.IsInfinity(unit) || double.IsNaN(unit)) throw new InvalidDataException("Invalid PDF UserUnit.");
            var rotation = ((source.GetRotation() % 360) + 360) % 360;
            var width = crop.GetWidth() * unit; var height = crop.GetHeight() * unit;
            var transform = new TransformPt(unit, 0, 0, unit, -crop.GetLeft() * unit, -crop.GetBottom() * unit);
            switch (rotation)
            {
                case 90: transform = transform.Then(new TransformPt(0, -1, 1, 0, 0, width)); break;
                case 180: transform = transform.Then(new TransformPt(-1, 0, 0, -1, width, height)); break;
                case 270: transform = transform.Then(new TransformPt(0, 1, -1, 0, height, 0)); break;
                case 0: break;
                default: throw new InvalidDataException("Unsupported non-right-angle page rotation.");
            }
            var page = new PositionedPage { PhysicalPageIndex = i - 1, SourcePageId = "pdf-page-" + i, WidthPt = rotation % 180 == 0 ? width : height, HeightPt = rotation % 180 == 0 ? height : width, SourceToNormalized = transform };
            var listener = new Listener(page, limits, timeout.Token, result);
            new PdfCanvasProcessor(listener).ProcessPageContent(source);
            result.Pages.Add(page);
        }
        return result;
    }

    private sealed class Listener : IEventListener
    {
        private readonly PositionedPage page; private readonly ExtractionLimits limits; private readonly CancellationToken token;
        private readonly PositionedDocument document; private RectPt? clip;
        public Listener(PositionedPage page, ExtractionLimits limits, CancellationToken token, PositionedDocument document)
        { this.page = page; this.limits = limits; this.token = token; this.document = document; clip = new RectPt(0, 0, page.WidthPt, page.HeightPt); }
        public ICollection<EventType> GetSupportedEvents() => new HashSet<EventType> { EventType.RENDER_TEXT, EventType.RENDER_PATH, EventType.CLIP_PATH_CHANGED };
        private static PointPt P(Vector v) => new PointPt(v.Get(0), v.Get(1));
        private static TransformPt MatrixTransform(Matrix m) => new TransformPt(m.Get(Matrix.I11), m.Get(Matrix.I12), m.Get(Matrix.I21), m.Get(Matrix.I22), m.Get(Matrix.I31), m.Get(Matrix.I32));
        public void EventOccurred(IEventData data, EventType type)
        {
            token.ThrowIfCancellationRequested();
            if (page.Text.Count + page.Lines.Count > limits.MaximumElementsPerPage) throw new InvalidDataException("PDF element budget exceeded.");
            if (type == EventType.RENDER_TEXT)
            {
                var run = (TextRenderInfo)data;
                if (run.GetTextRenderMode() == 3 || run.GetTextRenderMode() == 7) return;
                foreach (var glyph in run.GetCharacterRenderInfos())
                {
                    var points = new[] { P(glyph.GetAscentLine().GetStartPoint()), P(glyph.GetAscentLine().GetEndPoint()), P(glyph.GetDescentLine().GetStartPoint()), P(glyph.GetDescentLine().GetEndPoint()) };
                    var raw = new RectPt(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
                    var bounds = page.SourceToNormalized.Apply(raw);
                    page.Text.Add(new PositionedText { Id = $"p{page.PhysicalPageIndex}:t{page.Text.Count}", Text = glyph.GetText(), RawBounds = raw, Bounds = bounds, Baseline = page.SourceToNormalized.Apply(P(glyph.GetBaseline().GetStartPoint())), Font = glyph.GetFont().GetFontProgram().GetFontNames().GetFontName(), HasGlyphGeometry = true, IsClipped = clip.HasValue && (!clip.Value.Contains(new PointPt(bounds.Left, bounds.Bottom), .2) || !clip.Value.Contains(new PointPt(bounds.Right, bounds.Top), .2)), SourceToNormalized = page.SourceToNormalized });
                }
            }
            else if (type == EventType.RENDER_PATH)
            {
                var path = (PathRenderInfo)data;
                if ((path.GetOperation() & PathRenderInfo.STROKE) == 0) return;
                var transform = MatrixTransform(path.GetCtm()).Then(page.SourceToNormalized);
                foreach (var subpath in path.GetPath().GetSubpaths())
                {
                    // Curves must not become spurious calibration rulings.
                    if (subpath.GetSegments().Any(s => !(s is Line))) continue;
                    var points = subpath.GetPiecewiseLinearApproximation();
                    for (int i = 1; i < points.Count; i++) AddLine(points[i - 1], points[i], transform);
                    if (subpath.IsClosed() && points.Count > 1) AddLine(points[points.Count - 1], points[0], transform);
                }
            }
            else if (type == EventType.CLIP_PATH_CHANGED)
            {
                var info = (ClippingPathInfo)data; var transform = MatrixTransform(info.GetCtm()).Then(page.SourceToNormalized);
                var subpaths = info.GetClippingPath().GetSubpaths();
                var pts = subpaths.SelectMany(s => s.GetPiecewiseLinearApproximation()).Select(p => transform.Apply(new PointPt(p.GetX(), p.GetY()))).ToArray();
                if (pts.Length > 0) clip = new RectPt(pts.Min(p => p.X), pts.Min(p => p.Y), pts.Max(p => p.X), pts.Max(p => p.Y));
                if (subpaths.Count > 1 || subpaths.Any(s => s.GetSegments().Any(segment => !(segment is Line))))
                {
                    document.CompleteDecode = false;
                    if (!document.Diagnostics.Any(d => d.Code == "complexPdfClip")) document.Diagnostics.Add(new EngineDiagnostic { Code = "complexPdfClip", Message = "Complex PDF clipping is retained as bounds; clean success is disabled." });
                }
            }
        }
        private void AddLine(Point a, Point b, TransformPt transform)
        {
            var p = transform.Apply(new PointPt(a.GetX(), a.GetY())); var q = transform.Apply(new PointPt(b.GetX(), b.GetY()));
            if (clip.HasValue && (!clip.Value.Contains(p, .2) || !clip.Value.Contains(q, .2))) return;
            page.Lines.Add(new PositionedLine { Id = $"p{page.PhysicalPageIndex}:l{page.Lines.Count}", Start = p, End = q });
        }
    }
}
