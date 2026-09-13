namespace PdfLayoutEngine;

public static class TextNormalizer
{
    public static string Normalize(string text) => text.Replace('\u00a0', ' ');
}
