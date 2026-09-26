using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Tests;

public sealed class ITextPdfTokenExtractorTests
{
    [Fact]
    public void Normalizes_rotated_page_glyphs_into_horizontal_text_runs()
    {
        var stream = new MemoryStream();
        using (var writer = CreateWriter(stream))
        using (var pdf = new PdfDocument(writer))
        {
            var page = pdf.AddNewPage().SetRotation(90);
            var canvas = new PdfCanvas(page);
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            canvas.BeginText().SetFontAndSize(font, 10).SetTextMatrix(0, 1, -1, 0, 35, 50);
            foreach (var character in "_PREDVAS3.GMX")
                canvas.ShowText(character.ToString());
            canvas.EndText();
        }
        stream.Position = 0;

        var token = Assert.Single(new ITextPdfTokenExtractor().Extract(stream).Tokens);
        Assert.Equal("_PREDVAS3.GMX", token.Text);
        Assert.True(token.Right > token.Left);
    }

    [Fact]
    public void Reassembles_individually_rendered_glyphs_without_joining_distant_columns()
    {
        var stream = new MemoryStream();
        using (var writer = CreateWriter(stream))
        using (var pdf = new PdfDocument(writer))
        {
            pdf.AddNewPage();
            var canvas = new PdfCanvas(pdf.GetPage(1));
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            canvas.BeginText().SetFontAndSize(font, 10).MoveText(20, 700);
            foreach (var character in "_DENNIK1.GMX")
                canvas.ShowText(character.ToString());
            canvas.EndText().BeginText().SetFontAndSize(font, 10)
                .MoveText(200, 700).ShowText("OTHER").EndText();
        }
        stream.Position = 0;

        var tokens = new ITextPdfTokenExtractor().Extract(stream).Tokens;
        Assert.Equal(2, tokens.Count);
        Assert.Equal("_DENNIK1.GMX", tokens[0].Text);
        Assert.Equal("OTHER", tokens[1].Text);
    }

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

    [Fact]
    public void IText_recognizer_extracts_and_delegates_to_generic_orchestration()
    {
        using var stream = CreateSinglePagePdf("Only");
        var layout = new LayoutDefinition
        {
            Sections = new List<SectionDefinition>
            {
                new SectionDefinition { Id = "body" }
            },
            RecordRules = new List<RecordRecognitionRuleDefinition>
            {
                new RecordRecognitionRuleDefinition
                {
                    Id = "only-record",
                    SectionId = "body",
                    Text = "Only"
                }
            },
            RecordFields = new List<RecordFieldSetDefinition>
            {
                new RecordFieldSetDefinition
                {
                    RecordRuleId = "only-record",
                    Fields = new List<RecordFieldDefinition>
                    {
                        new RecordFieldDefinition
                        {
                            Id = "label",
                            Horizontal = new HorizontalMatchDefinition
                            {
                                Anchor = HorizontalAnchor.Left,
                                Position = 10.5
                            }
                        }
                    }
                }
            }
        };

        var result = new ITextLayoutRecognizer().Recognize(stream, layout);

        Assert.Single(result.Document.Pages);
        var record = Assert.Single(result.Records);
        Assert.Equal(RuleMatchStatus.Matched, record.Status);
        Assert.Equal("only-record", record.RuleId);
        var field = Assert.Single(record.Fields);
        Assert.Equal("Only", field.Value!.Value);
        Assert.Same(
            Assert.Single(result.Document.Tokens),
            Assert.Single(field.Value.Evidence.SourceTokens));
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
