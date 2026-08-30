namespace RegisterUz.Core;

public interface IRegisterUzClient
{
    Task<RegisterUzChangeFeedPage> GetChangedIdsAsync(
        RegisterUzObjectType objectType,
        DateTime changedSinceUtc,
        long? continueAfterId,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<long>> FindAccountingEntityIdsByIcoAsync(
        string ico,
        CancellationToken cancellationToken = default);

    Task<RegisterUzDocument<AccountingEntityDto>> GetAccountingEntityAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<RegisterUzDocument<FinancialStatementDto>> GetFinancialStatementAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<RegisterUzDocument<AnnualReportDto>> GetAnnualReportAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<RegisterUzDocument<FinancialReportDto>> GetFinancialReportAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<RegisterUzDocument<FinancialReportTemplateDto>> GetTemplateAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<RegisterUzCatalogPackage> GetCatalogsAsync(
        CancellationToken cancellationToken = default);
}

public interface IRegisterUzPackageRepository
{
    Task<long> BeginRunAsync(string ico, CancellationToken cancellationToken = default);

    Task SavePackageAsync(
        long syncRunId,
        RegisterUzEntityPackage package,
        CancellationToken cancellationToken = default);

    Task<RegisterUzCatalogSyncResult> SaveCatalogsAsync(
        long syncRunId,
        RegisterUzCatalogPackage catalogs,
        CancellationToken cancellationToken = default);

    Task CompleteRunAsync(
        long syncRunId,
        RegisterUzLoadResult result,
        CancellationToken cancellationToken = default);

    Task FailRunAsync(
        long syncRunId,
        Exception exception,
        CancellationToken cancellationToken = default);
}
