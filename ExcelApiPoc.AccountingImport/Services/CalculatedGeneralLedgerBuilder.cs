using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AccountingImport.Services
{
    public static class CalculatedGeneralLedgerBuilder
    {
        public static CalculatedGeneralLedger Build(JournalImport journal)
        {
            if (journal == null)
                throw new ArgumentNullException(nameof(journal));

            bool containsOpening =
                journal.Rows.Any(
                    row =>
                        row.RecordKind == JournalRecordKind.Opening &&
                        row.UsedForReportCalculation);
            bool containsClosing =
                journal.Rows.Any(row => row.RecordKind == JournalRecordKind.Closing);

            var rowsByAccount =
                new Dictionary<string, CalculatedGeneralLedgerRow>(
                    StringComparer.Ordinal);

            foreach (JournalRow source in journal.Rows)
            {
                if (!source.UsedForReportCalculation)
                    continue;

                Add(
                    rowsByAccount,
                    source.DebitAccount,
                    source.DebitAmount ?? 0m,
                    true,
                    source.RecordKind);

                Add(
                    rowsByAccount,
                    source.CreditAccount,
                    source.CreditAmount ?? 0m,
                    false,
                    source.RecordKind);
            }

            var result = new CalculatedGeneralLedger
            {
                Ico = journal.Ico,
                FiscalYear = journal.FiscalYear,
                JournalContainsOpeningRecords = containsOpening,
                JournalContainsClosingRecords = containsClosing,
                SourceRecordCount = journal.Rows.Count
            };

            foreach (CalculatedGeneralLedgerRow row in
                rowsByAccount.Values.OrderBy(
                    value => value.AccountCode,
                    StringComparer.Ordinal))
            {
                if (containsOpening)
                {
                    decimal balance =
                        row.OpeningDebit -
                        row.OpeningCredit +
                        row.DebitTurnover -
                        row.CreditTurnover;

                    row.ClosingDebit = balance > 0m ? balance : 0m;
                    row.ClosingCredit = balance < 0m ? -balance : 0m;
                }

                result.Rows.Add(row);
            }

            return result;
        }

        private static void Add(
            IDictionary<string, CalculatedGeneralLedgerRow> rows,
            string accountCode,
            decimal amount,
            bool debit,
            JournalRecordKind recordKind)
        {
            string normalizedCode =
                AccountCodeNormalizer.Normalize(accountCode);

            if (normalizedCode.Length == 0)
                return;

            if (!rows.TryGetValue(
                    normalizedCode,
                    out CalculatedGeneralLedgerRow row))
            {
                row = new CalculatedGeneralLedgerRow
                {
                    AccountCode = normalizedCode
                };
                rows.Add(normalizedCode, row);
            }

            if (recordKind == JournalRecordKind.Opening)
            {
                if (debit)
                {
                    row.OpeningDebitEntryCount++;
                    row.OpeningDebit += amount;
                }
                else
                {
                    row.OpeningCreditEntryCount++;
                    row.OpeningCredit += amount;
                }

                return;
            }

            if (debit)
            {
                row.DebitEntryCount++;
                row.DebitTurnover += amount;
            }
            else
            {
                row.CreditEntryCount++;
                row.CreditTurnover += amount;
            }
        }
    }
}
