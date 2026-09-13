using System;
using System.IO;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Extraction;

namespace PdfLayoutEngine.IText.Recognition;

public sealed class ITextLayoutRecognizer
{
    private readonly ITextPdfTokenExtractor _extractor = new ITextPdfTokenExtractor();
    private readonly PdfLayoutEngine.Recognition.LayoutRecognizer _recognizer =
        new PdfLayoutEngine.Recognition.LayoutRecognizer();

    public PdfLayoutEngine.Recognition.LayoutRecognitionResult Recognize(
        string filePath,
        LayoutDefinition layout,
        string? password = null)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        return _recognizer.Recognize(
            _extractor.Extract(filePath, password),
            layout);
    }

    public PdfLayoutEngine.Recognition.LayoutRecognitionResult Recognize(
        Stream pdfStream,
        LayoutDefinition layout,
        string? password = null)
    {
        if (pdfStream == null) throw new ArgumentNullException(nameof(pdfStream));
        return _recognizer.Recognize(
            _extractor.Extract(pdfStream, password),
            layout);
    }
}
