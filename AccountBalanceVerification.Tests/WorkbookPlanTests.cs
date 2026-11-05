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
    [Theory]
    [InlineData(320, false)]
    [InlineData(321, true)]
    public void JournalOnlyAccountWithCancelledCreditDoesNotBlockInventory(int correction, bool shouldFail)
    {
        var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            var journal = System.IO.Path.Combine(folder, "U_DENNIK_12345678_202512.CSV");
            var ledger = System.IO.Path.Combine(folder, "HL_KNIHA_12345678_202512.CSV");
            System.IO.File.WriteAllText(journal,
                "12345678 TEST\nUctovny dennik\n" +
                "DD;CisloD;Datum;Popis operacie;Ucet_MD;Pol_MD;Zdr_MD;Str_MD;Zak_MD;Suma_MD;Ucet_D;Odd_D;Pol_D;Zdr_D;Str_D;Zak_D;Suma_D\n" +
                "PS;0;01.01.2025;Opening;211;;;;;100;701;;;;;;100\n" +
                "ZI;1;30.06.2025;Correction;;;;;;0;082;;;;;;-320\n" +
                "OP;2;31.10.2025;Correction;;;;;;0;082;;;;;;" + correction + "\n",
                new System.Text.UTF8Encoding(true));
            System.IO.File.WriteAllText(ledger,
                ";;12345678 TEST;\n;;Hlavna kniha k 12/2025;\n" +
                "Syn;Ana;Typ;P;Odd;Polozka;KZdroja;Program;Stred;Zakaz;Nazov uctu;Poc_M;Poc_D;Roc_M;Roc_D;12/2025;12/2025;Kon_M;Kon_D;Plan\n" +
                "211;;M;N;;;;;;;Cash;100;;;;;;100;;\n" +
                "701;;D;N;;;;;;;Opening;;100;;;;;;100;\n",
                new System.Text.UTF8Encoding(true));
            if (shouldFail)
            {
                var error = Assert.Throws<InvalidOperationException>(() =>
                    AccountBalanceVerification.Import.FileSource.Read("IfoSoft", journal, ledger, "12345678", 2025));
                Assert.Contains("082", error.Message);
            }
            else
            {
                var package = AccountBalanceVerification.Import.FileSource.Read("IfoSoft", journal, ledger, "12345678", 2025);
                Assert.Equal(100m, package.Rows.Single(x => x.AccountNo == "211").ClosingBalance);
                Assert.DoesNotContain(WorkbookPlan.Build(package, false), x => x.Code == "082");
            }
        }
        finally { System.IO.Directory.Delete(folder, true); }
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
