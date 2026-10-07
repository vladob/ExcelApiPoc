using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace PdfLayoutEngine.Simple;
public sealed class LayoutDefinition
{
    public int Version { get; set; }
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public string Marker { get; set; } = "";
    public string Title { get; set; } = "";
    public double UnitsPerInch { get; set; }
    public string Origin { get; set; } = "";
    public double[] HeaderFrame { get; set; } = Array.Empty<double>();
    public Dictionary<string, FontStyle> Fonts { get; set; } = new Dictionary<string, FontStyle>();
    public Dictionary<string, double[]> Header { get; set; } = new Dictionary<string, double[]>();
    public Dictionary<string, double[]> AmountColumns { get; set; } = new Dictionary<string, double[]>();
    public List<RowDefinition> Rows { get; set; } = new List<RowDefinition>();
    public Dictionary<string, double[]> ReportFooter { get; set; } = new Dictionary<string, double[]>();
    public FieldFormats Formats { get; set; } = new FieldFormats();
    public static LayoutDefinition Load(string path)
    {
        var d = JsonSerializer.Deserialize<LayoutDefinition>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true, UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow }) ?? throw new InvalidDataException("Empty layout.");
        d.Validate(); return d;
    }
    public void Validate()
    {
        if (Version != 1 || Origin != "topLeft" || UnitsPerInch <= 0 || string.IsNullOrWhiteSpace(Marker) || Rows.Count == 0) throw new InvalidDataException("Invalid layout identity/coordinates.");
        foreach (var r in Header.Values.Concat(ReportFooter.Values).Concat(new[] { HeaderFrame }).Concat(Rows.SelectMany(r => r.ItemFields.Values.Concat(new[] { r.Account, r.Name }).Where(x => x.Length > 0))))
            if (r.Length != 4 || r.Any(x => double.IsNaN(x) || double.IsInfinity(x)) || r[2] <= r[0] || r[3] <= r[1]) throw new InvalidDataException("Invalid field rectangle.");
        foreach (var r in AmountColumns.Values.Concat(Rows.Select(r=>r.AmountY)))
            if (r.Length != 2 || r[1] <= r[0]) throw new InvalidDataException("Invalid amount range.");
        foreach (var r in Rows)
            if (!Fonts.ContainsKey(r.AmountFont) || r.Account.Length > 0 && !Fonts.ContainsKey(r.AccountFont)) throw new InvalidDataException("Unknown font style.");
        if (Formats.Amount != "decimalComma" || Formats.BlankAmount != "zero" || Formats.Period != "MM/yyyy") throw new InvalidDataException("Unsupported field format.");
    }
}
public sealed class FontStyle { public string Family { get; set; } = ""; public double Size { get; set; } public int Weight { get; set; } }
public sealed class FieldFormats { public string Amount { get; set; } = ""; public string BlankAmount { get; set; } = ""; public string Period { get; set; } = ""; public int MaximumPeriod { get; set; } }
public sealed class RowDefinition
{
    public string Kind { get; set; } = "";
    public string Section { get; set; } = "";
    public double AnchorY { get; set; }
    public string[] Labels { get; set; } = Array.Empty<string>();
    public double[] LabelRange { get; set; } = Array.Empty<double>();
    public double[] Account { get; set; } = Array.Empty<double>();
    public double[] Name { get; set; } = Array.Empty<double>();
    public string AccountFont { get; set; } = "";
    public string AmountFont { get; set; } = "";
    public double[] AmountY { get; set; } = Array.Empty<double>();
    public string ItemKind { get; set; } = "";
    public Dictionary<string, double[]> ItemFields { get; set; } = new Dictionary<string, double[]>();
}
