using System;
using System.Collections.Generic;
using System.Linq;
namespace AccountBalanceVerification.Core
{
    public sealed class LedgerRow
    {
        public string AccountNo { get; set; } = "";
        public string AccountName { get; set; } = "";
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal MovementsDebit { get; set; }
        public decimal MovementsCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
        public decimal OpeningBalance => OpeningDebit - OpeningCredit;
        public decimal ClosingBalance => ClosingDebit - ClosingCredit;
        public bool HasValues => OpeningBalance != 0 || MovementsDebit != 0 || MovementsCredit != 0 || ClosingBalance != 0;
    }
    public sealed class OfficialAccount
    {
        public string AccountCode { get; set; } = "";
        public string AccountName { get; set; } = "";
        public int AccountLevel { get; set; }
        public string ParentAccountCode { get; set; }
    }
    public sealed class VerificationPackage
    {
        public int ClientErpId { get; set; }
        public int FiscalYear { get; set; }
        public int LegalForm { get; set; }
        public int AccountFrameworkVersionId { get; set; }
        public string EntityName { get; set; } = "";
        public string Ico { get; set; } = "";
        public string Source { get; set; } = "MkSoftRaw";
        public DateTime RetrievedAtUtc { get; set; }
        public List<OfficialAccount> OfficialAccounts { get; set; } = new List<OfficialAccount>();
        public List<LedgerRow> Rows { get; set; } = new List<LedgerRow>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
    public sealed class AccountSheet
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<LedgerRow> Rows { get; set; }
    }
    public static class WorkbookPlan
    {
        public static bool InScope(string code) => !string.IsNullOrEmpty(code) && code[0] >= '0' && code[0] <= '4';
        public static List<AccountSheet> Build(VerificationPackage package, bool includeEmptySyntheticAccounts)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            var rows = package.Rows.Where(x => InScope(x.AccountNo) && x.HasValues).ToList();
            if (rows.Any(x => x.AccountNo.Length < 3 || !x.AccountNo.Take(3).All(char.IsDigit)))
                throw new InvalidOperationException("Invalid synthetic account prefix.");
            if (rows.GroupBy(x => x.AccountNo).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Duplicate ledger accounts must be resolved before workbook creation.");
            var definitions = package.OfficialAccounts.Where(x => x.AccountLevel == 3 && InScope(x.AccountCode)).ToDictionary(x => x.AccountCode);
            var codes = new SortedSet<string>(rows.Select(x => x.AccountNo.Substring(0, 3)), StringComparer.Ordinal);
            if (includeEmptySyntheticAccounts) codes.UnionWith(definitions.Keys);
            return codes.Select(code => new AccountSheet {
                Code = code,
                Name = definitions.TryGetValue(code, out var definition) ? definition.AccountName : "Názov syntetického účtu nie je dostupný",
                Rows = rows.Where(x => x.AccountNo.StartsWith(code, StringComparison.Ordinal)).OrderBy(x => x.AccountNo, StringComparer.Ordinal).ToList()
            }).ToList();
        }
    }
}
