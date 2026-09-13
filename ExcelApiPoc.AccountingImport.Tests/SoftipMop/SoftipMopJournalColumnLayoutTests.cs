using ExcelApiPoc.AccountingImport.Services.SoftipMop;

namespace ExcelApiPoc.AccountingImport.Tests.SoftipMop;

public sealed class SoftipMopJournalColumnLayoutTests
{
    [Fact]
    public void TryDiscover_UsesNormalizedNamesInsteadOfFixedPositions()
    {
        string[] headers =
        [
            "Unused",
            " DAL ",
            "Stredisko",
            "Číslo dokladu",
            "Účet",
            "Má dať",
            "Popis zápisu",
            "Dát.účt.",
            "DD",
            "Účt.mes.",
            "kód dávky"
        ];

        bool recognized = SoftipMopJournalColumnLayout.TryDiscover(
            headers,
            out SoftipMopJournalColumnLayout? layout,
            out string? error);

        Assert.True(recognized, error);
        Assert.NotNull(layout);
        Assert.Equal(9, layout.AccountingPeriod);
        Assert.Equal(8, layout.DocumentType);
        Assert.Equal(3, layout.DocumentNumber);
        Assert.Equal(4, layout.Account);
        Assert.Equal(7, layout.PostingDate);
        Assert.Equal(6, layout.Description);
        Assert.Equal(5, layout.DebitAmount);
        Assert.Equal(1, layout.CreditAmount);
        Assert.Equal(2, layout.CostCenter);
    }

    [Fact]
    public void TryDiscover_IgnoresUnnamedTrailingColumns()
    {
        string[] headers =
        [
            "Účt.mes.", "DD", "Číslo dokladu", "Účet",
            "Dát.účt.", "Popis zápisu", "Má dať", "DAL",
            null!, "", "   "
        ];

        bool recognized = SoftipMopJournalColumnLayout.TryDiscover(
            headers,
            out SoftipMopJournalColumnLayout? layout,
            out string? error);

        Assert.True(recognized, error);
        Assert.NotNull(layout);
        Assert.Null(layout.CostCenter);
    }

    [Fact]
    public void TryDiscover_RejectsMissingRequiredColumn()
    {
        string[] headers =
        [
            "Účt.mes.", "DD", "Číslo dokladu", "Účet",
            "Dát.účt.", "Popis zápisu", "Má dať"
        ];

        bool recognized = SoftipMopJournalColumnLayout.TryDiscover(
            headers,
            out SoftipMopJournalColumnLayout? layout,
            out string? error);

        Assert.False(recognized);
        Assert.Null(layout);
        Assert.Contains("DAL", error);
    }
}
