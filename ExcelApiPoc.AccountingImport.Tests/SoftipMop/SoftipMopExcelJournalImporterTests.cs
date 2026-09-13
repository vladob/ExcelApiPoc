using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.SoftipMop;

namespace ExcelApiPoc.AccountingImport.Tests.SoftipMop;

public sealed class SoftipMopExcelJournalImporterTests
{
    [Fact]
    public void CanImport_RecognizesContentWithoutDependingOnFileName()
    {
        string fixture = GetFixturePath("Omida dennik 01 2025.xlsx");
        var importer = new SoftipMopExcelJournalImporter();

        Assert.True(importer.CanImport(fixture, "Softip-MOP"));
        Assert.True(importer.CanImport(fixture, "softip-mop"));
        Assert.False(importer.CanImport(fixture, "IVES"));
        Assert.False(importer.CanImport(Path.ChangeExtension(fixture, ".xls"), "Softip-MOP"));
    }

    [Theory]
    [InlineData(
        "Omida dennik 01 2025.xlsx", 64_065, 58_821, 5_244, 1_898, 0,
        18_644_638.09, 2,
        "638ADC07A9187D6372E5C03D908CF79A9ECB9E5862164D5AE772CB3945B4D1D5")]
    [InlineData(
        "Omida dennik 02 2025.xlsx", 61_045, 56_096, 4_949, 1_725, 0,
        17_710_710.99, 2,
        "B8C284463D2658F062A40AFDE50DABB32D4BDFDE2A5E34F5925396E0B77FB4DB")]
    [InlineData(
        "Omida dennik 12 2025.xlsx", 65_385, 59_966, 5_419, 3_545, 3_140,
        23_882_057.32, 3,
        "B60788F1503B51FCE168F729381BBD93CF037A50273AD8B15FB0015EB7A10BF2")]
    public void Import_MapsMonthlyFileAndAttachesSuccessfulReport(
        string fileName,
        int sourceRows,
        int transactions,
        int zeroAmountRows,
        int outsideMonthRows,
        int outsideFiscalYearRows,
        double expectedTotal,
        int diagnosticCount,
        string expectedHash)
    {
        JournalImport result = ImportFixture(fileName);

        Assert.Equal(fileName, result.SourceFileName);
        Assert.Equal("Excel", result.TechnicalType);
        Assert.Equal("Softip-MOP", result.AccountingFormat);
        Assert.Null(result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(expectedHash, result.SourceFileHash);
        Assert.Equal(transactions, result.Rows.Count);

        Assert.NotNull(result.ImportReport);
        Assert.True(result.ImportReport.IsValid);
        Assert.Equal(sourceRows, result.ImportReport.RecordCounts["SourceRows"]);
        Assert.Equal(transactions, result.ImportReport.RecordCounts["Transactions"]);
        Assert.Equal(zeroAmountRows, result.ImportReport.RecordCounts["ZeroAmountRows"]);
        Assert.Equal(outsideMonthRows, result.ImportReport.RecordCounts["PostingDatesOutsideAccountingMonth"]);
        Assert.Equal(outsideFiscalYearRows, result.ImportReport.RecordCounts["PostingDatesOutsideFiscalYear"]);
        Assert.Equal(diagnosticCount, result.ImportReport.Diagnostics.Count);
        Assert.All(result.ImportReport.Diagnostics,
            diagnostic => Assert.Equal(ImportDiagnosticSeverity.Information, diagnostic.Severity));

        ImportValidationResult balance = Assert.Single(result.ImportReport.ValidationResults);
        Assert.True(balance.IsValid);
        Assert.Equal(Convert.ToDecimal(expectedTotal), balance.ExpectedAmount);
        Assert.Equal(Convert.ToDecimal(expectedTotal), balance.ActualAmount);
        Assert.Equal(0m, balance.Difference);
    }

    [Fact]
    public void Import_DiscoversExtendedDecemberLayoutAndMapsCostCenter()
    {
        JournalImport result = ImportFixture("Omida dennik 12 2025.xlsx");
        JournalRow row = Assert.Single(result.Rows.Where(candidate =>
            candidate.DocumentNumber == "120012" &&
            candidate.DebitAccount == "45910" &&
            candidate.DebitAmount == 1_138m));

        Assert.Equal("111001", row.DebitCostCenter);
        Assert.Null(row.CreditCostCenter);
    }

    [Fact]
    public void Import_TreatsAccountingPeriodAsAuthoritativeForDecember()
    {
        JournalImport result = ImportFixture("Omida dennik 12 2025.xlsx");

        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(3_138, result.Rows.Count(row => row.PostingDate.Year == 2026));
        Assert.Contains(result.ImportReport.Diagnostics,
            diagnostic => diagnostic.Code == "SOFTIP_MOP_POSTING_DATE_OUTSIDE_FISCAL_YEAR");
    }

    [Fact]
    public void Import_MapsOneSidedPostingAndPreservesIdentifiers()
    {
        JournalRow first = ImportFixture("Omida dennik 01 2025.xlsx").Rows[0];

        Assert.Equal(1, first.SequenceNumber);
        Assert.Equal(1, first.SourceRecordNumber);
        Assert.Equal(2, first.SourceStartLineNumber);
        Assert.Equal(2, first.SourceEndLineNumber);
        Assert.Equal(
            "Omida dennik 01 2025.xlsx, worksheet Hárok1, row 2",
            first.SourceLocation);
        Assert.Equal("1", first.DocumentType);
        Assert.Equal("010001", first.DocumentNumber);
        Assert.Equal(new DateTime(2025, 2, 11), first.PostingDate);
        Assert.Equal("5500", first.Description);
        Assert.Null(first.DebitAccount);
        Assert.Null(first.DebitAmount);
        Assert.Equal("37820", first.CreditAccount);
        Assert.Equal(100m, first.CreditAmount);
        Assert.Equal(JournalRecordKind.Normal, first.RecordKind);
    }

    [Fact]
    public void Import_PreservesNegativeAmountsOnTheirOriginalSide()
    {
        JournalImport result = ImportFixture("Omida dennik 01 2025.xlsx");
        JournalRow[] negativeRows = [.. result.Rows.Where(row => row.DebitAmount < 0m || row.CreditAmount < 0m)];

        Assert.NotEmpty(negativeRows);
        Assert.Contains(negativeRows,
            row => row.DebitAccount == "39520" && row.DebitAmount == -22_715.24m);
        Assert.Contains(negativeRows,
            row => row.CreditAccount == "21111" && row.CreditAmount == -0.01m);
    }

    [Fact]
    public void Import_ExcludesOnlyRowsWithNoFinancialAmount()
    {
        JournalImport result = ImportFixture("Omida dennik 01 2025.xlsx");

        Assert.DoesNotContain(result.Rows,
            row => row.DebitAmount.GetValueOrDefault() == 0m &&
                   row.CreditAmount.GetValueOrDefault() == 0m);
        Assert.All(result.Rows, row =>
        {
            Assert.Equal(row.DebitAccount == null, row.DebitAmount == null);
            Assert.Equal(row.CreditAccount == null, row.CreditAmount == null);
        });
    }

    private static JournalImport ImportFixture(string fileName)
    {
        return new SoftipMopExcelJournalImporter().Import(GetFixturePath(fileName));
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
