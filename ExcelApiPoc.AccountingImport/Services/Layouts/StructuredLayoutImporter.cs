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
using System.Xml;
using System.Xml.Linq;
using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using PdfLayoutEngine.Simple;

namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    public sealed class StructuredDefinition
    {
        public int Version {get;set;}
        public string Producer {get;set;}="IfoSoft";
        public string Id {get;set;}="";
        public string Format {get;set;}="";
        public string Category {get;set;}="";
        public int Encoding {get;set;}=1250;
        public string Delimiter {get;set;}=";";
        public string TitlePattern {get;set;}="";
        public string[] Headers {get;set;}=Array.Empty<string>();
        public Dictionary<string,int> Columns {get;set;}=new Dictionary<string,int>();
        public string Root {get;set;}="";
        public string DocumentType {get;set;}="";
        public string RecordsPath {get;set;}="";
        public Dictionary<string,string> XmlFields {get;set;}=new Dictionary<string,string>();
    }
    // Uses legacy CSV tokenization and AJ row/date parsing, leaving existing Add-In entry points intact.
    public sealed class StructuredLayoutImporter : IDisposable
    {
        readonly string path,format;
        readonly int? selectedYear;
        readonly StructuredDefinition[] definitions;
        readonly ImportResult result=new ImportResult();
        StructuredDefinition definition;
        IfoSoftCsvJournalImporter.CsvRecord[] prefix;
        List<IfoSoftCsvJournalImporter.CsvRecord> csvRows;
        List<XElement> xmlRows;
        XElement xmlHeader;
        string xmlControl;
        XDocument repairedDocument;
        object validated;
        static readonly string[] AjColumns={"DocumentType","DocumentNumber","PostingDate","Description","DebitAccount","DebitItem","DebitFundingSource","DebitCostCenter","DebitOrder","DebitAmount","CreditAccount","CreditSection","CreditItem","CreditFundingSource","CreditCostCenter","CreditOrder","CreditAmount"};
        static readonly CultureInfo Sk=CultureInfo.GetCultureInfo("sk-SK");
        public object Canonical {get;private set;}
        public StructuredLayoutImporter(string file,string layouts,int? fiscalYear=null)
        {
            path=Path.GetFullPath(file);format=Path.GetExtension(path).Equals(".xml",StringComparison.OrdinalIgnoreCase)?"XML":"CSV";selectedYear=fiscalYear;
            if(fiscalYear.HasValue&&(fiscalYear<1900||fiscalYear>9999))throw new ArgumentOutOfRangeException(nameof(fiscalYear));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);result.Format=format;
            definitions=LayoutFiles.Family(layouts,"structured-")
                .Select(p=>JsonSerializer.Deserialize<StructuredDefinition>(File.ReadAllText(p),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow}))
                .Where(d=>d.Version==1&&d.Format==format).ToArray();
            foreach(var d in definitions){
                if(d.Format=="CSV"&&(d.Encoding!=1250||d.Delimiter!=";"||d.Headers.Length==0||d.Columns.Values.Any(i=>i<0||i>=d.Headers.Length)))throw new InvalidDataException("Unsupported CSV definition: "+d.Id);
                if(d.Format=="XML"&&(d.Category!="AJ"||d.RecordsPath!="vety/veta"))throw new InvalidDataException("Unsupported XML definition: "+d.Id);
            }
        }
        void Issue(string code,string message,string severity="warning")
        {if(!result.Issues.Any(i=>i.Code==code&&i.Message==message))result.Issues.Add(new ImportIssue{Code=code,Message=message,Severity=severity});}
        static string Single(IfoSoftCsvJournalImporter.CsvRecord r)=>r==null?"":string.Join(" ",r.Fields.Where(x=>!string.IsNullOrWhiteSpace(x))).Trim();
        string Csv(IfoSoftCsvJournalImporter.CsvRecord r,string key)=>definition.Columns.TryGetValue(key,out var i)&&i<r.Fields.Length?r.Fields[i].Trim():"";
        static string Xml(XElement e,string path)
        {foreach(var part in path.Split('/')){e=e?.Element(part);if(e==null)return "";}return e.Value.Trim();}
        string X(XElement e,string key)=>definition.XmlFields.TryGetValue(key,out var p)?Xml(e,p):"";
        static XmlReaderSettings Settings()=>new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,IgnoreComments=true};
        void Recognize(CancellationToken token)
        {
            if(format=="CSV"){
                prefix=ReadCsv(token).Take(3).ToArray();
                if(prefix.Length<3)return;
                var candidates=definitions.Where(d=>Regex.IsMatch(Single(prefix[1]),d.TitlePattern,RegexOptions.IgnoreCase)&&prefix[2].Fields.Length==d.Headers.Length&&d.Headers.Select((h,i)=>h=="{period}"?Regex.IsMatch(prefix[2].Fields[i].Trim(),@"^\d{1,2}/\d{4}$"):string.Equals(h,prefix[2].Fields[i].Trim(),StringComparison.OrdinalIgnoreCase)).All(x=>x)).ToArray();
                if(candidates.Length>1)throw new AmbiguousLayoutException(candidates.Select(d=>d.Id));definition=candidates.SingleOrDefault();
            }else{
                using(var reader=XmlReader.Create(path,Settings())){
                    reader.MoveToContent();string root=reader.Name;if(reader.NamespaceURI.Length>0)return;
                    xmlHeader=new XElement(root);int nodes=0;
                    if(reader.IsEmptyElement)return;reader.Read();
                    while(!reader.EOF){token.ThrowIfCancellationRequested();if(++nodes>1000)throw new InvalidDataException("XML identification header exceeds limit.");
                        if(reader.NodeType==XmlNodeType.Element&&reader.Depth==1){if(reader.Name=="vety")break;xmlHeader.Add((XElement)XNode.ReadFrom(reader));}else reader.Read();
                    }
                    var matches=definitions.Where(d=>d.Root==root&&Xml(xmlHeader,"typDoc")==d.DocumentType&&reader.NodeType==XmlNodeType.Element&&reader.Name=="vety").ToArray();
                    if(matches.Length>1)throw new AmbiguousLayoutException(matches.Select(d=>d.Id));definition=matches.SingleOrDefault();
                }
            }
            if(definition!=null){result.Category=definition.Category;result.LayoutId=definition.Id;result.Producer=definition.Producer;}
        }
        // Streams records at identification level; creates no retained journal/source rows.
        IEnumerable<XElement> XmlRecords(CancellationToken token)
        {
            if(repairedDocument!=null){foreach(var e in repairedDocument.Root.Element("vety").Elements()){token.ThrowIfCancellationRequested();if(e.Name!="veta")throw new InvalidDataException("Unexpected XML record element.");yield return e;}xmlControl=Xml(repairedDocument.Root,"sucetKontrola");yield break;}
            using(var reader=XmlReader.Create(path,Settings())){
                bool records=false;
                while(!reader.EOF){token.ThrowIfCancellationRequested();
                    if(reader.NodeType==XmlNodeType.Element&&reader.Depth==1&&reader.Name=="vety"){records=true;reader.Read();continue;}
                    if(records&&reader.NodeType==XmlNodeType.EndElement&&reader.Depth==1)records=false;
                    if(records&&reader.NodeType==XmlNodeType.Element&&reader.Depth==2){if(reader.Name!="veta")throw new InvalidDataException("Unexpected XML record element.");yield return (XElement)XNode.ReadFrom(reader);continue;}
                    if(reader.NodeType==XmlNodeType.Element&&reader.Depth==1&&reader.Name=="sucetKontrola"){xmlControl=reader.ReadElementContentAsString().Trim();continue;}
                    reader.Read();
                }
            }
        }
        void ReadXmlWithLegacyRepair(Action action,CancellationToken token)
        {
            try{action();}catch(XmlException){token.ThrowIfCancellationRequested();repairedDocument=IfoSoftXmlJournalImporter.Read(path,out var count);if(count>0)Issue("xmlTextNormalized",count+" invalid characters in descriptions/notes normalized by the existing XML reader.");action();}
        }
        void Identify(CancellationToken token)
        {
            result.Identifiers["cin"]=null;result.Identifiers["fiscalYear"]=null;
            if(selectedYear.HasValue)result.Identifiers["selectedFiscalYear"]=selectedYear.Value.ToString(CultureInfo.InvariantCulture);
            if(format=="CSV"){
                string entity=Single(prefix[0]);var stamp=Regex.Match(entity,@"^\d{8}_\d{4}\s+(?<name>.+)$");var ico=Regex.Match(entity,@"^(?<ico>\d{8})\s+(?<name>.+)$");
                result.Identifiers["AccountingEntityName"]=stamp.Success?stamp.Groups["name"].Value:ico.Success?ico.Groups["name"].Value:entity;
                if(!stamp.Success&&ico.Success)result.Identifiers["cin"]=ico.Groups["ico"].Value;
                var title=Regex.Match(Single(prefix[1]),definition.TitlePattern,RegexOptions.IgnoreCase);
                if(definition.Category!="AJ"){
                    result.Identifiers["fiscalYear"]=title.Groups["year"].Value;
                    if(definition.Category=="GL")result.Identifiers["PeriodTo"]=title.Groups["month"].Value+"/"+title.Groups["year"].Value;
                }else{
                    var years=new HashSet<int>();int count=0;
                    foreach(var r in ReadCsv(token).Skip(3)){token.ThrowIfCancellationRequested();if(r.Fields.All(string.IsNullOrWhiteSpace))continue;
                        if(DateTime.TryParseExact(Csv(r,"PostingDate"),new[]{"d.M.yyyy","dd.MM.yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)&&date.Year>=1900)years.Add(date.Year);count++;
                    }
                    SetYears(years);result.Identifiers["recordCount"]=count.ToString();result.Identifiers["fiscalYearBasis"]="CSV posting dates";
                }
            }else{
                result.Identifiers["AccountingEntityName"]=Xml(xmlHeader,"identifikacia/identifikator/nazov");result.Identifiers["cin"]=Xml(xmlHeader,"identifikacia/identifikator/ico");
                result.Identifiers["exportHeaderYear"]=Xml(xmlHeader,"obdobie/rok");
                var years=new HashSet<int>();int count=0;
                ReadXmlWithLegacyRepair(()=>{years.Clear();count=0;foreach(var r in XmlRecords(token)){if(XmlDate(r,out var date))years.Add(date.Year);else Issue("invalidSourceDate","Some XML dates are invalid or inconsistent; original values are retained for review.");count++;if(count>1000000)throw new InvalidDataException("Too many XML records.");}},token);
                SetYears(years);result.Identifiers["recordCount"]=count.ToString();result.Identifiers["fiscalYearBasis"]="XML record rok";
                if(result.Identifiers["fiscalYear"]!=null&&result.Identifiers["exportHeaderYear"]!=result.Identifiers["fiscalYear"])Issue("exportPeriodDiffers","XML header year "+result.Identifiers["exportHeaderYear"]+" differs from record year "+result.Identifiers["fiscalYear"]+".");
            }
            if(string.IsNullOrEmpty(result.Identifiers["cin"]))Issue("entityIdentifierUnavailable","Source does not establish IČO; confirm entity context. Filename is not used.");
        }
        bool XmlDate(XElement record,out DateTime date)
        {
            return DateTime.TryParseExact(X(record,"ucPripDat"),"dd.MM.yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out date)
                &&date.Year>=1900&&X(record,"rok")==date.Year.ToString(CultureInfo.InvariantCulture)
                &&X(record,"mes")==date.Month.ToString("00",CultureInfo.InvariantCulture);
        }
        IEnumerable<IfoSoftCsvJournalImporter.CsvRecord> ReadCsv(CancellationToken token)
        {
            // Recover only a single physical record with the exact column count,
            // and only malformed quoting inside its known description/name column.
            using(var reader=new StreamReader(path,Encoding.GetEncoding(1250),true)){
                int line=0,start=1;var buffer=new StringBuilder();string text;
                while((text=reader.ReadLine())!=null){token.ThrowIfCancellationRequested();line++;
                    if(buffer.Length==0)start=line;else buffer.Append("\r\n");buffer.Append(text);
                    if(TryCsv(buffer.ToString(),out var fields)){
                        yield return new IfoSoftCsvJournalImporter.CsvRecord{Fields=fields,RawRecord=buffer.ToString(),StartLineNumber=start,EndLineNumber=line};buffer.Clear();continue;
                    }
                    if(start==line&&definition!=null){
                        var parts=text.Split(';');int index=definition.Columns.TryGetValue("Description",out var description)?description:definition.Columns.TryGetValue("AccountName",out var name)?name:-1;
                        if(parts.Length==definition.Headers.Length&&index>=0&&parts[index].StartsWith("\"")&&parts[index].EndsWith("\"")){
                            bool valid=true;var recovered=new string[parts.Length];
                            for(int c=0;c<parts.Length;c++){
                                if(c==index){recovered[c]=parts[c].Substring(1,parts[c].Length-2);continue;}
                                if(!TryCsv(parts[c],out var cell)||cell.Length!=1){valid=false;break;}recovered[c]=cell[0];
                            }
                            if(valid){Issue("csvQuotesRecovered","Nonstandard quotes in description/name fields preserved; original description/name text retained.");
                                yield return new IfoSoftCsvJournalImporter.CsvRecord{Fields=recovered,RawRecord=text,StartLineNumber=line,EndLineNumber=line};buffer.Clear();continue;}
                        }
                    }
                    if(buffer.Length>1048576||line-start>100)throw new InvalidDataException(Path.GetFileName(path)+": ambiguous CSV quoting at line "+start);
                }
                if(buffer.Length>0)throw new InvalidDataException(Path.GetFileName(path)+": unterminated or invalid CSV quoting at line "+start);
            }
        }
        static bool TryCsv(string text,out string[] fields)
        {
            var values=new List<string>();var value=new StringBuilder();bool quoted=false,closed=false;
            for(int i=0;i<text.Length;i++){
                char c=text[i];
                if(quoted){if(c=='"'){if(i+1<text.Length&&text[i+1]=='"'){value.Append('"');i++;}else{quoted=false;closed=true;}}else value.Append(c);}
                else if(c==';'){values.Add(value.ToString());value.Clear();closed=false;}
                else if(c=='"'&&value.Length==0&&!closed)quoted=true;
                else if(closed||c=='"'){fields=null;return false;}
                else value.Append(c);
            }
            if(quoted){fields=null;return false;}values.Add(value.ToString());fields=values.ToArray();return true;
        }
        void SetYears(HashSet<int> years)
        {
            result.Identifiers["observedYears"]=string.Join(",",years.OrderBy(y=>y));result.Identifiers["fiscalYear"]=years.Count==1?years.Single().ToString():null;
            if(years.Count!=1)Issue("fiscalYearSelectionRequired","Records do not establish one fiscal year; caller selection is required for calculation.");
        }
        int Year=>int.TryParse(result.Identifiers["fiscalYear"],out var y)&&y>0?y:selectedYear??0;
        public ImportResult Examine(ImportLevel level,CancellationToken token=default)
        {
            token.ThrowIfCancellationRequested();if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));result.RequestedLevel=level;
            if(result.CompletedLevel==0){Recognize(token);result.CompletedLevel=1;}if(definition==null)return result;
            if(level>=ImportLevel.Identify&&result.CompletedLevel<2){Identify(token);result.CompletedLevel=2;}
            if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
                var rows=new List<SourceRow>();
                if(format=="CSV"){
                    var read=new List<IfoSoftCsvJournalImporter.CsvRecord>();foreach(var r in ReadCsv(token).Skip(3)){token.ThrowIfCancellationRequested();if(r.Fields.All(string.IsNullOrWhiteSpace))continue;if(r.Fields.Length!=definition.Headers.Length)throw new InvalidDataException(Path.GetFileName(path)+", "+r.Location+": expected "+definition.Headers.Length+" fields, found "+r.Fields.Length);
                        var fields=definition.Columns.ToDictionary(k=>k.Key,k=>r.Fields[k.Value]);for(int column=0;column<r.Fields.Length;column++)fields["source:"+column+":"+definition.Headers[column]]=r.Fields[column];fields["SourceLocation"]=r.Location;fields["SourceCsvRecord"]=r.RawRecord;
                        string kind=definition.Category+"Record";
                        if(definition.Category=="AF"){if(Csv(r,"AnalyticalCode")=="****")kind="AccountHeading";else if(Csv(r,"SyntheticCode")==""&&Csv(r,"AnalyticalCode")==""&&(Csv(r,"AccountName")==""||Csv(r,"AccountName").StartsWith("Prázdny účet",StringComparison.Ordinal)))kind="EmptyAccount";}
                        rows.Add(new SourceRow{Kind=kind,Fields=fields});read.Add(r);if(read.Count>500000)throw new InvalidDataException("Too many CSV records.");
                    }csvRows=read;
                }else{
                    List<XElement> read=null;ReadXmlWithLegacyRepair(()=>{read=XmlRecords(token).ToList();},token);xmlRows=read;
                    for(int i=0;i<read.Count;i++){token.ThrowIfCancellationRequested();var fields=new Dictionary<string,string>();foreach(var leaf in read[i].Descendants().Where(e=>!e.HasElements)){string key=string.Join("/",leaf.AncestorsAndSelf().TakeWhile(e=>e!=read[i]).Reverse().Select(e=>e.Name.LocalName));if(fields.ContainsKey(key))throw new InvalidDataException("Duplicate XML field: "+key);fields[key]=leaf.Value;}fields["SourceLocation"]="XML record "+(i+1);rows.Add(new SourceRow{Kind="AJRecord",Fields=fields});}
                }result.Rows=rows;result.CompletedLevel=3;
            }
            if(level>=ImportLevel.Validate&&result.CompletedLevel<4){validated=Validate(token);result.CompletedLevel=4;}
            result.Status=result.Issues.Any(i=>i.Severity=="error")?"invalid":"completed";
            if(level==ImportLevel.Normalize&&result.Status=="completed"&&Canonical==null){token.ThrowIfCancellationRequested();Canonical=validated;result.CompletedLevel=5;}
            return result;
        }
        object Validate(CancellationToken token)
        {
            if(result.Rows.Count==0)Issue("noData","No data records.","error");
            string hash;using(var sha=SHA256.Create())using(var f=File.OpenRead(path))hash=BitConverter.ToString(sha.ComputeHash(f)).Replace("-","").ToLowerInvariant();
            if(definition.Category=="AJ")return ValidateJournal(hash,token);
            object model=definition.Category=="GL"?(object)new GeneralLedgerImport():new AccountingFrameworkImport();SetMetadata(model,hash);
            if(definition.Category=="GL"){
                var gl=(GeneralLedgerImport)model;gl.PeriodHeader=result.Identifiers["PeriodTo"];int month=int.Parse(gl.PeriodHeader.Split('/')[0]);gl.ThroughMonth=month;
                if(month<0||month>14||(definition.Headers.Contains("{period}")&&(!SamePeriod(prefix[2].Fields[definition.Columns["PeriodDebitTurnover"]],gl.PeriodHeader)||!SamePeriod(prefix[2].Fields[definition.Columns["PeriodCreditTurnover"]],gl.PeriodHeader))))Issue("invalidPeriod","GL header period columns disagree with title.","error");
                if(selectedYear.HasValue&&result.Identifiers["fiscalYear"]!=selectedYear.Value.ToString())Issue("fiscalYearConflict","Caller year conflicts with GL source year.","error");
            }
            for(int i=0;i<csvRows.Count;i++){
                token.ThrowIfCancellationRequested();var r=csvRows[i];var source=result.Rows[i];if(source.Kind=="EmptyAccount"||source.Kind=="AccountHeading")continue;
                try{
                    object row=definition.Category=="GL"?(object)new GeneralLedgerRow():new AccountingFrameworkRow();
                    foreach(var map in definition.Columns){var prop=row.GetType().GetProperty(map.Key);if(prop==null)throw new InvalidDataException("Unknown canonical mapping: "+map.Key);string raw=Csv(r,map.Key);if(prop.PropertyType==typeof(decimal)){decimal amount=Amount(raw);prop.SetValue(row,amount);source.Amounts[map.Key]=amount;}else if(prop.PropertyType==typeof(string))prop.SetValue(row,JournalTextNormalizer.NormalizeText(raw,out _).Trim());else throw new InvalidDataException("Unsupported mapping type: "+map.Key);}
                    string syn=Csv(r,"SyntheticCode"),ana=Csv(r,"AnalyticalCode");if(syn.Length==0)throw new InvalidDataException("Synthetic account is blank.");
                    string account=AccountCodeNormalizer.Normalize(syn+ana);source.Account=account;source.Name=Csv(r,"AccountName");
                    if(row is GeneralLedgerRow gl){gl.SequenceNumber=((GeneralLedgerImport)model).Rows.Count+1;gl.SourceRecordNumber=i+1;gl.AccountCode=account;gl.SourceDimensions=new Dictionary<string,string>(source.Fields);gl.SourceAvailableAmountFields=source.Amounts.Keys.ToArray();((GeneralLedgerImport)model).Rows.Add(gl);}
                    else{var af=(AccountingFrameworkRow)row;af.SequenceNumber=((AccountingFrameworkImport)model).Rows.Count+1;af.SourceRecordNumber=i+1;af.SourceSyntheticCode=syn;af.SourceAnalyticalCode=ana;af.AccountCode=account;af.RowKind=ana.Length==0?AccountingFrameworkRowKind.SyntheticAccount:AccountingFrameworkRowKind.AnalyticalAccount;af.SourceFields=new Dictionary<string,string>(source.Fields);((AccountingFrameworkImport)model).Rows.Add(af);}
                }catch(InvalidDataException ex){Issue("invalidRecord",r.Location+": "+ex.Message,"error");}
            }return model;
        }
        object ValidateJournal(string hash,CancellationToken token)
        {
            var aj=new JournalImport();SetMetadata(aj,hash);int year=Year;decimal md=0,dal=0;
            for(int i=0;i<result.Rows.Count;i++){
                token.ThrowIfCancellationRequested();try{
                    JournalRow row;
                    if(format=="CSV"){
                        var source=csvRows[i];var ordered=AjColumns.Select(k=>Csv(source,k)).ToArray();
                        // The legacy mapper requires a year even for undated rows. This temporary
                        // value is not source identification and is reset before publication.
                        if(aj.FiscalYear==0)aj.FiscalYear=1900;
                        if(DateTime.TryParseExact(ordered[2],new[]{"d.M.yyyy","dd.MM.yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsedDate))ordered[2]=parsedDate.ToString("dd.MM.yyyy",CultureInfo.InvariantCulture);
                        row=IfoSoftCsvJournalImporter.MapJournalRow(new IfoSoftCsvJournalImporter.CsvRecord{Fields=ordered,StartLineNumber=source.StartLineNumber,EndLineNumber=source.EndLineNumber},i+1,i+1,aj);
                    }else{
                        var mapped=new XElement("veta");foreach(var k in definition.XmlFields.Keys){string value=X(xmlRows[i],k);if(new[]{"funkcKlasif","ekonKlasif","akciaCis"}.Contains(k)){var budget=mapped.Element("rozpocCis");if(budget==null){budget=new XElement("rozpocCis");mapped.Add(budget);}budget.Add(new XElement(k,value));}else mapped.Add(new XElement(k,value));}
                        bool validDate=XmlDate(xmlRows[i],out var sourceDate);if(!validDate){mapped.SetElementValue("ucPripDat","");mapped.SetElementValue("rok","");mapped.SetElementValue("mes","");}row=IfoSoftXmlJournalImporter.ReadRecord(mapped,i+1,year);if(!validDate)row.DateExceptionResolution=JournalDateExceptionResolution.Excluded;row.SequenceNumber=i+1;row.RecordKind=IfoSoftXmlJournalImporter.Classify(row);
                    }
                    row.SourceFields=new Dictionary<string,string>(result.Rows[i].Fields);
                    if(string.IsNullOrEmpty(row.DebitAccount)&&string.IsNullOrEmpty(row.CreditAccount))throw new InvalidDataException("Both journal accounts are blank.");
                    if((row.DebitAmount??0)!=0&&string.IsNullOrEmpty(row.DebitAccount)||(row.CreditAmount??0)!=0&&string.IsNullOrEmpty(row.CreditAccount))throw new InvalidDataException("Nonzero amount has no account.");
                    JournalDateReview.Apply(row,year,row.PostingDate!=DateTime.MinValue);
                    if(row.DateExceptionResolution==JournalDateExceptionResolution.Excluded)Issue("dateReviewRequired","Some journal dates need fiscal-year review; affected rows remain excluded from calculation.");
                    if(row.TextNormalizationApplied)Issue("sourceTextNormalized","Legacy text/date normalization applied; original field values retained in SourceFields.");
                    md+=row.DebitAmount??0;dal+=row.CreditAmount??0;
                    result.Rows[i].Amounts["DebitAmount"]=row.DebitAmount;result.Rows[i].Amounts["CreditAmount"]=row.CreditAmount;aj.Rows.Add(row);
                }catch(InvalidDataException ex){Issue("invalidRecord",result.Rows[i].Fields["SourceLocation"]+": "+ex.Message,"error");}
            }
            aj.FiscalYear=year;
            if(format=="XML"){
                try{if(string.IsNullOrWhiteSpace(xmlControl)||Amount(xmlControl)!=md+result.Rows.Count||md!=dal)Issue("xmlControlMismatch","XML control total does not agree with debit/credit amounts and source-record count.","error");}catch(InvalidDataException ex){Issue("xmlControlInvalid",ex.Message,"error");}
            }
            return aj;
        }
        static bool SamePeriod(string a,string b)
        {var x=a.Trim().Split('/');var y=b.Split('/');return x.Length==2&&y.Length==2&&int.TryParse(x[0],out var m)&&int.TryParse(y[0],out var n)&&m==n&&x[1]==y[1];}
        static decimal Amount(string raw)
        {if(string.IsNullOrWhiteSpace(raw))return 0;if(decimal.TryParse(raw,NumberStyles.Number,Sk,out var v))return v;throw new InvalidDataException("Invalid amount: "+raw);}
        void SetMetadata(object obj,string hash)
        {
            var values=new Dictionary<string,object>{{"SourceFileName",Path.GetFileName(path)},{"SourceFilePath",path},{"SourceFileHash",hash},{"TechnicalType",format},{"AccountingFormat",definition.Producer},{"Ico",result.Identifiers["cin"]},{"CompanyName",result.Identifiers["AccountingEntityName"]},{"FiscalYear",Year},{"ImportedAtUtc",DateTime.UtcNow}};
            foreach(var v in values)obj.GetType().GetProperty(v.Key).SetValue(obj,v.Value);
        }
        public void Dispose(){}
    }
}
