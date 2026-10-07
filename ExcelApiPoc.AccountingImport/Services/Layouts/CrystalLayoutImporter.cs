using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using PdfLayoutEngine.Simple;
namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    public sealed class CrystalDefinition
    {
        public int Version {get;set;}
        public string Id {get;set;}="";
        public string Producer {get;set;}="";
        public string Category {get;set;}="";
        public string Namespace {get;set;}="";
        public string RecordMarker {get;set;}="";
        public string DateField {get;set;}="";
        public string GroupField {get;set;}="";
        public string ContinuationMarker {get;set;}="";
        public Dictionary<string,string> Fields {get;set;}=new Dictionary<string,string>();
        public Dictionary<string,string> Controls {get;set;}=new Dictionary<string,string>();
        public string[] Amounts {get;set;}=Array.Empty<string>();
        public string[] CompactFields {get;set;}=Array.Empty<string>();
    }
    // Streams Crystal sections, using FieldName rather than unstable visual Field.Name values.
    public sealed class CrystalLayoutImporter : IDisposable
    {
        readonly string path;readonly int? selectedYear;readonly CrystalDefinition[] definitions;
        readonly ImportResult result=new ImportResult{Format="XML"};CrystalDefinition definition;
        public object Canonical {get;private set;}
        static XmlReaderSettings Settings()=>new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,IgnoreComments=true,MaxCharactersInDocument=268435456};
        public CrystalLayoutImporter(string file,string layouts,int? year=null){
            path=Path.GetFullPath(file);selectedYear=year;
            if(year.HasValue&&(year<1900||year>9999))throw new ArgumentOutOfRangeException(nameof(year));
            definitions=LayoutFiles.Family(layouts,"crystal-").Select(p=>JsonSerializer.Deserialize<CrystalDefinition>(File.ReadAllText(p),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow})).ToArray();
            foreach(var d in definitions)if(d.Version!=1||!new[]{"AJ","GL","AF"}.Contains(d.Category)||d.Namespace.Length==0||d.RecordMarker.Length==0||d.Amounts.Concat(d.CompactFields).Any(k=>!d.Fields.ContainsKey(k)))throw new InvalidDataException("Invalid Crystal layout: "+d.Id);
        }
        void Issue(string code,string message,string severity="error"){if(!result.Issues.Any(v=>v.Code==code&&v.Message==message))result.Issues.Add(new ImportIssue{Code=code,Message=message,Severity=severity});}
        IEnumerable<Dictionary<string,string>> Sections(CancellationToken token){
            using(var reader=XmlReader.Create(path,Settings())){
                reader.MoveToContent();if(reader.LocalName!="CrystalReport")yield break;string ns=reader.NamespaceURI;
                if(!definitions.Any(d=>d.Namespace==ns))yield break;
                while(!reader.EOF){token.ThrowIfCancellationRequested();
                    if(reader.NodeType==XmlNodeType.Element&&reader.LocalName=="Section"&&reader.NamespaceURI==ns){
                        var section=(XElement)XNode.ReadFrom(reader);var fields=new Dictionary<string,string>(StringComparer.Ordinal);
                        foreach(var f in section.Elements(XName.Get("Field",ns))){string key=(string)f.Attribute("FieldName");if(key==null)continue;string value=f.Element(XName.Get("Value",ns))?.Value??f.Element(XName.Get("FormattedValue",ns))?.Value??"";if(fields.ContainsKey(key))throw new InvalidDataException("Duplicate XML field: "+key);fields[key]=value.Trim();}
                        yield return fields;continue;
                    }
                    reader.Read();
                }
            }
        }
        static string Get(Dictionary<string,string> fields,string key)=>fields.TryGetValue(key,out var v)?v:"";
        static bool Date(string raw,out DateTime date)=>DateTime.TryParseExact(raw,new[]{"yyyy-MM-ddTHH:mm:ss","yyyy-MM-ddTHH:mm:ss.FFFFFFF","yyyy-MM-dd","d.M.yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out date)&&date.Year>=1900;
        public ImportResult Examine(ImportLevel level,CancellationToken token=default){
            if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));token.ThrowIfCancellationRequested();result.RequestedLevel=level;
            if(result.CompletedLevel==0){
                int inspected=0;foreach(var fields in Sections(token)){
                    var matches = definitions.Where(layout => fields.ContainsKey(layout.RecordMarker) && layout.Fields.Values.All(fields.ContainsKey)).ToArray();
                    if(matches.Length>1)throw new AmbiguousLayoutException(matches.Select(candidate=>candidate.Id));if(matches.Length==1){definition=matches[0];break;}if(++inspected>=4096)break;
                }
                result.CompletedLevel=1;if(definition!=null){result.LayoutId=definition.Id;result.Producer=definition.Producer;result.Category=definition.Category;}
            }
            if(definition==null)return result;var d=definition;
            if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
                var years=new HashSet<int>();int count=0;bool badDate=false;
                foreach(var f in Sections(token)){if(f.ContainsKey(d.RecordMarker))count++;if(d.DateField.Length>0&&f.ContainsKey(d.DateField)){if(Date(f[d.DateField],out var dt))years.Add(dt.Year);else badDate=true;}}
                result.Identifiers["cin"]=null;result.Identifiers["AccountingEntityName"]=null;result.Identifiers["fiscalYear"]=years.Count==1?years.Single().ToString():null;
                result.Identifiers["observedYears"]=string.Join(",",years.OrderBy(y=>y));result.Identifiers["fiscalYearBasis"]=d.DateField.Length>0?"XML transaction dates":"not present";result.Identifiers["recordCount"]=count.ToString();
                if(selectedYear.HasValue)result.Identifiers["selectedFiscalYear"]=selectedYear.ToString();
                Issue("entityIdentifierUnavailable","XML contains no verified entity identifier; caller context is required. Filename is not used.","warning");
                if(d.Category!="AF"&&years.Count!=1)Issue("fiscalYearSelectionRequired","Transaction dates do not establish one fiscal year.","warning");
                if(badDate)Issue("invalidSourceDate","Invalid transaction dates retained for review.","warning");result.CompletedLevel=2;
            }
            if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
                string group="";SourceRow previous=null;int section=0;
                foreach(var fields in Sections(token)){
                    section++;if(d.GroupField.Length>0&&fields.ContainsKey(d.GroupField)){group=fields[d.GroupField];previous=null;}
                    bool record=fields.ContainsKey(d.RecordMarker);bool control=d.Controls.Count>0&&d.Controls.Values.Any(fields.ContainsKey);
                    if(!record&&!control){
                        if(previous!=null&&d.ContinuationMarker.Length>0&&fields.ContainsKey(d.ContinuationMarker))foreach(var pair in fields)previous.Fields["source:"+pair.Key]=pair.Value;
                        continue;
                    }
                    var row=new SourceRow{Kind=record?"Record":"GroupTotal"};row.Fields["Group"]=group;row.Fields["SourceLocation"]="XML section "+section;
                    foreach(var pair in fields)row.Fields["source:"+pair.Key]=pair.Value;
                    foreach(var pair in record?d.Fields:d.Controls)row.Fields[pair.Key]=Get(fields,pair.Value);
                    foreach(var key in d.CompactFields)if(row.Fields.ContainsKey(key))row.Fields[key]=Regex.Replace(row.Fields[key],@"\s","");
                    if(record&&row.Fields.TryGetValue("Date",out var date)&&Date(date,out var dt))row.Fields["Date"]=dt.ToString("dd.MM.yyyy",CultureInfo.InvariantCulture);
                    row.Account=Get(row.Fields,"Account");row.Name=Get(row.Fields,"Name");
                    if(d.Category=="AF"&&row.Account.Length>=3){row.Fields["SyntheticAccount"]=row.Account.Substring(0,3);row.Fields["AnalythicAccount"]=row.Account.Substring(3);}
                    result.Rows.Add(row);previous=record?row:null;
                }
                result.CompletedLevel=3;
            }
            if(level>=ImportLevel.Validate&&result.CompletedLevel<4){
                var rows=result.Rows.Where(r=>r.Kind=="Record").ToArray();if(rows.Length==0)Issue("noData","No XML records.");
                foreach(var row in result.Rows){
                    foreach(var key in d.Amounts){string raw=Get(row.Fields,key);if(decimal.TryParse(raw,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var value))row.Amounts[key]=value;else{row.Amounts[key]=null;Issue("invalidAmount",Get(row.Fields,"SourceLocation")+": "+key+" = "+raw);}}
                    if(row.Kind!="Record")continue;
                    if(d.Category=="AJ"){
                        if(!CompactSession.Date(Get(row.Fields,"Date"),out var dt)||dt.Year<1900)Issue("invalidDate",Get(row.Fields,"SourceLocation")+": "+Get(row.Fields,"Date"),"warning");
                        if(Get(row.Fields,"DebitAccount").Length==0&&Get(row.Fields,"CreditAccount").Length==0)Issue("missingAccount",Get(row.Fields,"SourceLocation"));
                    }else if(row.Account.Length==0)Issue("missingAccount",Get(row.Fields,"SourceLocation"));
                }
                if(d.Controls.Count>0)foreach(var group in rows.GroupBy(r=>Get(r.Fields,"Group"))){
                    var controls=result.Rows.Where(r=>r.Kind=="GroupTotal"&&Get(r.Fields,"Group")==group.Key).ToArray();
                    if(controls.Length!=1){Issue("missingGroupTotal",group.Key);continue;}
                    foreach(var key in d.Amounts)if(!controls[0].Amounts[key].HasValue||group.Any(r=>!r.Amounts[key].HasValue)||Math.Abs(group.Sum(r=>r.Amounts[key].Value)-controls[0].Amounts[key].Value)>.005m)Issue("groupTotalMismatch",group.Key+": "+key);
                }
                result.CompletedLevel=4;
            }
            result.Status=result.Issues.Any(v=>v.Severity=="error")?"invalid":"completed";
            if(level>=ImportLevel.Normalize&&result.Status=="completed"&&result.CompletedLevel<5){Canonical=CompactLayoutImporter.Map(result,path,selectedYear,d.Producer,new[]{"Record"});result.CompletedLevel=5;}
            return result;
        }
        public void Dispose(){}
    }
}
