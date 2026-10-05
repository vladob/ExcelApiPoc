using System.Text;
using ExcelApiPoc.AccountingImport.Services.Dbf;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public sealed class DbfJournalTests
{
    static string Definitions=>Path.Combine(AppContext.BaseDirectory,"Compact");
    static readonly (string name,char type,int width,int scale)[] Fields={ ("DD",'C',2,0),("CDOK",'N',6,0),("DATUM",'D',8,0),("DATVY",'D',8,0),("POPIS",'C',60,0),("SUMA",'N',13,2),("MU",'C',7,0),("DU",'C',7,0),("MSUMA",'N',13,2),("DSUMA",'N',13,2) };
    static byte[] Bytes(string date="20251201",bool deleted=false,string amount="12.34",string secondDate="")
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);var enc=Encoding.GetEncoding(852);int count=secondDate.Length>0?2:1,h=33+Fields.Length*32,len=1+Fields.Sum(f=>f.width);var bytes=new byte[h+count*len+1];
        bytes[0]=3;BitConverter.GetBytes(count).CopyTo(bytes,4);BitConverter.GetBytes((ushort)h).CopyTo(bytes,8);BitConverter.GetBytes((ushort)len).CopyTo(bytes,10);
        for(int i=0;i<Fields.Length;i++){var f=Fields[i];Encoding.ASCII.GetBytes(f.name).CopyTo(bytes,32+i*32);bytes[32+i*32+11]=(byte)f.type;bytes[32+i*32+16]=(byte)f.width;bytes[32+i*32+17]=(byte)f.scale;}
        bytes[h-1]=13;bytes[bytes.Length-1]=26;
        for(int r=0;r<count;r++){
            int at=h+r*len;bytes[at++]=(byte)(deleted&&r==0?'*':' ');
            string[] values={"BU","17",r==0?date:secondDate,r==0?"20251201":secondDate,"Žltý účet",amount,"001A","321",amount,amount};
            for(int i=0;i<Fields.Length;i++){var f=Fields[i];enc.GetBytes(values[i].PadRight(f.width)).CopyTo(bytes,at);at+=f.width;}
        }return bytes;
    }
    static void WithFile(byte[] bytes,Action<string> action){string p=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".DBF");try{File.WriteAllBytes(p,bytes);action(p);}finally{File.Delete(p);}}
    [Fact]public void Stages_preserve_CP852_codes_and_decimal_amounts()=>WithFile(Bytes(),p=>{
        using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Recognize);Assert.Equal("AJ",r.Category);Assert.Empty(r.Rows);Assert.Empty(r.Identifiers);
        r=i.Examine(ImportLevel.Identify);Assert.Empty(r.Rows);Assert.Equal("2025",r.Identifiers["fiscalYear"]);Assert.Null(r.Identifiers["cin"]);
        r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);var row=Assert.Single(a.Rows);Assert.Equal("Žltý účet",row.Description);Assert.Equal("001A",row.DebitAccount);Assert.Equal(12.34m,row.DebitAmount);Assert.Equal("DBF record 1",row.SourceLocation);
    });
    [Theory][InlineData("20250229","invalidDate")][InlineData("20251301","invalidDate")]
    public void Invalid_dates_are_retained_and_excluded(string date,string code)=>WithFile(Bytes(date),p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);Assert.Contains(r.Issues,x=>x.Code==code && x.Severity=="warning");Assert.False(Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows).UsedForReportCalculation);});
    [Fact]public void Invalid_decimal_is_not_repaired()=>WithFile(Bytes(amount:"1,23"),p=>{using var i=new CompactLayoutImporter(p,Definitions);Assert.Equal("invalid",i.Examine(ImportLevel.Normalize).Status);Assert.Null(i.Canonical);});
    [Fact]public void Deleted_records_are_excluded_and_physical_numbers_preserved()=>WithFile(Bytes(deleted:true,secondDate:"20251202"),p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("1",r.Identifiers["deletedRecordCount"]);Assert.Equal(2,Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows).SourceRecordNumber);});
    [Fact]public void Mixed_years_require_review()=>WithFile(Bytes(secondDate:"20260101"),p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Null(r.Identifiers["fiscalYear"]);Assert.Contains(r.Issues,x=>x.Code=="mixedFiscalYears");Assert.All(Assert.IsType<JournalImport>(i.Canonical).Rows,row=>Assert.False(row.UsedForReportCalculation));});
    [Fact]public void Explicit_year_keeps_all_rows_but_excludes_other_years()=>WithFile(Bytes(secondDate:"20260101"),p=>{using var i=new CompactLayoutImporter(p,Definitions,2025);i.Examine(ImportLevel.Normalize);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(2,a.Rows.Count);Assert.True(a.Rows[0].UsedForReportCalculation);Assert.False(a.Rows[1].UsedForReportCalculation);});
    [Fact]public void Implausible_date_is_preserved()=>WithFile(Bytes(date:"02040704"),p=>{using var i=new CompactLayoutImporter(p,Definitions);var r=i.Examine(ImportLevel.Normalize);var row=Assert.Single(Assert.IsType<JournalImport>(i.Canonical).Rows);Assert.Equal("02040704",row.SourceFields["DATUM"]);Assert.False(row.UsedForReportCalculation);Assert.Contains(r.Issues,x=>x.Code=="invalidDate");});
    [Fact]public void Truncated_file_is_rejected()=>WithFile(Bytes().Take(90).ToArray(),p=>Assert.Throws<InvalidDataException>(()=>new DbfTable(p)));
    [Fact]public void Cancellation_does_not_prevent_retry()=>WithFile(Bytes(),p=>{using var i=new CompactLayoutImporter(p,Definitions);Assert.Throws<OperationCanceledException>(()=>i.Examine(ImportLevel.Normalize,new CancellationToken(true)));Assert.Equal("completed",i.Examine(ImportLevel.Normalize).Status);});
    [Fact]public void Supplied_October_and_December_samples()
    {
        string? root=Environment.GetEnvironmentVariable("IFOSOFT_DBF_TEST_ROOT");if(root==null)return;
        foreach(var v in new[]{("U_DENNIK_00323292_202510.DBF",2,68.50m),("U_DENNIK_00323292_202512.DBF",85,18677.05m)}){
            using var i=new CompactLayoutImporter(Path.Combine(root,v.Item1),Definitions);var r=i.Examine(ImportLevel.Normalize);Assert.Equal("completed",r.Status);var a=Assert.IsType<JournalImport>(i.Canonical);Assert.Equal(v.Item2,a.Rows.Count);Assert.Equal(v.Item3,a.Rows.Sum(x=>x.DebitAmount));Assert.Equal(v.Item3,a.Rows.Sum(x=>x.CreditAmount));Assert.Equal(2025,a.FiscalYear);
        }
    }
}
