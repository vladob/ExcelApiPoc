using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
namespace PdfLayoutEngine.Simple;
public sealed partial class CompactSession
{
    static List<PositionedText[]> TableBands(IEnumerable<PositionedText> text){
        var bands=new List<PositionedText[]>();var row=new List<PositionedText>();double y=0;
        foreach(var v in text.OrderByDescending(v=>v.Baseline.Y)){
            if(row.Count>0&&y-v.Baseline.Y>3){bands.Add(row.OrderBy(v=>v.Baseline.X).ToArray());row.Clear();}
            if(row.Count==0)y=v.Baseline.Y;row.Add(v);
        }
        if(row.Count>0)bands.Add(row.OrderBy(v=>v.Baseline.X).ToArray());return bands;
    }
    static string MonetaryText(PositionedText[] band,Frame f,double[] column,Dictionary<PositionedText,int> order,out PositionedText[] selected){
        var chunks=new List<PositionedText[]>();var current=new List<PositionedText>();
        foreach(var v in band.OrderBy(v=>order[v])){
            bool numeric=Regex.IsMatch(v.Text,@"^[\s\d,.+−\-€]+$");
            if(current.Count>0){var prev=current[current.Count-1];if(!numeric||Math.Abs(v.Baseline.Y-prev.Baseline.Y)>.35||v.Baseline.X<prev.Baseline.X-.1||v.Baseline.X-prev.AdvanceEnd.X>Math.Max(3,prev.FontSize*.7)){chunks.Add(current.ToArray());current.Clear();}}
            if(numeric)current.Add(v);
        }
        if(current.Count>0)chunks.Add(current.ToArray());
        var candidates=chunks.Where(c=>c.Any(v=>v.Text.Any(char.IsDigit))).Select(c=>new{Tokens=c,Text=string.Concat(c.Select(v=>v.Text)).Replace("€","").Replace("−","-").Trim(),Left=c.Min(v=>v.Baseline.X),Right=c.Where(v=>v.Text.Any(char.IsDigit)).Max(v=>v.AdvanceEnd.X)})
            .Where(c=>c.Left>=f.Px(column[0])-1&&c.Right<=f.Px(column[1])+1&&Regex.IsMatch(c.Text,@"^[+-]?(?:\d{1,3}(?:[ \u00a0]\d{3})+|\d+)[,.]\d{2}$"))
            .OrderBy(c=>Math.Abs(c.Right-f.Px(column[1]))).ToArray();
        selected=candidates.Length>0?candidates[0].Tokens:Array.Empty<PositionedText>();
        return candidates.Length>0?candidates[0].Text:"";
    }
    static bool Matches(string s,string pattern)=>Regex.IsMatch(s,pattern,RegexOptions.IgnoreCase,TimeSpan.FromSeconds(1));
    static (Frame frame,int score)? FitTable(PositionedPage p,CompactLayout layout)
    {
        var t=layout.Table!;if(t.Orientation.Length>0&&t.Orientation!=(p.WidthPt>p.HeightPt?"landscape":"portrait"))return null;var bands=TableBands(p.Text.Where(v=>v.Baseline.Y>p.HeightPt*.65));
        var strings=bands.Select(LayoutSession.Assemble).ToArray();
        if(!strings.Any(s=>Matches(s,t.TitlePattern)))return null;
        if(t.RequiredPageText.Length>0&&!Compact(string.Concat(p.Text.Select(v=>v.Text))).Contains(Compact(t.RequiredPageText)))return null;
        string all=Compact(string.Join(" ",strings));
        if(t.RequiredHeader.Length>0&&!all.Contains(Compact(t.RequiredHeader)))return null;
        if(t.ForbiddenHeader.Length>0&&all.Contains(Compact(t.ForbiddenHeader)))return null;
        var left=bands.Select(b=>Find(b,t.LeftLabel)).FirstOrDefault(v=>v.Length>0);
        var right=bands.Select(b=>Find(b,t.RightLabel)).FirstOrDefault(v=>v.Length>0);
        var last=bands.Select(b=>Find(b,t.LastHeaderLabel)).FirstOrDefault(v=>v.Length>0);
        if(left==null||right==null||last==null)return null;
        double u=(right[0].Baseline.X-left[0].Baseline.X)/(t.Right-t.Left);
        if(u<.5||u>1.6)return null;
        return (new Frame{U=u,X=left[0].Baseline.X-u*t.Left,Y=last.Min(v=>v.Baseline.Y)-t.HeaderPadding*u},10);
    }
    string tableGroup="";
    string tableDocument="";
    decimal groupAmount;
    bool groupHasRows;
    ImportResult ExamineTable(ImportLevel level,CancellationToken token)
    {
        var d=Layout!;var t=d.Table!;
        if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
            var p=source.ReadPage(0,token);var lines=TableBands(p.Text.Where(v=>v.Baseline.Y>p.HeightPt*.65)).Select(LayoutSession.Assemble).ToArray();
            var header=string.Join("\n",lines);
            var cin=Regex.Match(header,@"IČO\s*:\s*(\d{8})");
            result.Identifiers["cin"]=cin.Success?cin.Groups[1].Value:null;
            result.Identifiers["AccountingEntityName"]=lines.FirstOrDefault()?.Split(new[]{"  "},StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()??"";
            // Read entity at the left edge, excluding right-aligned print information.
            var first=TableBands(p.Text).FirstOrDefault();
            if(first!=null)result.Identifiers["AccountingEntityName"]=LayoutSession.Assemble(first.Where(v=>v.Baseline.X<p.WidthPt*t.EntityWidth));
            var period=lines.FirstOrDefault(v=>v.IndexOf("Dátum",StringComparison.OrdinalIgnoreCase)>=0&&v.Contains("do:"));
            if(period!=null){var ds=Regex.Matches(period,@"\b\d{1,2}\.\d{1,2}\.\d{4}\b");if(ds.Count==2){result.Identifiers["PeriodFrom"]=ds[0].Value;result.Identifiers["PeriodTo"]=ds[1].Value;result.Identifiers["fiscalYear"]=Year(ds[1].Value)?.ToString();}}
            if(t.ControlMode=="export"){
                var yearMatch=Regex.Match(header,@"(?:Rok:|rok)\s*(\d{4})",RegexOptions.IgnoreCase);
                if(yearMatch.Success)result.Identifiers["fiscalYear"]=yearMatch.Groups[1].Value;
                var periodLine=lines.FirstOrDefault(v=>v.Contains("Obdobie:"));
                if(periodLine!=null){var dates=Regex.Matches(periodLine,@"\d{1,2}\.\s*\d{1,2}\.\s*\d{4}");if(dates.Count==2){result.Identifiers["PeriodFrom"]=Regex.Replace(dates[0].Value,@"\s","");result.Identifiers["PeriodTo"]=Regex.Replace(dates[1].Value,@"\s","");}}
                if(!cin.Success)Issue("entityIdentifierUnavailable","No verified IČO in report header; filename is not used.",null,"warning");
            }
            foreach(var identifier in t.IdentifierPatterns){var match=Regex.Match(header,identifier.Value,RegexOptions.Multiline);result.Identifiers[identifier.Key]=match.Success?match.Groups["value"].Value.Trim():null;}
            if(t.IdentifierPatterns.Count>0){
                result.Identifiers["fiscalYearBasis"]="report header";
                if(!cin.Success)Issue("entityIdentifierUnavailable","No verified IČO in report header; filename is not used.",null,"warning");
            }
            if(result.Identifiers.TryGetValue("accountingPeriod",out var accountingPeriod)&&SingleAccountJournal.Period(accountingPeriod??"",out var periodYear,out var periodMonth)){
                result.Identifiers["PeriodFrom"]=new DateTime(periodYear,periodMonth,1).ToString("dd.MM.yyyy");result.Identifiers["PeriodTo"]=new DateTime(periodYear,periodMonth,DateTime.DaysInMonth(periodYear,periodMonth)).ToString("dd.MM.yyyy");
            }
            result.PageCount=source.PageCount;result.Identifiers["orientation"]=p.WidthPt>p.HeightPt?"landscape":"portrait";result.CompletedLevel=2;
        }
        if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
            int previousPage=0;SourceRow? pending=null;
            for(int i=0;i<source.PageCount;i++){
                token.ThrowIfCancellationRequested();var p=source.ReadPage(i,token);
                if(!p.Text.Any(v=>!string.IsNullOrWhiteSpace(v.Text))){Issue("criticalPageContentMissing","Page has no readable text; extraction stopped.",i+1);extractionFailed=true;break;}
                var fit=FitTable(p,d);if(!fit.HasValue){Issue("pageLayoutMismatch","Table header could not be located.",i+1);continue;}
                var f=fit.Value.frame;var sourceOrder=p.Text.Select((v,n)=>(v,n)).ToDictionary(v=>v.v,v=>v.n);
                var header=string.Join(" ",TableBands(p.Text.Where(v=>v.Baseline.Y>f.Y)).Select(LayoutSession.Assemble));
                var pn=Regex.Match(header,@"Strana\s*:\s*(\d+)\s*/\s*(\d+)");
                if(t.PageNumberPattern.Length>0){var custom=Regex.Match(t.PageNumberInFooter?string.Join(" ",TableBands(p.Text).Select(LayoutSession.Assemble)):header,t.PageNumberPattern);if(!custom.Success||int.Parse(custom.Groups["page"].Value)!=i+1)Issue("pageSequence","Printed page sequence is inconsistent.",i+1);}
                else if(!pn.Success||int.Parse(pn.Groups[1].Value)!=previousPage+1||int.Parse(pn.Groups[2].Value)!=source.PageCount)Issue("pageSequence","Printed page sequence or total is inconsistent.",i+1);
                if(pn.Success)previousPage=int.Parse(pn.Groups[1].Value);
                var cin=Regex.Match(header,@"IČO\s*:\s*(\d{8})");if(cin.Success&&cin.Groups[1].Value!=Value(result.Identifiers,"cin"))Issue("inconsistentHeader","IČO differs between pages.",i+1);
                if(!t.ContinueAcrossPages)pending=null;
                foreach(var band in TableBands(p.Text.Where(v=>v.Baseline.Y<f.Y))){
                    token.ThrowIfCancellationRequested();string text=LayoutSession.Assemble(band);if(string.IsNullOrWhiteSpace(text))continue;
                    if(t.IgnorePattern.Length>0&&Matches(text,t.IgnorePattern))continue;
                    var g=Regex.Match(text,t.GroupPattern,RegexOptions.IgnoreCase);
                    if(g.Success){string name=g.Groups["group"].Value.Trim();if(name!=tableGroup){if(groupHasRows)Issue("missingGroupTotal","Previous journal group has no printed total.",i+1);tableGroup=name;groupAmount=0;groupHasRows=false;}pending=null;result.Rows.Add(new SourceRow{Kind="Group",Page=i+1,Fields=new Dictionary<string,string>{{"Group",tableGroup},{"text",text}}});continue;}
                    var row=new SourceRow{Page=i+1,BaselinePt=band.Average(v=>v.Baseline.Y)};
                    foreach(var c in t.Columns)row.Fields[c.Key]=LayoutSession.Assemble(band.Where(v=>v.Baseline.X>=f.Px(c.Value[0])-.2&&v.Baseline.X<f.Px(c.Value[1])-.2));
                    foreach(var key in t.CompactFields)row.Fields[key]=Regex.Replace(row.Fields[key],@"\s","");
                    var rule=t.Rules.FirstOrDefault(v=>Matches(row.Fields[v.Field],v.Pattern));
                    if(rule==null){
                        if(pending!=null&&t.ContinueAmountKinds.Contains(pending.Kind)&&t.Amounts.Length>0&&t.Amounts.All(k=>pending.Amounts.TryGetValue(k,out var old)&&!old.HasValue&&Value(row.Fields,k).Length>0)&&band.Where(v=>!string.IsNullOrWhiteSpace(v.Text)).All(v=>t.Amounts.Any(k=>v.Baseline.X>=f.Px(t.Columns[k][0])-.2&&v.Baseline.X<f.Px(t.Columns[k][1])-.2))){
                            foreach(var key in t.Amounts){pending.Fields[key]=row.Fields[key];if(TryMoney(row.Fields[key],out var amount))pending.Amounts[key]=amount;else Issue("invalidAmount",key+": "+row.Fields[key],i+1);}
                            pending.Fields["AmountContinuationPage"]=(i+1).ToString();pending=null;continue;
                        }

                        if(pending!=null&&pending.Kind==t.ContinuationKind&& (t.ContinuationFields.Length==0||band.Where(v=>!string.IsNullOrWhiteSpace(v.Text)).All(v=>t.ContinuationFields.Any(k=>v.Baseline.X>=f.Px(t.Columns[k][0])-.2&&v.Baseline.X<f.Px(t.Columns[k][1])-.2)))){
                            if(t.ContinuationField.Length>0){string extra=Value(row.Fields,t.ContinuationField);if(extra.Length>0)pending.Fields[t.ContinuationField]+=" "+extra;}
                            foreach(var key in t.ContinuationFields){if(Value(row.Fields,key).Length>0)pending.Fields[key]=(Value(pending.Fields,key)+" "+row.Fields[key]).Trim();}
                            pending.Fields["Continuation"]=(Value(pending.Fields,"Continuation")+" "+text).Trim();pending.Name=Value(pending.Fields,"Name","AccountName");continue;
                        }
                        if(text.Any(char.IsDigit)||d.Category=="AF")Issue("unresolvedRow",text,i+1);continue;
                    }
                    row.Kind=rule.Kind;if(t.ControlMode=="export"){
                        row.Fields["SourceLocation"]="page "+(i+1);
                        if(row.Kind=="DocumentStart"){tableDocument=Value(row.Fields,"Structure").Substring(7).Trim();}
                        if(d.Category=="AJ"&&row.Kind=="Record")row.Fields["DocumentNumber"]=tableDocument;
                    }row.Fields["Group"]=tableGroup;row.Fields["SourceText"]=text;
                    row.Account=Value(row.Fields,"Account","AnalyticalRowAccount");row.Name=Value(row.Fields,"Name","AccountName");
                    if((d.Category=="AF"||(d.Category=="GL"&&(t.ControlMode=="export"||t.ControlMode=="accountTotals")))&&row.Account.Length>=3){row.Fields["SyntheticAccount"]=row.Account.Substring(0,3);row.Fields["AnalythicAccount"]=row.Account.Substring(3);}
                    var amountGlyphs=new HashSet<PositionedText>();
                    foreach(var k in (t.ControlMode=="export"?(d.Category=="AJ"?(row.Kind=="Record"?t.Amounts:(row.Kind=="DocumentTotal"||row.Kind=="ReportTotal"?new[]{"DebitAccount","CreditAccount"}:Array.Empty<string>())):(row.Kind=="Record"||row.Kind=="ReportTotal"?t.Amounts:Array.Empty<string>())):(row.Kind=="Transaction"?Array.Empty<string>():t.Amounts))){var raw=MonetaryText(t.ControlMode=="export"?band.Where(v=>v.Baseline.X>=f.Px(t.Columns[k][0])-.2&&v.Baseline.X<f.Px(t.Columns[k][1])-.2).ToArray():band,f,t.Columns[k],sourceOrder,out var used).Replace("€","").Trim();if(!t.AmountsInSourceOrder||(t.ControlMode=="export"&&row.Kind!="Record"))raw=Value(row.Fields,k).Replace("€","").Trim();foreach(var glyph in used)amountGlyphs.Add(glyph);if(raw.Length==0)row.Amounts[k]=t.BlankAmountsAreZero?0m:(decimal?)null;else if(t.ControlMode=="export"?ExportControls.Money(raw,out var amount):TryMoney(raw,out amount))row.Amounts[k]=amount;else {row.Amounts[k]=null;Issue("invalidAmount",k+": "+raw,i+1);}}
                    if(t.AmountsInSourceOrder&&t.Columns.TryGetValue("OperationDescription",out var descriptionColumn))row.Fields["OperationDescription"]=LayoutSession.Assemble(band.Where(v=>v.Baseline.X>=f.Px(descriptionColumn[0])-.2&&v.Baseline.X<f.Px(descriptionColumn[1])-.2&&!amountGlyphs.Contains(v)));
                    if(t.Columns.TryGetValue("AccountName",out var nameColumn)){row.Name=LayoutSession.Assemble(band.Where(v=>v.Baseline.X>=f.Px(nameColumn[0])-.2&&v.Baseline.X<f.Px(nameColumn[1])-.2&&!amountGlyphs.Contains(v)));row.Fields["AccountName"]=row.Name;}
                    foreach(var sum in t.AmountSums)row.Amounts[sum.Key]=sum.Value.All(k=>row.Amounts.TryGetValue(k,out var value)&&value.HasValue)?sum.Value.Sum(k=>row.Amounts[k]!.Value):(decimal?)null;
                    result.Rows.Add(row);pending=d.ImportKinds.Contains(row.Kind)||t.ContinueAmountKinds.Contains(row.Kind)?row:null;
                    if(d.Category=="AJ"&&t.ControlMode==""){
                        if(d.ImportKinds.Contains(row.Kind)){groupHasRows=true;if(row.Amounts.TryGetValue("Amount",out var a)&&a.HasValue)groupAmount+=a.Value;}
                        if(row.Kind=="GroupTotal"){
                            if(!row.Amounts.TryGetValue("Amount",out var a)||!a.HasValue||Math.Abs(a.Value-groupAmount)>.005m)Issue("groupTotalMismatch",tableGroup+": extracted "+groupAmount.ToString(CultureInfo.InvariantCulture)+", printed "+Value(row.Fields,"Amount"),i+1);
                            groupAmount=0;groupHasRows=false;
                        }
                    }
                }
            }
            if(d.Category=="AJ"&&t.ControlMode==""&&groupHasRows)Issue("missingGroupTotal","Journal ends without a group total.");
            result.CompletedLevel=3;
        }
        if(level>=ImportLevel.Validate&&result.CompletedLevel<4){
            if(!source.Document.CompleteDecode)Issue("decodeIncomplete","Adapter reported unsupported source features.");
            var rows=result.Rows.Where(v=>d.ImportKinds.Contains(v.Kind)).ToArray();
            if(rows.Length==0)Issue("noData","No importable rows.");
            foreach(var row in rows){
                foreach(var k in t.Amounts)if(!row.Amounts[k].HasValue)Issue("missingAmount",k+" is blank.",row.Page);
                if(d.Category=="AJ"){
                    if(!Date(Value(row.Fields,"Date"),out var date)||date.Year<1900)Issue("invalidDate","Invalid source date: "+Value(row.Fields,"Date"),row.Page,"warning");
                    else if(Year(Value(result.Identifiers,"PeriodTo")) is int year&&date.Year!=year)Issue("dateOutsideFiscalYear","Source date is outside report year: "+Value(row.Fields,"Date"),row.Page,"warning");
                    if(Value(row.Fields,"DebitAccount").Length==0&&Value(row.Fields,"CreditAccount").Length==0)Issue("missingAccount","Journal account missing.",row.Page);
                }
                else if(row.Account.Length==0)Issue("missingAccount","Account missing.",row.Page);
            }
            if(t.ControlMode=="separateSides"||t.ControlMode=="accountTotals")IndependentControls.Validate(result,t.ControlMode,t.Amounts,true,t.AccountPattern);
            if(t.ControlMode=="export")ExportControls.Validate(result,d.Category,t.DocumentGroups);
            if(d.Category=="GL"&&t.ControlMode=="")foreach(var group in rows.GroupBy(v=>Value(v.Fields,"Group"))){
                var controls=result.Rows.Where(v=>v.Kind=="ReportTotal"&&Value(v.Fields,"Group")==group.Key).ToArray();
                if(controls.Length!=1){Issue("missingGroupTotal","Expected one GL total for "+group.Key);continue;}
                foreach(var key in t.Amounts){
                    decimal? printed=controls[0].Amounts.TryGetValue(key,out var amount)?amount:null;
                    if(!printed.HasValue||group.Any(v=>!v.Amounts.TryGetValue(key,out var a)||!a.HasValue)||Math.Abs(group.Sum(v=>v.Amounts[key]!.Value)-printed.Value)>.005m)Issue("groupTotalMismatch",group.Key+": "+key+" differs from printed total.",controls[0].Page);
                }
            }
            result.CompletedLevel=4;
        }
        result.DecodedPageCount=source.DecodedPageCount;result.Status=result.Issues.Any(v=>v.Severity=="error")?"invalid":"completed";return result;
    }
}
