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

    private static string Sample(string fileName)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_AF_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root), "Set IFOSOFT_PDF_GL_AF_TEST_DIR to the folder containing the supplied IfoSoft PDFs.");
        string path = Path.Combine(root!, fileName);
        Assert.True(File.Exists(path), "Missing PDF sample: " + path);
        return path;
    }
}
