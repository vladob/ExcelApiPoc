using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;

public sealed class UnifiedDiscoveryTests
{
    static string Root => Path.Combine(AppContext.BaseDirectory, "AllLayouts");
    static string Save(string contents, string extension)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        File.WriteAllText(path, contents, Encoding.GetEncoding(1250));
        return path;
    }
    [Fact] public void Root_discovers_all_five_producers_and_excludes_legacy_definitions()
    {
        Assert.True(LayoutFiles.Validate(Root) > 40);
        var producers = LayoutFiles.Family(Root, "").Select(CompactLayout.Load).Select(d => d.Producer).Distinct().ToArray();
        Assert.Equal(5, producers.Length);
        Assert.Contains("Urbis", producers);
        Assert.All(LayoutFiles.All(Root), p => Assert.Equal("Compact", Path.GetFileName(Path.GetDirectoryName(p))));
    }
    [Theory, MemberData(nameof(CompactTests.Definitions), MemberType = typeof(CompactTests))]
    public void IfoSoft_pdf_headers_still_recognize_among_all_producers(string file)
    {
        var d = CompactLayout.Load(Path.Combine(AppContext.BaseDirectory, "Compact", file));
        using var source = new CompactTests.Fixture(d);
        using var session = new CompactSession(source, LayoutFiles.Family(Root, "").Select(CompactLayout.Load));
        var result = session.Examine(ImportLevel.Identify);
        Assert.Equal(d.Id, result.LayoutId);
        Assert.Equal("IfoSoft", result.Producer);
        Assert.Empty(result.Rows);
    }
    [Fact] public void Equal_pdf_matches_report_candidates_without_selecting_one()
    {
        var d = CompactLayout.Load(Path.Combine(AppContext.BaseDirectory, "Compact", "hlknia4.json"));
        var other = CompactLayout.Load(Path.Combine(AppContext.BaseDirectory, "Compact", "hlknia4.json"));
        other.Id = "competing"; other.Producer = "Other";
        using var source = new CompactTests.Fixture(d);
        using var session = new CompactSession(source, new[] { d, other });
        var result = session.Examine(ImportLevel.Normalize);
        Assert.Equal("ambiguous", result.Status); Assert.Null(result.LayoutId); Assert.Empty(result.Rows);
        Assert.Equal(2, result.CandidateLayouts.Count);
    }
    [Fact] public void IfoSoft_xml_is_not_routed_to_Crystal_when_catalogues_are_combined()
    {
        var p = Save("<uctovny_vykaz><typDoc>UCT_VETA</typDoc><obdobie><rok>2025</rok></obdobie><vety><veta><ucSuv>211</ucSuv><ucPripDat>01.01.2025</ucPripDat><rok>2025</rok><mes>01</mes><md>1,00</md><dal>1,00</dal></veta></vety><sucetKontrola>3,00</sucetKontrola></uctovny_vykaz>", ".xml");
        try { using var i = new CompactLayoutImporter(p, Root); var r = i.Examine(ImportLevel.Identify); Assert.Equal("ifosoft-xml-aj", r.LayoutId); Assert.Equal("IfoSoft", r.Producer); Assert.Empty(r.Rows); }
        finally { File.Delete(p); }
    }
    [Fact] public void Crystal_xml_uses_source_fields_with_all_catalogues_loaded()
    {
        var definition = JsonSerializer.Deserialize<CrystalDefinition>(File.ReadAllText(Path.Combine(Root, "Ives", "Compact", "crystal-aj.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        XNamespace ns = definition.Namespace;
        var xml = new XElement(ns + "CrystalReport", new XElement(ns + "Section", definition.Fields.Values.Select(v => new XElement(ns + "Field", new XAttribute("FieldName", v), new XElement(ns + "Value", "1")))));
        var p = Save(xml.ToString(), ".xml");
        try { using var i = new CompactLayoutImporter(p, Root); var r = i.Examine(ImportLevel.Recognize); Assert.Equal("ives-xml-aj", r.LayoutId); Assert.Equal("Ives", r.Producer); }
        finally { File.Delete(p); }
    }
    [Theory, InlineData("IfoSoft"), InlineData("Ives")]
    public void Identical_IfoSoft_and_Ives_csv_signatures_are_reported_as_ambiguous(string producer)
    {
        var layout = Path.Combine(Root, producer, "Compact", "structured-csv-aj.json");
        using var json = JsonDocument.Parse(File.ReadAllText(layout));
        var headers = json.RootElement.GetProperty("headers").EnumerateArray().Select(v => v.GetString());
        var p = Save("Test entity\nUctovny dennik\n" + string.Join(";", headers) + "\n", ".csv");
        try { using var i = new CompactLayoutImporter(p, Root); var ex = Assert.Throws<AmbiguousLayoutException>(() => i.Examine(ImportLevel.Recognize)); Assert.Contains("ifosoft-csv-aj", ex.Candidates); Assert.Contains("ives-csv-aj", ex.Candidates); Assert.Null(i.Canonical); }
        finally { File.Delete(p); }
    }
    [Theory, InlineData("Urbis"), InlineData("Omega"), InlineData("SoftipMop")]
    public void Excel_headers_distinguish_producers(string producer)
    {
        string[][] rows = producer == "Urbis" ? new[] { new[] { "Doklad", "Dátum", "Poznámka", "Suma", "Účet MD", "Účet Dal", "" } }
            : producer == "Omega" ? new[] { new[] { "Stredisko", "Účt. obd.", "DUÚP", "Dátum splatnosti", "Interné číslo", "Externé číslo", "Partner", "Text", "Suma MD [EUR]", "Suma DAL [EUR]", "MD synt.", "MD anal.", "DAL synt.", "DAL anal." } }
            : new[] { new[] { "Účt.mes.", "DD", "Číslo dokladu", "Účet", "Dát.účt.", "Popis zápisu", "St.", "DAL", "Stredisko", "Má dať" } };
        var p = UrbisLayoutTests.Workbook(rows);
        try { using var i = new CompactLayoutImporter(p, Root); var r = i.Examine(ImportLevel.Recognize); Assert.Equal(producer == "Omega" ? "KROS OMEGA" : producer == "SoftipMop" ? "Softip-MOP" : producer, r.Producer); Assert.Empty(r.Rows); Assert.Null(i.Canonical); }
        finally { File.Delete(p); }
    }
    [Fact] public void Unknown_xml_is_unrecognized_not_forced_into_IfoSoft()
    {
        var p = Save("<unknown><value>123</value></unknown>", ".xml");
        try { using var i = new CompactLayoutImporter(p, Root); var r = i.Examine(ImportLevel.Normalize); Assert.Equal("unrecognized", r.Status); Assert.Null(r.Producer); Assert.Null(i.Canonical); }
        finally { File.Delete(p); }
    }
    [Fact] public void Duplicate_ids_fail_catalogue_validation()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(folder);
        try { File.WriteAllText(Path.Combine(folder, "one.json"), "{\"id\":\"duplicate\"}"); File.Copy(Path.Combine(folder, "one.json"), Path.Combine(folder, "two.json")); Assert.Throws<InvalidDataException>(() => LayoutFiles.Validate(folder)); }
        finally { Directory.Delete(folder, true); }
    }
    [Fact] public void Duplicate_csv_signatures_report_ambiguity()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(folder);
        var layout = File.ReadAllText(Path.Combine(Root, "IfoSoft", "Compact", "structured-csv-aj.json"));
        using var json = JsonDocument.Parse(layout);
        var p = Save("Entity\nUctovny dennik\n" + string.Join(";", json.RootElement.GetProperty("headers").EnumerateArray().Select(v => v.GetString())) + "\n", ".csv");
        try {
            File.WriteAllText(Path.Combine(folder, "structured-one.json"), layout);
            File.WriteAllText(Path.Combine(folder, "structured-two.json"), layout.Replace("ifosoft-csv-aj", "competing-csv-aj"));
            using var i = new CompactLayoutImporter(p, folder);
            Assert.Equal(2, Assert.Throws<AmbiguousLayoutException>(() => i.Examine(ImportLevel.Recognize)).Candidates.Length);
        } finally { File.Delete(p); Directory.Delete(folder, true); }
    }
}
