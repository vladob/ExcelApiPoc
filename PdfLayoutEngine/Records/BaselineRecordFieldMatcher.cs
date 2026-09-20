using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordFieldMatcher
{
    private readonly TokenRuleMatcher _tokenMatcher = new TokenRuleMatcher();

    public IReadOnlyList<RecordFieldMatchResult> Match(
        BaselineRecord record,
        RecordFieldSetDefinition fieldSet,
        MatchingDefaults? defaults = null)
    {
        if (record == null) throw new ArgumentNullException(nameof(record));
        if (fieldSet == null) throw new ArgumentNullException(nameof(fieldSet));

        return Array.AsReadOnly((fieldSet.Fields ?? new List<RecordFieldDefinition>())
            .Select(field => Match(record, field, defaults))
            .ToArray());
    }

    public RecordFieldMatchResult Match(
        BaselineRecord record,
        RecordFieldDefinition field,
        MatchingDefaults? defaults = null)
    {
        if (record == null) throw new ArgumentNullException(nameof(record));
        if (field == null) throw new ArgumentNullException(nameof(field));

        var rule = new RecognitionRuleDefinition
        {
            Id = field.Id,
            Text = field.Text,
            TextMatch = field.TextMatch,
            IgnoreCase = field.IgnoreCase,
            Horizontal = field.Horizontal,
            IsBold = field.IsBold,
            IsItalic = field.IsItalic
        };
        var match = _tokenMatcher.Match(rule, record.SourceTokens, defaults);
        var evidence = match.Status == RuleMatchStatus.Matched ? match.Matches[0] : null;
        var value = evidence == null
            ? null
            : new RecognizedValue<string>(evidence.SourceTokens[0].Text, evidence);
        return new RecordFieldMatchResult(field.Id, match.Status, match.Matches, value);
    }
}
