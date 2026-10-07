using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Services.Dbf
{
    // GL/AF share the existing DBF reader and staged result contract. No business reconciliation here.
    public sealed class IfoSoftDbfAccountImporter : IDisposable
    {
        static readonly string[] AmountKeys={"OpeningDebit","OpeningCredit","TurnoverDebit","TurnoverCredit","PeriodTurnoverDebit","PeriodTurnoverCredit","ClosingDebit","ClosingCredit","Plan"};
        readonly string path;
        readonly int? selectedYear;
        readonly DbfTable table;
        readonly DbfJournalDefinition definition;
        readonly Encoding encoding;
        readonly ImportResult result=new ImportResult{Format="DBF"};
        List<DbfTable.Record> records;
        public object Canonical {get;private set;}

        public IfoSoftDbfAccountImporter(string file,string definitions,int? fiscalYear=null)
        {
            if(fiscalYear.HasValue&&(fiscalYear<1900||fiscalYear>9999))throw new ArgumentOutOfRangeException(nameof(fiscalYear));
            path=Path.GetFullPath(file);selectedYear=fiscalYear;table=new DbfTable(path);
            try {
                var matches=LayoutFiles.Family(definitions,"dbf-")
                    .Select(DbfJournalDefinition.Load).Where(d=>d!=null&&d.Version==1&&d.Format=="DBF"&&(d.Category=="GL"||d.Category=="AF")&&d.RequiredFields.Count>0&&d.RequiredFields.All(k=>table.Fields.Any(f=>f.Name==k.Key&&f.Type.ToString()==k.Value))).ToArray();
                if(matches.Length>1)throw new AmbiguousLayoutException(matches.Select(d=>d.Id));
                definition=matches.SingleOrDefault();
                if(definition==null)return;
                var required=definition.Category=="GL"?AmountKeys.Concat(new[]{"account","name","period"}):new[]{"account","synthetic","analytical","name"};
                foreach(var key in required)if(!definition.Map.ContainsKey(key))throw new InvalidDataException("Missing DBF mapping: "+key);
                foreach(var map in definition.Map)if(!table.Fields.Any(f=>f.Name==map.Value))throw new InvalidDataException("Missing mapped DBF field: "+map.Value);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                encoding=Encoding.GetEncoding(definition.Encoding,EncoderFallback.ExceptionFallback,DecoderFallback.ExceptionFallback);
            }catch{table.Dispose();throw;}
        }
        string Get(DbfTable.Record r,string key)=>definition.Map.TryGetValue(key,out var f)&&r.Fields.TryGetValue(f,out var v)?v.Trim():"";
        void Issue(string code,string message,string severity="warning")=>result.Issues.Add(new ImportIssue{Code=code,Message=message,Severity=severity});
        static bool Period(string value,out int month,out int year)
        {
            month=year=0;var m=Regex.Match(value,@"^(\d{1,2})/(\d{4})$");
            return m.Success&&int.TryParse(m.Groups[1].Value,out month)&&int.TryParse(m.Groups[2].Value,out year)&&month<=14&&year>=1900;
        }
        string Kind(DbfTable.Record r)
        {
            if(definition.Category=="GL")return "GlRecord";
            if(Get(r,"analytical")=="****")return "GroupHeading";
            if(Get(r,"account").Length==0&&Get(r,"synthetic").Length==0&&Get(r,"analytical").Length==0&&Get(r,"name").Length==0)return "Empty";
            return Get(r,"analytical").Length==0?"SyntheticAccount":"AnalyticalAccount";
        }
        public ImportResult Examine(ImportLevel level,CancellationToken token=default)
        {
            token.ThrowIfCancellationRequested();if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));
            result.RequestedLevel=level;
            if(result.CompletedLevel==0){result.CompletedLevel=1;if(definition!=null){result.LayoutId=definition.Id;result.Producer="IfoSoft";result.Category=definition.Category;}}
            if(definition==null)return result;
            if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
                var periods=new HashSet<string>();int active=0,deleted=0;
                var selected=new HashSet<string>();if(definition.Category=="GL")selected.Add(definition.Map["period"]);
                foreach(var r in table.Records(encoding,token,selected)){if(r.Deleted){deleted++;continue;}active++;if(definition.Category=="GL")periods.Add(Get(r,"period"));}
                result.Identifiers["recordCount"]=table.RecordCount.ToString(CultureInfo.InvariantCulture);
                result.Identifiers["activeRecordCount"]=active.ToString(CultureInfo.InvariantCulture);result.Identifiers["deletedRecordCount"]=deleted.ToString(CultureInfo.InvariantCulture);
                result.Identifiers["encoding"]=definition.Encoding.ToString(CultureInfo.InvariantCulture);result.Identifiers["languageDriver"]=table.LanguageDriver.ToString(CultureInfo.InvariantCulture);
                result.Identifiers["cin"]=null;result.Identifiers["AccountingEntityName"]=null;result.Identifiers["fiscalYear"]=null;
                Issue("entityUnavailable","No verified entity identifier in this DBF layout; use caller-confirmed entity context.");
                if(selectedYear.HasValue)result.Identifiers["selectedFiscalYear"]=selectedYear.Value.ToString(CultureInfo.InvariantCulture);
                if(deleted>0)Issue("deletedRecords",deleted+" records marked deleted were excluded.");
                if(definition.Category=="GL"){
                    var years=new HashSet<int>();bool valid=true;
                    foreach(var p in periods){if(Period(p,out var month,out var year))years.Add(year);else valid=false;}
                    if(valid&&years.Count==1)result.Identifiers["fiscalYear"]=years.Single().ToString(CultureInfo.InvariantCulture);
                    if(valid&&periods.Count==1){result.Identifiers["PeriodTo"]=periods.Single();result.Identifiers["PeriodFrom"]=null;}
                    result.Identifiers["fiscalYearBasis"]="MES";
                    if(selectedYear.HasValue&&years.Any(y=>y!=selectedYear.Value))Issue("fiscalYearConflict","Selected fiscal year conflicts with MES; source year is preserved.","error");
                }else Issue("fiscalYearUnavailable","AF contains no fiscal-year field; use confirmed AJ/context. Filename is not evidence.");
                result.CompletedLevel=2;
            }
            if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
                var read=table.Records(encoding,token).Where(r=>!r.Deleted).ToList();
                var rows=read.Select(r=>new SourceRow{Kind=Kind(r),Account=Get(r,"account"),Name=Get(r,"name"),Fields=new Dictionary<string,string>(r.Fields)}).ToList();
                records=read;result.Rows=rows;result.CompletedLevel=3;
            }
            if(level>=ImportLevel.Validate&&result.CompletedLevel<4){
                int accounts=0;var periods=new HashSet<string>();
                for(int i=0;i<records.Count;i++){
                    token.ThrowIfCancellationRequested();var r=records[i];var row=result.Rows[i];string loc="DBF record "+r.Number+": ";
                    if(row.Kind=="Empty"||row.Kind=="GroupHeading")continue;
                    accounts++;
                    if(definition.Category=="GL"){
                        if(row.Account.Length==0)Issue("missingAccount",loc+"UCET is blank.","error");
                        else if(!Regex.IsMatch(row.Account,@"^\d{3}"))Issue("unusualAccount",loc+"Nonstandard UCET retained: "+row.Account);
                        string p=Get(r,"period");if(!Period(p,out var month,out var year))Issue("invalidPeriod",loc+"MES = "+p,"error");else periods.Add(month+"/"+year);
                        foreach(var key in AmountKeys){string raw=Get(r,key);if(Regex.IsMatch(raw,@"^[+-]?\d+(?:\.\d{1,2})?$")&&decimal.TryParse(raw,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var value))row.Amounts[key]=value;else Issue("invalidAmount",loc+definition.Map[key]+" = "+raw,"error");}
                    }else {
                        string syn=Get(r,"synthetic"),ana=Get(r,"analytical");
                        if(syn.Length==0)Issue("missingAccount",loc+"SYN is blank.","error");
                        else if(!Regex.IsMatch(syn,@"^\d{3}$"))Issue("unusualSyntheticAccount",loc+"Nonstandard SYN retained: "+syn);
                        if(AccountCodeNormalizer.Normalize(row.Account)!=AccountCodeNormalizer.Normalize(syn+ana))Issue("accountCodeMismatch",loc+"UCET differs from SYN + ANA; no account code guessed.","error");
                    }
                }
                if(accounts==0)Issue("noData","No active account records.","error");
                if(periods.Count>1)Issue("mixedPeriods","GL contains multiple MES periods; normalization stopped to avoid combining snapshots.","error");
                result.CompletedLevel=4;
            }
            result.Status=result.Issues.Any(x=>x.Severity=="error")?"invalid":"completed";
            if(level==ImportLevel.Normalize&&result.Status=="completed"&&Canonical==null){Canonical=Normalize(token);result.CompletedLevel=5;}
            return result;
        }
        object Normalize(CancellationToken token)
        {
            string hash;using(var sha=SHA256.Create())using(var input=File.OpenRead(path))hash=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
            int sourceYear=int.TryParse(result.Identifiers["fiscalYear"],out var year)?year:0;
            if(definition.Category=="GL"){
                var gl=new GeneralLedgerImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType="DBF",AccountingFormat="IfoSoft",CompanyName="",FiscalYear=sourceYear,ImportedAtUtc=DateTime.UtcNow,PeriodHeader=Get(records[0],"period")};
                Period(gl.PeriodHeader,out var month,out var ignored);gl.ThroughMonth=month;
                for(int i=0;i<records.Count;i++){
                    token.ThrowIfCancellationRequested();var r=records[i];var a=result.Rows[i].Amounts;string rawAccount=Get(r,"account"),account=AccountCodeNormalizer.Normalize(rawAccount);
                    int split=Math.Min(3,rawAccount.Length);string synthetic=rawAccount.Substring(0,split).Trim(),analytical=rawAccount.Substring(split).Trim();
                    gl.Rows.Add(new GeneralLedgerRow{SequenceNumber=i+1,SourceRecordNumber=r.Number,SourceRowKind="GlRecord",SourceDimensions=new Dictionary<string,string>(r.Fields),AccountCode=account,SyntheticCode=synthetic,AnalyticalCode=analytical,AccountName=Get(r,"name"),Type=Get(r,"type"),P=Get(r,"p"),Section=Get(r,"section"),Item=Get(r,"item"),FundingSource=Get(r,"fundingSource"),Program=Get(r,"program"),CostCenter=Get(r,"costCenter"),Order=Get(r,"order"),OpeningDebit=a["OpeningDebit"].Value,OpeningCredit=a["OpeningCredit"].Value,AnnualDebitTurnover=a["TurnoverDebit"].Value,AnnualCreditTurnover=a["TurnoverCredit"].Value,PeriodDebitTurnover=a["PeriodTurnoverDebit"].Value,PeriodCreditTurnover=a["PeriodTurnoverCredit"].Value,ClosingDebit=a["ClosingDebit"].Value,ClosingCredit=a["ClosingCredit"].Value,Plan=a["Plan"].Value,SourceAvailableAmountFields=AmountKeys.ToArray()});
                }return gl;
            }
            var af=new AccountingFrameworkImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType="DBF",AccountingFormat="IfoSoft",CompanyName="",FiscalYear=selectedYear??0,ImportedAtUtc=DateTime.UtcNow};
            for(int i=0;i<records.Count;i++){
                token.ThrowIfCancellationRequested();if(result.Rows[i].Kind=="Empty"||result.Rows[i].Kind=="GroupHeading")continue;
                var r=records[i];string syn=Get(r,"synthetic"),ana=Get(r,"analytical");
                af.Rows.Add(new AccountingFrameworkRow{SequenceNumber=af.Rows.Count+1,SourceRecordNumber=r.Number,SourceFields=new Dictionary<string,string>(r.Fields),SourceSyntheticCode=syn,SourceAnalyticalCode=ana,SyntheticCode=syn,AnalyticalCode=ana,AccountCode=AccountCodeNormalizer.Normalize(syn+ana),AccountName=Get(r,"name"),Type=Get(r,"type"),TaxFlag=Get(r,"tax"),BalanceFlag=Get(r,"balance"),VatFlag=Get(r,"vat"),RowKind=ana.Length==0?AccountingFrameworkRowKind.SyntheticAccount:AccountingFrameworkRowKind.AnalyticalAccount});
            }return af;
        }
        public void Dispose()=>table.Dispose();
    }
}
