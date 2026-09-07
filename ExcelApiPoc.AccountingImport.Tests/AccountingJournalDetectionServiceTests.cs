using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;

namespace ExcelApiPoc.AccountingImport.Tests;

public sealed class AccountingJournalDetectionServiceTests
{
    [Fact]
    public void TryDetect_DetectsUrbisJournal()
    {
        bool detected =
            AccountingJournalDetectionService.TryDetect(
                GetFixturePath(
                    "U_DENNIK_00325791_202412.xlsx"),
                out JournalDetectionResult result);

        Assert.True(detected);
        Assert.NotNull(result);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("Urbis", result.AccountingFormat);
        Assert.Equal("00325791", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
    }

    [Fact]
    public void TryDetect_DoesNotTreatLedgerAsJournal()
    {
        bool detected =
            AccountingJournalDetectionService.TryDetect(
                GetFixturePath(
                    "HL_KNIHA_00325791_202412.xls"),
                out JournalDetectionResult result);

        Assert.False(detected);
        Assert.Null(result);
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            fileName);
    }
}