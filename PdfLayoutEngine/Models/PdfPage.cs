using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfLayoutEngine.Models;

public sealed class PdfPage
{
    private readonly IReadOnlyList<PdfTextToken> _tokens;

    public PdfPage(int pageNumber, IEnumerable<PdfTextToken> tokens)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));

        PageNumber = pageNumber;
        var tokenArray = tokens.ToArray();
        if (tokenArray.Any(token => token.PageNumber != pageNumber))
            throw new ArgumentException("Every token must belong to this page.", nameof(tokens));
        _tokens = Array.AsReadOnly(tokenArray);
    }

    public int PageNumber { get; }
    public IReadOnlyList<PdfTextToken> Tokens => _tokens;
}
