using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PdfLayoutEngine.V2;
public sealed class FieldValue
{
    public string State { get; set; } = "unresolved";
    public string? Value { get; set; }
    public string? Origin { get; set; }
    public string RawText { get; set; } = "";
    public List<string> EvidenceRefs { get; set; } = new List<string>();
    public List<string> Alternatives { get; set; } = new List<string>();
    public List<string> DerivationInputs { get; set; } = new List<string>();
    public static FieldValue Derived(string value, params string[] inputs) => new FieldValue { State = "present", Value = value, Origin = "derived", DerivationInputs = inputs.ToList() };
}
public static class ValueParser
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    public static FieldValue Parse(string raw, JsonElement profile, bool confidentlyLocated = true, int? explicitYear = null)
    {
        var result = new FieldValue { RawText = raw, Origin = "printed" };
        if (!confidentlyLocated) return result;
        if (string.IsNullOrWhiteSpace(raw)) { result.State = "blank"; return result; }
        var text = raw.Normalize(NormalizationForm.FormC); var values = new HashSet<string>();
        switch (profile.S("op"))
        {
            case "text":
                if (profile.B("normalizeNbsp")) text = text.Replace('\u00a0', ' ');
                values.Add(profile.B("trim") ? text.Trim() : text); break;
            case "identifier": values.Add(profile.B("trimOuterWhitespace") ? text.Trim() : text); break;
            case "integer":
                text = text.Trim(); if (profile.B("allowTrailingDot") && text.EndsWith(".", StringComparison.Ordinal)) text = text.Substring(0, text.Length - 1);
                if (Regex.IsMatch(text, @"^[+-]?\d+$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)) && long.TryParse(text, NumberStyles.AllowLeadingSign, Invariant, out var integer)) values.Add(integer.ToString(Invariant)); break;
            case "decimal":
                foreach (var format in profile.A("formats"))
                {
                    var t = text.Trim(); var sign = "";
                    if (t.StartsWith("-", StringComparison.Ordinal) || t.StartsWith("+", StringComparison.Ordinal))
                    { if (!profile.B("allowLeadingSign")) continue; sign = t.Substring(0, 1); t = t.Substring(1); if (profile.B("allowSpaceAfterSign")) t = t.TrimStart(); }
                    var separator = format.S("decimalSeparator");
                    if (format.GetProperty("wholeAmountSuffix").ValueKind == JsonValueKind.String)
                    { var suffix = format.S("wholeAmountSuffix"); if (t.EndsWith(suffix, StringComparison.Ordinal)) t = t.Substring(0, t.Length - suffix.Length) + separator + "00"; }
                    var parts = t.Split(new[] { separator }, StringSplitOptions.None); if (parts.Length > 2) continue;
                    if (parts.Length == 2 && (parts[1].Length != profile.N("fractionDigits") || !Digits(parts[1]))) continue;
                    var whole = parts[0]; var groups = format.Strings("groupingSeparators").Where(whole.Contains).ToArray();
                    if (groups.Length > 1) continue;
                    if (groups.Length == 1)
                    {
                        var chunks = whole.Split(new[] { groups[0] }, StringSplitOptions.None);
                        if (chunks[0].Length < 1 || chunks[0].Length > 3 || chunks.Skip(1).Any(c => c.Length != 3) || chunks.Any(c => !Digits(c))) continue;
                        whole = string.Concat(chunks);
                    }
                    if (!Digits(whole)) continue;
                    var normalized = sign + whole + (parts.Length == 2 ? "." + parts[1] : "");
                    if (decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, Invariant, out var amount)) values.Add(amount.ToString("G29", Invariant));
                }
                break;
            case "exactDate":
                foreach (var format in profile.Strings("formats"))
                {
                    var twoDigit = format.Contains("yy") && !format.Contains("yyyy");
                    if (twoDigit && explicitYear == null) continue;
                    if (!DateTime.TryParseExact(text.Trim(), format, Invariant, DateTimeStyles.None, out var date)) continue;
                    if (twoDigit)
                    {
                        if (date.Year % 100 != explicitYear!.Value % 100) continue;
                        try { date = new DateTime(explicitYear.Value, date.Month, date.Day); } catch (ArgumentOutOfRangeException) { continue; }
                    }
                    values.Add(date.ToString("yyyy-MM-dd", Invariant));
                }
                if (values.Count == 0 && explicitYear == null && Regex.IsMatch(text.Trim(), @"^\d{1,2}\.\d{1,2}\.\d{2}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
                { result.State = "unresolved"; return result; }
                break;
            case "exactTime":
                foreach (var format in profile.Strings("formats")) if (DateTime.TryParseExact(text.Trim(), format, Invariant, DateTimeStyles.None, out var time)) values.Add(time.ToString("HH:mm:ss", Invariant));
                break;
            case "accountingPeriod":
                var match = Regex.Match(text.Trim(), profile.S("pattern"), RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
                if (match.Success) values.Add(match.Groups[profile.S("codeGroup")].Value + "/" + match.Groups[profile.S("yearGroup")].Value);
                break;
            default: throw new NotSupportedException("Unknown parser: " + profile.S("op"));
        }
        result.State = values.Count == 0 ? "invalid" : values.Count == 1 ? "present" : "ambiguous";
        result.Value = values.Count == 1 ? values.Single() : null;
        if (values.Count > 1) result.Alternatives = values.OrderBy(x => x, StringComparer.Ordinal).ToList();
        return result;
    }
    private static bool Digits(string text) => text.Length > 0 && text.All(c => c >= '0' && c <= '9');
}
