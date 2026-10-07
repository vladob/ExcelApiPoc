using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PdfLayoutEngine.Models;
using LayoutDocument = PdfLayoutEngine.Models.PdfDocument;
using LayoutPage = PdfLayoutEngine.Models.PdfPage;

namespace PdfLayoutEngine.IText.Extraction;

/// <summary>Reads positioned text from OpenXPS without printing or rasterizing pages.</summary>
public sealed class OpenXpsTokenExtractor
{
    private const double PointScale = 72d / 96d;

    public LayoutDocument Extract(string filePath)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        using var stream = File.OpenRead(filePath);
        return Extract(stream);
    }

    public LayoutDocument Extract(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var sequence = ReadXml(archive, "FixedDocumentSequence.fdseq");
        var pages = new List<LayoutPage>();

        foreach (var documentReference in sequence.Descendants().Where(e => e.Name.LocalName == "DocumentReference"))
        {
            var documentPath = Resolve("FixedDocumentSequence.fdseq", Required(documentReference, "Source"));
            var document = ReadXml(archive, documentPath);
            foreach (var pageReference in document.Descendants().Where(e => e.Name.LocalName == "PageContent"))
            {
                var pagePath = Resolve(documentPath, Required(pageReference, "Source"));
                var page = ReadXml(archive, pagePath);
                var height = Number(Required(page.Root!, "Height"));
                var pageNumber = pages.Count + 1;
                var tokens = new List<PdfTextToken>();

                foreach (var glyph in page.Descendants().Where(e => e.Name.LocalName == "Glyphs"))
                {
                    var value = (string?)glyph.Attribute("UnicodeString");
                    if (value is null || value.Length == 0) continue;
                    var x = Number(Required(glyph, "OriginX"));
                    var y = Number(Required(glyph, "OriginY"));
                    var em = Number(Required(glyph, "FontRenderingEmSize"));
                    var width = GetAdvance(glyph, value, em);
                    var font = (string?)glyph.Attribute("FontUri") ?? string.Empty;
                    var style = (string?)glyph.Attribute("StyleSimulations") ?? string.Empty;
                    tokens.Add(new PdfTextToken(pageNumber, value, x * PointScale,
                        (x + width) * PointScale, (height - y) * PointScale,
                        font.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        style.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0,
                        font.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        style.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0));
                }

                pages.Add(new LayoutPage(pageNumber, tokens));
            }
        }

        if (pages.Count == 0) throw new InvalidDataException("The OpenXPS document contains no pages.");
        return new LayoutDocument(pages);
    }

    private static double GetAdvance(XElement glyph, string value, double em)
    {
        var indices = (string?)glyph.Attribute("Indices");
        if (indices is null || indices.Length == 0) return value.Length * em * 0.5;

        // XPS glyph advances are hundredths of the font em. A missing advance
        // uses the font's intrinsic width, which is unavailable without decoding
        // the embedded (potentially obfuscated) font; approximate those only.
        var total = 0d;
        var count = 0;
        foreach (var item in indices.Split(';'))
        {
            if (item.Length == 0) continue;
            var pieces = item.Split(',');
            if (pieces.Length > 1 && double.TryParse(pieces[1], NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var advance))
                total += advance * em / 100d;
            else
                total += em * 0.5;
            count++;
        }
        return total + Math.Max(0, value.Length - count) * em * 0.5;
    }

    private static XDocument ReadXml(ZipArchive archive, string path)
    {
        var entry = archive.GetEntry(path) ?? throw new InvalidDataException("Missing OpenXPS part: " + path);
        using var input = entry.Open();
        using var reader = XmlReader.Create(input, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        });
        return XDocument.Load(reader);
    }

    private static string Required(XElement element, string name) =>
        (string?)element.Attribute(name) ?? throw new InvalidDataException("Missing OpenXPS attribute: " + name);

    private static double Number(string value) => double.Parse(value, CultureInfo.InvariantCulture);

    private static string Resolve(string source, string target)
    {
        var segments = new List<string>();
        var combined = target.StartsWith("/", StringComparison.Ordinal)
            ? target.Substring(1)
            : source.Substring(0, source.LastIndexOf('/') + 1) + target;
        foreach (var segment in combined.Replace('\\', '/').Split('/'))
        {
            if (segment == "..")
            {
                if (segments.Count == 0) throw new InvalidDataException("Invalid OpenXPS part path.");
                segments.RemoveAt(segments.Count - 1);
            }
            else if (segment.Length > 0 && segment != ".") segments.Add(segment);
        }
        return string.Join("/", segments);
    }
}
