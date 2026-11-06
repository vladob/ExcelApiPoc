using ExcelApiPoc.Api.Data;
using ExcelApiPoc.MkSoft.DataAccess;
namespace ExcelApiPoc.Api;
public static class MkSoftEndpoints
{
    public static void MapMkSoft(this WebApplication app)
    {
        app.MapGet("/api/v1/mksoft/entities",
            async (int fiscalYear, int? clientErpId, string? ico, HttpContext http,
                AccountDetailSettingsRepository users, MkSoftRepository repository, CancellationToken ct) =>
            {
                string? key = http.Request.Headers["X-Api-Key"].FirstOrDefault();
                if (key is null || key.Length != 64 || key.Any(c => !Uri.IsHexDigit(c))) return Results.Unauthorized();
                var access = await users.ResolveMkSoftAccessAsync(key, ct);
                if (access is null) return Results.Unauthorized();
                if (access != true) return Results.StatusCode(StatusCodes.Status403Forbidden);
                ico = ico?.Trim();
                if (fiscalYear < 1900 || fiscalYear > 9999 || clientErpId <= 0 ||
                    (!clientErpId.HasValue && string.IsNullOrEmpty(ico)) ||
                    (!string.IsNullOrEmpty(ico) && (ico.Length != 8 || ico.Any(c => c < '0' || c > '9'))))
                    return Results.BadRequest();
                var matches = await repository.FindEntitiesAsync(clientErpId, ico ?? "", fiscalYear, ct);
                return Results.Ok(matches);
            }).WithTags("MkSoft").WithName("FindMkSoftEntitiesV1").WithOpenApi();

        app.MapGet("/api/v1/mksoft/clients/{clientErpId:int}/years/{fiscalYear:int}/accounting-framework",
            async (int clientErpId, int fiscalYear, HttpContext http, AccountDetailSettingsRepository users, MkSoftRepository repository, CancellationToken ct) =>
            {
                string? key = http.Request.Headers["X-Api-Key"].FirstOrDefault();
                if (key is null || key.Length != 64 || key.Any(c => !Uri.IsHexDigit(c))) return Results.Unauthorized();
                var access = await users.ResolveMkSoftAccessAsync(key, ct);
                if (access is null) return Results.Unauthorized();
                if (access != true) return Results.StatusCode(StatusCodes.Status403Forbidden);
                if (clientErpId <= 0 || fiscalYear < 1900 || fiscalYear > 9999) return Results.BadRequest();
                return Results.Ok(await repository.GetAccountingFrameworkAsync(clientErpId, fiscalYear, ct));
            }).WithTags("MkSoft").WithName("GetMkSoftAccountingFrameworkV1").WithOpenApi();

        app.MapGet("/api/v1/mksoft/clients/{clientErpId:int}/years/{fiscalYear:int}/general-ledger",
            async (int clientErpId, int fiscalYear, bool? includeEmptyAccounts, HttpContext http,
                AccountDetailSettingsRepository users, MkSoftRepository repository, CancellationToken ct) =>
            {
                string? key = http.Request.Headers["X-Api-Key"].FirstOrDefault();
                if (key is null || key.Length != 64 || key.Any(c => !Uri.IsHexDigit(c))) return Results.Unauthorized();
                bool? canAccessMkSoft = await users.ResolveMkSoftAccessAsync(key, ct);
                if (canAccessMkSoft is null) return Results.Unauthorized();
                if (canAccessMkSoft != true) return Results.StatusCode(StatusCodes.Status403Forbidden);
                if (clientErpId <= 0 || fiscalYear < 1900 || fiscalYear > 9999) return Results.BadRequest();
                var package = await repository.GetAsync(clientErpId, fiscalYear, includeEmptyAccounts ?? false, ct);
                return package is null ? (IResult)Results.NotFound() : Results.Ok(package);
            }).WithTags("MkSoft").WithName("GetMkSoftGeneralLedgerV1").WithOpenApi();
    }
}
