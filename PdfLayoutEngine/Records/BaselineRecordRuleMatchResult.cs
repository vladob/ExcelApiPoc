using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordRuleMatchResult
{
    private readonly IReadOnlyList<BaselineRecordRecognitionEvidence> _matches;

    public BaselineRecordRuleMatchResult(
        RuleMatchStatus status,
        IEnumerable<BaselineRecordRecognitionEvidence> matches)
    {
        Status = status;
        _matches = Array.AsReadOnly(matches?.ToArray() ?? throw new ArgumentNullException(nameof(matches)));
        if (status == RuleMatchStatus.Unmatched && _matches.Count != 0 ||
            status == RuleMatchStatus.Matched && _matches.Count != 1 ||
            status == RuleMatchStatus.Ambiguous && _matches.Count < 2)
            throw new ArgumentException("The number of matches is inconsistent with the result status.", nameof(matches));
    }

    public RuleMatchStatus Status { get; }
    public IReadOnlyList<BaselineRecordRecognitionEvidence> Matches => _matches;
}
