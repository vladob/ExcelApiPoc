using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;
using RegisterUz.Sync;

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

        RegisterUzAccountingEntityGraph? graph = await GetGraphAsync(ico, cancellationToken);

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

    public async Task<AuditCalculationPackageV1?> GetCalculationPackageAsync(
        string ico,
        int fiscalYear,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);
        RegisterUzAccountingEntityGraph? graph = await GetGraphAsync(ico, cancellationToken);
        if (graph is null)
            return null;

        var candidates = new List<AuditCalculationPackageCandidate>();

        foreach (FinancialStatementDto statement in graph.FinancialStatements.Where(
                     statement => AuditCalculationPackageSelectionPolicy.IsAuditedPeriod(statement, fiscalYear)))
        {
            foreach (FinancialReportDto report in statement.FinancialReports.Where(report => report.TemplateId.HasValue))
            {
                if (report.TemplateId!.Value is < int.MinValue or > int.MaxValue)
                    continue;

                int templateErpId = checked((int)report.TemplateId.Value);
                try
                {
                    AuditTemplatePackageV2 package = await _auditTemplateRepository.GetPackageV2Async(
                        templateErpId, fiscalYear, cancellationToken);
                    candidates.Add(new AuditCalculationPackageCandidate(
                        statement, report, templateErpId, package));
                }
                catch (AuditTemplatePackageV2ResolutionException exception) when (
                    AuditCalculationPackageSelectionPolicy.IsUnsupported(exception.Failure))
                {
                    // The absence of AuditAddIn configuration makes this a supporting/unsupported report.
                }
            }
        }

        AuditCalculationPackageCandidate selected =
            AuditCalculationPackageSelectionPolicy.SelectExactlyOne(candidates, ico, fiscalYear);
        string? titlePageLegalFormCode = selected.Report.TitlePage?.LegalFormCode;
        string? legalFormWarning = !string.IsNullOrWhiteSpace(graph.Entity.LegalFormCode) &&
            !string.IsNullOrWhiteSpace(titlePageLegalFormCode) &&
            !string.Equals(graph.Entity.LegalFormCode, titlePageLegalFormCode, StringComparison.Ordinal)
                ? $"Entity legal-form code '{graph.Entity.LegalFormCode}' differs from report legal-form code '{titlePageLegalFormCode}'."
                : null;

        return new AuditCalculationPackageV1
        {
            FinancialStatementId = selected.Statement.Id,
            FinancialReportId = selected.Report.Id,
            RegisterUzTemplateId = selected.TemplateErpId,
            LegalFormValidationWarning = legalFormWarning,
            CalculationPackage = selected.Package
        };
    }

    private async Task<RegisterUzAccountingEntityGraph?> GetGraphAsync(
        string ico,
        CancellationToken cancellationToken)
    {
        RegisterUzAccountingEntityGraph? graph =
            await _registerUzRepository.GetByIcoAsync(ico, cancellationToken);
        if (graph is not null)
            return graph;

        try
        {
            await _registerUzOnDemandLoadService.LoadByIcoAsync(ico, cancellationToken);
        }
        catch (RegisterUzAccountingEntityNotFoundException)
        {
            return null;
        }

        return await _registerUzRepository.GetByIcoAsync(ico, cancellationToken);
    }
}

public sealed class AuditCalculationPackageSelectionException : Exception
{
    public AuditCalculationPackageSelectionException(string message) : base(message) { }
}

public sealed class AuditCalculationPackageAmbiguousException : Exception
{
    public AuditCalculationPackageAmbiguousException(string message) : base(message) { }
}
