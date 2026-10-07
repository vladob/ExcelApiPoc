using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace ExcelApiPoc.AccountingImport.Services.Dbf
{
public sealed class DbfJournalDefinition
{
    public string Id {get;set;}="";
    public int Version {get;set;}
    public string Format {get;set;}="";
    public string Category {get;set;}="";
    public int Encoding {get;set;}
    public Dictionary<string,string> RequiredFields {get;set;}=new Dictionary<string,string>();
    public Dictionary<string,string> Map {get;set;}=new Dictionary<string,string>();
    public static DbfJournalDefinition Load(string path)=>JsonSerializer.Deserialize<DbfJournalDefinition>(File.ReadAllText(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow});
}
public sealed class IfoSoftDbfJournalImporter : IDisposable
{
    readonly int? selectedFiscalYear;
    readonly string path;readonly DbfTable table;readonly DbfJournalDefinition definition;readonly Encoding encoding;
    readonly ImportResult result=new ImportResult{Format="DBF"};
    List<DbfTable.Record> records;
    public object Canonical {get;private set;}
    public IfoSoftDbfJournalImporter(string file,string definitions,int? fiscalYear=null)
    {
        if(fiscalYear.HasValue&&(fiscalYear<1900||fiscalYear>9999))throw new ArgumentOutOfRangeException(nameof(fiscalYear));
            selectedFiscalYear=fiscalYear;path=Path.GetFullPath(file);table=new DbfTable(path);
        try {
            var matches=LayoutFiles.Family(definitions,"dbf-").Select(DbfJournalDefinition.Load).Where(d=>d!=null&&d.Version==1&&d.Format=="DBF"&&new[]{"AJ","GL","AF"}.Contains(d.Category)&&d.RequiredFields.Count>0&&d.RequiredFields.All(k=>table.Fields.Any(f=>f.Name==k.Key&&f.Type.ToString()==k.Value))).ToArray();
            if(matches.Length>1)throw new AmbiguousLayoutException(matches.Select(d=>d.Id));
            definition=matches.SingleOrDefault(d=>d.Category=="AJ");
            if(definition!=null){
                foreach(string k in new[]{"date","documentType","documentNumber","description","debitAccount","creditAccount","amount","debitAmount","creditAmount"})
                    if(!definition.Map.ContainsKey(k)||!table.Fields.Any(f=>f.Name==definition.Map[k]))throw new InvalidDataException("Missing DBF mapping: "+k);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);encoding=Encoding.GetEncoding(definition.Encoding,EncoderFallback.ExceptionFallback,DecoderFallback.ExceptionFallback);
            }
        }catch{table.Dispose();throw;}
    }
    void Issue(string code,string message,string severity="error"){if(!result.Issues.Any(i=>i.Code==code&&i.Message==message))result.Issues.Add(new ImportIssue{Code=code,Message=message,Severity=severity});}
    string Get(DbfTable.Record r,string key)=>definition.Map.TryGetValue(key,out var field)&&r.Fields.TryGetValue(field,out var v)?v.Trim():"";
    static bool Date(string raw,out DateTime date)=>DateTime.TryParseExact(raw,"yyyyMMdd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date);
    static bool Money(string raw,out decimal amount){amount=0;return Regex.IsMatch(raw,@"^[+-]?\d+(?:\.\d{1,2})?$")&&decimal.TryParse(raw,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out amount);}
    public ImportResult Examine(ImportLevel level,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));result.RequestedLevel=level;
        if(result.CompletedLevel==0){result.CompletedLevel=1;if(definition!=null){result.LayoutId=definition.Id;result.Producer="IfoSoft";result.Category="AJ";}}
        if(definition==null)return result;
        if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
            var dates=new List<DateTime>();int active=0,deleted=0,invalid=0;
            foreach(var r in table.Records(encoding,token,new HashSet<string>{definition.Map["date"],definition.Map.ContainsKey("accountingDate")?definition.Map["accountingDate"]:definition.Map["date"]})){
                if(r.Deleted){deleted++;continue;}active++;if(Date(Get(r,"accountingDate"),out var dt)&&dt.Year>=1900)dates.Add(dt);else invalid++;
            }
            result.Identifiers["recordCount"]=table.RecordCount.ToString();result.Identifiers["activeRecordCount"]=active.ToString();result.Identifiers["deletedRecordCount"]=deleted.ToString();
            result.Identifiers["encoding"]=definition.Encoding.ToString();result.Identifiers["languageDriver"]=table.LanguageDriver.ToString();result.Identifiers["invalidDateRecordCount"]=invalid.ToString();
            result.Identifiers["observedDateFrom"]=dates.Count==0?null:dates.Min().ToString("yyyy-MM-dd");result.Identifiers["observedDateTo"]=dates.Count==0?null:dates.Max().ToString("yyyy-MM-dd");
            var years=dates.Select(d=>d.Year).Distinct().ToArray();result.Identifiers["fiscalYear"]=years.Length==1&&invalid==0?years[0].ToString():null;
            result.Identifiers["fiscalYearBasis"]="DATVY (provisional accounting-period date)";
            if(selectedFiscalYear.HasValue)result.Identifiers["selectedFiscalYear"]=selectedFiscalYear.Value.ToString();
            if(years.Length!=1||invalid>0)Issue("fiscalYearSelectionRequired","DATVY does not establish one fiscal year; select a year before calculating GL.","warning");
            result.Identifiers["cin"]=null;result.Identifiers["AccountingEntityName"]=null;
            Issue("entityUnavailable","DBF entity fields are not verified; use caller-confirmed entity context.","warning");
            if(deleted>0)Issue("deletedRecords",deleted+" records marked deleted were excluded.","warning");
            result.CompletedLevel=2;
        }
        if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
            var read=table.Records(encoding,token).Where(r=>!r.Deleted).ToList();
            var rows=read.Select(r=>new SourceRow{Kind="AjRecord",Fields=new Dictionary<string,string>(r.Fields)}).ToList();
            records=read;result.Rows=rows;result.CompletedLevel=3;
        }
        if(level>=ImportLevel.Validate&&result.CompletedLevel<4){
            if(records.Count==0)Issue("noData","No active journal records.");
            var years=new HashSet<int>();
            for(int i=0;i<records.Count;i++){
                token.ThrowIfCancellationRequested();var r=records[i];string loc="DBF record "+r.Number+": ";
                if(!Date(Get(r,"date"),out var date)||date.Year<1900)Issue("invalidDate",loc+"DATUM = "+Get(r,"date"),"warning");
                if(!Date(Get(r,"accountingDate"),out var accountingDate)||accountingDate.Year<1900)Issue("invalidAccountingDate",loc+"DATVY = "+Get(r,"accountingDate"),"warning");else years.Add(accountingDate.Year);
                int target=selectedFiscalYear??(int.TryParse(result.Identifiers["fiscalYear"],out var fy)?fy:0);
                if(target>0&&(date.Year!=target||accountingDate.Year!=target))Issue("dateOutsideFiscalYear",loc+"DATUM = "+Get(r,"date")+", DATVY = "+Get(r,"accountingDate"),"warning");
                foreach(var key in new[]{"amount","debitAmount","creditAmount"}){
                    if(Money(Get(r,key),out var v))result.Rows[i].Amounts[key]=v;else Issue("invalidAmount",loc+definition.Map[key]+" = "+Get(r,key));
                }
                string debit=Get(r,"debitAccount"),credit=Get(r,"creditAccount");
                if(debit.Length==0&&credit.Length==0)Issue("missingAccount",loc+"Both accounts are blank.");
                foreach(var pair in new[]{("debitAccount","debitAmount"),("creditAccount","creditAmount")})
                    if(Get(r,pair.Item1).Length==0&&Money(Get(r,pair.Item2),out var v)&&v!=0)Issue("missingAccount",loc+"Nonzero "+pair.Item2+" without account.");
            }
            if(years.Count>1)Issue("mixedFiscalYears","DATVY spans multiple years; all records retained for fiscal-year review.","warning");
            result.CompletedLevel=4;
        }
        result.Status=result.Issues.Any(i=>i.Severity=="error")?"invalid":"completed";
        if(level==ImportLevel.Normalize&&result.Status=="completed"&&Canonical==null){
            token.ThrowIfCancellationRequested();var aj=new JournalImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,TechnicalType="DBF",AccountingFormat="IfoSoft",CompanyName="",Ico=null,FiscalYear=selectedFiscalYear??(int.TryParse(result.Identifiers["fiscalYear"],out var fy)?fy:0),ImportedAtUtc=DateTime.UtcNow};
            using(var sha=SHA256.Create())using(var input=File.OpenRead(path))aj.SourceFileHash=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
            for(int i=0;i<records.Count;i++){
                token.ThrowIfCancellationRequested();var r=records[i];Date(Get(r,"date"),out var date);
                aj.Rows.Add(new JournalRow{SequenceNumber=i+1,SourceRecordNumber=r.Number,SourceLocation="DBF record "+r.Number,SourceFields=new Dictionary<string,string>(r.Fields),DocumentType=Get(r,"documentType"),DocumentNumber=Get(r,"documentNumber"),PostingDate=date,Description=Get(r,"description"),DebitAccount=Get(r,"debitAccount"),CreditAccount=Get(r,"creditAccount"),DebitAmount=Get(r,"debitAccount").Length==0?(decimal?)null:result.Rows[i].Amounts["debitAmount"],CreditAmount=Get(r,"creditAccount").Length==0?(decimal?)null:result.Rows[i].Amounts["creditAmount"],DebitSection=Get(r,"debitSection"),CreditSection=Get(r,"creditSection"),DebitItem=Get(r,"debitItem"),CreditItem=Get(r,"creditItem"),DebitFundingSource=Get(r,"debitFundingSource"),CreditFundingSource=Get(r,"creditFundingSource"),DebitCostCenter=Get(r,"debitCostCenter"),CreditCostCenter=Get(r,"creditCostCenter"),DebitOrder=Get(r,"debitOrder"),CreditOrder=Get(r,"creditOrder")});
            }
            foreach(var row in aj.Rows){
                row.RecordKind=ExcelApiPoc.AccountingImport.Services.IfoSoft.IfoSoftCsvJournalImporter.ClassifyRecord(row);
                Date(row.SourceFields.TryGetValue(definition.Map["accountingDate"],out var raw)?raw:"",out var accountingDate);
                ExcelApiPoc.AccountingImport.Services.Layouts.JournalDateReview.Apply(row,aj.FiscalYear,row.PostingDate!=DateTime.MinValue,accountingDate);
            }
            Canonical=aj;result.CompletedLevel=5;
        }
        return result;
    }
    public void Dispose()=>table.Dispose();
}

}
