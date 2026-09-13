using System;

namespace PdfLayoutEngine.Models;

public sealed class PdfTextToken
{
    public PdfTextToken(
        int pageNumber,
        string originalText,
        double left,
        double right,
        double baseline,
        bool isBold,
        bool isItalic)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));

        PageNumber = pageNumber;
        OriginalText = originalText ?? throw new ArgumentNullException(nameof(originalText));
        Text = TextNormalizer.Normalize(originalText);
        Left = left;
        Right = right;
        Baseline = baseline;
        IsBold = isBold;
        IsItalic = isItalic;
    }

    public int PageNumber { get; }
    public string OriginalText { get; }
    public string Text { get; }
    public double Left { get; }
    public double Right { get; }
    public double Baseline { get; }
    public bool IsBold { get; }
    public bool IsItalic { get; }
}
