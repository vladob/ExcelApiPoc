using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftPdfDennik1JournalImporterTests
{
    [Fact]
    public void Imports_full_year_and_reconciles_printed_totals()
    {
        var path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_TEST_FILE to U_DENNIK_00323748_2023_a.pdf.");

        Assert.True(new IfoSoftPdfDennik1JournalImporter().CanImport(path!, "IfoSoft"));
        Assert.True(AccountingJournalDetectionService.TryDetect(path!, out var detection));
        Assert.Equal("PDF", detection!.TechnicalType);
        Assert.Equal("00323748", detection.Ico);
        Assert.Equal(2023, detection.FiscalYear);

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal(3759, import.Rows.Count);
        Assert.Equal(3062919.13m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(3062919.13m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Equal(350, import.Rows.Count(row => row.PostingDate.Month == 1));
        Assert.Equal(259, import.Rows.Count(row => row.PostingDate.Month == 7));
        Assert.Contains(import.Rows, row => row.RecordKind == JournalRecordKind.Opening);
        Assert.Contains(import.Rows, row => row.DebitAmount < 0 || row.CreditAmount < 0);
    }

    [Fact]
    public void Imports_second_entity_with_budget_marker_and_unicode_account()
    {
        var path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_SECOND_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_SECOND_TEST_FILE to U_DENNIK_00322857_2023.pdf.");

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal("00322857", import.Ico);
        Assert.Equal(854, import.Rows.Count);
        Assert.Equal(412470.07m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(412470.07m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(import.Rows, row => row.CreditAccount == "357ŽP");
        Assert.Contains(import.Rows, row => row.DebitAccount == "357ŽP");
        Assert.DoesNotContain(import.Rows, row =>
            (row.DebitAccount ?? "").Contains("R633") ||
            (row.CreditAccount ?? "").Contains("R633"));
    }
}
