namespace RegisterUz.Core;

public enum RegisterUzObjectType : byte
{
    AccountingEntity = 1,
    FinancialStatement = 2,
    FinancialReport = 3,
    AnnualReport = 4
}

public interface IRegisterUzRequestFailure
{
    string RequestPath { get; }
    DateTime RequestedAtUtc { get; }
    int? HttpStatusCode { get; }
    long? ResponseBytes { get; }
    string? ApiVersion { get; }
}

public sealed record RegisterUzChangeFeedPage(
    RegisterUzObjectType ObjectType,
    DateTime ChangedSinceUtc,
    long? ContinueAfterId,
    int PageSize,
    string RequestPath,
    DateTime RequestedAtUtc,
    RegisterUzDocument<IdListDto> Document);

public sealed record RegisterUzChangeFeedSession(
    long SyncRunId,
    RegisterUzObjectType ObjectType,
    DateTime ChangedSinceUtc,
    DateTime ScanStartedAtUtc,
    long? ContinueAfterId,
    int PageSize);

public sealed record RegisterUzChangeFeedPageSaveResult(
    bool FeedCompleted,
    long? ContinueAfterId);

public sealed record RegisterUzChangeFeedResult(
    RegisterUzObjectType ObjectType,
    long SyncRunId,
    DateTime ChangedSinceUtc,
    DateTime ScanStartedAtUtc,
    int PagesRetrieved,
    long ObservedIdCount,
    bool FeedCompleted,
    long? ContinueAfterId);

public interface IRegisterUzChangeFeedRepository
{
    Task<RegisterUzChangeFeedSession> BeginOrResumeAsync(
        RegisterUzObjectType objectType,
        DateTime initialChangedSinceUtc,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RegisterUzChangeFeedPageSaveResult> SavePageAsync(
        RegisterUzChangeFeedSession session,
        RegisterUzChangeFeedPage page,
        CancellationToken cancellationToken = default);

    Task CompleteBoundedRunAsync(
        RegisterUzChangeFeedSession session,
        int pagesRetrieved,
        long observedIdCount,
        CancellationToken cancellationToken = default);

    Task CancelRunAsync(
        RegisterUzChangeFeedSession session,
        CancellationToken cancellationToken = default);

    Task FailRunAsync(
        RegisterUzChangeFeedSession session,
        Exception exception,
        CancellationToken cancellationToken = default);
}
