using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;

namespace ExcelApiPoc.Api.Data;

public sealed class AccountingEntityPackageService
{
    private readonly RegisterUzAccountingEntityRepository
        _registerUzRepository;

    private readonly AuditTemplatePackageRepository
        _auditTemplateRepository;

    private readonly RegisterUzOnDemandLoadService
        _registerUzOnDemandLoadService;

    public AccountingEntityPackageService(
        RegisterUzAccountingEntityRepository registerUzRepository,
        AuditTemplatePackageRepository auditTemplateRepository,
        RegisterUzOnDemandLoadService registerUzOnDemandLoadService)
    {
        _registerUzRepository = registerUzRepository;
        _auditTemplateRepository = auditTemplateRepository;
        _registerUzOnDemandLoadService =
            registerUzOnDemandLoadService;
    }

    public async Task<AccountingEntityPackageV1?> GetPackageAsync(
        string ico,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);

        RegisterUzAccountingEntityGraph? graph =
            await _registerUzRepository.GetByIcoAsync(
                ico,
                cancellationToken);

        if (graph is null)
        {
            await _registerUzOnDemandLoadService.LoadByIcoAsync(
                ico,
                cancellationToken);

            graph =
                await _registerUzRepository.GetByIcoAsync(
                    ico,
                    cancellationToken);
        }

        if (graph is null)
        {
            return null;
        }

        long[] registerUzTemplateIds =
            graph.FinancialStatements
                .SelectMany(x => x.FinancialReports)
                .Concat(
                    graph.AnnualReports
                        .SelectMany(x => x.FinancialReports))
                .Where(x => x.TemplateId.HasValue)
                .Select(x => x.TemplateId!.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

        var templates =
            new List<AuditTemplatePackage>();

        var missingTemplateIds =
            new List<long>();

        foreach (long registerUzTemplateId in registerUzTemplateIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (registerUzTemplateId < int.MinValue ||
                registerUzTemplateId > int.MaxValue)
            {
                missingTemplateIds.Add(
                    registerUzTemplateId);

                continue;
            }

            AuditTemplatePackage? template =
                await _auditTemplateRepository.GetPackageAsync(
                    checked((int)registerUzTemplateId),
                    cancellationToken);

            if (template is null)
            {
                missingTemplateIds.Add(
                    registerUzTemplateId);

                continue;
            }

            templates.Add(template);
        }

        return new AccountingEntityPackageV1
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Entity = graph.Entity,
            FinancialStatements =
                graph.FinancialStatements,
            AnnualReports =
                graph.AnnualReports,
            Templates = templates,
            MissingTemplateIds =
                missingTemplateIds
        };
    }
}
