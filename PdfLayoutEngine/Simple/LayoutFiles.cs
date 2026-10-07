using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace PdfLayoutEngine.Simple;

/// <summary>One discovery rule for a single layout, a flat catalogue, or a tree of Compact catalogues.</summary>
public static class LayoutFiles
{
    static readonly string[] StructuredPrefixes = { "dbf-", "structured-", "crystal-", "excel-" };
    public static string[] All(string location)
    {
        if (File.Exists(location)) return new[] { Path.GetFullPath(location) };
        if (!Directory.Exists(location)) throw new DirectoryNotFoundException("Layout catalogue does not exist: " + location);
        // A repository root includes old Simple/GMX definitions. Only Compact directories are executable here.
        var compact = Directory.GetDirectories(location, "*", SearchOption.AllDirectories)
            .Where(d => string.Equals(Path.GetFileName(d), "Compact", StringComparison.OrdinalIgnoreCase)).ToArray();
        var roots = string.Equals(Path.GetFileName(Path.GetFullPath(location).TrimEnd(Path.DirectorySeparatorChar)), "Compact", StringComparison.OrdinalIgnoreCase)
            || compact.Length == 0 ? new[] { location } : compact;
        return roots.SelectMany(d => Directory.GetFiles(d, "*.json")).Select(Path.GetFullPath)
            .Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal).ToArray();
    }
    public static string[] Family(string location, string prefix)
    {
        if (File.Exists(location)) return All(location);
        return All(location).Where(p => prefix.Length == 0
            ? !StructuredPrefixes.Any(v => Path.GetFileName(p).StartsWith(v, StringComparison.OrdinalIgnoreCase))
            : Path.GetFileName(p).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
    public static int Validate(string location)
    {
        var files = All(location);
        if (files.Length == 0) throw new InvalidDataException("No compact JSON layouts found: " + location);
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in files)
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path));
            if (!json.RootElement.TryGetProperty("id", out var id) || string.IsNullOrWhiteSpace(id.GetString()))
                throw new InvalidDataException("Layout has no id: " + path);
            if (!ids.Add(id.GetString()!)) throw new InvalidDataException("Duplicate layout id: " + id.GetString());
        }
        return files.Length;
    }
}
public sealed class AmbiguousLayoutException : Exception
{
    public string[] Candidates { get; }
    public AmbiguousLayoutException(IEnumerable<string> candidates)
        : base("More than one layout matches: " + string.Join(", ", candidates)) { Candidates = candidates.ToArray(); }
}
