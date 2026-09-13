using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Sections;

public sealed class DiscoveredSection
{
    private readonly IReadOnlyList<PdfTextToken> _tokens;

    public DiscoveredSection(
        string sectionId,
        int startPageNumber,
        int endPageNumber,
        IEnumerable<PdfTextToken> tokens,
        RecognitionEvidence? startEvidence,
        RecognitionEvidence? endEvidence,
        bool mayContinueOnNextPage = false)
    {
        if (startPageNumber < 1) throw new ArgumentOutOfRangeException(nameof(startPageNumber));
        if (endPageNumber < startPageNumber) throw new ArgumentOutOfRangeException(nameof(endPageNumber));

        SectionId = sectionId ?? throw new ArgumentNullException(nameof(sectionId));
        StartPageNumber = startPageNumber;
        EndPageNumber = endPageNumber;
        _tokens = Array.AsReadOnly(tokens?.ToArray() ?? throw new ArgumentNullException(nameof(tokens)));
        StartEvidence = startEvidence;
        EndEvidence = endEvidence;
        MayContinueOnNextPage = mayContinueOnNextPage;
    }

    public string SectionId { get; }
    public int StartPageNumber { get; }
    public int EndPageNumber { get; }
    public IReadOnlyList<PdfTextToken> Tokens => _tokens;
    public RecognitionEvidence? StartEvidence { get; }
    public RecognitionEvidence? EndEvidence { get; }
    public bool MayContinueOnNextPage { get; }
}
