using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using PdfLayoutEngine.IText.Extraction;

namespace PdfLayoutEngine.Tests;

public sealed class ITextPdfTokenExtractorTests
{
    [Fact]
    public void Extracts_all_pages_with_fractional_coordinates_and_style()
    {
        using var stream = CreateTwoPagePdf();

        var document = new ITextPdfTokenExtractor().Extract(stream);

        Assert.Equal(2, document.Pages.Count);
        Assert.Equal(2, document.Tokens.Count);
        var first = document.Tokens[0];
        Assert.Equal(1, first.PageNumber);
        Assert.Equal("First", first.Text);
        Assert.True(first.IsBold);
        Assert.True(first.IsItalic);
        Assert.InRange(first.Left, 42.24, 42.26);
        Assert.NotEqual(Math.Truncate(first.Left), first.Left);
        Assert.InRange(first.Baseline, 700.74, 700.76);
        Assert.Equal(2, document.Tokens[1].PageNumber);
    }

    [Fact]
    public void Extractor_has_no_cross_document_state()
    {
        using var firstStream = CreateTwoPagePdf();
        using var secondStream = CreateSinglePagePdf("Only");
        var extractor = new ITextPdfTokenExtractor();

        var first = extractor.Extract(firstStream);
        var second = extractor.Extract(secondStream);

        Assert.Equal(2, first.Tokens.Count);
        Assert.Single(second.Tokens);
        Assert.Equal("Only", second.Tokens[0].Text);
    }

    private static MemoryStream CreateTwoPagePdf()
    {
        var stream = new MemoryStream();
        using (var writer = CreateWriter(stream, new WriterProperties().SetCompressionLevel(0)))
        using (var pdf = new PdfDocument(writer))
        {
            AddText(pdf, "First", 42.25f, 700.75f, StandardFonts.HELVETICA_BOLDOBLIQUE);
            pdf.AddNewPage();
            AddTextToPage(pdf, 2, "Second", 51.5f, 650.25f, StandardFonts.HELVETICA);
        }
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateSinglePagePdf(string text)
    {
        var stream = new MemoryStream();
        using (var writer = CreateWriter(stream))
        using (var pdf = new PdfDocument(writer))
            AddText(pdf, text, 10.5f, 20.25f, StandardFonts.HELVETICA);
        stream.Position = 0;
        return stream;
    }

    private static PdfWriter CreateWriter(Stream stream, WriterProperties? properties = null)
    {
        var writer = properties == null ? new PdfWriter(stream) : new PdfWriter(stream, properties);
        writer.SetCloseStream(false);
        return writer;
    }

    private static void AddText(PdfDocument pdf, string text, float x, float y, string fontName)
    {
        pdf.AddNewPage();
        AddTextToPage(pdf, 1, text, x, y, fontName);
    }

    private static void AddTextToPage(PdfDocument pdf, int pageNumber, string text, float x, float y, string fontName)
    {
        var font = PdfFontFactory.CreateFont(fontName);
        var canvas = new PdfCanvas(pdf.GetPage(pageNumber));
        canvas.BeginText().SetFontAndSize(font, 10).MoveText(x, y).ShowText(text).EndText();
    }
}
