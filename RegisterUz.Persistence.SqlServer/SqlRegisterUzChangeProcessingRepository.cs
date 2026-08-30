using System.Data;
using Microsoft.Data.SqlClient;
using RegisterUz.Core;

namespace RegisterUz.Persistence.SqlServer;

public sealed class SqlRegisterUzChangeProcessingRepository : IRegisterUzChangeProcessingRepository
{
    private readonly string _connectionString;

    public SqlRegisterUzChangeProcessingRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("RegisterUZ connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<RegisterUzObservationClaim>> ClaimObservationsAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ValidateBatch(batchSize);
        int leaseSeconds = ValidateLease(leaseDuration);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SET XACT_ABORT ON;

            INSERT INTO [Sync].[ObservedObjectWork]
            (
                [ObjectTypeId], [RegisterUzObjectId],
                [AcknowledgedObservationCount], [Status]
            )
            SELECT o.[ObjectTypeId], o.[RegisterUzObjectId], 0, 'Pending'
            FROM [Sync].[ObservedObject] o
            WHERE NOT EXISTS
            (
                SELECT 1 FROM [Sync].[ObservedObjectWork] w WITH (UPDLOCK, HOLDLOCK)
                WHERE w.[ObjectTypeId] = o.[ObjectTypeId]
                  AND w.[RegisterUzObjectId] = o.[RegisterUzObjectId]
            );

            ;WITH candidates AS
            (
                SELECT TOP (@BatchSize)
                    w.[ObjectTypeId], w.[RegisterUzObjectId], o.[ChangeObservationCount]
                FROM [Sync].[ObservedObjectWork] w WITH (UPDLOCK, READPAST, ROWLOCK)
                JOIN [Sync].[ObservedObject] o
                  ON o.[ObjectTypeId] = w.[ObjectTypeId]
                 AND o.[RegisterUzObjectId] = w.[RegisterUzObjectId]
                WHERE o.[ChangeObservationCount] > w.[AcknowledgedObservationCount]
                  AND
                  (
                      w.[Status] IN ('Pending', 'Resolved', 'Failed')
                      OR (w.[Status] = 'Resolving' AND w.[LeaseExpiresAtUtc] < SYSUTCDATETIME())
                  )
                ORDER BY o.[LastObservedAtUtc], w.[ObjectTypeId], w.[RegisterUzObjectId]
            )
            UPDATE w
            SET [Status] = 'Resolving',
                [ClaimObservationCount] = candidates.[ChangeObservationCount],
                [LeaseToken] = NEWID(),
                [LeaseExpiresAtUtc] = DATEADD(second, @LeaseSeconds, SYSUTCDATETIME()),
                [AttemptCount] = [AttemptCount] + 1,
                [LastAttemptAtUtc] = SYSUTCDATETIME(),
                [LastError] = NULL,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            OUTPUT INSERTED.[LeaseToken], INSERTED.[ObjectTypeId],
                   INSERTED.[RegisterUzObjectId], INSERTED.[ClaimObservationCount]
            FROM [Sync].[ObservedObjectWork] w
            JOIN candidates
              ON candidates.[ObjectTypeId] = w.[ObjectTypeId]
             AND candidates.[RegisterUzObjectId] = w.[RegisterUzObjectId];
            """;
        Add(command, "@BatchSize", SqlDbType.Int, batchSize);
        Add(command, "@LeaseSeconds", SqlDbType.Int, leaseSeconds);

        var result = new List<RegisterUzObservationClaim>();
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new RegisterUzObservationClaim(
                reader.GetGuid(0),
                (RegisterUzObjectType)reader.GetByte(1),
                reader.GetInt64(2),
                reader.GetInt64(3)));
        }
        return result;
    }

    public async Task CompleteObservationAsync(
        RegisterUzResolvedObservation observation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(observation);
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

                IF NOT EXISTS
                (
                    SELECT 1 FROM [Sync].[ObservedObjectWork] WITH (UPDLOCK, HOLDLOCK)
                    WHERE [ObjectTypeId] = @ObjectTypeId
                      AND [RegisterUzObjectId] = @ObjectId
                      AND [Status] = 'Resolving'
                      AND [LeaseToken] = @LeaseToken
                      AND [ClaimObservationCount] = @ObservationCount
                )
                    THROW 51530, 'Observation resolution lease no longer matches.', 1;

                UPDATE [Sync].[EntityRefreshQueue]
                SET [RequestedGeneration] = [RequestedGeneration] + 1,
                    [Status] = CASE WHEN [Status] = 'Refreshing' THEN [Status] ELSE 'Pending' END,
                    [LastError] = NULL,
                    [UpdatedAtUtc] = SYSUTCDATETIME()
                WHERE [RegisterUzEntityId] = @EntityId;

                IF @@ROWCOUNT = 0
                    INSERT INTO [Sync].[EntityRefreshQueue]
                    (
                        [RegisterUzEntityId], [RequestedGeneration], [CompletedGeneration], [Status]
                    )
                    VALUES (@EntityId, 1, 0, 'Pending');

                UPDATE w
                SET [AcknowledgedObservationCount] = @ObservationCount,
                    [ClaimObservationCount] = NULL,
                    [ResolvedEntityId] = @EntityId,
                    [Status] = CASE WHEN o.[ChangeObservationCount] > @ObservationCount
                                    THEN 'Pending' ELSE 'Resolved' END,
                    [LeaseToken] = NULL,
                    [LeaseExpiresAtUtc] = NULL,
                    [LastCompletedAtUtc] = SYSUTCDATETIME(),
                    [LastError] = NULL,
                    [UpdatedAtUtc] = SYSUTCDATETIME()
                FROM [Sync].[ObservedObjectWork] w
                JOIN [Sync].[ObservedObject] o
                  ON o.[ObjectTypeId] = w.[ObjectTypeId]
                 AND o.[RegisterUzObjectId] = w.[RegisterUzObjectId]
                WHERE w.[ObjectTypeId] = @ObjectTypeId
                  AND w.[RegisterUzObjectId] = @ObjectId
                  AND w.[LeaseToken] = @LeaseToken;
                """;
            AddObservationParameters(command, observation.Claim);
            Add(command, "@EntityId", SqlDbType.BigInt, observation.RegisterUzEntityId);
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public Task FailObservationAsync(
        RegisterUzObservationClaim observation,
        Exception exception,
        CancellationToken cancellationToken = default) =>
        FailAsync(
            """
            UPDATE [Sync].[ObservedObjectWork]
            SET [Status] = 'Failed', [ClaimObservationCount] = NULL,
                [LeaseToken] = NULL, [LeaseExpiresAtUtc] = NULL,
                [LastError] = @Error, [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [ObjectTypeId] = @ObjectTypeId
              AND [RegisterUzObjectId] = @ObjectId
              AND [Status] = 'Resolving' AND [LeaseToken] = @LeaseToken;
            """,
            observation,
            null,
            exception,
            cancellationToken);

    public async Task<IReadOnlyList<RegisterUzEntityRefreshClaim>> ClaimEntityRefreshesAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        ValidateBatch(batchSize);
        int leaseSeconds = ValidateLease(leaseDuration);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            ;WITH candidates AS
            (
                SELECT TOP (@BatchSize) *
                FROM [Sync].[EntityRefreshQueue] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE [RequestedGeneration] > [CompletedGeneration]
                  AND
                  (
                      [Status] IN ('Pending', 'Completed', 'Failed')
                      OR ([Status] = 'Refreshing' AND [LeaseExpiresAtUtc] < SYSUTCDATETIME())
                  )
                ORDER BY [UpdatedAtUtc], [RegisterUzEntityId]
            )
            UPDATE candidates
            SET [Status] = 'Refreshing',
                [ClaimGeneration] = [RequestedGeneration],
                [LeaseToken] = NEWID(),
                [LeaseExpiresAtUtc] = DATEADD(second, @LeaseSeconds, SYSUTCDATETIME()),
                [AttemptCount] = [AttemptCount] + 1,
                [LastAttemptAtUtc] = SYSUTCDATETIME(),
                [LastError] = NULL,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            OUTPUT INSERTED.[LeaseToken], INSERTED.[RegisterUzEntityId],
                   INSERTED.[ClaimGeneration];
            """;
        Add(command, "@BatchSize", SqlDbType.Int, batchSize);
        Add(command, "@LeaseSeconds", SqlDbType.Int, leaseSeconds);

        var result = new List<RegisterUzEntityRefreshClaim>();
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new RegisterUzEntityRefreshClaim(reader.GetGuid(0), reader.GetInt64(1), reader.GetInt64(2)));
        return result;
    }

    public async Task CompleteEntityRefreshAsync(
        RegisterUzEntityRefreshClaim entity,
        long syncRunId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE [Sync].[EntityRefreshQueue]
            SET [CompletedGeneration] = @Generation,
                [ClaimGeneration] = NULL,
                [Status] = CASE WHEN [RequestedGeneration] > @Generation
                                THEN 'Pending' ELSE 'Completed' END,
                [LeaseToken] = NULL,
                [LeaseExpiresAtUtc] = NULL,
                [LastCompletedAtUtc] = SYSUTCDATETIME(),
                [LastSyncRunId] = @SyncRunId,
                [LastError] = NULL,
                [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzEntityId] = @EntityId
              AND [Status] = 'Refreshing'
              AND [LeaseToken] = @LeaseToken
              AND [ClaimGeneration] = @Generation;

            IF @@ROWCOUNT = 0
                THROW 51531, 'Entity refresh lease no longer matches.', 1;
            """;
        AddEntityParameters(command, entity);
        Add(command, "@SyncRunId", SqlDbType.BigInt, syncRunId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task FailEntityRefreshAsync(
        RegisterUzEntityRefreshClaim entity,
        Exception exception,
        CancellationToken cancellationToken = default) =>
        FailAsync(
            """
            UPDATE [Sync].[EntityRefreshQueue]
            SET [Status] = 'Failed', [ClaimGeneration] = NULL,
                [LeaseToken] = NULL, [LeaseExpiresAtUtc] = NULL,
                [LastError] = @Error, [UpdatedAtUtc] = SYSUTCDATETIME()
            WHERE [RegisterUzEntityId] = @EntityId
              AND [Status] = 'Refreshing' AND [LeaseToken] = @LeaseToken;
            """,
            null,
            entity,
            exception,
            cancellationToken);

    private async Task FailAsync(
        string sql,
        RegisterUzObservationClaim? observation,
        RegisterUzEntityRefreshClaim? entity,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(exception);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        if (observation is not null)
        {
            Add(command, "@ObjectTypeId", SqlDbType.TinyInt, (byte)observation.ObjectType);
            Add(command, "@ObjectId", SqlDbType.BigInt, observation.RegisterUzObjectId);
            Add(command, "@LeaseToken", SqlDbType.UniqueIdentifier, observation.LeaseToken);
        }
        if (entity is not null)
        {
            Add(command, "@EntityId", SqlDbType.BigInt, entity.RegisterUzEntityId);
            Add(command, "@LeaseToken", SqlDbType.UniqueIdentifier, entity.LeaseToken);
        }
        Add(command, "@Error", SqlDbType.NVarChar, Truncate(exception.ToString(), 4000), 4000);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddObservationParameters(SqlCommand command, RegisterUzObservationClaim observation)
    {
        Add(command, "@ObjectTypeId", SqlDbType.TinyInt, (byte)observation.ObjectType);
        Add(command, "@ObjectId", SqlDbType.BigInt, observation.RegisterUzObjectId);
        Add(command, "@ObservationCount", SqlDbType.BigInt, observation.ChangeObservationCount);
        Add(command, "@LeaseToken", SqlDbType.UniqueIdentifier, observation.LeaseToken);
    }

    private static void AddEntityParameters(SqlCommand command, RegisterUzEntityRefreshClaim entity)
    {
        Add(command, "@EntityId", SqlDbType.BigInt, entity.RegisterUzEntityId);
        Add(command, "@Generation", SqlDbType.BigInt, entity.RequestedGeneration);
        Add(command, "@LeaseToken", SqlDbType.UniqueIdentifier, entity.LeaseToken);
    }

    private static int ValidateLease(TimeSpan leaseDuration)
    {
        if (leaseDuration <= TimeSpan.Zero || leaseDuration > TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        return Math.Max(1, checked((int)Math.Ceiling(leaseDuration.TotalSeconds)));
    }

    private static void ValidateBatch(int batchSize)
    {
        if (batchSize is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(batchSize));
    }

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length];

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
