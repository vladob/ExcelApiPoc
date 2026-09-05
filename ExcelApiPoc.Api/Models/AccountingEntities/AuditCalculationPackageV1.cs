namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record AuditCalculationPackageV1
{
    public int ContractVersion { get; init; } = 1;
    public required long FinancialStatementId { get; init; }
    public required long FinancialReportId { get; init; }
    public required int RegisterUzTemplateId { get; init; }
    public string? LegalFormValidationWarning { get; init; }
    public required AuditTemplatePackageV2 CalculationPackage { get; init; }
}
