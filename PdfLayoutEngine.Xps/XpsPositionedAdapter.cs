using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using PdfLayoutEngine.V2;

namespace PdfLayoutEngine.Xps;

/// <summary>Platform-independent XPS/OXPS adapter. Follows package references, never ZIP order.</summary>
public sealed class XpsPositionedAdapter : IPositionedDocumentAdapter
{
    public PositionedDocument Extract(string filePath, ExtractionLimits? limits = null, CancellationToken cancellationToken = default)
    {
        limits ??= new ExtractionLimits();
        if (new FileInfo(filePath).Length > limits.FileBytes) throw new InvalidDataException("XPS exceeds byte budget.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); timeout.CancelAfter(limits.ExecutionTimeMs);
        using var input = File.OpenRead(filePath); using var zip = new ZipArchive(input, ZipArchiveMode.Read);
        return new Decoder(zip, limits, timeout.Token).Read(DefinitionCatalog.Hash(File.ReadAllBytes(filePath)));
    }
    private sealed class Decoder
    {
        private readonly Dictionary<string, ZipArchiveEntry> entries; private readonly ExtractionLimits limits; private readonly CancellationToken token;
        private readonly Dictionary<string, FontMetrics> fonts = new Dictionary<string, FontMetrics>();
        private readonly PositionedDocument result = new PositionedDocument(); private int count;
        public Decoder(ZipArchive zip, ExtractionLimits limits, CancellationToken token)
        {
            this.limits = limits; this.token = token; entries = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
            long length = 0;
            foreach (var entry in zip.Entries)
            {
                length = checked(length + entry.Length); if (length > limits.DecompressedPackageBytes) throw new InvalidDataException("XPS decompression budget exceeded.");
                if (entry.FullName.EndsWith("/", StringComparison.Ordinal)) continue;
                var key = Resolve("", "/" + entry.FullName);
                if (entries.ContainsKey(key)) throw new InvalidDataException("Duplicate package part."); entries.Add(key, entry);
            }
        }
        private XDocument Xml(string path)
        {
            token.ThrowIfCancellationRequested();
            using var input = Entry(path).Open();
            using var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = Math.Min(limits.DecompressedPackageBytes, 64000000) });
            return XDocument.Load(reader);
        }
        private ZipArchiveEntry Entry(string path) => entries.TryGetValue(path, out var entry) ? entry : throw new InvalidDataException("Missing package part: " + path);
        public PositionedDocument Read(string hash)
        {
            result.DocumentHash = hash;
            var relations = Xml("_rels/.rels").Root!.Elements().Where(x => A(x, "Type").EndsWith("/fixedrepresentation", StringComparison.Ordinal)).ToArray();
            if (relations.Length != 1 || A(relations[0], "TargetMode") == "External") throw new InvalidDataException("Expected one internal fixed representation.");
            var sequencePath = Resolve("", A(relations[0], "Target")); var sequence = Xml(sequencePath);
            var ns = sequence.Root!.Name.NamespaceName;
            result.Format = ns == "http://schemas.openxps.org/oxps/v1.0" ? "OXPS" : ns == "http://schemas.microsoft.com/xps/2005/06" ? "XPS" : throw new InvalidDataException("Unknown fixed document namespace.");
            foreach (var docRef in sequence.Root.Elements().Where(x => x.Name.LocalName == "DocumentReference"))
            {
                var docPath = Resolve(sequencePath, A(docRef, "Source")); var doc = Xml(docPath);
                foreach (var pageRef in doc.Root!.Elements().Where(x => x.Name.LocalName == "PageContent"))
                {
                    token.ThrowIfCancellationRequested(); if (result.Pages.Count >= limits.PageCount) throw new InvalidDataException("XPS page budget exceeded.");
                    var pagePath = Resolve(docPath, A(pageRef, "Source")); var root = Xml(pagePath).Root!;
                    double w = N(root, "Width"), h = N(root, "Height"); if (w <= 0 || h <= 0) throw new InvalidDataException("Invalid fixed page size.");
                    var normal = new TransformPt(.75, 0, 0, -.75, 0, h * .75);
                    var page = new PositionedPage { PhysicalPageIndex = result.Pages.Count, SourcePageId = pagePath, WidthPt = w * .75, HeightPt = h * .75, SourceToNormalized = normal };
                    count = 0; Walk(root, TransformPt.Identity, new RectPt(0, 0, w, h), page, pagePath, normal);
                    result.Pages.Add(page);
                }
            }
            if (result.Pages.Count == 0) throw new InvalidDataException("Fixed representation contains no pages.");
            return result;
        }
        private void Unsupported(string message)
        {
            result.CompleteDecode = false;
            if (!result.Diagnostics.Any(d => d.Message == message)) result.Diagnostics.Add(new EngineDiagnostic { Code = "unsupportedXpsFeature", Message = message });
        }
        private void Walk(XElement node, TransformPt parent, RectPt? parentClip, PositionedPage page, string part, TransformPt normal)
        {
            token.ThrowIfCancellationRequested(); if (++count > limits.MaximumElementsPerPage) throw new InvalidDataException("XPS element budget exceeded.");
            var kind = node.Name.LocalName;
            if (kind.Contains(".")) return;
            var matrix = A(node, "RenderTransform");
            if (matrix.Length == 0) matrix = node.Elements().FirstOrDefault(x => x.Name.LocalName.EndsWith(".RenderTransform", StringComparison.Ordinal))?.Descendants().FirstOrDefault(x => x.Name.LocalName == "MatrixTransform")?.Attribute("Matrix")?.Value ?? "";
            var transform = matrix.Length == 0 ? parent : ParseMatrix(matrix).Then(parent);
            var clip = parentClip; var clipData = A(node, "Clip");
            if (clipData.Length > 0)
            {
                var lines = ParsePath(clipData);
                if (lines.Count > 0)
                {
                    var points = lines.SelectMany(l => new[] { transform.Apply(l.Item1), transform.Apply(l.Item2) }).ToArray();
                    var bounds = new RectPt(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
                    if (lines.Count != 4 || lines.Any(l => Math.Abs(l.Item1.X - l.Item2.X) > .001 && Math.Abs(l.Item1.Y - l.Item2.Y) > .001)) Unsupported("Nonrectangular clip: only conservative bounds retained.");
                    clip = Intersect(clip, bounds);
                }
            }
            if (node.Elements().Any(x => x.Name.LocalName.EndsWith(".Clip", StringComparison.Ordinal))) Unsupported("Property-element clipping is unsupported.");
            if (A(node, "OpacityMask").Length > 0 || node.Elements().Any(x => x.Name.LocalName.EndsWith(".OpacityMask", StringComparison.Ordinal))) Unsupported("Opacity masks are unsupported.");
            if (A(node, "Opacity") == "0") return;
            if (kind == "Glyphs")
            {
                try { Glyphs(node, transform, clip, page, part, normal); }
                catch (NotSupportedException ex) { Unsupported(ex.Message); }
                return;
            }
            if (kind == "Path")
            {
                if (A(node, "Stroke").Length > 0)
                {
                    foreach (var segment in ParsePath(A(node, "Data")))
                    {
                        var a = transform.Apply(segment.Item1); var b = transform.Apply(segment.Item2);
                        if (clip.HasValue && (!clip.Value.Contains(a, .1) || !clip.Value.Contains(b, .1))) continue;
                        page.Lines.Add(new PositionedLine { Id = $"p{page.PhysicalPageIndex}:l{page.Lines.Count}", Start = normal.Apply(a), End = normal.Apply(b) });
                    }
                }
                if (node.Elements().Any(x => x.Name.LocalName == "Path.Data")) Unsupported("Property-element path geometry is unsupported.");
                return;
            }
            if (kind != "Canvas" && kind != "FixedPage") { Unsupported("Unsupported visual: " + kind); return; }
            foreach (var child in node.Elements()) Walk(child, transform, clip, page, part, normal);
        }
        private void Glyphs(XElement node, TransformPt transform, RectPt? clip, PositionedPage page, string part, TransformPt normal)
        {
            if (A(node, "IsSideways") == "true" || (A(node, "BidiLevel").Length > 0 && (int)N(node, "BidiLevel") % 2 != 0)) throw new NotSupportedException("Sideways/RTL glyph runs are unsupported.");
            string text = A(node, "UnicodeString"); if (text.StartsWith("{}", StringComparison.Ordinal)) text = text.Substring(2);
            if (text.Length == 0) { Unsupported("Glyph run without UnicodeString cannot be assigned to semantic fields."); return; }
            var fontPath = Resolve(part, A(node, "FontUri"));
            if (!fonts.TryGetValue(fontPath, out var font))
            {
                using var input = Entry(fontPath).Open(); using var memory = new MemoryStream(); input.CopyTo(memory);
                font = new FontMetrics(memory.ToArray(), fontPath); fonts.Add(fontPath, font);
            }
            var indices = A(node, "Indices").Split(';'); double em = N(node, "FontRenderingEmSize"), x = N(node, "OriginX"), y = N(node, "OriginY");
            if (em <= 0) throw new InvalidDataException("Invalid glyph size.");
            int index = 0, character = 0;
            while (character < text.Length)
            {
                token.ThrowIfCancellationRequested(); if (++count > limits.MaximumElementsPerPage) throw new InvalidDataException("XPS glyph budget exceeded.");
                string spec = index < indices.Length ? indices[index] : ""; int chars = char.IsSurrogatePair(text, character) ? 2 : 1, glyphCount = 1;
                if (spec.StartsWith("(", StringComparison.Ordinal))
                {
                    int end = spec.IndexOf(')'); if (end < 0) throw new InvalidDataException("Invalid glyph cluster.");
                    var cluster = spec.Substring(1, end - 1).Split(':'); chars = int.Parse(cluster[0], CultureInfo.InvariantCulture); glyphCount = cluster.Length == 2 ? int.Parse(cluster[1], CultureInfo.InvariantCulture) : 1; spec = spec.Substring(end + 1);
                }
                if (chars <= 0 || glyphCount <= 0 || character + chars > text.Length || glyphCount > 10000) throw new InvalidDataException("Invalid glyph cluster count.");
                RectPt? union = null; double baselineX = x;
                for (int g = 0; g < glyphCount; g++, index++)
                {
                    if (g > 0) spec = index < indices.Length ? indices[index] : throw new InvalidDataException("Truncated glyph cluster.");
                    var values = spec.Split(','); int glyph = values[0].Length > 0 ? int.Parse(values[0], CultureInfo.InvariantCulture) : font.Glyph(char.ConvertToUtf32(text, character));
                    double advance = values.Length > 1 && values[1].Length > 0 ? D(values[1]) * em / 100 : font.Advance(glyph, em);
                    double u = values.Length > 2 && values[2].Length > 0 ? D(values[2]) * em / 100 : 0, v = values.Length > 3 && values[3].Length > 0 ? D(values[3]) * em / 100 : 0;
                    var b = font.Bounds(glyph, em, x + u, y - v);
                    union = union.HasValue ? new RectPt(Math.Min(union.Value.Left, b.Left), Math.Min(union.Value.Bottom, b.Bottom), Math.Max(union.Value.Right, b.Right), Math.Max(union.Value.Top, b.Top)) : b;
                    x += advance;
                }
                var raw = union!.Value; var sourceBounds = transform.Apply(raw); var composed = transform.Then(normal);
                page.Text.Add(new PositionedText { Id = $"p{page.PhysicalPageIndex}:t{page.Text.Count}", Text = text.Substring(character, chars), RawBounds = raw, Bounds = normal.Apply(sourceBounds), Baseline = composed.Apply(new PointPt(baselineX, y)), HasGlyphGeometry = true, Font = fontPath, IsClipped = clip.HasValue && (!clip.Value.Contains(new PointPt(sourceBounds.Left, sourceBounds.Bottom), .25) || !clip.Value.Contains(new PointPt(sourceBounds.Right, sourceBounds.Top), .25)), SourceToNormalized = composed });
                character += chars;
            }
            if (indices.Skip(index).Any(s => s.Length > 0)) Unsupported("Extra glyph indices without Unicode mapping.");
        }
        private List<Tuple<PointPt, PointPt>> ParsePath(string data)
        {
            var pieces = Regex.Matches(data, @"[A-Za-z]|[-+]?(?:\d*\.\d+|\d+\.?\d*)(?:[eE][-+]?\d+)?", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)).Cast<Match>().Select(m => m.Value).ToArray();
            var lines = new List<Tuple<PointPt, PointPt>>(); int i = 0; char command = '\0'; PointPt current = default, start = default;
            while (i < pieces.Length)
            {
                if (char.IsLetter(pieces[i][0])) command = pieces[i++][0];
                if (command == 'F') { if (i < pieces.Length) i++; command = '\0'; continue; }
                bool relative = char.IsLower(command); char op = char.ToUpperInvariant(command);
                if (op == 'Z') { lines.Add(Tuple.Create(current, start)); current = start; command = '\0'; continue; }
                int needed = op == 'M' || op == 'L' ? 2 : op == 'H' || op == 'V' ? 1 : 0;
                if (needed == 0 || i + needed > pieces.Length) { Unsupported("Unsupported or malformed path command: " + command); return new List<Tuple<PointPt, PointPt>>(); }
                double a = D(pieces[i++]), b = needed == 2 ? D(pieces[i++]) : 0;
                var next = op == 'H' ? new PointPt(relative ? current.X + a : a, current.Y) : op == 'V' ? new PointPt(current.X, relative ? current.Y + a : a) : new PointPt(a + (relative ? current.X : 0), b + (relative ? current.Y : 0));
                if (op == 'M') { start = next; command = relative ? 'l' : 'L'; } else lines.Add(Tuple.Create(current, next));
                current = next;
            }
            return lines;
        }
    }
    private static RectPt Intersect(RectPt? a, RectPt b)
    {
        if (!a.HasValue) return b; var v = a.Value; double l = Math.Max(v.Left, b.Left), t = Math.Max(v.Bottom, b.Bottom); return new RectPt(l, t, Math.Max(l, Math.Min(v.Right, b.Right)), Math.Max(t, Math.Min(v.Top, b.Top)));
    }
    private static string A(XElement e, string name) => e.Attribute(name)?.Value ?? "";
    private static double N(XElement e, string name) => D(A(e, name));
    private static double D(string value) { double d = double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture); if (double.IsNaN(d) || double.IsInfinity(d)) throw new InvalidDataException("Non-finite coordinate."); return d; }
    private static TransformPt ParseMatrix(string value)
    {
        if (value.StartsWith("{", StringComparison.Ordinal)) throw new NotSupportedException("Resource-based transform is unsupported.");
        var n = Regex.Split(value.Trim(), @"[,\s]+").Select(D).ToArray(); if (n.Length != 6) throw new InvalidDataException("Invalid affine transform.");
        var t = new TransformPt(n[0], n[1], n[2], n[3], n[4], n[5]); t.Inverse(); return t;
    }
    private static string Resolve(string parent, string target)
    {
        if (target.Length == 0 || target.Contains(":") || target.Contains("\\") || target.Contains("?") || target.Contains("#")) throw new InvalidDataException("Invalid internal part URI.");
        target = Uri.UnescapeDataString(target);
        if (target.Contains(":") || target.Contains("\\") || target.Contains("\0")) throw new InvalidDataException("Invalid escaped part URI.");
        var stack = new List<string>();
        var path = target.StartsWith("/", StringComparison.Ordinal) ? target : (parent.LastIndexOf('/') >= 0 ? parent.Substring(0, parent.LastIndexOf('/') + 1) : "") + target;
        foreach (var part in path.Split('/'))
        { if (part == "" || part == ".") continue; if (part == "..") { if (stack.Count == 0) throw new InvalidDataException("Part URI escapes package."); stack.RemoveAt(stack.Count - 1); } else stack.Add(part); }
        return string.Join("/", stack);
    }
}
