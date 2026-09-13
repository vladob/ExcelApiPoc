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
    private readonly IReadOnlyList<RecordContinuationEvidence> _continuations;

    public BaselineRecord(
        string sectionId,
        IEnumerable<BaselineGroup> groups,
        IEnumerable<RecordContinuationEvidence>? continuations = null)
    {
        SectionId = sectionId ?? throw new ArgumentNullException(nameof(sectionId));
        var groupArray = groups?.ToArray() ?? throw new ArgumentNullException(nameof(groups));
        if (groupArray.Length == 0)
            throw new ArgumentException("A record must contain at least one baseline group.", nameof(groups));
        var continuationArray = continuations?.ToArray() ?? Array.Empty<RecordContinuationEvidence>();
        if (continuationArray.Length != groupArray.Length - 1)
            throw new ArgumentException("A continuation evidence item is required between every joined group.", nameof(continuations));

        _groups = Array.AsReadOnly(groupArray);
        _sourceTokens = Array.AsReadOnly(groupArray.SelectMany(group => group.Tokens).ToArray());
        _continuations = Array.AsReadOnly(continuationArray);
        StartPageNumber = groupArray[0].PageNumber;
        EndPageNumber = groupArray[groupArray.Length - 1].PageNumber;
        Left = _sourceTokens.Min(token => token.Left);
        Right = _sourceTokens.Max(token => token.Right);
    }

    public string SectionId { get; }
    public int StartPageNumber { get; }
    public int EndPageNumber { get; }
    public bool CrossesPageBoundary => StartPageNumber != EndPageNumber;
    public double Left { get; }
    public double Right { get; }
    public IReadOnlyList<BaselineGroup> Groups => _groups;
    public IReadOnlyList<PdfTextToken> SourceTokens => _sourceTokens;
    public IReadOnlyList<RecordContinuationEvidence> Continuations => _continuations;
    public string Text => string.Join(" ", _groups.Select(group => group.Text));
}
