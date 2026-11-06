using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AccountBalanceVerification.Core;
using AccountBalanceVerification.Import;
using ExcelApiPoc.MkSoft.Client;
using Xunit;
namespace AccountBalanceVerification.Tests;
public class OutputWorkflowTests
{
    [Theory]
    [InlineData(true, "Title,All")]
    [InlineData(false, "Account,All")]
    public void SignaturesRespectPlacementAndIgnoreEmptyNames(bool title, string expected)
    {
        var rows = new[] {
            new Signatory { Role = "Predseda", Name = "Title", Placement = PrintPlan.FirstPage },
            new Signatory { Role = "Člen", Name = "Account", Placement = PrintPlan.Accounts },
            new Signatory { Role = "Člen", Name = "All", Placement = PrintPlan.All },
            new Signatory { Name = "" }
        };
        Assert.Equal(expected, string.Join(",", PrintPlan.Select(rows, title).Select(x => x.Name)));
    }
    [Fact]
    public void PastedInvalidPlacementIsNotSilentlyOmitted()
    {
        Assert.Throws<InvalidOperationException>(() => PrintPlan.Select(new[] { new Signatory { Role = "Člen", Name = "Name", Placement = "Invalid" } }, true));
    }
    [Fact]
    public void AmbiguousJournalRequestsSystemAndKeepsFilenameIdentity()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(folder);
        try {
            string path = Path.Combine(folder, "U_DENNIK_00332780_202512.CSV");
            File.WriteAllText(path, "00332780 TEST\nUctovny dennik\nDD;CisloD;Datum;Popis operacie;Ucet_MD;Pol_MD;Zdr_MD;Str_MD;Zak_MD;Suma_MD;Ucet_D;Odd_D;Pol_D;Zdr_D;Str_D;Zak_D;Suma_D\nPS;0;01.01.2025;Opening;211;;;;;100;701;;;;;;100\n");
            var metadata = FileMetadata.Read(path, null, null, null);
            Assert.Contains(metadata.Warnings, x => x.Contains("Vyberte účtovný systém"));
            Assert.Equal("00332780", metadata.Ico); Assert.Equal(2025, metadata.FiscalYear);
            var selected = FileMetadata.Read(path, null, null, "IfoSoft");
            Assert.Equal("TEST", selected.EntityName);
            Assert.DoesNotContain(selected.Warnings, x => x.Contains("Vyberte účtovný systém"));
        } finally { Directory.Delete(folder, true); }
    }
    [Fact]
    public void ExplicitAfHeadingOverridesGlNameWithoutAddingBalances()
    {
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(folder);
        try {
            var path = Path.Combine(folder, "UCT_ROZVRH_00332780_2025.CSV");
            File.WriteAllText(path, "00332780 TEST\nUctovny rozvrh 2025\nSyn;Ana;Typ;Druh;Dan;Nazov uctu;Saldo;DPH;Pods;Nazov uctu angl;Nazov uctu nem.\n211;****;M;;;AF heading;;;;;\n211;001;M;;;AF detail;;;;;\n");
            var p = new VerificationPackage();
            p.OfficialAccounts.Add(new OfficialAccount { AccountCode = "211", AccountName = "GL heading", AccountLevel = 3 });
            p.Rows.Add(new LedgerRow { AccountNo = "211001", AccountName = "GL detail", ClosingDebit = 12m });
            FileSource.ApplyChart(p, "IfoSoft", path, "00332780", 2025);
            Assert.Equal("AF heading", p.OfficialAccounts.Single().AccountName);
            Assert.Equal("AF detail", p.Rows.Single().AccountName);
            Assert.Equal(12m, p.Rows.Single().ClosingBalance);
        } finally { Directory.Delete(folder, true); }
    }
    [Fact]
    public void AnalyticalNameIsNeverUsedAsSyntheticHeading()
    {
        var p = new VerificationPackage(); p.Rows.Add(new LedgerRow { AccountNo = "211001", AccountName = "Cash location one", ClosingDebit = 1 });
        Assert.Equal("Názov syntetického účtu nie je dostupný", WorkbookPlan.Build(p, false).Single().Name);
    }
    [Fact]
    public async Task PosnovaClientPreservesVirtualDirectoryAndAccountZeros()
    {
        using var http = new HttpClient(new PosnovaHandler());
        var accounts = await new MkSoftClient(http, "https://example.test/audit", "key").GetAccountingFrameworkAsync(11, 2025, CancellationToken.None);
        Assert.Equal("021", accounts.Single().AccountCode);
    }
    private sealed class PosnovaHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("https://example.test/audit/api/v1/mksoft/clients/11/years/2025/accounting-framework", request.RequestUri.ToString());
            Assert.Equal("key", request.Headers.GetValues("X-Api-Key").Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[{\"accountCode\":\"021\",\"accountName\":\"Buildings\",\"accountLevel\":3}]") });
        }
    }
}
