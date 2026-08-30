using RegisterUz.Core;
using RegisterUz.Sync;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzChangeFeedCollectorTests
{
    private static readonly DateTime ChangedSince =
        new(2026, 8, 30, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ScanStarted =
        new(2026, 8, 30, 20, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Collector_saves_pages_and_completes_only_on_the_final_page()
    {
        var client = new FakeClient(
            Page([10, 20], true, null),
            Page([30], false, 20));
        var repository = new FakeRepository(Session());
        var collector = new RegisterUzChangeFeedCollector(client, repository);

        RegisterUzChangeFeedResult result = await collector.CollectAsync(
            RegisterUzObjectType.AccountingEntity, ChangedSince, 100, 5);

        Assert.True(result.FeedCompleted);
        Assert.Equal(2, result.PagesRetrieved);
        Assert.Equal(3, result.ObservedIdCount);
        Assert.Equal(2, repository.SavedPages.Count);
        Assert.False(repository.BoundedRunCompleted);
        Assert.False(repository.Failed);
    }

    [Fact]
    public async Task Collector_stops_at_the_bound_and_leaves_a_resume_cursor()
    {
        var client = new FakeClient(Page([10, 20], true, null));
        var repository = new FakeRepository(Session());
        var collector = new RegisterUzChangeFeedCollector(client, repository);

        RegisterUzChangeFeedResult result = await collector.CollectAsync(
            RegisterUzObjectType.AccountingEntity, ChangedSince, 100, 1);

        Assert.False(result.FeedCompleted);
        Assert.Equal(20, result.ContinueAfterId);
        Assert.True(repository.BoundedRunCompleted);
        Assert.False(repository.Failed);
    }

    [Fact]
    public async Task Collector_rejects_non_increasing_ids_and_marks_the_run_failed()
    {
        var client = new FakeClient(Page([10, 10], false, null));
        var repository = new FakeRepository(Session());
        var collector = new RegisterUzChangeFeedCollector(client, repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => collector.CollectAsync(
            RegisterUzObjectType.AccountingEntity, ChangedSince, 100, 1));

        Assert.Empty(repository.SavedPages);
        Assert.True(repository.Failed);
    }

    private static RegisterUzChangeFeedSession Session() =>
        new(42, RegisterUzObjectType.AccountingEntity, ChangedSince, ScanStarted, null, 100);

    private static RegisterUzChangeFeedPage Page(long[] ids, bool hasMore, long? continueAfterId)
    {
        var document = new RegisterUzDocument<IdListDto>(
            new IdListDto { Ids = ids, HasMoreIds = hasMore },
            "{}", ScanStarted.AddMinutes(1), 200, "2.5");
        return new RegisterUzChangeFeedPage(
            RegisterUzObjectType.AccountingEntity,
            ChangedSince,
            continueAfterId,
            100,
            "api/uctovne-jednotky",
            ScanStarted,
            document);
    }

    private sealed class FakeRepository(RegisterUzChangeFeedSession session)
        : IRegisterUzChangeFeedRepository
    {
        public List<RegisterUzChangeFeedPage> SavedPages { get; } = [];
        public bool BoundedRunCompleted { get; private set; }
        public bool Failed { get; private set; }
        public bool Cancelled { get; private set; }

        public Task<RegisterUzChangeFeedSession> BeginOrResumeAsync(
            RegisterUzObjectType objectType,
            DateTime initialChangedSinceUtc,
            int pageSize,
            CancellationToken cancellationToken = default) => Task.FromResult(session);

        public Task<RegisterUzChangeFeedPageSaveResult> SavePageAsync(
            RegisterUzChangeFeedSession currentSession,
            RegisterUzChangeFeedPage page,
            CancellationToken cancellationToken = default)
        {
            SavedPages.Add(page);
            long? next = page.Document.Value.HasMoreIds ? page.Document.Value.Ids[^1] : null;
            return Task.FromResult(new RegisterUzChangeFeedPageSaveResult(
                !page.Document.Value.HasMoreIds, next));
        }

        public Task CompleteBoundedRunAsync(
            RegisterUzChangeFeedSession currentSession,
            int pagesRetrieved,
            long observedIdCount,
            CancellationToken cancellationToken = default)
        {
            BoundedRunCompleted = true;
            return Task.CompletedTask;
        }

        public Task FailRunAsync(
            RegisterUzChangeFeedSession currentSession,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            Failed = true;
            return Task.CompletedTask;
        }

        public Task CancelRunAsync(
            RegisterUzChangeFeedSession currentSession,
            CancellationToken cancellationToken = default)
        {
            Cancelled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClient(params RegisterUzChangeFeedPage[] pages) : IRegisterUzClient
    {
        private readonly Queue<RegisterUzChangeFeedPage> _pages = new(pages);

        public Task<RegisterUzChangeFeedPage> GetChangedIdsAsync(
            RegisterUzObjectType objectType,
            DateTime changedSinceUtc,
            long? continueAfterId,
            int pageSize,
            CancellationToken cancellationToken = default) => Task.FromResult(_pages.Dequeue());

        public Task<IReadOnlyList<long>> FindAccountingEntityIdsByIcoAsync(string ico, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<AccountingEntityDto>> GetAccountingEntityAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialStatementDto>> GetFinancialStatementAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<AnnualReportDto>> GetAnnualReportAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialReportDto>> GetFinancialReportAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialReportTemplateDto>> GetTemplateAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzCatalogPackage> GetCatalogsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
