using PdfLayoutEngine.Runner;
using PdfLayoutEngine.V2;
using System.Text.Json;

namespace PdfLayoutEngine.Tests;
public class ManifestRunnerTests
{
    private static string Definitions => Path.Combine(AppContext.BaseDirectory, "TestData", "V2");
    [Fact] public void CsvSupportsQuotedSemicolonsEscapedQuotesAndMultilineFields()
    {
        using var temp = new Temp();
        File.WriteAllText(temp.Manifest, string.Join(';', Manifest.Headers) + "\n\"a;b.pdf\";\"00322792 - O\"\"bec\";HL_KNIHA;\"old\nmessage\";2024\n");
        var row = Assert.Single(Manifest.Read(temp.Manifest)); Assert.Equal(2, row.RowNumber);
        Assert.Equal("a;b.pdf", row.Expected.FilePath); Assert.Equal("00322792 - O\"bec", row.Expected.AccountingEntity); Assert.Equal("old\nmessage", row.Expected.PreviousTestResult);
    }
    [Fact] public void BadYearAndCategoryAreRejectedBeforeProcessing()
    {
        using var temp = new Temp(); temp.Write("a.pdf;;HL_KNIHA;;2024 extra"); Assert.Throws<InvalidDataException>(() => Manifest.Read(temp.Manifest));
        temp.Write("a.pdf;;WRONG;;2024"); Assert.Throws<InvalidDataException>(() => Manifest.Read(temp.Manifest));
    }
    [Fact] public void RootMappingIsBoundaryAwareAndPreservesRelativeDirectories()
    {
        string mapped = Manifest.ResolvePath(@"F:\Audit\TestAll\Entity\a.pdf", "manifest.csv", @"f:\Audit\TestAll", Path.GetTempPath());
        Assert.Equal(Path.Combine(Path.GetTempPath(), "Entity", "a.pdf"), mapped);
        Assert.Throws<InvalidDataException>(() => Manifest.ResolvePath(@"F:\Audit\TestAll2\a.pdf", "manifest.csv", @"F:\Audit\TestAll", Path.GetTempPath()));
        Assert.Throws<InvalidDataException>(() => Manifest.ResolvePath(@"F:\Audit\TestAll\..\a.pdf", "manifest.csv", @"F:\Audit\TestAll", Path.GetTempPath()));
    }
    [Fact] public void MissingActualYearOrCinIsInconclusiveAndExplicitMismatchIsFail()
    {
        var entry = new ManifestEntry(2, new("a.pdf", "00322792 - Example", "HL_KNIHA", "historical", "2024"));
        var engine = FakeResult();
        var result = Results.Evaluate(entry, engine); Assert.Equal("inconclusive", result.ExpectationStatus); Assert.Null(result.Actual.EntityCin); Assert.Null(result.Actual.FiscalYear);
        engine.Metadata["fiscalYear"] = new FieldValue { State = "present", Value = "2025" };
        Assert.Equal("fail", Results.Evaluate(entry, engine).ExpectationStatus);
        Assert.Equal("historical", result.Expected.PreviousTestResult);
    }
    [Fact] public void BatchContinuesAfterMissingFileAndProcessorFailureAndWritesSchemaValidResults()
    {
        using var temp = new Temp(); temp.Write("missing.pdf;;HL_KNIHA;;", "broken.pdf;;HL_KNIHA;;", "good.pdf;;HL_KNIHA;;");
        File.WriteAllText(Path.Combine(temp.Root, "broken.pdf"), "fixture"); File.WriteAllText(Path.Combine(temp.Root, "good.pdf"), "fixture");
        var report = new BatchRunner().Run(temp.Options(), processor: (path, _) => path.EndsWith("broken.pdf") ? throw new InvalidDataException("malformed fixture") : FakeResult());
        Assert.Equal("completed", report.Status); Assert.Equal(3, report.CompletedRowCount); Assert.Equal(2, report.ProcessingCounts["failed"]); Assert.Equal(1, report.ProcessingCounts["success"]);
        var catalog = DefinitionCatalog.Load(Definitions, true);
        var lines = File.ReadAllLines(Path.Combine(report.OutputDirectory, "results.jsonl")); Assert.Equal(3, lines.Length);
        foreach (var line in lines) catalog.Validate("manifest-result", JsonDocument.Parse(line).RootElement);
        Assert.True(File.Exists(Path.Combine(report.OutputDirectory, "files", "row-000004.engine.json")));
        Assert.Equal(4, File.ReadAllLines(Path.Combine(report.OutputDirectory, "summary.csv")).Length);
        Assert.True(File.Exists(Path.Combine(report.OutputDirectory, "manifest.csv")));
    }
    [Fact] public void TimeoutIsPerFileAndCancellationPreservesCompletedRows()
    {
        using var temp = new Temp(); temp.Write("a.pdf;;HL_KNIHA;;", "b.pdf;;HL_KNIHA;;");
        File.WriteAllText(Path.Combine(temp.Root, "a.pdf"), "fixture"); File.WriteAllText(Path.Combine(temp.Root, "b.pdf"), "fixture");
        var report = new BatchRunner().Run(temp.Options(), processor: (path, _) => path.EndsWith("a.pdf") ? throw new OperationCanceledException() : FakeResult());
        Assert.Equal(2, report.CompletedRowCount); Assert.Contains("timeout", File.ReadAllText(Path.Combine(report.OutputDirectory, "files", "row-000002.json")));
        using var cancellation = new CancellationTokenSource();
        report = new BatchRunner().Run(temp.Options(), log: message => { if (message.StartsWith("  success")) cancellation.Cancel(); }, cancellationToken: cancellation.Token, processor: (_, _) => FakeResult());
        Assert.Equal("cancelled", report.Status); Assert.Equal(1, report.CompletedRowCount);
        Assert.Single(File.ReadAllLines(Path.Combine(report.OutputDirectory, "results.jsonl")));
    }
    [Fact] public void RowRangeSelectsSourceLinesAndDuplicateEntriesAreNotDropped()
    {
        using var temp = new Temp(); temp.Write("a.pdf;;HL_KNIHA;;", "a.pdf;;HL_KNIHA;;", "a.pdf;;HL_KNIHA;;"); File.WriteAllText(Path.Combine(temp.Root, "a.pdf"), "fixture");
        var options = temp.Options(); options.FirstRow = 3; options.LastRow = 4;
        var report = new BatchRunner().Run(options, processor: (_, _) => FakeResult()); Assert.Equal(2, report.CompletedRowCount);
        Assert.True(File.Exists(Path.Combine(report.OutputDirectory, "files", "row-000003.json"))); Assert.True(File.Exists(Path.Combine(report.OutputDirectory, "files", "row-000004.json")));
    }
    private static EngineResult FakeResult() => new() { Format = "PDF", ProcessingStatus = "success", Provenance = new() { DocumentHash = "fixture" }, RecognitionDecisions = new() { new() { Target = "category", State = "confirmed", Candidates = new() { new() { Id = "GL", Score = 60 } } }, new() { Target = "producer", State = "confirmed", Candidates = new() { new() { Id = "IfoSoft", Score = 40 } } }, new() { Target = "layout", State = "confirmed", Candidates = new() { new() { Id = "fixture", Score = 70 } } } } };
    private sealed class Temp : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "layout-runner-" + Guid.NewGuid().ToString("N"));
        public string Manifest => Path.Combine(Root, "manifest.csv");
        public Temp() => Directory.CreateDirectory(Root);
        public void Write(params string[] rows) => File.WriteAllText(Manifest, string.Join(';', Runner.Manifest.Headers) + "\n" + string.Join('\n', rows) + "\n");
        public BatchOptions Options() => new() { ManifestPath = Manifest, DefinitionsPath = Definitions, OutputRoot = Path.Combine(Root, "output"), AllowReviewDrafts = true };
        public void Dispose() => Directory.Delete(Root, true);
    }
}
public sealed class ManifestCorpusFactAttribute : FactAttribute
{
    public ManifestCorpusFactAttribute() { if (Environment.GetEnvironmentVariable("IFOSOFT_V2_RUN") != "1") Skip = "Set IFOSOFT_V2_RUN=1 to run the local manifest corpus."; }
}
public class ManifestCorpusTests
{
    [ManifestCorpusFact]
    public void Run_corrected_manifest_and_log_every_file()
    {
        var manifest = Environment.GetEnvironmentVariable("IFOSOFT_V2_MANIFEST");
        if (manifest == null)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "PdfLayoutEngine.Tests", "FilesToTest.csv"))) dir = dir.Parent;
            Assert.NotNull(dir); manifest = Path.Combine(dir.FullName, "PdfLayoutEngine.Tests", "FilesToTest.csv");
        }
        var options = new BatchOptions { ManifestPath = manifest, DefinitionsPath = Path.Combine(AppContext.BaseDirectory, "TestData", "V2"), OutputRoot = Environment.GetEnvironmentVariable("IFOSOFT_V2_OUTPUT") ?? Path.Combine(Path.GetDirectoryName(manifest)!, "TestResults", "LayoutV2"), AllowReviewDrafts = true };
        if (int.TryParse(Environment.GetEnvironmentVariable("IFOSOFT_V2_LIMIT"), out var limit)) options.Limit = limit;
        var report = new BatchRunner().Run(options, Console.WriteLine);
        Assert.Equal("completed", report.Status); Assert.Equal(report.SelectedRowCount, report.CompletedRowCount);
        // The test verifies execution/reporting. Review findings stay in the reports.
        if (Environment.GetEnvironmentVariable("IFOSOFT_V2_STRICT") == "1")
        {
            Assert.DoesNotContain(report.ProcessingCounts, p => p.Key != "success" && p.Value > 0);
            Assert.DoesNotContain(report.ExpectationCounts, p => p.Key != "pass" && p.Value > 0);
            Assert.DoesNotContain(report.ValidationCounts, p => p.Key != "pass" && p.Value > 0);
        }
    }
}
