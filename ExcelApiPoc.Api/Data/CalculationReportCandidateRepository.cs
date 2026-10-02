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
                fs.[ApprovalDate],
                c.[RegisterUzFinancialReportId],
                c.[RegisterUzTemplateId],
                c.[TemplateId],
                c.[AccountFrameworkId],
                c.[FrameworkCode],
                c.[CalculationImplemented],
                fs.[SubmissionDate],
                CONVERT(bit, CASE WHEN
                    (fr.[CurrencyCode] = referenceReport.[CurrencyCode] OR
                     (fr.[CurrencyCode] IS NULL AND referenceReport.[CurrencyCode] IS NULL))
                    AND EXISTS
                    (
                        SELECT 1
                        FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS t
                        JOIN [RegisterUZ].[Reporting].[FinancialReportValue] AS v
                            ON v.[FinancialReportTableId] = t.[FinancialReportTableId]
                        WHERE t.[RegisterUzFinancialReportId] = c.[RegisterUzFinancialReportId]
                          AND v.[NumericValue] IS NOT NULL
                    )
                    AND NOT EXISTS
                    (
                        SELECT t.[TableOrdinal], v.[ValueOrdinal], v.[RowOrdinal],
                            v.[DataColumnOrdinal], v.[NumericValue],
                            CASE WHEN v.[NumericValue] IS NULL THEN v.[SourceValue] END
                        FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS t
                        LEFT JOIN [RegisterUZ].[Reporting].[FinancialReportValue] AS v
                            ON v.[FinancialReportTableId] = t.[FinancialReportTableId]
                        WHERE t.[RegisterUzFinancialReportId] = c.[RegisterUzFinancialReportId]
                        EXCEPT
                        SELECT t.[TableOrdinal], v.[ValueOrdinal], v.[RowOrdinal],
                            v.[DataColumnOrdinal], v.[NumericValue],
                            CASE WHEN v.[NumericValue] IS NULL THEN v.[SourceValue] END
                        FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS t
                        LEFT JOIN [RegisterUZ].[Reporting].[FinancialReportValue] AS v
                            ON v.[FinancialReportTableId] = t.[FinancialReportTableId]
                        WHERE t.[RegisterUzFinancialReportId] = referenceReport.[RegisterUzFinancialReportId]
                    )
                    AND NOT EXISTS
                    (
                        SELECT t.[TableOrdinal], v.[ValueOrdinal], v.[RowOrdinal],
                            v.[DataColumnOrdinal], v.[NumericValue],
                            CASE WHEN v.[NumericValue] IS NULL THEN v.[SourceValue] END
                        FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS t
                        LEFT JOIN [RegisterUZ].[Reporting].[FinancialReportValue] AS v
                            ON v.[FinancialReportTableId] = t.[FinancialReportTableId]
                        WHERE t.[RegisterUzFinancialReportId] = referenceReport.[RegisterUzFinancialReportId]
                        EXCEPT
                        SELECT t.[TableOrdinal], v.[ValueOrdinal], v.[RowOrdinal],
                            v.[DataColumnOrdinal], v.[NumericValue],
                            CASE WHEN v.[NumericValue] IS NULL THEN v.[SourceValue] END
                        FROM [RegisterUZ].[Reporting].[FinancialReportTable] AS t
                        LEFT JOIN [RegisterUZ].[Reporting].[FinancialReportValue] AS v
                            ON v.[FinancialReportTableId] = t.[FinancialReportTableId]
                        WHERE t.[RegisterUzFinancialReportId] = c.[RegisterUzFinancialReportId]
                    )
                    THEN 1 ELSE 0 END) AS [ContentMatchesFirstCandidate]
            FROM [Accounts].[GetCalculationReportCandidates]
                 (@Ico, @FiscalYear) AS c
            INNER JOIN [RegisterUZ].[Reporting].[FinancialStatement] AS fs
                ON fs.[RegisterUzStatementId] = c.[RegisterUzStatementId]
            INNER JOIN [RegisterUZ].[Reporting].[FinancialReport] AS fr
                ON fr.[RegisterUzFinancialReportId] = c.[RegisterUzFinancialReportId]
            CROSS APPLY
            (
                SELECT TOP (1) r.[RegisterUzFinancialReportId], r.[CurrencyCode]
                FROM [Accounts].[GetCalculationReportCandidates](@Ico, @FiscalYear) AS firstCandidate
                JOIN [RegisterUZ].[Reporting].[FinancialReport] AS r
                    ON r.[RegisterUzFinancialReportId] = firstCandidate.[RegisterUzFinancialReportId]
                ORDER BY firstCandidate.[RegisterUzFinancialReportId]
            ) AS referenceReport
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
                ApprovalDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                FinancialReportId = reader.GetInt64(6),
                RegisterUzTemplateId = reader.GetInt32(7),
                TemplateId = reader.GetInt32(8),
                AccountFrameworkId = reader.GetInt32(9),
                FrameworkCode = reader.GetString(10),
                CalculationImplemented = reader.GetBoolean(11),
                SubmissionDate = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                ContentMatchesFirstCandidate = reader.GetBoolean(13)
            });
        }

        return candidates;
    }
}
