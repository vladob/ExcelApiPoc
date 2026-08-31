using RegisterUz.Core;
using RegisterUz.Sync;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzAccountingEntityLoaderTests
{
    [Fact]
    public async Task Manual_ico_load_records_single_ico_origin()
    {
        var client = new FakeClient(Entity(10, "00325554"));
        var repository = new FakeRepository();
        var loader = new RegisterUzAccountingEntityLoader(client, repository);

        await loader.LoadByIcoAsync("00325554");

        Assert.Equal(RegisterUzLoadOrigin.SingleIco, repository.Origin);
        Assert.Equal(1, repository.CatalogSaveCount);
    }

    [Fact]
    public async Task Queue_load_records_entity_refresh_origin_and_can_reuse_catalogs()
    {
        var client = new FakeClient(Entity(10, "00325554"));
        var repository = new FakeRepository();
        var loader = new RegisterUzAccountingEntityLoader(client, repository);

        await loader.LoadByEntityIdAsync(
            10, client.Catalogs, synchronizeCatalogs: false);

        Assert.Equal(RegisterUzLoadOrigin.EntityRefresh, repository.Origin);
        Assert.Equal(0, repository.CatalogSaveCount);
    }

    private static AccountingEntityDto Entity(long id, string ico) =>
        new() { Id = id, Ico = ico };

    private static RegisterUzDocument<T> Document<T>(T value) =>
        new(value, "{}", DateTime.UtcNow, 200, "2.5");

    private sealed class FakeClient(AccountingEntityDto entity) : IRegisterUzClient
    {
        public RegisterUzCatalogPackage Catalogs { get; } = new(
            Document(new TemplateCatalogDto()),
            Document(new ClassificationCatalogDto()),
            Document(new ClassificationCatalogDto()),
            Document(new ClassificationCatalogDto()),
            Document(new ClassificationCatalogDto()),
            Document(new LocationCatalogDto()),
            Document(new LocationCatalogDto()));

        public Task<IReadOnlyList<long>> FindAccountingEntityIdsByIcoAsync(
            string ico, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<long>>([entity.Id]);

        public Task<RegisterUzDocument<AccountingEntityDto>> GetAccountingEntityAsync(
            long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Document(entity));

        public Task<RegisterUzCatalogPackage> GetCatalogsAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(Catalogs);

        public Task<RegisterUzChangeFeedPage> GetChangedIdsAsync(RegisterUzObjectType objectType, DateTime changedSinceUtc, long? continueAfterId, int pageSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialStatementDto>> GetFinancialStatementAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<AnnualReportDto>> GetAnnualReportAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialReportDto>> GetFinancialReportAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RegisterUzDocument<FinancialReportTemplateDto>> GetTemplateAsync(long id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeRepository : IRegisterUzPackageRepository
    {
        public RegisterUzLoadOrigin? Origin { get; private set; }
        public int CatalogSaveCount { get; private set; }

        public Task<long> BeginRunAsync(
            string ico,
            RegisterUzLoadOrigin origin,
            CancellationToken cancellationToken = default)
        {
            Origin = origin;
            return Task.FromResult(42L);
        }

        public Task SavePackageAsync(
            long syncRunId,
            RegisterUzEntityPackage package,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<RegisterUzCatalogSyncResult> SaveCatalogsAsync(
            long syncRunId,
            RegisterUzCatalogPackage catalogs,
            CancellationToken cancellationToken = default)
        {
            CatalogSaveCount++;
            return Task.FromResult(new RegisterUzCatalogSyncResult(7, 0, 0, 0, 0));
        }

        public Task CompleteRunAsync(
            long syncRunId,
            RegisterUzLoadResult result,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task FailRunAsync(
            long syncRunId,
            Exception exception,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
