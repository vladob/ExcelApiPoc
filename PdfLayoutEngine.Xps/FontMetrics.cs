using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfLayoutEngine.V2;

namespace PdfLayoutEngine.Xps;
internal sealed class FontMetrics
{
    private readonly byte[] data;
    private readonly Dictionary<string, int> tables = new Dictionary<string, int>();
    private readonly int units, metrics, glyphs;
    public FontMetrics(byte[] data, string part)
    {
        this.data = data;
        if (part.EndsWith(".odttf", StringComparison.OrdinalIgnoreCase))
        {
            var name = Path.GetFileNameWithoutExtension(part).Replace("-", "");
            if (name.Length != 32 || data.Length < 32) throw new InvalidDataException("Invalid obfuscated font.");
            var key = Enumerable.Range(0, 16).Select(i => Convert.ToByte(name.Substring(i * 2, 2), 16)).Reverse().ToArray();
            for (int i = 0; i < 32; i++) data[i] ^= key[i % 16];
        }
        if (U32(0) != 0x00010000 && U32(0) != 0x4f54544f) throw new NotSupportedException("Only single OpenType/TrueType fonts are supported.");
        for (int i = 0; i < U16(4); i++)
        {
            int entry = 12 + i * 16; var tag = Encoding.ASCII.GetString(data, entry, 4);
            var offset = checked((int)U32(entry + 8)); var length = checked((int)U32(entry + 12));
            if (offset < 0 || length < 0 || offset > data.Length - length) throw new InvalidDataException("Invalid font table.");
            tables.Add(tag, offset);
        }
        units = U16(Table("head") + 18); metrics = U16(Table("hhea") + 34); glyphs = U16(Table("maxp") + 4);
        if (units == 0 || metrics == 0 || metrics > glyphs) throw new InvalidDataException("Invalid font metrics.");
    }
    private int Table(string name) => tables.TryGetValue(name, out var offset) ? offset : throw new InvalidDataException("Missing font table: " + name);
    private int U16(int p) { if (p < 0 || p > data.Length - 2) throw new InvalidDataException("Truncated font."); return data[p] * 256 + data[p + 1]; }
    private int I16(int p) => unchecked((short)U16(p));
    private uint U32(int p) => ((uint)U16(p) << 16) | (uint)U16(p + 2);
    public double Advance(int glyph, double em)
    { if (glyph < 0 || glyph >= glyphs) throw new InvalidDataException("Glyph index outside font."); return U16(Table("hmtx") + Math.Min(glyph, metrics - 1) * 4) * em / units; }
    public RectPt Bounds(int glyph, double em, double x, double y)
    {
        var scale = em / units;
        if (tables.ContainsKey("glyf") && tables.ContainsKey("loca"))
        {
            int loca = Table("loca"); bool wide = I16(Table("head") + 50) != 0;
            int offset = wide ? checked((int)U32(loca + glyph * 4)) : U16(loca + glyph * 2) * 2;
            int next = wide ? checked((int)U32(loca + (glyph + 1) * 4)) : U16(loca + (glyph + 1) * 2) * 2;
            if (offset == next) return new RectPt(x, y, x + Advance(glyph, em), y);
            int g = Table("glyf") + offset;
            return new RectPt(x + I16(g + 2) * scale, y - I16(g + 8) * scale, x + I16(g + 6) * scale, y - I16(g + 4) * scale);
        }
        return new RectPt(x, y - I16(Table("hhea") + 4) * scale, x + Advance(glyph, em), y - I16(Table("hhea") + 6) * scale);
    }
    public int Glyph(int codepoint)
    {
        int cmap = Table("cmap"); var offsets = new List<int>();
        for (int i = 0; i < U16(cmap + 2); i++)
        { int entry = cmap + 4 + i * 8; if (U16(entry) == 0 || U16(entry) == 3) offsets.Add(cmap + checked((int)U32(entry + 4))); }
        foreach (var p in offsets.OrderByDescending(p => U16(p)))
        {
            if (U16(p) == 12)
            {
                var count = checked((int)U32(p + 12));
                for (int i = 0; i < count; i++) { int g = p + 16 + i * 12; uint start = U32(g), end = U32(g + 4); if (codepoint >= start && codepoint <= end) return checked((int)(U32(g + 8) + codepoint - start)); }
            }
            if (U16(p) == 4 && codepoint <= 65535)
            {
                int count = U16(p + 6) / 2, ends = p + 14, starts = ends + count * 2 + 2, deltas = starts + count * 2, ranges = deltas + count * 2;
                for (int i = 0; i < count; i++)
                {
                    if (codepoint < U16(starts + i * 2) || codepoint > U16(ends + i * 2)) continue;
                    int range = U16(ranges + i * 2), delta = I16(deltas + i * 2);
                    if (range == 0) return (codepoint + delta) & 65535;
                    int value = U16(ranges + i * 2 + range + (codepoint - U16(starts + i * 2)) * 2); return value == 0 ? 0 : (value + delta) & 65535;
                }
            }
        }
        throw new InvalidDataException("Unicode character has no font mapping.");
    }
}
