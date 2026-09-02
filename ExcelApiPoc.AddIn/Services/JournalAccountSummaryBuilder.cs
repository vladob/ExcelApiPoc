using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class JournalAccountSummaryBuilder
    {
        public static List<AccountSummary> Build(JournalImport journalImport)
        {
            if (journalImport == null)
                throw new ArgumentNullException(nameof(journalImport));

            var accounts = new Dictionary<string, AccountSummary>(StringComparer.Ordinal);

            foreach (JournalRow row in journalImport.Rows)
            {
                if (!row.UsedForReportCalculation)
                    continue;

                AddDebit(accounts, row.DebitAccount, row.DebitAmount);
                AddCredit(accounts, row.CreditAccount, row.CreditAmount);
            }

            return accounts.Values.OrderBy(account => account.AccountCode, StringComparer.Ordinal).ToList();
        }

        private static void AddDebit(IDictionary<string, AccountSummary> accounts, string accountCode, decimal? amount)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
                return;

            AccountSummary account = GetOrCreate(accounts, accountCode);

            account.DebitEntryCount++;
            account.DebitTurnover += amount ?? 0;
        }

        private static void AddCredit(IDictionary<string, AccountSummary> accounts,string accountCode,decimal? amount)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
                return;

            AccountSummary account = GetOrCreate(accounts, accountCode);

            account.CreditEntryCount++;
            account.CreditTurnover += amount ?? 0;
        }

        private static AccountSummary GetOrCreate(IDictionary<string, AccountSummary> accounts,string accountCode)
        {
            string normalizedCode = AccountCodeNormalizer.Normalize(accountCode);

            if (accounts.TryGetValue(normalizedCode, out AccountSummary account))
            {
                return account;
            }

            account = new AccountSummary
            {
                AccountCode = normalizedCode,
                AccountName = string.Empty,
                AccountNameSource = "Synthetic",
                EntityAccountName = string.Empty,
                SyntheticAccountCode = GetSyntheticAccountCode(normalizedCode),
                FrameworkAccountCode = string.Empty,
                FrameworkAccountName = string.Empty,
                IsFrameworkMatch = null
            };
            accounts.Add(normalizedCode, account);
            return account;
        }

        private static string GetSyntheticAccountCode(string accountCode)
        {
            if (accountCode.Length < 3)
                return string.Empty;

            if (!char.IsDigit(accountCode[0]) || !char.IsDigit(accountCode[1]) || !char.IsDigit(accountCode[2]))
            {
                return string.Empty;
            }
            return accountCode.Substring(0, 3);
        }
    }
}
