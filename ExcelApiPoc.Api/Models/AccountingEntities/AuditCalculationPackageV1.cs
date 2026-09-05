using ExcelApiPoc.Api.Models;

namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record AuditCalculationPackageV1
{
    public int ContractVersion { get; init; } = 1;
    public required DateTimeOffset GeneratedAtUtc { get; init; }
    public required string Ico { get; init; }
    public required int FiscalYear { get; init; }
    public required long RegisterUzEntityId { get; init; }
    public string? EntityName { get; init; }
    public string? LegalFormCode { get; init; }
    public required long FinancialStatementId { get; init; }
    public required long FinancialReportId { get; init; }
    public required int RegisterUzTemplateId { get; init; }
    public required AuditTemplatePackageV2 CalculationPackage { get; init; }
}
