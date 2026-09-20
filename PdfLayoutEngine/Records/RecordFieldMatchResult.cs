using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class RecordFieldMatchResult
{
    private readonly IReadOnlyList<RecognitionEvidence> _matches;

    public RecordFieldMatchResult(
        string fieldId,
        RuleMatchStatus status,
        IEnumerable<RecognitionEvidence> matches,
        RecognizedValue<string>? value)
    {
        FieldId = fieldId ?? throw new ArgumentNullException(nameof(fieldId));
        Status = status;
        _matches = Array.AsReadOnly(matches?.ToArray() ?? throw new ArgumentNullException(nameof(matches)));
        Value = value;
        if (status == RuleMatchStatus.Unmatched && (_matches.Count != 0 || value != null) ||
            status == RuleMatchStatus.Matched && (_matches.Count != 1 || value == null) ||
            status == RuleMatchStatus.Ambiguous && (_matches.Count < 2 || value != null))
            throw new ArgumentException("Matches and value are inconsistent with the field status.", nameof(matches));
    }

    public string FieldId { get; }
    public RuleMatchStatus Status { get; }
    public IReadOnlyList<RecognitionEvidence> Matches => _matches;
    public RecognizedValue<string>? Value { get; }
}
