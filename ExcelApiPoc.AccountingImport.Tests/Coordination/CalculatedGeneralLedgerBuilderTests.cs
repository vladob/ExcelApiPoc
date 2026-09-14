using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;

namespace ExcelApiPoc.AccountingImport.Tests.Coordination;

public sealed class CalculatedGeneralLedgerBuilderTests
{
    [Fact]
    public void Build_CalculatesOpeningTurnoverAndClosingFromJournal()
    {
        JournalImport journal = Journal(
            Row(JournalRecordKind.Opening, "22101", 100m, "701", 100m),
            Row(JournalRecordKind.Normal, "22101", 50m, "32102", 50m),
            Row(JournalRecordKind.Closing, "702", 150m, "22101", 150m));

        CalculatedGeneralLedger result =
            CalculatedGeneralLedgerBuilder.Build(journal);

        Assert.True(result.JournalContainsOpeningRecords);
        Assert.True(result.JournalContainsClosingRecords);
        Assert.Equal(3, result.SourceRecordCount);

        CalculatedGeneralLedgerRow bank =
            Assert.Single(result.Rows, row => row.AccountCode == "22101");

        Assert.Equal(100m, bank.OpeningDebit);
        Assert.Equal(50m, bank.DebitTurnover);
        Assert.Equal(0m, bank.CreditTurnover);
        Assert.Equal(150m, bank.ClosingDebit);
        Assert.Equal(0m, bank.ClosingCredit);
        Assert.DoesNotContain(
            result.Rows,
            row => row.AccountCode == "702");
    }

    [Fact]
    public void Build_LeavesClosingUnavailableWhenJournalHasNoOpening()
    {
        JournalImport journal = Journal(
            Row(JournalRecordKind.Normal, "22101", 50m, "32102", 50m));

        CalculatedGeneralLedger result =
            CalculatedGeneralLedgerBuilder.Build(journal);

        Assert.False(result.JournalContainsOpeningRecords);

        CalculatedGeneralLedgerRow bank =
            Assert.Single(result.Rows, row => row.AccountCode == "22101");

        Assert.Equal(50m, bank.DebitTurnover);
        Assert.Null(bank.ClosingDebit);
        Assert.Null(bank.ClosingCredit);
    }

    [Fact]
    public void Build_IsAvailableWithoutImportedGeneralLedger()
    {
        JournalImport journal = Journal(
            Row(JournalRecordKind.Opening, "22101", 25m, "701", 25m));

        CalculatedGeneralLedger result =
            CalculatedGeneralLedgerBuilder.Build(journal);

        Assert.Equal("00322792", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(2, result.Rows.Count);
    }

    private static JournalImport Journal(params JournalRow[] rows)
    {
        var result = new JournalImport
        {
            Ico = "00322792",
            FiscalYear = 2024
        };

        result.Rows.AddRange(rows);
        return result;
    }

    private static JournalRow Row(
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
}
