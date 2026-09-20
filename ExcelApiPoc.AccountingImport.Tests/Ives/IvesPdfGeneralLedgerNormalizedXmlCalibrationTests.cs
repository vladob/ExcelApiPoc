using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfGeneralLedgerNormalizedXmlCalibrationTests
{
    private const string PdfVariable = "IVES_GENERAL_LEDGER_PDF_TEST_FILE";
    private const string WorkbookVariable = "IVES_GENERAL_LEDGER_XML_ANALYSIS_FILE";

    private readonly ITestOutputHelper output;

    public IvesPdfGeneralLedgerNormalizedXmlCalibrationTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Calibrate_pdf_geometry_against_normalized_xml_workbook()
    {
        string pdfPath = RequireFile(PdfVariable);
        string workbookPath = RequireFile(WorkbookVariable);

        ReferenceData reference = ReadWorkbook(workbookPath);

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);
        IReadOnlyList<BaselineGroup> groups =
            new BaselineGroupBuilder().Build(document);

        BaselineGroup[] documents = groups.Where(IsDocument).ToArray();
        BaselineGroup[] accounts = groups.Where(IsAccount).ToArray();
        BaselineGroup[] synthetics = groups.Where(IsSynthetic).ToArray();
        BaselineGroup[] totals = groups.Where(IsTotal).ToArray();

        output.WriteLine("IVES GL PDF / normalized XML calibration");
        output.WriteLine("-----------------------------------------");
        output.WriteLine("PDF      : " + Path.GetFileName(pdfPath));
        output.WriteLine("Workbook : " + Path.GetFileName(workbookPath));
        output.WriteLine("");
        WriteCount("Documents", documents.Length, reference.Documents.Count);
        WriteCount("Accounts", accounts.Length, reference.Accounts.Count);
        WriteCount("Synthetic", synthetics.Length, reference.Synthetics.Count);
        WriteCount("Totals", totals.Length, reference.Totals.Count);

        Assert.Equal(reference.Documents.Count, documents.Length);
        Assert.Equal(reference.Accounts.Count, accounts.Length);
        Assert.Equal(reference.Synthetics.Count, synthetics.Length);
        Assert.Equal(reference.Totals.Count, totals.Length);

        var stats = new Stats(output);

        for (int i = 0; i < reference.Documents.Count; i++)
        {
            DocumentRef expected = reference.Documents[i];
            BaselineGroup actual = documents[i];

            stats.Identity(
                "Document",
                Compact(actual).StartsWith(
                    expected.Date.ToString("dd.MM.", CultureInfo.InvariantCulture) +
                    expected.DocumentNumber,
                    StringComparison.Ordinal),
                expected.Key,
                actual);

            stats.Left("Document.Date", actual,
                expected.Date.ToString("dd.MM.", CultureInfo.InvariantCulture),
                20, 60, expected.Key);
            stats.Left("Document.DocumentNumber", actual,
                expected.DocumentNumber, 40, 110, expected.Key);
            stats.Left("Document.Account", actual,
                Prefix(CompactValue(expected.Account), 14),
                90, 230, expected.Key);
            stats.Left("Document.Text", actual,
                Prefix(CompactValue(expected.Text), 14),
                210, 430, expected.Key, optional: true);
            stats.Amount("Document.Debit", actual,
                expected.Debit, 375, 445, 434.7, expected.Key);
            stats.Amount("Document.Credit", actual,
                expected.Credit, 445, 513, 502.2, expected.Key);
        }

        for (int i = 0; i < reference.Accounts.Count; i++)
        {
            AccountRef expected = reference.Accounts[i];
            BaselineGroup actual = accounts[i];
            string account = CompactValue(expected.Account);

            stats.Identity(
                "AccountSummary",
                Compact(actual).StartsWith("--" + account, StringComparison.Ordinal),
                expected.Key,
                actual);

            stats.Left("AccountSummary.Account", actual,
                Prefix(account, 16), 90, 230, expected.Key);
            stats.Left("AccountSummary.Text", actual,
                Prefix(CompactValue(expected.Text), 16),
                210, 450, expected.Key, optional: true);
            stats.Amount("AccountSummary.Opening", actual,
                expected.Opening, 285, 380, 363.4, expected.Key);
            stats.Amount("AccountSummary.Debit", actual,
                expected.Debit, 375, 445, 434.7, expected.Key);
            stats.Amount("AccountSummary.Credit", actual,
                expected.Credit, 445, 513, 502.2, expected.Key);
            stats.Amount("AccountSummary.Closing", actual,
                expected.Closing, 513, 580, 570.1, expected.Key);
        }

        for (int i = 0; i < reference.Synthetics.Count; i++)
        {
            SyntheticRef expected = reference.Synthetics[i];
            BaselineGroup actual = synthetics[i];

            stats.Identity(
                "SyntheticSummary",
                Compact(actual).StartsWith(
                    "--" + expected.AccountCode,
                    StringComparison.Ordinal),
                expected.Key,
                actual);

            stats.Amount("SyntheticSummary.Opening", actual,
                expected.Opening, 285, 380, 363.4, expected.Key);
            stats.Amount("SyntheticSummary.Debit", actual,
                expected.Debit, 375, 445, 434.7, expected.Key);
            stats.Amount("SyntheticSummary.Credit", actual,
                expected.Credit, 445, 513, 502.2, expected.Key);
            stats.Amount("SyntheticSummary.Closing", actual,
                expected.Closing, 513, 580, 570.1, expected.Key);
        }

        for (int i = 0; i < reference.Totals.Count; i++)
        {
            TotalRef expected = reference.Totals[i];
            BaselineGroup actual = totals[i];

            stats.Identity(
                "ReportTotal",
                Compact(actual).StartsWith("Celkom", StringComparison.OrdinalIgnoreCase),
                expected.Key,
                actual);

            stats.Amount("ReportTotal.Opening", actual,
                expected.Opening, 285, 380, 363.4, expected.Key);
            stats.Amount("ReportTotal.Debit", actual,
                expected.Debit, 375, 445, 434.7, expected.Key);
            stats.Amount("ReportTotal.Credit", actual,
                expected.Credit, 445, 513, 502.2, expected.Key);
            stats.Amount("ReportTotal.Closing", actual,
                expected.Closing, 513, 580, 570.1, expected.Key);
        }

        output.WriteLine("");
        stats.Write();
    }

    private void WriteCount(string label, int pdf, int reference)
    {
        output.WriteLine("{0,-10}: PDF {1,5}, reference {2,5}",
            label, pdf, reference);
    }

    private static ReferenceData ReadWorkbook(string path)
    {
        var result = new ReferenceData();

        using IExcelDataReader reader = ExcelWorkbookReader.Open(path);
        do
        {
            string name = reader.Name ?? string.Empty;

            if (name == "Details22")
                ReadDocuments(reader, result.Documents);
            else if (name == "Footer17")
                ReadAccounts(reader, result.Accounts);
            else if (name == "Footer03")
                ReadSynthetics(reader, result.Synthetics);
            else if (name == "Footer02")
                ReadTotals(reader, result.Totals);
        }
        while (reader.NextResult());

        return result;
    }

    private static void ReadDocuments(
        IExcelDataReader reader,
        ICollection<DocumentRef> rows)
    {
        Dictionary<string, int> h = Header(reader);

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            rows.Add(new DocumentRef(
                Key(reader, h, "RowNo02", "RowNo03", "RowNo17", "RowNo22"),
                Date(reader, h, "Datumvytvpohybu1.Value"),
                Text(reader, h, "Field6.Value"),
                Text(reader, h, "aUText1.Value"),
                Text(reader, h, "Field19.Value"),
                Number(reader, h, "Field40.Value"),
                Number(reader, h, "Field41.Value")));
        }
    }

    private static void ReadAccounts(
        IExcelDataReader reader,
        ICollection<AccountRef> rows)
    {
        Dictionary<string, int> h = Header(reader);

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            rows.Add(new AccountRef(
                Key(reader, h, "RowNo02", "RowNo03", "RowNo17"),
                Text(reader, h, "aUText6.Value"),
                Text(reader, h, "Field58.Value"),
                Number(reader, h, "Field164.Value"),
                Number(reader, h, "Field56.Value"),
                Number(reader, h, "Field57.Value"),
                Number(reader, h, "Field55.Value")));
        }
    }

    private static void ReadSynthetics(
        IExcelDataReader reader,
        ICollection<SyntheticRef> rows)
    {
        Dictionary<string, int> h = Header(reader);

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            string marker = Text(reader, h, "Field60.Value");
            rows.Add(new SyntheticRef(
                Key(reader, h, "RowNo02", "RowNo03"),
                marker.Split('.')[0].Trim(),
                Number(reader, h, "Field47.Value"),
                Number(reader, h, "Field7.Value"),
                Number(reader, h, "Field8.Value"),
                Number(reader, h, "Field59.Value")));
        }
    }

    private static void ReadTotals(
        IExcelDataReader reader,
        ICollection<TotalRef> rows)
    {
        Dictionary<string, int> h = Header(reader);

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            rows.Add(new TotalRef(
                Key(reader, h, "RowNo02"),
                Number(reader, h, "PocstavCelk1.Value"),
                Number(reader, h, "Field156.Value"),
                Number(reader, h, "Field157.Value"),
                Number(reader, h, "Field158.Value")));
        }
    }

    private static Dictionary<string, int> Header(IExcelDataReader reader)
    {
        while (reader.Read())
        {
            var h = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string? value = ExcelWorkbookReader.GetText(reader, i);
                if (!string.IsNullOrWhiteSpace(value))
                    h[value.Trim()] = i;
            }

            if (h.ContainsKey("RowNo02"))
                return h;
        }

        throw new InvalidDataException("Normalized XML worksheet header not found.");
    }

    private static string Key(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        params string[] names)
    {
        return string.Join(",",
            names.Select(name => name + "=" + Text(reader, h, name)));
    }

    private static string Text(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        string name)
    {
        object? value = reader.GetValue(h[name]);
        if (value == null || value == DBNull.Value) return string.Empty;

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static decimal Number(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        string name)
    {
        object? value = reader.GetValue(h[name]);
        return value == null || value == DBNull.Value
            ? 0m
            : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    private static DateTime Date(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        string name)
    {
        object value = reader.GetValue(h[name]);
        if (value is DateTime date) return date.Date;
        if (value is double serial) return DateTime.FromOADate(serial).Date;

        return DateTime.Parse(
            Convert.ToString(value, CultureInfo.InvariantCulture)!,
            CultureInfo.InvariantCulture);
    }

    private static bool Blank(IExcelDataReader reader)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object? value = reader.GetValue(i);
            if (value != null && value != DBNull.Value &&
                !(value is string text && string.IsNullOrWhiteSpace(text)))
                return false;
        }

        return true;
    }

    private static bool IsDocument(BaselineGroup group) =>
        Regex.IsMatch(Compact(group), @"^\d{2}\.\d{2}\.");

    private static bool IsAccount(BaselineGroup group)
    {
        string text = Compact(group);
        return text.StartsWith("--", StringComparison.Ordinal) &&
               text.IndexOf("====", StringComparison.Ordinal) < 0;
    }

    private static bool IsSynthetic(BaselineGroup group)
    {
        string text = Compact(group);
        return text.StartsWith("--", StringComparison.Ordinal) &&
               text.IndexOf("====", StringComparison.Ordinal) >= 0;
    }

    private static bool IsTotal(BaselineGroup group) =>
        Compact(group).StartsWith("Celkom", StringComparison.OrdinalIgnoreCase);

    private static string Compact(BaselineGroup group) =>
        string.Concat(group.Tokens.Select(token => token.Text ?? string.Empty))
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty);

    private static string CompactValue(string value) =>
        string.Concat(value.Where(c => !char.IsWhiteSpace(c) && c != '\u00A0'));

    private static string Prefix(string value, int length) =>
        value.Length <= length ? value : value.Substring(0, length);

    private static string RequireFile(string variable)
    {
        string? path = Environment.GetEnvironmentVariable(variable);
        Assert.False(string.IsNullOrWhiteSpace(path), "Set " + variable + ".");
        Assert.True(File.Exists(path), variable + " file does not exist: " + path);
        return path!;
    }

    private sealed class Stats
    {
        private readonly ITestOutputHelper output;
        private readonly Dictionary<string, FieldStats> fields =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, CountStats> identities =
            new(StringComparer.Ordinal);

        public Stats(ITestOutputHelper output) => this.output = output;

        public void Identity(
            string kind,
            bool matched,
            string key,
            BaselineGroup actual)
        {
            if (!identities.TryGetValue(kind, out CountStats? stats))
            {
                stats = new CountStats();
                identities[kind] = stats;
            }

            stats.Total++;
            if (matched) stats.Matched++;
            else if (stats.Samples.Count < 6)
                stats.Samples.Add(key + " => " + Compact(actual));
        }

        public void Left(
            string field,
            BaselineGroup group,
            string expected,
            double left,
            double right,
            string key,
            bool optional = false)
        {
            if (optional && string.IsNullOrWhiteSpace(expected)) return;

            FieldStats stats = Get(field);
            GlyphMatch? match = Find(group, expected, left, right, null);
            stats.Add(match, null, key, expected, group);
        }

        public void Amount(
            string field,
            BaselineGroup group,
            decimal expected,
            double left,
            double right,
            double anchor,
            string key)
        {
            string text = expected
                .ToString("0.00", CultureInfo.InvariantCulture)
                .Replace('.', ',');

            FieldStats stats = Get(field);
            GlyphMatch? match = Find(group, text, left, right, anchor);
            stats.Add(match, anchor, key, text, group);
        }

        public void Write()
        {
            output.WriteLine("Identity alignment:");
            foreach (var pair in identities.OrderBy(pair => pair.Key))
            {
                output.WriteLine("  {0,-18} {1}/{2}",
                    pair.Key, pair.Value.Matched, pair.Value.Total);
                foreach (string sample in pair.Value.Samples)
                    output.WriteLine("    mismatch: " + sample);
            }

            output.WriteLine("");
            output.WriteLine("Field geometry:");
            foreach (var pair in fields.OrderBy(pair => pair.Key))
            {
                FieldStats s = pair.Value;
                output.WriteLine("  {0,-32} {1}/{2}",
                    pair.Key, s.Matched, s.Total);

                if (s.Matched > 0)
                {
                    output.WriteLine(
                        "    left  min/median/max : {0:F3} / {1:F3} / {2:F3}",
                        s.Left.Min(), Median(s.Left), s.Left.Max());
                    output.WriteLine(
                        "    right min/median/max : {0:F3} / {1:F3} / {2:F3}",
                        s.Right.Min(), Median(s.Right), s.Right.Max());

                    if (s.AnchorDelta.Count > 0)
                        output.WriteLine(
                            "    anchor delta median/max: {0:F3} / {1:F3}",
                            Median(s.AnchorDelta), s.AnchorDelta.Max());
                }

                foreach (string sample in s.Samples)
                    output.WriteLine("    unmatched: " + sample);
            }
        }

        private FieldStats Get(string field)
        {
            if (!fields.TryGetValue(field, out FieldStats? stats))
            {
                stats = new FieldStats();
                fields[field] = stats;
            }

            return stats;
        }

        private static double Median(IReadOnlyList<double> values)
        {
            double[] sorted = values.OrderBy(value => value).ToArray();
            int middle = sorted.Length / 2;
            return sorted.Length % 2 == 1
                ? sorted[middle]
                : (sorted[middle - 1] + sorted[middle]) / 2.0;
        }
    }

    private sealed class CountStats
    {
        public int Total { get; set; }
        public int Matched { get; set; }
        public List<string> Samples { get; } = new();
    }

    private sealed class FieldStats
    {
        public int Total { get; private set; }
        public int Matched { get; private set; }
        public List<double> Left { get; } = new();
        public List<double> Right { get; } = new();
        public List<double> AnchorDelta { get; } = new();
        public List<string> Samples { get; } = new();

        public void Add(
            GlyphMatch? match,
            double? anchor,
            string key,
            string expected,
            BaselineGroup actual)
        {
            Total++;
            if (match == null)
            {
                if (Samples.Count < 6)
                    Samples.Add(key + " expected '" + expected +
                        "' => " + Compact(actual));
                return;
            }

            Matched++;
            Left.Add(match.Left);
            Right.Add(match.Right);
            if (anchor.HasValue)
                AnchorDelta.Add(Math.Abs(match.Right - anchor.Value));
        }
    }

    private static GlyphMatch? Find(
        BaselineGroup group,
        string expected,
        double minimumLeft,
        double maximumRight,
        double? preferredRight)
    {
        string target = CompactValue(expected);
        if (target.Length == 0) return null;

        List<Glyph> glyphs = new();

        foreach (PdfTextToken token in group.Tokens
            .Where(token => token.Right >= minimumLeft && token.Left <= maximumRight)
            .OrderBy(token => token.Left)
            .ThenBy(token => token.Right))
        {
            foreach (char character in token.Text ?? string.Empty)
            {
                if (!char.IsWhiteSpace(character) && character != '\u00A0')
                    glyphs.Add(new Glyph(character, token.Left, token.Right));
            }
        }

        IEnumerable<int> endings = Enumerable.Range(0, glyphs.Count)
            .Where(i => glyphs[i].Character == target[^1]);

        if (preferredRight.HasValue)
        {
            endings = endings
                .Where(i => Math.Abs(glyphs[i].Right - preferredRight.Value) <= 3.0)
                .OrderBy(i => Math.Abs(glyphs[i].Right - preferredRight.Value));
        }

        foreach (int end in endings)
        {
            List<Glyph> matched = new() { glyphs[end] };
            int at = end;
            bool failed = false;

            for (int t = target.Length - 2; t >= 0; t--)
            {
                int found = -1;

                for (int i = at - 1; i >= 0; i--)
                {
                    if (glyphs[i].Character == target[t] &&
                        glyphs[at].Left - glyphs[i].Right <= 8.0 &&
                        glyphs[at].Left - glyphs[i].Right >= -5.0)
                    {
                        found = i;
                        break;
                    }
                }

                if (found < 0)
                {
                    failed = true;
                    break;
                }

                at = found;
                matched.Add(glyphs[at]);
            }

            if (!failed)
            {
                matched.Reverse();
                return new GlyphMatch(
                    matched.Min(g => g.Left),
                    matched.Max(g => g.Right));
            }
        }

        return null;
    }

    private sealed record Glyph(char Character, double Left, double Right);
    private sealed record GlyphMatch(double Left, double Right);

    private sealed class ReferenceData
    {
        public List<DocumentRef> Documents { get; } = new();
        public List<AccountRef> Accounts { get; } = new();
        public List<SyntheticRef> Synthetics { get; } = new();
        public List<TotalRef> Totals { get; } = new();
    }

    private sealed record DocumentRef(
        string Key, DateTime Date, string DocumentNumber,
        string Account, string Text, decimal Debit, decimal Credit);

    private sealed record AccountRef(
        string Key, string Account, string Text,
        decimal Opening, decimal Debit, decimal Credit, decimal Closing);

    private sealed record SyntheticRef(
        string Key, string AccountCode,
        decimal Opening, decimal Debit, decimal Credit, decimal Closing);

    private sealed record TotalRef(
        string Key, decimal Opening, decimal Debit, decimal Credit, decimal Closing);
}
