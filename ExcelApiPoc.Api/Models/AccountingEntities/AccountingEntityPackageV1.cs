using ExcelApiPoc.Api.Models;

namespace ExcelApiPoc.Api.Models.AccountingEntities;

public sealed record AccountingEntityPackageV1
{
    public int ContractVersion { get; init; } = 1;

    public required DateTimeOffset GeneratedAtUtc { get; init; }

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

    public required IReadOnlyList<AuditTemplatePackage> Templates
    {
        get;
        init;
    }

    public required IReadOnlyList<long> MissingTemplateIds
    {
        get;
        init;
    }
}
