using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using RegisterUz.Core;

namespace RegisterUz.Persistence.SqlServer;

public sealed class SqlRegisterUzChangeFeedRepository : IRegisterUzChangeFeedRepository
{
    private readonly string _connectionString;

    public SqlRegisterUzChangeFeedRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("RegisterUZ connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public async Task<RegisterUzChangeFeedSession> BeginOrResumeAsync(
        RegisterUzObjectType objectType,
        DateTime initialChangedSinceUtc,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                SET XACT_ABORT ON;
                DECLARE @NowUtc datetime2(0) = CONVERT(datetime2(0), SYSUTCDATETIME());
                DECLARE @LockResult int;
                DECLARE @LockResource nvarchar(255) =
                    CONCAT(N'RegisterUZ.ChangeFeed.', @ObjectTypeId);

                EXEC @LockResult = sys.sp_getapplock
                    @Resource = @LockResource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Transaction',
                    @LockTimeout = 0;
                IF @LockResult < 0
                    THROW 51500, 'Another collector is already starting this change feed.', 1;

                IF @InitialChangedSinceUtc > @NowUtc
                    THROW 51501, 'Initial changed-since timestamp cannot be in the future.', 1;

                INSERT INTO [Sync].[Run] ([RunType], [RequestedBy], [Notes])
                VALUES
                (
                    'ChangeFeed', ORIGINAL_LOGIN(),
                    CONCAT(N'Object type ', @ObjectTypeId, N'; page size ', @PageSize)
                );
                DECLARE @SyncRunId bigint = CONVERT(bigint, SCOPE_IDENTITY());

                IF NOT EXISTS
                (
                    SELECT 1 FROM [Sync].[ChangeFeedCheckpoint] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [ObjectTypeId] = @ObjectTypeId
                )
                BEGIN
                    INSERT INTO [Sync].[ChangeFeedCheckpoint]
                    (
                        [ObjectTypeId], [ChangedSinceUtc], [ScanStartedAtUtc],
                        [ContinueAfterId], [PageSize], [Status], [LastRunId],
                        [UpdatedAtUtc]
                    )
                    VALUES
                    (
                        @ObjectTypeId, @InitialChangedSinceUtc, @NowUtc,
                        NULL, @PageSize, 'Running', @SyncRunId, SYSUTCDATETIME()
                    );
                END
                ELSE
                BEGIN
                    UPDATE [Sync].[ChangeFeedCheckpoint]
                    SET [ScanStartedAtUtc] =
                            CASE WHEN [Status] IN ('Completed', 'Pending')
                                 THEN @NowUtc ELSE [ScanStartedAtUtc] END,
                        [ContinueAfterId] =
                            CASE WHEN [Status] IN ('Completed', 'Pending')
                                 THEN NULL ELSE [ContinueAfterId] END,
                        [PageSize] = @PageSize,
                        [Status] = 'Running',
                        [LastRunId] = @SyncRunId,
                        [CompletedAtUtc] = NULL,
                        [UpdatedAtUtc] = SYSUTCDATETIME()
                    WHERE [ObjectTypeId] = @ObjectTypeId;
                END;

                SELECT
                    @SyncRunId,
                    [ObjectTypeId], [ChangedSinceUtc], [ScanStartedAtUtc],
                    [ContinueAfterId], [PageSize]
                FROM [Sync].[ChangeFeedCheckpoint]
                WHERE [ObjectTypeId] = @ObjectTypeId;
                """;
            Add(command, "@ObjectTypeId", SqlDbType.TinyInt, (byte)objectType);
            Add(command, "@InitialChangedSinceUtc", SqlDbType.DateTime2, initialChangedSinceUtc);
            Add(command, "@PageSize", SqlDbType.Int, pageSize);

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                throw new InvalidOperationException("Change-feed checkpoint was not returned.");
            var session = new RegisterUzChangeFeedSession(
                reader.GetInt64(0),
                (RegisterUzObjectType)reader.GetByte(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
                DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc),
                reader.IsDBNull(4) ? null : reader.GetInt64(4),
                reader.GetInt32(5));
            await reader.CloseAsync();
            await transaction.CommitAsync(cancellationToken);
            return session;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<RegisterUzChangeFeedPageSaveResult> SavePageAsync(
        RegisterUzChangeFeedSession session,
        RegisterUzChangeFeedPage page,
        CancellationToken cancellationToken = default)
    {
        long[] ids = page.Document.Value.Ids;
        long? nextId = page.Document.Value.HasMoreIds ? ids[^1] : null;
        string idsJson = JsonSerializer.Serialize(ids);
        long responseBytes = Encoding.UTF8.GetByteCount(page.Document.RawJson);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlTransaction transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                SET XACT_ABORT ON;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM [Sync].[ChangeFeedCheckpoint] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [ObjectTypeId] = @ObjectTypeId
                      AND [LastRunId] = @SyncRunId
                      AND [Status] = 'Running'
                      AND [ChangedSinceUtc] = @ChangedSinceUtc
                      AND [ScanStartedAtUtc] = @ScanStartedAtUtc
                      AND (([ContinueAfterId] IS NULL AND @ExpectedContinueAfterId IS NULL)
                           OR [ContinueAfterId] = @ExpectedContinueAfterId)
                )
                    THROW 51502, 'Change-feed checkpoint no longer matches this page.', 1;

                INSERT INTO [Sync].[Request]
                (
                    [SyncRunId], [ObjectTypeId], [RequestKind], [RequestUri],
                    [RequestedAtUtc], [CompletedAtUtc], [HttpStatusCode],
                    [ApiVersion], [ResponseBytes], [Succeeded]
                )
                VALUES
                (
                    @SyncRunId, @ObjectTypeId, 'ChangeFeedPage', @RequestUri,
                    @RequestedAtUtc, @CompletedAtUtc, @HttpStatusCode,
                    @ApiVersion, @ResponseBytes, 1
                );

                DECLARE @Ids TABLE ([RegisterUzObjectId] bigint NOT NULL PRIMARY KEY);
                INSERT INTO @Ids ([RegisterUzObjectId])
                SELECT CONVERT(bigint, [value]) FROM OPENJSON(@IdsJson);

                UPDATE o
                SET [LastObservedAtUtc] = @CompletedAtUtc,
                    [LastObservedInRunId] = @SyncRunId,
                    [ObservationCount] = [ObservationCount] + 1
                FROM [Sync].[ObservedObject] o
                JOIN @Ids i
                  ON i.[RegisterUzObjectId] = o.[RegisterUzObjectId]
                WHERE o.[ObjectTypeId] = @ObjectTypeId;

                INSERT INTO [Sync].[ObservedObject]
                (
                    [ObjectTypeId], [RegisterUzObjectId],
                    [FirstObservedAtUtc], [LastObservedAtUtc],
                    [FirstObservedInRunId], [LastObservedInRunId]
                )
                SELECT
                    @ObjectTypeId, i.[RegisterUzObjectId],
                    @CompletedAtUtc, @CompletedAtUtc, @SyncRunId, @SyncRunId
                FROM @Ids i
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM [Sync].[ObservedObject] o WITH (UPDLOCK, HOLDLOCK)
                    WHERE o.[ObjectTypeId] = @ObjectTypeId
                      AND o.[RegisterUzObjectId] = i.[RegisterUzObjectId]
                );

                UPDATE [Sync].[Run]
                SET [ObservedIdCount] = [ObservedIdCount] + @ObservedIdCount
                WHERE [SyncRunId] = @SyncRunId;

                IF @HasMoreIds = 1
                BEGIN
                    UPDATE [Sync].[ChangeFeedCheckpoint]
                    SET [ContinueAfterId] = @NextContinueAfterId,
                        [LastPageRetrievedAtUtc] = @CompletedAtUtc,
                        [UpdatedAtUtc] = SYSUTCDATETIME()
                    WHERE [ObjectTypeId] = @ObjectTypeId;
                END
                ELSE
                BEGIN
                    UPDATE [Sync].[ChangeFeedCheckpoint]
                    SET [ChangedSinceUtc] = [ScanStartedAtUtc],
                        [ScanStartedAtUtc] = NULL,
                        [ContinueAfterId] = NULL,
                        [Status] = 'Completed',
                        [LastPageRetrievedAtUtc] = @CompletedAtUtc,
                        [CompletedAtUtc] = @CompletedAtUtc,
                        [UpdatedAtUtc] = SYSUTCDATETIME()
                    WHERE [ObjectTypeId] = @ObjectTypeId;

                    UPDATE [Sync].[Run]
                    SET [Status] = 'Completed', [CompletedAtUtc] = @CompletedAtUtc,
                        [Notes] = CONCAT([Notes], N'; feed completed')
                    WHERE [SyncRunId] = @SyncRunId;
                END;
                """;
            AddSessionParameters(command, session);
            Add(command, "@ExpectedContinueAfterId", SqlDbType.BigInt, page.ContinueAfterId);
            Add(command, "@RequestUri", SqlDbType.NVarChar, page.RequestPath, 2000);
            Add(command, "@RequestedAtUtc", SqlDbType.DateTime2, page.RequestedAtUtc);
            Add(command, "@CompletedAtUtc", SqlDbType.DateTime2, page.Document.RetrievedAtUtc);
            Add(command, "@HttpStatusCode", SqlDbType.Int, page.Document.HttpStatusCode);
            Add(command, "@ApiVersion", SqlDbType.VarChar, page.Document.ApiVersion, 50);
            Add(command, "@ResponseBytes", SqlDbType.BigInt, responseBytes);
            Add(command, "@IdsJson", SqlDbType.NVarChar, idsJson, -1);
            Add(command, "@ObservedIdCount", SqlDbType.BigInt, ids.LongLength);
            Add(command, "@HasMoreIds", SqlDbType.Bit, page.Document.Value.HasMoreIds);
            Add(command, "@NextContinueAfterId", SqlDbType.BigInt, nextId);
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new RegisterUzChangeFeedPageSaveResult(!page.Document.Value.HasMoreIds, nextId);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task CompleteBoundedRunAsync(
        RegisterUzChangeFeedSession session,
        int pagesRetrieved,
        long observedIdCount,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(session, """
            UPDATE [Sync].[Run]
            SET [Status] = 'Completed', [CompletedAtUtc] = SYSUTCDATETIME(),
                [Notes] = CONCAT([Notes], N'; bounded after ', @PagesRetrieved,
                                 N' page(s), ', @ObservedIdCount, N' observed ID(s)')
            WHERE [SyncRunId] = @SyncRunId AND [Status] = 'Running';

            UPDATE [Sync].[ChangeFeedCheckpoint]
            SET [Status] = 'Paused', [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [ObjectTypeId] = @ObjectTypeId
              AND [LastRunId] = @SyncRunId
              AND [Status] = 'Running';
            """, pagesRetrieved, observedIdCount, null, null, cancellationToken);
    }

    public async Task FailRunAsync(
        RegisterUzChangeFeedSession session,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        IRegisterUzRequestFailure? requestFailure = exception as IRegisterUzRequestFailure;
        await ExecuteAsync(session, """
            DECLARE @SyncRequestId bigint = NULL;
            IF @RequestUri IS NOT NULL
            BEGIN
                INSERT INTO [Sync].[Request]
                (
                    [SyncRunId], [ObjectTypeId], [RequestKind], [RequestUri],
                    [RequestedAtUtc], [CompletedAtUtc], [HttpStatusCode],
                    [ApiVersion], [ResponseBytes], [Succeeded], [ErrorMessage]
                )
                VALUES
                (
                    @SyncRunId, @ObjectTypeId, 'ChangeFeedPage', @RequestUri,
                    @RequestedAtUtc, SYSUTCDATETIME(), @HttpStatusCode,
                    @ApiVersion, @ResponseBytes, 0, @ErrorMessage
                );
                SET @SyncRequestId = CONVERT(bigint, SCOPE_IDENTITY());
            END;

            UPDATE [Sync].[Run]
            SET [Status] = 'Failed', [CompletedAtUtc] = SYSUTCDATETIME(),
                [ErrorCount] = [ErrorCount] + 1
            WHERE [SyncRunId] = @SyncRunId AND [Status] = 'Running';

            UPDATE [Sync].[ChangeFeedCheckpoint]
            SET [Status] = 'Failed', [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [ObjectTypeId] = @ObjectTypeId AND [LastRunId] = @SyncRunId;

            INSERT INTO [Sync].[Error]
                ([SyncRunId], [SyncRequestId], [ErrorStage], [ErrorCode], [Message], [Details])
            VALUES
                (@SyncRunId, @SyncRequestId, 'ChangeFeed', @ErrorCode, @ErrorMessage, @ErrorDetails);
            """, 0, 0, exception, requestFailure, cancellationToken);
    }

    public async Task CancelRunAsync(
        RegisterUzChangeFeedSession session,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(session, """
            UPDATE [Sync].[Run]
            SET [Status] = 'Cancelled', [CompletedAtUtc] = SYSUTCDATETIME(),
                [Notes] = CONCAT([Notes], N'; cancelled')
            WHERE [SyncRunId] = @SyncRunId AND [Status] = 'Running';

            UPDATE [Sync].[ChangeFeedCheckpoint]
            SET [Status] = 'Paused', [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [ObjectTypeId] = @ObjectTypeId AND [LastRunId] = @SyncRunId;
            """, 0, 0, null, null, cancellationToken);
    }

    private async Task ExecuteAsync(
        RegisterUzChangeFeedSession session,
        string sql,
        int pagesRetrieved,
        long observedIdCount,
        Exception? exception,
        IRegisterUzRequestFailure? requestFailure,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddSessionParameters(command, session);
        Add(command, "@PagesRetrieved", SqlDbType.Int, pagesRetrieved);
        Add(command, "@ObservedIdCount", SqlDbType.BigInt, observedIdCount);
        Add(command, "@ErrorCode", SqlDbType.VarChar, exception?.GetType().Name, 100);
        Add(command, "@ErrorMessage", SqlDbType.NVarChar, exception?.Message, -1);
        Add(command, "@ErrorDetails", SqlDbType.NVarChar, exception?.ToString(), -1);
        Add(command, "@RequestUri", SqlDbType.NVarChar, requestFailure?.RequestPath, 2000);
        Add(command, "@RequestedAtUtc", SqlDbType.DateTime2, requestFailure?.RequestedAtUtc);
        Add(command, "@HttpStatusCode", SqlDbType.Int, requestFailure?.HttpStatusCode);
        Add(command, "@ApiVersion", SqlDbType.VarChar, requestFailure?.ApiVersion, 50);
        Add(command, "@ResponseBytes", SqlDbType.BigInt, requestFailure?.ResponseBytes);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddSessionParameters(SqlCommand command, RegisterUzChangeFeedSession session)
    {
        Add(command, "@SyncRunId", SqlDbType.BigInt, session.SyncRunId);
        Add(command, "@ObjectTypeId", SqlDbType.TinyInt, (byte)session.ObjectType);
        Add(command, "@ChangedSinceUtc", SqlDbType.DateTime2, session.ChangedSinceUtc);
        Add(command, "@ScanStartedAtUtc", SqlDbType.DateTime2, session.ScanStartedAtUtc);
    }

    private static void Add(
        SqlCommand command,
        string name,
        SqlDbType type,
        object? value,
        int? size = null)
    {
        SqlParameter parameter = command.Parameters.Add(name, type);
        if (size.HasValue)
            parameter.Size = size.Value;
        parameter.Value = value ?? DBNull.Value;
    }
}
