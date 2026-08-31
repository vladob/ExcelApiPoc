namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record AnnualReportDto
{
    public required long Id { get; init; }

    public string? EntityNameAtSubmission { get; init; }

    public string? AnnualReportType { get; init; }

    public string? FundName { get; init; }

    public string? LeiCode { get; init; }

    public string? PeriodFrom { get; init; }

    public string? PeriodTo { get; init; }

    public DateOnly? SubmissionDate { get; init; }

    public DateOnly? AssemblyDate { get; init; }

    public required IReadOnlyList<AnnualReportAttachmentDto> Attachments
    {
        get;
        init;
    }

    public required IReadOnlyList<FinancialReportDto> FinancialReports
    {
        get;
        init;
    }
}