using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PdfLayoutEngine.V2;
internal static class J
{
    public static string S(this JsonElement e, string name) => e.GetProperty(name).GetString() ?? "";
    public static double N(this JsonElement e, string name) => e.GetProperty(name).GetDouble();
    public static bool B(this JsonElement e, string name) => e.GetProperty(name).GetBoolean();
    public static JsonElement[] A(this JsonElement e, string name) => e.GetProperty(name).EnumerateArray().ToArray();
    public static string[] Strings(this JsonElement e, string name) => e.A(name).Select(x => x.GetString()!).ToArray();
    public static bool Has(this JsonElement e, string name) => e.TryGetProperty(name, out _);
    public static RectPt Rect(this JsonElement e) { var a = e.EnumerateArray().Select(x => x.GetDouble()).ToArray(); return new RectPt(a[0], a[1], a[2], a[3]); }
    public static PointPt Point(this JsonElement e) { var a = e.EnumerateArray().Select(x => x.GetDouble()).ToArray(); return new PointPt(a[0], a[1]); }
    public static IEnumerable<JsonElement> Walk(JsonElement e)
    {
        yield return e;
        if (e.ValueKind == JsonValueKind.Object) foreach (var p in e.EnumerateObject()) foreach (var c in Walk(p.Value)) yield return c;
        if (e.ValueKind == JsonValueKind.Array) foreach (var p in e.EnumerateArray()) foreach (var c in Walk(p)) yield return c;
    }
}
