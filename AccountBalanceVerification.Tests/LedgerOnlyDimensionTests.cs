using System;
using System.IO;
using System.Linq;
using AccountBalanceVerification.Core;
using AccountBalanceVerification.Import;
using Xunit;

namespace AccountBalanceVerification.Tests;
public class LedgerOnlyDimensionTests
{
    [Fact]
    public void IfoSoftLedgerOnlySumsBudgetDimensionsBeforeCreatingAccountSheets()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".CSV");
        try {
            File.WriteAllText(path,
                ";;\"00323110 TEST\";\n;;\"Hlavna kniha k 12/2025\";\n" +
                "Syn;Ana;Typ;P;Odd;Polozka;KZdroja;Program;Stred;Zakaz;Nazov uctu;Poc_M;Poc_D;Roc_M;Roc_D;12/2025;12/2025;Kon_M;Kon_D;Plan\n" +
                "211;10;M;N;;;;;;;Cash;100;;50;;;;150;;\n" +
                "211;10;M;N;0111;632003;41;;;;Cash;;;;20;;;-20;;\n" +
                "321;01;D;N;;;;;;;Supplier;;30;;10;;;;40;\n");
            var p = FileSource.Read("IfoSoft", null, path, "00323110", 2025);
            Assert.Equal(2, p.Rows.Count);
            var cash = p.Rows.Single(x => x.AccountNo == "21110");
            Assert.Equal(100m, cash.OpeningBalance);
            Assert.Equal(50m, cash.MovementsDebit);
            Assert.Equal(20m, cash.MovementsCredit);
            Assert.Equal(130m, cash.ClosingBalance);
            Assert.Equal(-40m, p.Rows.Single(x => x.AccountNo == "32101").ClosingBalance);
            Assert.Equal(2, WorkbookPlan.Build(p, false).Count);
        } finally { File.Delete(path); }
    }
}
