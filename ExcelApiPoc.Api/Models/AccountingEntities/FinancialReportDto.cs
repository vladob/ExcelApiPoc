namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record FinancialReportDto
{
    public required long Id { get; init; }

    public long? TemplateId { get; init; }

    public string? CurrencyCode { get; init; }

    public string? TaxOfficeCode { get; init; }

    public string? DataAvailability { get; init; }

    public FinancialReportTitlePageDto? TitlePage { get; init; }

    public required IReadOnlyList<FinancialReportAttachmentDto> Attachments
    {
        get;
        init;
    }

    public required IReadOnlyList<FinancialReportTableDto> Tables
    {
        get;
        init;
    }
}