using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Simple;
namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    public sealed class ExcelLayoutDefinition
    {
        public int Version {get;set;}
        public string Id {get;set;}="";
        public string Producer {get;set;}="";
        public string Category {get;set;}="";
        public string[][] Headers {get;set;}=Array.Empty<string[]>();
        public Dictionary<string,int> Columns {get;set;}=new Dictionary<string,int>();
        public string[] Amounts {get;set;}=Array.Empty<string>();
        public int[] ColumnCounts {get;set;}=Array.Empty<int>();
        public Dictionary<string,string[]> AmountSums {get;set;}=new Dictionary<string,string[]>();
        public Dictionary<string,string> NamedColumns {get;set;}=new Dictionary<string,string>();
        public string[] RequiredFields {get;set;}=Array.Empty<string>();
        public bool SingleAccount {get;set;}
        public bool IndependentAmounts {get;set;}
        public Dictionary<string,string[]> AccountParts {get;set;}=new Dictionary<string,string[]>();
        public string UnsupportedReason {get;set;}="";
        public bool DocumentGroups {get;set;}
    }
    public sealed class ExcelLayoutImporter:IDisposable
    {
        readonly string path;readonly int? selectedYear;readonly ExcelLayoutDefinition[] definitions;
        readonly ImportResult result=new ImportResult{Format="Excel"};ExcelLayoutDefinition layout;
        public object Canonical {get;private set;}
        public ExcelLayoutImporter(string file,string catalogue,int? year=null){
            path=Path.GetFullPath(file);selectedYear=year;
            definitions=(Directory.Exists(catalogue)?Directory.GetFiles(catalogue,"excel-*.json"):new[]{catalogue}).Select(p=>JsonSerializer.Deserialize<ExcelLayoutDefinition>(File.ReadAllText(p),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow})).ToArray();
            foreach(var d in definitions)if(d.Version!=1||!new[]{"AJ","GL","AF"}.Contains(d.Category)||d.ColumnCounts.Any(n=>n<=0)||d.AmountSums.Values.Any(v=>v.Length==0||v.Any(k=>!d.Amounts.Contains(k)))||d.Headers.Length==0||d.Columns.Values.Any(n=>n<0)||d.Amounts.Any(k=>!d.Columns.ContainsKey(k)&&!d.NamedColumns.ContainsKey(k)))throw new InvalidDataException("Invalid Excel layout "+d.Id);
            foreach(var d in definitions)if(d.RequiredFields.Any(k=>!d.NamedColumns.ContainsKey(k))||d.SingleAccount&&(d.Category!="AJ"||!new[]{"AccountingPeriod","Account","Date","DebitAmount","CreditAmount"}.All(k=>d.NamedColumns.ContainsKey(k)&&d.RequiredFields.Contains(k))))throw new InvalidDataException("Invalid named journal layout "+d.Id);
            foreach(var d in definitions)if(d.AccountParts.Values.Any(v=>v.Length!=2||v.Any(k=>!d.Columns.ContainsKey(k)))||d.IndependentAmounts&&(d.Category!="AJ"||!new[]{"DebitAmount","CreditAmount"}.All(k=>d.Amounts.Contains(k))))throw new InvalidDataException("Invalid independent journal layout "+d.Id);
        }
        static string Text(string s)=>Regex.Replace(s??"",@"\s+"," ").Trim();
        static string Cell(string[] a,int n)=>n<a.Length?a[n]:"";
        string Field(string[] a,string key)=>layout.Columns.TryGetValue(key,out var n)?Text(Cell(a,n)):"";
        void Issue(string code,string message,string severity="error")=>result.Issues.Add(new ImportIssue{Code=code,Message=message,Severity=severity});
        IEnumerable<string[]> Read(CancellationToken token){
            using(var reader=ExcelWorkbookReader.Open(path)){
                if(reader.ResultsCount!=1)throw new InvalidDataException("Expected one original export worksheet; found "+reader.ResultsCount+". Combined/audit workbooks are not accepted.");
                while(reader.Read()){token.ThrowIfCancellationRequested();var a=new string[reader.FieldCount];for(int i=0;i<a.Length;i++)a[i]=ExcelWorkbookReader.GetUrbisText(reader,i,Path.GetExtension(path))??"";yield return a;}
            }
        }
        static bool Date(string raw,out DateTime date)=>DateTime.TryParseExact(Regex.Replace(raw,@"\s", ""),new[]{"yyyy-MM-dd","d.M.yyyy","dd.MM.yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out date)&&date.Year>=1900;
        public ImportResult Examine(ImportLevel level,CancellationToken token=default){
            if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));result.RequestedLevel=level;token.ThrowIfCancellationRequested();
            if(result.CompletedLevel==0){
                var head=Read(token).Take(Math.Max(2,definitions.Select(d=>d.Headers.Length).DefaultIfEmpty(2).Max())).ToArray();
                var matches=definitions.Where(d=>head.Length>=d.Headers.Length&&(d.ColumnCounts.Length==0||d.ColumnCounts.Contains(head[0].Length))&&d.Headers.Select((h,i)=>h.Select((pattern,j)=>Regex.IsMatch(Text(Cell(head[i],j)),pattern,RegexOptions.None,TimeSpan.FromSeconds(1))).All(v=>v)).All(v=>v)).ToArray();
                if(matches.Length>1)throw new InvalidDataException("Ambiguous Excel layout.");layout=matches.SingleOrDefault();result.CompletedLevel=1;if(layout!=null){result.LayoutId=layout.Id;result.Category=layout.Category;
                    if(layout.NamedColumns.Count>0){
                        var headers=head[0].Select(Text).ToArray();var named=headers.Where(v=>v.Length>0).ToArray();
                        if(named.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=named.Length)throw new InvalidDataException("Duplicate worksheet headers.");
                        foreach(var field in layout.NamedColumns){int at=Array.FindIndex(headers,h=>string.Equals(h,field.Value,StringComparison.OrdinalIgnoreCase));if(at>=0)layout.Columns[field.Key]=at;else if(layout.RequiredFields.Contains(field.Key))throw new InvalidDataException("Missing required header: "+field.Value);}
                    }
                }
            }
            if(layout==null)return result;
            if(layout.UnsupportedReason.Length>0){if(!result.Issues.Any(v=>v.Code=="unsupportedExport"))Issue("unsupportedExport",layout.UnsupportedReason);result.Status="unsupported";return result;}
            if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
                result.Identifiers["cin"]=null;result.Identifiers["AccountingEntityName"]=null;
                Issue("entityIdentifierUnavailable","Export contains no verified entity identifier; filename is not used.","warning");
                if(layout.SingleAccount){
                    string period=Read(token).Skip(layout.Headers.Length).Select(a=>Field(a,"AccountingPeriod")).FirstOrDefault(v=>v.Length>0)??"";
                    SingleAccountJournal.Identify(result,period);
                }else if(layout.Category=="AJ"){
                    var years=new HashSet<int>();foreach(var a in Read(token).Skip(layout.Headers.Length))if(Date(Field(a,"Date"),out var dt))years.Add(dt.Year);
                    result.Identifiers["fiscalYear"]=years.Count==1?years.Single().ToString():null;result.Identifiers["observedYears"]=string.Join(",",years.OrderBy(v=>v));result.Identifiers["fiscalYearBasis"]="transaction dates";
                    if(years.Count!=1)Issue("fiscalYearSelectionRequired","Transaction dates do not establish one fiscal year.","warning");
                }else if(layout.Category=="GL"){
                    var h=Read(token).Take(2).ToArray();string period=Field(h[0],"TurnoverDebit");var dates=Regex.Matches(period,@"\d{1,2}\.\s*\d{1,2}\.\s*\d{4}");
                    if(dates.Count!=2||!Date(dates[0].Value,out var start)||!Date(dates[1].Value,out var end)||start>end)Issue("invalidPeriod","Cannot read GL reporting period.");
                    else{result.Identifiers["PeriodFrom"]=start.ToString("dd.MM.yyyy");result.Identifiers["PeriodTo"]=end.ToString("dd.MM.yyyy");result.Identifiers["fiscalYear"]=start.Year==end.Year?end.Year.ToString():null;result.Identifiers["fiscalYearBasis"]="report header";}
                }else{result.Identifiers["fiscalYear"]=null;result.Identifiers["fiscalYearBasis"]="not present";}
                result.CompletedLevel=2;
            }
            if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
                int n=layout.Headers.Length;string document="";
                foreach(var a in Read(token).Skip(layout.Headers.Length)){
                    n++;if(a.All(string.IsNullOrWhiteSpace))continue;
                    if(result.Rows.Count>=500000)throw new InvalidDataException("Maximum source rows exceeded.");
                    var row=new SourceRow{Kind="Record"};row.Fields["SourceLocation"]="worksheet row "+n;
                    for(int c=0;c<a.Length;c++)row.Fields["source:"+(c+1)]=a[c];
                    foreach(var c in layout.Columns)row.Fields[c.Key]=Text(Cell(a,c.Value));
                    foreach(var account in layout.AccountParts){
                        string syn=Field(a,account.Value[0]),ana=Field(a,account.Value[1]);
                        row.Fields[account.Key]=syn.Length==0&&ana.Length==0?"":Regex.IsMatch(syn,@"^\d{1,3}$")&&Regex.IsMatch(ana,@"^\d{1,3}$")?syn.PadLeft(3,'0')+ana.PadLeft(3,'0'):"INVALID:"+syn+"/"+ana;
                    }
                    string label=Field(a,"Structure");
                    if(layout.Category=="AJ"){
                        if(label.StartsWith("Doklad:",StringComparison.Ordinal)){row.Kind="DocumentStart";document=label.Substring(7).Trim();}
                        else if(label.StartsWith("Spolu za doklad",StringComparison.Ordinal))row.Kind="DocumentTotal";
                        else if(label.StartsWith("Spolu za ",StringComparison.Ordinal))row.Kind="DateTotal";
                        else if(label=="Spolu")row.Kind="ReportTotal";
                        else if(label.Length>0)row.Kind="Unknown";
                        row.Fields["DocumentNumber"]=layout.DocumentGroups?document:layout.Columns.ContainsKey("DocumentNumber")?Field(a,"DocumentNumber"):Field(a,"Ordinal");
                        if(Date(Field(a,"Date"),out var date))row.Fields["Date"]=date.ToString("dd.MM.yyyy");
                    }else{
                        row.Account=Regex.Replace(Field(a,"Account"),@"\s", "");row.Name=Field(a,"Name");
                        if(layout.Category=="GL"&&row.Account.Length==0)row.Kind=label=="Spolu"?"ReportTotal":label.Contains("*")?"Subtotal":new[]{"Aktíva","Pasíva","Rozdiel","Výnosy","Náklady","Podsúvaha"}.Contains(label)?"Summary":"Unknown";
                        if(row.Account.Length>=3){row.Fields["SyntheticAccount"]=row.Account.Substring(0,3);row.Fields["AnalythicAccount"]=row.Account.Substring(3);}
                        if(layout.Category=="AF"){if(row.Account.Length<3)row.Kind="Heading";else{row.Fields["SyntheticAccount"]=row.Account.Substring(0,3);row.Fields["AnalythicAccount"]=row.Account.Substring(3);}}
                    }
                    result.Rows.Add(row);
                }
                result.CompletedLevel=3;
            }
            if(level>=ImportLevel.Validate&&result.CompletedLevel<4){
                if(layout.SingleAccount)SingleAccountJournal.Validate(result);
                else foreach(var row in result.Rows){
                    if(layout.Category=="AJ"&&row.Kind!="Record"){
                        if(new[]{"Ordinal","Date","OperationDescription","Amount"}.Any(k=>CompactSession.Value(row.Fields,k).Length>0)||(row.Kind=="DocumentStart"&&new[]{"DebitAccount","CreditAccount"}.Any(k=>CompactSession.Value(row.Fields,k).Length>0)))Issue("invalidStructuralRow",row.Fields["SourceLocation"]);
                    }
                    if(row.Kind=="Unknown")Issue("unresolvedRow",row.Fields["SourceLocation"]);
                    if(row.Kind!="Record"&&row.Kind!="ReportTotal"&&row.Kind!="DocumentTotal"&&row.Kind!="DateTotal")continue;
                    var amounts=layout.Category=="AJ"&&row.Kind!="Record"?new[]{"DebitAccount","CreditAccount"}:layout.Amounts;
                    foreach(var key in amounts){string raw=CompactSession.Value(row.Fields,key);if(layout.IndependentAmounts&&raw.Length==0)row.Amounts[key]=0m;else if(ExportControls.Money(raw,out var value))row.Amounts[key]=value;else{row.Amounts[key]=null;Issue("invalidAmount",row.Fields["SourceLocation"]+": "+key+" = "+raw);}}
                    foreach(var sum in layout.AmountSums)row.Amounts[sum.Key]=sum.Value.All(k=>row.Amounts.TryGetValue(k,out var value)&&value.HasValue)?sum.Value.Sum(k=>row.Amounts[k].Value):(decimal?)null;
                    if(row.Kind!="Record")continue;
                    if(layout.Category=="AJ"){
                        if(!Date(CompactSession.Value(row.Fields,"Date"),out var dt))Issue("invalidDate",row.Fields["SourceLocation"],"warning");
                        if(CompactSession.Value(row.Fields,"DebitAccount").Length==0&&CompactSession.Value(row.Fields,"CreditAccount").Length==0)Issue("missingAccount",row.Fields["SourceLocation"]);
                    }else if(row.Account.Length<3)Issue("missingAccount",row.Fields["SourceLocation"]);
                }
                if(layout.SingleAccount){}else if(layout.IndependentAmounts)IndependentControls.Validate(result,"separateSides",layout.Amounts,false);else ExportControls.Validate(result,layout.Category,layout.DocumentGroups);
                result.CompletedLevel=4;
            }
            result.Status=result.Issues.Any(v=>v.Severity=="error")?"invalid":"completed";
            if(level>=ImportLevel.Normalize&&result.CompletedLevel<5&&result.Status=="completed"){Canonical=CompactLayoutImporter.Map(result,path,selectedYear,layout.Producer,new[]{"Record"});result.CompletedLevel=5;}
            return result;
        }
        public void Dispose(){}
    }
}
