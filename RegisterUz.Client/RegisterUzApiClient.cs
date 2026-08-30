using System.Net;
using System.Globalization;
using System.Text;
using System.Text.Json;
using RegisterUz.Core;

namespace RegisterUz.Client;

public sealed class RegisterUzApiClient : IRegisterUzClient
{
    public const string DefaultBaseAddress = "https://www.registeruz.sk/cruz-public/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public RegisterUzApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress ??= new Uri(DefaultBaseAddress, UriKind.Absolute);
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ExcelApiPoc-RegisterUz/1.0");
    }

    public async Task<IReadOnlyList<long>> FindAccountingEntityIdsByIcoAsync(
        string ico,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);

        var ids = new List<long>();
        long? continueAfterId = null;

        do
        {
            string path =
                "api/uctovne-jednotky?zmenene-od=2000-01-01" +
                $"&ico={Uri.EscapeDataString(ico)}" +
                (continueAfterId.HasValue
                    ? $"&pokracovat-za-id={continueAfterId.Value}"
                    : string.Empty);

            RegisterUzDocument<IdListDto> page =
                await GetAsync<IdListDto>(path, cancellationToken);

            ids.AddRange(page.Value.Ids);
            continueAfterId = page.Value.HasMoreIds && page.Value.Ids.Length > 0
                ? page.Value.Ids[^1]
                : null;
        }
        while (continueAfterId.HasValue);

        return ids.Distinct().OrderBy(id => id).ToArray();
    }

    public async Task<RegisterUzChangeFeedPage> GetChangedIdsAsync(
        RegisterUzObjectType objectType,
        DateTime changedSinceUtc,
        long? continueAfterId,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (changedSinceUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Changed-since timestamp must be UTC.", nameof(changedSinceUtc));
        if (continueAfterId is <= 0)
            throw new ArgumentOutOfRangeException(nameof(continueAfterId));
        if (pageSize is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        string path = GetListPath(objectType) +
                      "?zmenene-od=" + Uri.EscapeDataString(
                          changedSinceUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture)) +
                      $"&max-zaznamov={pageSize}" +
                      (continueAfterId.HasValue
                          ? $"&pokracovat-za-id={continueAfterId.Value}"
                          : string.Empty);
        DateTime requestedAtUtc = DateTime.UtcNow;
        RegisterUzDocument<IdListDto> document =
            await GetAsync<IdListDto>(path, cancellationToken);

        return new RegisterUzChangeFeedPage(
            objectType,
            changedSinceUtc,
            continueAfterId,
            pageSize,
            path,
            requestedAtUtc,
            document);
    }

    public Task<RegisterUzDocument<AccountingEntityDto>> GetAccountingEntityAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        GetAsync<AccountingEntityDto>($"api/uctovna-jednotka?id={ValidateId(id)}", cancellationToken);

    public Task<RegisterUzDocument<FinancialStatementDto>> GetFinancialStatementAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        GetAsync<FinancialStatementDto>($"api/uctovna-zavierka?id={ValidateId(id)}", cancellationToken);

    public Task<RegisterUzDocument<AnnualReportDto>> GetAnnualReportAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        GetAsync<AnnualReportDto>($"api/vyrocna-sprava?id={ValidateId(id)}", cancellationToken);

    public Task<RegisterUzDocument<FinancialReportDto>> GetFinancialReportAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        GetAsync<FinancialReportDto>($"api/uctovny-vykaz?id={ValidateId(id)}", cancellationToken);

    public Task<RegisterUzDocument<FinancialReportTemplateDto>> GetTemplateAsync(
        long id,
        CancellationToken cancellationToken = default) =>
        GetAsync<FinancialReportTemplateDto>($"api/sablona?id={ValidateId(id)}", cancellationToken);

    public async Task<RegisterUzCatalogPackage> GetCatalogsAsync(
        CancellationToken cancellationToken = default)
    {
        // Catalog requests deliberately run sequentially. RegisterUZ is a public
        // anonymous service and seven small requests add negligible latency.
        RegisterUzDocument<TemplateCatalogDto> templates =
            await GetAsync<TemplateCatalogDto>("api/sablony", cancellationToken);
        RegisterUzDocument<ClassificationCatalogDto> legalForms =
            await GetAsync<ClassificationCatalogDto>("api/pravne-formy", cancellationToken);
        RegisterUzDocument<ClassificationCatalogDto> skNace =
            await GetAsync<ClassificationCatalogDto>("api/sk-nace", cancellationToken);
        RegisterUzDocument<ClassificationCatalogDto> ownershipTypes =
            await GetAsync<ClassificationCatalogDto>("api/druhy-vlastnictva", cancellationToken);
        RegisterUzDocument<ClassificationCatalogDto> organizationSizes =
            await GetAsync<ClassificationCatalogDto>("api/velkosti-organizacie", cancellationToken);
        RegisterUzDocument<LocationCatalogDto> regions =
            await GetAsync<LocationCatalogDto>("api/kraje", cancellationToken);
        RegisterUzDocument<LocationCatalogDto> districts =
            await GetAsync<LocationCatalogDto>("api/okresy", cancellationToken);

        return new RegisterUzCatalogPackage(
            templates,
            legalForms,
            skNace,
            ownershipTypes,
            organizationSizes,
            regions,
            districts);
    }

    private async Task<RegisterUzDocument<T>> GetAsync<T>(
        string relativePath,
        CancellationToken cancellationToken)
    {
        DateTime requestedAtUtc = DateTime.UtcNow;
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(
                relativePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new RegisterUzApiException(
                relativePath, null, string.Empty, requestedAtUtc,
                "RegisterUZ request failed before an HTTP response was received.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RegisterUzApiException(
                relativePath, null, string.Empty, requestedAtUtc,
                "RegisterUZ request timed out before an HTTP response was received.", exception);
        }

        using (response)
        {

            string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            string? apiVersion = response.Headers.TryGetValues("X-API-Version", out IEnumerable<string>? values)
                ? values.FirstOrDefault()
                : null;
            if (!response.IsSuccessStatusCode)
            {
                throw new RegisterUzApiException(
                    relativePath,
                    response.StatusCode,
                    rawJson,
                    requestedAtUtc,
                    apiVersion: apiVersion);
            }

            T? value;
            try
            {
                value = JsonSerializer.Deserialize<T>(rawJson, JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new RegisterUzApiException(
                    relativePath,
                    response.StatusCode,
                    rawJson,
                    requestedAtUtc,
                    "RegisterUZ returned invalid or unexpected JSON.",
                    exception,
                    apiVersion);
            }

            if (value is null)
                throw new RegisterUzApiException(
                    relativePath, response.StatusCode, rawJson, requestedAtUtc,
                    "RegisterUZ returned an empty JSON document.", apiVersion: apiVersion);

            return new RegisterUzDocument<T>(
                value,
                rawJson,
                DateTime.UtcNow,
                (int)response.StatusCode,
                apiVersion);
        }
    }

    private static long ValidateId(long id) =>
        id > 0 ? id : throw new ArgumentOutOfRangeException(nameof(id));

    private static string GetListPath(RegisterUzObjectType objectType) => objectType switch
    {
        RegisterUzObjectType.AccountingEntity => "api/uctovne-jednotky",
        RegisterUzObjectType.FinancialStatement => "api/uctovne-zavierky",
        RegisterUzObjectType.FinancialReport => "api/uctovne-vykazy",
        RegisterUzObjectType.AnnualReport => "api/vyrocne-spravy",
        _ => throw new ArgumentOutOfRangeException(nameof(objectType))
    };
}

public sealed class RegisterUzApiException : Exception, IRegisterUzRequestFailure
{
    public RegisterUzApiException(
        string requestPath,
        HttpStatusCode? statusCode,
        string responseBody,
        DateTime requestedAtUtc,
        string? message = null,
        Exception? innerException = null,
        string? apiVersion = null)
        : base(message ?? $"RegisterUZ request failed with HTTP {(int)statusCode!.Value} ({statusCode}).", innerException)
    {
        RequestPath = requestPath;
        StatusCode = statusCode;
        ResponseBody = responseBody;
        RequestedAtUtc = requestedAtUtc;
        ApiVersion = apiVersion;
    }

    public string RequestPath { get; }
    public HttpStatusCode? StatusCode { get; }
    public string ResponseBody { get; }
    public DateTime RequestedAtUtc { get; }
    int? IRegisterUzRequestFailure.HttpStatusCode => StatusCode.HasValue ? (int)StatusCode.Value : null;
    public long? ResponseBytes => Encoding.UTF8.GetByteCount(ResponseBody);
    public string? ApiVersion { get; }
}
