using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using PdfLayoutEngine.V2;
using PdfLayoutEngine.Xps;
using PdfLayoutEngine.IText.V2;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Geom;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace PdfLayoutEngine.Tests;
public class V2Tests
{
    private static string Root => System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", "V2");
    private static readonly Lazy<DefinitionCatalog> Catalog = new(() => DefinitionCatalog.Load(Root, true));
    private static JsonElement Parser(string id) => Catalog.Value.Resolve("ifosoft.parsers").A("profiles").Single(p => p.S("id") == id);
    [Fact] public void CatalogRequiresReviewOptInAndValidatesAllDefinitions()
    {
        Assert.Throws<InvalidDataException>(() => DefinitionCatalog.Load(Root));
        Assert.Equal(11, Catalog.Value.Layouts.Count());
        Assert.Equal(43, Catalog.Value.Catalog.A("definitions").Length);
    }
    [Fact] public void DefinitionHashNormalizesOnlyCrLf()
    {
        Assert.Equal(DefinitionCatalog.DefinitionHash("ž\r\n"), DefinitionCatalog.DefinitionHash("ž\n"));
        Assert.NotEqual(DefinitionCatalog.DefinitionHash("a\n"), DefinitionCatalog.DefinitionHash("a \n"));
    }
    [Theory]
    [InlineData("1.234,56", "money-comma", "1234.56")]
    [InlineData("- 1 234.50", "money-dot", "-1234.5")]
    [InlineData("123,-", "money-comma", "123")]
    [InlineData("001 020", "code", "001 020")]
    [InlineData("14/2024", "period", "14/2024")]
    public void ParsesExplicitFormats(string raw, string parser, string expected)
    { var v = ValueParser.Parse(raw, Parser(parser)); Assert.Equal("present", v.State); Assert.Equal(expected, v.Value); }
    [Theory] [InlineData("12 34.50")] [InlineData("1,234.50")] [InlineData("1.234")]
    public void RejectsInvalidMoneyWithoutGuessing(string raw) => Assert.Equal("invalid", ValueParser.Parse(raw, Parser("money-dot")).State);
    [Fact] public void BlankMissingAndZeroAreDistinct()
    {
        Assert.Equal("blank", ValueParser.Parse("", Parser("money-dot")).State);
        Assert.Equal("unresolved", ValueParser.Parse("", Parser("money-dot"), false).State);
        Assert.Equal("0", ValueParser.Parse("0.00", Parser("money-dot")).Value);
    }
    [Fact] public void TwoDigitYearRequiresPrintedContextAndInvalidDatesAreNotRepaired()
    {
        Assert.Equal("unresolved", ValueParser.Parse("1.2.24", Parser("date")).State);
        Assert.Equal("2024-02-01", ValueParser.Parse("1.2.24", Parser("date"), true, 2024).Value);
        Assert.Equal("invalid", ValueParser.Parse("31.2.2024", Parser("date")).State);
        Assert.Equal("invalid", ValueParser.Parse("1.2.24", Parser("date"), true, 2025).State);
    }
    [Fact] public void ConflictingMetadataRetainsBothValues()
    {
        var values = new Dictionary<string, FieldValue>();
        PositionedLayoutEngine.Merge(values, "reportingEntity.name", ValueParser.Parse("Alpha", Parser("text")));
        PositionedLayoutEngine.Merge(values, "reportingEntity.name", ValueParser.Parse("Beta", Parser("text")));
        Assert.Equal("ambiguous", values["reportingEntity.name"].State); Assert.Null(values["reportingEntity.name"].Value);
        Assert.Equal(new[] { "Alpha", "Beta" }, values["reportingEntity.name"].Alternatives);
    }
    [Fact] public void CalibrationUsesRulingsAndLabelsNotPaperSize()
    {
        var variant = Catalog.Value.Resolve("ifosoft.account-plan-uctrozvrh").A("variants")[0];
        var transform = new TransformPt(.91, 0, 0, .91, 12, -3); var page = Header(variant, transform);
        page.WidthPt = 1000; page.HeightPt = 1200;
        var fit = CalibrationFitter.Fit(page, variant);
        Assert.Equal("resolved", fit.State); Assert.Equal(.91, fit.Scale, 5); Assert.Equal(12, fit.Tx, 4); Assert.Equal(-3, fit.Ty, 4);
        page.Lines.Clear(); Assert.Equal("unresolved", CalibrationFitter.Fit(page, variant).State);
    }
    [Fact] public void MalformedRecordValuesDoNotSuppressCandidateDetection()
    {
        var variant = Catalog.Value.Resolve("ifosoft.account-plan-uctrozvrh").A("variants")[0]; var page = Header(variant, TransformPt.Identity);
        var block = variant.A("blocks").Single(b => b.S("id") == "record");
        foreach (var field in block.A("fields")) { var rect = new TransformPt(1, 0, 0, 1, 0, 700).Apply(field.GetProperty("rectPt").Rect()); AddText(page, "bad!", rect); }
        var blocks = OccurrenceDetector.Detect(page, variant, new PageCalibration { State = "resolved" }, 10000, default);
        Assert.Contains(blocks, b => b.Block.S("id") == "record");
    }
    [Fact] public void GlyphCrossingAndClippingRemainAmbiguous()
    {
        var page = new PositionedPage(); AddText(page, "123", new RectPt(5, 2, 25, 10)); page.Text[0].HasGlyphGeometry = false;
        Assert.Equal("ambiguous", PositionedLayoutEngine.ReadField(page, new RectPt(0, 0, 20, 15), Parser("integer"), 1).State);
        page.Text[0].HasGlyphGeometry = true; page.Text[0].IsClipped = true;
        Assert.Equal("ambiguous", PositionedLayoutEngine.ReadField(page, new RectPt(0, 0, 30, 15), Parser("integer"), 1).State);
    }
    [Fact] public void FooterInheritanceRetainsConflictsAndDoesNotCrossUnprovenPages()
    {
        var layout = Catalog.Value.Resolve("ifosoft.journal-dennik-simplified");
        var detail = new ExtractedRecord { Id = "detail", BlockRef = "record", SemanticClass = "journalEntry", PageIndex = 0, OriginY = 700, AggregationMemberships = new() { "exportedDetails" } };
        detail.Fields["documentNumber"] = ValueParser.Parse("A", Parser("code"));
        var footer = new ExtractedRecord { Id = "footer", BlockRef = "foot2", SemanticClass = "documentControl", PageIndex = 0, OriginY = 650 };
        footer.Fields["documentNumber"] = ValueParser.Parse("B", Parser("code")); footer.Fields["documentType"] = ValueParser.Parse("VF", Parser("code"));
        var result = new EngineResult { Records = new() { detail, footer } }; GroupProcessor.Apply(layout, result);
        Assert.Equal("ambiguous", detail.Fields["documentNumber"].State); Assert.Equal("inherited", detail.Fields["documentType"].Origin);
        var other = new ExtractedRecord { Id = "prior", BlockRef = "record", SemanticClass = "journalEntry", PageIndex = 0, OriginY = 700, AggregationMemberships = new() { "exportedDetails" } };
        footer.PageIndex = 1; result = new EngineResult { Records = new() { other, footer } }; GroupProcessor.Apply(layout, result);
        Assert.False(other.Fields.ContainsKey("documentNumber")); Assert.Contains(result.Diagnostics, d => d.Code == "groupContinuityUnresolved");
    }
    [Fact] public void BalanceEquationUsesDecimalAndNeverTreatsBlankAsZero()
    {
        var layout = Catalog.Value.Resolve("ifosoft.general-ledger-hlknia4b"); var row = new ExtractedRecord { Id = "r", VariantRef = "original", BlockRef = "foot4", SemanticClass = "accountBalance" };
        foreach (var pair in new[] { ("openingNet", "10.01"), ("turnoverDebit", "2.02"), ("turnoverCredit", "1.01"), ("closingNet", "11.02") }) row.Fields[pair.Item1] = ValueParser.Parse(pair.Item2, Parser("money-dot"));
        var result = new EngineResult { Records = new() { row } }; ValidationProcessor.Apply(Catalog.Value, layout, result, "internalConsistency", default);
        Assert.Equal("pass", result.ValidationResults.Single(v => v.RuleId == "balance-equation").State);
        Assert.All(result.ValidationResults.Where(v => v.RuleId.StartsWith("compare.")), v => Assert.Equal("notEvaluated", v.State));
        row.Fields["turnoverCredit"] = ValueParser.Parse("", Parser("money-dot")); result = new EngineResult { Records = new() { row } }; ValidationProcessor.Apply(Catalog.Value, layout, result, "internalConsistency", default);
        Assert.Equal("inconclusive", result.ValidationResults.Single(v => v.RuleId == "balance-equation").State);
    }
    [Fact] public void PdfNormalizesCropRotationAndUserUnitAndExtractsRulings()
    {
        using var temp = new TempFile(".pdf");
        using (var pdf = new PdfDocument(new PdfWriter(temp.Path)))
        {
            var page = pdf.AddNewPage(new PageSize(300, 400)); page.SetCropBox(new Rectangle(10, 20, 200, 300)); page.SetRotation(90); page.GetPdfObject().Put(PdfName.UserUnit, new PdfNumber(2));
            new PdfCanvas(page).MoveTo(20, 30).LineTo(120, 30).Stroke().BeginText().SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10).MoveText(20, 40).ShowText("AB").EndText();
        }
        var doc = new ITextPositionedAdapter().Extract(temp.Path); var p = Assert.Single(doc.Pages);
        Assert.Equal(600, p.WidthPt); Assert.Equal(400, p.HeightPt); Assert.Equal(2, p.Text.Count); Assert.NotEmpty(p.Lines);
        Assert.Equal(20, p.Lines[0].Start.X, 3); Assert.Equal(380, p.Lines[0].Start.Y, 3); Assert.Equal(180, p.Lines[0].End.Y, 3);
    }
    [Fact] public void XpsFollowsReferencesAndTransformsAndRejectsTraversal()
    {
        using var temp = MakeXps(false); var doc = new XpsPositionedAdapter().Extract(temp.Path);
        Assert.Equal(new[] { "Pages/2.fpage", "Pages/1.fpage" }, doc.Pages.Select(p => p.SourcePageId));
        var line = Assert.Single(doc.Pages[0].Lines); Assert.Equal(15, line.Start.X); Assert.Equal(52.5, line.Start.Y); Assert.Equal(30, line.End.X);
        using var malicious = MakeXps(true); Assert.Throws<InvalidDataException>(() => new XpsPositionedAdapter().Extract(malicious.Path));
        Assert.Throws<InvalidDataException>(() => new XpsPositionedAdapter().Extract(temp.Path, new ExtractionLimits { DecompressedPackageBytes = 10 }));
    }
    [Fact] public void ResultContractIsValidForUnrecognizedInputAndLevelsPromote()
    {
        var document = new PositionedDocument { Format = "PDF", DocumentHash = "test" }; document.Pages.Add(new PositionedPage { SourcePageId = "1", WidthPt = 100, HeightPt = 100 });
        var result = new PositionedLayoutEngine(Catalog.Value).Process(document, new EngineOptions { Extraction = "recognition", Validation = "completeness" });
        Assert.Equal("records", result.EffectiveLevels.Extraction); Assert.Equal("incomplete", result.ProcessingStatus); Assert.Equal("unknown", result.Coverage.OriginalInputCompleteness);
        Catalog.Value.Validate("result", JsonDocument.Parse(result.ToJson()).RootElement);
    }
    private static PositionedPage Header(JsonElement variant, TransformPt transform)
    {
        var page = new PositionedPage { PhysicalPageIndex = 0, SourcePageId = "fixture", WidthPt = 595, HeightPt = 842 };
        var header = variant.A("blocks").Single(b => b.S("kind") == "pageHeader");
        foreach (var line in header.A("lines")) { var points = line.A("pointsPt"); page.Lines.Add(new PositionedLine { Start = transform.Apply(points[0].Point()), End = transform.Apply(points[1].Point()) }); }
        foreach (var label in header.A("labels")) AddText(page, label.S("text"), transform.Apply(label.GetProperty("rectPt").Rect())); return page;
    }
    private static void AddText(PositionedPage page, string text, RectPt rect) => page.Text.Add(new PositionedText { Id = "t" + page.Text.Count, Text = text, RawBounds = rect, Bounds = rect, Baseline = new PointPt(rect.Left, rect.Bottom + 2), HasGlyphGeometry = true });
    private static TempFile MakeXps(bool traversal)
    {
        var temp = new TempFile(".xps"); using var zip = ZipFile.Open(temp.Path, ZipArchiveMode.Create);
        void Part(string name, string data) { using var w = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)); w.Write(data); }
        Part("_rels/.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='r' Type='http://schemas.microsoft.com/xps/2005/06/fixedrepresentation' Target='Sequence.fdseq'/></Relationships>");
        Part("Sequence.fdseq", "<FixedDocumentSequence xmlns='http://schemas.microsoft.com/xps/2005/06'><DocumentReference Source='Document.fdoc'/></FixedDocumentSequence>");
        Part("Document.fdoc", "<FixedDocument xmlns='http://schemas.microsoft.com/xps/2005/06'><PageContent Source='" + (traversal ? "../outside.fpage" : "Pages/2.fpage") + "'/><PageContent Source='Pages/1.fpage'/></FixedDocument>");
        foreach (int i in new[] { 1, 2 }) Part("Pages/" + i + ".fpage", "<FixedPage xmlns='http://schemas.microsoft.com/xps/2005/06' Width='100' Height='100'><Canvas RenderTransform='2,0,0,2,10,20'><Path Stroke='#000000' Data='M 5,5 L 15,5'/></Canvas></FixedPage>"); return temp;
    }
    private sealed class TempFile : IDisposable
    { public string Path { get; } public TempFile(string extension) { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid() + extension); } public void Dispose() { if (File.Exists(Path)) File.Delete(Path); } }
}
