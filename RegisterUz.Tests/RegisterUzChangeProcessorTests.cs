using RegisterUz.Core;
using RegisterUz.Sync;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzChangeProcessorTests
{
    [Fact]
    public async Task Processor_resolves_all_four_object_paths()
    {
        RegisterUzObservationClaim[] claims =
        [
            Claim(RegisterUzObjectType.AccountingEntity, 10),
            Claim(RegisterUzObjectType.FinancialStatement, 20),
            Claim(RegisterUzObjectType.AnnualReport, 30),
            Claim(RegisterUzObjectType.FinancialReport, 40),
            Claim(RegisterUzObjectType.FinancialReport, 41)
        ];
        var repository = new FakeRepository(claims, []);
        var client = new FakeClient
        {
            Statements =
            {
                [20] = new FinancialStatementDto { Id = 20, EntityId = 100 },
                [21] = new FinancialStatementDto { Id = 21, EntityId = 300 }
            },
            AnnualReports =
            {
                [30] = new AnnualReportDto { Id = 30, EntityId = 200 },
                [31] = new AnnualReportDto { Id = 31, EntityId = 400 }
            },
            FinancialReports =
            {
                [40] = new FinancialReportDto { Id = 40, FinancialStatementId = 21 },
                [41] = new FinancialReportDto { Id = 41, AnnualReportId = 31 }
            }
        };
        var processor = new RegisterUzChangeProcessor(client, repository, new FakeLoader());

        RegisterUzChangeProcessingResult result = await processor.ProcessAsync(
            10, 0, TimeSpan.FromMinutes(5));

        Assert.Equal(5, result.ObservationsResolved);
        Assert.Equal(0, result.ObservationsFailed);
        Assert.Equal(
            new long[] { 10, 100, 200, 300, 400 },
            repository.Resolved.Select(x => x.RegisterUzEntityId).ToArray());
    }

    [Fact]
    public async Task Processor_records_an_invalid_report_parent_as_a_resolution_failure()
    {
        RegisterUzObservationClaim claim = Claim(RegisterUzObjectType.FinancialReport, 40);
        var repository = new FakeRepository([claim], []);
        var client = new FakeClient
        {
            FinancialReports =
            {
                [40] = new FinancialReportDto
                {
                    Id = 40,
                    FinancialStatementId = 21,
                    AnnualReportId = 31
                }
            }
        };
        var processor = new RegisterUzChangeProcessor(client, repository, new FakeLoader());

        RegisterUzChangeProcessingResult result = await processor.ProcessAsync(
            1, 0, TimeSpan.FromMinutes(5));

        Assert.Equal(1, result.ObservationsFailed);
        Assert.Single(repository.FailedObservations);
        Assert.Empty(repository.Resolved);
    }

    [Fact]
    public async Task Processor_completes_and_fails_entity_refreshes_independently()
    {
        RegisterUzEntityRefreshClaim[] entities =
        [
            new(Guid.NewGuid(), 100, 3),
            new(Guid.NewGuid(), 200, 5)
        ];
        var repository = new FakeRepository([], entities);
        var loader = new FakeLoader(200);
        var processor = new RegisterUzChangeProcessor(new FakeClient(), repository, loader);

        RegisterUzChangeProcessingResult result = await processor.ProcessAsync(
            0, 10, TimeSpan.FromMinutes(5));

        Assert.Equal(2, result.EntitiesClaimed);
        Assert.Equal(1, result.EntitiesRefreshed);
        Assert.Equal(1, result.EntitiesFailed);
        Assert.Single(repository.CompletedEntities);
        Assert.Single(repository.FailedEntities);
        Assert.Equal(100, repository.CompletedEntities[0].RegisterUzEntityId);
    }

    private static RegisterUzObservationClaim Claim(RegisterUzObjectType type, long id) =>
        new(Guid.NewGuid(), type, id, 1);

    private sealed class FakeRepository(
        IReadOnlyList<RegisterUzObservationClaim> observations,
        IReadOnlyList<RegisterUzEntityRefreshClaim> entities)
        : IRegisterUzChangeProcessingRepository
    {
        public List<RegisterUzResolvedObservation> Resolved { get; } = [];
        public List<RegisterUzObservationClaim> FailedObservations { get; } = [];
        public List<RegisterUzEntityRefreshClaim> CompletedEntities { get; } = [];
        public List<RegisterUzEntityRefreshClaim> FailedEntities { get; } = [];

        public Task<IReadOnlyList<RegisterUzObservationClaim>> ClaimObservationsAsync(
            int batchSize, TimeSpan leaseDuration, CancellationToken cancellationToken = default) =>
            Task.FromResult(observations);

        public Task CompleteObservationAsync(
            RegisterUzResolvedObservation observation, CancellationToken cancellationToken = default)
        {
            Resolved.Add(observation);
            return Task.CompletedTask;
        }

        public Task FailObservationAsync(
            RegisterUzObservationClaim observation, Exception exception,
            CancellationToken cancellationToken = default)
        {
            FailedObservations.Add(observation);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RegisterUzEntityRefreshClaim>> ClaimEntityRefreshesAsync(
            int batchSize, TimeSpan leaseDuration, CancellationToken cancellationToken = default) =>
            Task.FromResult(entities);

        public Task CompleteEntityRefreshAsync(
            RegisterUzEntityRefreshClaim entity, long syncRunId,
            CancellationToken cancellationToken = default)
        {
            CompletedEntities.Add(entity);
            return Task.CompletedTask;
        }

        public Task FailEntityRefreshAsync(
            RegisterUzEntityRefreshClaim entity, Exception exception,
            CancellationToken cancellationToken = default)
        {
            FailedEntities.Add(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLoader(params long[] failingEntityIds) : IRegisterUzEntityPackageLoader
    {
        private readonly HashSet<long> _failingEntityIds = failingEntityIds.ToHashSet();

        public Task<RegisterUzLoadResult> LoadByEntityIdAsync(
            long entityId, CancellationToken cancellationToken = default)
        {
            if (_failingEntityIds.Contains(entityId))
                throw new InvalidOperationException("Synthetic load failure.");
            return Task.FromResult(new RegisterUzLoadResult(
                entityId.ToString(), entityId, 0, 0, 0, 0, 0,
                new RegisterUzCatalogSyncResult(0, 0, 0, 0, 0),
                entityId + 1000, DateTime.UtcNow));
        }
    }

    private sealed class FakeClient : IRegisterUzClient
    {
        public Dictionary<long, FinancialStatementDto> Statements { get; } = [];
        public Dictionary<long, AnnualReportDto> AnnualReports { get; } = [];
        public Dictionary<long, FinancialReportDto> FinancialReports { get; } = [];

        public Task<RegisterUzDocument<FinancialStatementDto>> GetFinancialStatementAsync(
            long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Document(Statements[id]));

        public Task<RegisterUzDocument<AnnualReportDto>> GetAnnualReportAsync(
            long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Document(AnnualReports[id]));

        public Task<RegisterUzDocument<FinancialReportDto>> GetFinancialReportAsync(
            long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Document(FinancialReports[id]));

        private static RegisterUzDocument<T> Document<T>(T value) =>
            new(value, "{}", DateTime.UtcNow, 200, "2.5");

        public Task<RegisterUzChangeFeedPage> GetChangedIdsAsync(RegisterUzObjectType objectType, DateTime changedSinceUtc, long? continueAfterId, int pageSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<long>> FindAccountingEntityIdsByIcoAsync(string ico, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<AccountingEntityDto>> GetAccountingEntityAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialReportTemplateDto>> GetTemplateAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzCatalogPackage> GetCatalogsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
