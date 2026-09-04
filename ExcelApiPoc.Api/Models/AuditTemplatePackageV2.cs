namespace ExcelApiPoc.Api.Models;

public sealed class AuditTemplatePackageV2
{
    public int ContractVersion { get; init; } = 4;
    public DateTime GeneratedAtUtc { get; init; }
    public string FrameworkCode { get; init; } = string.Empty;
    public string FrameworkVersionCode { get; init; } = string.Empty;
    public string CalculationConfigurationCode { get; init; } = string.Empty;
    public DateOnly ApplicableDate { get; init; }
    public AuditTemplateDefinition Template { get; init; } = new();
    public IReadOnlyList<AuditAccountGroupDefinition> AccountGroups { get; init; } = Array.Empty<AuditAccountGroupDefinition>();
    public IReadOnlyList<AuditReportMappingRuleDefinition> ReportMappingRules { get; init; } = Array.Empty<AuditReportMappingRuleDefinition>();
    public IReadOnlyList<AuditCalculationDependencyDefinition> CalculationPlan { get; init; } = Array.Empty<AuditCalculationDependencyDefinition>();
}
