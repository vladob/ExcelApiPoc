using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfGeneralLedgerAmountSlotCalibrationTests
{
    private const string PdfVariable = "IVES_GENERAL_LEDGER_PDF_TEST_FILE";
    private const string WorkbookVariable = "IVES_GENERAL_LEDGER_XML_ANALYSIS_FILE";

    private readonly ITestOutputHelper output;

    public IvesPdfGeneralLedgerAmountSlotCalibrationTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Derive_amount_glyph_slots_from_normalized_xml()
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

        Assert.Equal(reference.Documents.Count, documents.Length);
        Assert.Equal(reference.Accounts.Count, accounts.Length);
        Assert.Equal(reference.Synthetics.Count, synthetics.Length);
        Assert.Equal(reference.Totals.Count, totals.Length);

        var stats = new Dictionary<string, AmountSlotStats>(
            StringComparer.Ordinal)
        {
            ["Opening"] = new AmountSlotStats("Opening", 363.4),
            ["Debit"] = new AmountSlotStats("Debit", 434.7),
            ["Credit"] = new AmountSlotStats("Credit", 502.2),
            ["Closing"] = new AmountSlotStats("Closing", 570.1)
        };

        for (int i = 0; i < reference.Documents.Count; i++)
        {
            Observe(stats["Debit"], documents[i],
                reference.Documents[i].Debit, 375, 445,
                reference.Documents[i].Key);
            Observe(stats["Credit"], documents[i],
                reference.Documents[i].Credit, 445, 513,
                reference.Documents[i].Key);
        }

        for (int i = 0; i < reference.Accounts.Count; i++)
        {
            AccountAmounts row = reference.Accounts[i];
            Observe(stats["Opening"], accounts[i], row.Opening, 285, 380, row.Key);
            Observe(stats["Debit"], accounts[i], row.Debit, 375, 445, row.Key);
            Observe(stats["Credit"], accounts[i], row.Credit, 445, 513, row.Key);
            Observe(stats["Closing"], accounts[i], row.Closing, 513, 580, row.Key);
        }

        for (int i = 0; i < reference.Synthetics.Count; i++)
        {
            AccountAmounts row = reference.Synthetics[i];
            Observe(stats["Opening"], synthetics[i], row.Opening, 285, 380, row.Key);
            Observe(stats["Debit"], synthetics[i], row.Debit, 375, 445, row.Key);
            Observe(stats["Credit"], synthetics[i], row.Credit, 445, 513, row.Key);
            Observe(stats["Closing"], synthetics[i], row.Closing, 513, 580, row.Key);
        }

        for (int i = 0; i < reference.Totals.Count; i++)
        {
            AccountAmounts row = reference.Totals[i];
            Observe(stats["Opening"], totals[i], row.Opening, 285, 380, row.Key);
            Observe(stats["Debit"], totals[i], row.Debit, 375, 445, row.Key);
            Observe(stats["Credit"], totals[i], row.Credit, 445, 513, row.Key);
            Observe(stats["Closing"], totals[i], row.Closing, 513, 580, row.Key);
        }

        output.WriteLine("IVES GL PDF amount glyph-slot calibration");
        output.WriteLine("------------------------------------------");
        output.WriteLine("PDF      : " + Path.GetFileName(pdfPath));
        output.WriteLine("Workbook : " + Path.GetFileName(workbookPath));
        output.WriteLine("");

        foreach (AmountSlotStats column in stats.Values)
        {
            column.Write(output);
            Assert.Equal(column.Total, column.Matched);
        }
    }

    private static void Observe(
        AmountSlotStats stats,
        BaselineGroup group,
        decimal expected,
        double minimumLeft,
        double maximumRight,
        string key)
    {
        string text = expected
            .ToString("0.00", CultureInfo.InvariantCulture)
            .Replace('.', ',');

        GlyphMatch? match = MatchAmount(
            group,
            text,
            minimumLeft,
            maximumRight,
            stats.Anchor);

        stats.Total++;

        if (match == null)
        {
            if (stats.Unmatched.Count < 8)
                stats.Unmatched.Add(key + " expected '" + text +
                    "' => " + Compact(group));
            return;
        }

        stats.Matched++;

        for (int i = 0; i < match.Glyphs.Count; i++)
        {
            Glyph glyph = match.Glyphs[i];
            int positionFromRight = match.Glyphs.Count - 1 - i;
            double? gapToRight = i + 1 < match.Glyphs.Count
                ? match.Glyphs[i + 1].Left - glyph.Right
                : null;

            stats.Add(
                positionFromRight,
                glyph.Character,
                stats.Anchor - glyph.Left,
                stats.Anchor - glyph.Right,
                glyph.Right - glyph.Left,
                gapToRight);
        }
    }

    private static GlyphMatch? MatchAmount(
        BaselineGroup group,
        string expected,
        double minimumLeft,
        double maximumRight,
        double anchor)
    {
        string target = CompactValue(expected);

        List<Glyph> glyphs = new();

        foreach (PdfTextToken token in group.Tokens
            .Where(token =>
                token.Right >= minimumLeft &&
                token.Left <= maximumRight)
            .OrderBy(token => token.Left)
            .ThenBy(token => token.Right))
        {
            string text = token.Text ?? string.Empty;

            foreach (char character in text)
            {
                if (!char.IsWhiteSpace(character) &&
                    character != '\u00A0')
                {
                    glyphs.Add(
                        new Glyph(character, token.Left, token.Right));
                }
            }
        }

        int[] endings = glyphs
            .Select((glyph, index) => new { glyph, index })
            .Where(item =>
                item.glyph.Character == target[^1] &&
                Math.Abs(item.glyph.Right - anchor) <= 3.0)
            .OrderBy(item => Math.Abs(item.glyph.Right - anchor))
            .Select(item => item.index)
            .ToArray();

        foreach (int ending in endings)
        {
            var reversed = new List<Glyph> { glyphs[ending] };
            int at = ending;
            bool failed = false;

            for (int targetIndex = target.Length - 2;
                targetIndex >= 0;
                targetIndex--)
            {
                int found = -1;

                for (int i = at - 1; i >= 0; i--)
                {
                    if (glyphs[i].Character != target[targetIndex])
                        continue;

                    double gap = glyphs[at].Left - glyphs[i].Right;

                    if (gap >= -5.0 && gap <= 8.0)
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
                reversed.Add(glyphs[at]);
            }

            if (!failed)
            {
                reversed.Reverse();
                return new GlyphMatch(reversed);
            }
        }

        return null;
    }

    private sealed class AmountSlotStats
    {
        private readonly Dictionary<int, SlotStats> slots = new();

        public AmountSlotStats(string name, double anchor)
        {
            Name = name;
            Anchor = anchor;
        }

        public string Name { get; }
        public double Anchor { get; }
        public int Total { get; set; }
        public int Matched { get; set; }
        public List<string> Unmatched { get; } = new();

        public void Add(
            int positionFromRight,
            char character,
            double leftOffset,
            double rightOffset,
            double width,
            double? gapToRight)
        {
            if (!slots.TryGetValue(positionFromRight, out SlotStats? slot))
            {
                slot = new SlotStats(positionFromRight);
                slots[positionFromRight] = slot;
            }

            slot.Characters.TryGetValue(character, out int count);
            slot.Characters[character] = count + 1;
            slot.LeftOffsets.Add(leftOffset);
            slot.RightOffsets.Add(rightOffset);
            slot.Widths.Add(width);

            if (gapToRight.HasValue)
                slot.GapsToRight.Add(gapToRight.Value);
        }

        public void Write(ITestOutputHelper output)
        {
            output.WriteLine(
                "{0} anchor {1:F3}: matched {2}/{3}",
                Name,
                Anchor,
                Matched,
                Total);

            output.WriteLine(
                "  pos  role               chars      right offset min/med/max   gap-to-right min/med/max");

            foreach (SlotStats slot in slots.Values.OrderBy(s => s.Position))
            {
                string chars = string.Join(
                    "",
                    slot.Characters
                        .OrderByDescending(pair => pair.Value)
                        .ThenBy(pair => pair.Key)
                        .Select(pair => pair.Key));

                output.WriteLine(
                    "  {0,3}  {1,-18} {2,-10} {3,7:F3}/{4,7:F3}/{5,7:F3}   {6}",
                    slot.Position,
                    Role(slot.Position, slot.Characters.Keys),
                    chars,
                    slot.RightOffsets.Min(),
                    Median(slot.RightOffsets),
                    slot.RightOffsets.Max(),
                    slot.GapsToRight.Count == 0
                        ? "-"
                        : string.Format(
                            CultureInfo.InvariantCulture,
                            "{0:F3}/{1:F3}/{2:F3}",
                            slot.GapsToRight.Min(),
                            Median(slot.GapsToRight),
                            slot.GapsToRight.Max()));
            }

            foreach (string unmatched in Unmatched)
                output.WriteLine("  unmatched: " + unmatched);

            output.WriteLine("");
        }

        private static string Role(
            int position,
            IEnumerable<char> characters)
        {
            if (characters.Contains('-'))
                return "sign/leading";

            return position switch
            {
                0 => "decimal-2",
                1 => "decimal-1",
                2 => "decimal-comma",
                3 => "units",
                4 => "tens",
                5 => "hundreds",
                _ => IntegerRole(position - 3)
            };
        }

        private static string IntegerRole(int digitFromUnits)
        {
            int group = digitFromUnits / 3;
            int inGroup = digitFromUnits % 3;

            string groupName = group switch
            {
                1 => "thousands",
                2 => "millions",
                3 => "billions",
                4 => "trillions",
                _ => "10^" + (group * 3)
            };

            string digitName = inGroup switch
            {
                0 => "units",
                1 => "tens",
                _ => "hundreds"
            };

            return groupName + "-" + digitName;
        }
    }

    private sealed class SlotStats
    {
        public SlotStats(int position) => Position = position;

        public int Position { get; }
        public Dictionary<char, int> Characters { get; } = new();
        public List<double> LeftOffsets { get; } = new();
        public List<double> RightOffsets { get; } = new();
        public List<double> Widths { get; } = new();
        public List<double> GapsToRight { get; } = new();
    }

    private static double Median(IReadOnlyList<double> values)
    {
        double[] ordered = values.OrderBy(value => value).ToArray();
        int middle = ordered.Length / 2;

        return ordered.Length % 2 == 1
            ? ordered[middle]
            : (ordered[middle - 1] + ordered[middle]) / 2.0;
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
                ReadAccountAmounts(reader, result.Accounts, "Footer17");
            else if (name == "Footer03")
                ReadAccountAmounts(reader, result.Synthetics, "Footer03");
            else if (name == "Footer02")
                ReadAccountAmounts(reader, result.Totals, "Footer02");
        }
        while (reader.NextResult());

        return result;
    }

    private static void ReadDocuments(
        IExcelDataReader reader,
        ICollection<DocumentAmounts> rows)
    {
        Dictionary<string, int> h = Header(reader);

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            rows.Add(new DocumentAmounts(
                Key(reader, h, "RowNo02", "RowNo03", "RowNo17", "RowNo22"),
                Number(reader, h, "Field40.Value"),
                Number(reader, h, "Field41.Value")));
        }
    }

    private static void ReadAccountAmounts(
        IExcelDataReader reader,
        ICollection<AccountAmounts> rows,
        string sheet)
    {
        Dictionary<string, int> h = Header(reader);

        string opening;
        string debit;
        string credit;
        string closing;
        string[] keys;

        if (sheet == "Footer17")
        {
            opening = "Field164.Value";
            debit = "Field56.Value";
            credit = "Field57.Value";
            closing = "Field55.Value";
            keys = new[] { "RowNo02", "RowNo03", "RowNo17" };
        }
        else if (sheet == "Footer03")
        {
            opening = "Field47.Value";
            debit = "Field7.Value";
            credit = "Field8.Value";
            closing = "Field59.Value";
            keys = new[] { "RowNo02", "RowNo03" };
        }
        else
        {
            opening = "PocstavCelk1.Value";
            debit = "Field156.Value";
            credit = "Field157.Value";
            closing = "Field158.Value";
            keys = new[] { "RowNo02" };
        }

        while (reader.Read())
        {
            if (Blank(reader)) continue;

            rows.Add(new AccountAmounts(
                Key(reader, h, keys),
                Number(reader, h, opening),
                Number(reader, h, debit),
                Number(reader, h, credit),
                Number(reader, h, closing)));
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

        throw new InvalidDataException(
            "Normalized XML worksheet header not found.");
    }

    private static string Key(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        params string[] names)
    {
        return string.Join(
            ",",
            names.Select(name => name + "=" + Text(reader, h, name)));
    }

    private static string Text(
        IExcelDataReader reader,
        IReadOnlyDictionary<string, int> h,
        string name)
    {
        object? value = reader.GetValue(h[name]);

        if (value == null || value == DBNull.Value)
            return string.Empty;

        return Convert.ToString(
            value,
            CultureInfo.InvariantCulture) ?? string.Empty;
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

    private static bool Blank(IExcelDataReader reader)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            object? value = reader.GetValue(i);

            if (value != null &&
                value != DBNull.Value &&
                !(value is string text &&
                  string.IsNullOrWhiteSpace(text)))
            {
                return false;
            }
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
        Compact(group).StartsWith(
            "Celkom",
            StringComparison.OrdinalIgnoreCase);

    private static string Compact(BaselineGroup group) =>
        string.Concat(group.Tokens.Select(token => token.Text ?? string.Empty))
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty);

    private static string CompactValue(string value) =>
        string.Concat(value.Where(c =>
            !char.IsWhiteSpace(c) &&
            c != '\u00A0'));

    private static string RequireFile(string variable)
    {
        string? path = Environment.GetEnvironmentVariable(variable);

        Assert.False(
            string.IsNullOrWhiteSpace(path),
            "Set " + variable + ".");

        Assert.True(
            File.Exists(path),
            variable + " file does not exist: " + path);

        return path!;
    }

    private sealed record Glyph(
        char Character,
        double Left,
        double Right);

    private sealed record GlyphMatch(
        IReadOnlyList<Glyph> Glyphs);

    private sealed class ReferenceData
    {
        public List<DocumentAmounts> Documents { get; } = new();
        public List<AccountAmounts> Accounts { get; } = new();
        public List<AccountAmounts> Synthetics { get; } = new();
        public List<AccountAmounts> Totals { get; } = new();
    }

    private sealed record DocumentAmounts(
        string Key,
        decimal Debit,
        decimal Credit);

    private sealed record AccountAmounts(
        string Key,
        decimal Opening,
        decimal Debit,
        decimal Credit,
        decimal Closing);
}
