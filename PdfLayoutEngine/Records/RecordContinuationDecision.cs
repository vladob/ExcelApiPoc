using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public enum RecordContinuationDecisionStatus
{
    NotContinued,
    Continued,
    Ambiguous
}

public sealed class RecordContinuationEvidence
{
    private readonly IReadOnlyList<MatchCriterionEvidence> _criteria;

    public RecordContinuationEvidence(
        string definitionId,
        BaselineGroup previousGroup,
        BaselineGroup nextGroup,
        IEnumerable<MatchCriterionEvidence> criteria)
    {
        DefinitionId = definitionId ?? throw new ArgumentNullException(nameof(definitionId));
        PreviousGroup = previousGroup ?? throw new ArgumentNullException(nameof(previousGroup));
        NextGroup = nextGroup ?? throw new ArgumentNullException(nameof(nextGroup));
        _criteria = Array.AsReadOnly(criteria?.ToArray() ?? throw new ArgumentNullException(nameof(criteria)));
    }

    public string DefinitionId { get; }
    public BaselineGroup PreviousGroup { get; }
    public BaselineGroup NextGroup { get; }
    public IReadOnlyList<MatchCriterionEvidence> Criteria => _criteria;
}

public sealed class RecordContinuationDecision
{
    private readonly IReadOnlyList<RecordContinuationEvidence> _matches;

    public RecordContinuationDecision(
        RecordContinuationDecisionStatus status,
        IEnumerable<RecordContinuationEvidence>? matches = null)
    {
        Status = status;
        _matches = Array.AsReadOnly(matches?.ToArray() ?? Array.Empty<RecordContinuationEvidence>());
        if (status == RecordContinuationDecisionStatus.NotContinued && _matches.Count != 0 ||
            status == RecordContinuationDecisionStatus.Continued && _matches.Count != 1 ||
            status == RecordContinuationDecisionStatus.Ambiguous && _matches.Count < 2)
            throw new ArgumentException("The number of matches is inconsistent with the decision status.", nameof(matches));
    }

    public RecordContinuationDecisionStatus Status { get; }
    public IReadOnlyList<RecordContinuationEvidence> Matches => _matches;
}
