using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Diagnostics;

public enum RecognitionDiagnosticKind
{
    UnmatchedRule,
    AmbiguousRule,
    UnmatchedSectionBoundary,
    AmbiguousSectionBoundary,
    InvalidSectionBoundaryOrder,
    AmbiguousRecordContinuation
}

public sealed class RecognitionDiagnostic
{
    private readonly IReadOnlyList<PdfTextToken> _candidateTokens;

    public RecognitionDiagnostic(
        RecognitionDiagnosticKind kind,
        string ruleId,
        string message,
        IEnumerable<PdfTextToken>? candidateTokens = null)
    {
        Kind = kind;
        RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        _candidateTokens = Array.AsReadOnly(candidateTokens?.ToArray() ?? Array.Empty<PdfTextToken>());
    }

    public RecognitionDiagnosticKind Kind { get; }
    public string RuleId { get; }
    public string Message { get; }
    public IReadOnlyList<PdfTextToken> CandidateTokens => _candidateTokens;
}
