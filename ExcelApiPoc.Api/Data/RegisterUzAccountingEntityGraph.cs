using ExcelApiPoc.Api.Models.AccountingEntities;

namespace ExcelApiPoc.Api.Data;

public sealed record RegisterUzAccountingEntityGraph
{
    public required AccountingEntityDto Entity { get; init; }

    public required IReadOnlyList<FinancialStatementDto> FinancialStatements
    {
        get;
        init;
    }

    public required IReadOnlyList<AnnualReportDto> AnnualReports
    {
        get;
        init;
    }
}