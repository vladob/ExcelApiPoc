using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using PdfLayoutEngine.V2;
using PdfLayoutEngine.IText.V2;
using PdfLayoutEngine.Xps;

namespace PdfLayoutEngine.Runner;
public sealed class BatchOptions
{
    public string ManifestPath { get; set; } = "PdfLayoutEngine.Tests/FilesToTest.csv";
    public string DefinitionsPath { get; set; } = "LayoutDefinitions/v2";
    public string OutputRoot { get; set; } = "TestResults/LayoutV2";
    public string? SourceRoot { get; set; }
    public string? InputRoot { get; set; }
    public int? FirstRow { get; set; }
    public int? LastRow { get; set; }
    public int? Limit { get; set; }
    public int TimeoutSeconds { get; set; } = 300;
    public bool AllowReviewDrafts { get; set; }
    public string Extraction { get; set; } = "records";
    public string Validation { get; set; } = "internalConsistency";
}
public sealed class BatchReport
{
    public string RunId { get; set; } = "";
    public DateTimeOffset StartedUtc { get; set; }
    public DateTimeOffset FinishedUtc { get; set; }
    public string Status { get; set; } = "running";
    public string OutputDirectory { get; set; } = "";
    public string ManifestSha256 { get; set; } = "";
    public string CatalogSha256 { get; set; } = "";
    public BatchOptions Options { get; set; } = new();
    public int ManifestRowCount { get; set; }
    public int SelectedRowCount { get; set; }
    public int CompletedRowCount { get; set; }
    public Dictionary<string, int> ProcessingCounts { get; set; } = new();
    public Dictionary<string, int> ExpectationCounts { get; set; } = new();
    public Dictionary<string, int> ValidationCounts { get; set; } = new();
}
public sealed class BatchRunner
{
    // Test seam accepts only a resolved path and token. Expected values never reach extraction.
    public delegate EngineResult DocumentProcessor(string path, CancellationToken cancellationToken);
    public BatchReport Run(BatchOptions options, Action<string>? log = null, CancellationToken cancellationToken = default, DocumentProcessor? processor = null)
    {
        if (options.TimeoutSeconds < 1 || options.TimeoutSeconds > 86400 || options.Limit is <= 0 || options.FirstRow is < 2 || options.LastRow is < 2 || options.FirstRow > options.LastRow) throw new ArgumentException("Invalid timeout, row range or limit.");
        if ((options.SourceRoot == null) != (options.InputRoot == null)) throw new ArgumentException("Specify both --source-root and --input-root.");
        if (!new[] { "recognition", "metadata", "records" }.Contains(options.Extraction) || !new[] { "none", "completeness", "internalConsistency" }.Contains(options.Validation)) throw new ArgumentException("Unknown extraction or validation level.");
        var allRows = Manifest.Read(options.ManifestPath);
        var rows = allRows.Where(r => (!options.FirstRow.HasValue || r.RowNumber >= options.FirstRow) && (!options.LastRow.HasValue || r.RowNumber <= options.LastRow)).Take(options.Limit ?? int.MaxValue).ToArray();
        if (rows.Length == 0) throw new InvalidDataException("No manifest entries were selected.");
        var catalog = DefinitionCatalog.Load(options.DefinitionsPath, options.AllowReviewDrafts);
        var engine = new PositionedLayoutEngine(catalog);
        processor ??= (path, ct) => engine.Process(path, Adapter(path), new EngineOptions { Extraction = options.Extraction, Validation = options.Validation, ExecutionTimeMs = checked(options.TimeoutSeconds * 1000) }, new ExtractionLimits { ExecutionTimeMs = checked(options.TimeoutSeconds * 1000) }, ct);
        var report = new BatchReport { RunId = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N")[..8], StartedUtc = DateTimeOffset.UtcNow, Options = options, ManifestRowCount = allRows.Count, SelectedRowCount = rows.Length, ManifestSha256 = DefinitionCatalog.Hash(File.ReadAllBytes(options.ManifestPath)), CatalogSha256 = DefinitionCatalog.Hash(File.ReadAllBytes(Path.Combine(options.DefinitionsPath, "catalog.json"))) };
        report.OutputDirectory = Path.GetFullPath(Path.Combine(options.OutputRoot, report.RunId));
        Directory.CreateDirectory(Path.Combine(report.OutputDirectory, "files"));
        File.Copy(options.ManifestPath, Path.Combine(report.OutputDirectory, "manifest.csv"));
        SaveReport(); log?.Invoke("Results: " + report.OutputDirectory);
        using var csv = new StreamWriter(Path.Combine(report.OutputDirectory, "summary.csv"), false, new UTF8Encoding(true)) { AutoFlush = true };
        using var jsonl = new StreamWriter(Path.Combine(report.OutputDirectory, "results.jsonl"), false, new UTF8Encoding(false)) { AutoFlush = true };
        WriteCsv(csv, new[] { "ManifestRowNumber", "FilePath", "AccountingEntity", "PerceivedCategory", "PreviousTestResult", "ExpectedYear", "ActualCategory", "ActualEntityCin", "ActualEntityName", "ActualFiscalYear", "ProcessingStatus", "RecognitionStatus", "ExpectationStatus", "ValidationStatus", "EligibleRecordCount", "AvailablePages", "InspectedPages", "OriginalInputCompleteness", "UnresolvedCandidates", "OrphanTokens", "ElapsedMilliseconds", "DetailResultPath", "Explanations" });
        try
        {
            foreach (var entry in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var watch = Stopwatch.StartNew(); string? path = null, enginePath = null, errorType = null, errorMessage = null, errorDetail = null;
                bool writingEngine = false;
                var result = new ManifestResult { ManifestRowNumber = entry.RowNumber, Expected = entry.Expected };
                string stem = "files/row-" + entry.RowNumber.ToString("D6", CultureInfo.InvariantCulture);
                log?.Invoke($"[{report.CompletedRowCount + 1}/{rows.Length}] Row {entry.RowNumber}: {entry.Expected.FilePath}");
                try
                {
                    path = Manifest.ResolvePath(entry.Expected.FilePath, options.ManifestPath, options.SourceRoot, options.InputRoot);
                    if (!File.Exists(path)) throw new FileNotFoundException("Manifest file was not found.", path);
                    _ = Adapter(path); // Extension dispatch only; no expected category/year enters the engine.
                    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); deadline.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));
                    var extracted = processor(path, deadline.Token);
                    deadline.Token.ThrowIfCancellationRequested();
                    enginePath = stem + ".engine.json";
                    writingEngine = true;
                    AtomicWrite(Path.Combine(report.OutputDirectory, enginePath), extracted.ToJson());
                    writingEngine = false;
                    using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(report.OutputDirectory, enginePath)))) catalog.Validate("result", document.RootElement);
                    result = Results.Evaluate(entry, extracted);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex) when (ex is not OutOfMemoryException && !(writingEngine && (ex is IOException || ex is UnauthorizedAccessException)))
                {
                    result.ProcessingStatus = ex is NotSupportedException ? "unsupported" : "failed";
                    errorType = ex is OperationCanceledException ? "timeout" : ex.GetType().Name;
                    errorMessage = ex.Message; errorDetail = ex.ToString(); result.Explanations.Add(errorType + ": " + errorMessage);
                }
                watch.Stop();
                result.DetailResultPath = stem + ".json";
                using (var summary = JsonDocument.Parse(JsonSerializer.Serialize(result, Results.JsonOptions))) catalog.Validate("manifest-result", summary.RootElement);
                var detail = new FileDetail(entry.RowNumber, path, watch.ElapsedMilliseconds, enginePath, errorType, errorMessage, errorDetail, result);
                AtomicWrite(Path.Combine(report.OutputDirectory, result.DetailResultPath), JsonSerializer.Serialize(detail, Results.JsonOptions));
                jsonl.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions(Results.JsonOptions) { WriteIndented = false }));
                WriteCsv(csv, new[] { entry.RowNumber.ToString(CultureInfo.InvariantCulture), entry.Expected.FilePath, entry.Expected.AccountingEntity, entry.Expected.PerceivedCategory, entry.Expected.PreviousTestResult, entry.Expected.ExpectedYear, result.Actual.Category, result.Actual.EntityCin, result.Actual.EntityName, result.Actual.FiscalYear?.ToString(CultureInfo.InvariantCulture), result.ProcessingStatus, result.RecognitionStatus, result.ExpectationStatus, result.ValidationStatus, result.RecordCount.ToString(CultureInfo.InvariantCulture), result.Coverage.AvailablePageCount.ToString(CultureInfo.InvariantCulture), string.Join(",", result.Coverage.InspectedPages), result.Coverage.OriginalInputCompleteness, result.Coverage.UnresolvedCandidateCount.ToString(CultureInfo.InvariantCulture), result.Coverage.OrphanTokenCount.ToString(CultureInfo.InvariantCulture), watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture), result.DetailResultPath, string.Join(" | ", result.Explanations) });
                report.CompletedRowCount++; Increment(report.ProcessingCounts, result.ProcessingStatus); Increment(report.ExpectationCounts, result.ExpectationStatus); Increment(report.ValidationCounts, result.ValidationStatus); SaveReport();
                log?.Invoke($"  {result.ProcessingStatus}; recognition={result.RecognitionStatus}; expectations={result.ExpectationStatus}; eligible records={result.RecordCount}; {watch.Elapsed.TotalSeconds:F1}s");
            }
            report.Status = "completed";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { report.Status = "cancelled"; }
        catch { report.Status = "failed"; throw; }
        finally { report.FinishedUtc = DateTimeOffset.UtcNow; SaveReport(); }
        return report;
        void SaveReport() => AtomicWrite(Path.Combine(report.OutputDirectory, "run.json"), JsonSerializer.Serialize(report, Results.JsonOptions));
    }
    private static IPositionedDocumentAdapter Adapter(string path) => Path.GetExtension(path).ToLowerInvariant() switch { ".pdf" => new ITextPositionedAdapter(), ".xps" or ".oxps" => new XpsPositionedAdapter(), _ => throw new NotSupportedException("Supported document extensions: PDF, XPS, OXPS.") };
    private static void Increment(Dictionary<string, int> counts, string key) => counts[key] = counts.GetValueOrDefault(key) + 1;
    private static void AtomicWrite(string path, string text) { File.WriteAllText(path + ".tmp", text, new UTF8Encoding(false)); File.Move(path + ".tmp", path, true); }
    private static void WriteCsv(TextWriter writer, IEnumerable<string?> fields) => writer.WriteLine(string.Join(";", fields.Select(v => "\"" + (v ?? "").Replace("\"", "\"\"") + "\"")));
}
