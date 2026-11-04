using ExcelApiPoc.Api.Data;
using ExcelApiPoc.MkSoft.DataAccess;
namespace ExcelApiPoc.Api;
public static class MkSoftEndpoints
{
    public static void MapMkSoft(this WebApplication app)
    {
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
