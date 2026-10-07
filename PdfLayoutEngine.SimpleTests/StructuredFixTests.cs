using System.Text;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public sealed class StructuredFixTests
{
    static string Layouts=>Path.Combine(AppContext.BaseDirectory,"Compact");
    static string Save(string text,string extension){Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);var p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+extension);File.WriteAllText(p,text,Encoding.GetEncoding(1250));return p;}
    const string Header="DD;CisloD;Datum;Popis operacie;Ucet_MD;Pol_MD;Zdr_MD;Str_MD;Zak_MD;Suma_MD;Ucet_D;Odd_D;Pol_D;Zdr_D;Str_D;Zak_D;Suma_D";
    static string Csv(string description)=>"00323110 Test\nUctovny dennik\n"+Header+"\n1;1;01.01.2024;"+description+";211;;;;;12,34;321;;;;;;12,34\n";
    [Theory]
    [InlineData("\"PD \"Novostavba\"","PD \"Novostavba")]
    [InlineData("\"traktor\"\"","traktor\"")]
    [InlineData("\"proper \"\"quote\"\"\"","proper \"quote\"")]
    [InlineData("\"two\nlines\"","two\r\nlines")]
    public void Quotes_preserve_text_and_amounts(string text,string expected)
    {var p=Save(Csv(text),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(expected,Assert.Single(a.Rows).SourceFields["Description"]);Assert.Equal(12.34m,a.Rows[0].DebitAmount);}finally{File.Delete(p);}}
    [Fact] public void Ambiguous_quotes_do_not_publish_rows()
    {var p=Save(Csv("\"bad\";shifted\""),".csv");try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Throws<InvalidDataException>(()=>i.Examine(ImportLevel.Normalize));Assert.Null(i.Canonical);}finally{File.Delete(p);}}
    static string Entry(string date,string year,string month,string md,string dal)=>"<veta><ucSuv>211</ucSuv><ucPripDat>"+date+"</ucPripDat><rok>"+year+"</rok><mes>"+month+"</mes><md>"+md+"</md><dal>"+dal+"</dal></veta>";
    [Theory]
    [InlineData("28.02.0222","0222","02")]
    [InlineData("24.63.8031","8031","63")]
    [InlineData("02.06.0202","0202","06")]
    public void Bad_dates_keep_amounts_and_do_not_pollute_year(string date,string year,string month)
    {var p=Save("<uctovny_vykaz><typDoc>UCT_VETA</typDoc><obdobie><rok>2025</rok></obdobie><vety>"+Entry("01.01.2025","2025","01","1,00","")+Entry(date,year,month,"","1,00")+"</vety><sucetKontrola>3,00</sucetKontrola></uctovny_vykaz>",".xml");try{using var i=new CompactLayoutImporter(p,Layouts);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Equal("2025",r.Identifiers["fiscalYear"]);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(2,a.Rows.Count);Assert.Equal(1m,a.Rows[1].CreditAmount);Assert.Equal(date,a.Rows[1].SourceFields["ucPripDat"]);Assert.Equal(JournalDateExceptionResolution.Excluded,a.Rows[1].DateExceptionResolution);Assert.Equal(DateTime.MinValue,a.Rows[1].PostingDate);}finally{File.Delete(p);}}
    [Theory]
    [InlineData("Ucet;Datum;DruhDokladu;Doklad;Popis operacie;MD;D;Rozdiel;Protiucet","022;01.01.2024;PS;1;Opening;10,00;10,00;0,00;701")]
    [InlineData("Ucet;DatumUskut;DatumVystav;DruhDokladu;Doklad;Popis operacie;MD;D;Rozdiel;Protiucet;ICO;DIC;ICDPH;FIRMA;FAKTC","022;01.01.2024;02.01.2024;PS;1;Opening;10,00;10,00;0,00;701;;;;;")]
    public void Business_journals_map_sides_and_posting_date(string headers,string row)
    {var p=Save("00323110 Test\nUctovny dennik\n"+headers+"\n"+row,".csv");try{using var i=new CompactLayoutImporter(p,Layouts);Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);var r=Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows);Assert.Equal("022",r.DebitAccount);Assert.Equal("701",r.CreditAccount);Assert.Equal(new DateTime(2024,1,1),r.PostingDate);Assert.Equal(10m,r.DebitAmount);Assert.Equal(10m,r.CreditAmount);}finally{File.Delete(p);}}
}
