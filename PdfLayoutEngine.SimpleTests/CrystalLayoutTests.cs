using System.Xml.Linq;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public class CrystalLayoutTests
{
    static string Layouts=>Path.Combine(AppContext.BaseDirectory,"Ives");
    static readonly XNamespace Ns="urn:crystal-reports:schemas:report-detail";
    static XElement Section(params string[] pairs)=>new(Ns+"Section",Enumerable.Range(0,pairs.Length/2).Select(i=>new XElement(Ns+"Field",new XAttribute("FieldName",pairs[2*i]),new XElement(Ns+"Value",pairs[2*i+1]))));
    static XElement Record(string date="2025-01-02T00:00:00",string amount="12.34")=>Section("{uctDennik.Datum_vytv_pohybu}",date,"{uctDennik.Cislo_dokladu}","001","{uctDennik.aU_Text_MD}","211. 001","{uctDennik.aU_Text_Dal}","321","{uctDennik.Obnos}",amount,"{uctDennik.text_operacie}","Description");
    static string Save(params XElement[] sections){var p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".xml");new XDocument(new XElement(Ns+"CrystalReport",sections)).Save(p);return p;}
    static XElement Total(string amount="12.34")=>Section("Sum ({uctDennik.Obnos}, {uctDennik.Kod_meny})",amount);
    [Fact]public void Stages_preserve_source_and_use_value_not_display_format(){var p=Save(Record(),Section("{uctDennik.Kod_strediska}","01"),Total());try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Identify);Assert.Empty(r.Rows);Assert.Null(i.Canonical);Assert.Null(r.Identifiers["cin"]);Assert.Equal("2025",r.Identifiers["fiscalYear"]);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal("Ives",a.AccountingFormat);var row=Assert.Single(a.Rows);Assert.Equal("211.001",row.DebitAccount);Assert.Equal(12.34m,row.DebitAmount);Assert.Equal("01",row.SourceFields["source:{uctDennik.Kod_strediska}"]);Assert.StartsWith("XML section",row.SourceLocation);}finally{File.Delete(p);}}
    [Theory,InlineData("bad","12.34","invalidAmount"),InlineData("12.34","13.34","groupTotalMismatch")]
    public void Invalid_amount_or_control_blocks_canonical(string amount,string total,string code){var p=Save(Record(amount:amount),Total(total));try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Contains(i.Examine(ImportLevel.Normalize).Issues,v=>v.Code==code);Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Fact]public void Missing_control_blocks_canonical(){var p=Save(Record());try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Contains(i.Examine(ImportLevel.Normalize).Issues,v=>v.Code=="missingGroupTotal");Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    [Fact]public void Invalid_date_is_retained_and_excluded(){var p=Save(Record("2025-99-01"),Total());try{using var i=new CompactLayoutImporter(p,Layouts,2025);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var row=Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows);Assert.Equal("2025-99-01",row.SourceFields["source:{uctDennik.Datum_vytv_pohybu}"]);Assert.Equal(JournalDateExceptionResolution.Excluded,row.DateExceptionResolution);}finally{File.Delete(p);}}
    [Fact]public void Mixed_years_without_selection()=>MixedYears(null);
    [Fact]public void Mixed_years_with_selection()=>MixedYears(2025);
    static void MixedYears(int? year){var p=Save(Record(),Record("2024-12-31T00:00:00"),Total("24.68"));try{using var i=new CompactLayoutImporter(p,Layouts,year);i.Examine(ImportLevel.Normalize);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(year??0,a.FiscalYear);Assert.Equal(JournalDateExceptionResolution.Excluded,a.Rows[1].DateExceptionResolution);if(year==null)Assert.Equal(JournalDateExceptionResolution.Excluded,a.Rows[0].DateExceptionResolution);}finally{File.Delete(p);}}
    [Fact]public void Wrong_namespace_is_unrecognized(){var p=Save(Record(),Total());try{File.WriteAllText(p,File.ReadAllText(p).Replace(Ns.NamespaceName,"urn:other"));using var i=new CompactLayoutImporter(p,Layouts);Assert.Null(i.Examine(ImportLevel.Normalize).LayoutId);}finally{File.Delete(p);}}
    [Fact]public void Malformed_trailing_xml_is_not_silently_imported(){var p=Save(Record(),Total());try{File.AppendAllText(p,"<broken>");using var i=new CompactLayoutImporter(p,Layouts);Assert.Throws<System.Xml.XmlException>(()=>i.Examine(ImportLevel.Normalize));Assert.Null(i.Canonical);}finally{File.Delete(p);}}
}
