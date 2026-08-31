public sealed record FinancialReportTitlePageDto
{
    public string? EntityName { get; init; }

    public string? Ico { get; init; }

    public string? Dic { get; init; }

    public string? Sid { get; init; }

    public string? Address { get; init; }

    public string? LegalFormCode { get; init; }

    public string? SkNaceCode { get; init; }

    public string? ReportType { get; init; }

    public bool? IsConsolidated { get; init; }

    public bool? IsConsolidatedCentralGovernment { get; init; }

    public bool? IsSummaryPublicAdministration { get; init; }

    public string? EntityType { get; init; }

    public string? CommercialRegister { get; init; }

    public string? FundName { get; init; }

    public string? LeiCode { get; init; }

    public string? PeriodFrom { get; init; }

    public string? PeriodTo { get; init; }

    public string? PreviousPeriodFrom { get; init; }

    public string? PreviousPeriodTo { get; init; }

    public DateOnly? CompletionDate { get; init; }

    public DateOnly? ApprovalDate { get; init; }

    public DateOnly? PreparationDate { get; init; }

    public DateOnly? AssemblyDate { get; init; }

    public DateOnly? AuditorReportAttachmentDate { get; init; }
}