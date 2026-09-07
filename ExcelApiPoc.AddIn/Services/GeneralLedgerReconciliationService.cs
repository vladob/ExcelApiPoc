using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class GeneralLedgerReconciliationService
    {
        public static GeneralLedgerReconciliationResult Reconcile(JournalImport journal, GeneralLedgerImport ledger, IList<AccountSummary> accounts)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            if (accounts == null)
            {
                throw new ArgumentNullException(nameof(accounts));
            }

            JournalLedgerReconciliationResult reconciliation = ExcelApiPoc.AccountingImport.Services.JournalLedgerReconciliationService.Reconcile(journal, ledger);
            return CanonicalReconciliationAccountSummaryAdapter.Apply(reconciliation,accounts);
        }
        public static void ResolveNames(IReadOnlyList<AccountSummary> accounts, GeneralLedgerImport ledger)
        {
            var analyticalCodes = new HashSet<string>(
                ledger.Rows.Where(x => !string.IsNullOrWhiteSpace(x.AnalyticalCode)).Select(x => x.AccountCode), StringComparer.Ordinal);
            foreach (AccountSummary account in accounts)
            {
                string preferredReferenceName = !string.IsNullOrWhiteSpace(account.EntityAccountName)
                    ? account.EntityAccountName
                    : account.FrameworkAccountName;
                account.AccountNameComparisonStatus = CompareNames(preferredReferenceName, account.GeneralLedgerAccountName);
                if (analyticalCodes.Contains(account.AccountCode) &&
                    string.IsNullOrWhiteSpace(account.EntityAccountName) &&
                    !string.IsNullOrWhiteSpace(account.GeneralLedgerAccountName))
                {
                    account.AccountName = account.GeneralLedgerAccountName;
                    account.AccountNameSource = "GeneralLedger";
                }
            }
        }

        private static string CompareNames(string frameworkName, string ledgerName)
        {
            string a = NormalizeName(frameworkName); string b = NormalizeName(ledgerName);
            if (a.Length == 0 && b.Length == 0) return "MissingBoth";
            if (a.Length == 0) return "FrameworkMissing";
            if (b.Length == 0) return "LedgerMissing";
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return "Match";
            if (a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase)) return "Truncated";
            return "Different";
        }

        private static string NormalizeName(string value) => Regex.Replace((value ?? string.Empty).Trim(), @"\s+", " ");
    }
}
