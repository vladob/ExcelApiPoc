using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
namespace PdfLayoutEngine.Simple;

/// <summary>One file, one caller. Retains decoded pages between cumulative stages; dispose after import/cancel.</summary>
public sealed class LayoutSession : IDisposable
{
    private readonly IPageSource source;
    private readonly LayoutDefinition layout;
    private readonly ImportResult result = new ImportResult();
    private readonly Dictionary<int, Frame> frames = new Dictionary<int, Frame>();
    public LayoutSession(IPageSource source, LayoutDefinition layout) { layout.Validate(); this.source = source; this.layout = layout; result.Format = source.Format; }
    public void Dispose() => source.Dispose();
    public ImportResult Examine(ImportLevel level, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (level < ImportLevel.Recognize || level > ImportLevel.Validate) throw new ArgumentOutOfRangeException(nameof(level), "Canonical mapping belongs to the accounting importer (level 5).");
        result.RequestedLevel = level;
        if (result.CompletedLevel == 0)
        {
            // Recognition deliberately reads at most the first three pages, never the whole corpus.
            for (int i = 0; i < Math.Min(3, source.PageCount); i++)
            {
                token.ThrowIfCancellationRequested();
                var p = source.ReadPage(i, token); var text = Compact(string.Concat(p.Text.Select(t=>t.Text)));
                if (text.Contains(Compact(layout.Marker)) && text.Contains(Compact(layout.Title))) { result.LayoutId = layout.Id; result.Category = layout.Category; break; }
            }
            result.CompletedLevel = 1;
        }
        if (result.LayoutId == null) { result.DecodedPageCount = source.DecodedPageCount; return result; }
        if (level >= ImportLevel.Identify && result.CompletedLevel < 2)
        {
            var p = source.ReadPage(0, token); result.PageCount = source.PageCount;
            result.Identifiers["orientation"] = p.WidthPt > p.HeightPt ? "landscape" : "portrait";
            var frame = Calibrate(p); frames[0] = frame;
            foreach (var f in layout.Header) result.Identifiers[f.Key] = Read(p.Text, frame, f.Value);
            result.Identifiers["cin"] = null; // No printed CIN locator in this GMX. Never substitute the filename.
            Issue("identifierUnavailable", "This layout does not define a printed CIN locator.", 1, "info");
            var from = Period(result.Identifiers["periodFrom"]); var to = Period(result.Identifiers["periodTo"]);
            result.Identifiers["fiscalYear"] = from.HasValue && to.HasValue && from.Value.year == to.Value.year ? from.Value.year.ToString(CultureInfo.InvariantCulture) : null;
            result.CompletedLevel = 2;
        }
        if (level >= ImportLevel.Extract && result.CompletedLevel < 3)
        {
            // Build transactionally so cancellation can be retried without duplicate rows.
            var rows = new List<SourceRow>();
            for (int i = 0; i < source.PageCount; i++)
            {
                token.ThrowIfCancellationRequested(); var p = source.ReadPage(i, token);
                if (!frames.TryGetValue(i, out var frame)) frames[i] = frame = Calibrate(p);
                if (!Compact(string.Concat(p.Text.Select(t=>t.Text))).Contains(Compact(layout.Marker)))
                    Issue("pageLayoutMismatch", "Expected layout marker is missing.", i+1);
                var header = new SourceRow { Page = i+1, Kind = "PageLevel" };
                foreach (var f in layout.Header) header.Fields[f.Key] = Read(p.Text, frame, f.Value);
                rows.Add(header); Extract(p, frame, rows, token);
            }
            result.Rows = rows; result.CompletedLevel = 3;
        }
        if (level >= ImportLevel.Validate && result.CompletedLevel < 4) { Validate(); result.CompletedLevel = 4; }
        result.DecodedPageCount = source.DecodedPageCount;
        result.Status = result.Issues.Any(i=>i.Severity == "error") ? "invalid" : "completed";
        return result;
    }
    private void Issue(string code, string message, int? page = null, string severity = "error")
    {
        if (!result.Issues.Any(i=>i.Code==code && i.Page==page && i.Message==message)) result.Issues.Add(new ImportIssue { Code=code, Message=message, Page=page, Severity=severity });
    }
    private readonly struct Frame
    {
        public Frame(double unit, double x, double y) { Unit=unit; X=x; Y=y; }
        public double Unit { get; } public double X { get; } public double Y { get; }
        public double ToX(double v)=>X+v*Unit;
        public double ToY(double v)=>Y-v*Unit;
    }
    private Frame Calibrate(PositionedPage p)
    {
        var r=layout.HeaderFrame; double nominal=72/layout.UnitsPerInch;
        var lines=p.Lines.Where(l=>Math.Abs(l.Start.Y-l.End.Y)<.3 && Math.Abs(l.Start.X-l.End.X)>p.WidthPt*.55 && Math.Max(l.Start.Y,l.End.Y)>p.HeightPt*.7).OrderByDescending(l=>l.Start.Y);
        foreach(var line in lines)
        {
            double unit=Math.Abs(line.Start.X-line.End.X)/(r[2]-r[0]);
            if(unit/nominal<.65 || unit/nominal>1.4) continue;
            var f=new Frame(unit,Math.Min(line.Start.X,line.End.X)-r[0]*unit,line.Start.Y+r[1]*unit);
            // Require the lower header border as a second independent vertical anchor.
            if(p.Lines.Any(l=>Math.Abs(l.Start.Y-f.ToY(r[3]))<1 && Math.Abs(l.End.Y-f.ToY(r[3]))<1 && Math.Abs(l.Start.X-l.End.X)>(r[2]-r[0])*unit*.95)) return f;
        }
        throw new InvalidDataException("Header frame could not be calibrated on page "+(p.PhysicalPageIndex+1));
    }
    private static string Compact(string s)=>string.Concat(s.Where(c=>!char.IsWhiteSpace(c))).ToUpperInvariant();
    private static IEnumerable<PositionedText[]> Bands(IEnumerable<PositionedText> text)
    {
        var band=new List<PositionedText>(); double baseline=0;
        foreach(var t in text.OrderByDescending(t=>t.Baseline.Y))
        {
            if(band.Count>0 && baseline-t.Baseline.Y>2) { yield return band.OrderBy(t=>t.Baseline.X).ToArray(); band.Clear(); }
            if(band.Count==0) baseline=t.Baseline.Y;
            band.Add(t);
        }
        if(band.Count>0) yield return band.OrderBy(t=>t.Baseline.X).ToArray();
    }
    public static string Assemble(IEnumerable<PositionedText> text)
    {
        var output=new StringBuilder(); PositionedText? previous=null;
        foreach(var t in text.OrderBy(t=>t.Baseline.X))
        {
            // A run already supplies its spaces. For different runs use advance positions, never ink-box gaps.
            if(previous!=null && output.Length>0 && previous.RunId!=t.RunId && t.Baseline.X-previous.AdvanceEnd.X>Math.Max(.8,t.FontSize*.25)
                && !char.IsWhiteSpace(output[output.Length-1]) && !string.IsNullOrWhiteSpace(t.Text)) output.Append(' ');
            output.Append(t.Text); previous=t;
        }
        return output.ToString().Trim();
    }
    private static PositionedText[] InX(IEnumerable<PositionedText> ts, Frame f, double left, double right)=>ts.Where(t=>t.Baseline.X>=f.ToX(left)-.25 && t.Baseline.X<f.ToX(right)+.25).ToArray();
    private static string Read(IEnumerable<PositionedText> ts, Frame f, double[] r)=>Assemble(InX(ts.Where(t=>t.Baseline.Y<=f.ToY(r[1])+.5 && t.Baseline.Y>=f.ToY(r[3])-.5),f,r[0],r[2]));
    private bool Style(IEnumerable<PositionedText> text, string style, Frame f)
    {
        var ts=text.Where(t=>!string.IsNullOrWhiteSpace(t.Text)).ToArray(); if(ts.Length==0)return false;
        var s=layout.Fonts[style]; double size=s.Size*f.Unit/(72/layout.UnitsPerInch);
        return ts.All(t=>t.FontWeight > 0 && (t.FontWeight >= 600) == (s.Weight >= 600) && Math.Abs(t.FontSize-size)<.65);
    }
    private static bool MatchesLabel(string text, string label)
    {
        var actual=Compact(text); var expected=Compact(label);
        // Asterisks inside an account identifier are literal account characters, not footer labels.
        return expected.All(c=>c=='*') ? actual==expected : actual.Contains(expected);
    }
    private void Extract(PositionedPage page, Frame f, List<SourceRow> rows, CancellationToken token)
    {
        bool footer=false;
        foreach(var band in Bands(page.Text.Where(t=>t.Baseline.Y<f.ToY(layout.HeaderFrame[3])-.5)))
        {
            token.ThrowIfCancellationRequested();
            if(footer) continue;
            var candidates=layout.Rows.Where(d=>d.Labels.Length>0
                ? d.Labels.Any(label=>MatchesLabel(Assemble(InX(band,f,d.LabelRange[0],d.LabelRange[1])),label))
                : Style(InX(band,f,d.Account[0],d.Account[2]),d.AccountFont,f)).ToArray();
            // Fixed placeholders take precedence over a subtotal's overlapping account rectangle.
            var definition=candidates.FirstOrDefault(d=>d.Labels.Length>0) ?? (candidates.Length==1?candidates[0]:null);
            if(definition==null)
            {
                if(band.Any(t=>!string.IsNullOrWhiteSpace(t.Text))) rows.Add(new SourceRow { Page=page.PhysicalPageIndex+1, BaselinePt=band.Average(t=>t.Baseline.Y), Kind="Unresolved", Fields=new Dictionary<string,string>{{"text",Assemble(band)}} });
                continue;
            }
            var row=new SourceRow { Page=page.PhysicalPageIndex+1, BaselinePt=band.Average(t=>t.Baseline.Y), Kind=definition.Kind };
            if(definition.Account.Length>0) row.Account=Assemble(InX(band,f,definition.Account[0],definition.Account[2]));
            if(definition.Name.Length>0) row.Name=Assemble(InX(band,f,definition.Name[0],definition.Name[2]));
            foreach(var field in definition.ItemFields) row.Fields[field.Key]=Assemble(InX(band,f,field.Value[0],field.Value[2]));
            if(definition.ItemKind.Length>0 && row.Fields.Values.Any(v=>v.Length>0))row.Kind=definition.ItemKind;
            foreach(var col in layout.AmountColumns) row.Fields[col.Key]=Assemble(InX(band,f,col.Value[0],col.Value[1]));
            if (band.Any(t=>t.IsClipped && !string.IsNullOrWhiteSpace(t.Text)))
                Issue("clippedText", "The source clips some glyph outlines; their explicit Unicode text is retained.", row.Page, "warning");
            rows.Add(row);
            if(row.Kind=="ReportLevel")
            {
                footer=true;
                // Locate footer fields by their offset from the printed total row baseline.
                var total=band.Where(t=>t.Baseline.X>=f.ToX(layout.AmountColumns.First().Value[0]) && !string.IsNullOrWhiteSpace(t.Text)).ToArray();
                double baseline=(total.Length>0?total:band).Average(t=>t.Baseline.Y);
                var local=new Frame(f.Unit,f.X,baseline+(definition.AmountY[1]-7)*f.Unit);
                foreach(var field in layout.ReportFooter) row.Fields[field.Key]=Read(page.Text,local,field.Value);
            }
        }
    }
    private (int period,int year)? Period(string? raw)
    {
        var m=Regex.Match(raw??"",@"^(\d{2})/(\d{4})$");
        if(!m.Success)return null;
        int p=int.Parse(m.Groups[1].Value),y=int.Parse(m.Groups[2].Value);
        return p<=layout.Formats.MaximumPeriod && y>=1900 && y<=9999 ? (p,y) : ((int,int)?)null;
    }
    public static bool TryAmount(string raw,out decimal value)
    {
        string s=Regex.Replace(raw,@"[\s\u00a0]","");
        if(s.EndsWith(",-",StringComparison.Ordinal))s=s.Substring(0,s.Length-1)+"00";
        if(!Regex.IsMatch(s,@"^[+-]?(?:\d+|\d{1,3}(?:\.\d{3})+),\d{2}$")) { value=0;return false; }
        return decimal.TryParse(s.Replace(".","").Replace(',','.'),NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out value);
    }
    private void Validate()
    {
        if(!source.Document.CompleteDecode)Issue("decodeIncomplete","The adapter reported unsupported source features.");
        if (string.IsNullOrWhiteSpace(result.Identifiers["entityName"])) Issue("missingEntity", "Printed entity name is missing.");
        if (result.Identifiers["fiscalYear"] == null) Issue("unresolvedYear", "A single fiscal year could not be established.");
        foreach(var row in result.Rows)
        {
            if(row.Kind=="PageLevel")
            {
                if(row.Fields["entityName"]!=result.Identifiers["entityName"]) Issue("inconsistentEntity", "Printed entity differs between pages.", row.Page);
                foreach(var key in new[]{"periodFrom","periodTo"})
                    if(!Period(row.Fields[key]).HasValue)Issue("invalidPeriod",key+": "+row.Fields[key],row.Page);
                    else if(row.Fields[key]!=result.Identifiers[key])Issue("inconsistentPeriod",key+" differs between pages.",row.Page);
                if(!int.TryParse(row.Fields["pageNumber"].TrimStart(':',' '),out var p) || p!=row.Page)Issue("pageSequence","Printed page number differs from physical sequence.",row.Page);
                continue;
            }
            if(row.Kind=="Unresolved") { Issue("unresolvedRow",row.Fields["text"],row.Page);continue; }
            if(row.Kind!="ReportLevel" && row.Account.Length==0)Issue("invalidAccount","Missing account: "+row.Account,row.Page);
            foreach(var col in layout.AmountColumns.Keys)
            {
                string raw=row.Fields[col];
                if(raw.Length==0)row.Amounts[col]=0; // Explicit layout suppression policy; raw blank is retained.
                else if(TryAmount(raw,out var amount))row.Amounts[col]=amount;
                else { row.Amounts[col]=null;Issue("invalidAmount",col+": "+raw,row.Page); }
            }
        }
    }
}
