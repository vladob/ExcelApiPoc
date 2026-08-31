namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record FinancialReportValueDto
{
    public required int ValueOrdinal { get; init; }

    public required int RowOrdinal { get; init; }

    public required int DataColumnOrdinal { get; init; }

    public required decimal NumericValue { get; init; }

    public required string SourceValue { get; init; }
}