using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace PdfLayoutEngine.V2;

/// <summary>Immutable JSON snapshots. Load once and reuse for multiple documents.</summary>
public sealed class DefinitionCatalog
{
    private readonly Dictionary<string, JsonElement> definitions = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
    private readonly Dictionary<string, JsonSchema> schemas = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
    private readonly EvaluationOptions options = new EvaluationOptions { OutputFormat = OutputFormat.List };
    public JsonElement Catalog { get; private set; }
    public IEnumerable<JsonElement> Layouts => definitions.Values.Where(x => x.S("kind") == "layout");
    public IEnumerable<JsonElement> Identification => definitions.Values.Where(x => x.S("kind") == "identification");
    public JsonElement Resolve(string id) => definitions.TryGetValue(id, out var value) ? value : throw new InvalidDataException("Unknown definition: " + id);
    public JsonElement Resolve(JsonElement reference)
    {
        var result = Resolve(reference.S("id"));
        if (result.S("definitionVersion") != reference.S("version")) throw new InvalidDataException("Definition version mismatch: " + reference.S("id"));
        return result;
    }
    public static string Hash(byte[] data)
    {
        using var sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(data).Select(x => x.ToString("x2")));
    }
    public static string DefinitionHash(string text) => Hash(Encoding.UTF8.GetBytes(text.Replace("\r\n", "\n")));
    private static JsonElement Read(string path)
    {
        using var d = JsonDocument.Parse(File.ReadAllText(path, new UTF8Encoding(false, true)), new JsonDocumentOptions { MaxDepth = 128 });
        foreach (var e in J.Walk(d.RootElement).Where(x => x.ValueKind == JsonValueKind.Object))
        {
            var properties = e.EnumerateObject().Select(p => p.Name).ToArray();
            if (properties.Distinct(StringComparer.Ordinal).Count() != properties.Length) throw new InvalidDataException("Duplicate JSON property: " + path);
        }
        return d.RootElement.Clone();
    }
    public static DefinitionCatalog Load(string directory, bool allowReviewDrafts = false)
    {
        var c = new DefinitionCatalog(); var root = Path.GetFullPath(directory);
        foreach (var path in Directory.GetFiles(Path.Combine(root, "schemas"), "*.schema.json"))
        {
            var schema = JsonSchema.FromText(File.ReadAllText(path, Encoding.UTF8));
            c.options.SchemaRegistry.Register(schema);
            c.schemas.Add(Path.GetFileName(path).Replace(".schema.json", ""), schema);
        }
        c.Catalog = Read(Path.Combine(root, "catalog.json"));
        c.Validate("catalog", c.Catalog);
        foreach (var item in c.Catalog.A("definitions"))
        {
            var relative = item.S("path").Replace('/', Path.DirectorySeparatorChar);
            var path = Path.GetFullPath(Path.Combine(root, relative));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal) || Path.IsPathRooted(relative)) throw new InvalidDataException("Catalog path escapes root.");
            var text = File.ReadAllText(path, new UTF8Encoding(false, true));
            if (DefinitionHash(text) != item.S("sha256")) throw new InvalidDataException("Definition hash mismatch: " + relative);
            var d = Read(path); c.Validate(item.S("kind"), d);
            if (d.S("kind") != item.S("kind")) throw new InvalidDataException("Catalog kind mismatch.");
            if (!allowReviewDrafts && (!d.B("productionApproved") || d.S("status") != "approved")) throw new InvalidDataException("Review definition requires explicit opt-in: " + d.S("id"));
            if (c.definitions.ContainsKey(d.S("id"))) throw new InvalidDataException("Duplicate definition ID.");
            c.definitions.Add(d.S("id"), d);
        }
        foreach (var i in c.Catalog.A("definitions")) c.Resolve(i.GetProperty("ref"));
        c.CheckReferences();
        return c;
    }
    public void Validate(string kind, JsonElement value)
    {
        if (!schemas.TryGetValue(kind, out var schema)) throw new InvalidDataException("Unknown definition kind: " + kind);
        var result = schema.Evaluate(JsonNode.Parse(value.GetRawText()), options);
        if (!result.IsValid) throw new InvalidDataException("JSON Schema validation failed (" + kind + "): " + JsonSerializer.Serialize(result));
    }
    private static void Require(bool value, string reason) { if (!value) throw new InvalidDataException(reason); }
    private static void Unique(IEnumerable<JsonElement> list, string scope)
    { var ids = list.Select(x => x.S("id")).ToArray(); Require(ids.Distinct().Count() == ids.Length, "Duplicate IDs in " + scope); }
    private void CheckReferences()
    {
        foreach (var d in definitions.Values.Append(Catalog))
        {
            Require(d.Strings("requiresCapabilities").All(x => x == "positioned.v2"), "Unsupported capability in " + d.S("id"));
            foreach (var x in J.Walk(d).Where(x => x.ValueKind == JsonValueKind.Object))
            {
                if (x.Has("id") && x.Has("version") && x.EnumerateObject().Count() == 2) Resolve(x);
                foreach (var property in new[] { "rectPt", "offsetRectPt", "regionFraction" })
                    if (x.Has(property)) { var r = x.GetProperty(property).Rect(); Require(r.Width > 0 && r.Height > 0, "Degenerate rectangle."); }
            }
            if (d.S("kind") == "parsers") Unique(d.A("profiles"), d.S("id"));
            if (d.S("kind") == "model") Unique(d.A("fields"), d.S("id"));
            if (d.S("kind") == "layout") CheckLayout(d);
            if (d.S("kind") == "validation") CheckValidation(d);
            if (d.S("kind") == "identification")
            {
                Unique(d.A("signals"), "signals"); Unique(d.A("evidenceGroups"), "evidenceGroups");
                var ids = d.A("signals").Select(s => s.S("id")).ToArray();
                foreach (var decision in d.A("decisions")) Require(decision.Strings("requiredSignals").Concat(decision.Strings("vetoSignals")).All(ids.Contains), "Unknown decision signal.");
                foreach (var signal in d.A("signals"))
                {
                    Require(d.A("evidenceGroups").Any(g => g.S("id") == signal.S("evidenceGroup")), "Unknown evidence group.");
                    if (signal.S("op") == "geometryFit")
                    {
                        var layout = Resolve(signal.GetProperty("layoutRef"));
                        Require(signal.Strings("variantRefs").All(id => layout.A("variants").Any(v => v.S("id") == id)), "Unknown geometry variant.");
                    }
                }
            }
        }
    }
    private void CheckLayout(JsonElement d)
    {
        var model = Resolve(d.GetProperty("modelRef")); Require(model.S("kind") == "model", "Invalid model reference.");
        var fields = model.A("fields").Select(x => x.S("id")).ToArray();
        var parsers = d.A("parserRefs").SelectMany(r => Resolve(r).A("profiles")).Select(x => x.S("id")).ToArray();
        foreach (var key in new[] { "variants", "metadata", "metadataDerivations", "classification", "groups", "completeness" }) Unique(d.A(key), key);
        var blocks = d.A("variants").SelectMany(v => v.A("blocks")).Select(b => b.S("id")).Distinct().ToArray();
        foreach (var v in d.A("variants"))
        {
            Unique(v.A("blocks"), "blocks"); Unique(v.A("anchors"), "anchors");
            foreach (var b in v.A("blocks"))
            {
                foreach (var k in new[] { "fields", "lines", "labels" }) Unique(b.A(k), k);
                foreach (var f in b.A("fields")) Require(fields.Contains(f.S("semanticName")) && parsers.Contains(f.S("parserRef")), "Unknown field/parser.");
            }
            foreach (var a in v.A("anchors"))
            {
                var b = v.A("blocks").SingleOrDefault(x => x.S("id") == a.S("blockRef"));
                Require(b.ValueKind != JsonValueKind.Undefined, "Unknown anchor block.");
                var line = b.A("lines").SingleOrDefault(x => x.S("id") == a.S("lineRef"));
                Require(line.ValueKind != JsonValueKind.Undefined, "Unknown anchor line.");
                Require(line.A("pointsPt")[(int)a.N("endpointIndex")].Point().Distance(a.GetProperty("pointPt").Point()) < 1e-8, "Anchor differs from ruling endpoint.");
            }
            var cal = v.GetProperty("calibration"); var bounds = cal.A("scaleRange");
            Require(bounds[0].GetDouble() < bounds[1].GetDouble(), "Invalid scale range.");
            Require(cal.Strings("anchorRefs").All(id => v.A("anchors").Any(a => a.S("id") == id)), "Unknown calibration anchor.");
            Require(cal.N("minimumInliers") <= cal.Strings("anchorRefs").Distinct().Count(), "Insufficient anchors.");
        }
        foreach (var metadata in d.A("metadata"))
        {
            Require(parsers.Contains(metadata.S("parserRef")), "Unknown metadata parser.");
            var locator = metadata.GetProperty("locator");
            if (locator.Has("variantRef"))
            {
                var variant = d.A("variants").Single(v => v.S("id") == locator.S("variantRef"));
                if (locator.S("kind") == "blockField") Require(variant.A("blocks").Any(b => b.S("id") == locator.S("blockRef") && b.A("fields").Any(f => f.S("id") == locator.S("fieldRef"))), "Unknown metadata field.");
                if (locator.S("kind") == "relativeToAnchor") Require(variant.A("anchors").Any(a => a.S("id") == locator.S("anchorRef")), "Unknown metadata anchor.");
            }
        }
        foreach (var part in new[] { "classification", "groups", "completeness" })
        foreach (var x in J.Walk(d.GetProperty(part)).Where(x => x.ValueKind == JsonValueKind.Object))
        {
            if (x.Has("blockRefs")) Require(x.Strings("blockRefs").All(blocks.Contains), "Unknown block predicate.");
            if (x.Has("fieldRef")) Require(fields.Contains(x.S("fieldRef")), "Unknown semantic field.");
            foreach (var key in new[] { "fieldRefs", "keyFields" }) if (x.Has(key)) Require(x.Strings(key).All(fields.Contains), "Unknown required/key field.");
        }
        var groups = d.A("groups").ToDictionary(x => x.S("id"));
        foreach (var group in groups.Values)
        {
            var seen = new HashSet<string>(); var current = group;
            while (true)
            {
                Require(seen.Add(current.S("id")), "Group cycle.");
                var parent = current.GetProperty("parentRef"); if (parent.ValueKind == JsonValueKind.Null) break;
                Require(groups.ContainsKey(parent.GetString()!), "Unknown parent group."); current = groups[parent.GetString()!];
            }
        }
    }
    private void CheckValidation(JsonElement d)
    {
        var layout = Resolve(d.GetProperty("layoutRef")); Require(layout.S("kind") == "layout", "Invalid validation layout.");
        var fieldNames = Resolve(layout.GetProperty("modelRef")).A("fields").Select(x => x.S("id")).ToArray();
        foreach (var key in new[] { "calculations", "printedControls", "rules" }) Unique(d.A(key), key);
        var calcs = d.A("calculations").ToDictionary(x => x.S("id"));
        Action<string, HashSet<string>> visit = null!;
        visit = (id, chain) =>
        {
            Require(calcs.ContainsKey(id), "Unknown calculation."); Require(chain.Add(id), "Calculation cycle.");
            foreach (var input in calcs[id].A("inputRefs"))
                if (input.S("kind") == "calculation") visit(input.S("ruleRef"), new HashSet<string>(chain));
                else if (input.S("kind") == "field") Require(fieldNames.Contains(input.S("semanticName")), "Unknown calculation field.");
        };
        foreach (var id in calcs.Keys) visit(id, new HashSet<string>());
        foreach (var c in d.A("printedControls")) Require(layout.A("variants").Any(v => v.S("id") == c.S("variantRef") && v.A("blocks").Any(b => b.S("id") == c.S("blockRef") && b.A("fields").Any(f => f.S("id") == c.S("fieldRef")))), "Unknown printed control.");
        foreach (var r in d.A("rules").Where(x => x.S("op") == "compare"))
        {
            Require(calcs.ContainsKey(r.S("calculatedRef")), "Unknown comparison calculation.");
            if (r.Has("otherCalculatedRef")) Require(calcs.ContainsKey(r.S("otherCalculatedRef")), "Unknown comparison calculation.");
            if (r.Has("printedControlRef")) Require(d.A("printedControls").Any(c => c.S("id") == r.S("printedControlRef")), "Unknown comparison control.");
        }
    }
}
