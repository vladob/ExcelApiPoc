using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace PdfLayoutEngine.V2;
internal static class TextMatching
{
    public static string Compact(string text) => string.Concat(text.Normalize(NormalizationForm.FormC).Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    public static IEnumerable<PositionedText[]> Rows(IEnumerable<PositionedText> input)
    {
        var rows = new List<List<PositionedText>>();
        foreach (var t in input.OrderByDescending(t => t.Baseline.Y))
        {
            if (rows.Count == 0 || Math.Abs(rows[rows.Count - 1][0].Baseline.Y - t.Baseline.Y) > 2) rows.Add(new List<PositionedText>());
            rows[rows.Count - 1].Add(t);
        }
        return rows.Select(r => r.OrderBy(t => t.Bounds.Left).ToArray());
    }
    public static string Assemble(IEnumerable<PositionedText> input)
    {
        var text = new StringBuilder(); PositionedText? previous = null;
        foreach (var token in Rows(input).SelectMany(r => r))
        {
            if (previous != null && Math.Abs(previous.Baseline.Y - token.Baseline.Y) > 2) text.Append('\n');
            else if (previous != null && token.Bounds.Left - previous.Bounds.Right > 1) text.Append(' ');
            text.Append(token.Text); previous = token;
        }
        return text.ToString();
    }
    public static bool Matches(string text, JsonElement spec)
    {
        text = Normalize(text, spec.S("whitespace")); var pattern = spec.S("value");
        var options = RegexOptions.CultureInvariant | (spec.B("ignoreCase") ? RegexOptions.IgnoreCase : RegexOptions.None);
        if (spec.S("mode") == "regex") return Regex.IsMatch(text, pattern, options, TimeSpan.FromMilliseconds(spec.N("regexTimeoutMs")));
        pattern = Normalize(pattern, spec.S("whitespace")); var comparison = spec.B("ignoreCase") ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return spec.S("mode") == "exact" ? string.Equals(text, pattern, comparison) : text.IndexOf(pattern, comparison) >= 0;
    }
    private static string Normalize(string text, string whitespace)
    {
        text = text.Normalize(NormalizationForm.FormC);
        return whitespace == "remove" ? string.Concat(text.Where(c => !char.IsWhiteSpace(c))) : whitespace == "collapse" ? Regex.Replace(text, @"\s+", " ").Trim() : text;
    }
}
