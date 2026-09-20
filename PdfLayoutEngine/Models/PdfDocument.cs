using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfLayoutEngine.Models;

public sealed class PdfDocument
{
    private readonly IReadOnlyList<PdfPage> _pages;
    private readonly IReadOnlyList<PdfTextToken> _tokens;

    public PdfDocument(IEnumerable<PdfPage> pages)
    {
        if (pages == null) throw new ArgumentNullException(nameof(pages));
        var pageArray = pages.OrderBy(page => page.PageNumber).ToArray();
        if (pageArray.Select(page => page.PageNumber).Distinct().Count() != pageArray.Length)
            throw new ArgumentException("Page numbers must be unique.", nameof(pages));

        _pages = Array.AsReadOnly(pageArray);
        _tokens = Array.AsReadOnly(pageArray.SelectMany(page => page.Tokens).ToArray());
    }

    public IReadOnlyList<PdfPage> Pages => _pages;
    public IReadOnlyList<PdfTextToken> Tokens => _tokens;
}
