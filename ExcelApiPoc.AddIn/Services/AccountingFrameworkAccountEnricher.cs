using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountingFrameworkAccountEnricher
    {
        public static AccountingFrameworkAccountEnrichmentResult Enrich(
            IReadOnlyList<AccountSummary> accounts,
            AccountingFrameworkImport accountingFramework)
        {
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));
            if (accountingFramework == null) throw new ArgumentNullException(nameof(accountingFramework));

            var rowsByCode = new Dictionary<string, List<AccountingFrameworkRow>>(StringComparer.Ordinal);
            foreach (AccountingFrameworkRow row in accountingFramework.Rows)
            {
                if (row.RowKind != AccountingFrameworkRowKind.SyntheticAccount &&
                    row.RowKind != AccountingFrameworkRowKind.AnalyticalAccount)
                    continue;
                if (!rowsByCode.TryGetValue(row.AccountCode, out List<AccountingFrameworkRow> rows))
                {
                    rows = new List<AccountingFrameworkRow>();
                    rowsByCode.Add(row.AccountCode, rows);
                }
                rows.Add(row);
            }

            var result = new AccountingFrameworkAccountEnrichmentResult();
            foreach (KeyValuePair<string, List<AccountingFrameworkRow>> pair in rowsByCode)
            {
                if (pair.Value.Count <= 1) continue;
                result.DuplicateNormalizedAccountCount++;
                string firstName = pair.Value[0].AccountName;
                foreach (AccountingFrameworkRow row in pair.Value)
                    if (!string.Equals(firstName, row.AccountName, StringComparison.Ordinal))
                    {
                        result.ConflictingDuplicateAccountCount++;
                        break;
                    }
            }

            foreach (AccountSummary account in accounts)
            {
                account.EntityAccountName = string.Empty;
                account.AccountName = account.FrameworkAccountName ?? string.Empty;
                account.AccountNameSource = string.IsNullOrWhiteSpace(account.AccountName)
                    ? "Synthetic"
                    : "StatutoryFramework";

                if (!rowsByCode.TryGetValue(account.AccountCode, out List<AccountingFrameworkRow> matches))
                {
                    result.UnmatchedAccountCount++;
                    continue;
                }

                AccountingFrameworkRow selected = matches[0];
                if (selected.RowKind == AccountingFrameworkRowKind.AnalyticalAccount &&
                    !string.IsNullOrWhiteSpace(selected.AccountName))
                {
                    account.EntityAccountName = selected.AccountName;
                    account.AccountName = selected.AccountName;
                    account.AccountNameSource = "AccountingFramework";
                }
                result.MatchedAccountCount++;
            }
            return result;
        }
    }

    internal sealed class AccountingFrameworkAccountEnrichmentResult
    {
        public int MatchedAccountCount { get; set; }
        public int UnmatchedAccountCount { get; set; }
        public int DuplicateNormalizedAccountCount { get; set; }
        public int ConflictingDuplicateAccountCount { get; set; }
    }
}
