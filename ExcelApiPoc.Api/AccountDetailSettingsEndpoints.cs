using ExcelApiPoc.Api.Data;
using ExcelApiPoc.Api.Models;
using Microsoft.Data.SqlClient;

namespace ExcelApiPoc.Api;

public static class AccountDetailSettingsEndpoints
{
    public static void MapAccountDetailSettings(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/account-detail")
            .WithTags("Account detail settings")
            .AddEndpointFilter(async (context, next) =>
            {
                var http = context.HttpContext;
                string? key = http.Request.Headers["X-Api-Key"].FirstOrDefault();
                if (key is null || key.Length != 64 || key.Any(c => !Uri.IsHexDigit(c)))
                    return Results.Unauthorized();
                var repository = http.RequestServices.GetRequiredService<AccountDetailSettingsRepository>();
                int? userId = await repository.ResolveUserAsync(key, http.RequestAborted);
                if (userId is null) return Results.Unauthorized();
                http.Items["AccountDetailUserId"] = userId.Value;
                return await next(context);
            });

        group.MapGet("/texts", async (HttpContext http, AccountDetailSettingsRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetTextsAsync(UserId(http), ct)));

        group.MapGet("/layout", async (HttpContext http, AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            var layout = await repository.GetLayoutAsync(UserId(http), ct);
            return layout is null ? (IResult)Results.NotFound() : Results.Ok(layout);
        });

        group.MapPost("/texts", async (
            HttpContext http, CreatePredefinedTextRequest request,
            AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            if (!ValidCategory(request.CategoryCode) || !ValidText(request.TextSk)
                || request.SortOrder is <= 0)
                return Results.BadRequest(new { message = "Invalid category, text, or sort order." });
            var created = await repository.CreateTextAsync(UserId(http), request, ct);
            return created is null
                ? (IResult)Results.BadRequest(new { message = "Unknown text category." })
                : Results.Created($"/api/v1/account-detail/texts/{created.CategoryCode}/{created.TextId}", created);
        });

        group.MapPut("/texts/{category}/{textId:int}", async (
            HttpContext http, string category, int textId, UpdatePredefinedTextRequest request,
            AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            if (!ValidCategory(category) || textId <= 0 || !ValidText(request.TextSk) || request.SortOrder <= 0)
                return Results.BadRequest();
            return await repository.UpdateTextAsync(UserId(http), category, textId, request, ct)
                ? (IResult)Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/texts/{category}/{textId:int}", async (
            HttpContext http, string category, int textId,
            AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            if (!ValidCategory(category) || textId <= 0) return Results.BadRequest();
            try
            {
                return await repository.DeleteTextAsync(UserId(http), category, textId, ct)
                    ? (IResult)Results.NoContent() : Results.NotFound();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return Results.Conflict(new { message = "This text is an account default. Clear or replace those defaults first." });
            }
        });

        group.MapPut("/defaults/{account}/{category}", async (
            HttpContext http, string account, string category, SetAccountTextDefaultRequest request,
            AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            if (!ValidAccount(account) || !ValidCategory(category) || request.TextId <= 0)
                return Results.BadRequest();
            return await repository.SetDefaultAsync(UserId(http), account, category, request.TextId, ct)
                ? (IResult)Results.NoContent()
                : Results.BadRequest(new { message = "The text does not exist in this auditor's category." });
        });

        group.MapDelete("/defaults/{account}/{category}", async (
            HttpContext http, string account, string category,
            AccountDetailSettingsRepository repository, CancellationToken ct) =>
        {
            if (!ValidAccount(account) || !ValidCategory(category)) return Results.BadRequest();
            return await repository.DeleteDefaultAsync(UserId(http), account, category, ct)
                ? (IResult)Results.NoContent() : Results.NotFound();
        });
    }

    private static int UserId(HttpContext http) => (int)http.Items["AccountDetailUserId"]!;
    private static bool ValidAccount(string account) => account.Length == 3 && account.All(c => c is >= '0' and <= '9');
    private static bool ValidCategory(string? category) => category?.Length is > 0 and <= 50 && category.All(c => char.IsLetterOrDigit(c));
    private static bool ValidText(string? text) => !string.IsNullOrWhiteSpace(text) && text.Length <= 500;
}
