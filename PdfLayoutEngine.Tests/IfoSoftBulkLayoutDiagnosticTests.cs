using System.Diagnostics;
using System.Text;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Recognition;
using Xunit.Abstractions;

namespace PdfLayoutEngine.Tests;

/// <summary>
/// Opt-in inventory scan. Set IFOSOFT_BULK_TEST_ROOT to the TestAll folder and
/// IFOSOFT_BULK_REPORT_FILE to a writable CSV path before running this test.
/// This checks extraction and layout identification, not financial row import.
/// </summary>
public sealed class IfoSoftBulkLayoutDiagnosticTests(ITestOutputHelper output)
{
    [Fact]
    public void Detects_layouts_for_all_ifosoft_documents()
    {
        var root = Environment.GetEnvironmentVariable("IFOSOFT_BULK_TEST_ROOT");
        var report = Environment.GetEnvironmentVariable("IFOSOFT_BULK_REPORT_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(root) && Directory.Exists(root),
            "Set IFOSOFT_BULK_TEST_ROOT to the existing IfoSoft TestAll folder.");
        Assert.False(string.IsNullOrWhiteSpace(report),
            "Set IFOSOFT_BULK_REPORT_FILE to a writable CSV path.");

        var files = Directory.EnumerateFiles(root!, "*", SearchOption.AllDirectories)
            .Where(path => IsSupportedExtension(path) && IsKnownReportName(Path.GetFileName(path)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        Assert.NotEmpty(files);

        var layoutFolder = Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft");
        var layouts = Directory.GetFiles(layoutFolder, "*.json")
            .Select(path => new LayoutDefinitionLoader().Load(File.ReadAllText(path)).Definition!)
            .ToArray();
        var detector = new LayoutSignatureDetector();
        var rows = new List<string> { "Path,Extension,Pages,Tokens,Layout,Status,ElapsedMs,Error" };
        var failures = 0;
        foreach (var file in files)
        {
            var watch = Stopwatch.StartNew();
            var pages = 0;
            var tokens = 0;
            var layout = "";
            var status = "OK";
            var error = "";
            try
            {
                var document = Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                    ? new ITextPdfTokenExtractor().Extract(file)
                    : new OpenXpsTokenExtractor().Extract(file);
                pages = document.Pages.Count;
                tokens = document.Tokens.Count;
                var matches = detector.Detect(document, layouts);
                layout = string.Join(";", matches.Select(match => match.Id));
                if (matches.Count != 1)
                {
                    status = matches.Count == 0 ? "Unmatched" : "Ambiguous";
                    failures++;
                }
            }
            catch (Exception ex)
            {
                status = "Error";
                error = ex.GetType().Name + ": " + ex.Message;
                failures++;
            }
            watch.Stop();
            rows.Add(string.Join(",", Csv(file), Csv(Path.GetExtension(file)), pages, tokens,
                Csv(layout), Csv(status), watch.ElapsedMilliseconds, Csv(error)));
        }

        var reportPath = Path.GetFullPath(report!);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllLines(reportPath, rows, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        output.WriteLine($"Scanned {files.Length} files; {failures} failed. Report: {reportPath}");
        Assert.True(failures == 0,
            $"{failures} of {files.Length} files failed extraction or unique layout detection. See {reportPath}");
    }

    private static bool IsSupportedExtension(string path) =>
        new[] { ".pdf", ".oxps", ".xps" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    private static bool IsKnownReportName(string name) =>
        new[] { "HL_KNIHA_", "U_DENNIK_", "UCT_ROZVRH_", "PBUDOKLAD_" }
            .Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"").Replace('\r', ' ').Replace('\n', ' ') + "\"";
}
