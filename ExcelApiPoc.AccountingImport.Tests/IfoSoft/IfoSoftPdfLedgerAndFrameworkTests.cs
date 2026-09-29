using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftPdfLedgerAndFrameworkTests
{
    [Theory]
    [InlineData("HL_KNIHA_00322857_2021.pdf", 2021, 12, 19688694, 37)]
    [InlineData("HL_KNIHA_00322857_2023_12.pdf", 2023, 12, 26341457, 50)]
    [InlineData("HL_KNIHA_00322857_2023.pdf", 2023, 13, 31727023, 51)]
    [InlineData("HL_KNIHA_00322857_2024_a.pdf", 2024, 12, 25653979, 126)]
    [InlineData("HL_KNIHA_00322857_2024_b.pdf", 2024, 12, 25653979, 56)]
    public void Ledger_matches_printed_annual_totals(string fileName, int year, int month, int cents, int rows)
    {
        string path = Sample(fileName);
        var importer = new IfoSoftPdfGeneralLedgerImporter();
        Assert.True(importer.CanImport(path, "IfoSoft"));
        GeneralLedgerImport result = importer.Import(path);
        Assert.Equal("00322857", result.Ico);
        Assert.Equal(year, result.FiscalYear);
        Assert.Equal(month, result.ThroughMonth);
        Assert.Equal(rows, result.Rows.Count);
        Assert.Equal(cents / 100m, result.Rows.Sum(r => r.AnnualDebitTurnover));
        Assert.Equal(cents / 100m, result.Rows.Sum(r => r.AnnualCreditTurnover));
    }

    [Fact]
    public void Two_2024_layouts_agree_by_account()
    {
        var importer = new IfoSoftPdfGeneralLedgerImporter();
        var detailed = importer.Import(Sample("HL_KNIHA_00322857_2024_a.pdf"));
        var analytical = importer.Import(Sample("HL_KNIHA_00322857_2024_b.pdf"));
        var first = detailed.Rows.GroupBy(r => r.AccountCode)
            .ToDictionary(g => g.Key, g => (Debit: g.Sum(r => r.AnnualDebitTurnover), Credit: g.Sum(r => r.AnnualCreditTurnover)));
        var second = analytical.Rows.GroupBy(r => r.AccountCode)
            .Where(g => g.Key != "701")
            .ToDictionary(g => g.Key, g => (Debit: g.Sum(r => r.AnnualDebitTurnover), Credit: g.Sum(r => r.AnnualCreditTurnover)));
        Assert.Equal(first, second);
    }

    [Fact]
    public void Hudcovce_hlknia4c_reconciles_annual_turnover()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_HUDCOVCE_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_GL_HUDCOVCE_TEST_FILE to HL_KNIHA_00323012_2024.pdf.");
        GeneralLedgerImport result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        Assert.Equal("00323012", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Contains(result.Rows, r => r.AccountCode == "042VO" && r.AnnualDebitTurnover == 10920.56m);
        Assert.Contains(result.Rows, r => r.AccountCode == "081" && r.OpeningCredit == 820560.87m);
        Assert.Equal(1090180.74m, result.Rows.Sum(r => r.AnnualDebitTurnover));
        Assert.Equal(1090180.74m, result.Rows.Sum(r => r.AnnualCreditTurnover));
    }

    [Fact]
    public void Framework_preserves_numbered_rows_and_analytical_accounts()
    {
        var result = new IfoSoftPdfAccountingFrameworkImporter().Import(Sample("UCT_ROZVRH_00322857_2023.pdf"));
        Assert.Equal("00322857", result.Ico);
        Assert.Equal(2023, result.FiscalYear);
        Assert.Equal(439, result.Rows.Count);
        Assert.Contains(result.Rows, r => r.AccountCode == "021130" && r.RowKind == AccountingFrameworkRowKind.AnalyticalAccount);
        Assert.Contains(result.Rows, r => r.SyntheticCode == "01" && r.RowKind == AccountingFrameworkRowKind.GroupHeading);
        Assert.Contains(result.Rows, r => r.AccountCode == "357UPSV" && r.AccountName == "");
        Assert.Contains(result.Rows, r => r.AccountCode == "518TEL." && r.AccountName == "Ostatné služby telefon");
    }

    [Fact]
    public void Hudcovce_framework_preserves_hyphenated_analytical_codes()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_HUDCOVCE_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_AF_HUDCOVCE_TEST_FILE to UCT_ROZVRH_00323012_2024.pdf.");
        AccountingFrameworkImport result = new IfoSoftPdfAccountingFrameworkImporter().Import(path!);
        Assert.Equal("00323012", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Contains(result.Rows, r => r.SourceRecordNumber == 27 && r.AccountCode == "042A-PO" &&
            r.RowKind == AccountingFrameworkRowKind.AnalyticalAccount);
        Assert.Contains(result.Rows, r => r.AccountCode == "042MŠ-U");
        Assert.Contains(result.Rows, r => r.SourceRecordNumber == 181 && r.AccountCode == "321-POD");
        Assert.Contains(result.Rows, r => r.SourceRecordNumber == 387 && r.AnalyticalCode == "-POD");
    }

    [Fact]
    public void Kamienka_2025_framework_preserves_inserted_row_number()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_KAMIENKA_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_AF_KAMIENKA_TEST_FILE to UCT_ROZVRH_00323110_2025.pdf.");
        AccountingFrameworkImport result = new IfoSoftPdfAccountingFrameworkImporter().Import(path!);
        Assert.Equal(705, result.Rows.Count);
        Assert.Equal(new[] { 424, 705, 425 }, result.Rows.Skip(423).Take(3)
            .Select(row => row.SourceRecordNumber).ToArray());
        Assert.Contains(result.Rows, row => row.SourceRecordNumber == 705 &&
            row.AccountCode == "4282025");
    }

    [Fact]
    public void Kolonica_framework_preserves_percentage_analytical_code()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_AF_KOLONICA_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_AF_KOLONICA_TEST_FILE to UCT_ROZVRH_00323161_2022.pdf.");
        AccountingFrameworkImport result = new IfoSoftPdfAccountingFrameworkImporter().Import(path!);
        Assert.Contains(result.Rows, row => row.SourceRecordNumber == 415 &&
            row.AccountCode == "3575%" && row.RowKind == AccountingFrameworkRowKind.AnalyticalAccount);
    }

    [Theory]
    [InlineData("HL_KNIHA_00323161_2022.pdf")]
    [InlineData("HL_KNIHA_00323161_2022_b.pdf")]
    public void Kolonica_2022_ledgers_reconcile_printed_turnover(string fileName)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_KOLONICA_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root),
            "Set IFOSOFT_PDF_GL_KOLONICA_TEST_DIR to the folder containing both Kolonica ledger PDFs.");
        string path = Path.Combine(root!, fileName);
        Assert.True(File.Exists(path), "Missing ledger PDF: " + path);
        GeneralLedgerImport result = new IfoSoftPdfGeneralLedgerImporter().Import(path);
        Assert.Equal("00323161", result.Ico);
        Assert.Equal(2022, result.FiscalYear);
        Assert.Equal(1566981.92m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1566981.92m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }

    [Fact]
    public void Kolonica_2023_ledger_includes_spaced_asset_register_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_KOLONICA_2023_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_GL_KOLONICA_2023_TEST_FILE to HL_KNIHA_00323161_2023.pdf.");
        GeneralLedgerImport result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        Assert.Equal("00323161", result.Ico);
        Assert.Equal(2023, result.FiscalYear);
        Assert.Contains(result.Rows, row => row.AccountCode == "75ZŠ" && row.SyntheticCode == "75" &&
            row.AnalyticalCode == "ZŠ" && row.AnnualDebitTurnover == 22929.45m);
        Assert.Contains(result.Rows, row => row.AccountCode == "79ZŠ" && row.SyntheticCode == "79" &&
            row.AnalyticalCode == "ZŠ" && row.AnnualCreditTurnover == 22929.45m);
        Assert.Equal(1599671.47m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1599671.47m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }

    [Fact]
    public void Kolonica_2025_detailed_ledger_does_not_double_count_question_mark_subtotals()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_KOLONICA_2025_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_GL_KOLONICA_2025_TEST_FILE to HL_KNIHA_00323161_2025_a.pdf.");
        GeneralLedgerImport result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        Assert.Equal("00323161", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Contains(result.Rows, row => row.AccountCode == "751976" &&
            row.AnnualDebitTurnover == 6748.44m && row.AnnualCreditTurnover == 3329.91m);
        Assert.DoesNotContain(result.Rows, row => row.AccountCode == "751");
        Assert.Single(result.Rows.Where(row => row.AccountCode == "799"));
        Assert.Equal(1811217.28m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1811217.28m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }

    [Fact]
    public void Kolonica_2025_hlknia4d_synthetic_ledger_reconciles_its_printed_total()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_KOLONICA_2025_SUMMARY_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_PDF_GL_KOLONICA_2025_SUMMARY_TEST_FILE to HL_KNIHA_00323161_2025_b.pdf.");
        GeneralLedgerImport result = new IfoSoftPdfGeneralLedgerImporter().Import(path!);
        Assert.Equal("00323161", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Contains(result.Rows, row => row.AccountCode == "021" &&
            row.AnnualDebitTurnover == 31660.28m);
        Assert.Contains(result.Rows, row => row.AccountCode == "751" &&
            row.AnnualDebitTurnover == 6748.44m && row.AnnualCreditTurnover == 3329.91m);
        Assert.Contains(result.Rows, row => row.AccountCode == "799" &&
            row.AnnualDebitTurnover == 3329.91m && row.AnnualCreditTurnover == 6748.44m);
        Assert.Equal(1811222.87m, result.Rows.Sum(row => row.AnnualDebitTurnover));
        Assert.Equal(1811222.87m, result.Rows.Sum(row => row.AnnualCreditTurnover));
    }

    private static string Sample(string fileName)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_AF_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root), "Set IFOSOFT_PDF_GL_AF_TEST_DIR to the folder containing the supplied IfoSoft PDFs.");
        string path = Path.Combine(root!, fileName);
        Assert.True(File.Exists(path), "Missing PDF sample: " + path);
        return path;
    }
}
