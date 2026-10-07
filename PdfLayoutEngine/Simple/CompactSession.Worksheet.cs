using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
namespace PdfLayoutEngine.Simple;
public sealed partial class CompactSession
{
    static (Frame frame,int score)? FitWorksheet(PositionedPage page,WorksheetLayout layout,int panelIndex,WorksheetPanel? alternative=null){
        if(Math.Abs(page.WidthPt-layout.PageWidth)>2||Math.Abs(page.HeightPt-layout.PageHeight)>2)return null;
        var panel=alternative??layout.Panels[panelIndex];
        foreach(var band in TableBands(page.Text.Where(t=>t.Baseline.Y>page.HeightPt*.8))){
            if(layout.DynamicHeaders){
                var headers=panel.HeaderFields.Select(h=>new{Field=h.Key,Text=Find(band,h.Value)}).ToArray();
                if(headers.Any(h=>h.Text.Length==0))continue;
                var used=new HashSet<PositionedText>(headers.SelectMany(h=>h.Text));
                if(band.Any(t=>!string.IsNullOrWhiteSpace(t.Text)&&!used.Contains(t)))continue;
                var ordered=headers.OrderBy(h=>h.Text[0].Baseline.X).ToArray();
                var expected=panel.Columns.OrderBy(c=>c.Value[0]).Select(c=>c.Key).ToArray();
                if(!ordered.Select(h=>h.Field).SequenceEqual(expected))continue;
                double panelRight=panel.Columns.Values.Max(c=>c[1]);
                var columns=new Dictionary<string,double[]>();bool valid=true;
                for(int n=0;n<ordered.Length;n++){
                    double columnLeft=ordered[n].Text[0].Baseline.X-.6,end=n+1<ordered.Length?ordered[n+1].Text[0].Baseline.X-.6:panelRight;
                    if(end<=columnLeft||ordered[n].Text.Max(t=>t.AdvanceEnd.X)>end+1){valid=false;break;}
                    columns.Add(ordered[n].Field,new[]{columnLeft,end});
                }
                if(valid)return(new Frame{U=1,X=0,Y=band.Min(t=>t.Baseline.Y)-2,WorksheetColumns=columns},headers.Length);
                continue;
            }
            var found=panel.Headers.Select(h=>new{Header=h,Text=Find(band,h.Key)}).ToArray();if(found.Any(h=>h.Text.Length==0))continue;
            var left=found.OrderBy(h=>h.Header.Value).First();var right=found.OrderBy(h=>h.Header.Value).Last();
            double scale=(right.Text[0].Baseline.X-left.Text[0].Baseline.X)/(right.Header.Value-left.Header.Value);
            if(scale<.9||scale>1.1)continue;
            var frame=new Frame{U=scale,X=left.Text[0].Baseline.X-scale*left.Header.Value,Y=band.Min(t=>t.Baseline.Y)-2};
            if(found.All(h=>Math.Abs(h.Text[0].Baseline.X-frame.Px(h.Header.Value))<1.5))return(frame,panel.Headers.Count);
        }return null;
    }
    bool WorksheetPanelsMatch(WorksheetLayout layout,CancellationToken token){
        if(source.PageCount%layout.Panels.Count!=0)return false;
        int count=source.PageCount/layout.Panels.Count;
        for(int n=0;n<layout.Panels.Count;n++){
            var page=source.ReadPage(n*count,token);
            if(new[]{layout.Panels[n]}.Concat(layout.Panels[n].Alternatives).Count(p=>FitWorksheet(page,layout,n,p).HasValue)!=1)return false;
        }
        return true;
    }
    List<(double y,Dictionary<string,string> fields)> WorksheetRows(PositionedPage page,WorksheetLayout layout,int panelIndex,Frame frame,bool first,WorksheetPanel? alternative=null){
        var rows=new List<(double,Dictionary<string,string>)>();var panel=alternative??layout.Panels[panelIndex];
        foreach(var band in TableBands(page.Text.Where(t=>t.Baseline.Y>layout.BodyBottom&&t.Baseline.Y<(first?frame.Y:page.HeightPt-layout.BodyTop)))){
            if(band.All(t=>string.IsNullOrWhiteSpace(t.Text)))continue;
            var fields=new Dictionary<string,string>();foreach(var column in frame.WorksheetColumns??panel.Columns)fields[column.Key]=LayoutSession.Assemble(band.Where(t=>t.Baseline.X>=frame.Px(column.Value[0])-.2&&t.Baseline.X<frame.Px(column.Value[1])-.2));
            if(band.Any(t=>!string.IsNullOrWhiteSpace(t.Text)&&!(frame.WorksheetColumns??panel.Columns).Values.Any(c=>t.Baseline.X>=frame.Px(c[0])-.2&&t.Baseline.X<frame.Px(c[1])-.2)))Issue("unmappedWorksheetText","Text outside declared columns.",page.PhysicalPageIndex+1);
            rows.Add((band.Average(t=>t.Baseline.Y),fields));
        }return rows;
    }
    ImportResult ExamineWorksheet(ImportLevel level,CancellationToken token){
        var layout=Layout!.Worksheet!;
        if(level>=ImportLevel.Identify&&result.CompletedLevel<2){
            var page=source.ReadPage(0,token);var first=WorksheetRows(page,layout,0,frames[0],true).FirstOrDefault();
            result.Identifiers["cin"]=null;result.Identifiers["AccountingEntityName"]=null;
            Issue("entityIdentifierUnavailable","Printed worksheet has no verified entity identification; filename is not used.",null,"warning");
            SingleAccountJournal.Identify(result,first.fields==null?"":Value(first.fields,"AccountingPeriod"));result.PageCount=source.PageCount;
            result.Identifiers["orientation"]=page.WidthPt>page.HeightPt?"landscape":"portrait";result.CompletedLevel=2;
        }
        if(level>=ImportLevel.Extract&&result.CompletedLevel<3){
            if(source.PageCount%layout.Panels.Count!=0){Issue("incompletePanels","Page count does not divide into the required worksheet panels.");extractionFailed=true;}
            else{
                int pagesPerPanel=source.PageCount/layout.Panels.Count;var fits=new Frame[layout.Panels.Count];var resolved=new WorksheetPanel[layout.Panels.Count];bool valid=true;
                for(int panel=0;panel<fits.Length;panel++){
                    var firstPage=source.ReadPage(panel*pagesPerPanel,token);var candidates=new[]{layout.Panels[panel]}.Concat(layout.Panels[panel].Alternatives).Select(candidate=>new{Panel=candidate,Fit=FitWorksheet(firstPage,layout,panel,candidate)}).Where(x=>x.Fit.HasValue).ToArray();var fit=candidates.Length==1?candidates[0].Fit:null;if(!fit.HasValue){Issue("panelHeaderMismatch","Expected worksheet panel header is missing.",panel*pagesPerPanel+1);valid=false;break;}fits[panel]=fit.Value.frame;resolved[panel]=candidates[0].Panel;
                }
                if(valid)for(int pageIndex=0;pageIndex<pagesPerPanel;pageIndex++){
                    token.ThrowIfCancellationRequested();List<(double y,Dictionary<string,string> fields)>? merged=null;var pageRefs=new List<int>();
                    for(int panel=0;panel<fits.Length;panel++){
                        int physical=panel*pagesPerPanel+pageIndex;var page=source.ReadPage(physical,token);pageRefs.Add(physical+1);
                        if(Math.Abs(page.WidthPt-layout.PageWidth)>2||Math.Abs(page.HeightPt-layout.PageHeight)>2){Issue("pageLayoutMismatch","Worksheet page size changed.",physical+1);valid=false;break;}
                        var rows=WorksheetRows(page,layout,panel,fits[panel],pageIndex==0,resolved[panel]);
                        if(rows.Count==0){Issue("criticalPageContentMissing","Worksheet panel contains no rows.",physical+1);valid=false;break;}
                        if(merged==null)merged=rows;
                        else{
                            if(rows.Count!=merged.Count||rows.Where((r,n)=>Math.Abs(r.y-merged[n].y)>.8).Any()){Issue("panelRowMismatch","Worksheet panels have different row counts or baselines; cannot safely join.",physical+1);valid=false;break;}
                            for(int r=0;r<rows.Count;r++)foreach(var field in rows[r].fields)merged[r].fields.Add(field.Key,field.Value);
                        }
                    }
                    if(!valid)break;
                    foreach(var item in merged!){
                        var row=new SourceRow{Kind="Record",Page=pageIndex+1,BaselinePt=item.y,Fields=item.fields};row.Fields["SourcePages"]=string.Join(",",pageRefs);row.Fields["SourceLocation"]="pages "+string.Join(",",pageRefs)+", row "+(result.Rows.Count+1);result.Rows.Add(row);
                        if(result.Rows.Count>500000)throw new System.IO.InvalidDataException("Maximum source rows exceeded.");
                    }
                }
                if(!valid)extractionFailed=true;
            }
            result.CompletedLevel=3;
        }
        if(level>=ImportLevel.Validate&&result.CompletedLevel<4&&!extractionFailed){
            foreach(var row in result.Rows)foreach(var key in new[]{"DebitAmount","CreditAmount"})if(Value(row.Fields,key).Length==0)Issue("missingAmount","Printed amount is missing: "+key,row.Page);
            SingleAccountJournal.Validate(result);if(!source.Document.CompleteDecode)Issue("decodeIncomplete","Adapter reported unsupported features.");
            Issue("noPrintedTotals","Worksheet printout contains no printed control totals; reconcile against its structured export.",null,"warning");result.CompletedLevel=4;
        }
        result.DecodedPageCount=source.DecodedPageCount;result.Status=result.Issues.Any(i=>i.Severity=="error")?"invalid":"completed";return result;
    }
}
