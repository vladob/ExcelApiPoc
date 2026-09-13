using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecord
{
    private readonly IReadOnlyList<BaselineGroup> _groups;
    private readonly IReadOnlyList<PdfTextToken> _sourceTokens;

    public BaselineRecord(string sectionId, IEnumerable<BaselineGroup> groups)
    {
        SectionId = sectionId ?? throw new ArgumentNullException(nameof(sectionId));
        var groupArray = groups?.ToArray() ?? throw new ArgumentNullException(nameof(groups));
        if (groupArray.Length == 0)
            throw new ArgumentException("A record must contain at least one baseline group.", nameof(groups));

        _groups = Array.AsReadOnly(groupArray);
        _sourceTokens = Array.AsReadOnly(groupArray.SelectMany(group => group.Tokens).ToArray());
        StartPageNumber = groupArray[0].PageNumber;
        EndPageNumber = groupArray[groupArray.Length - 1].PageNumber;
    }

    public string SectionId { get; }
    public int StartPageNumber { get; }
    public int EndPageNumber { get; }
    public bool CrossesPageBoundary => StartPageNumber != EndPageNumber;
    public IReadOnlyList<BaselineGroup> Groups => _groups;
    public IReadOnlyList<PdfTextToken> SourceTokens => _sourceTokens;
    public string Text => string.Join(" ", _groups.Select(group => group.Text));
}
