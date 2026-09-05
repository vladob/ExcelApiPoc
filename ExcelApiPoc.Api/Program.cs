using ExcelApiPoc.Api.Data;
using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;
using Microsoft.Data.SqlClient;
using RegisterUz.Sync;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditTemplateRepository>();
builder.Services.AddScoped<AuditTemplatePackageRepository>();
builder.Services.AddScoped<AccountFrameworkRepository>();
builder.Services.AddSingleton<RegisterUzAccountingEntityRepository>();
builder.Services.AddScoped<AccountingEntityPackageService>();
builder.Services.AddScoped<RegisterUzOnDemandLoadService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        message = "Excel API proof of concept is running.",
        serverTimeUtc = DateTime.UtcNow
    });
})
.WithName("GetHealth")
.WithOpenApi();

app.MapGet(
    "/api/health/registeruz-database",
    async (
        IConfiguration configuration,
        CancellationToken cancellationToken) =>
    {
        string? connectionString =
            configuration.GetConnectionString("RegisterUz");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Results.Problem(
                title: "RegisterUZ database connection is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(
                """
                SELECT
                    CAST(SERVERPROPERTY('ServerName') AS nvarchar(128)),
                    DB_NAME();
                """,
                connection);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return Results.Ok(
            new
            {
                status = "Healthy",
                server = reader.GetString(0),
                database = reader.GetString(1)
            });
    })
    .WithName("GetRegisterUzDatabaseHealth")
    .WithOpenApi();

app.MapGet(
    "/api/templates/{templateErpId:int}/metadata",
    async (
        int templateErpId,
        AuditTemplateRepository repository,
        CancellationToken cancellationToken) =>
    {
        AuditTemplateMetadata? template =
            await repository.GetMetadataAsync(
                templateErpId,
                cancellationToken);

        if (template is null)
        {
            return Results.NotFound(new
            {
                message = $"Template {templateErpId} was not found."
            });
        }

        return Results.Ok(template);
    })
    .WithName("GetAuditTemplateMetadata")
    .WithOpenApi();

app.MapGet(
    "/api/health/database",
    async (IConfiguration configuration) =>
    {
        string? connectionString =
            configuration.GetConnectionString("AuditAddIn");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Results.Problem(
                title: "Database connection is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand("SELECT DB_NAME();", connection);

        object? databaseName =
            await command.ExecuteScalarAsync();

        return Results.Ok(
            new
            {
                status = "Healthy",
                server = "SRVHPV",
                database = databaseName?.ToString()
            });
    })
    .WithName("GetDatabaseHealth")
    .WithOpenApi();

app.MapGet(
    "/api/v1/templates/{templateErpId:int}/package",
    async (
        int templateErpId,
        AuditTemplatePackageRepository repository,
        CancellationToken cancellationToken) =>
    {
        AuditTemplatePackage? package =
            await repository.GetPackageAsync(
                templateErpId,
                cancellationToken);

        if (package is null)
        {
            return Results.NotFound(
                new
                {
                    message =
                        $"Template package {templateErpId} was not found."
                });
        }

        return Results.Ok(package);
    })
    .WithName("GetAuditTemplatePackageV1")
    .WithOpenApi();

app.MapGet(
    "/api/v2/templates/{templateErpId:int}/package",
    async (
        int templateErpId,
        string? frameworkCode,
        int? fiscalYear,
        AuditTemplatePackageRepository repository,
        CancellationToken cancellationToken) =>
    {
        if (templateErpId <= 0)
        {
            return Results.BadRequest(new { message = "Template ERP ID must be positive." });
        }

        if (string.IsNullOrWhiteSpace(frameworkCode))
        {
            return Results.BadRequest(new { message = "Framework code is required." });
        }

        if (!fiscalYear.HasValue)
        {
            return Results.BadRequest(new { message = "Fiscal year is required." });
        }

        if (fiscalYear.Value < 1900 || fiscalYear.Value > 9999)
        {
            return Results.BadRequest(new { message = "Fiscal year must be between 1900 and 9999." });
        }

        try
        {
            AuditTemplatePackageV2 package = await repository.GetPackageV2Async(
                templateErpId,
                frameworkCode.Trim(),
                fiscalYear.Value,
                cancellationToken);

            return Results.Ok(package);
        }
        catch (AuditTemplatePackageV2ResolutionException exception)
        {
            return exception.Failure switch
            {
                AuditTemplatePackageV2ResolutionFailure.TemplateNotFound => Results.NotFound(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.FrameworkNotFound => Results.NotFound(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.TemplateNotApplicable => Results.NotFound(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.AssociationNotFound => Results.NotFound(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.MultipleAssociations => Results.Conflict(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.InconsistentFrameworkVersionReferences => Results.Conflict(new { message = exception.Message }),
                AuditTemplatePackageV2ResolutionFailure.InconsistentConfiguration => Results.Conflict(new { message = exception.Message }),
                _ => throw new InvalidOperationException("Unsupported template-package resolution failure.")
            };
        }
    })
    .WithName("GetAuditTemplatePackageV2")
    .WithOpenApi();

app.MapGet(
    "/api/v1/account-frameworks/{frameworkCode}/applicable",
    async (
        string frameworkCode,
        int fiscalYear,
        AccountFrameworkRepository repository,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(frameworkCode))
        {
            return Results.BadRequest(new
            {
                message = "Framework code is required."
            });
        }

        if (fiscalYear < 1900 || fiscalYear > 9999)
        {
            return Results.BadRequest(new
            {
                message = "Fiscal year must be between 1900 and 9999."
            });
        }

        ApplicableAccountFramework? framework =
            await repository.GetApplicableAsync(
                frameworkCode.Trim(),
                fiscalYear,
                cancellationToken);

        if (framework is null)
        {
            return Results.NotFound(new
            {
                message =
                    $"No applicable version of framework " +
                    $"'{frameworkCode}' was found for fiscal year " +
                    $"{fiscalYear}."
            });
        }

        return Results.Ok(framework);
    })
    .WithName("GetApplicableAccountFrameworkV1")
    .WithOpenApi();


app.MapGet(
    "/api/v1/accounting-entities/{ico}/package",
    async (
        string ico,
        AccountingEntityPackageService service,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(ico))
        {
            return Results.BadRequest(
                new
                {
                    message = "IČO is required."
                });
        }

        try
        {
            AccountingEntityPackageV1? package =
                await service.GetPackageAsync(
                    ico.Trim(),
                    cancellationToken);

            if (package is null)
            {
                return Results.NotFound(
                    new
                    {
                        message =
                            $"Accounting entity '{ico}' was not found in RegisterUZ."
                    });
            }

            return Results.Ok(package);
        }
        catch (RegisterUzMultipleAccountingEntitiesException ex)
        {
            return Results.Conflict(
                new
                {
                    message = ex.Message
                });
        }
    })
    .WithName("GetAccountingEntityPackageV1")
    .WithOpenApi();

app.MapGet(
    "/api/debug/registeruz/accounting-entities/{ico}",
    async (
        string ico,
        RegisterUzAccountingEntityRepository repository,
        CancellationToken cancellationToken) =>
    {
        RegisterUzAccountingEntityGraph? graph =
            await repository.GetByIcoAsync(
                ico,
                cancellationToken);

        if (graph is null)
        {
            return Results.NotFound();
        }

        var financialReports =
            graph.FinancialStatements
                .SelectMany(x => x.FinancialReports)
                .Concat(
                    graph.AnnualReports
                        .SelectMany(x => x.FinancialReports))
                .ToList();

        int financialReportCount =
            financialReports.Count;

        int distinctTemplateCount =
            financialReports
                .Where(x => x.TemplateId.HasValue)
                .Select(x => x.TemplateId!.Value)
                .Distinct()
                .Count();

        int titlePageCount =
            financialReports
                .Count(x => x.TitlePage is not null);

        int financialReportAttachmentCount =
            financialReports
                .Sum(x => x.Attachments.Count);

        int annualReportAttachmentCount =
            graph.AnnualReports
                .Sum(x => x.Attachments.Count);

        int financialReportTableCount =
            financialReports
                .Sum(x => x.Tables.Count);

        int financialReportValueCount =
            financialReports
                .SelectMany(x => x.Tables)
                .Sum(x => x.Values.Count);

        int explicitZeroValueCount =
            financialReports
                .SelectMany(x => x.Tables)
                .SelectMany(x => x.Values)
                .Count(x => x.NumericValue == 0m);

        return Results.Ok(
            new
            {
                graph.Entity.Id,
                graph.Entity.Ico,
                graph.Entity.Name,
                FinancialStatementCount =
                    graph.FinancialStatements.Count,
                AnnualReportCount =
                    graph.AnnualReports.Count,
                FinancialReportCount =
                    financialReportCount,
                DistinctTemplateCount =
                    distinctTemplateCount,
                TitlePageCount =
                    titlePageCount,
                FinancialReportAttachmentCount =
                    financialReportAttachmentCount,
                AnnualReportAttachmentCount =
                    annualReportAttachmentCount,
                FinancialReportTableCount =
                    financialReportTableCount,
                FinancialReportValueCount =
                    financialReportValueCount,
                ExplicitZeroValueCount =
                    explicitZeroValueCount
            });
    })
    .WithName("DebugGetRegisterUzAccountingEntity")
    .WithOpenApi();

app.Run();
