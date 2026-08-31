using ExcelApiPoc.Api.Models.AccountingEntities;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api.Data;

public sealed class RegisterUzAccountingEntityRepository
{
    private readonly string _connectionString;

    public RegisterUzAccountingEntityRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("RegisterUz")
            ?? throw new InvalidOperationException(
                "Connection string 'RegisterUz' is not configured.");
    }

    public async Task<RegisterUzAccountingEntityGraph?> GetByIcoAsync(
        string ico,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        AccountingEntityDto? entity =
            await ReadEntityAsync(
                connection,
                ico,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        List<FinancialReportRow> financialReports =
            await ReadFinancialReportsAsync(
                connection,
                entity.Id,
                cancellationToken);

        Dictionary<long, List<FinancialReportDto>>
            reportsByStatementId = financialReports
                .Where(x => x.StatementId.HasValue)
                .GroupBy(x => x.StatementId!.Value)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(y => y.Report).ToList());

        Dictionary<long, List<FinancialReportDto>>
            reportsByAnnualReportId = financialReports
                .Where(x => x.AnnualReportId.HasValue)
                .GroupBy(x => x.AnnualReportId!.Value)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(y => y.Report).ToList());

        IReadOnlyList<FinancialStatementDto> statements =
            await ReadFinancialStatementsAsync(
                connection,
                entity.Id,
                reportsByStatementId,
                cancellationToken);

        IReadOnlyList<AnnualReportDto> annualReports =
            await ReadAnnualReportsAsync(
                connection,
                entity.Id,
                reportsByAnnualReportId,
                cancellationToken);

        return new RegisterUzAccountingEntityGraph
        {
            Entity = entity,
            FinancialStatements = statements,
            AnnualReports = annualReports
        };
    }

    private static async Task<AccountingEntityDto?> ReadEntityAsync(
        SqlConnection connection,
        string ico,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                [RegisterUzEntityId],
                [Ico],
                [Dic],
                [Sid],
                [Name],
                [City],
                [Street],
                [PostalCode],
                [EstablishedDate],
                [CancellationDate],
                [LegalFormCode],
                [SkNaceCode],
                [OrganizationSizeCode],
                [OwnershipTypeCode],
                [RegionCode],
                [DistrictCode],
                [RegisteredOfficeCode],
                [IsConsolidated]
            FROM [Registry].[AccountingEntity]
            WHERE [Ico] = @Ico
              AND [IsDeleted] = 0;
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Ico", ico);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AccountingEntityDto
        {
            Id = reader.GetInt64(0),
            Ico = reader.GetString(1),
            Dic = GetNullableString(reader, 2),
            Sid = GetNullableString(reader, 3),
            Name = GetNullableString(reader, 4),
            City = GetNullableString(reader, 5),
            Street = GetNullableString(reader, 6),
            PostalCode = GetNullableString(reader, 7),
            EstablishedDate = GetNullableDateOnly(reader, 8),
            CancellationDate = GetNullableDateOnly(reader, 9),
            LegalFormCode = GetNullableString(reader, 10),
            SkNaceCode = GetNullableString(reader, 11),
            OrganizationSizeCode = GetNullableString(reader, 12),
            OwnershipTypeCode = GetNullableString(reader, 13),
            RegionCode = GetNullableString(reader, 14),
            DistrictCode = GetNullableString(reader, 15),
            RegisteredOfficeCode = GetNullableString(reader, 16),
            IsConsolidated = GetNullableBoolean(reader, 17)
        };
    }

    private static async Task<List<FinancialReportRow>> ReadFinancialReportsAsync(
        SqlConnection connection,
        long entityId,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                fr.[RegisterUzFinancialReportId],
                fr.[RegisterUzStatementId],
                fr.[RegisterUzAnnualReportId],
                fr.[RegisterUzTemplateId],
                fr.[CurrencyCode],
                fr.[TaxOfficeCode],
                fr.[DataAvailability]
            FROM [Reporting].[FinancialReport] fr
            WHERE fr.[IsDeleted] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[FinancialStatement] fs
                      WHERE fs.[RegisterUzStatementId] =
                            fr.[RegisterUzStatementId]
                        AND fs.[RegisterUzEntityId] = @EntityId
                        AND fs.[IsDeleted] = 0
                  )
                  OR
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[AnnualReport] ar
                      WHERE ar.[RegisterUzAnnualReportId] =
                            fr.[RegisterUzAnnualReportId]
                        AND ar.[RegisterUzEntityId] = @EntityId
                        AND ar.[IsDeleted] = 0
                  )
              )
            ORDER BY
                fr.[RegisterUzFinancialReportId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<FinancialReportRow>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(
                new FinancialReportRow
                {
                    StatementId = GetNullableInt64(reader, 1),
                    AnnualReportId = GetNullableInt64(reader, 2),

                    Report = new FinancialReportDto
                    {
                        Id = reader.GetInt64(0),
                        TemplateId = GetNullableInt64(reader, 3),
                        CurrencyCode = GetNullableString(reader, 4),
                        TaxOfficeCode = GetNullableString(reader, 5),
                        DataAvailability = GetNullableString(reader, 6),
                        TitlePage = null,
                        Attachments = [],
                        Tables = []
                    }
                });
        }

        await reader.CloseAsync();

        Dictionary<long, FinancialReportTitlePageDto> titlePages =
            await ReadFinancialReportTitlePagesAsync(
                connection,
                entityId,
                cancellationToken);

        Dictionary<long, List<FinancialReportAttachmentDto>> attachments =
            await ReadFinancialReportAttachmentsAsync(
                connection,
                entityId,
                cancellationToken);

        Dictionary<long, List<FinancialReportTableDto>> tables =
            await ReadFinancialReportTablesAsync(
                connection,
                entityId,
                cancellationToken);

        return result
            .Select(
                x =>
                {
                    titlePages.TryGetValue(
                        x.Report.Id,
                        out FinancialReportTitlePageDto? titlePage);

                    attachments.TryGetValue(
                        x.Report.Id,
                        out List<FinancialReportAttachmentDto>? reportAttachments);

                    tables.TryGetValue(
                        x.Report.Id,
                        out List<FinancialReportTableDto>? reportTables);

                    return new FinancialReportRow
                    {
                        StatementId = x.StatementId,
                        AnnualReportId = x.AnnualReportId,

                        Report = x.Report with
                        {
                            TitlePage = titlePage,
                            Attachments = reportAttachments ?? [],
                            Tables = reportTables ?? []
                        }
                    };
                })
            .ToList();
    }

    private static async Task<Dictionary<long, FinancialReportTitlePageDto>>
        ReadFinancialReportTitlePagesAsync(
            SqlConnection connection,
            long entityId,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                tp.[RegisterUzFinancialReportId],
                tp.[EntityName],
                tp.[Ico],
                tp.[Dic],
                tp.[Sid],
                tp.[Address],
                tp.[LegalFormCode],
                tp.[SkNaceCode],
                tp.[ReportType],
                tp.[IsConsolidated],
                tp.[IsConsolidatedCentralGovernment],
                tp.[IsSummaryPublicAdministration],
                tp.[EntityType],
                tp.[CommercialRegister],
                tp.[FundName],
                tp.[LeiCode],
                tp.[PeriodFrom],
                tp.[PeriodTo],
                tp.[PreviousPeriodFrom],
                tp.[PreviousPeriodTo],
                tp.[CompletionDate],
                tp.[ApprovalDate],
                tp.[PreparationDate],
                tp.[AssemblyDate],
                tp.[AuditorReportAttachmentDate]
            FROM [Reporting].[FinancialReportTitlePage] tp
            INNER JOIN [Reporting].[FinancialReport] fr
                ON fr.[RegisterUzFinancialReportId] =
                   tp.[RegisterUzFinancialReportId]
            WHERE fr.[IsDeleted] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[FinancialStatement] fs
                      WHERE fs.[RegisterUzStatementId] =
                            fr.[RegisterUzStatementId]
                        AND fs.[RegisterUzEntityId] = @EntityId
                        AND fs.[IsDeleted] = 0
                  )
                  OR
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[AnnualReport] ar
                      WHERE ar.[RegisterUzAnnualReportId] =
                            fr.[RegisterUzAnnualReportId]
                        AND ar.[RegisterUzEntityId] = @EntityId
                        AND ar.[IsDeleted] = 0
                  )
              );
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result =
            new Dictionary<long, FinancialReportTitlePageDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long financialReportId = reader.GetInt64(0);

            result[financialReportId] =
                new FinancialReportTitlePageDto
                {
                    EntityName = GetNullableString(reader, 1),
                    Ico = GetNullableString(reader, 2),
                    Dic = GetNullableString(reader, 3),
                    Sid = GetNullableString(reader, 4),
                    Address = GetNullableString(reader, 5),
                    LegalFormCode = GetNullableString(reader, 6),
                    SkNaceCode = GetNullableString(reader, 7),
                    ReportType = GetNullableString(reader, 8),
                    IsConsolidated = GetNullableBoolean(reader, 9),
                    IsConsolidatedCentralGovernment =
                        GetNullableBoolean(reader, 10),
                    IsSummaryPublicAdministration =
                        GetNullableBoolean(reader, 11),
                    EntityType = GetNullableString(reader, 12),
                    CommercialRegister = GetNullableString(reader, 13),
                    FundName = GetNullableString(reader, 14),
                    LeiCode = GetNullableString(reader, 15),
                    PeriodFrom = GetNullableString(reader, 16),
                    PeriodTo = GetNullableString(reader, 17),
                    PreviousPeriodFrom = GetNullableString(reader, 18),
                    PreviousPeriodTo = GetNullableString(reader, 19),
                    CompletionDate = GetNullableDateOnly(reader, 20),
                    ApprovalDate = GetNullableDateOnly(reader, 21),
                    PreparationDate = GetNullableDateOnly(reader, 22),
                    AssemblyDate = GetNullableDateOnly(reader, 23),
                    AuditorReportAttachmentDate =
                        GetNullableDateOnly(reader, 24)
                };
        }

        return result;
    }

    private static async Task<Dictionary<long, List<FinancialReportAttachmentDto>>>
        ReadFinancialReportAttachmentsAsync(
            SqlConnection connection,
            long entityId,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                fra.[RegisterUzFinancialReportId],
                fra.[RegisterUzAttachmentId],
                fra.[FileName],
                fra.[MimeType],
                fra.[FileSizeBytes],
                fra.[PageCount],
                fra.[LanguageCode]
            FROM [Reporting].[FinancialReportAttachment] fra
            INNER JOIN [Reporting].[FinancialReport] fr
                ON fr.[RegisterUzFinancialReportId] =
                   fra.[RegisterUzFinancialReportId]
            WHERE fr.[IsDeleted] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[FinancialStatement] fs
                      WHERE fs.[RegisterUzStatementId] =
                            fr.[RegisterUzStatementId]
                        AND fs.[RegisterUzEntityId] = @EntityId
                        AND fs.[IsDeleted] = 0
                  )
                  OR
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[AnnualReport] ar
                      WHERE ar.[RegisterUzAnnualReportId] =
                            fr.[RegisterUzAnnualReportId]
                        AND ar.[RegisterUzEntityId] = @EntityId
                        AND ar.[IsDeleted] = 0
                  )
              )
            ORDER BY
                fra.[RegisterUzFinancialReportId],
                fra.[RegisterUzAttachmentId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result =
            new Dictionary<long, List<FinancialReportAttachmentDto>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long financialReportId = reader.GetInt64(0);

            if (!result.TryGetValue(
                    financialReportId,
                    out List<FinancialReportAttachmentDto>? list))
            {
                list = [];
                result.Add(financialReportId, list);
            }

            list.Add(
                new FinancialReportAttachmentDto
                {
                    Id = reader.GetInt64(1),
                    FileName = GetNullableString(reader, 2),
                    MimeType = GetNullableString(reader, 3),
                    FileSizeBytes = GetNullableInt64(reader, 4),
                    PageCount = GetNullableInt32(reader, 5),
                    LanguageCode = GetNullableString(reader, 6)
                });
        }

        return result;
    }

    private static async Task<Dictionary<long, List<FinancialReportTableDto>>>
        ReadFinancialReportTablesAsync(
            SqlConnection connection,
            long entityId,
            CancellationToken cancellationToken)
    {
        Dictionary<long, List<FinancialReportValueDto>> valuesByTableId =
            await ReadFinancialReportValuesAsync(
                connection,
                entityId,
                cancellationToken);

        const string sql =
            """
            SELECT
                frt.[FinancialReportTableId],
                frt.[RegisterUzFinancialReportId],
                frt.[TemplateTableId],
                frt.[TableOrdinal],
                frt.[NameSk],
                frt.[NameEn]
            FROM [Reporting].[FinancialReportTable] frt
            INNER JOIN [Reporting].[FinancialReport] fr
                ON fr.[RegisterUzFinancialReportId] =
                   frt.[RegisterUzFinancialReportId]
            WHERE fr.[IsDeleted] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[FinancialStatement] fs
                      WHERE fs.[RegisterUzStatementId] =
                            fr.[RegisterUzStatementId]
                        AND fs.[RegisterUzEntityId] = @EntityId
                        AND fs.[IsDeleted] = 0
                  )
                  OR
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[AnnualReport] ar
                      WHERE ar.[RegisterUzAnnualReportId] =
                            fr.[RegisterUzAnnualReportId]
                        AND ar.[RegisterUzEntityId] = @EntityId
                        AND ar.[IsDeleted] = 0
                  )
              )
            ORDER BY
                frt.[RegisterUzFinancialReportId],
                frt.[TableOrdinal],
                frt.[FinancialReportTableId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result =
            new Dictionary<long, List<FinancialReportTableDto>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long tableId = reader.GetInt64(0);
            long financialReportId = reader.GetInt64(1);

            if (!result.TryGetValue(
                    financialReportId,
                    out List<FinancialReportTableDto>? list))
            {
                list = [];
                result.Add(financialReportId, list);
            }

            valuesByTableId.TryGetValue(
                tableId,
                out List<FinancialReportValueDto>? values);

            list.Add(
                new FinancialReportTableDto
                {
                    Id = tableId,
                    TemplateTableId = GetNullableInt64(reader, 2),
                    TableOrdinal = reader.GetInt32(3),
                    NameSk = GetNullableString(reader, 4),
                    NameEn = GetNullableString(reader, 5),
                    Values = values ?? []
                });
        }

        return result;
    }

    private static async Task<Dictionary<long, List<FinancialReportValueDto>>>
        ReadFinancialReportValuesAsync(
            SqlConnection connection,
            long entityId,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                frv.[FinancialReportTableId],
                frv.[ValueOrdinal],
                frv.[RowOrdinal],
                frv.[DataColumnOrdinal],
                frv.[NumericValue],
                frv.[SourceValue]
            FROM [Reporting].[FinancialReportValue] frv
            INNER JOIN [Reporting].[FinancialReportTable] frt
                ON frt.[FinancialReportTableId] =
                   frv.[FinancialReportTableId]
            INNER JOIN [Reporting].[FinancialReport] fr
                ON fr.[RegisterUzFinancialReportId] =
                   frt.[RegisterUzFinancialReportId]
            WHERE fr.[IsDeleted] = 0
              AND
              (
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[FinancialStatement] fs
                      WHERE fs.[RegisterUzStatementId] =
                            fr.[RegisterUzStatementId]
                        AND fs.[RegisterUzEntityId] = @EntityId
                        AND fs.[IsDeleted] = 0
                  )
                  OR
                  EXISTS
                  (
                      SELECT 1
                      FROM [Reporting].[AnnualReport] ar
                      WHERE ar.[RegisterUzAnnualReportId] =
                            fr.[RegisterUzAnnualReportId]
                        AND ar.[RegisterUzEntityId] = @EntityId
                        AND ar.[IsDeleted] = 0
                  )
              )
            ORDER BY
                frv.[FinancialReportTableId],
                frv.[ValueOrdinal];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result =
            new Dictionary<long, List<FinancialReportValueDto>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long tableId = reader.GetInt64(0);

            if (!result.TryGetValue(
                    tableId,
                    out List<FinancialReportValueDto>? list))
            {
                list = [];
                result.Add(tableId, list);
            }

            list.Add(
                new FinancialReportValueDto
                {
                    ValueOrdinal = reader.GetInt32(1),
                    RowOrdinal = reader.GetInt32(2),
                    DataColumnOrdinal = reader.GetInt32(3),
                    NumericValue = reader.GetDecimal(4),
                    SourceValue = reader.GetString(5)
                });
        }

        return result;
    }

    private static async Task<IReadOnlyList<FinancialStatementDto>>
        ReadFinancialStatementsAsync(
            SqlConnection connection,
            long entityId,
            IReadOnlyDictionary<long, List<FinancialReportDto>>
                reportsByStatementId,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                [RegisterUzStatementId],
                [PeriodFrom],
                [PeriodTo],
                [SubmissionDate],
                [PreparationDate],
                [ApprovalDate],
                [AssemblyDate],
                [AuditorReportAttachmentDate],
                [FundName],
                [LeiCode],
                [IsConsolidated],
                [IsConsolidatedCentralGovernment],
                [IsSummaryPublicAdministration],
                [StatementType]
            FROM [Reporting].[FinancialStatement]
            WHERE [RegisterUzEntityId] = @EntityId
              AND [IsDeleted] = 0
            ORDER BY
                [PeriodTo],
                [RegisterUzStatementId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<FinancialStatementDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long statementId = reader.GetInt64(0);

            reportsByStatementId.TryGetValue(
                statementId,
                out List<FinancialReportDto>? reports);

            result.Add(
                new FinancialStatementDto
                {
                    Id = statementId,
                    PeriodFrom = GetNullableString(reader, 1),
                    PeriodTo = GetNullableString(reader, 2),
                    SubmissionDate = GetNullableDateOnly(reader, 3),
                    PreparationDate = GetNullableDateOnly(reader, 4),
                    ApprovalDate = GetNullableDateOnly(reader, 5),
                    AssemblyDate = GetNullableDateOnly(reader, 6),
                    AuditorReportAttachmentDate =
                        GetNullableDateOnly(reader, 7),
                    FundName = GetNullableString(reader, 8),
                    LeiCode = GetNullableString(reader, 9),
                    IsConsolidated = GetNullableBoolean(reader, 10),
                    IsConsolidatedCentralGovernment =
                        GetNullableBoolean(reader, 11),
                    IsSummaryPublicAdministration =
                        GetNullableBoolean(reader, 12),
                    StatementType = GetNullableString(reader, 13),
                    FinancialReports = reports ?? []
                });
        }

        return result;
    }

    private static async Task<IReadOnlyList<AnnualReportDto>>
        ReadAnnualReportsAsync(
            SqlConnection connection,
            long entityId,
            IReadOnlyDictionary<long, List<FinancialReportDto>>
                reportsByAnnualReportId,
            CancellationToken cancellationToken)
    {
        Dictionary<long, List<AnnualReportAttachmentDto>> attachments =
            await ReadAnnualReportAttachmentsAsync(
                connection,
                entityId,
                cancellationToken);

        const string sql =
            """
            SELECT
                [RegisterUzAnnualReportId],
                [EntityNameAtSubmission],
                [AnnualReportType],
                [FundName],
                [LeiCode],
                [PeriodFrom],
                [PeriodTo],
                [SubmissionDate],
                [AssemblyDate]
            FROM [Reporting].[AnnualReport]
            WHERE [RegisterUzEntityId] = @EntityId
              AND [IsDeleted] = 0
            ORDER BY
                [PeriodTo],
                [RegisterUzAnnualReportId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<AnnualReportDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long annualReportId = reader.GetInt64(0);

            reportsByAnnualReportId.TryGetValue(
                annualReportId,
                out List<FinancialReportDto>? reports);

            attachments.TryGetValue(
                annualReportId,
                out List<AnnualReportAttachmentDto>? annualReportAttachments);

            result.Add(
                new AnnualReportDto
                {
                    Id = annualReportId,
                    EntityNameAtSubmission = GetNullableString(reader, 1),
                    AnnualReportType = GetNullableString(reader, 2),
                    FundName = GetNullableString(reader, 3),
                    LeiCode = GetNullableString(reader, 4),
                    PeriodFrom = GetNullableString(reader, 5),
                    PeriodTo = GetNullableString(reader, 6),
                    SubmissionDate = GetNullableDateOnly(reader, 7),
                    AssemblyDate = GetNullableDateOnly(reader, 8),
                    Attachments = annualReportAttachments ?? [],
                    FinancialReports = reports ?? []
                });
        }

        return result;
    }

    private static async Task<Dictionary<long, List<AnnualReportAttachmentDto>>>
        ReadAnnualReportAttachmentsAsync(
            SqlConnection connection,
            long entityId,
            CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                ara.[RegisterUzAnnualReportId],
                ara.[RegisterUzAttachmentId],
                ara.[FileName],
                ara.[MimeType],
                ara.[FileSizeBytes],
                ara.[LanguageCode]
            FROM [Reporting].[AnnualReportAttachment] ara
            INNER JOIN [Reporting].[AnnualReport] ar
                ON ar.[RegisterUzAnnualReportId] =
                   ara.[RegisterUzAnnualReportId]
            WHERE ar.[RegisterUzEntityId] = @EntityId
              AND ar.[IsDeleted] = 0
            ORDER BY
                ara.[RegisterUzAnnualReportId],
                ara.[RegisterUzAttachmentId];
            """;

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@EntityId", entityId);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result =
            new Dictionary<long, List<AnnualReportAttachmentDto>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            long annualReportId = reader.GetInt64(0);

            if (!result.TryGetValue(
                    annualReportId,
                    out List<AnnualReportAttachmentDto>? list))
            {
                list = [];
                result.Add(annualReportId, list);
            }

            list.Add(
                new AnnualReportAttachmentDto
                {
                    Id = reader.GetInt64(1),
                    FileName = GetNullableString(reader, 2),
                    MimeType = GetNullableString(reader, 3),
                    FileSizeBytes = GetNullableInt64(reader, 4),
                    LanguageCode = GetNullableString(reader, 5)
                });
        }

        return result;
    }

    private static string? GetNullableString(
        SqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static long? GetNullableInt64(
        SqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetInt64(ordinal);
    }

    private static int? GetNullableInt32(
        SqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetInt32(ordinal);
    }

    private static bool? GetNullableBoolean(
        SqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetBoolean(ordinal);
    }

    private static DateOnly? GetNullableDateOnly(
        SqlDataReader reader,
        int ordinal)
    {
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        DateTime value = reader.GetDateTime(ordinal);

        return DateOnly.FromDateTime(value);
    }

    private sealed record FinancialReportRow
    {
        public long? StatementId { get; init; }

        public long? AnnualReportId { get; init; }

        public required FinancialReportDto Report { get; init; }
    }
}
