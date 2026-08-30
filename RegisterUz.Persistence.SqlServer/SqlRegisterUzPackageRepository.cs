using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using RegisterUz.Core;

namespace RegisterUz.Persistence.SqlServer;

public sealed class SqlRegisterUzPackageRepository : IRegisterUzPackageRepository
{
    private const byte AccountingEntityObjectType = 1;
    private const byte FinancialStatementObjectType = 2;
    private const byte FinancialReportObjectType = 3;
    private const byte AnnualReportObjectType = 4;

    private enum PersistenceOutcome : byte
    {
        Unchanged = 0,
        Inserted = 1,
        Updated = 2
    }

    private sealed record RawSaveResult(long PayloadVersionId, PersistenceOutcome Outcome);

    private enum CatalogItemOutcome : byte
    {
        Unchanged = 0,
        Inserted = 1,
        Updated = 2,
        Reappeared = 3
    }

    private sealed record CatalogCounters(
        int Inserted,
        int Updated,
        int Removed);

    private readonly string _connectionString;

    public SqlRegisterUzPackageRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("RegisterUZ connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public async Task<long> BeginRunAsync(string ico, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO [Sync].[Run] ([RunType], [RequestedBy], [Notes])
            OUTPUT INSERTED.[SyncRunId]
            VALUES ('SingleIco', ORIGINAL_LOGIN(), N'IČO: ' + @Ico);

            IF NOT EXISTS (SELECT 1 FROM [Sync].[LoadTarget] WHERE [Ico] = @Ico)
                INSERT INTO [Sync].[LoadTarget] ([Ico], [RequestedBy]) VALUES (@Ico, ORIGINAL_LOGIN());
            ELSE
                UPDATE [Sync].[LoadTarget]
                SET [LastAttemptAtUtc] = SYSUTCDATETIME(), [LastStatus] = 'Running',
                    [LastError] = NULL, [UpdatedAtUtc] = SYSUTCDATETIME()
                WHERE [Ico] = @Ico;
            """;
        Add(command, "@Ico", SqlDbType.VarChar, ico, 20);
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    public async Task SavePackageAsync(
        long syncRunId,
        RegisterUzEntityPackage package,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            long insertedCount = 0;
            long updatedCount = 0;
            long unchangedCount = 0;

            foreach (RegisterUzDocument<FinancialReportTemplateDto> template in package.Templates)
                await SaveTemplateAsync(connection, transaction, template, cancellationToken);

            RawSaveResult entityRaw = await SaveRawAsync(
                connection, transaction, syncRunId, AccountingEntityObjectType,
                package.Entity.Value.Id, package.Entity.Value.LastModifiedDate,
                package.Entity.Value.Status, package.Entity, cancellationToken);
            Count(entityRaw.Outcome, ref insertedCount, ref updatedCount, ref unchangedCount);

            await EnsureEntityReferencesAsync(connection, transaction, package.Entity.Value, package.Entity.RetrievedAtUtc, cancellationToken);
            await SaveEntityAsync(connection, transaction, package.Entity, entityRaw.PayloadVersionId, cancellationToken);
            await ObserveAsync(connection, transaction, syncRunId, AccountingEntityObjectType,
                package.Entity.Value.Id, package.Entity.Value.LastModifiedDate,
                IsDeleted(package.Entity.Value.Status), package.Entity.RetrievedAtUtc, cancellationToken);

            foreach (RegisterUzDocument<FinancialStatementDto> statement in package.FinancialStatements)
            {
                RawSaveResult raw = await SaveRawAsync(
                    connection, transaction, syncRunId, FinancialStatementObjectType,
                    statement.Value.Id, statement.Value.LastModifiedDate,
                    statement.Value.Status, statement, cancellationToken);
                Count(raw.Outcome, ref insertedCount, ref updatedCount, ref unchangedCount);
                await SaveStatementAsync(connection, transaction, statement, raw.PayloadVersionId, cancellationToken);
                await ObserveAsync(connection, transaction, syncRunId, FinancialStatementObjectType,
                    statement.Value.Id, statement.Value.LastModifiedDate,
                    IsDeleted(statement.Value.Status), statement.RetrievedAtUtc, cancellationToken);
            }

            foreach (RegisterUzDocument<AnnualReportDto> annualReport in package.AnnualReports)
            {
                RawSaveResult raw = await SaveRawAsync(
                    connection, transaction, syncRunId, AnnualReportObjectType,
                    annualReport.Value.Id, annualReport.Value.LastModifiedDate,
                    annualReport.Value.Status, annualReport, cancellationToken);
                Count(raw.Outcome, ref insertedCount, ref updatedCount, ref unchangedCount);
                await SaveAnnualReportAsync(connection, transaction, annualReport, raw.PayloadVersionId, cancellationToken);
                await ObserveAsync(connection, transaction, syncRunId, AnnualReportObjectType,
                    annualReport.Value.Id, annualReport.Value.LastModifiedDate,
                    IsDeleted(annualReport.Value.Status), annualReport.RetrievedAtUtc, cancellationToken);
            }

            var templates = package.Templates.ToDictionary(x => x.Value.Id, x => x.Value);
            foreach (RegisterUzDocument<FinancialReportDto> report in package.FinancialReports)
            {
                RawSaveResult raw = await SaveRawAsync(
                    connection, transaction, syncRunId, FinancialReportObjectType,
                    report.Value.Id, report.Value.LastModifiedDate,
                    report.Value.Status, report, cancellationToken);
                Count(raw.Outcome, ref insertedCount, ref updatedCount, ref unchangedCount);

                FinancialReportTemplateDto? template = report.Value.TemplateId.HasValue &&
                                                       templates.TryGetValue(report.Value.TemplateId.Value, out var found)
                    ? found
                    : null;

                await SaveFinancialReportAsync(connection, transaction, report, template, raw.PayloadVersionId, cancellationToken);
                await ObserveAsync(connection, transaction, syncRunId, FinancialReportObjectType,
                    report.Value.Id, report.Value.LastModifiedDate,
                    IsDeleted(report.Value.Status), report.RetrievedAtUtc, cancellationToken);
            }

            await UpdateRunStatisticsAsync(
                connection,
                transaction,
                syncRunId,
                insertedCount + updatedCount + unchangedCount,
                insertedCount,
                updatedCount,
                unchangedCount,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<RegisterUzCatalogSyncResult> SaveCatalogsAsync(
        long syncRunId,
        RegisterUzCatalogPackage catalogs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(catalogs);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            int inserted = 0;
            int updated = 0;
            int removed = 0;

            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "Templates",
                catalogs.Templates, catalogs.Templates.Value.Templates.Length,
                JsonSerializer.Serialize(catalogs.Templates.Value.Templates.OrderBy(item => item.Id)), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "LegalForms",
                catalogs.LegalForms, catalogs.LegalForms.Value.Classifications.Length,
                SerializeClassificationCatalog(catalogs.LegalForms.Value.Classifications), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "SkNace",
                catalogs.SkNace, catalogs.SkNace.Value.Classifications.Length,
                SerializeClassificationCatalog(catalogs.SkNace.Value.Classifications), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "OwnershipTypes",
                catalogs.OwnershipTypes, catalogs.OwnershipTypes.Value.Classifications.Length,
                SerializeClassificationCatalog(catalogs.OwnershipTypes.Value.Classifications), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "OrganizationSizes",
                catalogs.OrganizationSizes, catalogs.OrganizationSizes.Value.Classifications.Length,
                SerializeClassificationCatalog(catalogs.OrganizationSizes.Value.Classifications), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "Regions",
                catalogs.Regions, catalogs.Regions.Value.Locations.Length,
                SerializeLocationCatalog(catalogs.Regions.Value.Locations), cancellationToken);
            await SaveCatalogObservationAsync(connection, transaction, syncRunId, "Districts",
                catalogs.Districts, catalogs.Districts.Value.Locations.Length,
                SerializeLocationCatalog(catalogs.Districts.Value.Locations), cancellationToken);

            foreach (FinancialReportTemplateDto template in catalogs.Templates.Value.Templates)
            {
                string itemJson = JsonSerializer.Serialize(template);
                string scope = template.Tables.Length == 0 ? "Metadata" : "Structure";
                CatalogItemOutcome outcome = await SaveCatalogItemStateAsync(
                    connection, transaction, syncRunId, "Templates", template.Id.ToString(CultureInfo.InvariantCulture),
                    itemJson, scope, catalogs.Templates.RetrievedAtUtc, cancellationToken);
                CountCatalogOutcome(outcome, ref inserted, ref updated);
                if (outcome != CatalogItemOutcome.Unchanged)
                {
                    var document = new RegisterUzDocument<FinancialReportTemplateDto>(
                        template, itemJson, catalogs.Templates.RetrievedAtUtc,
                        catalogs.Templates.HttpStatusCode, catalogs.Templates.ApiVersion);
                    await SaveTemplateAsync(connection, transaction, document, cancellationToken);
                }
            }
            removed += await MarkMissingCatalogItemsAsync(
                connection, transaction, syncRunId, "Templates", "Metadata",
                catalogs.Templates.RetrievedAtUtc, cancellationToken);
            await MarkMissingTemplatesAsync(connection, transaction, cancellationToken);

            AddCatalogCounters(await SaveClassificationCatalogAsync(connection, transaction, syncRunId,
                "LegalForms", "LegalForm", catalogs.LegalForms, cancellationToken),
                ref inserted, ref updated, ref removed);
            AddCatalogCounters(await SaveClassificationCatalogAsync(connection, transaction, syncRunId,
                "SkNace", "SkNace", catalogs.SkNace, cancellationToken),
                ref inserted, ref updated, ref removed);
            AddCatalogCounters(await SaveClassificationCatalogAsync(connection, transaction, syncRunId,
                "OwnershipTypes", "OwnershipType", catalogs.OwnershipTypes, cancellationToken),
                ref inserted, ref updated, ref removed);
            AddCatalogCounters(await SaveClassificationCatalogAsync(connection, transaction, syncRunId,
                "OrganizationSizes", "OrganizationSize", catalogs.OrganizationSizes, cancellationToken),
                ref inserted, ref updated, ref removed);
            AddCatalogCounters(await SaveLocationCatalogAsync(connection, transaction, syncRunId,
                "Regions", catalogs.Regions, cancellationToken),
                ref inserted, ref updated, ref removed);
            AddCatalogCounters(await SaveLocationCatalogAsync(connection, transaction, syncRunId,
                "Districts", catalogs.Districts, cancellationToken),
                ref inserted, ref updated, ref removed);
            await DeleteNonCatalogLocationsAsync(connection, transaction, cancellationToken);

            int reviewRequired = await GetCatalogReviewRequiredCountAsync(
                connection, transaction, syncRunId, cancellationToken);
            await UpdateCatalogRunStatisticsAsync(
                connection, transaction, syncRunId, 7, inserted, updated, removed,
                reviewRequired, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return new RegisterUzCatalogSyncResult(7, inserted, updated, removed, reviewRequired);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task CompleteRunAsync(
        long syncRunId,
        RegisterUzLoadResult result,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Sync].[Run]
            SET [CompletedAtUtc] = @CompletedAtUtc,
                [Status] = 'Completed',
                [DetailRequestCount] = @DetailRequestCount,
                [Notes] = N'IČO: ' + @Ico + N'; entity: ' + CONVERT(nvarchar(30), @EntityId)
            WHERE [SyncRunId] = @SyncRunId;

            UPDATE [Sync].[LoadTarget]
            SET [LastAttemptAtUtc] = @CompletedAtUtc,
                [LastSuccessfulLoadAtUtc] = @CompletedAtUtc,
                [LastStatus] = 'Completed', [LastError] = NULL,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [Ico] = @Ico;
            """;
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@Ico", SqlDbType.VarChar, result.Ico, 20);
        Add(command, "@EntityId", SqlDbType.BigInt, result.RegisterUzEntityId);
        Add(command, "@CompletedAtUtc", SqlDbType.DateTime2, result.CompletedAtUtc);
        Add(command, "@DetailRequestCount", SqlDbType.BigInt,
            1L + result.FinancialStatementCount + result.AnnualReportCount +
            result.FinancialReportCount + result.TemplateDetailRequestCount);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveCatalogObservationAsync<T>(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        string catalogCode,
        RegisterUzDocument<T> document,
        int recordCount,
        string canonicalJson,
        CancellationToken cancellationToken)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(document.RawJson);
        byte[] payloadHash = SHA256.HashData(utf8);
        byte[] canonicalHash = RegisterUzCanonicalJson.ComputeSha256(canonicalJson);
        byte[] compressed = Compress(utf8);

        await using var command = CreateCommand(connection, transaction, """
            DECLARE @PreviousCanonicalHash binary(32) =
            (
                SELECT TOP (1) [CanonicalSha256]
                FROM [Sync].[CatalogObservation]
                WHERE [CatalogCode] = @CatalogCode
                ORDER BY [CatalogObservationId] DESC
            );

            INSERT INTO [Sync].[CatalogObservation]
            (
                [SyncRunId], [CatalogCode], [RetrievedAtUtc], [HttpStatusCode],
                [RecordCount], [PayloadSha256], [CanonicalSha256],
                [PayloadCompressed], [CompressionCode], [UncompressedLengthBytes],
                [ApiVersion], [HasChanged]
            )
            VALUES
            (
                @SyncRunId, @CatalogCode, @RetrievedAtUtc, @HttpStatusCode,
                @RecordCount, @PayloadHash, @CanonicalHash,
                @Payload, 'GZIP', @Length,
                @ApiVersion,
                CASE WHEN @PreviousCanonicalHash = @CanonicalHash THEN 0 ELSE 1 END
            );
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@CatalogCode", SqlDbType.VarChar, catalogCode, 40);
        Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
        Add(command, "@HttpStatusCode", SqlDbType.Int, document.HttpStatusCode);
        Add(command, "@RecordCount", SqlDbType.Int, recordCount);
        Add(command, "@PayloadHash", SqlDbType.Binary, payloadHash, 32);
        Add(command, "@CanonicalHash", SqlDbType.Binary, canonicalHash, 32);
        Add(command, "@Payload", SqlDbType.VarBinary, compressed, -1);
        Add(command, "@Length", SqlDbType.BigInt, utf8.LongLength);
        Add(command, "@ApiVersion", SqlDbType.VarChar, document.ApiVersion, 50);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<CatalogItemOutcome> SaveCatalogItemStateAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        string catalogCode,
        string sourceObjectKey,
        string itemJson,
        string changeScope,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        byte[] canonicalHash = RegisterUzCanonicalJson.ComputeSha256(itemJson);
        await using var command = CreateCommand(connection, transaction, """
            DECLARE @OldHash binary(32);
            DECLARE @WasPresent bit;
            DECLARE @Outcome tinyint;
            DECLARE @ChangeType varchar(20);

            SELECT @OldHash = [CanonicalSha256], @WasPresent = [IsPresent]
            FROM [Sync].[CatalogItemState] WITH (UPDLOCK, HOLDLOCK)
            WHERE [CatalogCode] = @CatalogCode
              AND [SourceObjectKey] = @SourceObjectKey;

            IF @OldHash IS NULL
            BEGIN
                INSERT INTO [Sync].[CatalogItemState]
                (
                    [CatalogCode], [SourceObjectKey], [CanonicalSha256],
                    [FirstObservedAtUtc], [LastObservedAtUtc],
                    [FirstObservedInRunId], [LastObservedInRunId], [IsPresent]
                )
                VALUES
                (
                    @CatalogCode, @SourceObjectKey, @CanonicalHash,
                    @ObservedAtUtc, @ObservedAtUtc,
                    @SyncRunId, @SyncRunId, 1
                );
                SET @Outcome = 1;
                SET @ChangeType = 'Inserted';
            END
            ELSE IF @OldHash <> @CanonicalHash OR @WasPresent = 0
            BEGIN
                SET @Outcome = CASE WHEN @WasPresent = 0 THEN 3 ELSE 2 END;
                SET @ChangeType = CASE WHEN @WasPresent = 0 THEN 'Reappeared' ELSE 'Updated' END;
                UPDATE [Sync].[CatalogItemState]
                SET [CanonicalSha256] = @CanonicalHash,
                    [LastObservedAtUtc] = @ObservedAtUtc,
                    [LastObservedInRunId] = @SyncRunId,
                    [IsPresent] = 1
                WHERE [CatalogCode] = @CatalogCode
                  AND [SourceObjectKey] = @SourceObjectKey;
            END
            ELSE
            BEGIN
                SET @Outcome = 0;
                UPDATE [Sync].[CatalogItemState]
                SET [LastObservedAtUtc] = @ObservedAtUtc,
                    [LastObservedInRunId] = @SyncRunId
                WHERE [CatalogCode] = @CatalogCode
                  AND [SourceObjectKey] = @SourceObjectKey;
            END;

            IF @Outcome <> 0
                INSERT INTO [Sync].[CatalogChange]
                (
                    [SyncRunId], [CatalogCode], [SourceObjectKey],
                    [ChangeType], [ChangeScope],
                    [OldCanonicalSha256], [NewCanonicalSha256],
                    [ChangeDescription], [RequiresReview]
                )
                VALUES
                (
                    @SyncRunId, @CatalogCode, @SourceObjectKey,
                    @ChangeType, @ChangeScope,
                    @OldHash, @CanonicalHash,
                    CONCAT(@CatalogCode, ' ', @SourceObjectKey, ' ', LOWER(@ChangeType), '.'),
                    CASE WHEN EXISTS
                    (
                        SELECT 1
                        FROM [Sync].[CatalogObservation]
                        WHERE [CatalogCode] = @CatalogCode
                          AND [SyncRunId] <> @SyncRunId
                    ) THEN 1 ELSE 0 END
                );

            SELECT @Outcome;
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@CatalogCode", SqlDbType.VarChar, catalogCode, 40);
        Add(command, "@SourceObjectKey", SqlDbType.VarChar, sourceObjectKey, 100);
        Add(command, "@CanonicalHash", SqlDbType.Binary, canonicalHash, 32);
        Add(command, "@ChangeScope", SqlDbType.VarChar, changeScope, 20);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return (CatalogItemOutcome)Convert.ToByte(result, CultureInfo.InvariantCulture);
    }

    private static async Task<int> MarkMissingCatalogItemsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        string catalogCode,
        string changeScope,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            DECLARE @MissingCount int =
            (
                SELECT COUNT(*)
                FROM [Sync].[CatalogItemState]
                WHERE [CatalogCode] = @CatalogCode
                  AND [IsPresent] = 1
                  AND [LastObservedInRunId] <> @SyncRunId
            );

            INSERT INTO [Sync].[CatalogChange]
            (
                [SyncRunId], [CatalogCode], [SourceObjectKey],
                [ChangeType], [ChangeScope],
                [OldCanonicalSha256], [NewCanonicalSha256],
                [ChangeDescription], [RequiresReview]
            )
            SELECT
                @SyncRunId, [CatalogCode], [SourceObjectKey],
                'Removed', @ChangeScope,
                [CanonicalSha256], NULL,
                CONCAT([CatalogCode], ' ', [SourceObjectKey], ' removed from the official response.'), 1
            FROM [Sync].[CatalogItemState]
            WHERE [CatalogCode] = @CatalogCode
              AND [IsPresent] = 1
              AND [LastObservedInRunId] <> @SyncRunId;

            UPDATE [Sync].[CatalogItemState]
            SET [IsPresent] = 0,
                [LastObservedAtUtc] = @ObservedAtUtc,
                [LastObservedInRunId] = @SyncRunId
            WHERE [CatalogCode] = @CatalogCode
              AND [IsPresent] = 1
              AND [LastObservedInRunId] <> @SyncRunId;

            SELECT @MissingCount;
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@CatalogCode", SqlDbType.VarChar, catalogCode, 40);
        Add(command, "@ChangeScope", SqlDbType.VarChar, changeScope, 20);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task<CatalogCounters> SaveClassificationCatalogAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        string catalogCode,
        string tableName,
        RegisterUzDocument<ClassificationCatalogDto> document,
        CancellationToken cancellationToken)
    {
        int inserted = 0;
        int updated = 0;

        foreach (ClassificationDto item in document.Value.Classifications)
        {
            ValidateCatalogCode(item.Code, catalogCode);
            string itemJson = JsonSerializer.Serialize(item);
            CatalogItemOutcome outcome = await SaveCatalogItemStateAsync(
                connection, transaction, syncRunId, catalogCode, item.Code,
                itemJson, "Caption", document.RetrievedAtUtc, cancellationToken);
            CountCatalogOutcome(outcome, ref inserted, ref updated);
            if (outcome != CatalogItemOutcome.Unchanged)
            {
                await SaveClassificationAsync(
                    connection, transaction, tableName, item, document.RetrievedAtUtc, cancellationToken);
            }
        }

        int removed = await MarkMissingCatalogItemsAsync(
            connection, transaction, syncRunId, catalogCode, "Metadata",
            document.RetrievedAtUtc, cancellationToken);
        await MarkNormalizedMissingAsync(connection, transaction, catalogCode, tableName, cancellationToken);
        return new CatalogCounters(inserted, updated, removed);
    }

    private static async Task<CatalogCounters> SaveLocationCatalogAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        string catalogCode,
        RegisterUzDocument<LocationCatalogDto> document,
        CancellationToken cancellationToken)
    {
        int inserted = 0;
        int updated = 0;

        foreach (LocationDto item in document.Value.Locations)
        {
            ValidateCatalogCode(item.Code, catalogCode);
            string itemJson = JsonSerializer.Serialize(item);
            CatalogItemOutcome outcome = await SaveCatalogItemStateAsync(
                connection, transaction, syncRunId, catalogCode, item.Code,
                itemJson, "Metadata", document.RetrievedAtUtc, cancellationToken);
            CountCatalogOutcome(outcome, ref inserted, ref updated);
            if (outcome != CatalogItemOutcome.Unchanged)
            {
                await SaveLocationAsync(connection, transaction, item, document.RetrievedAtUtc, cancellationToken);
            }
        }

        int removed = await MarkMissingCatalogItemsAsync(
            connection, transaction, syncRunId, catalogCode, "Metadata",
            document.RetrievedAtUtc, cancellationToken);
        await MarkNormalizedMissingAsync(connection, transaction, catalogCode, "Location", cancellationToken);
        return new CatalogCounters(inserted, updated, removed);
    }

    private static async Task SaveClassificationAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string tableName,
        ClassificationDto item,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        string qualifiedTable = tableName switch
        {
            "LegalForm" => "[Reference].[LegalForm]",
            "SkNace" => "[Reference].[SkNace]",
            "OwnershipType" => "[Reference].[OwnershipType]",
            "OrganizationSize" => "[Reference].[OrganizationSize]",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported classification table.")
        };

        await using var command = CreateCommand(connection, transaction, $"""
            UPDATE {qualifiedTable}
            SET [TitleSk] = @TitleSk,
                [TitleEn] = @TitleEn,
                [IsDeleted] = 0,
                [LastObservedAtUtc] = @ObservedAtUtc,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [SourceCode] = @SourceCode;

            IF @@ROWCOUNT = 0
                INSERT INTO {qualifiedTable}
                (
                    [SourceCode], [TitleSk], [TitleEn], [IsDeleted],
                    [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @SourceCode, @TitleSk, @TitleEn, 0,
                    @ObservedAtUtc, @ObservedAtUtc
                );
            """);
        Add(command, "@SourceCode", SqlDbType.VarChar, item.Code, 100);
        Add(command, "@TitleSk", SqlDbType.NVarChar, item.Name?.Sk, 250);
        Add(command, "@TitleEn", SqlDbType.NVarChar, item.Name?.En, 250);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveLocationAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        LocationDto item,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Reference].[Location]
            SET [ParentSourceCode] = @ParentSourceCode,
                [TitleSk] = @TitleSk,
                [TitleEn] = @TitleEn,
                [IsDeleted] = 0,
                [LastObservedAtUtc] = @ObservedAtUtc,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [SourceCode] = @SourceCode;

            IF @@ROWCOUNT = 0
                INSERT INTO [Reference].[Location]
                (
                    [SourceCode], [ParentSourceCode], [TitleSk], [TitleEn], [IsDeleted],
                    [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @SourceCode, @ParentSourceCode, @TitleSk, @TitleEn, 0,
                    @ObservedAtUtc, @ObservedAtUtc
                );
            """);
        Add(command, "@SourceCode", SqlDbType.VarChar, item.Code, 100);
        Add(command, "@ParentSourceCode", SqlDbType.VarChar, item.ParentCode, 100);
        Add(command, "@TitleSk", SqlDbType.NVarChar, item.Name?.Sk, 250);
        Add(command, "@TitleEn", SqlDbType.NVarChar, item.Name?.En, 250);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task MarkNormalizedMissingAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string catalogCode,
        string tableName,
        CancellationToken cancellationToken)
    {
        string qualifiedTable = tableName switch
        {
            "LegalForm" => "[Reference].[LegalForm]",
            "SkNace" => "[Reference].[SkNace]",
            "OwnershipType" => "[Reference].[OwnershipType]",
            "OrganizationSize" => "[Reference].[OrganizationSize]",
            "Location" => "[Reference].[Location]",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported catalog table.")
        };

        await using var command = CreateCommand(connection, transaction, $"""
            UPDATE target
            SET [IsDeleted] = 1,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            FROM {qualifiedTable} target
            JOIN [Sync].[CatalogItemState] state
              ON state.[SourceObjectKey] = target.[SourceCode]
            WHERE state.[CatalogCode] = @CatalogCode
              AND state.[IsPresent] = 0
              AND target.[IsDeleted] = 0;
            """);
        Add(command, "@CatalogCode", SqlDbType.VarChar, catalogCode, 40);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task MarkMissingTemplatesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE template
            SET [IsDeleted] = 1,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            FROM [Templates].[FinancialReportTemplate] template
            JOIN [Sync].[CatalogItemState] state
              ON TRY_CONVERT(bigint, state.[SourceObjectKey]) = template.[RegisterUzTemplateId]
            WHERE state.[CatalogCode] = 'Templates'
              AND state.[IsPresent] = 0
              AND template.[IsDeleted] = 0;
            """);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task DeleteNonCatalogLocationsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            DELETE location
            FROM [Reference].[Location] location
            WHERE NOT EXISTS
            (
                SELECT 1
                FROM [Sync].[CatalogItemState] state
                WHERE state.[CatalogCode] IN ('Regions', 'Districts')
                  AND state.[SourceObjectKey] = location.[SourceCode]
                  AND state.[IsPresent] = 1
            );
            """);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void ValidateCatalogCode(string code, string catalogCode)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 100)
            throw new InvalidOperationException($"RegisterUZ catalog {catalogCode} returned an invalid source code.");
    }

    private static string SerializeClassificationCatalog(IEnumerable<ClassificationDto> items) =>
        JsonSerializer.Serialize(items.OrderBy(item => item.Code, StringComparer.Ordinal));

    private static string SerializeLocationCatalog(IEnumerable<LocationDto> items) =>
        JsonSerializer.Serialize(items.OrderBy(item => item.Code, StringComparer.Ordinal));

    private static void CountCatalogOutcome(
        CatalogItemOutcome outcome,
        ref int inserted,
        ref int updated)
    {
        switch (outcome)
        {
            case CatalogItemOutcome.Unchanged:
                return;
            case CatalogItemOutcome.Inserted:
                inserted++;
                return;
            case CatalogItemOutcome.Updated:
            case CatalogItemOutcome.Reappeared:
                updated++;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
        }
    }

    private static void AddCatalogCounters(
        CatalogCounters counters,
        ref int inserted,
        ref int updated,
        ref int removed)
    {
        inserted += counters.Inserted;
        updated += counters.Updated;
        removed += counters.Removed;
    }

    private static async Task<int> GetCatalogReviewRequiredCountAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            SELECT COUNT(*)
            FROM [Sync].[CatalogChange]
            WHERE [SyncRunId] = @SyncRunId
              AND [RequiresReview] = 1;
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task UpdateCatalogRunStatisticsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        int observationCount,
        int insertedCount,
        int updatedCount,
        int removedCount,
        int reviewRequiredCount,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Sync].[Run]
            SET [CatalogObservationCount] = @ObservationCount,
                [CatalogInsertedCount] = @InsertedCount,
                [CatalogUpdatedCount] = @UpdatedCount,
                [CatalogRemovedCount] = @RemovedCount,
                [CatalogReviewRequiredCount] = @ReviewRequiredCount
            WHERE [SyncRunId] = @SyncRunId;
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@ObservationCount", SqlDbType.BigInt, observationCount);
        Add(command, "@InsertedCount", SqlDbType.BigInt, insertedCount);
        Add(command, "@UpdatedCount", SqlDbType.BigInt, updatedCount);
        Add(command, "@RemovedCount", SqlDbType.BigInt, removedCount);
        Add(command, "@ReviewRequiredCount", SqlDbType.BigInt, reviewRequiredCount);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task FailRunAsync(
        long syncRunId,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DECLARE @Ico varchar(20) =
                (SELECT REPLACE(CONVERT(varchar(20), [Notes]), 'IČO: ', '')
                 FROM [Sync].[Run] WHERE [SyncRunId] = @SyncRunId);

            UPDATE [Sync].[Run]
            SET [CompletedAtUtc] = SYSUTCDATETIME(), [Status] = 'Failed',
                [ErrorCount] = [ErrorCount] + 1
            WHERE [SyncRunId] = @SyncRunId;

            INSERT INTO [Sync].[Error]
                ([SyncRunId], [ErrorStage], [ErrorCode], [Message], [Details])
            VALUES
                (@SyncRunId, 'SingleIcoLoad', @ErrorCode, @Message, @Details);

            UPDATE [Sync].[LoadTarget]
            SET [LastAttemptAtUtc] = SYSUTCDATETIME(), [LastStatus] = 'Failed',
                [LastError] = @Message, [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [Ico] = @Ico;
            """;
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@ErrorCode", SqlDbType.VarChar, exception.GetType().Name, 100);
        Add(command, "@Message", SqlDbType.NVarChar, exception.Message, -1);
        Add(command, "@Details", SqlDbType.NVarChar, exception.ToString(), -1);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<RawSaveResult> SaveRawAsync<T>(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        byte objectTypeId,
        long objectId,
        DateOnly? sourceLastModifiedDate,
        string? sourceStatus,
        RegisterUzDocument<T> document,
        CancellationToken cancellationToken)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(document.RawJson);
        byte[] payloadHash = SHA256.HashData(utf8);
        byte[] canonicalHash = RegisterUzCanonicalJson.ComputeSha256(document.RawJson);
        byte[] compressed = Compress(utf8);

        await using var command = CreateCommand(connection, transaction, """
            DECLARE @PayloadVersionId bigint;
            DECLARE @ObjectAlreadyExists bit =
                CASE WHEN EXISTS
                (
                    SELECT 1
                    FROM [Raw].[PayloadVersion]
                    WHERE [ObjectTypeId] = @ObjectTypeId
                      AND [RegisterUzObjectId] = @ObjectId
                ) THEN 1 ELSE 0 END;
            DECLARE @Outcome tinyint;

            SELECT @PayloadVersionId = [PayloadVersionId]
            FROM [Raw].[PayloadVersion] WITH (UPDLOCK, HOLDLOCK)
            WHERE [ObjectTypeId] = @ObjectTypeId
              AND [RegisterUzObjectId] = @ObjectId
              AND [CanonicalSha256] = @CanonicalHash;

            IF @PayloadVersionId IS NULL
            BEGIN
                INSERT INTO [Raw].[PayloadVersion]
                (
                    [ObjectTypeId], [RegisterUzObjectId],
                    [PayloadSha256], [CanonicalSha256],
                    [PayloadCompressed], [CompressionCode], [UncompressedLengthBytes],
                    [RetrievedAtUtc], [FirstObservedAtUtc], [LastObservedAtUtc],
                    [SourceLastModifiedDate], [SourceStatus], [IsDeleted],
                    [HttpStatusCode], [ApiVersion], [SyncRunId],
                    [ValidationStatus], [NormalizedAtUtc]
                )
                VALUES
                (
                    @ObjectTypeId, @ObjectId,
                    @PayloadHash, @CanonicalHash,
                    @Payload, 'GZIP', @Length,
                    @RetrievedAtUtc, @RetrievedAtUtc, @RetrievedAtUtc,
                    @SourceLastModifiedDate, @SourceStatus, @IsDeleted,
                    @HttpStatusCode, @ApiVersion, @SyncRunId,
                    'Valid', @RetrievedAtUtc
                );
                SET @PayloadVersionId = SCOPE_IDENTITY();
                SET @Outcome = CASE WHEN @ObjectAlreadyExists = 1 THEN 2 ELSE 1 END;
            END
            ELSE
            BEGIN
                UPDATE [Raw].[PayloadVersion]
                SET [LastObservedAtUtc] = @RetrievedAtUtc,
                    [SyncRunId] = @SyncRunId
                WHERE [PayloadVersionId] = @PayloadVersionId;
                SET @Outcome = 0;
            END;

            SELECT @PayloadVersionId AS [PayloadVersionId], @Outcome AS [Outcome];
            """);
        Add(command, "@ObjectTypeId", SqlDbType.TinyInt, objectTypeId);
        Add(command, "@ObjectId", SqlDbType.BigInt, objectId);
        Add(command, "@PayloadHash", SqlDbType.Binary, payloadHash, 32);
        Add(command, "@CanonicalHash", SqlDbType.Binary, canonicalHash, 32);
        Add(command, "@Payload", SqlDbType.VarBinary, compressed, -1);
        Add(command, "@Length", SqlDbType.BigInt, utf8.LongLength);
        Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
        Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(sourceLastModifiedDate));
        Add(command, "@SourceStatus", SqlDbType.NVarChar, sourceStatus, 30);
        Add(command, "@IsDeleted", SqlDbType.Bit, IsDeleted(sourceStatus));
        Add(command, "@HttpStatusCode", SqlDbType.Int, document.HttpStatusCode);
        Add(command, "@ApiVersion", SqlDbType.VarChar, document.ApiVersion, 50);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Raw payload save did not return its result.");
        return new RawSaveResult(reader.GetInt64(0), (PersistenceOutcome)reader.GetByte(1));
    }

    private static void Count(
        PersistenceOutcome outcome,
        ref long insertedCount,
        ref long updatedCount,
        ref long unchangedCount)
    {
        switch (outcome)
        {
            case PersistenceOutcome.Inserted:
                insertedCount++;
                break;
            case PersistenceOutcome.Updated:
                updatedCount++;
                break;
            case PersistenceOutcome.Unchanged:
                unchangedCount++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
        }
    }

    private static async Task UpdateRunStatisticsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        long observedCount,
        long insertedCount,
        long updatedCount,
        long unchangedCount,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Sync].[Run]
            SET [ObservedIdCount] = @ObservedCount,
                [InsertedObjectCount] = @InsertedCount,
                [UpdatedObjectCount] = @UpdatedCount,
                [UnchangedObjectCount] = @UnchangedCount
            WHERE [SyncRunId] = @SyncRunId;
            """);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@ObservedCount", SqlDbType.BigInt, observedCount);
        Add(command, "@InsertedCount", SqlDbType.BigInt, insertedCount);
        Add(command, "@UpdatedCount", SqlDbType.BigInt, updatedCount);
        Add(command, "@UnchangedCount", SqlDbType.BigInt, unchangedCount);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ObserveAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long syncRunId,
        byte objectTypeId,
        long objectId,
        DateOnly? sourceLastModifiedDate,
        bool isDeleted,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Sync].[ObservedObject]
            SET [LastObservedAtUtc] = @ObservedAtUtc,
                [LastObservedInRunId] = @SyncRunId,
                [ObservationCount] = [ObservationCount] + 1,
                [LastDetailAttemptAtUtc] = @ObservedAtUtc,
                [LastDetailRetrievedAtUtc] = @ObservedAtUtc,
                [LastDetailStatus] = 'Retrieved',
                [SourceLastModifiedDate] = @SourceLastModifiedDate,
                [IsDeleted] = @IsDeleted
            WHERE [ObjectTypeId] = @ObjectTypeId
              AND [RegisterUzObjectId] = @ObjectId;

            IF @@ROWCOUNT = 0
                INSERT INTO [Sync].[ObservedObject]
                (
                    [ObjectTypeId], [RegisterUzObjectId],
                    [FirstObservedAtUtc], [LastObservedAtUtc],
                    [FirstObservedInRunId], [LastObservedInRunId],
                    [LastDetailAttemptAtUtc], [LastDetailRetrievedAtUtc],
                    [LastDetailStatus], [SourceLastModifiedDate], [IsDeleted]
                )
                VALUES
                (
                    @ObjectTypeId, @ObjectId,
                    @ObservedAtUtc, @ObservedAtUtc,
                    @SyncRunId, @SyncRunId,
                    @ObservedAtUtc, @ObservedAtUtc,
                    'Retrieved', @SourceLastModifiedDate, @IsDeleted
                );
            """);
        Add(command, "@ObjectTypeId", SqlDbType.TinyInt, objectTypeId);
        Add(command, "@ObjectId", SqlDbType.BigInt, objectId);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(sourceLastModifiedDate));
        Add(command, "@IsDeleted", SqlDbType.Bit, isDeleted);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureEntityReferencesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        AccountingEntityDto entity,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await EnsureReferenceCodeAsync(connection, transaction, "LegalForm", entity.LegalFormCode, observedAtUtc, cancellationToken);
        await EnsureReferenceCodeAsync(connection, transaction, "SkNace", entity.SkNaceCode, observedAtUtc, cancellationToken);
        await EnsureReferenceCodeAsync(connection, transaction, "OrganizationSize", entity.OrganizationSizeCode, observedAtUtc, cancellationToken);
        await EnsureReferenceCodeAsync(connection, transaction, "OwnershipType", entity.OwnershipTypeCode, observedAtUtc, cancellationToken);
        await EnsureReferenceCodeAsync(connection, transaction, "Location", entity.RegionCode, observedAtUtc, cancellationToken);
        await EnsureReferenceCodeAsync(connection, transaction, "Location", entity.DistrictCode, observedAtUtc, cancellationToken);
    }

    private static async Task EnsureReferenceCodeAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string tableName,
        string? sourceCode,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return;

        string sql = $"""
            IF NOT EXISTS
            (
                SELECT 1 FROM [Reference].[{tableName}]
                WHERE [SourceCode] = @SourceCode
            )
                INSERT INTO [Reference].[{tableName}]
                (
                    [SourceCode], [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @SourceCode, @ObservedAtUtc, @ObservedAtUtc
                );
            """;
        await using var command = CreateCommand(connection, transaction, sql);
        Add(command, "@SourceCode", SqlDbType.VarChar, sourceCode, 100);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveEntityAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        RegisterUzDocument<AccountingEntityDto> document,
        long payloadVersionId,
        CancellationToken cancellationToken)
    {
        AccountingEntityDto entity = document.Value;
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Registry].[AccountingEntity]
            SET [Ico] = @Ico, [Dic] = @Dic, [Sid] = @Sid, [Name] = @Name,
                [City] = @City, [Street] = @Street, [PostalCode] = @PostalCode,
                [EstablishedDate] = @EstablishedDate, [CancellationDate] = @CancellationDate,
                [LegalFormCode] = @LegalFormCode, [SkNaceCode] = @SkNaceCode,
                [OrganizationSizeCode] = @OrganizationSizeCode,
                [OwnershipTypeCode] = @OwnershipTypeCode,
                [RegionCode] = @RegionCode, [DistrictCode] = @DistrictCode,
                [RegisteredOfficeCode] = @RegisteredOfficeCode,
                [IsConsolidated] = @IsConsolidated, [DataSourceCode] = @DataSourceCode,
                [SourceLastModifiedDate] = @SourceLastModifiedDate,
                [SourceStatus] = @SourceStatus, [IsDeleted] = @IsDeleted,
                [LastObservedAtUtc] = @RetrievedAtUtc,
                [LastDetailRetrievedAtUtc] = @RetrievedAtUtc,
                [CurrentPayloadVersionId] = @PayloadVersionId,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzEntityId] = @Id;

            IF @@ROWCOUNT = 0
                INSERT INTO [Registry].[AccountingEntity]
                (
                    [RegisterUzEntityId], [Ico], [Dic], [Sid], [Name], [City], [Street],
                    [PostalCode], [EstablishedDate], [CancellationDate], [LegalFormCode],
                    [SkNaceCode], [OrganizationSizeCode], [OwnershipTypeCode], [RegionCode],
                    [DistrictCode], [RegisteredOfficeCode], [IsConsolidated], [DataSourceCode],
                    [SourceLastModifiedDate], [SourceStatus], [IsDeleted],
                    [FirstObservedAtUtc], [LastObservedAtUtc], [LastDetailRetrievedAtUtc],
                    [CurrentPayloadVersionId]
                )
                VALUES
                (
                    @Id, @Ico, @Dic, @Sid, @Name, @City, @Street,
                    @PostalCode, @EstablishedDate, @CancellationDate, @LegalFormCode,
                    @SkNaceCode, @OrganizationSizeCode, @OwnershipTypeCode, @RegionCode,
                    @DistrictCode, @RegisteredOfficeCode, @IsConsolidated, @DataSourceCode,
                    @SourceLastModifiedDate, @SourceStatus, @IsDeleted,
                    @RetrievedAtUtc, @RetrievedAtUtc, @RetrievedAtUtc,
                    @PayloadVersionId
                );
            """);
        Add(command, "@Id", SqlDbType.BigInt, entity.Id);
        Add(command, "@Ico", SqlDbType.VarChar, entity.Ico, 20);
        Add(command, "@Dic", SqlDbType.VarChar, entity.Dic, 20);
        Add(command, "@Sid", SqlDbType.VarChar, entity.Sid, 20);
        Add(command, "@Name", SqlDbType.NVarChar, entity.Name, 500);
        Add(command, "@City", SqlDbType.NVarChar, entity.City, 200);
        Add(command, "@Street", SqlDbType.NVarChar, entity.Street, 500);
        Add(command, "@PostalCode", SqlDbType.VarChar, entity.PostalCode, 20);
        Add(command, "@EstablishedDate", SqlDbType.Date, ToDbDate(entity.EstablishedDate));
        Add(command, "@CancellationDate", SqlDbType.Date, ToDbDate(entity.CancellationDate));
        Add(command, "@LegalFormCode", SqlDbType.VarChar, entity.LegalFormCode, 100);
        Add(command, "@SkNaceCode", SqlDbType.VarChar, entity.SkNaceCode, 100);
        Add(command, "@OrganizationSizeCode", SqlDbType.VarChar, entity.OrganizationSizeCode, 100);
        Add(command, "@OwnershipTypeCode", SqlDbType.VarChar, entity.OwnershipTypeCode, 100);
        Add(command, "@RegionCode", SqlDbType.VarChar, entity.RegionCode, 100);
        Add(command, "@DistrictCode", SqlDbType.VarChar, entity.DistrictCode, 100);
        Add(command, "@RegisteredOfficeCode", SqlDbType.VarChar, entity.RegisteredOfficeCode, 100);
        Add(command, "@IsConsolidated", SqlDbType.Bit, entity.IsConsolidated);
        Add(command, "@DataSourceCode", SqlDbType.VarChar, entity.DataSourceCode, 30);
        Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(entity.LastModifiedDate));
        Add(command, "@SourceStatus", SqlDbType.NVarChar, entity.Status, 30);
        Add(command, "@IsDeleted", SqlDbType.Bit, IsDeleted(entity.Status));
        Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
        Add(command, "@PayloadVersionId", SqlDbType.BigInt, payloadVersionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveTemplateAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        RegisterUzDocument<FinancialReportTemplateDto> document,
        CancellationToken cancellationToken)
    {
        FinancialReportTemplateDto template = document.Value;
        await using (var command = CreateCommand(connection, transaction, """
            UPDATE [Templates].[FinancialReportTemplate]
            SET [Name] = @Name, [MinistrySpecification] = @Specification,
                [ValidFrom] = @ValidFrom, [ValidTo] = @ValidTo,
                [IsDeleted] = 0,
                [LastObservedAtUtc] = @ObservedAtUtc, [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzTemplateId] = @Id;
            IF @@ROWCOUNT = 0
                INSERT INTO [Templates].[FinancialReportTemplate]
                (
                    [RegisterUzTemplateId], [Name], [MinistrySpecification],
                    [ValidFrom], [ValidTo], [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @Id, @Name, @Specification,
                    @ValidFrom, @ValidTo, @ObservedAtUtc, @ObservedAtUtc
                );
            """))
        {
            Add(command, "@Id", SqlDbType.BigInt, template.Id);
            Add(command, "@Name", SqlDbType.NVarChar, template.Name, 255);
            Add(command, "@Specification", SqlDbType.NVarChar, template.MinistrySpecification, 100);
            Add(command, "@ValidFrom", SqlDbType.Date, ToDbDate(template.ValidFrom));
            Add(command, "@ValidTo", SqlDbType.Date, ToDbDate(template.ValidTo));
            Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        for (int tableOrdinal = 0; tableOrdinal < template.Tables.Length; tableOrdinal++)
        {
            TemplateTableDto table = template.Tables[tableOrdinal];
            int? numberOfColumns = CalculateColumnCount(table.Headers);
            long tableId;
            await using (var command = CreateCommand(connection, transaction, """
                UPDATE [Templates].[TemplateTable]
                SET [NameSk] = @NameSk, [NameEn] = @NameEn,
                    [NumberOfColumns] = @NumberOfColumns,
                    [UpdatedAtUtc] = SYSUTCDATETIME()
                WHERE [RegisterUzTemplateId] = @TemplateId AND [TableOrdinal] = @Ordinal;
                IF @@ROWCOUNT = 0
                    INSERT INTO [Templates].[TemplateTable]
                    (
                        [RegisterUzTemplateId], [TableOrdinal], [NameSk], [NameEn], [NumberOfColumns]
                    )
                    VALUES
                    (
                        @TemplateId, @Ordinal, @NameSk, @NameEn, @NumberOfColumns
                    );
                SELECT [TemplateTableId]
                FROM [Templates].[TemplateTable]
                WHERE [RegisterUzTemplateId] = @TemplateId AND [TableOrdinal] = @Ordinal;
                """))
            {
                Add(command, "@TemplateId", SqlDbType.BigInt, template.Id);
                Add(command, "@Ordinal", SqlDbType.Int, tableOrdinal);
                Add(command, "@NameSk", SqlDbType.NVarChar, table.Name?.Sk, 250);
                Add(command, "@NameEn", SqlDbType.NVarChar, table.Name?.En, 250);
                Add(command, "@NumberOfColumns", SqlDbType.Int, numberOfColumns);
                tableId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            }

            await ReplaceTemplateChildrenAsync(connection, transaction, tableId, table, cancellationToken);
        }
    }

    private static async Task ReplaceTemplateChildrenAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long templateTableId,
        TemplateTableDto table,
        CancellationToken cancellationToken)
    {
        await using (var delete = CreateCommand(connection, transaction, """
            DELETE FROM [Templates].[TemplateHeader] WHERE [TemplateTableId] = @TableId;
            DELETE FROM [Templates].[TemplateRow] WHERE [TemplateTableId] = @TableId;
            """))
        {
            Add(delete, "@TableId", SqlDbType.BigInt, templateTableId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        for (int ordinal = 0; ordinal < table.Headers.Length; ordinal++)
        {
            TemplateHeaderDto header = table.Headers[ordinal];
            await using var command = CreateCommand(connection, transaction, """
                INSERT INTO [Templates].[TemplateHeader]
                (
                    [TemplateTableId], [HeaderOrdinal], [TextSk], [TextEn],
                    [RowPosition], [ColumnPosition], [ColumnSpan], [RowSpan]
                )
                VALUES
                (
                    @TableId, @Ordinal, @TextSk, @TextEn,
                    @RowPosition, @ColumnPosition, @ColumnSpan, @RowSpan
                );
                """);
            Add(command, "@TableId", SqlDbType.BigInt, templateTableId);
            Add(command, "@Ordinal", SqlDbType.Int, ordinal);
            Add(command, "@TextSk", SqlDbType.NVarChar, header.Text?.Sk, -1);
            Add(command, "@TextEn", SqlDbType.NVarChar, header.Text?.En, -1);
            Add(command, "@RowPosition", SqlDbType.Int, header.RowPosition);
            Add(command, "@ColumnPosition", SqlDbType.Int, header.ColumnPosition);
            Add(command, "@ColumnSpan", SqlDbType.Int, header.ColumnSpan);
            Add(command, "@RowSpan", SqlDbType.Int, header.RowSpan);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        for (int ordinal = 0; ordinal < table.Rows.Length; ordinal++)
        {
            TemplateRowDto row = table.Rows[ordinal];
            await using var command = CreateCommand(connection, transaction, """
                INSERT INTO [Templates].[TemplateRow]
                (
                    [TemplateTableId], [RowOrdinal], [RowNumber], [Designation], [TextSk], [TextEn]
                )
                VALUES
                (
                    @TableId, @Ordinal, @RowNumber, @Designation, @TextSk, @TextEn
                );
                """);
            Add(command, "@TableId", SqlDbType.BigInt, templateTableId);
            Add(command, "@Ordinal", SqlDbType.Int, ordinal);
            Add(command, "@RowNumber", SqlDbType.Int, row.RowNumber);
            Add(command, "@Designation", SqlDbType.NVarChar, row.Designation, 100);
            Add(command, "@TextSk", SqlDbType.NVarChar, row.Text?.Sk, -1);
            Add(command, "@TextEn", SqlDbType.NVarChar, row.Text?.En, -1);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static int? CalculateColumnCount(IEnumerable<TemplateHeaderDto> headers)
    {
        int? maximum = null;
        foreach (TemplateHeaderDto header in headers)
        {
            if (!header.ColumnPosition.HasValue)
                continue;
            int end = header.ColumnPosition.Value + Math.Max(header.ColumnSpan ?? 1, 1) - 1;
            maximum = !maximum.HasValue || end > maximum.Value ? end : maximum;
        }
        return maximum;
    }

    private static async Task SaveStatementAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        RegisterUzDocument<FinancialStatementDto> document,
        long payloadVersionId,
        CancellationToken cancellationToken)
    {
        FinancialStatementDto statement = document.Value;
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Reporting].[FinancialStatement]
            SET [RegisterUzEntityId] = @EntityId,
                [PeriodFrom] = @PeriodFrom, [PeriodTo] = @PeriodTo,
                [SubmissionDate] = @SubmissionDate, [PreparationDate] = @PreparationDate,
                [ApprovalDate] = @ApprovalDate, [AssemblyDate] = @AssemblyDate,
                [AuditorReportAttachmentDate] = @AuditorReportAttachmentDate,
                [FundName] = @FundName, [LeiCode] = @LeiCode,
                [IsConsolidated] = @IsConsolidated,
                [IsConsolidatedCentralGovernment] = @IsConsolidatedCentralGovernment,
                [IsSummaryPublicAdministration] = @IsSummaryPublicAdministration,
                [StatementType] = @StatementType, [DataSourceCode] = @DataSourceCode,
                [SourceLastModifiedDate] = @SourceLastModifiedDate,
                [SourceStatus] = @SourceStatus, [IsDeleted] = @IsDeleted,
                [LastObservedAtUtc] = @RetrievedAtUtc,
                [LastDetailRetrievedAtUtc] = @RetrievedAtUtc,
                [CurrentPayloadVersionId] = @PayloadVersionId,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzStatementId] = @Id;
            IF @@ROWCOUNT = 0
                INSERT INTO [Reporting].[FinancialStatement]
                (
                    [RegisterUzStatementId], [RegisterUzEntityId], [PeriodFrom], [PeriodTo],
                    [SubmissionDate], [PreparationDate], [ApprovalDate], [AssemblyDate],
                    [AuditorReportAttachmentDate], [FundName], [LeiCode], [IsConsolidated],
                    [IsConsolidatedCentralGovernment], [IsSummaryPublicAdministration],
                    [StatementType], [DataSourceCode], [SourceLastModifiedDate],
                    [SourceStatus], [IsDeleted], [FirstObservedAtUtc], [LastObservedAtUtc],
                    [LastDetailRetrievedAtUtc], [CurrentPayloadVersionId]
                )
                VALUES
                (
                    @Id, @EntityId, @PeriodFrom, @PeriodTo,
                    @SubmissionDate, @PreparationDate, @ApprovalDate, @AssemblyDate,
                    @AuditorReportAttachmentDate, @FundName, @LeiCode, @IsConsolidated,
                    @IsConsolidatedCentralGovernment, @IsSummaryPublicAdministration,
                    @StatementType, @DataSourceCode, @SourceLastModifiedDate,
                    @SourceStatus, @IsDeleted, @RetrievedAtUtc, @RetrievedAtUtc,
                    @RetrievedAtUtc, @PayloadVersionId
                );
            """);
        Add(command, "@Id", SqlDbType.BigInt, statement.Id);
        Add(command, "@EntityId", SqlDbType.BigInt, statement.EntityId);
        Add(command, "@PeriodFrom", SqlDbType.Char, statement.PeriodFrom, 7);
        Add(command, "@PeriodTo", SqlDbType.Char, statement.PeriodTo, 7);
        Add(command, "@SubmissionDate", SqlDbType.Date, ToDbDate(statement.SubmissionDate));
        Add(command, "@PreparationDate", SqlDbType.Date, ToDbDate(statement.PreparationDate));
        Add(command, "@ApprovalDate", SqlDbType.Date, ToDbDate(statement.ApprovalDate));
        Add(command, "@AssemblyDate", SqlDbType.Date, ToDbDate(statement.AssemblyDate));
        Add(command, "@AuditorReportAttachmentDate", SqlDbType.Date, ToDbDate(statement.AuditorReportAttachmentDate));
        Add(command, "@FundName", SqlDbType.NVarChar, statement.FundName, 500);
        Add(command, "@LeiCode", SqlDbType.VarChar, statement.LeiCode, 20);
        Add(command, "@IsConsolidated", SqlDbType.Bit, statement.IsConsolidated);
        Add(command, "@IsConsolidatedCentralGovernment", SqlDbType.Bit, statement.IsConsolidatedCentralGovernment);
        Add(command, "@IsSummaryPublicAdministration", SqlDbType.Bit, statement.IsSummaryPublicAdministration);
        Add(command, "@StatementType", SqlDbType.NVarChar, statement.Type, 100);
        Add(command, "@DataSourceCode", SqlDbType.VarChar, statement.DataSourceCode, 30);
        Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(statement.LastModifiedDate));
        Add(command, "@SourceStatus", SqlDbType.NVarChar, statement.Status, 30);
        Add(command, "@IsDeleted", SqlDbType.Bit, IsDeleted(statement.Status));
        Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
        Add(command, "@PayloadVersionId", SqlDbType.BigInt, payloadVersionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveAnnualReportAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        RegisterUzDocument<AnnualReportDto> document,
        long payloadVersionId,
        CancellationToken cancellationToken)
    {
        AnnualReportDto report = document.Value;
        await using (var command = CreateCommand(connection, transaction, """
            UPDATE [Reporting].[AnnualReport]
            SET [RegisterUzEntityId] = @EntityId,
                [EntityNameAtSubmission] = @Name, [AnnualReportType] = @Type,
                [FundName] = @FundName, [LeiCode] = @LeiCode,
                [PeriodFrom] = @PeriodFrom, [PeriodTo] = @PeriodTo,
                [SubmissionDate] = @SubmissionDate, [AssemblyDate] = @AssemblyDate,
                [DataAvailability] = @DataAvailability, [DataSourceCode] = @DataSourceCode,
                [SourceLastModifiedDate] = @SourceLastModifiedDate,
                [SourceStatus] = @SourceStatus, [IsDeleted] = @IsDeleted,
                [LastObservedAtUtc] = @RetrievedAtUtc,
                [LastDetailRetrievedAtUtc] = @RetrievedAtUtc,
                [CurrentPayloadVersionId] = @PayloadVersionId,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzAnnualReportId] = @Id;
            IF @@ROWCOUNT = 0
                INSERT INTO [Reporting].[AnnualReport]
                (
                    [RegisterUzAnnualReportId], [RegisterUzEntityId], [EntityNameAtSubmission],
                    [AnnualReportType], [FundName], [LeiCode], [PeriodFrom], [PeriodTo],
                    [SubmissionDate], [AssemblyDate], [DataAvailability], [DataSourceCode],
                    [SourceLastModifiedDate], [SourceStatus], [IsDeleted],
                    [FirstObservedAtUtc], [LastObservedAtUtc], [LastDetailRetrievedAtUtc],
                    [CurrentPayloadVersionId]
                )
                VALUES
                (
                    @Id, @EntityId, @Name, @Type, @FundName, @LeiCode, @PeriodFrom, @PeriodTo,
                    @SubmissionDate, @AssemblyDate, @DataAvailability, @DataSourceCode,
                    @SourceLastModifiedDate, @SourceStatus, @IsDeleted,
                    @RetrievedAtUtc, @RetrievedAtUtc, @RetrievedAtUtc, @PayloadVersionId
                );
            """))
        {
            Add(command, "@Id", SqlDbType.BigInt, report.Id);
            Add(command, "@EntityId", SqlDbType.BigInt, report.EntityId);
            Add(command, "@Name", SqlDbType.NVarChar, report.EntityNameAtSubmission, 500);
            Add(command, "@Type", SqlDbType.NVarChar, report.Type, 100);
            Add(command, "@FundName", SqlDbType.NVarChar, report.FundName, 500);
            Add(command, "@LeiCode", SqlDbType.VarChar, report.LeiCode, 20);
            Add(command, "@PeriodFrom", SqlDbType.Char, report.PeriodFrom, 7);
            Add(command, "@PeriodTo", SqlDbType.Char, report.PeriodTo, 7);
            Add(command, "@SubmissionDate", SqlDbType.Date, ToDbDate(report.SubmissionDate));
            Add(command, "@AssemblyDate", SqlDbType.Date, ToDbDate(report.AssemblyDate));
            Add(command, "@DataAvailability", SqlDbType.NVarChar, report.DataAvailability, 30);
            Add(command, "@DataSourceCode", SqlDbType.VarChar, report.DataSourceCode, 30);
            Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(report.LastModifiedDate));
            Add(command, "@SourceStatus", SqlDbType.NVarChar, report.Status, 30);
            Add(command, "@IsDeleted", SqlDbType.Bit, IsDeleted(report.Status));
            Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
            Add(command, "@PayloadVersionId", SqlDbType.BigInt, payloadVersionId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await ReplaceAnnualAttachmentsAsync(connection, transaction, report, document.RetrievedAtUtc, cancellationToken);
    }

    private static async Task ReplaceAnnualAttachmentsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        AnnualReportDto report,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await using (var delete = CreateCommand(connection, transaction,
                         "DELETE FROM [Reporting].[AnnualReportAttachment] WHERE [RegisterUzAnnualReportId] = @Id;"))
        {
            Add(delete, "@Id", SqlDbType.BigInt, report.Id);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (AttachmentDto attachment in report.Attachments)
        {
            await using var command = CreateCommand(connection, transaction, """
                INSERT INTO [Reporting].[AnnualReportAttachment]
                (
                    [RegisterUzAttachmentId], [RegisterUzAnnualReportId], [FileName],
                    [MimeType], [FileSizeBytes], [DigestSha256], [LanguageCode],
                    [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @AttachmentId, @ReportId, @FileName,
                    @MimeType, @FileSizeBytes, @Digest, @LanguageCode,
                    @ObservedAtUtc, @ObservedAtUtc
                );
                """);
            AddAttachmentParameters(command, attachment, report.Id, observedAtUtc);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task SaveFinancialReportAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        RegisterUzDocument<FinancialReportDto> document,
        FinancialReportTemplateDto? template,
        long payloadVersionId,
        CancellationToken cancellationToken)
    {
        FinancialReportDto report = document.Value;
        bool hasStatement = report.FinancialStatementId.HasValue;
        bool hasAnnualReport = report.AnnualReportId.HasValue;
        if (hasStatement == hasAnnualReport)
            throw new InvalidOperationException($"Financial report {report.Id} does not have exactly one parent.");

        await using (var command = CreateCommand(connection, transaction, """
            UPDATE [Reporting].[FinancialReport]
            SET [RegisterUzStatementId] = @StatementId,
                [RegisterUzAnnualReportId] = @AnnualReportId,
                [RegisterUzTemplateId] = @TemplateId,
                [CurrencyCode] = @CurrencyCode, [TaxOfficeCode] = @TaxOfficeCode,
                [DataAvailability] = @DataAvailability, [DataSourceCode] = @DataSourceCode,
                [SourceLastModifiedDate] = @SourceLastModifiedDate,
                [SourceStatus] = @SourceStatus, [IsDeleted] = @IsDeleted,
                [LastObservedAtUtc] = @RetrievedAtUtc,
                [LastDetailRetrievedAtUtc] = @RetrievedAtUtc,
                [CurrentPayloadVersionId] = @PayloadVersionId,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzFinancialReportId] = @Id;
            IF @@ROWCOUNT = 0
                INSERT INTO [Reporting].[FinancialReport]
                (
                    [RegisterUzFinancialReportId], [RegisterUzStatementId],
                    [RegisterUzAnnualReportId], [RegisterUzTemplateId],
                    [CurrencyCode], [TaxOfficeCode], [DataAvailability], [DataSourceCode],
                    [SourceLastModifiedDate], [SourceStatus], [IsDeleted],
                    [FirstObservedAtUtc], [LastObservedAtUtc], [LastDetailRetrievedAtUtc],
                    [CurrentPayloadVersionId]
                )
                VALUES
                (
                    @Id, @StatementId, @AnnualReportId, @TemplateId,
                    @CurrencyCode, @TaxOfficeCode, @DataAvailability, @DataSourceCode,
                    @SourceLastModifiedDate, @SourceStatus, @IsDeleted,
                    @RetrievedAtUtc, @RetrievedAtUtc, @RetrievedAtUtc, @PayloadVersionId
                );
            """))
        {
            Add(command, "@Id", SqlDbType.BigInt, report.Id);
            Add(command, "@StatementId", SqlDbType.BigInt, report.FinancialStatementId);
            Add(command, "@AnnualReportId", SqlDbType.BigInt, report.AnnualReportId);
            Add(command, "@TemplateId", SqlDbType.BigInt, report.TemplateId);
            Add(command, "@CurrencyCode", SqlDbType.VarChar, report.CurrencyCode, 9);
            Add(command, "@TaxOfficeCode", SqlDbType.VarChar, report.TaxOfficeCode, 3);
            Add(command, "@DataAvailability", SqlDbType.NVarChar, report.DataAvailability, 30);
            Add(command, "@DataSourceCode", SqlDbType.VarChar, report.DataSourceCode, 30);
            Add(command, "@SourceLastModifiedDate", SqlDbType.Date, ToDbDate(report.LastModifiedDate));
            Add(command, "@SourceStatus", SqlDbType.NVarChar, report.Status, 30);
            Add(command, "@IsDeleted", SqlDbType.Bit, IsDeleted(report.Status));
            Add(command, "@RetrievedAtUtc", SqlDbType.DateTime2, document.RetrievedAtUtc);
            Add(command, "@PayloadVersionId", SqlDbType.BigInt, payloadVersionId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await ReplaceFinancialReportChildrenAsync(
            connection, transaction, report, template, document.RetrievedAtUtc, cancellationToken);
    }

    private static async Task ReplaceFinancialReportChildrenAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        FinancialReportDto report,
        FinancialReportTemplateDto? template,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        await using (var delete = CreateCommand(connection, transaction, """
            DELETE FROM [Reporting].[FinancialReportAttachment]
            WHERE [RegisterUzFinancialReportId] = @Id;
            DELETE FROM [Reporting].[FinancialReportTitlePage]
            WHERE [RegisterUzFinancialReportId] = @Id;
            DELETE FROM [Reporting].[FinancialReportTable]
            WHERE [RegisterUzFinancialReportId] = @Id;
            """))
        {
            Add(delete, "@Id", SqlDbType.BigInt, report.Id);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (AttachmentDto attachment in report.Attachments)
        {
            await using var command = CreateCommand(connection, transaction, """
                INSERT INTO [Reporting].[FinancialReportAttachment]
                (
                    [RegisterUzAttachmentId], [RegisterUzFinancialReportId], [FileName],
                    [MimeType], [FileSizeBytes], [PageCount], [DigestSha256], [LanguageCode],
                    [FirstObservedAtUtc], [LastObservedAtUtc]
                )
                VALUES
                (
                    @AttachmentId, @ReportId, @FileName,
                    @MimeType, @FileSizeBytes, @PageCount, @Digest, @LanguageCode,
                    @ObservedAtUtc, @ObservedAtUtc
                );
                """);
            AddAttachmentParameters(command, attachment, report.Id, observedAtUtc);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        if (report.Content?.TitlePage is not null)
            await SaveTitlePageAsync(connection, transaction, report.Id, report.Content.TitlePage, cancellationToken);

        FinancialReportTableDto[] reportTables = report.Content?.Tables ?? [];
        if (reportTables.Length == 0)
            return;

        if (template is null)
            throw new InvalidOperationException($"Financial report {report.Id} has structured tables but no template was available.");
        if (reportTables.Length != template.Tables.Length)
            throw new InvalidOperationException(
                $"Financial report {report.Id} contains {reportTables.Length} tables, " +
                $"but template {template.Id} contains {template.Tables.Length}.");

        for (int tableOrdinal = 0; tableOrdinal < reportTables.Length; tableOrdinal++)
        {
            FinancialReportTableDto reportTable = reportTables[tableOrdinal];
            TemplateTableDto templateTable = template.Tables[tableOrdinal];
            if (templateTable.Rows.Length == 0)
                throw new InvalidOperationException($"Template {template.Id}, table {tableOrdinal} has no rows.");
            if (reportTable.Data.Length % templateTable.Rows.Length != 0)
                throw new InvalidOperationException(
                    $"Report {report.Id}, table {tableOrdinal}: {reportTable.Data.Length} values cannot be divided " +
                    $"across {templateTable.Rows.Length} template rows.");

            int dataColumnCount = reportTable.Data.Length / templateTable.Rows.Length;
            long templateTableId = await GetTemplateTableIdAsync(
                connection, transaction, template.Id, tableOrdinal, cancellationToken);
            await UpdateTemplateDataColumnCountAsync(
                connection, transaction, templateTableId, dataColumnCount, cancellationToken);
            long reportTableId = await InsertReportTableAsync(
                connection, transaction, report.Id, templateTableId,
                tableOrdinal, reportTable, cancellationToken);

            for (int valueOrdinal = 0; valueOrdinal < reportTable.Data.Length; valueOrdinal++)
            {
                int rowOrdinal = valueOrdinal / dataColumnCount;
                int dataColumnOrdinal = valueOrdinal % dataColumnCount;
                await InsertReportValueAsync(
                    connection, transaction, reportTableId, valueOrdinal,
                    rowOrdinal, dataColumnOrdinal, reportTable.Data[valueOrdinal], cancellationToken);
            }
        }
    }

    private static async Task SaveTitlePageAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long reportId,
        TitlePageDto page,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            INSERT INTO [Reporting].[FinancialReportTitlePage]
            (
                [RegisterUzFinancialReportId], [EntityName], [Ico], [Dic], [Sid], [Address],
                [LegalFormCode], [SkNaceCode], [ReportType], [IsConsolidated],
                [IsConsolidatedCentralGovernment], [IsSummaryPublicAdministration],
                [EntityType], [CommercialRegister], [FundName], [LeiCode],
                [PeriodFrom], [PeriodTo], [PreviousPeriodFrom], [PreviousPeriodTo],
                [CompletionDate], [ApprovalDate], [PreparationDate], [AssemblyDate],
                [AuditorReportAttachmentDate]
            )
            VALUES
            (
                @ReportId, @EntityName, @Ico, @Dic, @Sid, @Address,
                @LegalFormCode, @SkNaceCode, @ReportType, @IsConsolidated,
                @IsConsolidatedCentralGovernment, @IsSummaryPublicAdministration,
                @EntityType, @CommercialRegister, @FundName, @LeiCode,
                @PeriodFrom, @PeriodTo, @PreviousPeriodFrom, @PreviousPeriodTo,
                @CompletionDate, @ApprovalDate, @PreparationDate, @AssemblyDate,
                @AuditorReportAttachmentDate
            );
            """);
        Add(command, "@ReportId", SqlDbType.BigInt, reportId);
        Add(command, "@EntityName", SqlDbType.NVarChar, page.EntityName, 500);
        Add(command, "@Ico", SqlDbType.VarChar, page.Ico, 20);
        Add(command, "@Dic", SqlDbType.VarChar, page.Dic, 20);
        Add(command, "@Sid", SqlDbType.VarChar, page.Sid, 20);
        Add(command, "@Address", SqlDbType.NVarChar, page.Address?.ToString(), -1);
        Add(command, "@LegalFormCode", SqlDbType.VarChar, page.LegalFormCode, 100);
        Add(command, "@SkNaceCode", SqlDbType.VarChar, page.SkNaceCode, 100);
        Add(command, "@ReportType", SqlDbType.NVarChar, page.ReportType, 100);
        Add(command, "@IsConsolidated", SqlDbType.Bit, page.IsConsolidated);
        Add(command, "@IsConsolidatedCentralGovernment", SqlDbType.Bit, page.IsConsolidatedCentralGovernment);
        Add(command, "@IsSummaryPublicAdministration", SqlDbType.Bit, page.IsSummaryPublicAdministration);
        Add(command, "@EntityType", SqlDbType.NVarChar, page.EntityType, 100);
        Add(command, "@CommercialRegister", SqlDbType.NVarChar, page.CommercialRegister, -1);
        Add(command, "@FundName", SqlDbType.NVarChar, page.FundName, 500);
        Add(command, "@LeiCode", SqlDbType.VarChar, page.LeiCode, 20);
        Add(command, "@PeriodFrom", SqlDbType.Char, page.PeriodFrom, 7);
        Add(command, "@PeriodTo", SqlDbType.Char, page.PeriodTo, 7);
        Add(command, "@PreviousPeriodFrom", SqlDbType.Char, page.PreviousPeriodFrom, 7);
        Add(command, "@PreviousPeriodTo", SqlDbType.Char, page.PreviousPeriodTo, 7);
        Add(command, "@CompletionDate", SqlDbType.Date, ToDbDate(page.CompletionDate));
        Add(command, "@ApprovalDate", SqlDbType.Date, ToDbDate(page.ApprovalDate));
        Add(command, "@PreparationDate", SqlDbType.Date, ToDbDate(page.PreparationDate));
        Add(command, "@AssemblyDate", SqlDbType.Date, ToDbDate(page.AssemblyDate));
        Add(command, "@AuditorReportAttachmentDate", SqlDbType.Date, ToDbDate(page.AuditorReportAttachmentDate));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> GetTemplateTableIdAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long templateId,
        int tableOrdinal,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            SELECT [TemplateTableId]
            FROM [Templates].[TemplateTable]
            WHERE [RegisterUzTemplateId] = @TemplateId AND [TableOrdinal] = @Ordinal;
            """);
        Add(command, "@TemplateId", SqlDbType.BigInt, templateId);
        Add(command, "@Ordinal", SqlDbType.Int, tableOrdinal);
        object? value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null || value == DBNull.Value
            ? throw new InvalidOperationException($"Template {templateId}, table ordinal {tableOrdinal} was not stored.")
            : Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static async Task UpdateTemplateDataColumnCountAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long templateTableId,
        int dataColumnCount,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            UPDATE [Templates].[TemplateTable]
            SET [NumberOfDataColumns] = @Count, [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [TemplateTableId] = @TableId
              AND ([NumberOfDataColumns] IS NULL OR [NumberOfDataColumns] = @Count);
            IF @@ROWCOUNT = 0
                THROW 50001, 'Template data-column count conflicts with a previously observed report.', 1;
            """);
        Add(command, "@TableId", SqlDbType.BigInt, templateTableId);
        Add(command, "@Count", SqlDbType.Int, dataColumnCount);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<long> InsertReportTableAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long reportId,
        long templateTableId,
        int tableOrdinal,
        FinancialReportTableDto table,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            INSERT INTO [Reporting].[FinancialReportTable]
                ([RegisterUzFinancialReportId], [TemplateTableId], [TableOrdinal], [NameSk], [NameEn])
            OUTPUT INSERTED.[FinancialReportTableId]
            VALUES
                (@ReportId, @TemplateTableId, @Ordinal, @NameSk, @NameEn);
            """);
        Add(command, "@ReportId", SqlDbType.BigInt, reportId);
        Add(command, "@TemplateTableId", SqlDbType.BigInt, templateTableId);
        Add(command, "@Ordinal", SqlDbType.Int, tableOrdinal);
        Add(command, "@NameSk", SqlDbType.NVarChar, table.Name?.Sk, 250);
        Add(command, "@NameEn", SqlDbType.NVarChar, table.Name?.En, 250);
        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private static async Task InsertReportValueAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        long reportTableId,
        int valueOrdinal,
        int rowOrdinal,
        int dataColumnOrdinal,
        string? sourceValue,
        CancellationToken cancellationToken)
    {
        decimal? numericValue = null;
        if (!string.IsNullOrWhiteSpace(sourceValue))
        {
            if (!decimal.TryParse(sourceValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed))
                throw new InvalidOperationException($"Financial report value '{sourceValue}' is not a valid invariant decimal.");
            numericValue = parsed;
        }

        await using var command = CreateCommand(connection, transaction, """
            INSERT INTO [Reporting].[FinancialReportValue]
            (
                [FinancialReportTableId], [ValueOrdinal], [RowOrdinal],
                [DataColumnOrdinal], [NumericValue], [SourceValue]
            )
            VALUES
            (
                @TableId, @ValueOrdinal, @RowOrdinal,
                @DataColumnOrdinal, @NumericValue, @SourceValue
            );
            """);
        Add(command, "@TableId", SqlDbType.BigInt, reportTableId);
        Add(command, "@ValueOrdinal", SqlDbType.Int, valueOrdinal);
        Add(command, "@RowOrdinal", SqlDbType.Int, rowOrdinal);
        Add(command, "@DataColumnOrdinal", SqlDbType.Int, dataColumnOrdinal);
        SqlParameter numeric = command.Parameters.Add("@NumericValue", SqlDbType.Decimal);
        numeric.Precision = 38;
        numeric.Scale = 10;
        numeric.Value = numericValue.HasValue ? numericValue.Value : DBNull.Value;
        Add(command, "@SourceValue", SqlDbType.NVarChar, sourceValue, 100);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddAttachmentParameters(
        SqlCommand command,
        AttachmentDto attachment,
        long reportId,
        DateTime observedAtUtc)
    {
        Add(command, "@AttachmentId", SqlDbType.BigInt, attachment.Id);
        Add(command, "@ReportId", SqlDbType.BigInt, reportId);
        Add(command, "@FileName", SqlDbType.NVarChar, attachment.FileName, 255);
        Add(command, "@MimeType", SqlDbType.VarChar, attachment.MimeType, 100);
        Add(command, "@FileSizeBytes", SqlDbType.BigInt, attachment.FileSizeBytes);
        Add(command, "@PageCount", SqlDbType.Int, attachment.PageCount);
        Add(command, "@Digest", SqlDbType.Binary, ParseSha256(attachment.DigestSha256), 32);
        Add(command, "@LanguageCode", SqlDbType.VarChar, attachment.LanguageCode, 10);
        Add(command, "@ObservedAtUtc", SqlDbType.DateTime2, observedAtUtc);
    }

    private static byte[]? ParseSha256(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (value.Length != 64)
            throw new InvalidOperationException($"Attachment digest '{value}' is not a 64-character SHA-256 value.");
        try
        {
            return Convert.FromHexString(value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"Attachment digest '{value}' is not hexadecimal.", exception);
        }
    }

    private static byte[] Compress(byte[] bytes)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            gzip.Write(bytes, 0, bytes.Length);
        return output.ToArray();
    }

    private static bool IsDeleted(string? status) =>
        string.Equals(status, "ZMAZANÉ", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "ZMAZANE", StringComparison.OrdinalIgnoreCase);

    private static object ToDbDate(DateOnly? value) =>
        value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        SqlTransaction transaction,
        string commandText) =>
        new(commandText, connection, transaction);

    private static void Add(
        SqlCommand command,
        string name,
        SqlDbType type,
        object? value,
        int size = 0)
    {
        SqlParameter parameter = size == 0
            ? command.Parameters.Add(name, type)
            : command.Parameters.Add(name, type, size);
        parameter.Value = value ?? DBNull.Value;
    }
}
