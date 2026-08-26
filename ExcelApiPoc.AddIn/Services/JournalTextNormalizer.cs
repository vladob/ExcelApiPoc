using System;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class JournalTextNormalizer
    {
        public static string NormalizeText(string value, out bool changed)
        {
            string original = value ?? string.Empty;
            string normalized = original
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .Replace('\t', ' ');

            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }
            normalized = normalized.Trim();
            changed = !string.Equals(original, normalized, StringComparison.Ordinal);

            return normalized;
        }
    }
}