using RegisterUz.Core;
using RegisterUz.Sync;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzChangeOrchestratorTests
{
    private static readonly DateTime ChangedSince =
        new(2026, 8, 30, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Orchestrator_collects_all_feeds_and_drains_until_an_empty_pass()
    {
        var collector = new FakeCollector(completed: true);
        var processor = new FakeProcessor(
            Result(10, 10, 0, 3, 3, 0),
            Result(2, 2, 0, 1, 1, 0),
            Result(0, 0, 0, 0, 0, 0));
        var orchestrator = new RegisterUzChangeOrchestrator(collector, processor);

        RegisterUzChangeOrchestrationResult result = await orchestrator.RunAsync(
            ChangedSince, 100, 1, 10, 3, 10, TimeSpan.FromMinutes(15));

        Assert.Equal(4, collector.ObjectTypes.Count);
        Assert.Equal(4, result.CompletedFeedCount);
        Assert.Equal(0, result.PausedFeedCount);
        Assert.Equal(3, result.ProcessingPasses);
        Assert.Equal(12, result.ObservationsResolved);
        Assert.Equal(4, result.EntitiesRefreshed);
        Assert.True(result.WorkDrained);
        Assert.False(result.ProcessingPassLimitReached);
        Assert.False(result.HasFailures);
    }

    [Fact]
    public async Task Orchestrator_stops_immediately_after_a_processing_failure()
    {
        var processor = new FakeProcessor(
            Result(5, 4, 1, 2, 2, 0),
            Result(0, 0, 0, 0, 0, 0));
        var orchestrator = new RegisterUzChangeOrchestrator(
            new FakeCollector(completed: false), processor);

        RegisterUzChangeOrchestrationResult result = await orchestrator.RunAsync(
            ChangedSince, 100, 1, 5, 2, 10, TimeSpan.FromMinutes(15));

        Assert.Equal(1, processor.CallCount);
        Assert.Equal(4, result.PausedFeedCount);
        Assert.True(result.HasFailures);
        Assert.False(result.WorkDrained);
        Assert.False(result.ProcessingPassLimitReached);
    }

    [Fact]
    public async Task Orchestrator_reports_when_the_processing_pass_limit_is_reached()
    {
        var processor = new FakeProcessor(
            Result(5, 5, 0, 2, 2, 0),
            Result(5, 5, 0, 2, 2, 0));
        var orchestrator = new RegisterUzChangeOrchestrator(
            new FakeCollector(completed: true), processor);

        RegisterUzChangeOrchestrationResult result = await orchestrator.RunAsync(
            ChangedSince, 100, 1, 5, 2, 2, TimeSpan.FromMinutes(15));

        Assert.Equal(2, result.ProcessingPasses);
        Assert.False(result.WorkDrained);
        Assert.True(result.ProcessingPassLimitReached);
        Assert.False(result.HasFailures);
    }

    private static RegisterUzChangeProcessingResult Result(
        int claimedObservations,
        int resolvedObservations,
        int observationFailures,
        int claimedEntities,
        int refreshedEntities,
        int entityFailures) =>
        new(
            claimedObservations,
            resolvedObservations,
            observationFailures,
            claimedEntities,
            refreshedEntities,
            entityFailures);

    private sealed class FakeCollector(bool completed) : IRegisterUzChangeFeedCollector
    {
        public List<RegisterUzObjectType> ObjectTypes { get; } = [];

        public Task<RegisterUzChangeFeedResult> CollectAsync(
            RegisterUzObjectType objectType,
            DateTime initialChangedSinceUtc,
            int pageSize,
            int maxPages,
            CancellationToken cancellationToken = default)
        {
            ObjectTypes.Add(objectType);
            return Task.FromResult(new RegisterUzChangeFeedResult(
                objectType,
                (long)objectType,
                initialChangedSinceUtc,
                initialChangedSinceUtc.AddMinutes(1),
                1,
                0,
                completed,
                completed ? null : 100));
        }
    }

    private sealed class FakeProcessor(params RegisterUzChangeProcessingResult[] results)
        : IRegisterUzChangeProcessor
    {
        private readonly Queue<RegisterUzChangeProcessingResult> _results = new(results);
        public int CallCount { get; private set; }

        public Task<RegisterUzChangeProcessingResult> ProcessAsync(
            int observationBatchSize,
            int entityBatchSize,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_results.Dequeue());
        }
    }
}
