using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordRecognitionEvidence
{
    private readonly IReadOnlyList<MatchCriterionEvidence> _criteria;

    public BaselineRecordRecognitionEvidence(
        string ruleId,
        BaselineRecord sourceRecord,
        IEnumerable<MatchCriterionEvidence> criteria)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        SourceRecord = sourceRecord ?? throw new ArgumentNullException(nameof(sourceRecord));
        _criteria = Array.AsReadOnly(criteria?.ToArray() ?? throw new ArgumentNullException(nameof(criteria)));
    }

    public string RuleId { get; }
    public BaselineRecord SourceRecord { get; }
    public IReadOnlyList<BaselineGroup> SourceGroups => SourceRecord.Groups;
    public IReadOnlyList<PdfTextToken> SourceTokens => SourceRecord.SourceTokens;
    public IReadOnlyList<MatchCriterionEvidence> Criteria => _criteria;
}
