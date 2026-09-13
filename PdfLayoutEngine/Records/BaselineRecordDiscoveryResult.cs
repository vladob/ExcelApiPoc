using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Diagnostics;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordDiscoveryResult
{
    private readonly IReadOnlyList<BaselineRecord> _records;
    private readonly IReadOnlyList<RecognitionDiagnostic> _diagnostics;

    public BaselineRecordDiscoveryResult(
        IEnumerable<BaselineRecord> records,
        IEnumerable<RecognitionDiagnostic> diagnostics)
    {
        _records = Array.AsReadOnly(records?.ToArray() ?? throw new ArgumentNullException(nameof(records)));
        _diagnostics = Array.AsReadOnly(diagnostics?.ToArray() ?? throw new ArgumentNullException(nameof(diagnostics)));
    }

    public IReadOnlyList<BaselineRecord> Records => _records;
    public IReadOnlyList<RecognitionDiagnostic> Diagnostics => _diagnostics;
}
