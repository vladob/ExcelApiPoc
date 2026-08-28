using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api.Data;

public sealed class AuditTemplatePackageRepository
{
    private readonly string _connectionString;

    public AuditTemplatePackageRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AuditAddIn") ?? throw new InvalidOperationException( "Connection string 'AuditAddIn' is not configured.");
    }

    public async Task<AuditTemplatePackage?> GetPackageAsync(int templateErpId, CancellationToken cancellationToken)
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
                r.[CategorySk],
                r.[MappingCaptionSk]
            FROM [Template].[Rows] r
            INNER JOIN [Template].[Tables] t
                ON t.[Id] = r.[TableId]
            WHERE t.[TemplateErpId] = @TemplateErpId
            ORDER BY
                r.[TableErpId],
                r.[RowNumber];

            SELECT
                ag.[Account],
                ag.[Title],
                ag.[Legend],
                ag.[ForAssets],
                ag.[ForLiabilities]
            FROM [Accounts].[AccountGroups] ag
            WHERE ag.[Type] =
            (
                SELECT
                    CASE
                        WHEN COUNT(DISTINCT agu.[Type]) = 1
                            THEN MIN(agu.[Type])
                        ELSE NULL
                    END
                FROM [Accounts].[AcountGroupsUsage] agu
                WHERE agu.[TemplateErpId] = @TemplateErpId
            )
            ORDER BY
                LEN(ag.[Account]),
                ag.[Account];

            SELECT
                [TableErpId],
                [Account3],
                [SumRow],
                [AccountTitle],
                [Au],
                [InBrutto],
                [InCorrection],
                [Usage]
            FROM [Accounts].[AcountGroupsUsage]
            WHERE [TemplateErpId] = @TemplateErpId
            ORDER BY
                [TableErpId],
                [SumRow],
                [Account3];

            SELECT
                [SumTableErpId],
                [SumRow],
                [SourceTableErpId],
                [SourceRow],
                [Coefficient],
                [CalculationLevel]
            FROM [Template].[SumCalculationPlan]
            WHERE [TemplateErpId] = @TemplateErpId
            ORDER BY
                [CalculationLevel],
                [SumTableErpId],
                [SumRow],
                [SourceTableErpId],
                [SourceRow];

            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@TemplateErpId", templateErpId);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        int actualTemplateErpId = reader.GetInt32(0);
        string? name = reader.IsDBNull(1) ? null : reader.GetString(1);
        string? mfSpecification = reader.IsDBNull(2) ? null : reader.GetString(2);
        DateOnly? validFrom = reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3));
        DateOnly? validTo = reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4));
        string? accountingModel = reader.IsDBNull(5) ? null : reader.GetString(5);
        var tables = new List<AuditReportTableDefinition>();
        var headersByTable = new Dictionary<int, List<AuditReportHeaderDefinition>>();
        var rowsByTable = new Dictionary<int, List<AuditReportRowDefinition>>();
        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);
            var headers = new List<AuditReportHeaderDefinition>();
            var rows = new List<AuditReportRowDefinition>();
            headersByTable.Add(tableErpId, headers);
            rowsByTable.Add(tableErpId, rows);

            tables.Add(new AuditReportTableDefinition
            {
                TableErpId = tableErpId,
                NameSk = reader.IsDBNull(1) ? null : reader.GetString(1),
                NameEn = reader.IsDBNull(2) ? null : reader.GetString(2),
                NumberOfColumns = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                NumberOfDataColumns = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                DontHaveRowNumbers = !reader.IsDBNull(5) && reader.GetByte(5) != 0,
                Headers = headers,
                Rows = rows
            });
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);

            if (!headersByTable.TryGetValue(tableErpId, out List<AuditReportHeaderDefinition>? headers))
            {
                throw new InvalidOperationException($"Header references unknown table {tableErpId}.");
            }

            headers.Add(new AuditReportHeaderDefinition
            {
                TextSk = reader.IsDBNull(1) ? null : reader.GetString(1),
                TextEn = reader.IsDBNull(2) ? null : reader.GetString(2),
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
            if (!rowsByTable.TryGetValue(tableErpId,out List<AuditReportRowDefinition>? rows))
            {
                throw new InvalidOperationException($"Row references unknown table {tableErpId}.");
            }

            rows.Add(new AuditReportRowDefinition
            {
                RowNumber = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                Designation = reader.IsDBNull(2) ? null : reader.GetString(2),
                TextSk = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextEn = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsSumRow = !reader.IsDBNull(5) && reader.GetByte(5) != 0,
                CategorySk = reader.IsDBNull(6) ? null : reader.GetString(6),
                MappingCaptionSk = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        var accountGroups = new List<AuditAccountGroupDefinition>();
        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            accountGroups.Add(new AuditAccountGroupDefinition
            {
                Account = reader.GetString(0),
                Title = reader.IsDBNull(1) ? null : reader.GetString(1),
                Legend = reader.IsDBNull(2) ? null : reader.GetString(2),
                AssetsValueSource = reader.IsDBNull(3) ? null : reader.GetString(3),
                LiabilitiesValueSource = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        var reportMappingRules = new List<AuditReportMappingRuleDefinition>();
        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            string usage = reader.IsDBNull(7) ? string.Empty : reader.GetString(7).Trim();
            string[] usageParts = usage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (usageParts.Length != 2)
            {
                throw new InvalidOperationException($"Invalid mapping-rule Usage value '{usage}'.");
            }

            reportMappingRules.Add(
                new AuditReportMappingRuleDefinition
                {
                    TableErpId = reader.GetInt32(0),
                    Account3 = reader.GetString(1),
                    ReportRowNumber = reader.GetInt32(2),
                    AccountTitle = reader.GetString(3),
                    RequiresAnalyticalMapping = !reader.IsDBNull(4) && reader.GetInt16(4) != 0,
                    IncludeInBrutto = !reader.IsDBNull(5) && reader.GetInt16(5) != 0,
                    IncludeInCorrection = !reader.IsDBNull(6) && reader.GetInt16(6) != 0,
                    Side = usageParts[0],
                    ValueSource = usageParts[1]
                });
        }

        var calculationPlan = new List<AuditCalculationDependencyDefinition>();

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            calculationPlan.Add(
                new AuditCalculationDependencyDefinition
                {
                    TargetTableErpId = reader.GetInt32(0),
                    TargetRowNumber = reader.GetInt32(1),
                    SourceTableErpId = reader.GetInt32(2),
                    SourceRowNumber = reader.GetInt32(3),
                    Coefficient = reader.GetInt32(4),
                    CalculationLevel = reader.GetInt32(5)
                });
        }

        return new AuditTemplatePackage
        {
            ContractVersion = 2,
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
            },
            AccountGroups = accountGroups,
            ReportMappingRules = reportMappingRules,
            CalculationPlan = calculationPlan
        };
    }
}