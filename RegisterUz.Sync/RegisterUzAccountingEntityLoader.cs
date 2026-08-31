using RegisterUz.Core;
using System.Text.Json;

namespace RegisterUz.Sync;

public sealed class RegisterUzAccountingEntityLoader : IRegisterUzEntityPackageLoader
{
    private readonly IRegisterUzClient _client;
    private readonly IRegisterUzPackageRepository _repository;

    public RegisterUzAccountingEntityLoader(
        IRegisterUzClient client,
        IRegisterUzPackageRepository repository)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RegisterUzLoadResult> LoadByIcoAsync(
        string ico,
        CancellationToken cancellationToken = default)
    {
        string normalizedIco = NormalizeIco(ico);
        IReadOnlyList<long> entityIds =
            await _client.FindAccountingEntityIdsByIcoAsync(normalizedIco, cancellationToken);

        if (entityIds.Count == 0)
            throw new InvalidOperationException($"No RegisterUZ accounting entity was found for IČO {normalizedIco}.");

        if (entityIds.Count != 1)
            throw new InvalidOperationException(
                $"RegisterUZ returned {entityIds.Count} accounting entities for IČO {normalizedIco}; exactly one is required.");

        RegisterUzDocument<AccountingEntityDto> entity =
            await _client.GetAccountingEntityAsync(entityIds[0], cancellationToken);

        if (!string.Equals(entity.Value.Ico, normalizedIco, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"RegisterUZ entity {entity.Value.Id} has IČO '{entity.Value.Ico}', not requested IČO '{normalizedIco}'.");

        RegisterUzCatalogPackage catalogs = await _client.GetCatalogsAsync(cancellationToken);
        return await LoadEntityAsync(
            normalizedIco, entity, catalogs,
            synchronizeCatalogs: true,
            cancellationToken: cancellationToken);
    }

    public async Task<RegisterUzLoadResult> LoadByEntityIdAsync(
        long entityId,
        CancellationToken cancellationToken = default)
    {
        if (entityId <= 0)
            throw new ArgumentOutOfRangeException(nameof(entityId));

        RegisterUzDocument<AccountingEntityDto> entity =
            await _client.GetAccountingEntityAsync(entityId, cancellationToken);
        if (entity.Value.Id != entityId)
            throw new InvalidOperationException(
                $"RegisterUZ returned entity {entity.Value.Id} when entity {entityId} was requested.");

        string normalizedIco = NormalizeIco(entity.Value.Ico ?? string.Empty);
        RegisterUzCatalogPackage catalogs = await _client.GetCatalogsAsync(cancellationToken);
        return await LoadEntityAsync(
            normalizedIco, entity, catalogs,
            synchronizeCatalogs: true,
            cancellationToken: cancellationToken);
    }

    public async Task<RegisterUzLoadResult> LoadByEntityIdAsync(
        long entityId,
        RegisterUzCatalogPackage catalogs,
        bool synchronizeCatalogs,
        CancellationToken cancellationToken = default)
    {
        if (entityId <= 0)
            throw new ArgumentOutOfRangeException(nameof(entityId));
        ArgumentNullException.ThrowIfNull(catalogs);

        RegisterUzDocument<AccountingEntityDto> entity =
            await _client.GetAccountingEntityAsync(entityId, cancellationToken);
        if (entity.Value.Id != entityId)
            throw new InvalidOperationException(
                $"RegisterUZ returned entity {entity.Value.Id} when entity {entityId} was requested.");

        string normalizedIco = NormalizeIco(entity.Value.Ico ?? string.Empty);
        return await LoadEntityAsync(
            normalizedIco, entity, catalogs,
            synchronizeCatalogs: synchronizeCatalogs,
            cancellationToken: cancellationToken);
    }

    private async Task<RegisterUzLoadResult> LoadEntityAsync(
        string normalizedIco,
        RegisterUzDocument<AccountingEntityDto> entity,
        RegisterUzCatalogPackage catalogs,
        bool synchronizeCatalogs,
        CancellationToken cancellationToken)
    {
        long syncRunId = await _repository.BeginRunAsync(normalizedIco, cancellationToken);

        try
        {
            RegisterUzCatalogSyncResult catalogResult = synchronizeCatalogs
                ? await _repository.SaveCatalogsAsync(syncRunId, catalogs, cancellationToken)
                : new RegisterUzCatalogSyncResult(0, 0, 0, 0, 0);

            var statements = new List<RegisterUzDocument<FinancialStatementDto>>();
            foreach (long id in entity.Value.FinancialStatementIds.Distinct())
            {
                RegisterUzDocument<FinancialStatementDto> statement =
                    await _client.GetFinancialStatementAsync(id, cancellationToken);
                if (statement.Value.EntityId != entity.Value.Id)
                    throw new InvalidOperationException($"Financial statement {id} belongs to entity {statement.Value.EntityId}, not {entity.Value.Id}.");
                statements.Add(statement);
            }

            var annualReports = new List<RegisterUzDocument<AnnualReportDto>>();
            foreach (long id in entity.Value.AnnualReportIds.Distinct())
            {
                RegisterUzDocument<AnnualReportDto> annualReport =
                    await _client.GetAnnualReportAsync(id, cancellationToken);
                if (annualReport.Value.EntityId != entity.Value.Id)
                    throw new InvalidOperationException($"Annual report {id} belongs to entity {annualReport.Value.EntityId}, not {entity.Value.Id}.");
                annualReports.Add(annualReport);
            }

            var reportParents = new Dictionary<long, (long? StatementId, long? AnnualReportId)>();
            foreach (RegisterUzDocument<FinancialStatementDto> statement in statements)
            {
                foreach (long reportId in statement.Value.FinancialReportIds.Distinct())
                    AddExpectedParent(reportParents, reportId, statement.Value.Id, null);
            }

            foreach (RegisterUzDocument<AnnualReportDto> annualReport in annualReports)
            {
                foreach (long reportId in annualReport.Value.FinancialReportIds.Distinct())
                    AddExpectedParent(reportParents, reportId, null, annualReport.Value.Id);
            }

            var reports = new List<RegisterUzDocument<FinancialReportDto>>();
            foreach (KeyValuePair<long, (long? StatementId, long? AnnualReportId)> pair in reportParents.OrderBy(x => x.Key))
            {
                long reportId = pair.Key;
                long? statementId = pair.Value.StatementId;
                long? annualReportId = pair.Value.AnnualReportId;
                RegisterUzDocument<FinancialReportDto> report =
                    await _client.GetFinancialReportAsync(reportId, cancellationToken);

                ValidateExactlyOneParent(report.Value);
                if (report.Value.FinancialStatementId != statementId || report.Value.AnnualReportId != annualReportId)
                    throw new InvalidOperationException($"Financial report {reportId} returned a parent different from its containing object.");

                reports.Add(report);
            }

            var templates = new List<RegisterUzDocument<FinancialReportTemplateDto>>();
            int templateDetailRequestCount = 0;
            IReadOnlyDictionary<long, FinancialReportTemplateDto> catalogTemplates =
                catalogs.Templates.Value.Templates.ToDictionary(template => template.Id);
            foreach (long templateId in reports
                         .Select(report => report.Value.TemplateId)
                         .Where(id => id.HasValue)
                         .Select(id => id!.Value)
                         .Distinct()
                         .OrderBy(id => id))
            {
                if (catalogTemplates.TryGetValue(templateId, out FinancialReportTemplateDto? template))
                {
                    templates.Add(new RegisterUzDocument<FinancialReportTemplateDto>(
                        template,
                        JsonSerializer.Serialize(template),
                        catalogs.Templates.RetrievedAtUtc,
                        catalogs.Templates.HttpStatusCode,
                        catalogs.Templates.ApiVersion));
                }
                else
                {
                    templates.Add(await _client.GetTemplateAsync(templateId, cancellationToken));
                    templateDetailRequestCount++;
                }
            }

            var package = new RegisterUzEntityPackage(entity, statements, annualReports, reports, templates);
            await _repository.SavePackageAsync(syncRunId, package, cancellationToken);

            var result = new RegisterUzLoadResult(
                normalizedIco,
                entity.Value.Id,
                statements.Count,
                annualReports.Count,
                reports.Count,
                templates.Count,
                templateDetailRequestCount,
                catalogResult,
                syncRunId,
                DateTime.UtcNow);

            await _repository.CompleteRunAsync(syncRunId, result, cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            await _repository.FailRunAsync(syncRunId, exception, CancellationToken.None);
            throw;
        }
    }

    private static string NormalizeIco(string ico)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);
        string normalized = ico.Trim();
        if (normalized.Length is < 1 or > 20 || normalized.Any(character => !char.IsDigit(character)))
            throw new ArgumentException("IČO must contain only digits and have at most 20 characters.", nameof(ico));
        return normalized.PadLeft(8, '0');
    }

    private static void AddExpectedParent(
        IDictionary<long, (long? StatementId, long? AnnualReportId)> parents,
        long reportId,
        long? statementId,
        long? annualReportId)
    {
        var proposed = (statementId, annualReportId);
        if (parents.TryGetValue(reportId, out var existing) && existing != proposed)
            throw new InvalidOperationException($"Financial report {reportId} is declared by more than one parent.");
        parents[reportId] = proposed;
    }

    private static void ValidateExactlyOneParent(FinancialReportDto report)
    {
        bool hasStatement = report.FinancialStatementId.HasValue;
        bool hasAnnualReport = report.AnnualReportId.HasValue;
        if (hasStatement == hasAnnualReport)
            throw new InvalidOperationException(
                $"Financial report {report.Id} must have exactly one parent, but returned " +
                $"statement={report.FinancialStatementId?.ToString() ?? "null"}, " +
                $"annualReport={report.AnnualReportId?.ToString() ?? "null"}.");
    }
}
