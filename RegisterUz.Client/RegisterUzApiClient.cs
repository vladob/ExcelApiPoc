using System.Net;
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

    private async Task<RegisterUzDocument<T>> GetAsync<T>(
        string relativePath,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(relativePath, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        string rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new RegisterUzApiException(
                relativePath,
                response.StatusCode,
                rawJson);
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
                "RegisterUZ returned invalid or unexpected JSON.",
                exception);
        }

        if (value is null)
            throw new RegisterUzApiException(relativePath, response.StatusCode, rawJson, "RegisterUZ returned an empty JSON document.");

        string? apiVersion = response.Headers.TryGetValues("X-API-Version", out IEnumerable<string>? values)
            ? values.FirstOrDefault()
            : null;

        return new RegisterUzDocument<T>(
            value,
            rawJson,
            DateTime.UtcNow,
            (int)response.StatusCode,
            apiVersion);
    }

    private static long ValidateId(long id) =>
        id > 0 ? id : throw new ArgumentOutOfRangeException(nameof(id));
}

public sealed class RegisterUzApiException : Exception
{
    public RegisterUzApiException(
        string requestPath,
        HttpStatusCode statusCode,
        string responseBody,
        string? message = null,
        Exception? innerException = null)
        : base(message ?? $"RegisterUZ request failed with HTTP {(int)statusCode} ({statusCode}).", innerException)
    {
        RequestPath = requestPath;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public string RequestPath { get; }
    public HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }
}
