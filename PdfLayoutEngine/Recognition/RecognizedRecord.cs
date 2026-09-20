using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Records;

namespace PdfLayoutEngine.Recognition;

public sealed class RecognizedRecord
{
    private readonly IReadOnlyList<RecordFieldMatchResult> _fields;

    public RecognizedRecord(
        BaselineRecord sourceRecord,
        BaselineRecordRuleMatchResult classification,
        IEnumerable<RecordFieldMatchResult> fields)
    {
        SourceRecord = sourceRecord ?? throw new ArgumentNullException(nameof(sourceRecord));
        Classification = classification ?? throw new ArgumentNullException(nameof(classification));
        _fields = Array.AsReadOnly(fields?.ToArray() ?? throw new ArgumentNullException(nameof(fields)));
        if (classification.Status != RuleMatchStatus.Matched && _fields.Count != 0)
            throw new ArgumentException(
                "Fields can only be recognized for a uniquely matched record.",
                nameof(fields));
    }

    public BaselineRecord SourceRecord { get; }
    public BaselineRecordRuleMatchResult Classification { get; }
    public RuleMatchStatus Status => Classification.Status;
    public string? RuleId => Status == RuleMatchStatus.Matched
        ? Classification.Matches[0].RuleId
        : null;
    public IReadOnlyList<RecordFieldMatchResult> Fields => _fields;
}
