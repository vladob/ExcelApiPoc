using System.Linq;
using AccountBalanceVerification.Core;
using Xunit;

namespace AccountBalanceVerification.Tests;
public class RecapPlanTests
{
    [Theory]
    [InlineData("00323110", 2026)]
    [InlineData("00323111", 2025)]
    public void MunicipalitySpecificMappingsAreNotReusedForOtherEntitiesOrYears(string ico, int year)
    {
        var items = RecapPlan.Build(new VerificationPackage { Ico = ico, FiscalYear = year }).SelectMany(x => x.Items).ToList();
        Assert.Equal(new[] { "" }, items.Single(x => x.Key == "752").Patterns);
        Assert.Equal(new[] { "" }, items.Single(x => x.Key == "221B").Patterns);
        Assert.Equal(new[] { "08*" }, items.Single(x => x.Key == "08").Patterns);
    }
    [Fact]
    public void MissingIncomeOrExpenseClassLeavesResultUnresolved()
    {
        var p = new VerificationPackage();
        p.Rows.Add(new LedgerRow { AccountNo = "501", ClosingDebit = 40 });
        Assert.Equal(new[] { "" }, RecapPlan.Build(p).SelectMany(x => x.Items).Single(x => x.Key == "RESULT").Patterns);
        p.Rows.Add(new LedgerRow { AccountNo = "601", ClosingCredit = 50 });
        Assert.Equal(new[] { "5*", "6*" }, RecapPlan.Build(p).SelectMany(x => x.Items).Single(x => x.Key == "RESULT").Patterns);
        Assert.Empty(WorkbookPlan.Build(p, false)); // Report inputs do not create class 5/6 account tabs.
    }
    [Fact]
    public void OffBalanceSplitAccountAndCreditNormalAssetsAreHandledExplicitly()
    {
        var items = RecapPlan.Build(new VerificationPackage { Ico = "00323110", FiscalYear = 2025 }).SelectMany(x => x.Items).ToList();
        Assert.Contains(RecapPlan.NormalizeAccount("75 2025"), items.Single(x => x.Key == "752").Patterns);
        Assert.Equal(-1, items.Single(x => x.Key == "08").Sign);
        Assert.Equal("Debit", items.Single(x => x.Key == "357D").BalanceSide);
        Assert.Equal("Credit", items.Single(x => x.Key == "357C").BalanceSide);
        Assert.Equal(items.Count, items.Select(x => x.Key).Distinct().Count());
    }
}
