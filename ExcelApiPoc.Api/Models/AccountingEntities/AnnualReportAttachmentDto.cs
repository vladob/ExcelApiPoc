public sealed record AnnualReportAttachmentDto
{
    public required long Id { get; init; }

    public string? FileName { get; init; }

    public string? MimeType { get; init; }

    public long? FileSizeBytes { get; init; }

    public string? LanguageCode { get; init; }
}

public sealed record FinancialReportAttachmentDto
{
    public required long Id { get; init; }

    public string? FileName { get; init; }

    public string? MimeType { get; init; }

    public long? FileSizeBytes { get; init; }

    public int? PageCount { get; init; }

    public string? LanguageCode { get; init; }
}