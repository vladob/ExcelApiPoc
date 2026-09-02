using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class GeneralLedgerReconciliationService
    {
        private const decimal Tolerance = 0.01m;

        public static GeneralLedgerReconciliationResult Reconcile(
            JournalImport journal,
            GeneralLedgerImport ledger,
            IList<AccountSummary> accounts)
        {
            if (journal == null) throw new ArgumentNullException(nameof(journal));
            if (ledger == null) throw new ArgumentNullException(nameof(ledger));
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));

            Dictionary<string, JournalLedgerAggregate> journalByAccount = BuildJournal(journal);
            Dictionary<string, LedgerAggregate> ledgerByAccount = BuildLedger(ledger);
            var accountsByCode = accounts.ToDictionary(x => x.AccountCode, StringComparer.Ordinal);
            foreach (string code in journalByAccount.Keys.Concat(ledgerByAccount.Keys).Distinct(StringComparer.Ordinal))
                if (!accountsByCode.ContainsKey(code))
                {
                    var account = CreateAccount(code);
                    accounts.Add(account);
                    accountsByCode.Add(code, account);
                }

            var result = new GeneralLedgerReconciliationResult
            {
                JournalAccountCount = journalByAccount.Count,
                LedgerAccountCount = ledgerByAccount.Count,
                JournalOnlyAccountCount = journalByAccount.Keys.Except(ledgerByAccount.Keys, StringComparer.Ordinal).Count(),
                LedgerOnlyAccountCount = ledgerByAccount.Keys.Except(journalByAccount.Keys, StringComparer.Ordinal).Count(),
                MatchedAccountCount = journalByAccount.Keys.Intersect(ledgerByAccount.Keys, StringComparer.Ordinal).Count()
            };

            foreach (AccountSummary account in accounts)
            {
                bool hasJournal = journalByAccount.TryGetValue(account.AccountCode, out JournalLedgerAggregate j);
                bool hasLedger = ledgerByAccount.TryGetValue(account.AccountCode, out LedgerAggregate l);
                if (hasJournal)
                {
                    account.JournalLedgerOpeningDebit = j.OpeningDebit;
                    account.JournalLedgerOpeningCredit = j.OpeningCredit;
                    account.JournalLedgerDebitTurnover = j.DebitTurnover;
                    account.JournalLedgerCreditTurnover = j.CreditTurnover;
                }
                if (hasLedger)
                {
                    account.GeneralLedgerAccountName = l.AccountName;
                    account.LedgerOpeningDebit = l.OpeningDebit;
                    account.LedgerOpeningCredit = l.OpeningCredit;
                    account.LedgerDebitTurnover = l.DebitTurnover;
                    account.LedgerCreditTurnover = l.CreditTurnover;
                    account.LedgerClosingDebit = l.ClosingDebit;
                    account.LedgerClosingCredit = l.ClosingCredit;
                }

                if (!hasJournal) account.LedgerReconciliationStatus = "LedgerOnly";
                else if (!hasLedger) account.LedgerReconciliationStatus = "JournalOnly";
                else if (IsZero(account.OpeningDebitDifference) && IsZero(account.OpeningCreditDifference) &&
                         IsZero(account.DebitTurnoverDifference) && IsZero(account.CreditTurnoverDifference) &&
                         IsZero(account.ClosingBalanceDifference))
                {
                    account.LedgerReconciliationStatus = "Reconciled";
                    result.ReconciledAccountCount++;
                }
                else
                {
                    account.LedgerReconciliationStatus = "Different";
                    result.DifferentAccountCount++;
                }
            }

            result.OpeningDebitDifference = accounts.Sum(x => x.OpeningDebitDifference);
            result.OpeningCreditDifference = accounts.Sum(x => x.OpeningCreditDifference);
            result.DebitTurnoverDifference = accounts.Sum(x => x.DebitTurnoverDifference);
            result.CreditTurnoverDifference = accounts.Sum(x => x.CreditTurnoverDifference);
            result.ClosingBalanceDifference = accounts.Sum(x => x.ClosingBalanceDifference);

            List<AccountSummary> sorted = accounts.OrderBy(x => x.AccountCode, StringComparer.Ordinal).ToList();
            accounts.Clear();
            foreach (AccountSummary account in sorted) accounts.Add(account);
            return result;
        }

        public static void ResolveNames(IReadOnlyList<AccountSummary> accounts, GeneralLedgerImport ledger)
        {
            var analyticalCodes = new HashSet<string>(
                ledger.Rows.Where(x => !string.IsNullOrWhiteSpace(x.AnalyticalCode)).Select(x => x.AccountCode),
                StringComparer.Ordinal);
            foreach (AccountSummary account in accounts)
            {
                string preferredReferenceName = !string.IsNullOrWhiteSpace(account.EntityAccountName)
                    ? account.EntityAccountName
                    : account.FrameworkAccountName;
                account.AccountNameComparisonStatus = CompareNames(
                    preferredReferenceName, account.GeneralLedgerAccountName);
                if (analyticalCodes.Contains(account.AccountCode) &&
                    string.IsNullOrWhiteSpace(account.EntityAccountName) &&
                    !string.IsNullOrWhiteSpace(account.GeneralLedgerAccountName))
                {
                    account.AccountName = account.GeneralLedgerAccountName;
                    account.AccountNameSource = "GeneralLedger";
                }
            }
        }

        private static Dictionary<string, JournalLedgerAggregate> BuildJournal(JournalImport journal)
        {
            var result = new Dictionary<string, JournalLedgerAggregate>(StringComparer.Ordinal);
            foreach (JournalRow row in journal.Rows)
            {
                if (row.RecordKind == JournalRecordKind.Closing) continue;
                AddJournal(result, row.DebitAccount, row.DebitAmount ?? 0m, true, row.RecordKind);
                AddJournal(result, row.CreditAccount, row.CreditAmount ?? 0m, false, row.RecordKind);
            }
            return result;
        }

        private static void AddJournal(Dictionary<string, JournalLedgerAggregate> result, string code, decimal amount, bool debit, JournalRecordKind kind)
        {
            code = AccountCodeNormalizer.Normalize(code);
            if (code.Length == 0) return;
            if (!result.TryGetValue(code, out JournalLedgerAggregate x)) { x = new JournalLedgerAggregate(); result.Add(code, x); }
            if (kind == JournalRecordKind.Opening) { if (debit) x.OpeningDebit += amount; else x.OpeningCredit += amount; }
            else { if (debit) x.DebitTurnover += amount; else x.CreditTurnover += amount; }
        }

        private static Dictionary<string, LedgerAggregate> BuildLedger(GeneralLedgerImport ledger)
        {
            var result = new Dictionary<string, LedgerAggregate>(StringComparer.Ordinal);
            foreach (GeneralLedgerRow row in ledger.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.AccountCode)) continue;
                if (!result.TryGetValue(row.AccountCode, out LedgerAggregate x)) { x = new LedgerAggregate(); result.Add(row.AccountCode, x); }
                if (string.IsNullOrWhiteSpace(x.AccountName) && !string.IsNullOrWhiteSpace(row.AccountName)) x.AccountName = row.AccountName;
                x.OpeningDebit += row.OpeningDebit; x.OpeningCredit += row.OpeningCredit;
                x.DebitTurnover += row.AnnualDebitTurnover; x.CreditTurnover += row.AnnualCreditTurnover;
                x.ClosingDebit += row.ClosingDebit; x.ClosingCredit += row.ClosingCredit;
            }
            return result;
        }

        private static AccountSummary CreateAccount(string code)
        {
            return new AccountSummary
            {
                AccountCode = code, AccountName = string.Empty, AccountNameSource = "Synthetic",
                EntityAccountName = string.Empty,
                SyntheticAccountCode = code.Length >= 3 ? code.Substring(0, 3) : string.Empty,
                FrameworkAccountCode = string.Empty, FrameworkAccountName = string.Empty
            };
        }

        private static bool IsZero(decimal value) => Math.Abs(value) <= Tolerance;

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

        private static string NormalizeName(string value) =>
            Regex.Replace((value ?? string.Empty).Trim(), @"\s+", " ");

        private sealed class JournalLedgerAggregate
        {
            public decimal OpeningDebit, OpeningCredit, DebitTurnover, CreditTurnover;
        }
        private sealed class LedgerAggregate
        {
            public string AccountName;
            public decimal OpeningDebit, OpeningCredit, DebitTurnover, CreditTurnover, ClosingDebit, ClosingCredit;
        }
    }
}
