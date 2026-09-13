using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Recognition;

public sealed class RecognitionEvidence
{
    private readonly IReadOnlyList<PdfTextToken> _sourceTokens;

    public RecognitionEvidence(string ruleId, IEnumerable<PdfTextToken> sourceTokens, string explanation)
    {
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        _sourceTokens = Array.AsReadOnly(sourceTokens?.ToArray() ?? throw new ArgumentNullException(nameof(sourceTokens)));
        Explanation = explanation ?? throw new ArgumentNullException(nameof(explanation));
    }

    public string RuleId { get; }
    public IReadOnlyList<PdfTextToken> SourceTokens => _sourceTokens;
    public string Explanation { get; }
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
