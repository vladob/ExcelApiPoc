using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class CanonicalReconciliationAccountSummaryAdapter
    {
        public static GeneralLedgerReconciliationResult Apply(JournalLedgerReconciliationResult reconciliation, IList<AccountSummary> accounts)
        {
            if (reconciliation == null)
            {
                throw new ArgumentNullException(nameof(reconciliation));
            }

            if (accounts == null)
            {
                throw new ArgumentNullException(nameof(accounts));
            }

            Dictionary<string, AccountSummary> accountsByCode = accounts.ToDictionary( account => account.AccountCode, StringComparer.Ordinal);

            foreach (JournalLedgerAccountReconciliation source in reconciliation.Accounts)
            {
                if (!accountsByCode.TryGetValue(source.AccountCode, out AccountSummary target))
                {
                    target = CreateAccount(source.AccountCode);
                    accounts.Add(target);
                    accountsByCode.Add(source.AccountCode, target);
                }

                ApplyAccount(source, target);
            }

            List<AccountSummary> sorted = accounts.OrderBy(account => account.AccountCode, StringComparer.Ordinal).ToList();

            accounts.Clear();

            foreach (AccountSummary account in sorted)
            {
                accounts.Add(account);
            }

            return CreateLegacyResult(reconciliation, accounts);
        }

        private static void ApplyAccount(JournalLedgerAccountReconciliation source, AccountSummary target)
        {
            target.GeneralLedgerAccountName =
                source.GeneralLedgerAccountName;

            /*
             * DebitTurnover and CreditTurnover are the existing
             * AccountSummary values consumed by financial-report
             * calculation.
             *
             * They must include the effective opening balance.
             * That opening comes from AJ when AJ contains opening
             * records, otherwise from GL.
             */
            target.DebitTurnover = source.EffectiveOpeningDebit + source.JournalDebitTurnover;
            target.CreditTurnover = source.EffectiveOpeningCredit + source.JournalCreditTurnover;

            /*
             * These columns retain the existing workbook schema.
             * For an AJ without opening records, the effective
             * GL-supplied opening is shown here so that the
             * difference columns remain meaningful.
             */
            target.JournalLedgerOpeningDebit = source.EffectiveOpeningDebit;
            target.JournalLedgerOpeningCredit = source.EffectiveOpeningCredit;
            target.JournalLedgerDebitTurnover = source.JournalDebitTurnover;
            target.JournalLedgerCreditTurnover = source.JournalCreditTurnover;
            target.LedgerOpeningDebit = source.LedgerOpeningDebit;
            target.LedgerOpeningCredit = source.LedgerOpeningCredit;
            target.LedgerDebitTurnover = source.LedgerDebitTurnover;
            target.LedgerCreditTurnover = source.LedgerCreditTurnover;
            target.LedgerClosingDebit = source.LedgerClosingDebit;
            target.LedgerClosingCredit = source.LedgerClosingCredit;
            target.LedgerReconciliationStatus = GetStatus(source);
        }

        private static string GetStatus(JournalLedgerAccountReconciliation account)
        {
            if (account.IsReconciled)
            {
                return "Reconciled";
            }

            if (!account.HasGeneralLedgerAccount)
            {
                return "JournalOnly";
            }

            if (!account.HasJournalActivity)
            {
                return "LedgerOnly";
            }

            return "Different";
        }

        private static GeneralLedgerReconciliationResult CreateLegacyResult(JournalLedgerReconciliationResult source, IEnumerable<AccountSummary> accounts)
        {
            List<AccountSummary> materialized = accounts.ToList();

            return new GeneralLedgerReconciliationResult
            {
                JournalAccountCount = source.JournalAccountCount,
                LedgerAccountCount = source.LedgerAccountCount,
                MatchedAccountCount = source.Accounts.Count(account => account.HasJournalActivity && account.HasGeneralLedgerAccount),
                JournalOnlyAccountCount = materialized.Count(account => account.LedgerReconciliationStatus == "JournalOnly"),
                LedgerOnlyAccountCount = materialized.Count(account => account.LedgerReconciliationStatus == "LedgerOnly"),
                ReconciledAccountCount = materialized.Count(account => account.LedgerReconciliationStatus == "Reconciled"),
                DifferentAccountCount = materialized.Count(account => account.LedgerReconciliationStatus == "Different"),
                OpeningDebitDifference = source.OpeningDebitDifference,
                OpeningCreditDifference = source.OpeningCreditDifference,
                DebitTurnoverDifference = source.DebitTurnoverDifference,
                CreditTurnoverDifference = source.CreditTurnoverDifference,
                ClosingBalanceDifference = source.ClosingBalanceDifference
            };
        }

        private static AccountSummary CreateAccount(string accountCode)
        {
            return new AccountSummary
            {
                AccountCode = accountCode,
                AccountName = string.Empty,
                AccountNameSource = "Synthetic",
                EntityAccountName = string.Empty,
                SyntheticAccountCode = GetSyntheticAccountCode(accountCode),
                FrameworkAccountCode = string.Empty,
                FrameworkAccountName = string.Empty,
                IsFrameworkMatch = null
            };
        }

        private static string GetSyntheticAccountCode(string accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode) || accountCode.Length < 3)
            {
                return string.Empty;
            }

            if (!char.IsDigit(accountCode[0]) || !char.IsDigit(accountCode[1]) || !char.IsDigit(accountCode[2]))
            {
                return string.Empty;
            }

            return accountCode.Substring(0, 3);
        }
    }
}