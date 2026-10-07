using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
namespace PdfLayoutEngine.Simple;
/// <summary>Lazy, cumulative examination using a small catalogue of coordinate layouts.</summary>
public sealed partial class CompactSession : IDisposable
{
    readonly IPageSource source;
    readonly CompactLayout[] layouts;
    readonly ImportResult result=new ImportResult();
    readonly HashSet<Tuple<string,string,int?>> issueKeys=new HashSet<Tuple<string,string,int?>>();
    public CompactLayout? Layout { get; private set; }
    bool extractionFailed;
    readonly Dictionary<int,Frame> frames=new Dictionary<int,Frame>();
    public CompactSession(IPageSource source,IEnumerable<CompactLayout> layouts){this.source=source;this.layouts=layouts.ToArray();result.Format=source.Format;}
    public void Dispose()=>source.Dispose();
    struct Frame { public Dictionary<string,double[]>? WorksheetColumns; public double U,X,Y; public double Px(double x)=>X+x*U; public double Py(double y)=>Y-y*U; }
    static string Compact(string s)=>string.Concat(s.Where(c=>!char.IsWhiteSpace(c))).ToUpperInvariant();
    static List<PositionedText[]> Bands(IEnumerable<PositionedText> ts)
    {
        var result=new List<PositionedText[]>();var b=new List<PositionedText>();double y=0;
        foreach(var t in ts.OrderByDescending(t=>t.Baseline.Y)){
            if(b.Count>0 && y-t.Baseline.Y>1.6){result.Add(b.OrderBy(v=>v.Baseline.X).ToArray());b.Clear();}
            if(b.Count==0)y=t.Baseline.Y;b.Add(t);
        }
        if(b.Count>0)result.Add(b.OrderBy(v=>v.Baseline.X).ToArray());return result;
    }
    static PositionedText[] Find(PositionedText[] band,string label)
    {
        var chars=new List<char>();var map=new List<int>();
        for(int i=0;i<band.Length;i++)foreach(var c in Compact(band[i].Text)){chars.Add(c);map.Add(i);}
        int pos=new string(chars.ToArray()).IndexOf(Compact(label),StringComparison.Ordinal);
        return pos<0?Array.Empty<PositionedText>():band.Skip(map[pos]).Take(map[pos+Compact(label).Length-1]-map[pos]+1).ToArray();
    }
    // Fit header anchor starts; font size supplies an independent scale check.
    static (Frame frame,int score)? Fit(PositionedPage p,CompactLayout d)
    {
        if(d.Worksheet!=null)return FitWorksheet(p,d.Worksheet,0);
        if(d.Table!=null)return FitTable(p,d);
        var bands=Bands(p.Text.Where(t=>t.Baseline.Y>p.HeightPt*.65));
        if(d.RequiredText.Any(label=>!bands.Any(b=>Find(b,label).Length>0)))return null;
        var hits=new List<(CompactAnchor a,PositionedText[] ts)>();
        foreach(var a in d.Anchors){var ts=bands.Select(b=>Find(b,a.Text)).FirstOrDefault(t=>t.Length>0);if(ts!=null)hits.Add((a,ts));}
        if(hits.Count<2)return null;
        (Frame,int)? best=null;
        foreach(var h in hits)foreach(var k in hits){
            double dx=k.a.Rect[0]-h.a.Rect[0];if(Math.Abs(dx)<500)continue;
            double u=(k.ts[0].Baseline.X-h.ts[0].Baseline.X)/dx;
            if(u<72/d.UnitsPerInch*.6 || u>72/d.UnitsPerInch*1.5)continue;
            double y=h.ts.Average(t=>t.Baseline.Y)+(h.a.Rect[1]+h.a.Size*d.UnitsPerInch/72*.8)*u;
            var f=new Frame{U=u,X=h.ts[0].Baseline.X-h.a.Rect[0]*u,Y=y};
            int score=hits.Count(v=>Math.Abs(v.ts[0].Baseline.X-f.Px(v.a.Rect[0]))<3 && Math.Abs(v.ts.Average(t=>t.Baseline.Y)-f.Py(v.a.Rect[1]+v.a.Size*d.UnitsPerInch/72*.8))<3);
            if(score>=2 && (!best.HasValue || score>best.Value.Item2))best=(f,score);
        }
        if(best.HasValue){
            var f=best.Value.Item1;
            var good=hits.Where(v=>Math.Abs(v.ts[0].Baseline.X-f.Px(v.a.Rect[0]))<3 && Math.Abs(v.ts.Average(t=>t.Baseline.Y)-f.Py(v.a.Rect[1]+v.a.Size*d.UnitsPerInch/72*.8))<3).ToArray();
            double mx=good.Average(v=>v.a.Rect[0]),my=good.Average(v=>v.ts[0].Baseline.X);
            double den=good.Sum(v=>Math.Pow(v.a.Rect[0]-mx,2));
            if(den>0){f.U=good.Sum(v=>(v.a.Rect[0]-mx)*(v.ts[0].Baseline.X-my))/den;f.X=my-f.U*mx;}
            var ys=good.Select(v=>v.ts.Average(t=>t.Baseline.Y)+(v.a.Rect[1]+v.a.Size*d.UnitsPerInch/72*.8)*f.U).OrderBy(v=>v).ToArray();f.Y=ys[ys.Length/2];
            best=(f,best.Value.Item2);
        }
        return best;
    }
    static PositionedText[] InX(IEnumerable<PositionedText> ts,Frame f,double[] r)=>ts.Where(t=>t.Baseline.X>=f.Px(r[0])-.2 && t.Baseline.X<f.Px(r[2])-.1).ToArray();
    static string Read(IEnumerable<PositionedText> ts,Frame f,double[] r)=>LayoutSession.Assemble(InX(ts.Where(t=>t.Baseline.Y<=f.Py(r[1])+.5 && t.Baseline.Y>=f.Py(r[3])-.5),f,r));
    void Issue(string code,string message,int? page=null,string severity="error") {if(issueKeys.Add(Tuple.Create(code,message,page)))result.Issues.Add(new ImportIssue{Code=code,Message=message,Page=page,Severity=severity});}
    public ImportResult Examine(ImportLevel level,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();result.RequestedLevel=level;
        if(extractionFailed)return result;
        if(result.CompletedLevel==0){
            var p=source.ReadPage(0,token);
            if(!p.Text.Any(v=>!string.IsNullOrWhiteSpace(v.Text))){Issue("noReadableText","No readable text on the first page. Scanned/image-only documents require a text export; OCR is not supported.",1);result.Status="unsupported";result.PageCount=source.PageCount;result.DecodedPageCount=source.DecodedPageCount;result.CompletedLevel=1;extractionFailed=true;return result;}
            var candidates=layouts.Select(d=>(d,fit:Fit(p,d))).Where(x=>x.fit.HasValue&&(x.d.Worksheet==null||!x.d.Worksheet.DynamicHeaders||WorksheetPanelsMatch(x.d.Worksheet,token))).OrderByDescending(x=>x.fit!.Value.score).ToArray();
            result.CandidateLayouts=candidates.Select(c=>c.d.Id).ToList();
            if(candidates.Length>0 && candidates.Select(c=>c.d.Producer).Distinct(StringComparer.OrdinalIgnoreCase).Count()==1 && (candidates.Length==1 || candidates[0].fit!.Value.score>candidates[1].fit!.Value.score)){
                Layout=candidates[0].d;frames[0]=candidates[0].fit!.Value.frame;result.LayoutId=Layout.Id;result.Category=Layout.Category;result.Producer=Layout.Producer;
            } else if(candidates.Length>0){result.Status="ambiguous";Issue("ambiguousLayout","More than one layout matches the header: "+string.Join(", ",result.CandidateLayouts));}
            result.CompletedLevel=1;
        }
        result.DecodedPageCount=source.DecodedPageCount;
        if(Layout==null)return result;
        var d=Layout;
        if(d.Worksheet!=null)return ExamineWorksheet(level,token);
        if(d.Table!=null)return ExamineTable(level,token);
        if(level>=ImportLevel.Identify && result.CompletedLevel<2){
            var p=source.ReadPage(0,token);result.PageCount=source.PageCount;
            foreach(var h in d.Header)result.Identifiers[h.Key]=Read(p.Text,frames[0],h.Value);
            result.Identifiers["orientation"]=p.WidthPt>p.HeightPt?"landscape":"portrait";
            result.Identifiers["cin"]=null;
            result.Identifiers["fiscalYear"]=Year(Value(result.Identifiers,"PeriodTo"))?.ToString(CultureInfo.InvariantCulture);
            result.CompletedLevel=2;
        }
        if(level>=ImportLevel.Extract && result.CompletedLevel<3){
            var rows=new List<SourceRow>();
            for(int i=0;i<source.PageCount;i++){
                token.ThrowIfCancellationRequested();var p=source.ReadPage(i,token);
                if(!p.Text.Any(t=>!string.IsNullOrWhiteSpace(t.Text))){
                    Issue("criticalPageContentMissing","Page has no readable text; report completeness cannot be established. Import stopped.",i+1);
                    result.Rows=rows;result.Status="invalid";result.DecodedPageCount=source.DecodedPageCount;extractionFailed=true;return result;
                }
                var fit=Fit(p,d);
                if(!fit.HasValue){Issue("pageLayoutMismatch","Header could not be aligned.",i+1);continue;}
                frames[i]=fit.Value.frame;var head=new SourceRow{Kind="PageLevel",Page=i+1};foreach(var h in d.Header)head.Fields[h.Key]=Read(p.Text,fit.Value.frame,h.Value);rows.Add(head);
                Extract(p,fit.Value.frame,rows,token);
            }
            // Document identifiers follow the group in DENNIK; do not invent them from counters.
            if(d.Category=="AJ"){
                var pending=new List<SourceRow>();foreach(var row in rows){
                    if(row.Kind=="AjRecord")pending.Add(row);
                    if(row.Kind=="DocumentTotals"){
                        foreach(var detail in pending)foreach(var key in new[]{"DocumentNumber","DocumentSubnumber","DocumentNo","Date"})
                            if(!detail.Fields.ContainsKey(key)&&row.Fields.TryGetValue(key,out var value))detail.Fields[key]=value;
                        pending.Clear();
                    }
                }
                if(d.Sections.Any(s=>s.Kind=="DocumentTotals")&&pending.Count>0)Issue("missingDocumentFooter","Records have no following document subtotal.");
            }
            result.Rows=rows;result.CompletedLevel=3;
        }
        if(level>=ImportLevel.Validate && result.CompletedLevel<4){Validate(token);result.CompletedLevel=4;}
        result.DecodedPageCount=source.DecodedPageCount;result.Status=result.Issues.Any(i=>i.Severity=="error")?"invalid":"completed";return result;
    }
    static IEnumerable<PositionedText> YRange(PositionedText[] sorted,double low,double high)
    {
        int left=0,right=sorted.Length;
        while(left<right){int mid=left+(right-left)/2;if(sorted[mid].Baseline.Y>high)left=mid+1;else right=mid;}
        for(int i=left;i<sorted.Length && sorted[i].Baseline.Y>=low;i++)yield return sorted[i];
    }
    void Extract(PositionedPage p,Frame f,List<SourceRow> rows,CancellationToken token)
    {
        if(p.Text.Any(t=>t.IsClipped))Issue("clippedText","Source clips some glyph outlines; explicit Unicode is retained.",p.PhysicalPageIndex+1,"warning");
        var d=Layout!;double headerBottom=d.Anchors.Max(a=>a.Rect[3]);
        var body=p.Text.Where(t=>t.Baseline.Y<f.Py(headerBottom)-.5).OrderByDescending(t=>t.Baseline.Y).ToArray();var bands=Bands(body);
        var consumed=new HashSet<PositionedText>();bool ended=false;
        foreach(var band in bands){
            token.ThrowIfCancellationRequested();if(ended)break;
            if(band.All(t=>consumed.Contains(t)))continue;
            CompactSection? selected=null;PositionedText[] anchor=Array.Empty<PositionedText>();
            foreach(var s in d.Sections.OrderByDescending(s=>s.Anchor.Text.Length>0)){
                var a=s.Anchor;var ts=InX(band,f,a.Rect).Where(t=>!string.IsNullOrWhiteSpace(t.Text)).ToArray();
                if(a.Text.Length>0){ts=Find(band,a.Text);if(ts.Length==0 || ts[0].Baseline.X>f.Px(s.Fields.Values.Select(r=>r[0]).DefaultIfEmpty(a.Rect[2]).Max()))continue;}
                else {
                    if(ts.Length==0 || ts.All(t=>consumed.Contains(t)))continue;
                    double size=a.Size*f.U/(72/d.UnitsPerInch);
                    if(ts.Any(t=>a.Weight.HasValue && (t.FontWeight<=0 || (t.FontWeight>=600)!=(a.Weight>=600))))continue;
                    string raw=LayoutSession.Assemble(ts);
                    if(d.Category=="AJ" && !TryMoney(raw,out _))continue;
                    if(d.Category!="AJ" && a.Numeric && !raw.Any(char.IsDigit))continue;
                }
                selected=s;anchor=ts;break;
            }
            if(selected==null)continue;
            var def=selected;var local=new Frame{U=f.U,X=f.X,Y=anchor.Average(t=>t.Baseline.Y)+(def.Anchor.Rect[1]+(def.Anchor.Rect[3]-def.Anchor.Rect[1])*.75)*f.U};
            var row=new SourceRow{Page=p.PhysicalPageIndex+1,Kind=def.Kind,BaselinePt=anchor.Average(t=>t.Baseline.Y)};
            foreach(var field in def.Fields){
                if(d.Category!="AJ" && def.Kind!="ReportLevel"){
                    var fieldTokens=InX(band,f,field.Value);
                    if(source.Format.Equals("PDF",StringComparison.OrdinalIgnoreCase) && def.Amounts.Contains(field.Key)){
                        var moneyTokens=band.Where(t=>t.Baseline.X>=f.Px(def.Amounts.Min(k=>def.Fields[k][0])) && t.Text.Any(char.IsDigit)).ToArray();
                        if(moneyTokens.Length>0){var ys=moneyTokens.Select(t=>t.Baseline.Y).OrderBy(v=>v).ToArray();double amountY=ys[ys.Length/2];fieldTokens=fieldTokens.Where(t=>Math.Abs(t.Baseline.Y-amountY)<.4).ToArray();}
                    }
                    row.Fields[field.Key]=LayoutSession.Assemble(fieldTokens);
                }
                else {
                    var rect=field.Value;double expected=local.Py(rect[1]+(rect[3]-rect[1])*.75);
                    row.Fields[field.Key]=LayoutSession.Assemble(InX(YRange(body,expected-2.1,expected+2.1),local,rect));
                }
            }
            if(def.ItemFields.Any(k=>row.Fields[k].Length>0))row.Kind="ItemRow";
            row.Account=Value(row.Fields,"AnalyticalRowAccount","AnalyticalAccount","SyntheticAccount","AccountClass");row.Name=Value(row.Fields,"AccountName","Name");
            if(d.Category=="AF" && row.Kind=="AnalythicAccount" && Value(row.Fields,"SyntheticAccount").Length==0 && Value(row.Fields,"AnalythicAccount").Trim('*').Length==0 && (Value(row.Fields,"Name").Length==0 || string.Equals(Value(row.Fields,"Name").Trim(),"Prázdny účet",StringComparison.OrdinalIgnoreCase)))row.Kind="EmptyAccount";
            if(d.Category=="AF" && row.Kind=="AnalythicAccount" && Value(row.Fields,"AnalythicAccount")=="****")row.Kind="AccountHeading";
            rows.Add(row);
            double top=Math.Min(def.Anchor.Rect[1],def.Fields.Values.Select(r=>r[1]).DefaultIfEmpty(def.Anchor.Rect[1]).Min());
            double bottom=def.Kind=="ReportLevel"?def.Anchor.Rect[3]:def.Fields.Values.Select(r=>r[3]).DefaultIfEmpty(def.Anchor.Rect[3]).Max();
            foreach(var t in YRange(body,local.Py(bottom)-1,local.Py(top)+1))consumed.Add(t);
            if(def.Kind=="ReportLevel")ended=true;
        }
        // Every unexplained numeric body band is visible to validation; never silently lose a transaction.
        foreach(var b in bands.Where(b=>b.Any(t=>!consumed.Contains(t)&&t.Text.Any(char.IsDigit)))){
            if(ended && b.Average(t=>t.Baseline.Y)<rows.Last().BaselinePt)continue;
            rows.Add(new SourceRow{Kind="Unresolved",Page=p.PhysicalPageIndex+1,Fields=new Dictionary<string,string>{{"text",LayoutSession.Assemble(b)}}});
        }
    }
    public static string Value<T>(IDictionary<string,T> values,params string[] keys){foreach(var key in keys)if(values.TryGetValue(key,out var v)&&v!=null)return v.ToString()??"";return "";}
    public static int? Year(string raw){if(Date(raw,out var date))return date.Year;var m=Regex.Match(raw,@"(?:^|[/\.])(\d{4})$");return m.Success?int.Parse(m.Groups[1].Value):(int?)null;}
    public static bool Date(string raw,out DateTime date)=>DateTime.TryParseExact(raw,new[]{"d.M.yyyy","dd.MM.yyyy","d.M.yy","dd.MM.yy","dd/MM/yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out date);
    public static bool TryMoney(string raw,out decimal amount){
        if(LayoutSession.TryAmount(raw,out amount))return true;
        string compact=Regex.Replace(raw,@"\s","");
        return Regex.IsMatch(compact,@"^[+-]?\d+\.\d{2}$") && decimal.TryParse(compact,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out amount);
    }
    void Validate(CancellationToken token){
        var d=Layout!;int? previousPrintedPage=null;
        if(!source.Document.CompleteDecode)Issue("decodeIncomplete","Adapter reported unsupported source features.");
        foreach(var row in result.Rows){
            token.ThrowIfCancellationRequested();
            if(row.Kind=="Unresolved"){Issue("unresolvedRow",row.Fields["text"],row.Page);continue;}
            if(row.Kind=="PageLevel"){
                foreach(var key in d.Header.Keys.Where(k=>k.StartsWith("Period"))){
                    var raw=Value(row.Fields,key);var m=Regex.Match(raw,@"^(\d{2})/(\d{4})$");
                    if(!Date(raw,out _) && !(m.Success && int.Parse(m.Groups[1].Value)<=14 && int.Parse(m.Groups[2].Value)>=1900))Issue("invalidPeriod",key+": "+raw,row.Page);
                }
                foreach(var key in d.Header.Keys.Where(k=>k!="PageNumber"&&k!="PrintDate"&&k!="PrintTime"))if(Value(row.Fields,key)!=Value(result.Identifiers,key))Issue("inconsistentHeader",key+" differs between pages.",row.Page);
                if(row.Fields.TryGetValue("PageNumber",out var page)){
                    if(!int.TryParse(page.TrimStart(':',' '),out int pn)||pn<1)Issue("pageSequence","Invalid printed page number: "+page,row.Page);
                    else {
                        if(!previousPrintedPage.HasValue && pn!=1)Issue("partialReport","Report starts at printed page "+pn+"; coverage is incomplete.",row.Page,"warning");
                        if(previousPrintedPage.HasValue && pn!=previousPrintedPage.Value+1)Issue("pageSequence","Printed pages are not consecutive.",row.Page);
                        previousPrintedPage=pn;
                    }
                }
                continue;
            }
            var def=d.Sections.FirstOrDefault(s=>s.Kind==row.Kind || row.Kind=="ItemRow"&&s.ItemFields.Length>0);if(def==null)continue;
            foreach(var key in def.Amounts){string raw=Value(row.Fields,key);if(raw.Length==0)row.Amounts[key]=0;else if(TryMoney(raw,out var amount))row.Amounts[key]=amount;else Issue("invalidAmount",key+": "+raw,row.Page);}
            if(d.ImportKinds.Contains(row.Kind)){
                if(d.Category=="GL"&&row.Account.Length==0)Issue("missingAccount","Account is blank.",row.Page);
                if(d.Category=="AJ"){
                    if(!Date(Value(row.Fields,"Date"),out var postingDate)||postingDate.Year<1900)Issue("invalidDate","Missing, invalid or implausible posting date: "+Value(row.Fields,"Date"),row.Page,"warning");
                    else if(Year(Value(result.Identifiers,"PeriodTo")) is int fiscalYear && postingDate.Year!=fiscalYear)Issue("dateOutsideFiscalYear","Posting date outside report year: "+Value(row.Fields,"Date"),row.Page,"warning");
                    if(Value(row.Fields,"DebitAccount","Debit").Length==0 && Value(row.Fields,"CreditAccount","Credit").Length==0)Issue("missingAccount","Both journal accounts are blank.",row.Page);
                }
                if(d.Category=="AF"&&Value(row.Fields,"SyntheticAccount").Length==0)Issue("missingAccount","Synthetic code is blank.",row.Page);
            }
        }
        if(!result.Rows.Any(r=>d.ImportKinds.Contains(r.Kind)))Issue("noData","No importable records were extracted.");
        if(Value(result.Identifiers,"AccountingEntityName").Length==0)Issue("entityUnavailable","Only licensed-user information is printed; entity identification requires caller confirmation.",severity:"warning");
        if(d.Category!="AF" && !Year(Value(result.Identifiers,"PeriodTo")).HasValue)Issue("yearUnavailable","Header does not establish a fiscal year.",severity:"warning");
    }
}
