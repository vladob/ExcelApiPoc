namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record AccountingEntityDto
{
    public required long Id { get; init; }

    public required string Ico { get; init; }

    public string? Dic { get; init; }

    public string? Sid { get; init; }

    public string? Name { get; init; }

    public string? City { get; init; }

    public string? Street { get; init; }

    public string? PostalCode { get; init; }

    public DateOnly? EstablishedDate { get; init; }

    public DateOnly? CancellationDate { get; init; }

    public string? LegalFormCode { get; init; }

    public string? SkNaceCode { get; init; }

    public string? OrganizationSizeCode { get; init; }

    public string? OwnershipTypeCode { get; init; }

    public string? RegionCode { get; init; }

    public string? DistrictCode { get; init; }

    public string? RegisteredOfficeCode { get; init; }

    public bool? IsConsolidated { get; init; }
}