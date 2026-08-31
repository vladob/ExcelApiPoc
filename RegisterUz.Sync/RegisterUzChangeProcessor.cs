using RegisterUz.Core;

namespace RegisterUz.Sync;

public sealed class RegisterUzChangeProcessor
{
    private readonly IRegisterUzClient _client;
    private readonly IRegisterUzChangeProcessingRepository _repository;
    private readonly IRegisterUzEntityPackageLoader _loader;

    public RegisterUzChangeProcessor(
        IRegisterUzClient client,
        IRegisterUzChangeProcessingRepository repository,
        IRegisterUzEntityPackageLoader loader)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public async Task<RegisterUzChangeProcessingResult> ProcessAsync(
        int observationBatchSize,
        int entityBatchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (observationBatchSize < 0)
            throw new ArgumentOutOfRangeException(nameof(observationBatchSize));
        if (entityBatchSize < 0)
            throw new ArgumentOutOfRangeException(nameof(entityBatchSize));
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));

        int resolved = 0;
        int resolutionFailures = 0;
        IReadOnlyList<RegisterUzObservationClaim> observations = observationBatchSize == 0
            ? []
            : await _repository.ClaimObservationsAsync(
                observationBatchSize, leaseDuration, cancellationToken);

        foreach (RegisterUzObservationClaim observation in observations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                long entityId = await ResolveEntityIdAsync(observation, cancellationToken);
                await _repository.CompleteObservationAsync(
                    new RegisterUzResolvedObservation(observation, entityId), cancellationToken);
                resolved++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await _repository.FailObservationAsync(
                    observation, exception, CancellationToken.None);
                resolutionFailures++;
            }
        }

        int refreshed = 0;
        int refreshFailures = 0;
        IReadOnlyList<RegisterUzEntityRefreshClaim> entities = entityBatchSize == 0
            ? []
            : await _repository.ClaimEntityRefreshesAsync(
                entityBatchSize, leaseDuration, cancellationToken);

        RegisterUzCatalogPackage? catalogs = null;
        if (entities.Count > 0)
        {
            try
            {
                catalogs = await _client.GetCatalogsAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                foreach (RegisterUzEntityRefreshClaim entity in entities)
                {
                    await _repository.FailEntityRefreshAsync(
                        entity, exception, CancellationToken.None);
                    refreshFailures++;
                }

                return new RegisterUzChangeProcessingResult(
                    observations.Count,
                    resolved,
                    resolutionFailures,
                    entities.Count,
                    refreshed,
                    refreshFailures);
            }
        }

        bool catalogsSynchronized = false;
        foreach (RegisterUzEntityRefreshClaim entity in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                RegisterUzLoadResult result = await _loader.LoadByEntityIdAsync(
                    entity.RegisterUzEntityId,
                    catalogs!,
                    synchronizeCatalogs: !catalogsSynchronized,
                    cancellationToken: cancellationToken);
                await _repository.CompleteEntityRefreshAsync(
                    entity, result.SyncRunId, cancellationToken);
                catalogsSynchronized = true;
                refreshed++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await _repository.FailEntityRefreshAsync(
                    entity, exception, CancellationToken.None);
                refreshFailures++;
            }
        }

        return new RegisterUzChangeProcessingResult(
            observations.Count,
            resolved,
            resolutionFailures,
            entities.Count,
            refreshed,
            refreshFailures);
    }

    private async Task<long> ResolveEntityIdAsync(
        RegisterUzObservationClaim observation,
        CancellationToken cancellationToken)
    {
        long objectId = observation.RegisterUzObjectId;
        return observation.ObjectType switch
        {
            RegisterUzObjectType.AccountingEntity => objectId,
            RegisterUzObjectType.FinancialStatement =>
                ResolveStatement(await GetStatementAsync(objectId, cancellationToken), objectId),
            RegisterUzObjectType.AnnualReport =>
                ResolveAnnualReport(await GetAnnualReportAsync(objectId, cancellationToken), objectId),
            RegisterUzObjectType.FinancialReport =>
                await ResolveFinancialReportAsync(objectId, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(observation.ObjectType))
        };
    }

    private async Task<FinancialStatementDto> GetStatementAsync(
        long id,
        CancellationToken cancellationToken) =>
        (await _client.GetFinancialStatementAsync(id, cancellationToken)).Value;

    private async Task<AnnualReportDto> GetAnnualReportAsync(
        long id,
        CancellationToken cancellationToken) =>
        (await _client.GetAnnualReportAsync(id, cancellationToken)).Value;

    private static long ResolveStatement(FinancialStatementDto statement, long requestedId) =>
        ValidateParent(requestedId, statement.Id, statement.EntityId, "financial statement");

    private static long ResolveAnnualReport(AnnualReportDto report, long requestedId) =>
        ValidateParent(requestedId, report.Id, report.EntityId, "annual report");

    private async Task<long> ResolveFinancialReportAsync(
        long id,
        CancellationToken cancellationToken)
    {
        FinancialReportDto report =
            (await _client.GetFinancialReportAsync(id, cancellationToken)).Value;
        if (report.Id != id)
            throw new InvalidOperationException(
                $"RegisterUZ returned financial report {report.Id} when {id} was requested.");

        bool hasStatement = report.FinancialStatementId.HasValue;
        bool hasAnnualReport = report.AnnualReportId.HasValue;
        if (hasStatement == hasAnnualReport)
            throw new InvalidOperationException(
                $"Financial report {id} must have exactly one parent.");

        if (report.FinancialStatementId is long statementId)
        {
            FinancialStatementDto statement = await GetStatementAsync(statementId, cancellationToken);
            return ValidateParent(statementId, statement.Id, statement.EntityId, "financial statement");
        }

        long annualReportId = report.AnnualReportId!.Value;
        AnnualReportDto annualReport = await GetAnnualReportAsync(annualReportId, cancellationToken);
        return ValidateParent(annualReportId, annualReport.Id, annualReport.EntityId, "annual report");
    }

    private static long ValidateParent(
        long requestedId,
        long returnedId,
        long entityId,
        string objectName)
    {
        if (returnedId != requestedId)
            throw new InvalidOperationException(
                $"RegisterUZ returned {objectName} {returnedId} when {requestedId} was requested.");
        if (entityId <= 0)
            throw new InvalidOperationException(
                $"RegisterUZ {objectName} {requestedId} has no valid accounting entity.");
        return entityId;
    }
}
