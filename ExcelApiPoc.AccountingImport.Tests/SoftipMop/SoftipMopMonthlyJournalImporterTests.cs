using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.SoftipMop;

namespace ExcelApiPoc.AccountingImport.Tests.SoftipMop;

public sealed class SoftipMopMonthlyJournalImporterTests
{
    [Fact]
    public void Import_OrdersFilesCombinesReportsAndAssignsContinuousSequences()
    {
        string january = GetFixturePath("Omida dennik 01 2025.xlsx");
        string february = GetFixturePath("Omida dennik 02 2025.xlsx");
        string december = GetFixturePath("Omida dennik 12 2025.xlsx");

        JournalImport result = new SoftipMopMonthlyJournalImporter().Import(
            new[] { december, january, february });

        Assert.Equal("3 Softip-MOP monthly journal files", result.SourceFileName);
        Assert.Null(result.SourceFilePath);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("Softip-MOP", result.AccountingFormat);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(174_883, result.Rows.Count);
        Assert.Equal(1, result.Rows[0].SequenceNumber);
        Assert.Equal(174_883, result.Rows[^1].SequenceNumber);

        Assert.Contains("Omida dennik 01 2025.xlsx", result.Rows[0].SourceLocation);
        Assert.Contains("Omida dennik 02 2025.xlsx", result.Rows[58_821].SourceLocation);
        Assert.Contains("Omida dennik 12 2025.xlsx", result.Rows[114_917].SourceLocation);
        Assert.Equal(1, result.Rows[58_821].SourceRecordNumber);
        Assert.Equal(1, result.Rows[114_917].SourceRecordNumber);

        Assert.True(result.ImportReport.IsValid);
        Assert.Equal(3, result.ImportReport.RecordCounts["SourceFiles"]);
        Assert.Equal(3, result.ImportReport.RecordCounts["AccountingPeriods"]);
        Assert.Equal(190_495, result.ImportReport.RecordCounts["SourceRows"]);
        Assert.Equal(174_883, result.ImportReport.RecordCounts["Transactions"]);
        Assert.Equal(15_612, result.ImportReport.RecordCounts["ZeroAmountRows"]);
        Assert.Equal(7_168, result.ImportReport.RecordCounts["PostingDatesOutsideAccountingMonth"]);
        Assert.Equal(3_140, result.ImportReport.RecordCounts["PostingDatesOutsideFiscalYear"]);
        Assert.Equal(9, result.ImportReport.RecordCounts["MissingAccountingPeriods"]);

        Assert.NotNull(result.ImportReport.Performance);
        Assert.True(result.ImportReport.Performance.ElapsedMilliseconds >= 0);
        Assert.True(result.ImportReport.Performance.ManagedMemoryBeforeBytes >= 0);
        Assert.True(result.ImportReport.Performance.ManagedMemoryAfterBytes >= 0);
        Assert.Equal(3, result.ImportReport.Performance.SourceFileCount);
        Assert.Equal(174_883, result.ImportReport.Performance.CanonicalRowCount);

        ImportValidationResult combined = Assert.Single(
            result.ImportReport.ValidationResults.Where(validation =>
                validation.Code == "SOFTIP_MOP_COMBINED_DEBIT_CREDIT_BALANCE"));
        Assert.True(combined.IsValid);
        Assert.Equal(60_237_406.40m, combined.ExpectedAmount);
        Assert.Equal(60_237_406.40m, combined.ActualAmount);
        Assert.Equal(0m, combined.Difference);

        ImportDiagnostic missing = Assert.Single(
            result.ImportReport.Diagnostics.Where(diagnostic =>
                diagnostic.Code == "SOFTIP_MOP_MISSING_ACCOUNTING_PERIODS"));
        Assert.Equal(ImportDiagnosticSeverity.Warning, missing.Severity);
        Assert.Contains("202503", missing.Message);
        Assert.Contains("202511", missing.Message);
    }

    [Fact]
    public void Import_TwoConsecutivePeriodsDoesNotReportMissingMonth()
    {
        JournalImport result = new SoftipMopMonthlyJournalImporter().Import(
            new[]
            {
                GetFixturePath("Omida dennik 02 2025.xlsx"),
                GetFixturePath("Omida dennik 01 2025.xlsx")
            });

        Assert.Equal(0, result.ImportReport.RecordCounts["MissingAccountingPeriods"]);
        Assert.DoesNotContain(result.ImportReport.Diagnostics,
            diagnostic => diagnostic.Code == "SOFTIP_MOP_MISSING_ACCOUNTING_PERIODS");
    }

    [Fact]
    public void Import_RejectsDuplicateAccountingPeriod()
    {
        string january = GetFixturePath("Omida dennik 01 2025.xlsx");

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            new SoftipMopMonthlyJournalImporter().Import(new[] { january, january }));

        Assert.Contains("Accounting period 202501", exception.Message);
        Assert.Contains("more than one", exception.Message);
    }

    [Fact]
    public void ValidatePeriods_RejectsMixedFiscalYears()
    {
        var files = new[]
        {
            new SoftipMopJournalFileDescriptor("January.xlsx", "202501", 2025, 1),
            new SoftipMopJournalFileDescriptor("December.xlsx", "202612", 2026, 12)
        };

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() =>
            SoftipMopMonthlyJournalImporter.ValidatePeriods(files));

        Assert.Contains("mixed fiscal years 2025 and 2026", exception.Message);
    }

    [Fact]
    public void Import_RejectsEmptyFileCollection()
    {
        Assert.Throws<ArgumentException>(() =>
            new SoftipMopMonthlyJournalImporter().Import(Array.Empty<string>()));
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            fileName);
    }
}
