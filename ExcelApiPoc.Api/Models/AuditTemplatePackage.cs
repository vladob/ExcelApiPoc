namespace ExcelApiPoc.Api.Models;

public sealed class AuditTemplatePackage
{
    public int ContractVersion { get; init; }

    public DateTime GeneratedAtUtc { get; init; }

    public AuditTemplateDefinition Template { get; init; }
        = new();
}

public sealed class AuditTemplateDefinition
{
    public int TemplateErpId { get; init; }

    public string? Name { get; init; }

    public string? MfSpecification { get; init; }

    public DateOnly? ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public string? AccountingModel { get; init; }

    public IReadOnlyList<AuditReportTableDefinition> Tables { get; init; }
        = Array.Empty<AuditReportTableDefinition>();
}

public sealed class AuditReportTableDefinition
{
    public int TableErpId { get; init; }

    public string? NameSk { get; init; }

    public string? NameEn { get; init; }

    public int? NumberOfColumns { get; init; }

    public int? NumberOfDataColumns { get; init; }

    public bool DontHaveRowNumbers { get; init; }

    public IReadOnlyList<AuditReportHeaderDefinition> Headers { get; init; }
        = Array.Empty<AuditReportHeaderDefinition>();

    public IReadOnlyList<AuditReportRowDefinition> Rows { get; init; }
        = Array.Empty<AuditReportRowDefinition>();
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
    public int? RowNumber { get; init; }

    public string? Designation { get; init; }

    public string? TextSk { get; init; }

    public string? TextEn { get; init; }

    public bool IsSumRow { get; init; }

    public string? CategorySk { get; init; }
}