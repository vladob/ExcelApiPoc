using System;
using System.IO;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.IText.Recognition;

public sealed class OpenXpsLayoutRecognizer
{
    private readonly OpenXpsTokenExtractor _extractor = new OpenXpsTokenExtractor();
    private readonly LayoutRecognizer _recognizer = new LayoutRecognizer();

    public LayoutRecognitionResult Recognize(string filePath, LayoutDefinition definition)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        using var stream = File.OpenRead(filePath);
        return Recognize(stream, definition);
    }

    public LayoutRecognitionResult Recognize(Stream stream, LayoutDefinition definition) =>
        _recognizer.Recognize(_extractor.Extract(stream), definition);
}
