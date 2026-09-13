using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Recognition;

public sealed class LayoutRecognitionResult
{
    private readonly IReadOnlyList<SectionDiscoveryResult> _sections;
    private readonly IReadOnlyList<RecognizedRecord> _records;

    public LayoutRecognitionResult(
        PdfDocument document,
        IEnumerable<SectionDiscoveryResult> sections,
        BaselineRecordDiscoveryResult recordDiscovery,
        IEnumerable<RecognizedRecord> records)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));
        _sections = Array.AsReadOnly(sections?.ToArray() ?? throw new ArgumentNullException(nameof(sections)));
        RecordDiscovery = recordDiscovery ?? throw new ArgumentNullException(nameof(recordDiscovery));
        _records = Array.AsReadOnly(records?.ToArray() ?? throw new ArgumentNullException(nameof(records)));
    }

    public PdfDocument Document { get; }
    public IReadOnlyList<SectionDiscoveryResult> Sections => _sections;
    public BaselineRecordDiscoveryResult RecordDiscovery { get; }
    public IReadOnlyList<RecognizedRecord> Records => _records;
    public IReadOnlyList<RecognitionDiagnostic> Diagnostics => RecordDiscovery.Diagnostics;
}
