using PdfLayoutEngine.Runner;

if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("""
    Run from the repository root:
      dotnet run --project PdfLayoutEngine.Runner -- --allow-review-drafts

    Options:
      --manifest PATH       Default: PdfLayoutEngine.Tests/FilesToTest.csv
      --definitions PATH    Default: LayoutDefinitions/v2
      --output PATH         Default: TestResults/LayoutV2 (unique subfolder per run)
      --allow-review-drafts Required for the current review definitions
      --first-row N         First CSV source line (header is line 1)
      --last-row N          Last CSV source line, inclusive
      --limit N             Process at most N selected entries
      --timeout-seconds N   Cooperative per-file deadline; default 300
      --source-root PATH    Old prefix in manifest paths, used with --input-root
      --input-root PATH     Local replacement for --source-root
      --extraction LEVEL    recognition | metadata | records (default records)
      --validation LEVEL    none | completeness | internalConsistency (default)
      --strict              Exit 2 for incomplete/inconclusive/unsupported results too

    Exit: 0 completed without explicit failures; 1 setup/run error; 2 file,
    expectation or validation failure (or strict review finding); 130 cancelled.
    CSV and JSONL are flushed after each file. Ctrl+C preserves completed results.
    """);
    return 0;
}
try
{
    var options = new BatchOptions(); bool strict = false;
    for (int i = 0; i < args.Length; i++)
    {
        string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing option value.");
        switch (args[i])
        {
            case "--manifest": options.ManifestPath = Value(); break;
            case "--definitions": options.DefinitionsPath = Value(); break;
            case "--output": options.OutputRoot = Value(); break;
            case "--source-root": options.SourceRoot = Value(); break;
            case "--input-root": options.InputRoot = Value(); break;
            case "--first-row": options.FirstRow = int.Parse(Value()); break;
            case "--last-row": options.LastRow = int.Parse(Value()); break;
            case "--limit": options.Limit = int.Parse(Value()); break;
            case "--timeout-seconds": options.TimeoutSeconds = int.Parse(Value()); break;
            case "--extraction": options.Extraction = Value(); break;
            case "--validation": options.Validation = Value(); break;
            case "--allow-review-drafts": options.AllowReviewDrafts = true; break;
            case "--strict": strict = true; break;
            default: throw new ArgumentException("Unknown option: " + args[i]);
        }
    }
    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
    var report = new BatchRunner().Run(options, Console.WriteLine, cancellation.Token);
    Console.WriteLine($"{report.Status}: {report.CompletedRowCount}/{report.SelectedRowCount} entries. Results: {report.OutputDirectory}");
    if (report.Status == "cancelled") return 130;
    bool failed = report.ProcessingCounts.GetValueOrDefault("failed") > 0 || report.ExpectationCounts.GetValueOrDefault("fail") > 0 || report.ValidationCounts.GetValueOrDefault("fail") > 0;
    bool review = report.ProcessingCounts.Any(p => p.Key != "success" && p.Value > 0) || report.ExpectationCounts.Any(p => p.Key != "pass" && p.Value > 0) || report.ValidationCounts.Any(p => p.Key != "pass" && p.Value > 0);
    return failed || strict && review ? 2 : 0;
}
catch (Exception ex) { Console.Error.WriteLine(ex.GetType().Name + ": " + ex.Message); return 1; }
