using System.Net;
using System.Text;
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
}
