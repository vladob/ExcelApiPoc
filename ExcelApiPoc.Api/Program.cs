using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;
using ExcelApiPoc.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditTemplateRepository>();
builder.Services.AddScoped<AuditTemplatePackageRepository>();
builder.Services.AddScoped<AccountFrameworkRepository>();
builder.Services.AddSingleton<RegisterUzAccountingEntityRepository>();

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
    async (int templateErpId, AuditTemplateRepository repository, CancellationToken cancellationToken) =>
    {
        AuditTemplateMetadata? template = await repository.GetMetadataAsync(templateErpId, cancellationToken);
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

app.MapGet("/api/health/database",
    async (IConfiguration configuration) =>
    {
        string? connectionString = configuration.GetConnectionString("AuditAddIn");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Results.Problem(title: "Database connection is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT DB_NAME();", connection);
        object? databaseName = await command.ExecuteScalarAsync();

        return Results.Ok(new {status = "Healthy", server = "SRVHPV", database = databaseName?.ToString()});
    })
.WithName("GetDatabaseHealth")
.WithOpenApi();

app.MapGet(
    "/api/v1/templates/{templateErpId:int}/package",
    async (int templateErpId,AuditTemplatePackageRepository repository,CancellationToken cancellationToken) =>
    {
        AuditTemplatePackage? package = await repository.GetPackageAsync( templateErpId, cancellationToken);

        if (package is null)
        {
            return Results.NotFound(new{message = $"Template package {templateErpId} was not found."});
        }

        return Results.Ok(package);
    })
    .WithName("GetAuditTemplatePackageV1")
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

        ApplicableAccountFramework? framework = await repository.GetApplicableAsync(frameworkCode.Trim(), fiscalYear, cancellationToken);

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

        int financialReportCount =
            graph.FinancialStatements
                .Sum(x => x.FinancialReports.Count)
            +
            graph.AnnualReports
                .Sum(x => x.FinancialReports.Count);

        int distinctTemplateCount =
            graph.FinancialStatements
                .SelectMany(x => x.FinancialReports)
                .Concat(
                    graph.AnnualReports
                        .SelectMany(x => x.FinancialReports))
                .Where(x => x.TemplateId.HasValue)
                .Select(x => x.TemplateId!.Value)
                .Distinct()
                .Count();

        int titlePageCount =
            graph.FinancialStatements
                .SelectMany(x => x.FinancialReports)
                .Concat(
                    graph.AnnualReports
                        .SelectMany(x => x.FinancialReports))
                .Count(x => x.TitlePage is not null);

        int financialReportAttachmentCount =
            graph.FinancialStatements
                .SelectMany(x => x.FinancialReports)
                .Concat(
                    graph.AnnualReports
                        .SelectMany(x => x.FinancialReports))
                .Sum(x => x.Attachments.Count);

        int annualReportAttachmentCount =
            graph.AnnualReports
                .Sum(x => x.Attachments.Count);

        return Results.Ok(
            new
            {
                graph.Entity.Id,
                graph.Entity.Ico,
                graph.Entity.Name,
                FinancialStatementCount = graph.FinancialStatements.Count,
                AnnualReportCount = graph.AnnualReports.Count,
                FinancialReportCount = financialReportCount,
                DistinctTemplateCount = distinctTemplateCount,
                TitlePageCount = titlePageCount,
                FinancialReportAttachmentCount = financialReportAttachmentCount,
                AnnualReportAttachmentCount = annualReportAttachmentCount
            });
    })
    .WithName("DebugGetRegisterUzAccountingEntity")
    .WithOpenApi();

app.Run();