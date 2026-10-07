using System.Diagnostics;
using System.Text.Json;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;

int timeoutSeconds = 120;
var timeoutOptions = args.Where(a => a.StartsWith("--timeout-seconds=", StringComparison.Ordinal)).ToArray();
if (timeoutOptions.Length > 1 || timeoutOptions.Length == 1 &&
    (!int.TryParse(timeoutOptions[0].Substring("--timeout-seconds=".Length), out timeoutSeconds) || timeoutSeconds < 1 || timeoutSeconds > 3600))
    throw new ArgumentException("Timeout must be 1..3600 seconds.");
args = args.Where(a => !a.StartsWith("--timeout-seconds=", StringComparison.Ordinal)).ToArray();
if (args.Length < 3 || args.Length > 5)
{
    Console.Error.WriteLine("Usage: SimpleRunner <PdfLayouts-root-or-catalogue-or-layout.json> <level:1..5> <file-directory-or-manifest.csv> [output-directory] [selected-fiscal-year] [--timeout-seconds=120]");
    return 1;
}
if (!int.TryParse(args[1], out int level) || level < 1 || level > 5) throw new ArgumentException("Level must be 1..5.");
int? selectedYear = args.Length > 4 ? int.Parse(args[4]) : null;
if (selectedYear.HasValue && (selectedYear < 1900 || selectedYear > 9999)) throw new ArgumentException("Selected fiscal year must be 1900..9999.");
int layoutCount = LayoutFiles.Validate(args[0]);
string output = Path.GetFullPath(args.Length > 3 ? args[3] : "TestResults/CompactLayouts");
// Materialize before creating output so the run cannot discover its own result files.
var files = (Path.GetExtension(args[2]).Equals(".csv", StringComparison.OrdinalIgnoreCase) && IsManifest(args[2])
    ? ReadManifest(args[2])
    : Directory.Exists(args[2])
        ? Directory.GetFiles(args[2], "*", SearchOption.AllDirectories)
            .Where(f => !Path.GetFullPath(f).StartsWith(output + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase).Select(f => new Input(Path.GetFullPath(f)))
        : new[] { new Input(Path.GetFullPath(args[2])) }).ToArray();
Directory.CreateDirectory(output);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
using var summary = new StreamWriter(Path.Combine(output, "summary.csv"));
summary.WriteLine("File,RequestedLevel,CompletedLevel,Status,Layout,Category,FiscalYear,DecodedPages,Rows,Milliseconds,Errors,Warnings,ExpectedYearCheck,ExpectedCategoryCheck,Software,Format,ExpectedSoftwareCheck,IssueCodes,Message,CandidateLayouts,CanonicalRows");
Console.WriteLine($"Loaded {layoutCount} layouts; examining {files.Length} files at level {level}.");
int failures = 0, ordinal = 0;
foreach (var input in files)
{
    var watch = Stopwatch.StartNew();
    string stem = (++ordinal).ToString("D4") + "-" + Path.GetFileName(input.FilePath);
    ImportResult result = new() { RequestedLevel = (ImportLevel)level, Format = Path.GetExtension(input.FilePath).TrimStart('.').ToUpperInvariant() };
    object? canonical = null;
    string? exception = null;
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
    try
    {
        if (!File.Exists(input.FilePath)) throw new FileNotFoundException("Input file not found.", input.FilePath);
        if (!new[] { ".pdf", ".xps", ".oxps", ".csv", ".xml", ".dbf", ".xls", ".xlsx" }.Contains(Path.GetExtension(input.FilePath).ToLowerInvariant()))
            throw new NotSupportedException("No staged importer for extension " + Path.GetExtension(input.FilePath));
        using var importer = new CompactLayoutImporter(input.FilePath, args[0], selectedYear);
        // Keep the recognised result even if a later stage throws.
        result = importer.Examine(ImportLevel.Recognize, timeout.Token);
        if (result.LayoutId != null && result.Status != "unsupported" && level > 1)
            result = importer.Examine((ImportLevel)level, timeout.Token);
        canonical = importer.Canonical;
        if (result.Status == "unrecognized" && !result.Issues.Any(i => i.Code == "unrecognizedLayout"))
            result.Issues.Add(new ImportIssue { Code = "unrecognizedLayout", Severity = "warning", Message = "No supported layout matches the source content; software and category could not be established." });
    }
    catch (Exception ex)
    {
        exception = ex.ToString();
        string code;
        if (ex is AmbiguousLayoutException ambiguous)
        {
            result.Status = "ambiguous"; code = "ambiguousLayout";
            result.CandidateLayouts = ambiguous.Candidates.ToList();
        }
        else if (ex is OperationCanceledException) { result.Status = "timeout"; code = "examinationTimeout"; }
        else if (ex is FileNotFoundException || ex is DirectoryNotFoundException) { result.Status = "error"; code = "fileNotFound"; }
        else if (ex is NotSupportedException) { result.Status = "unsupported"; code = "unsupportedFormat"; }
        else { result.Status = "error"; code = "importException"; }
        result.Issues.Add(new ImportIssue { Code = code, Message = ex.Message });
    }
    result.RequestedLevel = (ImportLevel)level;
    string year = CompactSession.Value(result.Identifiers, "fiscalYear");
    string yearCheck = Check(input.ExpectedYear, year, level >= 2 && result.CompletedLevel >= 2);
    string categoryCheck = Check(ExpectedCategory(input.PerceivedCategory), result.Category);
    string softwareCheck = Check(Software(input.ExpectedSoftware), Software(result.Producer));
    // Preserve extraction/validation failures even when an expectation also fails.
    string status = result.Status == "completed" && new[] { yearCheck, categoryCheck, softwareCheck }.Contains("fail") ? "expectationMismatch" : result.Status;
    int errors = result.Issues.Count(i => i.Severity == "error"), warnings = result.Issues.Count(i => i.Severity == "warning");
    var canonicalRows = canonical switch
    {
        ExcelApiPoc.AccountingImport.Models.JournalImport aj => aj.Rows.Count,
        ExcelApiPoc.AccountingImport.Models.GeneralLedgerImport gl => gl.Rows.Count,
        ExcelApiPoc.AccountingImport.Models.AccountingFrameworkImport af => af.Rows.Count,
        _ => 0
    };
    File.WriteAllText(Path.Combine(output, stem + ".json"), JsonSerializer.Serialize(new
    {
        input, status, result, canonical, expectedYearCheck = yearCheck, expectedCategoryCheck = categoryCheck,
        expectedSoftwareCheck = softwareCheck, exception
    }, options));
    var messages = result.Issues.Select(i => i.Message).Take(5).ToList();
    if (yearCheck == "fail") messages.Add($"Expected year {input.ExpectedYear}; detected {year}.");
    if (categoryCheck == "fail") messages.Add($"Expected category {input.PerceivedCategory}; detected {result.Category}.");
    if (softwareCheck == "fail") messages.Add($"Expected software {input.ExpectedSoftware}; detected {result.Producer}.");
    summary.WriteLine(string.Join(",", new[]
    {
        input.FilePath, level.ToString(), result.CompletedLevel.ToString(), status, result.LayoutId ?? "", result.Category ?? "", year,
        result.DecodedPageCount.ToString(), result.Rows.Count.ToString(), watch.ElapsedMilliseconds.ToString(), errors.ToString(), warnings.ToString(),
        yearCheck, categoryCheck, result.Producer ?? "", result.Format, softwareCheck,
        string.Join(";", result.Issues.Select(i => i.Code).Distinct()), string.Join(" | ", messages), string.Join(";", result.CandidateLayouts), canonicalRows.ToString()
    }.Select(Csv)));
    summary.Flush();
    Console.WriteLine($"{Path.GetFileName(input.FilePath)}: {status}; {result.Producer ?? "unknown software"}; {result.LayoutId ?? "no layout"}; decoded {result.DecodedPageCount} pages; {result.Rows.Count} source rows; {watch.ElapsedMilliseconds} ms; {errors} errors");
    if (status != "completed") failures++;
}
Console.WriteLine($"Finished {files.Length} files: {files.Length - failures} completed, {failures} requiring review. Results: {output}");
return failures > 0 ? 2 : 0;

static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
static string Check(string expected, string? actual, bool examined = true) => string.IsNullOrWhiteSpace(expected) ? "notSpecified" : !examined ? "notExamined" : string.IsNullOrWhiteSpace(actual) ? "unavailable" : string.Equals(expected.Trim(), actual.Trim(), StringComparison.OrdinalIgnoreCase) ? "pass" : "fail";
static string Software(string? raw) => new string((raw ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant() switch { "KROSOMEGA" => "OMEGA", var value => value };
static string ExpectedCategory(string raw) => raw.Trim().ToUpperInvariant() switch { "GL" or "HL_KNIHA" => "GL", "AJ" or "U_DENNIK" or "DENNIK" => "AJ", "AF" or "UCT_ROZVRH" or "UROZVRH" => "AF", var value => value };
static IEnumerable<Input> ReadManifest(string path)
{
    using var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(path);
    parser.SetDelimiters(";"); parser.HasFieldsEnclosedInQuotes = true;
    var header = (parser.ReadFields() ?? throw new InvalidDataException("Empty manifest.")).Select(h => h.Trim().TrimStart('\uFEFF')).ToArray();
    int Index(string name) => Array.FindIndex(header, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));
    int fileIndex = Index("FilePath");
    if (fileIndex < 0) throw new InvalidDataException("Manifest requires FilePath.");
    string Field(string[] values, string key) { int i = Index(key); return i < 0 ? "" : values[i].Trim(); }
    while (!parser.EndOfData)
    {
        var fields = parser.ReadFields()!;
        if (fields.Length != header.Length) throw new InvalidDataException("Malformed manifest at line " + parser.LineNumber);
        string file = fields[fileIndex].Trim();
        if (file.Length == 0) throw new InvalidDataException("Empty FilePath at line " + parser.LineNumber);
        if (!Path.IsPathRooted(file)) file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, file));
        string software = Field(fields, "ExpectedSoftware");
        if (software.Length == 0) software = Field(fields, "Software");
        yield return new Input(file, Field(fields, "ExpectedYear"), Field(fields, "PerceivedCategory"), Field(fields, "AccountingEntity"), software);
    }
}
static bool IsManifest(string path) { using var r = new StreamReader(path); var line = (r.ReadLine() ?? "").TrimStart('\uFEFF'); return line.Split(';').Any(v => v.Trim().Trim('"').Equals("FilePath", StringComparison.OrdinalIgnoreCase)); }
record Input(string FilePath, string ExpectedYear = "", string PerceivedCategory = "", string AccountingEntity = "", string ExpectedSoftware = "");
