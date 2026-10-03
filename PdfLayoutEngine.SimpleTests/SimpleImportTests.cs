using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
using PdfLayoutEngine.IText.Simple;
using PdfLayoutEngine.Xps;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;
namespace PdfLayoutEngine.SimpleTests;
public sealed class SimpleImportTests
{
    private static string Layout=>System.IO.Path.Combine(AppContext.BaseDirectory,"hlknia4.json");
    [Fact] public void Identify_decodes_one_page_and_resume_reuses_it()
    {
        string file=CreatePdf(2);
        try
        {
            using var importer=new SimpleLayoutImporter(file,Layout);
            var r=importer.Examine(ImportLevel.Recognize);
            Assert.Equal(1,r.DecodedPageCount);Assert.Empty(r.Rows);Assert.Empty(r.Identifiers);
            r=importer.Examine(ImportLevel.Identify);
            Assert.Equal(1,r.DecodedPageCount);Assert.Equal(2,r.PageCount);Assert.Empty(r.Rows);Assert.Equal("2024",r.Identifiers["fiscalYear"]);
            r=importer.Examine(ImportLevel.Extract);Assert.Equal(2,r.DecodedPageCount);Assert.Null(importer.Canonical);Assert.All(r.Rows,x=>Assert.Empty(x.Amounts));
            r=importer.Examine(ImportLevel.Normalize);Assert.True(r.Status=="completed",System.Text.Json.JsonSerializer.Serialize(r.Issues));Assert.Equal(5,r.CompletedLevel);
            Assert.NotNull(importer.Canonical); Assert.Equal(4,importer.Canonical.Rows.Count); // analytical and item per page, no subtotal duplication
            Assert.Contains(importer.Canonical.Rows,x=>x.AccountCode=="021" && x.SourceOpeningNet==404572.17m);
            Assert.Contains(importer.Canonical.Rows,x=>x.AccountCode=="75 001" && x.SourceDimensions["KP"]=="633001");
            Assert.Null(importer.Canonical.Ico);
            Assert.Equal(5,importer.Examine(ImportLevel.Normalize).CompletedLevel);
        }
        finally { File.Delete(file); }
    }
    [Fact] public void Invalid_amount_prevents_canonical_mapping()
    {
        string file=CreatePdf(1,"12x,00");
        try { using var importer=new SimpleLayoutImporter(file,Layout);var r=importer.Examine(ImportLevel.Normalize);Assert.Equal("invalid",r.Status);Assert.Null(importer.Canonical);Assert.Contains(r.Issues,x=>x.Code=="invalidAmount"); }
        finally { File.Delete(file); }
    }
    [Fact] public void Cancellation_does_not_publish_partial_rows()
    {
        string file=CreatePdf(2);
        try { using var importer=new SimpleLayoutImporter(file,Layout);importer.Examine(ImportLevel.Identify);using var c=new CancellationTokenSource();c.Cancel();Assert.Throws<OperationCanceledException>(()=>importer.Examine(ImportLevel.Extract,c.Token));var final=importer.Examine(ImportLevel.Normalize); Assert.True(final.Status=="completed",System.Text.Json.JsonSerializer.Serialize(final.Issues)); }
        finally { File.Delete(file); }
    }
    [Fact] public void Different_marker_is_not_accepted()
    {
        string file=CreatePdf(1,marker:"_HLKNIA4D.GMX");
        try { using var importer=new SimpleLayoutImporter(file,Layout);var r=importer.Examine(ImportLevel.Normalize);Assert.Equal("unrecognized",r.Status);Assert.Empty(r.Rows);Assert.Null(importer.Canonical); }
        finally { File.Delete(file); }
    }
    [Theory][InlineData("404.572,17",404572.17)][InlineData("- 182.742,51",-182742.51)][InlineData("1.440,-",1440)][InlineData("0,00",0)]
    public void Amount_formats(string raw,decimal expected) { Assert.True(LayoutSession.TryAmount(raw,out var value));Assert.Equal(expected,value); }
    [Theory][InlineData("12.34,56")][InlineData("1,2")][InlineData("1,00 2,00")]
    public void Invalid_amount_formats(string raw)=>Assert.False(LayoutSession.TryAmount(raw,out _));
    [Fact] public void Ink_gaps_do_not_introduce_spaces_inside_a_source_run()
    {
        var glyphs="021".Select((c,i)=>new PositionedText{Text=c.ToString(),RunId="same",Baseline=new PointPt(i*5,100),AdvanceEnd=new PointPt(i*5+5,100),FontSize=8,Bounds=new RectPt(i*5,99,i*5+1,105)});
        Assert.Equal("021",LayoutSession.Assemble(glyphs));
    }
    [Fact] public void Pdf_rotation_is_normalized()
    {
        string file=CreatePdf(1);
        string rotated=System.IO.Path.GetTempFileName()+".pdf";
        try { using(var doc=new PdfDocument(new PdfReader(file),new PdfWriter(rotated)))doc.GetPage(1).SetRotation(90);
            using var source=new PdfPageSource(rotated);var p=source.ReadPage(0);Assert.True(p.WidthPt>p.HeightPt);Assert.NotEmpty(p.Text);Assert.All(p.Text,x=>Assert.True(x.FontSize>0)); }
        finally { File.Delete(file);File.Delete(rotated); }
    }
    [Fact] public void Scaled_pdf_uses_calibrated_font_size_and_columns()
    {
        string file=CreatePdf(1,scale:.8);
        try { using var importer=new SimpleLayoutImporter(file,Layout);var r=importer.Examine(ImportLevel.Normalize);Assert.True(r.Status=="completed",System.Text.Json.JsonSerializer.Serialize(r.Issues));Assert.Equal(2,importer.Canonical.Rows.Count); }
        finally { File.Delete(file); }
    }
    [Fact] public void Asterisks_inside_account_are_not_total_placeholders()
    {
        string file=CreatePdf(1,account:"357***");
        try { using var importer=new SimpleLayoutImporter(file,Layout);var r=importer.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Contains(importer.Canonical.Rows,x=>x.AccountCode=="357***"); }
        finally { File.Delete(file); }
    }
    private static string CreatePdf(int pages,string amount="404.572,17",string marker="_HLKNIA4.GMX",double scale=1,string account="021")
    {
        string path=System.IO.Path.GetTempFileName()+".pdf";
        using var pdf=new PdfDocument(new PdfWriter(path));
        var normal=PdfFontFactory.CreateFont(StandardFonts.HELVETICA);var bold=PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        const double u=72.0/254;
        for(int i=0;i<pages;i++)
        {
            var page=pdf.AddNewPage(PageSize.A4);var c=new PdfCanvas(page);c.ConcatMatrix(scale,0,0,scale,3,-5);double h=page.GetPageSize().GetHeight();
            c.MoveTo(10*u,h-10*u).LineTo(2020*u,h-10*u).MoveTo(10*u,h-210*u).LineTo(2020*u,h-210*u).Stroke();
            void Text(string text,double x,double y,bool b=false,float size=8) { c.BeginText().SetFontAndSize(b?bold:normal,size).MoveText(x*u,h-y*u).ShowText(text).EndText(); }
            Text("HLAVNÁ KNIHA",710,80);Text(marker,20,150);Text("Obec Test",20,48);Text("00/2024",1480,90);Text("12/2024",1650,90);Text((i+1).ToString(),1950,125);
            Text(account,20,250);Text(amount,760,250);Text(amount,1810,250);
            Text("75 001",20,290);Text("633001",240,290);Text("1.440,-",970,290);Text("1.440,-",1810,290);
            Text("021",20,330,true,7);Text("Stavby",90,330,true,7);Text(amount,760,330,true);
            Text("0",20,370,true,7);Text("**",40,370);Text(amount,760,370,true);
            if(i==pages-1){Text("***",20,410);Text(amount,760,410,true);}
        }
        return path;
    }
    [Fact] public void Supplied_corpus_when_configured_matches_known_controls()
    {
        string? root=Environment.GetEnvironmentVariable("IFOSOFT_SIMPLE_TEST_ROOT");
        if(string.IsNullOrEmpty(root))return;
        foreach(var spec in new[]{("HL_KNIHA_00322792_2023.oxps",24,0m), ("HL_KNIHA_00322792_2024_1.oxps",262,602463.11m),("HL_KNIHA_00323586_2024.oxps",480,714592.70m)})
        {
            using var importer=new SimpleLayoutImporter(System.IO.Path.Combine(root,spec.Item1),Layout);
            var id=importer.Examine(ImportLevel.Identify);Assert.Equal(1,id.DecodedPageCount);Assert.Empty(id.Rows);
            var r=importer.Examine(ImportLevel.Normalize);Assert.True(r.Status=="completed",System.Text.Json.JsonSerializer.Serialize(r.Issues));Assert.Equal(spec.Item2,importer.Canonical.Rows.Count);
            Assert.Equal(spec.Item3,importer.Canonical.Rows.Sum(x=>x.AnnualDebitTurnover));Assert.Equal(spec.Item3,importer.Canonical.Rows.Sum(x=>x.AnnualCreditTurnover));
            var total=Assert.Single(r.Rows,x=>x.Kind=="ReportLevel");Assert.Equal(spec.Item3,total.Amounts["turnoverDebit"]);
            Assert.DoesNotContain(r.Rows,x=>x.Kind=="Unresolved");
        }
    }
}
