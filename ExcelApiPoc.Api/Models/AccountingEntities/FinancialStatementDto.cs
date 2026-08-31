namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record FinancialStatementDto
{
    public required long Id { get; init; }

    public string? PeriodFrom { get; init; }

    public string? PeriodTo { get; init; }

    public DateOnly? SubmissionDate { get; init; }

    public DateOnly? PreparationDate { get; init; }

    public DateOnly? ApprovalDate { get; init; }

    public DateOnly? AssemblyDate { get; init; }

    public DateOnly? AuditorReportAttachmentDate { get; init; }

    public string? FundName { get; init; }

    public string? LeiCode { get; init; }

    public bool? IsConsolidated { get; init; }

    public bool? IsConsolidatedCentralGovernment { get; init; }

    public bool? IsSummaryPublicAdministration { get; init; }

    public string? StatementType { get; init; }

    public required IReadOnlyList<FinancialReportDto> FinancialReports
    {
        get;
        init;
    }
}