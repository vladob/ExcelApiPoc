using Microsoft.Data.SqlClient;
using System.Data;

namespace ExcelApiPoc.Api.Data;

public sealed class CalculationReportCandidateRepository
{
    private readonly string _connectionString;

    public CalculationReportCandidateRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("AuditAddIn")
            ?? throw new InvalidOperationException(
                "Connection string 'AuditAddIn' is not configured.");
    }

    public async Task<bool> AccountingEntityExistsAsync(
        string ico,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS
            (
                SELECT 1
                FROM [RegisterUZ].[Registry].[AccountingEntity] AS ae
                WHERE ae.[Ico] = @Ico
                  AND ae.[IsDeleted] = 0
            )
            THEN CONVERT(bit, 1)
            ELSE CONVERT(bit, 0)
            END;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Ico", SqlDbType.VarChar, 20).Value = ico;

        return (bool)(await command.ExecuteScalarAsync(cancellationToken)
            ?? false);
    }

    public async Task<IReadOnlyList<CalculationReportCandidate>> GetAsync(
        string ico,
        int fiscalYear,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.[RegisterUzEntityId],
                c.[Ico],
                c.[EntityName],
                c.[LegalFormCode],
                c.[RegisterUzStatementId],
                c.[RegisterUzFinancialReportId],
                c.[RegisterUzTemplateId],
                c.[TemplateId],
                c.[AccountFrameworkId],
                c.[FrameworkCode],
                c.[CalculationImplemented]
            FROM [Accounts].[GetCalculationReportCandidates]
                 (@Ico, @FiscalYear) AS c
            ORDER BY
                c.[RegisterUzFinancialReportId];
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Ico", SqlDbType.VarChar, 20).Value = ico;
        command.Parameters.Add("@FiscalYear", SqlDbType.Int).Value = fiscalYear;

        var candidates = new List<CalculationReportCandidate>();

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            candidates.Add(new CalculationReportCandidate
            {
                RegisterUzEntityId = reader.GetInt64(0),
                Ico = reader.GetString(1),
                EntityName = reader.IsDBNull(2) ? null : reader.GetString(2),
                LegalFormCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                FinancialStatementId = reader.GetInt64(4),
                FinancialReportId = reader.GetInt64(5),
                RegisterUzTemplateId = reader.GetInt32(6),
                TemplateId = reader.GetInt32(7),
                AccountFrameworkId = reader.GetInt32(8),
                FrameworkCode = reader.GetString(9),
                CalculationImplemented = reader.GetBoolean(10)
            });
        }

        return candidates;
    }
}
