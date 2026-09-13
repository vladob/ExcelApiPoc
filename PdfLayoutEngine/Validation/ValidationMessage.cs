namespace PdfLayoutEngine.Validation;

public enum ValidationSeverity
{
    Warning,
    Error
}

public sealed class ValidationMessage
{
    public ValidationMessage(ValidationSeverity severity, string path, string message)
    {
        Severity = severity;
        Path = path;
        Message = message;
    }

    public ValidationSeverity Severity { get; }
    public string Path { get; }
    public string Message { get; }
}
