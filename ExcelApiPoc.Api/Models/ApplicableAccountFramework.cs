namespace ExcelApiPoc.Api.Models;

public sealed class ApplicableAccountFramework
{
    public string FrameworkCode { get; init; } = string.Empty;
    public string FrameworkName { get; init; } = string.Empty;
    public int FrameworkVersionId { get; init; }
    public string VersionCode { get; init; } = string.Empty;
    public DateOnly ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }
    public string? LegalReference { get; init; }
    public string? SourceUrl { get; init; }
    public string? SourceSha256 { get; init; }
    public IReadOnlyList<AccountDefinitionMetadata> Definitions { get; init; } = Array.Empty<AccountDefinitionMetadata>();
    public IReadOnlyList<AccountRangeMetadata> Ranges { get; init; } = Array.Empty<AccountRangeMetadata>();
}

public sealed class AccountDefinitionMetadata
{
    public int AccountLevel { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string OfficialName { get; init; } = string.Empty;
}

public sealed class AccountRangeMetadata
{
    public int AccountLevel { get; init; }
    public string FromAccountCode { get; init; } = string.Empty;
    public string ToAccountCode { get; init; } = string.Empty;
    public string OfficialName { get; init; } = string.Empty;
}
