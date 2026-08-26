using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api.Data;

public sealed class AuditTemplateRepository
{
    private readonly string _connectionString;

    public AuditTemplateRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AuditAddIn")
            ?? throw new InvalidOperationException(
                "Connection string 'AuditAddIn' is not configured.");
    }

    public async Task<AuditTemplateMetadata?> GetMetadataAsync(
        int templateErpId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                [ErpId],
                [Name],
                [MfSpecification],
                [ValidFrom],
                [ValidTo]
            FROM [Template].[Templates]
            WHERE [ErpId] = @TemplateErpId;

            SELECT
                CONVERT(int, [TableErpId]) AS [TableErpId],
                [NameSk],
                [NameEn],
                [NumberOfColumns],
                [NumberOfDataColumns],
                [DontHaveRowNumbers]
            FROM [Template].[Tables]
            WHERE [TemplateErpId] = @TemplateErpId
            ORDER BY CONVERT(int, [TableErpId]);
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

        int erpId = reader.GetInt32(0);

        string name = reader.IsDBNull(1)
            ? string.Empty
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

        var tables = new List<AuditTableMetadata>();

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(new AuditTableMetadata
            {
                TableErpId = reader.GetInt32(0),

                NameSk = reader.IsDBNull(1)
                    ? string.Empty
                    : reader.GetString(1),

                NameEn = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),

                NumberOfColumns = reader.GetInt32(3),

                NumberOfDataColumns = reader.GetInt32(4),

                DontHaveRowNumbers =
                    !reader.IsDBNull(5) &&
                    reader.GetByte(5) != 0
            });
        }

        return new AuditTemplateMetadata
        {
            TemplateErpId = erpId,
            Name = name,
            MfSpecification = mfSpecification,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Tables = tables
        };
    }
}