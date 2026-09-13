using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Diagnostics;

namespace PdfLayoutEngine.Sections;

public enum SectionDiscoveryStatus
{
    Discovered,
    Unmatched,
    Ambiguous
}

public sealed class SectionDiscoveryResult
{
    private readonly IReadOnlyList<DiscoveredSection> _sections;
    private readonly IReadOnlyList<RecognitionDiagnostic> _diagnostics;

    public SectionDiscoveryResult(
        string sectionId,
        SectionDiscoveryStatus status,
        IEnumerable<DiscoveredSection> sections,
        IEnumerable<RecognitionDiagnostic> diagnostics)
    {
        SectionId = sectionId ?? throw new ArgumentNullException(nameof(sectionId));
        Status = status;
        _sections = Array.AsReadOnly(sections?.ToArray() ?? throw new ArgumentNullException(nameof(sections)));
        _diagnostics = Array.AsReadOnly(diagnostics?.ToArray() ?? throw new ArgumentNullException(nameof(diagnostics)));
    }

    public string SectionId { get; }
    public SectionDiscoveryStatus Status { get; }
    public IReadOnlyList<DiscoveredSection> Sections => _sections;
    public IReadOnlyList<RecognitionDiagnostic> Diagnostics => _diagnostics;
}
