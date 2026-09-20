using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Grouping;

public sealed class BaselineGroup
{
    private readonly IReadOnlyList<PdfTextToken> _tokens;

    public BaselineGroup(int pageNumber, IEnumerable<PdfTextToken> tokens)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));

        var tokenArray = tokens.OrderBy(token => token.Left).ToArray();
        if (tokenArray.Length == 0)
            throw new ArgumentException("A baseline group must contain at least one token.", nameof(tokens));
        if (tokenArray.Any(token => token.PageNumber != pageNumber))
            throw new ArgumentException("Every token must belong to this page.", nameof(tokens));

        PageNumber = pageNumber;
        _tokens = Array.AsReadOnly(tokenArray);
        Baseline = tokenArray.Average(token => token.Baseline);
        Left = tokenArray.Min(token => token.Left);
        Right = tokenArray.Max(token => token.Right);
        Text = string.Join(" ", tokenArray
            .Select(token => token.Text.Trim())
            .Where(text => text.Length > 0));
    }

    public int PageNumber { get; }
    public double Baseline { get; }
    public double Left { get; }
    public double Right { get; }
    public string Text { get; }
    public IReadOnlyList<PdfTextToken> Tokens => _tokens;
}
