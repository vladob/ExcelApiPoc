using System.Net;
using System.Text;
using System.Text.Json;
using RegisterUz.Client;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzApiClientTests
{
    [Fact]
    public async Task AccountingEntity_detail_is_deserialized_with_natural_child_order()
    {
        const string json = """
            {
              "id": 30514,
              "idUctovnychZavierok": [6739816, 2267867],
              "idVyrocnychSprav": [3015939, 5321462],
              "ico": "00325554",
              "nazovUJ": "Obec Oreské",
              "datumPoslednejUpravy": "2026-07-03"
            }
            """;
        RegisterUzApiClient client = CreateClient(json);

        var document = await client.GetAccountingEntityAsync(30514);

        Assert.Equal("00325554", document.Value.Ico);
        Assert.Equal(new long[] { 6739816L, 2267867L }, document.Value.FinancialStatementIds);
        Assert.Equal(new long[] { 3015939L, 5321462L }, document.Value.AnnualReportIds);
        Assert.Equal(json, document.RawJson);
    }

    [Fact]
    public async Task Financial_report_keeps_table_and_flattened_value_order()
    {
        const string json = """
            {
              "id": 10005263,
              "idUctovnejZavierky": 6739816,
              "idSablony": 690,
              "obsah": {
                "tabulky": [
                  { "nazov": { "sk": "Strana aktív" }, "data": ["1", "2", "3", "4"] },
                  { "nazov": { "sk": "Strana pasív" }, "data": ["5", "6"] }
                ]
              }
            }
            """;
        RegisterUzApiClient client = CreateClient(json);

        var document = await client.GetFinancialReportAsync(10005263);

        Assert.Equal("Strana aktív", document.Value.Content!.Tables[0].Name!.Sk);
        Assert.Equal(new string?[] { "1", "2", "3", "4" }, document.Value.Content.Tables[0].Data);
        Assert.Equal("Strana pasív", document.Value.Content.Tables[1].Name!.Sk);
    }

    [Fact]
    public async Task Catalog_bundle_loads_seven_useful_endpoints_and_excludes_registered_offices()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/cruz-public/api/sablony"] = """{"sablony":[{"id":15181,"nazov":"Výkaz"}]}""",
            ["/cruz-public/api/pravne-formy"] = Classification("801", "Obec"),
            ["/cruz-public/api/sk-nace"] = Classification("84110", "Všeobecná verejná správa"),
            ["/cruz-public/api/druhy-vlastnictva"] = Classification("5", "Samospráva"),
            ["/cruz-public/api/velkosti-organizacie"] = Classification("04", "3-4 zamestnanci"),
            ["/cruz-public/api/kraje"] = Location("SK042", "Košický kraj", null),
            ["/cruz-public/api/okresy"] = Location("SK0427", "Michalovce", "SK042")
        };
        var handler = new RoutingStubHandler(responses);
        var client = new RegisterUzApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri(RegisterUzApiClient.DefaultBaseAddress)
        });

        RegisterUz.Core.RegisterUzCatalogPackage catalogs = await client.GetCatalogsAsync();

        Assert.Single(catalogs.Templates.Value.Templates);
        Assert.Equal(15181, catalogs.Templates.Value.Templates[0].Id);
        Assert.Equal("801", Assert.Single(catalogs.LegalForms.Value.Classifications).Code);
        Assert.Equal("SK0427", Assert.Single(catalogs.Districts.Value.Locations).Code);
        Assert.Equal(7, handler.RequestPaths.Count);
        Assert.DoesNotContain(handler.RequestPaths, path => path.Contains("sidla", StringComparison.Ordinal));
    }

    private static string Classification(string code, string name) =>
        JsonSerializer.Serialize(new
        {
            klasifikacie = new[]
            {
                new { kod = code, nazov = new { sk = name } }
            }
        });

    private static string Location(string code, string name, string? parent) =>
        parent is null
            ? JsonSerializer.Serialize(new
            {
                lokacie = new[]
                {
                    new { kod = code, nazov = new { sk = name } }
                }
            })
            : JsonSerializer.Serialize(new
            {
                lokacie = new[]
                {
                    new { nadradenaLokacia = parent, kod = code, nazov = new { sk = name } }
                }
            });

    private static RegisterUzApiClient CreateClient(string json)
    {
        var handler = new StubHandler(json);
        return new RegisterUzApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri(RegisterUzApiClient.DefaultBaseAddress)
        });
    }

    private sealed class StubHandler(string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
                RequestMessage = request
            });
    }

    private sealed class RoutingStubHandler(IReadOnlyDictionary<string, string> responses) : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            string path = request.RequestUri?.AbsolutePath
                          ?? throw new InvalidOperationException("Request URI is missing.");
            RequestPaths.Add(path);
            if (!responses.TryGetValue(path, out string? content))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
                RequestMessage = request
            });
        }
    }
}
