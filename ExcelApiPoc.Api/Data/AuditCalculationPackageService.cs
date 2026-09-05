using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;
using RegisterUz.Sync;

namespace ExcelApiPoc.Api.Data;

public sealed class AuditCalculationPackageService
{
    private readonly CalculationReportCandidateRepository _candidateRepository;
    private readonly AuditTemplatePackageRepository _templatePackageRepository;
    private readonly RegisterUzOnDemandLoadService _onDemandLoadService;

    public AuditCalculationPackageService(
        CalculationReportCandidateRepository candidateRepository,
        AuditTemplatePackageRepository templatePackageRepository,
        RegisterUzOnDemandLoadService onDemandLoadService)
    {
        _candidateRepository = candidateRepository;
        _templatePackageRepository = templatePackageRepository;
        _onDemandLoadService = onDemandLoadService;
    }

    public async Task<AuditCalculationPackageV1?> GetAsync(
        string ico,
        int fiscalYear,
        CancellationToken cancellationToken)
    {
        if (!await _candidateRepository.AccountingEntityExistsAsync(
                ico,
                cancellationToken))
        {
            try
            {
                await _onDemandLoadService.LoadByIcoAsync(
                    ico,
                    cancellationToken);
            }
            catch (RegisterUzAccountingEntityNotFoundException)
            {
                return null;
            }

            if (!await _candidateRepository.AccountingEntityExistsAsync(
                    ico,
                    cancellationToken))
            {
                return null;
            }
        }

        IReadOnlyList<CalculationReportCandidate> candidates =
            await _candidateRepository.GetAsync(
                ico,
                fiscalYear,
                cancellationToken);

        CalculationReportCandidate candidate =
            CalculationReportCandidateSelection.SelectExactlyOne(
                candidates,
                ico,
                fiscalYear);

        AuditTemplatePackageV2 package =
            await _templatePackageRepository.GetPackageV2Async(
                candidate.RegisterUzTemplateId,
                candidate.FrameworkCode,
                fiscalYear,
                cancellationToken);

        if (package.AccountFrameworkId != candidate.AccountFrameworkId)
        {
            throw new AuditTemplatePackageV2ResolutionException(
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration,
                "The calculation-template mapping and resolved package reference " +
                "different accounting frameworks.");
        }

        return new AuditCalculationPackageV1
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Ico = candidate.Ico,
            FiscalYear = fiscalYear,
            RegisterUzEntityId = candidate.RegisterUzEntityId,
            EntityName = candidate.EntityName,
            LegalFormCode = candidate.LegalFormCode,
            FinancialStatementId = candidate.FinancialStatementId,
            FinancialReportId = candidate.FinancialReportId,
            RegisterUzTemplateId = candidate.RegisterUzTemplateId,
            CalculationPackage = package
        };
    }
}
