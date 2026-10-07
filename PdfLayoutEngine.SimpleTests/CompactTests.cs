using PdfLayoutEngine.Simple;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using System.Text.Json;
namespace PdfLayoutEngine.SimpleTests;
public class CompactTests
{
    static string Root=>Path.Combine(AppContext.BaseDirectory,"Compact");
    static CompactLayout[] Catalogue()=>Directory.GetFiles(Root,"*.json").Where(p=>!Path.GetFileName(p).StartsWith("dbf-")&&!Path.GetFileName(p).StartsWith("structured-")).Select(CompactLayout.Load).ToArray();
    public static IEnumerable<object[]> Definitions()=>Directory.GetFiles(Root,"*.json").Where(p=>!Path.GetFileName(p).StartsWith("dbf-")&&!Path.GetFileName(p).StartsWith("structured-")).Select(p=>new object[]{Path.GetFileName(p)});
    [Theory,MemberData(nameof(Definitions))]
    public void Every_layout_recognizes_and_extracts_its_record_block(string file)
    {
        var d=CompactLayout.Load(Path.Combine(Root,file));using var source=new Fixture(d);using var session=new CompactSession(source,Catalogue());
        var r=session.Examine(ImportLevel.Identify);Assert.Equal(d.Id,r.LayoutId);Assert.Equal(1,r.DecodedPageCount);Assert.Empty(r.Rows);
        r=session.Examine(ImportLevel.Validate);Assert.True(r.Status=="completed",JsonSerializer.Serialize(r.Issues));
        var row=Assert.Single(r.Rows,x=>d.ImportKinds.Contains(x.Kind));Assert.NotEmpty(row.Fields);
        if(d.Category=="AJ"){Assert.Equal("01.01.2024",row.Fields["Date"]);Assert.Equal(123.45m,row.Amounts.Values.Single());}
    }
    [Fact]public void Wrong_title_is_not_recognized()
    {
        var d=Catalogue()[0];using var source=new Fixture(d);source.Document.Pages[0].Text.Clear();using var session=new CompactSession(source,Catalogue());Assert.Equal("unsupported",session.Examine(ImportLevel.Identify).Status);
    }
    [Theory,InlineData("3 805 101.04",3805101.04),InlineData("1.234,56",1234.56),InlineData("-12,00",-12)]
    public void Printed_number_formats(string raw,decimal expected){Assert.True(CompactSession.TryMoney(raw,out var actual));Assert.Equal(expected,actual);}
    [Theory,InlineData("abc12,00"),InlineData("1.234"),InlineData("12,3")]
    public void Malformed_amount_is_not_silently_repaired(string raw)=>Assert.False(CompactSession.TryMoney(raw,out _));
    [Fact]public void Supplied_GL_corpus_reconciles_to_printed_turnover()
    {
        string? root=Environment.GetEnvironmentVariable("IFOSOFT_SIMPLE_TEST_ROOT");if(string.IsNullOrEmpty(root))return;
        foreach(var path in Directory.GetFiles(root).Where(p=>p.EndsWith("xps"))){using var importer=new CompactLayoutImporter(path,Root);var r=importer.Examine(ImportLevel.Normalize);Assert.True(r.Status=="completed",path+JsonSerializer.Serialize(r.Issues));var gl=Assert.IsType<GeneralLedgerImport>(importer.Canonical);var total=Assert.Single(r.Rows,x=>x.Kind=="ReportLevel");Assert.Equal(total.Amounts["TurnoverDebit"],gl.Rows.Sum(x=>x.AnnualDebitTurnover));Assert.Equal(total.Amounts["TurnoverCredit"],gl.Rows.Sum(x=>x.AnnualCreditTurnover));}
    }
    [Fact]public void Supplied_repair_samples_preserve_bold_and_classify_AF_placeholders()
    {
        string? root=Environment.GetEnvironmentVariable("IFOSOFT_REPAIR_TEST_ROOT");if(string.IsNullOrEmpty(root))return;
        using(var source=new PdfLayoutEngine.IText.Simple.PdfPageSource(Path.Combine(root,"U_DENNIK_00331503_2023_a.pdf"))){
            var page=source.ReadPage(0);var bold=page.Text.Where(t=>t.Font.Contains("Bold")).ToArray();Assert.NotEmpty(bold);Assert.All(bold,t=>Assert.True(t.FontWeight>=600));
        }
        using(var importer=new CompactLayoutImporter(Path.Combine(root,"UCT_ROZVRH_00323012_2024(1).pdf"),Root)){
            var r=importer.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Single(r.Rows,x=>x.Kind=="EmptyAccount");Assert.Equal(63,r.Rows.Count(x=>x.Kind=="AccountHeading"));
            var af=Assert.IsType<AccountingFrameworkImport>(importer.Canonical);Assert.Equal(569,af.Rows.Count);Assert.DoesNotContain(af.Rows,x=>x.AccountCode.Contains("*"));
        }
        using(var importer=new CompactLayoutImporter(Path.Combine(root,"HL_KNIHA_36731501_2024_a.pdf"),Root)){
            var r=importer.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Equal("ifosoft_predvas3_firma",r.LayoutId);Assert.Equal("2024",CompactSession.Value(r.Identifiers,"fiscalYear"));
            var gl=Assert.IsType<GeneralLedgerImport>(importer.Canonical);Assert.Equal("022",gl.Rows[0].AccountCode);Assert.Equal(7222352.15m,gl.Rows.Sum(x=>x.AnnualDebitTurnover));Assert.Equal(7222352.15m,gl.Rows.Sum(x=>x.AnnualCreditTurnover));
        }
    }
    internal sealed class Fixture:IPageSource
    {
        public string Format=>"fixture";public int PageCount=>1;public int DecodedPageCount{get;private set;}
        public PositionedDocument Document{get;}=new();
        public Fixture(CompactLayout d){
            var p=new PositionedPage{WidthPt=850,HeightPt=900};Document.Pages.Add(p);
            void Put(string text,double[] rect,double baseline,int weight=400,double size=8){
                double x=10+rect[0]*72/d.UnitsPerInch, y=870-baseline*72/d.UnitsPerInch;
                foreach(char c in text){double advance=size*.42;p.Text.Add(new PositionedText{Text=c.ToString(),Baseline=new(x,y),AdvanceEnd=new(x+advance,y),FontSize=size,FontWeight=weight,RunId="r"+baseline+rect[0],Bounds=new(x,y-2,x+advance,y+size)});x+=advance;}
            }
            foreach(var a in d.Anchors)Put(a.Text,a.Rect,a.Rect[1]+a.Size*d.UnitsPerInch/72*.8,400,a.Size);
            foreach(var h in d.Header){string v=h.Key=="PageNumber"?"1":h.Key.StartsWith("Period")?"01/2024":"Entity";Put(v,h.Value,h.Value[1]+(h.Value[3]-h.Value[1])*.75);}
            var data=d.Sections.First(s=>d.ImportKinds.Contains(s.Kind));
            void Section(CompactSection s,double shift){
                foreach(var field in s.Fields){string key=field.Key;string value=s.Amounts.Contains(key)?"123,45":key=="Date"?"01.01.2024":(key.Contains("Account") || key=="Debit" || key=="Credit" || key=="Ordinal")?"123":key.Contains("Name")?"Account":key.Contains("Document")?"7":"";
                    if(s.ItemFields.Contains(key))value="";
                    var a=s.Anchor;double y=field.Value[1]+(field.Value[3]-field.Value[1])*.75+shift;
                    Put(value,field.Value,y,a.Weight??400,a.Size);
                }
                if(s.Anchor.Text.Length>0)Put(s.Anchor.Text,s.Anchor.Rect,s.Anchor.Rect[1]+(s.Anchor.Rect[3]-s.Anchor.Rect[1])*.75+shift,400,s.Anchor.Size);
            }
            Section(data,400-data.Anchor.Rect[1]);
            var footer=d.Sections.FirstOrDefault(s=>s.Kind=="DocumentTotals");if(footer!=null)Section(footer,520-footer.Anchor.Rect[1]);
        }
        public PositionedPage ReadPage(int index,CancellationToken token=default){token.ThrowIfCancellationRequested();DecodedPageCount=1;return Document.Pages[index];}
        public void Dispose(){}
    }
}
