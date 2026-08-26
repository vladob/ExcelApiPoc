using ExcelApiPoc.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapGet("/api/templates/{templateErpId:int}/metadata",
    (int templateErpId) =>
    {
        if (templateErpId != 690)
        {
            return Results.NotFound(new
            {
                message = $"Template {templateErpId} was not found."
            });
        }

        var template = new AuditTemplateMetadata
        {
            TemplateErpId = 690,
            Name = "Súvaha Úè ROPO SFOV 1-01",
            MfSpecification = "MF/21227/2014-31",
            ValidFrom = new DateOnly(2014, 1, 1),
            ValidTo = null,
            Tables =
            [
                new AuditTableMetadata
            {
                TableErpId = 69001,
                NameSk = "Strana aktív",
                NameEn = "Assets",
                NumberOfColumns = 7,
                NumberOfDataColumns = 4,
                DontHaveRowNumbers = false
            },
            new AuditTableMetadata
            {
                TableErpId = 69002,
                NameSk = "Strana pasív",
                NameEn = "Liabilities and Equity",
                NumberOfColumns = 5,
                NumberOfDataColumns = 2,
                DontHaveRowNumbers = false
            }
            ]
        };

        return Results.Ok(template);
    })
.WithName("GetAuditTemplateMetadata")
.WithOpenApi();

app.Run();