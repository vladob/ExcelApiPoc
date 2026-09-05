namespace ExcelApiPoc.Api.Data;

public sealed record CalculationReportCandidate
{
    public long RegisterUzEntityId { get; init; }
    public string Ico { get; init; } = string.Empty;
    public string? EntityName { get; init; }
    public string? LegalFormCode { get; init; }
    public long FinancialStatementId { get; init; }
    public long FinancialReportId { get; init; }
    public int RegisterUzTemplateId { get; init; }
    public int TemplateId { get; init; }
    public int AccountFrameworkId { get; init; }
    public string FrameworkCode { get; init; } = string.Empty;
    public bool CalculationImplemented { get; init; }
}
