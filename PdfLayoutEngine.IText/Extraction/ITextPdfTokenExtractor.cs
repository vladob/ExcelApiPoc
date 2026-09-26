using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using PdfLayoutEngine.Models;
using LayoutDocument = PdfLayoutEngine.Models.PdfDocument;
using LayoutPage = PdfLayoutEngine.Models.PdfPage;

namespace PdfLayoutEngine.IText.Extraction;

public sealed class ITextPdfTokenExtractor
{
    public LayoutDocument Extract(string filePath, string? password = null)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        using var stream = File.OpenRead(filePath);
        return Extract(stream, password);
    }

    public LayoutDocument Extract(Stream pdfStream, string? password = null)
    {
        if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));

        var properties = new ReaderProperties();
        if (!string.IsNullOrEmpty(password)) properties.SetPassword(Encoding.UTF8.GetBytes(password));

        using var reader = new PdfReader(pdfStream, properties);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(reader);
        var pages = new List<LayoutPage>(pdf.GetNumberOfPages());
        for (var pageNumber = 1; pageNumber <= pdf.GetNumberOfPages(); pageNumber++)
        {
            var page = pdf.GetPage(pageNumber);
            var listener = new TextTokenEventListener(pageNumber, page.GetRotation(),
                page.GetPageSize().GetWidth(), page.GetPageSize().GetHeight());
            new PdfCanvasProcessor(listener).ProcessPageContent(page);
            pages.Add(new LayoutPage(pageNumber, listener.Tokens));
        }

        return new LayoutDocument(pages);
    }

    private sealed class TextTokenEventListener(int pageNumber, int rotation, double pageWidth, double pageHeight) : IEventListener
    {
        private static readonly ICollection<EventType> SupportedEvents = [EventType.RENDER_TEXT];
        private readonly int _pageNumber = pageNumber;
        private readonly int _rotation = (rotation % 360 + 360) % 360;
        private readonly double _pageWidth = pageWidth;
        private readonly double _pageHeight = pageHeight;
        private readonly List<PdfTextToken> _tokens = [];

        public IReadOnlyList<PdfTextToken> Tokens => AssembleCharacterRuns(_tokens, _rotation == 90 || _rotation == 270);

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT || data is not TextRenderInfo textRenderInfo) return;

            var baseline = textRenderInfo.GetBaseline().GetBoundingRectangle();
            var fontName = textRenderInfo.GetFont()?.GetFontProgram()?.ToString() ?? string.Empty;
            var left = (double)baseline.GetLeft();
            var right = (double)baseline.GetRight();
            var y = (double)baseline.GetTop();
            if (_rotation == 90)
            {
                left = baseline.GetBottom();
                right = baseline.GetTop();
                y = _pageWidth - baseline.GetLeft();
            }
            else if (_rotation == 270)
            {
                left = _pageHeight - baseline.GetTop();
                right = _pageHeight - baseline.GetBottom();
                y = baseline.GetLeft();
            }
            _tokens.Add(new PdfTextToken(
                _pageNumber,
                textRenderInfo.GetText() ?? string.Empty,
                left,
                right,
                y,
                ContainsStyle(fontName, "bold"),
                ContainsStyle(fontName, "italic") || ContainsStyle(fontName, "oblique")));
        }

        public ICollection<EventType> GetSupportedEvents() => SupportedEvents;

        // Some accounting print drivers emit one PDF text operation per glyph.
        // Reassemble adjacent glyphs before matching phrases in layout rules.
        // Keep complete text operations intact, and do not join across columns
        // or baselines. The original glyph positions still determine the run box.
        private static IReadOnlyList<PdfTextToken> AssembleCharacterRuns(List<PdfTextToken> tokens, bool rotated)
        {
            var result = new List<PdfTextToken>();
            var index = 0;
            while (index < tokens.Count)
            {
                var first = tokens[index];
                if (first.OriginalText.Length != 1)
                {
                    result.Add(first);
                    index++;
                    continue;
                }

                var text = new StringBuilder(first.OriginalText);
                var right = first.Right;
                var end = index + 1;
                while (end < tokens.Count)
                {
                    var next = tokens[end];
                    var gap = next.Left - right;
                    // Some print drivers assign every glyph in one text run the
                    // same bounding box, so consecutive glyph boxes coincide.
                    var coincident = Math.Abs(next.Left - first.Left) <= 0.25 &&
                                     Math.Abs(next.Right - first.Right) <= 0.25;
                    if (next.OriginalText.Length != 1 ||
                        Math.Abs(next.Baseline - first.Baseline) > 0.75 ||
                        (!coincident && (gap < -1.5 || gap > (rotated ? 10 : 2))) ||
                        next.IsBold != first.IsBold || next.IsItalic != first.IsItalic)
                        break;
                    text.Append(next.OriginalText);
                    right = next.Right;
                    end++;
                }

                result.Add(end == index + 1 ? first : new PdfTextToken(
                    first.PageNumber, text.ToString(), first.Left, right,
                    first.Baseline, first.IsBold, first.IsItalic));
                index = end;
            }
            return result;
        }

        private static bool ContainsStyle(string fontName, string style) =>
            fontName.IndexOf(style, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
