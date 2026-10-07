using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace PdfLayoutEngine.Simple;
public sealed class CompactLayout
{
    public int Version { get; set; }
    public string Producer { get; set; } = "IfoSoft";
    public WorksheetLayout? Worksheet { get; set; }
    public TabularLayout? Table { get; set; }
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public double UnitsPerInch { get; set; }
    public Dictionary<string,double[]> Header { get; set; } = new Dictionary<string,double[]>();
    public List<CompactAnchor> Anchors { get; set; } = new List<CompactAnchor>();
    public List<CompactSection> Sections { get; set; } = new List<CompactSection>();
    public string[] RequiredText { get; set; } = Array.Empty<string>();
    public string[] ImportKinds { get; set; } = Array.Empty<string>();
    public static CompactLayout Load(string path)
    {
        var d=JsonSerializer.Deserialize<CompactLayout>(File.ReadAllText(path),new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow}) ?? throw new InvalidDataException(path);
        if(d.Version==2&&d.Worksheet!=null){if(d.Table!=null||d.Category!="AJ")throw new InvalidDataException("Invalid worksheet category.");d.Worksheet.Validate();return d;}
        if(d.Version==2 && d.Table!=null){d.Table.Validate();if(!new[]{"AJ","GL","AF"}.Contains(d.Category))throw new InvalidDataException("Invalid category");return d;}
        if(d.Version!=1 || d.UnitsPerInch<=0 || d.Anchors.Count<2 || d.Sections.Count==0 || !new[]{"GL","AJ","AF"}.Contains(d.Category))throw new InvalidDataException("Invalid compact layout: "+path);
        foreach(var r in d.Header.Values.Concat(d.Anchors.Select(a=>a.Rect)).Concat(d.Sections.SelectMany(s=>s.Fields.Values.Concat(new[]{s.Anchor.Rect}))))
            if(r.Length!=4 || r.Any(v=>double.IsNaN(v)||double.IsInfinity(v)) || r[2]<=r[0] || r[3]<=r[1])throw new InvalidDataException("Invalid rectangle: "+path);
        foreach(var s in d.Sections)if(s.Amounts.Any(k=>!s.Fields.ContainsKey(k)))throw new InvalidDataException("Unknown amount field.");
        return d;
    }
}
public sealed class CompactAnchor
{
    public double[] Rect { get; set; } = Array.Empty<double>();
    public string Text { get; set; } = "";
    public double Size { get; set; }
    public int? Weight { get; set; }
    public bool Numeric { get; set; } = true;
}
public sealed class CompactSection
{
    public string Kind { get; set; } = "";
    public string SourceSection { get; set; } = "";
    public CompactAnchor Anchor { get; set; } = new CompactAnchor();
    public Dictionary<string,double[]> Fields { get; set; } = new Dictionary<string,double[]>();
    public string[] Amounts { get; set; } = Array.Empty<string>();
    public string[] ItemFields { get; set; } = Array.Empty<string>();
}
