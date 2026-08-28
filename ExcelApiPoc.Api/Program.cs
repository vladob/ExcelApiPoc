using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;
using ExcelApiPoc.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditTemplateRepository>();
builder.Services.AddScoped<AuditTemplatePackageRepository>();
builder.Services.AddScoped<AccountFrameworkRepository>();

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

app.Run();