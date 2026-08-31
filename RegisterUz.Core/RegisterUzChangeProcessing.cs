namespace RegisterUz.Core;

public interface IRegisterUzEntityPackageLoader
{
    Task<RegisterUzLoadResult> LoadByEntityIdAsync(
        long entityId,
        CancellationToken cancellationToken = default);

    Task<RegisterUzLoadResult> LoadByEntityIdAsync(
        long entityId,
        RegisterUzCatalogPackage catalogs,
        bool synchronizeCatalogs,
        CancellationToken cancellationToken = default);
}

public sealed record RegisterUzObservationClaim(
    Guid LeaseToken,
    RegisterUzObjectType ObjectType,
    long RegisterUzObjectId,
    long ChangeObservationCount);

public sealed record RegisterUzResolvedObservation(
    RegisterUzObservationClaim Claim,
    long RegisterUzEntityId);

public sealed record RegisterUzEntityRefreshClaim(
    Guid LeaseToken,
    long RegisterUzEntityId,
    long RequestedGeneration);

public sealed record RegisterUzChangeProcessingResult(
    int ObservationsClaimed,
    int ObservationsResolved,
    int ObservationsFailed,
    int EntitiesClaimed,
    int EntitiesRefreshed,
    int EntitiesFailed);

public interface IRegisterUzChangeProcessingRepository
{
    Task<IReadOnlyList<RegisterUzObservationClaim>> ClaimObservationsAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task CompleteObservationAsync(
        RegisterUzResolvedObservation observation,
        CancellationToken cancellationToken = default);

    Task FailObservationAsync(
        RegisterUzObservationClaim observation,
        Exception exception,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegisterUzEntityRefreshClaim>> ClaimEntityRefreshesAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task CompleteEntityRefreshAsync(
        RegisterUzEntityRefreshClaim entity,
        long syncRunId,
        CancellationToken cancellationToken = default);

    Task FailEntityRefreshAsync(
        RegisterUzEntityRefreshClaim entity,
        Exception exception,
        CancellationToken cancellationToken = default);
}
