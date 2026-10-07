using System.IO.Compression;
using System.Xml.Linq;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public class UrbisLayoutTests
{
    static string Layouts=>Path.Combine(AppContext.BaseDirectory,"Urbis");
    // Minimal OOXML test fixtures: no real entity data or external spreadsheet dependency.
    internal static string Workbook(string[][] rows,int sheets=1){
        string p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xlsx");
        using(var zip=ZipFile.Open(p,ZipArchiveMode.Create)){
            void Put(string name,string text){using var w=new StreamWriter(zip.CreateEntry(name).Open());w.Write(text);}
            const string ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            Put("[Content_Types].xml","<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'><Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/><Default Extension='xml' ContentType='application/xml'/><Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/></Types>");
            Put("_rels/.rels","<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>");
            Put("xl/workbook.xml","<workbook xmlns='"+ns+"' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><sheets>"+string.Concat(Enumerable.Range(1,sheets).Select(i=>"<sheet name='Export"+i+"' sheetId='"+i+"' r:id='rId"+i+"'/>"))+"</sheets></workbook>");
            Put("xl/_rels/workbook.xml.rels","<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'>"+string.Concat(Enumerable.Range(1,sheets).Select(i=>"<Relationship Id='rId"+i+"' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet"+i+".xml'/>"))+"</Relationships>");
            XNamespace n=ns;var sheet=new XElement(n+"worksheet",new XElement(n+"sheetData",rows.Select((row,i)=>new XElement(n+"row",new XAttribute("r",i+1),row.Select((v,j)=>new XElement(n+"c",new XAttribute("r",ColumnName(j)+(i+1)),new XAttribute("t","inlineStr"),new XElement(n+"is",new XElement(n+"t",v))))))));
            for(int i=1;i<=sheets;i++)Put("xl/worksheets/sheet"+i+".xml",sheet.ToString());
        }return p;
    }
    static string ColumnName(int index) { string name=""; for(int n=index+1;n>0;n=(n-1)/26) name=(char)('A'+(n-1)%26)+name; return name; }
    static string[][] Journal(string date="1.1.2025",string amount="12,34",string total="*12.34")=>new[]{new[]{"Pč","Dátum","Poznámka","Suma","Účet MD","Účet Dal",""},new[]{"","","","","","","Doklad: B1"},new[]{"1",date,"Test",amount,"021A","321",""},new[]{"","","","",total,total,"Spolu za doklad B1"},new[]{"","","","",total,total,"Spolu"}};
    [Fact]public void Recognition_uses_content_and_L2_retains_no_rows(){var p=Workbook(Journal());try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Identify);Assert.Equal("urbis-aj-document",r.LayoutId);Assert.Equal("2025",r.Identifiers["fiscalYear"]);Assert.Null(r.Identifiers["cin"]);Assert.Empty(r.Rows);Assert.Null(i.Canonical);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal("Urbis",a.AccountingFormat);var row=Assert.Single(a.Rows);Assert.Equal("B1",row.DocumentNumber);Assert.Equal("021A",row.DebitAccount);Assert.Equal(12.34m,row.DebitAmount);}finally{File.Delete(p);}}
    [Fact]public void Multiple_sheets_are_rejected(){var p=Workbook(Journal(),2);try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Throws<InvalidDataException>(()=>i.Examine(ImportLevel.Recognize));}finally{File.Delete(p);}}
    [Fact]public void Unrelated_workbook_is_unrecognized(){var p=Workbook(new[]{new[]{"Audit workbook","Notes"}});try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Null(i.Examine(ImportLevel.Normalize).LayoutId);Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Theory,InlineData("bad","*12.34","invalidAmount"),InlineData("12.34","*13.34","printedTotalMismatch")]
    public void Bad_values_block_publication(string amount,string total,string code){var p=Workbook(Journal(amount:amount,total:total));try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Contains(i.Examine(ImportLevel.Normalize).Issues,x=>x.Code==code);Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Fact]public void Invalid_dates_are_retained_for_review(){var p=Workbook(Journal("31.2.2025"));try{using var i=new CompactLayoutImporter(p,Layouts,2025);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var row=Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows);Assert.Equal("31.2.2025",row.SourceFields["source:2"]);Assert.Equal(JournalDateExceptionResolution.Excluded,row.DateExceptionResolution);}finally{File.Delete(p);}}
    [Fact]public void Missing_document_total_blocks_publication(){var rows=Journal().Where((v,n)=>n!=3).ToArray();var p=Workbook(rows);try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Contains(i.Examine(ImportLevel.Normalize).Issues,x=>x.Code=="missingDocumentTotal");Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Fact]public void Daily_totals_do_not_include_other_single_record_days(){
        var rows=new[]{new[]{"Doklad","Dátum","Poznámka","Suma","Účet MD","Účet Dal",""},new[]{"B1","1.1.2025","One","1","211","321",""},new[]{"B2","2.1.2025","Two","2","211","321",""},new[]{"","","","","*2","*2","Spolu za 2.1.2025"},new[]{"","","","","3","3","Spolu"}};
        var p=Workbook(rows);try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);Assert.Equal(2,Assert.IsType<JournalImport>(i.Canonical).Rows.Count);}finally{File.Delete(p);}}
    [Fact]public void Gl_period_is_taken_from_header(){
        var p=Workbook(new[]{new[]{"Účet","Popis","Počiatočný stav","Obraty od 1.1.2024 do 31.3.2024","Konečný stav","","",""},new[]{"","","k 1.1.2024","Má dať","k 31.3.2024","Dal","",""},new[]{"021A","Building","10","2","11","1","A",""},new[]{"","","10","2","11","1","","Spolu"}});
        try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var g=Assert.IsType<GeneralLedgerImport>(i.Canonical);Assert.Equal(2024,g.FiscalYear);Assert.Equal(3,g.ThroughMonth);Assert.Equal(11m,Assert.Single(g.Rows).SourceClosingNet);}finally{File.Delete(p);}}
    [Fact]public void Account_framework_keeps_headings_out_of_canonical(){
        var headers=new[]{"Pč","Účet","Názov","A/P","RS","HS"}.Concat(Enumerable.Range(1,15).Select(n=>"An"+n)).ToArray();
        string[] Row(string number,string account,string name,string type)=>new[]{number,account,name,type,"N","N"}.Concat(Enumerable.Repeat("",15)).ToArray();
        var p=Workbook(new[]{headers,Row("1","0","Class","T"),Row("2","01","Group","K"),Row("3","012","Synthetic","A"),Row("4","012A","Analytical","A")});
        try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Equal(4,r.Rows.Count);Assert.Null(r.Identifiers["fiscalYear"]);var a=Assert.IsType<AccountingFrameworkImport>(i.Canonical);Assert.Equal(2,a.Rows.Count);Assert.Equal(AccountingFrameworkRowKind.SyntheticAccount,a.Rows[0].RowKind);Assert.Equal(AccountingFrameworkRowKind.AnalyticalAccount,a.Rows[1].RowKind);}finally{File.Delete(p);}}
    [Fact]public void Extra_journal_columns_are_not_accepted(){var rows=Journal().Select(v=>v.Concat(new[]{"extra"}).ToArray()).ToArray();var p=Workbook(rows);try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Null(i.Examine(ImportLevel.Normalize).LayoutId);}finally{File.Delete(p);}}
    [Fact]public void Three_pdf_layouts_load(){foreach(var p in Directory.GetFiles(Layouts,"pdf-*.json"))Assert.Equal("Urbis",CompactLayout.Load(p).Producer);}
}
