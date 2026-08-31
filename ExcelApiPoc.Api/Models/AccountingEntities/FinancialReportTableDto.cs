namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record FinancialReportTableDto
{
    public required long Id { get; init; }

    public long? TemplateTableId { get; init; }

    public required int TableOrdinal { get; init; }

    public string? NameSk { get; init; }

    public string? NameEn { get; init; }

    public required IReadOnlyList<FinancialReportValueDto> Values
    {
        get;
        init;
    }
}