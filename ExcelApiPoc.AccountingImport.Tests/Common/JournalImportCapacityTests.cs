using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

public sealed class JournalImportCapacityTests
{
    [Fact]
    public void MaximumJournalRows_MatchesAvailableWorksheetDataRows()
    {
        Assert.Equal(1_048_576, JournalImportCapacity.ExcelWorksheetRows);
        Assert.Equal(4, JournalImportCapacity.JournalHeaderRow);
        Assert.Equal(1_048_572, JournalImportCapacity.MaximumJournalRows);
    }

    [Fact]
    public void EnsureCanAppend_AllowsExactWorksheetCapacity()
    {
        JournalImportCapacity.EnsureCanAppend(
            JournalImportCapacity.MaximumJournalRows - 1,
            1,
            "December.xlsx");
    }

    [Theory]
    [InlineData(261_075)]
    [InlineData(729_747)]
    [InlineData(791_097)]
    public void EnsureCanAppend_AllowsKnownSoftipMopAnnualSizesWithoutAllocation(
        int annualSourceRows)
    {
        JournalImportCapacity.EnsureCanAppend(
            0,
            annualSourceRows,
            "Softip-MOP annual journal");
    }

    [Fact]
    public void EnsureCanAppend_AllowsKnownMonthlyCountsIncrementally()
    {
        int[] monthlySourceRows =
        [
            64_065, 61_045, 66_617, 62_202, 65_982, 67_556,
            69_621, 65_425, 68_031, 70_412, 64_756, 65_385
        ];
        int accumulatedRows = 0;

        foreach (int monthlyRows in monthlySourceRows)
        {
            JournalImportCapacity.EnsureCanAppend(
                accumulatedRows,
                monthlyRows,
                "next monthly journal");
            accumulatedRows += monthlyRows;
        }

        Assert.Equal(791_097, accumulatedRows);
    }

    [Fact]
    public void EnsureCanAppend_RejectsOneRowBeyondWorksheetCapacity()
    {
        InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            JournalImportCapacity.EnsureCanAppend(
                JournalImportCapacity.MaximumJournalRows,
                1,
                "December.xlsx"));

        Assert.Contains("1,048,573", exception.Message);
        Assert.Contains("December.xlsx", exception.Message);
        Assert.Contains("1,048,572", exception.Message);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void EnsureCanAppend_RejectsNegativeCounts(
        int existingRows,
        int additionalRows)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JournalImportCapacity.EnsureCanAppend(
                existingRows,
                additionalRows,
                "source"));
    }
}
