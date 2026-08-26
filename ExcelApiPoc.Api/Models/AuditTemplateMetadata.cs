namespace ExcelApiPoc.Api.Models;

public sealed class AuditTemplateMetadata
{
    public int TemplateErpId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? MfSpecification { get; init; }

    public DateOnly? ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public IReadOnlyList<AuditTableMetadata> Tables { get; init; }
        = Array.Empty<AuditTableMetadata>();
}

public sealed class AuditTableMetadata
{
    public int TableErpId { get; init; }

    public string NameSk { get; init; } = string.Empty;

    public string? NameEn { get; init; }

    public int NumberOfColumns { get; init; }

    public int NumberOfDataColumns { get; init; }

    public bool DontHaveRowNumbers { get; init; }
}