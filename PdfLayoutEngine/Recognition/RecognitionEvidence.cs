using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Recognition;

public sealed class RecognitionEvidence
{
    private readonly IReadOnlyList<PdfTextToken> _sourceTokens;
    private readonly IReadOnlyList<MatchCriterionEvidence> _criteria;

    public RecognitionEvidence(
        string ruleId,
        IEnumerable<PdfTextToken> sourceTokens,
        string explanation,
        IEnumerable<MatchCriterionEvidence>? criteria = null)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        _sourceTokens = Array.AsReadOnly(sourceTokens?.ToArray() ?? throw new ArgumentNullException(nameof(sourceTokens)));
        Explanation = explanation ?? throw new ArgumentNullException(nameof(explanation));
        _criteria = Array.AsReadOnly(criteria?.ToArray() ?? Array.Empty<MatchCriterionEvidence>());
    }

    public string RuleId { get; }
    public IReadOnlyList<PdfTextToken> SourceTokens => _sourceTokens;
    public string Explanation { get; }
    public IReadOnlyList<MatchCriterionEvidence> Criteria => _criteria;
}

public sealed class MatchCriterionEvidence
{
    public MatchCriterionEvidence(
        string criterion,
        bool isMatch,
        string expected,
        string actual,
        double? tolerance = null)
    {
        Criterion = criterion ?? throw new ArgumentNullException(nameof(criterion));
        IsMatch = isMatch;
        Expected = expected ?? throw new ArgumentNullException(nameof(expected));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
        Tolerance = tolerance;
    }

    public string Criterion { get; }
    public bool IsMatch { get; }
    public string Expected { get; }
    public string Actual { get; }
    public double? Tolerance { get; }
}

public sealed class RecognizedValue<T>
{
    public RecognizedValue(T value, RecognitionEvidence evidence)
    {
        Value = value;
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
    }

    public T Value { get; }
    public RecognitionEvidence Evidence { get; }
}
