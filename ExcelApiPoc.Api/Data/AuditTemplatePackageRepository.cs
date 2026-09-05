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
                t.[Name_sk] AS [Name],
                t.[MfSpecification],
                t.[ValidFrom],
                t.[ValidTo],
                (
                    SELECT
                        CASE WHEN COUNT(DISTINCT ccv.[AccountingModelCode]) = 1
                            THEN MIN(ccv.[AccountingModelCode]) END
                    FROM [Accounts].[TemplateFrameworkVersion] tfv
                    INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv
                        ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
                    WHERE tfv.[TemplateId] = t.[Id]
                      AND ccv.[ValidTo] IS NULL
                ) AS [AccountingModel]
            FROM [Template].[Templates] t
            WHERE t.[ErpId] = @TemplateErpId;

            SELECT
                tt.[TableErpId],
                tt.[TableOrdinal],
                tt.[Name_sk] AS [NameSk],
                tt.[Name_en] AS [NameEn],
                tt.[NumberOfColumns],
                tt.[NumberOfDataColumns],
                tt.[DontHaveRowNumbers]
            FROM [Template].[Tables] tt
            INNER JOIN [Template].[Templates] t
                ON t.[Id] = tt.[TemplateId]
            WHERE t.[ErpId] = @TemplateErpId
            ORDER BY tt.[TableOrdinal];

            SELECT
                t.[TableErpId],
                h.[Text_sk] AS [TextSk],
                h.[Text_en] AS [TextEn],
                h.[RowPosition],
                h.[ColumnPosition],
                h.[RowSpan],
                h.[ColumnSpan]
            FROM [Template].[Headers] h
            INNER JOIN [Template].[Tables] t
                ON t.[Id] = h.[TableId]
            INNER JOIN [Template].[Templates] template
                ON template.[Id] = t.[TemplateId]
            WHERE template.[ErpId] = @TemplateErpId
            ORDER BY
                t.[TableOrdinal],
                h.[HeaderOrdinal];

            SELECT
                t.[TableErpId],
                r.[RowOrdinal],
                r.[RowNumber],
                r.[Designation],
                r.[Text_sk] AS [TextSk],
                r.[Text_en] AS [TextEn],
                r.[IsSumRow],
                r.[Category_sk] AS [CategorySk],
                r.[MappingCaption_sk] AS [MappingCaptionSk]
            FROM [Template].[Rows] r
            INNER JOIN [Template].[Tables] t
                ON t.[Id] = r.[TableId]
            INNER JOIN [Template].[Templates] template
                ON template.[Id] = t.[TemplateId]
            WHERE template.[ErpId] = @TemplateErpId
            ORDER BY
                t.[TableOrdinal],
                r.[RowOrdinal];

            SELECT
                r.[AccountCode] AS [Account],
                r.[AccountName_sk] AS [Title],
                r.[Legend],
                r.[AssetsValueSourceCode] AS [ForAssets],
                r.[LiabilitiesValueSourceCode] AS [ForLiabilities]
            FROM [Accounts].[AccountCalculationRuleDetails] r
            WHERE r.[CalculationConfigurationVersionId] IN
            (
                SELECT tfv.[CalculationConfigurationVersionId]
                FROM [Accounts].[TemplateFrameworkVersion] tfv
                INNER JOIN [Template].[Templates] t
                    ON t.[Id] = tfv.[TemplateId]
                INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv
                    ON ccv.[Id] = tfv.[CalculationConfigurationVersionId]
                WHERE t.[ErpId] = @TemplateErpId
                  AND ccv.[ValidTo] IS NULL
            )
            ORDER BY
                LEN(r.[AccountCode]),
                r.[AccountCode];

            SELECT
                [TableErpId],
                [AccountCode] AS [Account3],
                [ReportRowNumber] AS [SumRow],
                [AccountName_sk] AS [AccountTitle],
                [RequiresAnalyticalMapping] AS [Au],
                [IncludeInBrutto] AS [InBrutto],
                [IncludeInCorrection] AS [InCorrection],
                CONCAT([Side], ' ', [ValueSourceCode]) AS [Usage]
            FROM [Accounts].[ReportAccountMappingDetails]
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
            FROM [Template].[GetCalculationPlan](@TemplateErpId)
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
                TableOrdinal = reader.GetInt32(1),
                NameSk = reader.IsDBNull(2) ? null : reader.GetString(2),
                NameEn = reader.IsDBNull(3) ? null : reader.GetString(3),
                NumberOfColumns = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                NumberOfDataColumns = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                DontHaveRowNumbers = !reader.IsDBNull(6) && reader.GetBoolean(6),
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
                RowOrdinal = reader.GetInt32(1),
                RowNumber = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Designation = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextSk = reader.IsDBNull(4) ? null : reader.GetString(4),
                TextEn = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsSumRow = !reader.IsDBNull(6) && reader.GetBoolean(6),
                CategorySk = reader.IsDBNull(7) ? null : reader.GetString(7),
                MappingCaptionSk = reader.IsDBNull(8) ? null : reader.GetString(8)
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
                    RequiresAnalyticalMapping = !reader.IsDBNull(4) && reader.GetBoolean(4),
                    IncludeInBrutto = !reader.IsDBNull(5) && reader.GetBoolean(5),
                    IncludeInCorrection = !reader.IsDBNull(6) && reader.GetBoolean(6),
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
            ContractVersion = 3,
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

    public async Task<AuditTemplatePackageV2> GetPackageV2Async(
        int templateErpId,
        string frameworkCode,
        int fiscalYear,
        CancellationToken cancellationToken)
    {
        DateOnly applicableDate = new(fiscalYear, 12, 31);

        const string resolutionSql = """
            SELECT [t].[ValidFrom], [t].[ValidTo]
            FROM [Template].[Templates] AS [t]
            WHERE [t].[ErpId] = @TemplateErpId;

            SELECT [af].[Id]
            FROM [Accounts].[AccountFramework] AS [af]
            WHERE [af].[Code] = @FrameworkCode;

            SELECT [tfv].[Id] AS [TemplateFrameworkVersionId], [afv].[Id] AS [AccountFrameworkVersionId],
                   [afv].[VersionCode] AS [FrameworkVersionCode], [ccv].[Id] AS [CalculationConfigurationVersionId],
                   [ccv].[AccountFrameworkVersionId] AS [ConfigurationFrameworkVersionId],
                   [ccv].[Code] AS [CalculationConfigurationCode], [ccv].[AccountingModelCode],
                   [ccv].[ValidFrom] AS [ConfigurationValidFrom], [ccv].[ValidTo] AS [ConfigurationValidTo]
            FROM [Template].[Templates] AS [t]
            INNER JOIN [Accounts].[TemplateFrameworkVersion] AS [tfv] ON [tfv].[TemplateId] = [t].[Id]
            INNER JOIN [Accounts].[AccountFrameworkVersion] AS [afv] ON [afv].[Id] = [tfv].[AccountFrameworkVersionId]
            INNER JOIN [Accounts].[AccountFramework] AS [af] ON [af].[Id] = [afv].[AccountFrameworkId]
            INNER JOIN [Accounts].[CalculationConfigurationVersion] AS [ccv] ON [ccv].[Id] = [tfv].[CalculationConfigurationVersionId]
            WHERE [t].[ErpId] = @TemplateErpId
              AND [af].[Code] = @FrameworkCode
              AND [afv].[ValidFrom] <= @ApplicableDate
              AND ([afv].[ValidTo] IS NULL OR [afv].[ValidTo] >= @ApplicableDate)
            ORDER BY [tfv].[Id];
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var resolutionCommand = new SqlCommand(resolutionSql, connection);
        resolutionCommand.Parameters.Add("@TemplateErpId", System.Data.SqlDbType.Int).Value = templateErpId;
        resolutionCommand.Parameters.Add("@FrameworkCode", System.Data.SqlDbType.NVarChar, 50).Value = frameworkCode;
        resolutionCommand.Parameters.Add("@ApplicableDate", System.Data.SqlDbType.Date).Value = applicableDate.ToDateTime(TimeOnly.MinValue);

        (int TemplateFrameworkVersionId, int FrameworkVersionId, string FrameworkVersionCode,
            int ConfigurationId, int ConfigurationFrameworkVersionId, string ConfigurationCode,
            string AccountingModelCode, DateOnly ConfigurationValidFrom, DateOnly? ConfigurationValidTo) resolution;

        await using (SqlDataReader reader = await resolutionCommand.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.TemplateNotFound,
                    $"Template {templateErpId} was not found.");
            }

            DateOnly? validFrom = reader.IsDBNull(0) ? null : DateOnly.FromDateTime(reader.GetDateTime(0));
            DateOnly? validTo = reader.IsDBNull(1) ? null : DateOnly.FromDateTime(reader.GetDateTime(1));

            if ((validFrom.HasValue && validFrom.Value > applicableDate) ||
                (validTo.HasValue && validTo.Value < applicableDate))
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.TemplateNotApplicable,
                    $"Template {templateErpId} is not valid on {applicableDate:yyyy-MM-dd}.");
            }

            await reader.NextResultAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.FrameworkNotFound,
                    $"Framework '{frameworkCode}' was not found.");
            }

            await reader.NextResultAsync(cancellationToken);
            var resolutions = new List<(int TemplateFrameworkVersionId, int FrameworkVersionId, string FrameworkVersionCode,
                int ConfigurationId, int ConfigurationFrameworkVersionId, string ConfigurationCode,
                string AccountingModelCode, DateOnly ConfigurationValidFrom, DateOnly? ConfigurationValidTo)>();

            while (await reader.ReadAsync(cancellationToken))
            {
                resolutions.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3),
                    reader.GetInt32(4), reader.GetString(5), reader.GetString(6), DateOnly.FromDateTime(reader.GetDateTime(7)),
                    reader.IsDBNull(8) ? null : DateOnly.FromDateTime(reader.GetDateTime(8))));
            }

            if (resolutions.Any(candidate => candidate.FrameworkVersionId != candidate.ConfigurationFrameworkVersionId))
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.InconsistentFrameworkVersionReferences,
                    "A template association and its calculation configuration reference different framework versions.");
            }

            var applicableResolutions = resolutions
                .Where(candidate => candidate.ConfigurationValidFrom <= applicableDate &&
                    (!candidate.ConfigurationValidTo.HasValue || candidate.ConfigurationValidTo.Value >= applicableDate))
                .ToList();

            if (applicableResolutions.Count == 0)
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.AssociationNotFound,
                    $"No applicable association was found for template {templateErpId}, framework '{frameworkCode}', and date {applicableDate:yyyy-MM-dd}.");
            }

            if (applicableResolutions.Count > 1)
            {
                throw new AuditTemplatePackageV2ResolutionException(
                    AuditTemplatePackageV2ResolutionFailure.MultipleAssociations,
                    $"Multiple applicable associations were found for template {templateErpId}, framework '{frameworkCode}', and date {applicableDate:yyyy-MM-dd}.");
            }

            resolution = applicableResolutions[0];
        }

        return await LoadPackageV2Async(
            connection, templateErpId, frameworkCode, applicableDate, resolution, cancellationToken);
    }

    private static async Task<AuditTemplatePackageV2> LoadPackageV2Async(
        SqlConnection connection,
        int templateErpId,
        string frameworkCode,
        DateOnly applicableDate,
        (int TemplateFrameworkVersionId, int FrameworkVersionId, string FrameworkVersionCode,
            int ConfigurationId, int ConfigurationFrameworkVersionId, string ConfigurationCode,
            string AccountingModelCode, DateOnly ConfigurationValidFrom, DateOnly? ConfigurationValidTo) resolution,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT [r].[Id]
            FROM [Accounts].[AccountCalculationRules] AS [r]
            INNER JOIN [Accounts].[Accounts] AS [a] ON [a].[Id] = [r].[AccountId]
            WHERE [r].[CalculationConfigurationVersionId] = @CalculationConfigurationVersionId
              AND [a].[AccountFrameworkVersionId] <> @AccountFrameworkVersionId;

            SELECT [m].[AccountCalculationRuleId]
            FROM [Accounts].[ReportAccountMappings] AS [m]
            INNER JOIN [Accounts].[AccountCalculationRules] AS [r] ON [r].[Id] = [m].[AccountCalculationRuleId]
            WHERE [m].[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
              AND [r].[CalculationConfigurationVersionId] <> @CalculationConfigurationVersionId;

            SELECT [m].[AccountCalculationRuleId]
            FROM [Accounts].[ReportAccountMappings] AS [m]
            INNER JOIN [Accounts].[AccountCalculationRules] AS [r] ON [r].[Id] = [m].[AccountCalculationRuleId]
            INNER JOIN [Accounts].[Accounts] AS [a] ON [a].[Id] = [r].[AccountId]
            WHERE [m].[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
              AND [a].[AccountFrameworkVersionId] <> @AccountFrameworkVersionId;

            SELECT [m].[TemplateRowId]
            FROM [Accounts].[ReportAccountMappings] AS [m]
            INNER JOIN [Template].[Rows] AS [tr] ON [tr].[Id] = [m].[TemplateRowId]
            WHERE [m].[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
              AND [tr].[RowNumber] IS NULL;

            SELECT [t].[ErpId], [t].[Name_sk] AS [Name], [t].[MfSpecification], [t].[ValidFrom], [t].[ValidTo]
            FROM [Template].[Templates] AS [t]
            WHERE [t].[ErpId] = @TemplateErpId;

            SELECT [tt].[TableErpId], [tt].[TableOrdinal], [tt].[Name_sk] AS [NameSk], [tt].[Name_en] AS [NameEn],
                   [tt].[NumberOfColumns], [tt].[NumberOfDataColumns], [tt].[DontHaveRowNumbers]
            FROM [Template].[Tables] AS [tt]
            INNER JOIN [Template].[Templates] AS [t] ON [t].[Id] = [tt].[TemplateId]
            WHERE [t].[ErpId] = @TemplateErpId
            ORDER BY [tt].[TableOrdinal];

            SELECT [t].[TableErpId], [h].[Text_sk] AS [TextSk], [h].[Text_en] AS [TextEn], [h].[RowPosition],
                   [h].[ColumnPosition], [h].[RowSpan], [h].[ColumnSpan]
            FROM [Template].[Headers] AS [h]
            INNER JOIN [Template].[Tables] AS [t] ON [t].[Id] = [h].[TableId]
            INNER JOIN [Template].[Templates] AS [template] ON [template].[Id] = [t].[TemplateId]
            WHERE [template].[ErpId] = @TemplateErpId
            ORDER BY [t].[TableOrdinal], [h].[HeaderOrdinal];

            SELECT [t].[TableErpId], [r].[RowOrdinal], [r].[RowNumber], [r].[Designation], [r].[Text_sk] AS [TextSk],
                   [r].[Text_en] AS [TextEn], [r].[IsSumRow], [r].[Category_sk] AS [CategorySk], [r].[MappingCaption_sk] AS [MappingCaptionSk]
            FROM [Template].[Rows] AS [r]
            INNER JOIN [Template].[Tables] AS [t] ON [t].[Id] = [r].[TableId]
            INNER JOIN [Template].[Templates] AS [template] ON [template].[Id] = [t].[TemplateId]
            WHERE [template].[ErpId] = @TemplateErpId
            ORDER BY [t].[TableOrdinal], [r].[RowOrdinal];

            SELECT [r].[AccountCode] AS [Account], [r].[AccountName_sk] AS [Title], [r].[Legend],
                   [r].[AssetsValueSourceCode] AS [ForAssets], [r].[LiabilitiesValueSourceCode] AS [ForLiabilities]
            FROM [Accounts].[AccountCalculationRuleDetails] AS [r]
            WHERE [r].[CalculationConfigurationVersionId] = @CalculationConfigurationVersionId
            ORDER BY LEN([r].[AccountCode]), [r].[AccountCode];

            SELECT [tt].[TableErpId], [a].[AccountCode] AS [Account3], [tr].[RowNumber] AS [SumRow],
                   [a].[AccountName_sk] AS [AccountTitle], [m].[RequiresAnalyticalMapping] AS [Au],
                   [m].[IncludeInBrutto] AS [InBrutto], [m].[IncludeInCorrection] AS [InCorrection],
                   CONCAT([m].[Side], ' ', [m].[ValueSourceCode]) AS [Usage]
            FROM [Accounts].[ReportAccountMappings] AS [m]
            INNER JOIN [Template].[Rows] AS [tr] ON [tr].[Id] = [m].[TemplateRowId]
            INNER JOIN [Template].[Tables] AS [tt] ON [tt].[Id] = [tr].[TableId]
            INNER JOIN [Accounts].[AccountCalculationRules] AS [r] ON [r].[Id] = [m].[AccountCalculationRuleId]
            INNER JOIN [Accounts].[Accounts] AS [a] ON [a].[Id] = [r].[AccountId]
            WHERE [m].[TemplateFrameworkVersionId] = @TemplateFrameworkVersionId
            ORDER BY [tt].[TableErpId], [tr].[RowNumber], [a].[AccountCode];

            SELECT [p].[SumTableErpId], [p].[SumRow], [p].[SourceTableErpId], [p].[SourceRow], [p].[Coefficient], [p].[CalculationLevel]
            FROM [Template].[GetCalculationPlan](@TemplateErpId) AS [p]
            ORDER BY [p].[CalculationLevel], [p].[SumTableErpId], [p].[SumRow], [p].[SourceTableErpId], [p].[SourceRow];
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@TemplateErpId", System.Data.SqlDbType.Int).Value = templateErpId;
        command.Parameters.Add("@CalculationConfigurationVersionId", System.Data.SqlDbType.Int).Value = resolution.ConfigurationId;
        command.Parameters.Add("@TemplateFrameworkVersionId", System.Data.SqlDbType.Int).Value = resolution.TemplateFrameworkVersionId;
        command.Parameters.Add("@AccountFrameworkVersionId", System.Data.SqlDbType.Int).Value = resolution.FrameworkVersionId;
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            throw new AuditTemplatePackageV2ResolutionException(
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration,
                "An account calculation rule references an account from a different framework version.");
        }

        await reader.NextResultAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            throw new AuditTemplatePackageV2ResolutionException(
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration,
                "A report mapping references an account calculation rule from a different calculation configuration.");
        }

        await reader.NextResultAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            throw new AuditTemplatePackageV2ResolutionException(
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration,
                "A report mapping reaches an account from a different framework version.");
        }

        await reader.NextResultAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            throw new AuditTemplatePackageV2ResolutionException(
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration,
                "A report mapping references a template row without a row number.");
        }

        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        int actualTemplateErpId = reader.GetInt32(0);
        string? name = reader.IsDBNull(1) ? null : reader.GetString(1);
        string? mfSpecification = reader.IsDBNull(2) ? null : reader.GetString(2);
        DateOnly? validFrom = reader.IsDBNull(3) ? null : DateOnly.FromDateTime(reader.GetDateTime(3));
        DateOnly? validTo = reader.IsDBNull(4) ? null : DateOnly.FromDateTime(reader.GetDateTime(4));
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
                TableOrdinal = reader.GetInt32(1),
                NameSk = reader.IsDBNull(2) ? null : reader.GetString(2),
                NameEn = reader.IsDBNull(3) ? null : reader.GetString(3),
                NumberOfColumns = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                NumberOfDataColumns = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                DontHaveRowNumbers = !reader.IsDBNull(6) && reader.GetBoolean(6),
                Headers = headers,
                Rows = rows
            });
        }

        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            int tableErpId = reader.GetInt32(0);
            if (!headersByTable.TryGetValue(tableErpId, out List<AuditReportHeaderDefinition>? headers))
                throw new InvalidOperationException($"Header references unknown table {tableErpId}.");
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
            if (!rowsByTable.TryGetValue(tableErpId, out List<AuditReportRowDefinition>? rows))
                throw new InvalidOperationException($"Row references unknown table {tableErpId}.");
            rows.Add(new AuditReportRowDefinition
            {
                RowOrdinal = reader.GetInt32(1),
                RowNumber = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Designation = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextSk = reader.IsDBNull(4) ? null : reader.GetString(4),
                TextEn = reader.IsDBNull(5) ? null : reader.GetString(5),
                IsSumRow = !reader.IsDBNull(6) && reader.GetBoolean(6),
                CategorySk = reader.IsDBNull(7) ? null : reader.GetString(7),
                MappingCaptionSk = reader.IsDBNull(8) ? null : reader.GetString(8)
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
                throw new InvalidOperationException($"Invalid mapping-rule Usage value '{usage}'.");
            reportMappingRules.Add(new AuditReportMappingRuleDefinition
            {
                TableErpId = reader.GetInt32(0),
                Account3 = reader.GetString(1),
                ReportRowNumber = reader.GetInt32(2),
                AccountTitle = reader.GetString(3),
                RequiresAnalyticalMapping = !reader.IsDBNull(4) && reader.GetBoolean(4),
                IncludeInBrutto = !reader.IsDBNull(5) && reader.GetBoolean(5),
                IncludeInCorrection = !reader.IsDBNull(6) && reader.GetBoolean(6),
                Side = usageParts[0],
                ValueSource = usageParts[1]
            });
        }

        var calculationPlan = new List<AuditCalculationDependencyDefinition>();
        await reader.NextResultAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            calculationPlan.Add(new AuditCalculationDependencyDefinition
            {
                TargetTableErpId = reader.GetInt32(0),
                TargetRowNumber = reader.GetInt32(1),
                SourceTableErpId = reader.GetInt32(2),
                SourceRowNumber = reader.GetInt32(3),
                Coefficient = reader.GetInt32(4),
                CalculationLevel = reader.GetInt32(5)
            });
        }

        return new AuditTemplatePackageV2
        {
            GeneratedAtUtc = DateTime.UtcNow,
            FrameworkCode = frameworkCode,
            FrameworkVersionCode = resolution.FrameworkVersionCode,
            CalculationConfigurationCode = resolution.ConfigurationCode,
            ApplicableDate = applicableDate,
            Template = new AuditTemplateDefinition
            {
                TemplateErpId = actualTemplateErpId,
                Name = name,
                MfSpecification = mfSpecification,
                ValidFrom = validFrom,
                ValidTo = validTo,
                AccountingModel = resolution.AccountingModelCode,
                Tables = tables
            },
            AccountGroups = accountGroups,
            ReportMappingRules = reportMappingRules,
            CalculationPlan = calculationPlan
        };
    }
}
