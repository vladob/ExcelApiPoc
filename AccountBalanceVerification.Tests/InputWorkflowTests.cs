using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AccountBalanceVerification.Core;
using AccountBalanceVerification.Import;
using ExcelApiPoc.MkSoft.Client;
using Xunit;

namespace AccountBalanceVerification.Tests;
public class InputWorkflowTests
{
    [Theory]
    [InlineData("U_DENNIK_00332780_202512.CSV")]
    [InlineData("U_DENNIK_00332780_2025.CSV")]
    [InlineData("U_DENNIK_00332780_202512 (1).CSV")]
    public void FilenameRetainsLeadingZerosAndExtractsYear(string filename)
    {
        var metadata = FileMetadata.Read(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), filename), null, null, null);
        Assert.Equal("00332780", metadata.Ico);
        Assert.Equal(2025, metadata.FiscalYear);
    }

    [Fact]
    public void ChartMetadataUsesContentsAndReportsFilenameConflict()
    {
        WithChart("UCT_ROZVRH_12345678_2024.CSV", path => {
            var metadata = FileMetadata.Read(null, path, null, null);
            Assert.Equal("00332780", metadata.Ico);
            Assert.Equal(2025, metadata.FiscalYear);
            Assert.Equal("TEST ENTITY", metadata.EntityName);
            Assert.Equal("IfoSoft", metadata.Producer);
            Assert.Contains(metadata.Warnings, w => w.Contains("IČO v názve"));
            Assert.Contains(metadata.Warnings, w => w.Contains("Rok v názve"));
        });
    }

    [Fact]
    public void ChartSuppliesNamesWithoutChangingBalances()
    {
        WithChart("UCT_ROZVRH_00332780_2025.CSV", path => {
            var package = new VerificationPackage();
            package.Rows.Add(new LedgerRow { AccountNo = "211001", ClosingDebit = 125m });
            FileSource.ApplyChart(package, "IfoSoft", path, "00332780", 2025);
            Assert.Equal("Cash detail", package.Rows.Single().AccountName);
            Assert.Equal(125m, package.Rows.Single().ClosingDebit);
            Assert.Contains(package.OfficialAccounts, x => x.AccountCode == "211" && x.AccountName == "Cash");
            Assert.Throws<InvalidOperationException>(() => FileSource.ApplyChart(package, "IfoSoft", path, "12345678", 2025));
        });
    }

    [Theory]
    [InlineData(null, "00332780", "&ico=00332780")]
    [InlineData(11, "", "&clientErpId=11")]
    public async Task LookupPreservesIdentifiersAndAllCandidates(int? id, string ico, string query)
    {
        var handler = new Handler("[{\"clientErpId\":11,\"ico\":\"00332780\"},{\"clientErpId\":12,\"ico\":\"00332780\"}]");
        using var http = new HttpClient(handler);
        var matches = await new MkSoftClient(http, "https://example.test/audit", "test-key").FindEntitiesAsync(2025, id, ico, CancellationToken.None);
        Assert.Equal("https://example.test/audit/api/v1/mksoft/entities?fiscalYear=2025" + query, handler.Url);
        Assert.Equal("test-key", handler.Key);
        Assert.Equal(2, matches.Count);
        Assert.Equal("00332780", matches[0].Ico);
    }

    [Fact]
    public async Task OfficialFrameworkTakesPrecedenceOverChartNames()
    {
        using var http = new HttpClient(new Handler("{\"frameworkVersionId\":1,\"definitions\":[{\"accountCode\":\"211\",\"accountLevel\":3,\"officialName\":\"Official cash\"}]}"));
        var package = new VerificationPackage { FiscalYear = 2025 };
        package.OfficialAccounts.Add(new OfficialAccount { AccountCode = "211", AccountName = "Chart cash", AccountLevel = 3 });
        package.OfficialAccounts.Add(new OfficialAccount { AccountCode = "299", AccountName = "Chart fallback", AccountLevel = 3 });
        await new MkSoftClient(http, "https://example.test", "test-key").ApplyFrameworkAsync(package, "PROFIT", CancellationToken.None);
        Assert.Equal("Official cash", package.OfficialAccounts.Single(x => x.AccountCode == "211").AccountName);
        Assert.Equal("Chart fallback", package.OfficialAccounts.Single(x => x.AccountCode == "299").AccountName);
    }

    private static void WithChart(string filename, Action<string> action)
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        try {
            var path = Path.Combine(folder, filename);
            File.WriteAllText(path, "00332780 TEST ENTITY\nUctovny rozvrh 2025\n" +
                "Syn;Ana;Typ;Druh;Dan;Nazov uctu;Saldo;DPH;Pods;Nazov uctu angl;Nazov uctu nem.\n" +
                "211;;M;;;Cash;;;;;\n211;001;M;;;Cash detail;;;;;\n", new UTF8Encoding(true));
            action(path);
        }
        finally { Directory.Delete(folder, true); }
    }
    private sealed class Handler : HttpMessageHandler
    {
        private readonly string body;
        public string Url, Key;
        public Handler(string body) { this.body = body; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Url = request.RequestUri.ToString(); Key = request.Headers.GetValues("X-Api-Key").Single();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
        }
    }
}
