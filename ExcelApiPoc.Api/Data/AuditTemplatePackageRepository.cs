using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api.Data;

public sealed class AuditTemplatePackageRepository
{
    private readonly string _connectionString;

    public AuditTemplatePackageRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AuditAddIn")
            ?? throw new InvalidOperationException(
                "Connection string 'AuditAddIn' is not configured.");
    }

    public async Task<AuditTemplatePackage?> GetPackageAsync(
        int templateErpId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                t.[ErpId],
                t.[Name],
                t.[MfSpecification],
                t.[ValidFrom],
                t.[ValidTo],
                (
                    SELECT
                        CASE
                            WHEN COUNT(DISTINCT agu.[Type]) = 1
                                THEN MIN(agu.[Type])
                            ELSE NULL
                        END
                    FROM [Accounts].[AcountGroupsUsage] agu
                    WHERE agu.[TemplateErpId] = t.[ErpId]
                ) AS [AccountingModel]
            FROM [Template].[Templates] t
            WHERE t.[ErpId] = @TemplateErpId;

            SELECT
                CONVERT(int, [TableErpId]),
                [NameSk],
                [NameEn],
                [NumberOfColumns],
                [NumberOfDataColumns],
                [DontHaveRowNumbers]
            FROM [Template].[Tables]
            WHERE [TemplateErpId] = @TemplateErpId
            ORDER BY CONVERT(int, [TableErpId]);

            SELECT
                h.[TableErpId],
                h.[TextSk],
                h.[TextEn],
                h.[RowPosition],
                h.[ColumnPosition],
                h.[RowSpan],
                h.[ColumnSpan]
            FROM [Template].[Headers] h
            INNER JOIN [Template].[Tables] t
                ON t.[Id] = h.[TableId]
            WHERE t.[TemplateErpId] = @TemplateErpId
            ORDER BY
                h.[TableErpId],
                h.[RowPosition],
                h.[ColumnPosition];

            SELECT
                r.[TableErpId],
                r.[RowNumber],
                r.[Designation],
                r.[TextSk],
                r.[TextEn],
                r.[IsSumRow],
                r.[CategorySk]
            FROM [Template].[Rows] r
            INNER JOIN [Template].[Tables] t
                ON t.[Id] = r.[TableId]
            WHERE t.[TemplateErpId] = @TemplateErpId
            ORDER BY
                r.[TableErpId],
                r.[RowNumber];
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@TemplateErpId",
            templateErpId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        int actualTemplateErpId = reader.GetInt32(0);

        string? name = reader.IsDBNull(1)
            ? null
            : reader.GetString(1);

        string? mfSpecification = reader.IsDBNull(2)
            ? null
            : reader.GetString(2);

        DateOnly? validFrom = reader.IsDBNull(3)
            ? null
            : DateOnly.FromDateTime(reader.GetDateTime(3));

        DateOnly? validTo = reader.IsDBNull(4)
            ? null
            : DateOnly.FromDateTime(reader.GetDateTime(4));

        string? accountingModel = reader.IsDBNull(5)
            ? null
            : reader.GetString(5);

        var tables =
            new List<AuditReportTableDefinition>();

        var headersByTable =
            new Dictionary<
                int,
                List<AuditReportHeaderDefinition>>();

        var rowsByTable =
            new Dictionary<
                int,
                List<AuditReportRowDefinition>>();

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);

            var headers =
                new List<AuditReportHeaderDefinition>();

            var rows =
                new List<AuditReportRowDefinition>();

            headersByTable.Add(tableErpId, headers);
            rowsByTable.Add(tableErpId, rows);

            tables.Add(new AuditReportTableDefinition
            {
                TableErpId = tableErpId,

                NameSk = reader.IsDBNull(1)
                    ? null
                    : reader.GetString(1),

                NameEn = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),

                NumberOfColumns = reader.IsDBNull(3)
                    ? null
                    : reader.GetInt32(3),

                NumberOfDataColumns = reader.IsDBNull(4)
                    ? null
                    : reader.GetInt32(4),

                DontHaveRowNumbers =
                    !reader.IsDBNull(5) &&
                    reader.GetByte(5) != 0,

                Headers = headers,
                Rows = rows
            });
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);

            if (!headersByTable.TryGetValue(
                    tableErpId,
                    out List<AuditReportHeaderDefinition>? headers))
            {
                throw new InvalidOperationException(
                    $"Header references unknown table {tableErpId}.");
            }

            headers.Add(new AuditReportHeaderDefinition
            {
                TextSk = reader.IsDBNull(1)
                    ? null
                    : reader.GetString(1),

                TextEn = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),

                RowPosition = reader.GetInt32(3),
                ColumnPosition = reader.GetInt32(4),
                RowSpan = reader.GetInt32(5),
                ColumnSpan = reader.GetInt32(6)
            });
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);

            if (!rowsByTable.TryGetValue(
                    tableErpId,
                    out List<AuditReportRowDefinition>? rows))
            {
                throw new InvalidOperationException(
                    $"Row references unknown table {tableErpId}.");
            }

            rows.Add(new AuditReportRowDefinition
            {
                RowNumber = reader.IsDBNull(1)
                    ? null
                    : reader.GetInt32(1),

                Designation = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),

                TextSk = reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3),

                TextEn = reader.IsDBNull(4)
                    ? null
                    : reader.GetString(4),

                IsSumRow =
                    !reader.IsDBNull(5) &&
                    reader.GetByte(5) != 0,

                CategorySk = reader.IsDBNull(6)
                    ? null
                    : reader.GetString(6)
            });
        }

        return new AuditTemplatePackage
        {
            ContractVersion = 1,
            GeneratedAtUtc = DateTime.UtcNow,

            Template = new AuditTemplateDefinition
            {
                TemplateErpId = actualTemplateErpId,
                Name = name,
                MfSpecification = mfSpecification,
                ValidFrom = validFrom,
                ValidTo = validTo,
                AccountingModel = accountingModel,
                Tables = tables
            }
        };
    }
}