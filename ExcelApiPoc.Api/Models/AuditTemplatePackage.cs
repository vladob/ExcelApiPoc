namespace ExcelApiPoc.Api.Models;

public sealed class AuditTemplatePackage
{
    public int ContractVersion { get; init; }
    public DateTime GeneratedAtUtc { get; init; }
    public AuditTemplateDefinition Template { get; init; } = new();
    public IReadOnlyList<AuditAccountGroupDefinition> AccountGroups { get; init; } = Array.Empty<AuditAccountGroupDefinition>();
    public IReadOnlyList<AuditReportMappingRuleDefinition> ReportMappingRules { get; init; } = Array.Empty<AuditReportMappingRuleDefinition>();
    public IReadOnlyList<AuditCalculationDependencyDefinition>
    CalculationPlan { get; init; } = Array.Empty<AuditCalculationDependencyDefinition>();
}

public sealed class AuditTemplateDefinition
{
    public int TemplateErpId { get; init; }
    public string? Name { get; init; }
    public string? MfSpecification { get; init; }
    public DateOnly? ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    public string? AccountingModel { get; init; }
    public IReadOnlyList<AuditReportTableDefinition> Tables { get; init; } = Array.Empty<AuditReportTableDefinition>();
}

public sealed class AuditReportTableDefinition
{
    public int TableErpId { get; init; }
    public int TableOrdinal { get; init; }
    public string? NameSk { get; init; }
    public string? NameEn { get; init; }
    public int? NumberOfColumns { get; init; }
    public int? NumberOfDataColumns { get; init; }
    public bool DontHaveRowNumbers { get; init; }
    public IReadOnlyList<AuditReportHeaderDefinition> Headers { get; init; } = Array.Empty<AuditReportHeaderDefinition>();
    public IReadOnlyList<AuditReportRowDefinition> Rows { get; init; } = Array.Empty<AuditReportRowDefinition>();
}

public sealed class AuditReportHeaderDefinition
{
    public string? TextSk { get; init; }
    public string? TextEn { get; init; }
    public int RowPosition { get; init; }
    public int ColumnPosition { get; init; }
    public int RowSpan { get; init; }
    public int ColumnSpan { get; init; }
}

public sealed class AuditReportRowDefinition
{
    public int RowOrdinal { get; init; }
    public int? RowNumber { get; init; }
    public string? Designation { get; init; }
    public string? TextSk { get; init; }
    public string? TextEn { get; init; }
    public bool IsSumRow { get; init; }
    public string? CategorySk { get; init; }
    public string? MappingCaptionSk { get; init; }
}

public sealed class AuditAccountGroupDefinition
{
    public string Account { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Legend { get; init; }
    public string? AssetsValueSource { get; init; }
    public string? LiabilitiesValueSource { get; init; }
}

public sealed class AuditReportMappingRuleDefinition
{
    public int TableErpId { get; init; }
    public string Account3 { get; init; } = string.Empty;
    public int ReportRowNumber { get; init; }
    public string AccountTitle { get; init; } = string.Empty;
    public bool RequiresAnalyticalMapping { get; init; }
    public bool IncludeInBrutto { get; init; }
    public bool IncludeInCorrection { get; init; }
    public string Side { get; init; } = string.Empty;
    public string ValueSource { get; init; } = string.Empty;
}

public sealed class AuditCalculationDependencyDefinition
{
    public int TargetTableErpId { get; init; }
    public int TargetRowNumber { get; init; }
    public int SourceTableErpId { get; init; }
    public int SourceRowNumber { get; init; }
    public int Coefficient { get; init; }
    public int CalculationLevel { get; init; }
}
