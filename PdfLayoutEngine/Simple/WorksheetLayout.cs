using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace PdfLayoutEngine.Simple;
// A worksheet printed down each horizontal panel before continuing to the next panel.
public sealed class WorksheetLayout
{
    public bool DynamicHeaders {get;set;}
    public double PageWidth {get;set;}
    public double PageHeight {get;set;}
    public double BodyTop {get;set;}=45;
    public double BodyBottom {get;set;}=40;
    public List<WorksheetPanel> Panels {get;set;}=new List<WorksheetPanel>();
    public void Validate(){
        if(new[]{PageWidth,PageHeight,BodyTop,BodyBottom}.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||PageWidth<=0||PageHeight<=0||BodyTop<0||BodyBottom<0||BodyTop+BodyBottom>=PageHeight||Panels.Count<1||Panels.Count>4)throw new InvalidDataException("Invalid printed worksheet geometry.");
        foreach(var panel in Panels.SelectMany(p=>new[]{p}.Concat(p.Alternatives))){if(panel.Headers.Count<2||panel.Headers.Values.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||panel.Headers.Values.Distinct().Count()<2||panel.Columns.Count==0)throw new InvalidDataException("Printed panel requires header anchors and columns.");foreach(var c in panel.Columns.Values)if(c.Length!=2||c.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||c[1]<=c[0])throw new InvalidDataException("Invalid worksheet column.");}
        if(DynamicHeaders)foreach(var panel in Panels.SelectMany(p=>new[]{p}.Concat(p.Alternatives)))if(panel.HeaderFields.Count!=panel.Columns.Count||panel.Columns.Keys.Any(k=>!panel.HeaderFields.ContainsKey(k))||panel.HeaderFields.Values.Distinct().Count()!=panel.HeaderFields.Count)throw new InvalidDataException("Every dynamic column requires a unique header label.");
        var keys=Panels.SelectMany(p=>p.Columns.Keys).ToArray();if(keys.Distinct().Count()!=keys.Length||new[]{"AccountingPeriod","Account","Date","DebitAmount","CreditAmount"}.Any(k=>!keys.Contains(k)))throw new InvalidDataException("Missing or duplicated worksheet field.");
    }
}
public sealed class WorksheetPanel
{
    public Dictionary<string,string> HeaderFields {get;set;}=new Dictionary<string,string>();
    public List<WorksheetPanel> Alternatives {get;set;}=new List<WorksheetPanel>();
    public Dictionary<string,double> Headers {get;set;}=new Dictionary<string,double>();
    public Dictionary<string,double[]> Columns {get;set;}=new Dictionary<string,double[]>();
}
