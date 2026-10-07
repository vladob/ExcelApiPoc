using System.IO.Compression;
using System.Text;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Tests;

public sealed class OpenXpsTokenExtractorTests
{
    [Fact]
    public void Extracts_pages_in_document_order_and_converts_coordinates_to_pdf_points()
    {
        using var stream = CreateDocument();
        var result = new OpenXpsTokenExtractor().Extract(stream);

        Assert.Equal(2, result.Pages.Count);
        Assert.Equal("Prvá stránka", result.Pages[0].Tokens[0].Text);
        Assert.Equal("Druhá stránka", result.Pages[1].Tokens[0].Text);
        Assert.Equal(75, result.Pages[0].Tokens[0].Left);
        Assert.Equal(600, result.Pages[0].Tokens[0].Baseline);
        Assert.True(result.Pages[0].Tokens[0].Right > result.Pages[0].Tokens[0].Left);
        Assert.Equal(2, result.Pages[1].Tokens[0].PageNumber);
    }

    [Fact]
    public void Rejects_missing_page_instead_of_silently_importing_partial_data()
    {
        using var stream = CreateDocument(omitSecondPage: true);
        Assert.Throws<InvalidDataException>(() => new OpenXpsTokenExtractor().Extract(stream));
    }

    [Fact]
    public void All_ifosoft_layout_definitions_pass_validation()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft");
        var files = Directory.GetFiles(folder, "*.json");
        Assert.Equal(9, files.Length);
        foreach (var path in files)
        {
            var loaded = new LayoutDefinitionLoader().Load(File.ReadAllText(path));
            Assert.True(loaded.IsValid, path + ": " + string.Join("; ", loaded.Messages.Select(x => x.Message)));
        }
    }

    [Fact]
    public void Openxps_recognizer_uses_same_layout_engine()
    {
        using var stream = CreateDocument();
        var definition = new LayoutDefinition
        {
            SchemaVersion = 1,
            Id = "fixture",
            Sections = new List<SectionDefinition> { new() { Id = "page", Scope = SectionScope.PerPage } },
            RecordRules = new List<RecordRecognitionRuleDefinition>
            {
                new() { Id = "row", SectionId = "page", Text = "Prvá stránka" }
            }
        };
        var result = new OpenXpsLayoutRecognizer().Recognize(stream, definition);
        Assert.Contains(result.Records, x => x.RuleId == "row");
    }

    [Fact]
    public void Signature_detection_requires_title_and_marker_without_using_filename()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft");
        var definitions = Directory.GetFiles(folder, "*.json")
            .Select(path => new LayoutDefinitionLoader().Load(File.ReadAllText(path)).Definition!)
            .ToArray();
        var document = new PdfLayoutEngine.Models.PdfDocument(new[]
        {
            new PdfPage(1, new[]
            {
                new PdfTextToken(1, "HLAVNÁ KNIHA - PODĽA ÚČTOV", 0, 100, 700, false, false),
                new PdfTextToken(1, "_HLKNIA4B.GMX", 0, 100, 680, false, false)
            })
        });
        var matches = new LayoutSignatureDetector().Detect(document, definitions);
        Assert.Equal("ifosoft-general-ledger-hlknia4b", Assert.Single(matches).Id);
    }

    private static MemoryStream CreateDocument(bool omitSecondPage = false)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            Add(archive, "FixedDocumentSequence.fdseq",
                "<FixedDocumentSequence><DocumentReference Source='Documents/1/FixedDocument.fdoc'/></FixedDocumentSequence>");
            Add(archive, "Documents/1/FixedDocument.fdoc",
                "<FixedDocument><PageContent Source='Pages/2.fpage'/><PageContent Source='Pages/1.fpage'/></FixedDocument>");
            Add(archive, "Documents/1/Pages/2.fpage",
                "<FixedPage Height='900'><Glyphs UnicodeString='Prvá stránka' OriginX='100' OriginY='100' FontRenderingEmSize='12' Indices='50,50;50,50'/></FixedPage>");
            if (!omitSecondPage)
                Add(archive, "Documents/1/Pages/1.fpage",
                    "<FixedPage Height='900'><Glyphs UnicodeString='Druhá stránka' OriginX='120' OriginY='120' FontRenderingEmSize='12'/></FixedPage>");
        }
        stream.Position = 0;
        return stream;
    }

    private static void Add(ZipArchive archive, string path, string xml)
    {
        using var writer = new StreamWriter(archive.CreateEntry(path).Open(), new UTF8Encoding(false));
        writer.Write(xml);
    }
}
