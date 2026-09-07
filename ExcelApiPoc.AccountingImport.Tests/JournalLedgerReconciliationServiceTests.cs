using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;

namespace ExcelApiPoc.AccountingImport.Tests;

public sealed class JournalLedgerReconciliationServiceTests
{
    [Fact]
    public void Reconcile_UsesLedgerOpeningWhenJournalHasNone()
    {
        JournalImport journal = CreateJournal(
            Normal("22101", 50m, "32102", 50m));

        GeneralLedgerImport ledger = CreateLedger(
            Ledger("22101", 100m, 0m, 50m, 0m, 150m, 0m),
            Ledger("32102", 0m, 100m, 0m, 50m, 0m, 150m),
            Ledger("01301", 25m, 0m, 0m, 0m, 25m, 0m));

        JournalLedgerReconciliationResult result =
            JournalLedgerReconciliationService.Reconcile(
                journal,
                ledger);

        Assert.False(result.JournalContainsOpeningRecords);
        Assert.Equal(
            OpeningBalanceSource.GeneralLedger,
            result.OpeningBalanceSource);

        Assert.True(result.IsReconciled);
        Assert.Equal(3, result.ReconciledAccountCount);
        Assert.Equal(0, result.DifferentAccountCount);
        Assert.Equal(1, result.LedgerOnlyAccountCount);

        JournalLedgerAccountReconciliation bank =
            Assert.Single(
                result.Accounts,
                account => account.AccountCode == "22101");

        Assert.Equal(100m, bank.EffectiveOpeningDebit);
        Assert.Equal(150m, bank.CalculatedClosingDebit);
        Assert.True(bank.IsReconciled);

        JournalLedgerAccountReconciliation carried =
            Assert.Single(
                result.Accounts,
                account => account.AccountCode == "01301");

        Assert.False(carried.HasJournalActivity);
        Assert.True(carried.IsReconciled);
    }

    [Fact]
    public void Reconcile_UsesJournalOpeningWhenPresent()
    {
        JournalImport journal = CreateJournal(
            Opening("22101", 100m, "701", 100m),
            Normal("22101", 50m, "32102", 50m));

        GeneralLedgerImport ledger = CreateLedger(
            Ledger("22101", 100m, 0m, 50m, 0m, 150m, 0m),
            Ledger("701", 0m, 100m, 0m, 0m, 0m, 100m),
            Ledger("32102", 0m, 0m, 0m, 50m, 0m, 50m));

        JournalLedgerReconciliationResult result =
            JournalLedgerReconciliationService.Reconcile(
                journal,
                ledger);

        Assert.True(result.JournalContainsOpeningRecords);
        Assert.Equal(
            OpeningBalanceSource.Journal,
            result.OpeningBalanceSource);
        Assert.True(result.IsReconciled);
    }

    [Fact]
    public void Reconcile_DetectsAndExcludesClosingRecords()
    {
        JournalImport journal = CreateJournal(
            Opening("22101", 100m, "701", 100m),
            Normal("22101", 50m, "32102", 50m),
            Closing("702", 150m, "22101", 150m),
            Closing("32102", 50m, "702", 50m));

        GeneralLedgerImport ledger = CreateLedger(
            Ledger("22101", 100m, 0m, 50m, 0m, 150m, 0m),
            Ledger("701", 0m, 100m, 0m, 0m, 0m, 100m),
            Ledger("32102", 0m, 0m, 0m, 50m, 0m, 50m));

        JournalLedgerReconciliationResult result =
            JournalLedgerReconciliationService.Reconcile(
                journal,
                ledger);

        Assert.True(result.JournalContainsClosingRecords);
        Assert.True(result.IsReconciled);

        Assert.DoesNotContain(
            result.Accounts,
            account => account.AccountCode == "702");
    }

    [Fact]
    public void Reconcile_ReportsTurnoverAndClosingDifference()
    {
        JournalImport journal = CreateJournal(
            Normal("22101", 50m, "32102", 50m));

        GeneralLedgerImport ledger = CreateLedger(
            Ledger("22101", 100m, 0m, 40m, 0m, 140m, 0m),
            Ledger("32102", 0m, 100m, 0m, 50m, 0m, 150m));

        JournalLedgerReconciliationResult result =
            JournalLedgerReconciliationService.Reconcile(
                journal,
                ledger);

        Assert.False(result.IsReconciled);
        Assert.Equal(1, result.DifferentAccountCount);
        Assert.Equal(10m, result.DebitTurnoverDifference);
        Assert.Equal(10m, result.ClosingBalanceDifference);

        JournalLedgerAccountReconciliation bank =
            Assert.Single(
                result.Accounts,
                account => account.AccountCode == "22101");

        Assert.False(bank.IsReconciled);
        Assert.Equal(10m, bank.DebitTurnoverDifference);
        Assert.Equal(10m, bank.ClosingBalanceDifference);
    }

    private static JournalImport CreateJournal(
        params JournalRow[] rows)
    {
        var result = new JournalImport
        {
            Ico = "00325791",
            FiscalYear = 2024
        };

        result.Rows.AddRange(rows);
        return result;
    }

    private static GeneralLedgerImport CreateLedger(
        params GeneralLedgerRow[] rows)
    {
        var result = new GeneralLedgerImport
        {
            Ico = "00325791",
            FiscalYear = 2024
        };

        result.Rows.AddRange(rows);
        return result;
    }

    private static JournalRow Normal(
        string debitAccount,
        decimal debitAmount,
        string creditAccount,
        decimal creditAmount)
    {
        return Journal(
            JournalRecordKind.Normal,
            debitAccount,
            debitAmount,
            creditAccount,
            creditAmount);
    }

    private static JournalRow Opening(
        string debitAccount,
        decimal debitAmount,
        string creditAccount,
        decimal creditAmount)
    {
        return Journal(
            JournalRecordKind.Opening,
            debitAccount,
            debitAmount,
            creditAccount,
            creditAmount);
    }

    private static JournalRow Closing(
        string debitAccount,
        decimal debitAmount,
        string creditAccount,
        decimal creditAmount)
    {
        return Journal(
            JournalRecordKind.Closing,
            debitAccount,
            debitAmount,
            creditAccount,
            creditAmount);
    }

    private static JournalRow Journal(
        JournalRecordKind kind,
        string debitAccount,
        decimal debitAmount,
        string creditAccount,
        decimal creditAmount)
    {
        return new JournalRow
        {
            RecordKind = kind,
            DebitAccount = debitAccount,
            DebitAmount = debitAmount,
            CreditAccount = creditAccount,
            CreditAmount = creditAmount
        };
    }

    private static GeneralLedgerRow Ledger(
        string accountCode,
        decimal openingDebit,
        decimal openingCredit,
        decimal debitTurnover,
        decimal creditTurnover,
        decimal closingDebit,
        decimal closingCredit)
    {
        return new GeneralLedgerRow
        {
            AccountCode = accountCode,
            AccountName = accountCode,
            OpeningDebit = openingDebit,
            OpeningCredit = openingCredit,
            AnnualDebitTurnover = debitTurnover,
            AnnualCreditTurnover = creditTurnover,
            ClosingDebit = closingDebit,
            ClosingCredit = closingCredit
        };
    }
}