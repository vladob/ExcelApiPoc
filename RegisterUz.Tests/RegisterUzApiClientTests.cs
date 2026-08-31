using System.Net;
using System.Text;
using System.Text.Json;
using RegisterUz.Client;
using RegisterUz.Core;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzApiClientTests
{
    [Theory]
    [InlineData(RegisterUzObjectType.AccountingEntity, "/cruz-public/api/uctovne-jednotky")]
    [InlineData(RegisterUzObjectType.FinancialStatement, "/cruz-public/api/uctovne-zavierky")]
    [InlineData(RegisterUzObjectType.FinancialReport, "/cruz-public/api/uctovne-vykazy")]
    [InlineData(RegisterUzObjectType.AnnualReport, "/cruz-public/api/vyrocne-spravy")]
    public async Task Change_feed_uses_the_correct_list_path_and_exact_utc_cursor(
        RegisterUzObjectType objectType,
        string expectedPath)
    {
        var handler = new CapturingStubHandler("""{"id":[101,205],"existujeDalsieId":true}""");
        var client = new RegisterUzApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri(RegisterUzApiClient.DefaultBaseAddress)
        });

        RegisterUzChangeFeedPage page = await client.GetChangedIdsAsync(
            objectType,
            new DateTime(2026, 8, 30, 18, 33, 56, DateTimeKind.Utc),
            100,
            250);

        Assert.Equal(expectedPath, handler.RequestUri!.AbsolutePath);
        Assert.Contains("zmenene-od=2026-08-30T18%3A33%3A56Z", handler.RequestUri.Query);
        Assert.Contains("max-zaznamov=250", handler.RequestUri.Query);
        Assert.Contains("pokracovat-za-id=100", handler.RequestUri.Query);
        Assert.Equal(new long[] { 101, 205 }, page.Document.Value.Ids);
        Assert.True(page.Document.Value.HasMoreIds);
    }

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

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    [InlineData("null", null)]
    public async Task Financial_report_accepts_native_and_legacy_title_page_booleans(
        string jsonValue,
        bool? expected)
    {
        string json = $$"""
            {
              "id": 7642955,
              "idUctovnejZavierky": 3543641,
              "obsah": {
                "titulnaStrana": { "konsolidovana": {{jsonValue}} },
                "tabulky": []
              }
            }
            """;
        RegisterUzApiClient client = CreateClient(json);

        RegisterUzDocument<FinancialReportDto> document =
            await client.GetFinancialReportAsync(7642955);

        Assert.Equal(expected, document.Value.Content!.TitlePage!.IsConsolidated);
    }

    [Fact]
    public async Task Financial_report_rejects_an_unknown_title_page_boolean_string()
    {
        const string json = """
            {
              "id": 7642955,
              "idUctovnejZavierky": 3543641,
              "obsah": {
                "titulnaStrana": { "konsolidovana": "yes" },
                "tabulky": []
              }
            }
            """;
        RegisterUzApiClient client = CreateClient(json);

        await Assert.ThrowsAsync<RegisterUzApiException>(
            () => client.GetFinancialReportAsync(7642955));
    }

    [Fact]
    public async Task Catalog_bundle_loads_seven_useful_endpoints_and_excludes_registered_offices()
    {
        var responses = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/cruz-public/api/sablony"] = """{"sablony":[{"id":15181,"nazov":"Výkaz","tabulky":[{"nazov":{"sk":"Súvaha"},"hlavicka":[],"pocetStlpcov":7,"pocetDatovychStlpcov":4,"riadky":[]}]}]}""",
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
        TemplateTableDto table = Assert.Single(catalogs.Templates.Value.Templates[0].Tables);
        Assert.Equal(7, table.NumberOfColumns);
        Assert.Equal(4, table.NumberOfDataColumns);
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

    private sealed class CapturingStubHandler(string content) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
                RequestMessage = request
            });
        }
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
