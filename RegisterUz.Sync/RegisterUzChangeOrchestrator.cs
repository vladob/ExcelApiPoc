using RegisterUz.Core;

namespace RegisterUz.Sync;

public interface IRegisterUzChangeFeedCollector
{
    Task<RegisterUzChangeFeedResult> CollectAsync(
        RegisterUzObjectType objectType,
        DateTime initialChangedSinceUtc,
        int pageSize,
        int maxPages,
        CancellationToken cancellationToken = default);
}

public interface IRegisterUzChangeProcessor
{
    Task<RegisterUzChangeProcessingResult> ProcessAsync(
        int observationBatchSize,
        int entityBatchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);
}

public sealed record RegisterUzChangeOrchestrationResult(
    IReadOnlyList<RegisterUzChangeFeedResult> Feeds,
    int ProcessingPasses,
    long ObservationsResolved,
    long EntitiesRefreshed,
    int ObservationFailures,
    int EntityFailures,
    bool WorkDrained,
    bool ProcessingPassLimitReached)
{
    public int CompletedFeedCount => Feeds.Count(x => x.FeedCompleted);
    public int PausedFeedCount => Feeds.Count - CompletedFeedCount;
    public bool HasFailures => ObservationFailures > 0 || EntityFailures > 0;
}

public sealed class RegisterUzChangeOrchestrator
{
    private static readonly RegisterUzObjectType[] ObjectTypes =
    [
        RegisterUzObjectType.AccountingEntity,
        RegisterUzObjectType.FinancialStatement,
        RegisterUzObjectType.FinancialReport,
        RegisterUzObjectType.AnnualReport
    ];

    private readonly IRegisterUzChangeFeedCollector _collector;
    private readonly IRegisterUzChangeProcessor _processor;

    public RegisterUzChangeOrchestrator(
        IRegisterUzChangeFeedCollector collector,
        IRegisterUzChangeProcessor processor)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public async Task<RegisterUzChangeOrchestrationResult> RunAsync(
        DateTime initialChangedSinceUtc,
        int pageSize,
        int maxPagesPerFeed,
        int observationBatchSize,
        int entityBatchSize,
        int maxProcessingPasses,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (initialChangedSinceUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Initial changed-since timestamp must be UTC.", nameof(initialChangedSinceUtc));
        if (pageSize is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (maxPagesPerFeed < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPagesPerFeed));
        if (observationBatchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(observationBatchSize));
        if (entityBatchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(entityBatchSize));
        if (maxProcessingPasses < 1)
            throw new ArgumentOutOfRangeException(nameof(maxProcessingPasses));
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));

        var feeds = new List<RegisterUzChangeFeedResult>(ObjectTypes.Length);
        foreach (RegisterUzObjectType objectType in ObjectTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            feeds.Add(await _collector.CollectAsync(
                objectType,
                initialChangedSinceUtc,
                pageSize,
                maxPagesPerFeed,
                cancellationToken));
        }

        long observationsResolved = 0;
        long entitiesRefreshed = 0;
        int observationFailures = 0;
        int entityFailures = 0;

        for (int pass = 1; pass <= maxProcessingPasses; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RegisterUzChangeProcessingResult result = await _processor.ProcessAsync(
                observationBatchSize,
                entityBatchSize,
                leaseDuration,
                cancellationToken);

            observationsResolved += result.ObservationsResolved;
            entitiesRefreshed += result.EntitiesRefreshed;
            observationFailures += result.ObservationsFailed;
            entityFailures += result.EntitiesFailed;

            if (result.ObservationsFailed > 0 || result.EntitiesFailed > 0)
            {
                return Result(pass, workDrained: false, passLimitReached: false);
            }

            if (result.ObservationsClaimed == 0 && result.EntitiesClaimed == 0)
            {
                return Result(pass, workDrained: true, passLimitReached: false);
            }
        }

        return Result(maxProcessingPasses, workDrained: false, passLimitReached: true);

        RegisterUzChangeOrchestrationResult Result(
            int passes,
            bool workDrained,
            bool passLimitReached) =>
            new(
                feeds,
                passes,
                observationsResolved,
                entitiesRefreshed,
                observationFailures,
                entityFailures,
                workDrained,
                passLimitReached);
    }
}
