using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace PdfLayoutEngine.Runner;
public sealed record ExpectedInput(
    [property: JsonPropertyName("FilePath")] string FilePath,
    [property: JsonPropertyName("AccountingEntity")] string AccountingEntity,
    [property: JsonPropertyName("PerceivedCategory")] string PerceivedCategory,
    [property: JsonPropertyName("PreviousTestResult")] string PreviousTestResult,
    [property: JsonPropertyName("ExpectedYear")] string ExpectedYear);
public sealed record ManifestEntry(int RowNumber, ExpectedInput Expected);
public static class Manifest
{
    public static readonly string[] Headers = { "FilePath", "AccountingEntity", "PerceivedCategory", "PreviousTestResult", "ExpectedYear" };
    public static IReadOnlyList<ManifestEntry> Read(string path)
    {
        using var parser = new TextFieldParser(path, new UTF8Encoding(false, true), true) { HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = false };
        parser.SetDelimiters(";");
        if (parser.EndOfData || !parser.ReadFields()!.SequenceEqual(Headers)) throw new InvalidDataException("Expected semicolon-separated manifest header: " + string.Join(";", Headers));
        var rows = new List<ManifestEntry>();
        while (!parser.EndOfData)
        {
            int line = checked((int)parser.LineNumber);
            var f = parser.ReadFields()!;
            if (f.Length != 5 || string.IsNullOrWhiteSpace(f[0])) throw new InvalidDataException($"Manifest line {line}: expected five fields and a non-empty path.");
            if (!new[] { "HL_KNIHA", "U_DENNIK", "UCT_ROZVRH" }.Contains(f[2])) throw new InvalidDataException($"Manifest line {line}: unknown category '{f[2]}'.");
            if (f[4].Length != 0 && (f[4].Length != 4 || !int.TryParse(f[4], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1000)) throw new InvalidDataException($"Manifest line {line}: expected a four-digit year or blank.");
            rows.Add(new ManifestEntry(line, new ExpectedInput(f[0], f[1], f[2], f[3], f[4])));
        }
        return rows;
    }
    public static string ResolvePath(string input, string manifest, string? sourceRoot, string? inputRoot)
    {
        string Normalize(string s) => s.Replace('\\', '/').TrimEnd('/');
        if (sourceRoot != null && inputRoot != null)
        {
            string normalized = Normalize(input), prefix = Normalize(sourceRoot);
            if (!normalized.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Input path does not start with --source-root: " + input);
            string relative = normalized.Substring(prefix.Length + 1);
            if (relative.Split('/').Any(p => p == "..")) throw new InvalidDataException("Remapped input escapes its root.");
            return Path.GetFullPath(Path.Combine(inputRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        }
        if (!OperatingSystem.IsWindows() && (input.Contains('\\') || input.Length > 1 && input[1] == ':')) throw new InvalidDataException("Windows input paths on this platform require --source-root and --input-root.");
        return Path.GetFullPath(input, Path.GetDirectoryName(Path.GetFullPath(manifest))!);
    }
}
