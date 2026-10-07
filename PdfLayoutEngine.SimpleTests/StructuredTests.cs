using System.Text;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public sealed class StructuredTests
{
    static string Layouts=>Path.Combine(AppContext.BaseDirectory,"Compact");
    const string Header="DD;CisloD;Datum;Popis operacie;Ucet_MD;Pol_MD;Zdr_MD;Str_MD;Zak_MD;Suma_MD;Ucet_D;Odd_D;Pol_D;Zdr_D;Str_D;Zak_D;Suma_D";
    static string Csv(string date="01.01.2024",string amount="12,34")=>"20250303_1137 OBEC TEST\nUctovny dennik\n"+Header+"\n1;001;"+date+";test;211;;;;;"+amount+";321;;;;;;"+amount+"\n";
    static string Save(string text,string ext){Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);var p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+ext);File.WriteAllText(p,text,Encoding.GetEncoding(1250));return p;}
    [Fact] public void Csv_stages_preserve_identification_and_defer_canonical()
    {var p=Save(Csv("1.1.2024"),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Recognize);Assert.Equal("AJ",r.Category);Assert.Empty(r.Rows);Assert.Empty(r.Identifiers);r=i.Examine(ImportLevel.Identify);Assert.Equal("2024",r.Identifiers["fiscalYear"]);Assert.Null(r.Identifiers["cin"]);Assert.Empty(r.Rows);Assert.Null(i.Canonical);i.Examine(ImportLevel.Validate);Assert.Null(i.Canonical);r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(12.34m,Assert.Single(a.Rows).DebitAmount);}finally{File.Delete(p);}}
    [Fact] public void Invalid_amount_is_not_published()
    {var p=Save(Csv(amount:"wrong"),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("invalid",r.Status);Assert.Null(i.Canonical);Assert.Contains(r.Issues,x=>x.Code=="invalidRecord");}finally{File.Delete(p);}}
    [Fact] public void Undated_row_without_year_is_retained_and_excluded()
    {var p=Save(Csv(""),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(0,a.FiscalYear);Assert.Equal(JournalDateExceptionResolution.Excluded,Assert.Single(a.Rows).DateExceptionResolution);}finally{File.Delete(p);}}
    [Fact] public void Cancelled_session_can_retry()
    {var p=Save(Csv(),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Throws<OperationCanceledException>(()=>i.Examine(ImportLevel.Normalize,new CancellationToken(true)));Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);}finally{File.Delete(p);}}
    [Fact] public void Xml_dtd_is_prohibited()
    {var p=Save("<!DOCTYPE uctovny_vykaz [<!ENTITY x 'secret'>]><uctovny_vykaz><typDoc>UCT_VETA</typDoc><vety/></uctovny_vykaz>",".xml");try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Throws<System.Xml.XmlException>(()=>i.Examine(ImportLevel.Recognize));}finally{File.Delete(p);}}
    [Theory]
    [InlineData("U_DENNIK_00323110_2024(1).CSV",6229,2024,5262005.17)]
    [InlineData("U_DENNIK_00323110_2024.xml",11603,2024,5262005.17)]
    [InlineData("U_DENNIK_00323110_2025.xml",11257,2025,6472424.52)]
    public void Real_journals_have_expected_source_totals(string name,int count,int year,double total)
    {var root=Environment.GetEnvironmentVariable("IFOSOFT_STRUCTURED_TEST_ROOT");if(root==null)return;using var i=new CompactLayoutImporter(Path.Combine(root,name),Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(count,a.Rows.Count);Assert.Equal(year,a.FiscalYear);Assert.Equal((decimal)total,a.Rows.Sum(x=>x.DebitAmount??0));Assert.Equal((decimal)total,a.Rows.Sum(x=>x.CreditAmount??0));}
    [Fact] public void Xml_bad_control_blocks_canonical()
    {var p=Save("<uctovny_vykaz><typDoc>UCT_VETA</typDoc><identifikacia><identifikator><ico>00323110</ico><nazov>Test</nazov></identifikator></identifikacia><obdobie><rok>2024</rok></obdobie><vety></vety><sucetKontrola>1</sucetKontrola></uctovny_vykaz>",".xml");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("invalid",r.Status);Assert.Contains(r.Issues,x=>x.Code=="xmlControlMismatch");Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Theory]
    [InlineData("HL_KNIHA_00323110_2023.CSV",424)]
    [InlineData("HL_KNIHA_00323110_2024(1).CSV",394)]
    [InlineData("UCT_ROZVRH_00323110_2024(1).CSV",620)]
    public void Real_account_exports_preserve_rows(string name,int count)
    {var root=Environment.GetEnvironmentVariable("IFOSOFT_STRUCTURED_TEST_ROOT");if(root==null)return;using var i=new CompactLayoutImporter(Path.Combine(root,name),Layouts);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);if(i.Canonical is GeneralLedgerImport gl)Assert.Equal(count,gl.Rows.Count);else Assert.Equal(count,Assert.IsType<AccountingFrameworkImport>(i.Canonical).Rows.Count);}
}
