using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ExcelApiPoc.Api.Data;

public sealed class AccountFrameworkRepository
{
    private readonly string _connectionString;

    public AccountFrameworkRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AuditAddIn") ?? throw new InvalidOperationException("Connection string 'AuditAddIn' is not configured.");
    }

    public async Task<ApplicableAccountFramework?> GetApplicableAsync(string frameworkCode, int fiscalYear, CancellationToken cancellationToken)
    {
        const string sql = """
            DECLARE @ApplicableDate date =
                DATEFROMPARTS(@FiscalYear, 12, 31);

            DECLARE @AccountFrameworkVersionId int =
            (
                SELECT TOP (1)
                    afv.[Id]
                FROM [Accounts].[AccountFrameworkVersion] afv
                INNER JOIN [Accounts].[AccountFramework] af
                    ON af.[Id] = afv.[AccountFrameworkId]
                WHERE af.[Code] = @FrameworkCode
                  AND afv.[ValidFrom] <= @ApplicableDate
                  AND
                  (
                      afv.[ValidTo] IS NULL
                      OR afv.[ValidTo] >= @ApplicableDate
                  )
                ORDER BY
                    afv.[ValidFrom] DESC,
                    afv.[Id] DESC
            );

            SELECT
                af.[Code] AS [FrameworkCode],
                af.[Name] AS [FrameworkName],
                afv.[Id] AS [FrameworkVersionId],
                afv.[VersionCode],
                afv.[ValidFrom],
                afv.[ValidTo],
                afv.[LegalReference],
                afv.[SourceUrl],
                afv.[SourceSha256]
            FROM [Accounts].[AccountFrameworkVersion] afv
            INNER JOIN [Accounts].[AccountFramework] af
                ON af.[Id] = afv.[AccountFrameworkId]
            WHERE afv.[Id] = @AccountFrameworkVersionId;

            SELECT
                CONVERT(int, d.[AccountLevel]) AS [AccountLevel],
                d.[AccountCode],
                d.[OfficialName]
            FROM [Accounts].[AccountDefinition] d
            WHERE d.[AccountFrameworkVersionId] =
                @AccountFrameworkVersionId
            ORDER BY
                d.[AccountLevel],
                d.[AccountCode];
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@FrameworkCode", SqlDbType.NVarChar, 50).Value = frameworkCode;
        command.Parameters.Add("@FiscalYear", SqlDbType.Int).Value = fiscalYear;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var framework = new ApplicableAccountFramework
        {
            FrameworkCode = reader.GetString(0),
            FrameworkName = reader.GetString(1),
            FrameworkVersionId = reader.GetInt32(2),
            VersionCode = reader.GetString(3),
            ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(4)),
            ValidTo = reader.IsDBNull(5) ? null : DateOnly.FromDateTime(reader.GetDateTime(5)),
            LegalReference = reader.IsDBNull(6) ? null : reader.GetString(6),
            SourceUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
            SourceSha256 = reader.IsDBNull(8) ? null : reader.GetString(8)
        };

        var definitions = new List<AccountDefinitionMetadata>();
        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            definitions.Add(new AccountDefinitionMetadata
            {
                AccountLevel = reader.GetInt32(0),
                AccountCode = reader.GetString(1),
                OfficialName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            });
        }

        return new ApplicableAccountFramework
        {
            FrameworkCode = framework.FrameworkCode,
            FrameworkName = framework.FrameworkName,
            FrameworkVersionId = framework.FrameworkVersionId,
            VersionCode = framework.VersionCode,
            ValidFrom = framework.ValidFrom,
            ValidTo = framework.ValidTo,
            LegalReference = framework.LegalReference,
            SourceUrl = framework.SourceUrl,
            SourceSha256 = framework.SourceSha256,
            Definitions = definitions
        };
    }
}