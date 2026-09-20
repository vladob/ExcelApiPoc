using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfLayoutEngine.Recognition;

public enum RuleMatchStatus
{
    Matched,
    Unmatched,
    Ambiguous
}

public sealed class RuleMatchResult
{
    private readonly IReadOnlyList<RecognitionEvidence> _matches;

    public RuleMatchResult(string ruleId, RuleMatchStatus status, IEnumerable<RecognitionEvidence> matches)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        Status = status;
        _matches = Array.AsReadOnly(matches?.ToArray() ?? throw new ArgumentNullException(nameof(matches)));
    }

    public string RuleId { get; }
    public RuleMatchStatus Status { get; }
    public IReadOnlyList<RecognitionEvidence> Matches => _matches;
}
