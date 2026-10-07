using System.Text;
using System.Text.Json;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;

public sealed class DbfAccountTests
{
    static string Definitions=>Path.Combine(AppContext.BaseDirectory,"Compact");
    static void Fixture(string category,Action<string> action,params Dictionary<string,string>[] rows)
    {
        string def=category=="GL"?"dbf-general-ledger.json":"dbf-account-framework.json";
        using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(Definitions,def)));
        var fields=doc.RootElement.GetProperty("requiredFields").EnumerateObject().Select(x=>(name:x.Name,type:x.Value.GetString()![0],width:x.Value.GetString()=="N"?15:60)).ToArray();
        int h=33+fields.Length*32,len=1+fields.Sum(x=>x.width);var b=new byte[h+rows.Length*len+1];b[0]=3;
        BitConverter.GetBytes(rows.Length).CopyTo(b,4);BitConverter.GetBytes((ushort)h).CopyTo(b,8);BitConverter.GetBytes((ushort)len).CopyTo(b,10);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);var enc=Encoding.GetEncoding(852);
        for(int i=0;i<fields.Length;i++){var f=fields[i];int at=32+i*32;Encoding.ASCII.GetBytes(f.name).CopyTo(b,at);b[at+11]=(byte)f.type;b[at+16]=(byte)f.width;b[at+17]=(byte)(f.type=='N'?2:0);}
        b[h-1]=13;b[b.Length-1]=26;
        for(int i=0;i<rows.Length;i++){
            int at=h+i*len;b[at++]=(byte)(rows[i].ContainsKey("deleted")?'*':' ');
            foreach(var f in fields){string value=rows[i].TryGetValue(f.name,out var v)?v:f.type=='N'?"0.00":"";enc.GetBytes(value.PadRight(f.width)).CopyTo(b,at);at+=f.width;}
        }
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".dbf");try{File.WriteAllBytes(path,b);action(path);}finally{File.Delete(path);}
    }
    static Dictionary<string,string> Gl()=>new(){["UCET"]="001A",["NAZOV"]="Žltý účet",["MES"]="12/2025",["PM"]="100.00",["PD"]="30.00",["RM"]="9.25",["RD"]="12.00",["MM"]="2.00",["MD"]="3.00",["KM"]="-2.75",["KD"]="0.00",["ODD"]="0111",["STR"]="CO"};
    static Dictionary<string,string> Af(string syn="001",string ana="A")=>new(){["UCET"]=syn+ana,["SYN"]=syn,["ANA"]=ana,["NAZOV"]="Žltý účet"};
    [Fact]public void Gl_stages_preserve_signed_gross_balances_dimensions_and_leading_zeros()=>Fixture("GL",p=>{
        using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Recognize);Assert.Equal("GL",r.Category);Assert.Empty(r.Identifiers);Assert.Empty(r.Rows);
        r=i.Examine(ImportLevel.Identify);Assert.Equal("2025",r.Identifiers["fiscalYear"]);Assert.Empty(r.Rows);Assert.Null(i.Canonical);
        r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var gl=Assert.IsType<GeneralLedgerImport>(i.Canonical);var row=Assert.Single(gl.Rows);
        Assert.Equal(12,gl.ThroughMonth);Assert.Equal("001A",row.AccountCode);Assert.Equal("001",row.SyntheticCode);Assert.Equal("A",row.AnalyticalCode);Assert.Equal("Žltý účet",row.AccountName);
        Assert.Equal(100m,row.OpeningDebit);Assert.Equal(30m,row.OpeningCredit);Assert.Equal(-2.75m,row.ClosingDebit);Assert.Equal(9.25m,row.AnnualDebitTurnover);Assert.Equal(2m,row.PeriodDebitTurnover);Assert.Equal("0111",row.Section);Assert.Equal("CO",row.CostCenter);Assert.Null(row.SourceOpeningNet);
    },Gl());
    [Theory][InlineData("PM","1,20","invalidAmount")][InlineData("MES","15/2025","invalidPeriod")][InlineData("UCET","","missingAccount")]
    public void Gl_invalid_data_blocks_normalization(string key,string value,string code){var row=Gl();row[key]=value;Fixture("GL",p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("invalid",r.Status);Assert.Contains(r.Issues,x=>x.Code==code);Assert.Null(i.Canonical);},row);}
    [Fact]public void Mixed_gl_snapshots_are_not_combined(){var second=Gl();second["MES"]="11/2025";Fixture("GL",p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Contains(r.Issues,x=>x.Code=="mixedPeriods");Assert.Null(i.Canonical);},Gl(),second);}
    [Fact]public void Caller_year_cannot_silently_override_gl_source()=>Fixture("GL",p=>{using var i=new CompactLayoutImporter(p,Definitions,2024);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("2025",r.Identifiers["fiscalYear"]);Assert.Contains(r.Issues,x=>x.Code=="fiscalYearConflict");Assert.Null(i.Canonical);},Gl());
    [Fact]public void Af_filters_headings_retains_source_and_does_not_infer_year()=>Fixture("AF",p=>{
        using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Equal(4,r.Rows.Count);Assert.Null(r.Identifiers["fiscalYear"]);
        Assert.DoesNotContain(r.Issues, x => x.Code == "fiscalYearUnavailable");
        Assert.DoesNotContain(r.Issues, x => x.Message.Contains("Filename is not evidence"));
        var af=Assert.IsType<AccountingFrameworkImport>(i.Canonical);Assert.Equal(0,af.FiscalYear);Assert.Equal(2,af.Rows.Count);Assert.Equal(3,af.Rows[0].SourceRecordNumber);Assert.Equal(AccountingFrameworkRowKind.SyntheticAccount,af.Rows[0].RowKind);Assert.Equal(AccountingFrameworkRowKind.AnalyticalAccount,af.Rows[1].RowKind);Assert.Equal("001A",af.Rows[1].AccountCode);Assert.Equal("001A",af.Rows[1].SourceFields["UCET"]);
    },new(),Af("01","****"),Af("001",""),Af());
    [Fact]public void Af_context_year_is_distinct_from_observed_year()=>Fixture("AF",p=>{using var i=new CompactLayoutImporter(p,Definitions,2025);var r=i.Examine(ImportLevel.Normalize);Assert.Null(r.Identifiers["fiscalYear"]);Assert.Equal("2025",r.Identifiers["selectedFiscalYear"]);Assert.Equal(2025,Assert.IsType<AccountingFrameworkImport>(i.Canonical).FiscalYear);},Af());
    [Fact]
    public void Af_account_comparison_normalizes_both_sides_and_preserves_source()
    {
        Fixture("AF", path => {
            var af = StagedImportRuntime.Import<AccountingFrameworkImport>(path, "IfoSoft", "AF");
            var row = Assert.Single(af.Rows);
            Assert.Equal("336UNM", row.AccountCode);
            Assert.Equal("336UN M", row.SourceFields["UCET"]);
            Assert.Equal("UN M", row.SourceAnalyticalCode);
        }, Af("336", "UN M"));
    }
    [Fact]public void Af_inconsistent_code_is_not_guessed(){var row=Af();row["UCET"]="999X";Fixture("AF",p=>{using var i=new CompactLayoutImporter(p,Definitions);Assert.Equal("invalid",i.Examine(ImportLevel.Normalize).Status);Assert.Null(i.Canonical);},row);}
    [Fact]public void Unusual_af_synthetic_is_preserved_with_warning()=>Fixture("AF",p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Contains(r.Issues,x=>x.Code=="unusualSyntheticAccount");Assert.Equal("75D",Assert.Single(Assert.IsType<AccountingFrameworkImport>(i.Canonical).Rows).SourceSyntheticCode);},Af("75D","HZ"));
    [Fact]public void Deleted_gl_rows_excluded_and_dimensions_not_aggregated(){var deleted=Gl();deleted["deleted"]="yes";var second=Gl();second["ODD"]="0222";Fixture("GL",p=>{using var i=new CompactLayoutImporter(p,Definitions);i.Examine(ImportLevel.Normalize);var gl=Assert.IsType<GeneralLedgerImport>(i.Canonical);Assert.Equal(2,gl.Rows.Count);Assert.Equal(2,gl.Rows[0].SourceRecordNumber);Assert.Equal("0222",gl.Rows[1].Section);},deleted,Gl(),second);}
    [Theory][InlineData("GL")][InlineData("AF")]
    public void Cancellation_can_retry(string category)=>Fixture(category,p=>{using var i=new CompactLayoutImporter(p,Definitions);Assert.Throws<OperationCanceledException>(()=>i.Examine(ImportLevel.Normalize,new CancellationToken(true)));Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);},category=="GL"?Gl():Af());

    [Theory]
    [InlineData("HL_KNIHA_00331554_2025.DBF","GL",168)]
    [InlineData("HL_KNIHA_00325937_2025_a.DBF","GL",933)]
    [InlineData("HL_KNIHA_00323489_2025_b.DBF","GL",335)]
    [InlineData("HL_KNIHA_00323861_2025.DBF","GL",270)]
    [InlineData("UCT_ROZVRH_00331554_2025.DBF","AF",551)]
    [InlineData("UCT_ROZVRH_00325937_2025.DBF","AF",1704)]
    [InlineData("UCT_ROZVRH_00323489_2025_a.DBF","AF",703)]
    public void Supplied_account_samples(string name,string category,int sourceCount)
    {
        string? root=Environment.GetEnvironmentVariable("IFOSOFT_DBF_ACCOUNT_TEST_ROOT");if(root==null)return;
        using var i=new CompactLayoutImporter(Path.Combine(root,name),Definitions);var r=i.Examine(ImportLevel.Normalize);
        Assert.Equal("completed",r.Status);Assert.Equal(category,r.Category);Assert.Equal(sourceCount,r.Rows.Count);Assert.DoesNotContain(r.Issues,x=>x.Severity=="error");
        if(category=="GL"){
            var gl=Assert.IsType<GeneralLedgerImport>(i.Canonical);Assert.Equal(sourceCount,gl.Rows.Count);Assert.Equal(2025,gl.FiscalYear);
            Assert.Equal(r.Rows.Sum(x=>decimal.Parse(x.Fields["PM"],System.Globalization.CultureInfo.InvariantCulture)),gl.Rows.Sum(x=>x.OpeningDebit));
            Assert.Equal(r.Rows.Sum(x=>decimal.Parse(x.Fields["KD"],System.Globalization.CultureInfo.InvariantCulture)),gl.Rows.Sum(x=>x.ClosingCredit));
        }else {var af=Assert.IsType<AccountingFrameworkImport>(i.Canonical);Assert.Equal(r.Rows.Count(x=>x.Kind!="Empty"&&x.Kind!="GroupHeading"),af.Rows.Count);Assert.All(af.Rows,x=>Assert.DoesNotContain("****",x.AccountCode));}
    }
}
