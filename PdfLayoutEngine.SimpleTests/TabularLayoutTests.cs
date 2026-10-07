using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public class TabularLayoutTests
{
    [Fact]public void Six_Ives_definitions_load(){var files=Directory.GetFiles(Path.Combine(AppContext.BaseDirectory,"Ives"),"*.json").Where(p=>!Path.GetFileName(p).StartsWith("structured-")&&!Path.GetFileName(p).StartsWith("crystal-")).ToArray();Assert.Equal(6,files.Length);foreach(var file in files){var d=CompactLayout.Load(file);Assert.Equal("Ives",d.Producer);Assert.NotNull(d.Table);}}
    static CompactLayout Layout()=>new(){Version=2,Producer="Ives",Id="test",Category="AJ",ImportKinds=new[]{"AjRecord"},Table=new(){TitlePattern="^Journal$",LeftLabel="Date",RightLabel="Amount",Left=20,Right=400,LastHeaderLabel="Amount",Columns=new(){["Date"]=new[]{20d,100d},["DebitAccount"]=new[]{100d,200d},["CreditAccount"]=new[]{200d,300d},["Amount"]=new[]{300d,460d}},Amounts=new[]{"Amount"},Rules=new(){new(){Kind="GroupTotal",Field="Date",Pattern="^Spolu"},new(){Kind="AjRecord",Field="Date",Pattern=@"^\d{1,2}\.\d{1,2}\.\d{4}$"}},ContinuationKind="AjRecord",ContinueAcrossPages=true}};
    [Theory,InlineData(1d,0d),InlineData(.8,12d),InlineData(1.2,7d)]
    public void Scale_offsets_and_independent_fonts_preserve_amounts(double scale,double offset){
        using var s=new Fixture(scale,offset);using var session=new CompactSession(s,new[]{Layout()});
        Assert.Equal(2,session.Examine(ImportLevel.Identify).CompletedLevel);Assert.Equal(1,s.DecodedPageCount);
        var result=session.Examine(ImportLevel.Validate);Assert.Equal("completed",result.Status);Assert.Equal(12.34m,Assert.Single(result.Rows,r=>r.Kind=="AjRecord").Amounts["Amount"]);
    }
    [Fact]public void Currency_outside_amount_column_is_not_part_of_the_amount(){using var s=new Fixture(1,0,"12,34",true);using var session=new CompactSession(s,new[]{Layout()});Assert.Equal("completed",session.Examine(ImportLevel.Validate).Status);}
    [Fact]public void Wrong_printed_group_total_blocks_import(){using var s=new Fixture(1,0,"13,00");using var session=new CompactSession(s,new[]{Layout()});Assert.Contains(session.Examine(ImportLevel.Validate).Issues,i=>i.Code=="groupTotalMismatch");}
    [Fact]public void Invalid_column_is_rejected(){var d=Layout();d.Table!.Columns["Amount"]=new[]{4d,1d};Assert.Throws<InvalidDataException>(()=>d.Table.Validate());}
    [Fact]public void Source_order_separates_overlapping_description_digits(){using var s=new Fixture(1,0,overlap:true);var d=Layout();d.Table!.AmountsInSourceOrder=true;using var session=new CompactSession(s,new[]{d});Assert.Equal("completed",session.Examine(ImportLevel.Validate).Status);}
    [Fact]public void Wrong_title_does_not_match(){using var s=new Fixture(1,0);var d=Layout();d.Table!.TitlePattern="^Other$";using var session=new CompactSession(s,new[]{d});Assert.Null(session.Examine(ImportLevel.Recognize).LayoutId);}
    sealed class Fixture:IPageSource{
        public string Format=>"fixture";public int PageCount=>1;public int DecodedPageCount{get;private set;}public PositionedDocument Document{get;}=new();
        public Fixture(double scale,double offset,string total="12,34",bool currency=false,bool overlap=false){
            var p=new PositionedPage{WidthPt=650,HeightPt=850};Document.Pages.Add(p);int id=0;
            void Put(string text,double x,double y){string run=(id++).ToString();foreach(char c in text){p.Text.Add(new(){Text=c.ToString(),RunId=run,Id=run+"-"+x,Baseline=new(offset+x*scale,850-y*scale),FontSize=13,AdvanceEnd=new(offset+(x+3)*scale,850-y*scale)});x+=3;}}
            Put("Entity",20,20);Put("Journal",220,35);Put("Strana : 1 / 1",350,60);Put("Date",20,90);Put("Amount",400,90);Put("Hlavná činnosť EUR",20,120);
            Put("1.1.2024",20,150);Put("321",110,150);Put("221",210,150);if(overlap)Put("reference 3000",410,150);Put(currency?"12,34 €":"12,34",440,150);Put("Spolu",20,180);Put(total,440,180);
        }
        public PositionedPage ReadPage(int i,CancellationToken token=default){token.ThrowIfCancellationRequested();DecodedPageCount=1;return Document.Pages[i];}public void Dispose(){}
    }
}
