using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountBalanceVerification.Core;
using ExcelApiPoc.MkSoft.Client;
using Xunit;
namespace AccountBalanceVerification.Tests;
public class WorkbookPlanTests
{
    [Theory]
    [InlineData(false, 1)] [InlineData(true, 2)]
    public void CheckboxAddsOnlyEmptySyntheticSheets(bool includeEmpty, int count)
    {
        var p = new VerificationPackage();
        p.OfficialAccounts.Add(new OfficialAccount { AccountCode = "221", AccountLevel = 3 });
        p.OfficialAccounts.Add(new OfficialAccount { AccountCode = "022", AccountLevel = 3 });
        p.OfficialAccounts.Add(new OfficialAccount { AccountCode = "501", AccountLevel = 3 });
        p.Rows.Add(new LedgerRow { AccountNo = "22111", MovementsDebit = 25, MovementsCredit = 25 });
        p.Rows.Add(new LedgerRow { AccountNo = "22112" });
        p.Rows.Add(new LedgerRow { AccountNo = "50110", MovementsDebit = 25 });
        var result = WorkbookPlan.Build(p, includeEmpty);
        Assert.Equal(count, result.Count);
        Assert.Single(result.Single(s => s.Code == "221").Rows);
        Assert.DoesNotContain(result, s => s.Code == "501");
        if (includeEmpty) Assert.Empty(result.Single(s => s.Code == "022").Rows);
    }
    [Fact]
    public void PreservesLeadingZerosAlphanumericCodesAndNegativeCredit()
    {
        var p = new VerificationPackage();
        p.Rows.Add(new LedgerRow { AccountNo = "02210", OpeningDebit = 5, ClosingDebit = 5 });
        p.Rows.Add(new LedgerRow { AccountNo = "343OPOD", ClosingDebit = 1 });
        p.Rows.Add(new LedgerRow { AccountNo = "428120", OpeningCredit = -12.13m, ClosingDebit = 12.13m });
        var sheets = WorkbookPlan.Build(p, false);
        Assert.Equal(new[] { "022", "343", "428" }, sheets.Select(s => s.Code));
        Assert.Equal(12.13m, sheets[2].Rows[0].OpeningBalance);
    }
    [Fact]
    public void OpposingAnalyticalBalancesDoNotSuppressSheet()
    {
        var p = new VerificationPackage();
        p.Rows.Add(new LedgerRow { AccountNo = "34310", ClosingDebit = 10 });
        p.Rows.Add(new LedgerRow { AccountNo = "34311", ClosingCredit = 10 });
        Assert.Equal(2, WorkbookPlan.Build(p, false).Single().Rows.Count);
    }
    [Fact]
    public void DuplicateAccountsAreRejectedInsteadOfDoubleCounted()
    {
        var p = new VerificationPackage();
        p.Rows.Add(new LedgerRow { AccountNo = "22110", ClosingDebit = 1 });
        p.Rows.Add(new LedgerRow { AccountNo = "22110", ClosingDebit = 2 });
        Assert.Throws<InvalidOperationException>(() => WorkbookPlan.Build(p, false));
    }
    [Fact]
    public async Task ClientPreservesVirtualDirectoryAndSendsKey()
    {
        var handler = new FakeHandler();
        using var http = new HttpClient(handler);
        var client = new MkSoftClient(http, "https://example.test/audit", "test-key");
        await client.GetAsync(11, 2025, false, CancellationToken.None);
        Assert.Equal("https://example.test/audit/api/v1/mksoft/clients/11/years/2025/general-ledger?includeEmptyAccounts=false", handler.Url);
        Assert.Equal("test-key", handler.Key);
    }
    [Fact]
    public void LedgerFileUsesAnnualMovementsAndPreservesCorrections()
    {
        var file = System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", "HL_KNIHA_00325791_202412.xls");
        var package = AccountBalanceVerification.Import.FileSource.Read("Urbis", null, file, "00325791", 2024);
        Assert.Equal(4, package.Rows.Count);
        Assert.Equal(330m, package.Rows.Sum(x => x.MovementsDebit));
        Assert.Equal(205m, package.Rows.Sum(x => x.MovementsCredit));
        Assert.Equal(-30m, package.Rows.Single(x => x.AccountNo == "019").MovementsDebit);
        Assert.Contains(package.Rows, x => x.AccountNo == "431/B");
        Assert.NotEmpty(WorkbookPlan.Build(package, false));
    }
    private sealed class FakeHandler : HttpMessageHandler
    {
        public string Url; public string Key;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Url = request.RequestUri.ToString(); Key = request.Headers.GetValues("X-Api-Key").Single();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"rows\":[],\"officialAccounts\":[]}") });
        }
    }
}
