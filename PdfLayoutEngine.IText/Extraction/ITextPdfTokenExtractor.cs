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
            var listener = new TextTokenEventListener(pageNumber);
            new PdfCanvasProcessor(listener).ProcessPageContent(pdf.GetPage(pageNumber));
            pages.Add(new LayoutPage(pageNumber, listener.Tokens));
        }

        return new LayoutDocument(pages);
    }

    private sealed class TextTokenEventListener(int pageNumber) : IEventListener
    {
        private static readonly ICollection<EventType> SupportedEvents = [EventType.RENDER_TEXT];
        private readonly int _pageNumber = pageNumber;
        private readonly List<PdfTextToken> _tokens = [];

        public IReadOnlyList<PdfTextToken> Tokens => _tokens;

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT || data is not TextRenderInfo textRenderInfo) return;

            var baseline = textRenderInfo.GetBaseline().GetBoundingRectangle();
            var fontName = textRenderInfo.GetFont()?.GetFontProgram()?.ToString() ?? string.Empty;
            _tokens.Add(new PdfTextToken(
                _pageNumber,
                textRenderInfo.GetText() ?? string.Empty,
                baseline.GetLeft(),
                baseline.GetRight(),
                baseline.GetTop(),
                ContainsStyle(fontName, "bold"),
                ContainsStyle(fontName, "italic") || ContainsStyle(fontName, "oblique")));
        }

        public ICollection<EventType> GetSupportedEvents() => SupportedEvents;

        private static bool ContainsStyle(string fontName, string style) =>
            fontName.IndexOf(style, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
