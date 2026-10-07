using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
using PdfLayoutEngine.IText.Simple;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
namespace PdfLayoutEngine.SimpleTests;
public class OmegaLayoutTests
{
    static string Layouts=>Path.Combine(AppContext.BaseDirectory,"Omega");
    static readonly string[] Header={"Stredisko","Účt. obd.","DUÚP","Dátum splatnosti","Interné číslo","Externé číslo","  Partner","  Text","Suma MD [EUR]","Suma DAL [EUR]","MD synt.","MD anal.","DAL synt.","DAL anal."};
    static string[] Row(string date="2024-12-31",string debit="12.34",string credit="12.34",string syn="82",string ana="1")=>new[]{"100","12",date,"2025-01-15","D1","E1","Partner","Description",debit,credit,syn,ana,"321","10"};
    [Fact]public void Flat_journal_preserves_independent_amounts_and_pads_accounts(){
        var p=UrbisLayoutTests.Workbook(new[]{Header,Row(debit:"12.34",credit:"11.11")});
        try{using var i=new CompactLayoutImporter(p,Layouts);var early=i.Examine(ImportLevel.Identify);Assert.Empty(early.Rows);Assert.Equal("2024",early.Identifiers["fiscalYear"]);Assert.Null(early.Identifiers["cin"]);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);
            var journal=Assert.IsType<JournalImport>(i.Canonical);var row=Assert.Single(journal.Rows);Assert.Equal("KROS OMEGA",journal.AccountingFormat);Assert.Equal("082001",row.DebitAccount);Assert.Equal("321010",row.CreditAccount);Assert.Equal(12.34m,row.DebitAmount);Assert.Equal(11.11m,row.CreditAmount);Assert.Equal("D1",row.DocumentNumber);Assert.Equal("100",row.SourceFields["CostCentre"]);Assert.True(row.UsedForReportCalculation);
        }finally{File.Delete(p);}
    }
    [Fact]public void Negative_credit_only_entry_is_preserved(){
        var row=Row(debit:"0",credit:"-2.62",syn:"",ana:"");var p=UrbisLayoutTests.Workbook(new[]{Header,row});
        try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var item=Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows);Assert.Null(item.DebitAmount);Assert.Equal(-2.62m,item.CreditAmount);}finally{File.Delete(p);}
    }
    [Theory,InlineData("12","", "amountWithoutAccount"),InlineData("bad","82","invalidAmount"),InlineData("12","1234","invalidAccount")]
    public void Invalid_side_blocks_canonical(string amount,string syn,string code){
        var p=UrbisLayoutTests.Workbook(new[]{Header,Row(debit:amount,syn:syn,ana:syn.Length==0?"":"1")});
        try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Contains(i.Examine(ImportLevel.Normalize).Issues,x=>x.Code==code);Assert.Null(i.Canonical);}finally{File.Delete(p);}
    }
    [Fact]public void Mixed_years_wait_for_auditor_selection(){
        var p=UrbisLayoutTests.Workbook(new[]{Header,Row(),Row("2025-01-01")});
        try{using(var i=new CompactLayoutImporter(p,Layouts)){i.Examine(ImportLevel.Normalize);var j=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(0,j.FiscalYear);Assert.All(j.Rows,r=>Assert.False(r.UsedForReportCalculation));}
            using(var i=new CompactLayoutImporter(p,Layouts,2024)){i.Examine(ImportLevel.Normalize);var j=Assert.IsType<JournalImport>(i.Canonical);Assert.True(j.Rows[0].UsedForReportCalculation);Assert.False(j.Rows[1].UsedForReportCalculation);}
        }finally{File.Delete(p);}
    }
    [Fact]public void Collapsed_gl_export_is_explicitly_unsupported(){
        var p=UrbisLayoutTests.Workbook(new[]{new[]{"Hlavná kniha (zúžená) v EUR"}});
        try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Equal("unsupported",i.Examine(ImportLevel.Normalize).Status);Assert.Null(i.Canonical);Assert.Equal(1,i.Examine(ImportLevel.Normalize).CompletedLevel);}finally{File.Delete(p);}
    }
    [Fact]public void Separate_side_total_checks_both_sides(){
        var result=new ImportResult();var row=new SourceRow{Kind="Record",Fields=new(){["DebitAccount"]="082001",["CreditAccount"]="321010"},Amounts=new(){["DebitAmount"]=12m,["CreditAmount"]=11m}};
        result.Rows.Add(row);result.Rows.Add(new SourceRow{Kind="ReportTotal",Amounts=new(){["DebitAmount"]=12m,["CreditAmount"]=12m}});
        IndependentControls.Validate(result,"separateSides",new[]{"DebitAmount","CreditAmount"});Assert.Contains(result.Issues,i=>i.Code=="printedTotalMismatch");
    }
    [Fact]public void Gl_requires_matching_synthetic_totals(){
        var result=new ImportResult();result.Rows.Add(new SourceRow{Kind="Record",Account="021001",Amounts=new(){["OpeningNetto"]=10m,["TurnoverDebit"]=2m,["TurnoverCredit"]=1m,["ClosingNetto"]=11m}});
        IndependentControls.Validate(result,"accountTotals",new[]{"OpeningNetto","TurnoverDebit","TurnoverCredit","ClosingNetto"});Assert.Contains(result.Issues,i=>i.Code=="missingAccountTotal");Assert.DoesNotContain(result.Issues,i=>i.Code=="balanceMismatch");
    }
    [Fact]public void Pdf_cache_retains_header_and_current_page_but_counts_all_pages(){
        var p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".pdf");
        try{using(var pdf=new PdfDocument(new PdfWriter(p))){var font=PdfFontFactory.CreateFont(StandardFonts.HELVETICA);for(int n=0;n<4;n++)new PdfCanvas(pdf.AddNewPage()).BeginText().SetFontAndSize(font,10).MoveText(20,700).ShowText("Page "+(n+1)).EndText();}
            using var source=new PdfPageSource(p,retainDecodedPages:false);var first=source.ReadPage(0);for(int n=1;n<4;n++)source.ReadPage(n);Assert.Equal(4,source.DecodedPageCount);Assert.Equal(2,source.Document.Pages.Count);Assert.Same(first,source.ReadPage(0));source.ReadPage(1);Assert.Equal(4,source.DecodedPageCount);
        }finally{File.Delete(p);}
    }
    [Fact]public void Three_pdf_layouts_are_valid(){foreach(var p in Directory.GetFiles(Layouts,"pdf-*.json"))Assert.Equal("KROS OMEGA",CompactLayout.Load(p).Producer);}

    static CompactLayout JournalLayout()=>new(){Version=2,Producer="Fixture",Id="test",Category="AJ",ImportKinds=new[]{"Record"},Table=new(){TitlePattern="^Journal$",RequiredPageText="Created by Export",LeftLabel="Date",RightLabel="Credit",Left=20,Right=260,LastHeaderLabel="Credit",Columns=new(){["Date"]=new[]{20d,100d},["DebitAccount"]=new[]{100d,140d},["CreditAccount"]=new[]{140d,180d},["DebitAmount"]=new[]{180d,260d},["CreditAmount"]=new[]{260d,340d},["OperationDescription"]=new[]{340d,520d}},Amounts=new[]{"DebitAmount","CreditAmount"},Rules=new(){new(){Kind="ReportTotal",Field="Date",Pattern="^Total$"},new(){Kind="Record",Field="Date",Pattern=@"^\d{2}\.\d{2}\.\d{4}$"}},ControlMode="separateSides",BlankAmountsAreZero=true,GroupPattern="(?!)",IgnorePattern="^Created by Export",ContinuationKind="Record",ContinuationFields=new[]{"OperationDescription"}}};
    [Fact]public void Coordinate_journal_handles_blank_side_and_wrapped_text(){
        using var source=new JournalPage();using var session=new CompactSession(source,new[]{JournalLayout()});var result=session.Examine(ImportLevel.Validate);Assert.Equal("completed",result.Status);var record=Assert.Single(result.Rows,r=>r.Kind=="Record");Assert.Equal(0m,record.Amounts["CreditAmount"]);Assert.Equal("First second line",record.Fields["OperationDescription"]);
    }
    [Fact]public void Unknown_numeric_line_is_not_silently_absorbed_as_description(){
        using var source=new JournalPage(extra:true);using var session=new CompactSession(source,new[]{JournalLayout()});Assert.Contains(session.Examine(ImportLevel.Validate).Issues,x=>x.Code=="unresolvedRow");
    }
    [Fact]public void Producer_marker_is_required(){using var source=new JournalPage();var layout=JournalLayout();layout.Table!.RequiredPageText="Different producer";using var session=new CompactSession(source,new[]{layout});Assert.Null(session.Examine(ImportLevel.Recognize).LayoutId);}
    sealed class JournalPage:IPageSource{
        public string Format=>"fixture";public int PageCount=>1;public int DecodedPageCount{get;private set;}public PositionedDocument Document{get;}=new();
        public JournalPage(bool extra=false){var p=new PositionedPage{WidthPt=600,HeightPt=800};Document.Pages.Add(p);int run=0;
            void Put(string text,double x,double y){string id=(run++).ToString();foreach(char c in text){p.Text.Add(new(){Text=c.ToString(),RunId=id,Baseline=new(x,y),AdvanceEnd=new(x+3,y),FontSize=8});x+=3;}}
            Put("Journal",20,770);Put("Strana : 1 / 1",400,750);Put("Date",20,710);Put("Credit",260,710);Put("31.12.2024",20,680);Put("082001",100,680);Put("12.34",220,680);Put("First",340,680);Put("second line",340,670);if(extra)Put("999",100,660);Put("Total",20,630);Put("12.34",220,630);Put("Created by Export",20,30);
        }
        public PositionedPage ReadPage(int index,CancellationToken token=default){DecodedPageCount=1;return Document.Pages[index];}public void Dispose(){}
    }
}
