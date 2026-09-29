using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftPdfDennik1JournalImporterTests
{
    [Fact]
    public void Imports_ladomirov_2025_pdf_without_dropping_undated_entry()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_LADOMIROV_2025_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_LADOMIROV_2025_TEST_FILE to U_DENNIK_00323195_2025.pdf.");
        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal(2025, import.FiscalYear);
        Assert.Equal(2142, import.Rows.Count);
        Assert.Equal(1638050.39m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1638050.39m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        JournalRow undated = Assert.Single(import.Rows.Where(row => row.PostingDate == DateTime.MinValue));
        Assert.Equal("221", undated.DebitAccount);
        Assert.Equal("261", undated.CreditAccount);
        Assert.Equal(1266m, undated.DebitAmount);
        Assert.Equal(JournalDateExceptionResolution.Excluded, undated.DateExceptionResolution);
        Assert.Contains(import.Rows, row => row.PostingDate == new DateTime(2005, 9, 12));
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_PDF_MISSING_POSTING_DATE");
        Assert.Equal(11, JournalDateExceptionService.Apply(import, 2025));
    }

    [Fact]
    public void Imports_kolonica_2025_spaced_two_digit_opening_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_KOLONICA_2025_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_KOLONICA_2025_TEST_FILE to U_DENNIK_00323161_2025.pdf.");

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal(5000794.06m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(5000794.06m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(import.Rows, row => row.DebitAccount == "75ZŠ" &&
            row.CreditAccount == "701" && row.DebitAmount == 22929.45m);
        Assert.Contains(import.Rows, row => row.DebitAccount == "701" &&
            row.CreditAccount == "79ZŠ" && row.CreditAmount == 22929.45m);
    }

    [Fact]
    public void Imports_kolonica_pdf_with_spaced_analytical_account()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_KOLONICA_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing Kolonica PDF: " + path);
        JournalImport pdf = new IfoSoftPdfDennik1JournalImporter().Import(path);
        Assert.Equal(4260741.64m, pdf.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(4260741.64m, pdf.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Equal(144m, pdf.Rows.Where(row => row.DebitAccount == "551Č93")
            .Sum(row => row.DebitAmount ?? 0m));
    }

    [Theory]
    [InlineData("U_DENNIK_00323110_2024.pdf", 2024, "752020", "461001", "5262005.17")]
    [InlineData("U_DENNIK_00323110_2025.pdf", 2025, "752025", "461001", "6472424.52")]
    public void Imports_kamienka_journal_with_spaced_numeric_accounts(
        string fileName, int year, string debitCode, string creditCode, string totalText)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_KAMIENKA_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root),
            "Set IFOSOFT_PDF_JOURNAL_KAMIENKA_TEST_DIR to the folder containing the two Kamienka journal PDFs.");
        string path = Path.Combine(root!, fileName);
        Assert.True(File.Exists(path), "Missing PDF sample: " + path);

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path);
        decimal total = decimal.Parse(totalText, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(year, import.FiscalYear);
        Assert.Contains(import.Rows, row => row.DebitAccount == debitCode);
        Assert.Contains(import.Rows, row => row.CreditAccount == creditCode);
        Assert.Equal(total, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(total, import.Rows.Sum(row => row.CreditAmount ?? 0m));
    }

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

    [Fact]
    public void Imports_split_analytical_account_in_hankovce_journal()
    {
        var path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_HANKOVCE_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_HANKOVCE_TEST_FILE to U_DENNIK_00322962_2023_a.pdf.");

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal("00322962", import.Ico);
        Assert.Equal(2023, import.FiscalYear);
        Assert.Contains(import.Rows, row => row.DebitAccount == "518162" && row.DebitAmount == 110.50m);
        Assert.Equal(4848547.91m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(4848547.91m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
    }

    [Fact]
    public void Imports_hudcovce_journal_with_hyphenated_accounts_and_next_year_postings()
    {
        var path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_JOURNAL_HUDCOVCE_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_JOURNAL_HUDCOVCE_TEST_FILE to U_DENNIK_00323012_2024.pdf.");

        JournalImport import = new IfoSoftPdfDennik1JournalImporter().Import(path!);
        Assert.Equal("00323012", import.Ico);
        Assert.Equal(2024, import.FiscalYear);
        Assert.Contains(import.Rows, row => row.DebitAccount == "518P-OB");
        Assert.Contains(import.Rows, row => row.PostingDate.Year == 2025);
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_PDF_NEXT_YEAR_POSTINGS");
        Assert.Equal(4101584.38m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(4101584.38m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
