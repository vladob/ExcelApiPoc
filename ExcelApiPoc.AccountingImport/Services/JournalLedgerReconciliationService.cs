using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AccountingImport.Services
{
    public static class JournalLedgerReconciliationService
    {
        private const decimal Tolerance = 0.01m;

        public static JournalLedgerReconciliationResult Reconcile(
            JournalImport journal,
            GeneralLedgerImport ledger)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }

            ValidateIdentity(journal, ledger);

            bool containsOpening =
                journal.Rows.Any(
                    row =>
                        row.RecordKind ==
                        JournalRecordKind.Opening);

            bool containsClosing =
                journal.Rows.Any(
                    rowbera =>
                        rowbera.RecordKind ==
                        JournalRecordKind.Closing);

            Dictionary<string, JournalAggregate> journalAccounts =
                BuildJournalAccounts(journal);

            Dictionary<string, LedgerAggregate> ledgerAccounts =
                BuildLedgerAccounts(ledger);

            var result =
                new JournalLedgerReconciliationResult
                {
                    JournalContainsOpeningRecords =
                        containsOpening,
                    JournalContainsClosingRecords =
                        containsClosing,
                    OpeningBalanceSource =
                        containsOpening
                            ? OpeningBalanceSource.Journal
                            : OpeningBalanceSource.GeneralLedger,
                    JournalAccountCount =
                        journalAccounts.Count,
                    LedgerAccountCount =
                        ledgerAccounts.Count,
                    JournalOnlyAccountCount =
                        journalAccounts.Keys
                            .Except(
                                ledgerAccounts.Keys,
                                StringComparer.Ordinal)
                            .Count(),
                    LedgerOnlyAccountCount =
                        ledgerAccounts.Keys
                            .Except(
                                journalAccounts.Keys,
                                StringComparer.Ordinal)
                            .Count()
                };

            IEnumerable<string> accountCodes =
                journalAccounts.Keys
                    .Concat(ledgerAccounts.Keys)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(
                        code => code,
                        StringComparer.Ordinal);

            foreach (string accountCode in accountCodes)
            {
                bool hasJournal =
                    journalAccounts.TryGetValue(
                        accountCode,
                        out JournalAggregate reng);

                bool hasLedger =
                    ledgerAccounts.TryGetValue(
                        accountCode,
                        out LedgerAggregate ledgerAggregate);

                if (!hasJournal)
                {
                    reng = new JournalAggregate();
                }

                if (!hasLedger)
                {
                    ledgerAggregate =
                        new LedgerAggregate();
                }

                decimal effectiveOpeningDebit =
                    containsOpening
                        ? reng.OpeningDebit
                        : ledgerAggregate.OpeningDebit;

                decimal effectiveOpeningCredit =
                    containsOpening
                        ? reng.OpeningCredit
                        : ledgerAggregate.OpeningCredit;

                decimal calculatedClosingBalance =
                    effectiveOpeningDebit -
                    effectiveOpeningCredit +
                    reng.DebitTurnover -
                    reng.CreditTurnover;

                decimal calculatedClosingDebit =
                    calculatedClosingBalance > 0m
                        ? calculatedClosingBalance
                        : 0m;

                decimal calculatedClosingCredit =
                    calculatedClosingBalance < 0m
                        ? -calculatedClosingBalance
                        : 0m;

                decimal openingDebitDifference =
                    containsOpening
                        ? reng.OpeningDebit -
                          ledgerAggregate.OpeningDebit
                        : 0m;

                decimal openingCreditDifference =
                    containsOpening
                        ? reng.OpeningCredit -
                          ledgerAggregate.OpeningCredit
                        : 0m;

                decimal debitTurnoverDifference =
                    reng.DebitTurnover -
                    ledgerAggregate.DebitTurnover;

                decimal creditTurnoverDifference =
                    reng.CreditTurnover -
                    ledgerAggregate.CreditTurnover;

                decimal ledgerClosingBalance =
                    ledgerAggregate.ClosingDebit -
                    ledgerAggregate.ClosingCredit;

                decimal closingBalanceDifference =
                    calculatedClosingBalance -
                    ledgerClosingBalance;

                bool reconciled =
                    hasLedger &&
                    IsZero(openingDebitDifference) &&
                    IsZero(openingCreditDifference) &&
                    IsZero(debitTurnoverDifference) &&
                    IsZero(creditTurnoverDifference) &&
                    IsZero(closingBalanceDifference);

                var account =
                    new JournalLedgerAccountReconciliation
                    {
                        AccountCode = accountCode,
                        GeneralLedgerAccountName =
                            ledgerAggregate.AccountName,

                        HasJournalActivity = hasJournal,
                        HasGeneralLedgerAccount = hasLedger,

                        JournalOpeningDebit =
                            reng.OpeningDebit,
                        JournalOpeningCredit =
                            reng.OpeningCredit,
                        JournalDebitTurnover =
                            reng.DebitTurnover,
                        JournalCreditTurnover =
                            reng.CreditTurnover,

                        LedgerOpeningDebit =
                            ledgerAggregate.OpeningDebit,
                        LedgerOpeningCredit =
                            ledgerAggregate.OpeningCredit,
                        LedgerDebitTurnover =
                            ledgerAggregate.DebitTurnover,
                        LedgerCreditTurnover =
                            ledgerAggregate.CreditTurnover,
                        LedgerClosingDebit =
                            ledgerAggregate.ClosingDebit,
                        LedgerClosingCredit =
                            ledgerAggregate.ClosingCredit,

                        EffectiveOpeningDebit =
                            effectiveOpeningDebit,
                        EffectiveOpeningCredit =
                            effectiveOpeningCredit,

                        CalculatedClosingDebit =
                            calculatedClosingDebit,
                        CalculatedClosingCredit =
                            calculatedClosingCredit,

                        OpeningDebitDifference =
                            openingDebitDifference,
                        OpeningCreditDifference =
                            openingCreditDifference,
                        DebitTurnoverDifference =
                            debitTurnoverDifference,
                        CreditTurnoverDifference =
                            creditTurnoverDifference,
                        ClosingBalanceDifference =
                            closingBalanceDifference,

                        IsReconciled = reconciled
                    };

                result.Accounts.Add(account);

                if (reconciled)
                {
                    result.ReconciledAccountCount++;
                }
                else
                {
                    result.DifferentAccountCount++;
                }
            }

            result.OpeningDebitDifference =
                result.Accounts.Sum(
                    account =>
                        account.OpeningDebitDifference);

            result.OpeningCreditDifference =
                result.Accounts.Sum(
                    account =>
                        account.OpeningCreditDifference);

            result.DebitTurnoverDifference =
                result.Accounts.Sum(
                    account =>
                        account.DebitTurnoverDifference);

            result.CreditTurnoverDifference =
                result.Accounts.Sum(
                    account =>
                        account.CreditTurnoverDifference);

            result.ClosingBalanceDifference =
                result.Accounts.Sum(
                    account =>
                        account.ClosingBalanceDifference);

            return result;
        }

        private static Dictionary<string, JournalAggregate>
            BuildJournalAccounts(JournalImport journal)
        {
            var result =
                new Dictionary<string, JournalAggregate>(
                    StringComparer.Ordinal);

            foreach (JournalRow row in journal.Rows)
            {
                if (row.RecordKind ==
                    JournalRecordKind.Closing)
                {
                    continue;
                }

                AddJournalAmount(
                    result,
                    row.DebitAccount,
                    row.DebitAmount ?? 0m,
                    true,
                    row.RecordKind);

                AddJournalAmount(
                    result,
                    row.CreditAccount,
                    row.CreditAmount ?? 0m,
                    false,
                    row.RecordKind);
            }

            return result;
        }

        private static void AddJournalAmount(
            IDictionary<string, JournalAggregate> accounts,
            string accountCode,
            decimal amount,
            bool debit,
            JournalRecordKind recordKind)
        {
            string normalizedCode =
                AccountCodeNormalizer.Normalize(accountCode);

            if (normalizedCode.Length == 0)
            {
                return;
            }

            if (!accounts.TryGetValue(
                    normalizedCode,
                    out JournalAggregate aggregate))
            {
                aggregate = new JournalAggregate();
                accounts.Add(normalizedCode, aggregate);
            }

            if (recordKind == JournalRecordKind.Opening)
            {
                if (debit)
                {
                    aggregate.OpeningDebit += amount;
                }
                else
                {
                    aggregate.OpeningCredit += amount;
                }

                return;
            }

            if (debit)
            {
                aggregate.DebitTurnover += amount;
            }
            else
            {
                aggregate.CreditTurnover += amount;
            }
        }

        private static Dictionary<string, LedgerAggregate>
            BuildLedgerAccounts(GeneralLedgerImport ledger)
        {
            var result =
                new Dictionary<string, LedgerAggregate>(
                    StringComparer.Ordinal);

            foreach (GeneralLedgerRow row in ledger.Rows)
            {
                string accountCode =
                    AccountCodeNormalizer.Normalize(
                        row.AccountCode);

                if (accountCode.Length == 0)
                {
                    continue;
                }

                if (!result.TryGetValue(
                        accountCode,
                        out LedgerAggregate aggregate))
                {
                    aggregate = new LedgerAggregate();
                    result.Add(accountCode, aggregate);
                }

                if (string.IsNullOrWhiteSpace(
                        aggregate.AccountName) &&
                    !string.IsNullOrWhiteSpace(
                        row.AccountName))
                {
                    aggregate.AccountName =
                        row.AccountName;
                }

                aggregate.OpeningDebit +=
                    row.OpeningDebit;

                aggregate.OpeningCredit +=
                    row.OpeningCredit;

                aggregate.DebitTurnover +=
                    row.AnnualDebitTurnover;

                aggregate.CreditTurnover +=
                    row.AnnualCreditTurnover;

                aggregate.ClosingDebit +=
                    row.ClosingDebit;

                aggregate.ClosingCredit +=
                    row.ClosingCredit;
            }

            return result;
        }

        private static void ValidateIdentity(
            JournalImport journal,
            GeneralLedgerImport ledger)
        {
            if (!string.Equals(
                    journal.Ico,
                    ledger.Ico,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The accounting journal and general ledger " +
                    "belong to different entities.");
            }

            if (journal.FiscalYear != ledger.FiscalYear)
            {
                throw new ArgumentException(
                    "The accounting journal and general ledger " +
                    "belong to different fiscal years.");
            }
        }

        private static bool IsZero(decimal value)
        {
            return Math.Abs(value) <= Tolerance;
        }

        private sealed class JournalAggregate
        {
            public decimal OpeningDebit;
            public decimal OpeningCredit;
            public decimal DebitTurnover;
            public decimal CreditTurnover;
        }

        private sealed class LedgerAggregate
        {
            public string AccountName;
            public decimal OpeningDebit;
            public decimal OpeningCredit;
            public decimal DebitTurnover;
            public decimal CreditTurnover;
            public decimal ClosingDebit;
            public decimal ClosingCredit;
        }
    }
}