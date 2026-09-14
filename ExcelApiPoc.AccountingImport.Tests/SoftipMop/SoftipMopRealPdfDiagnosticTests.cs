using ExcelApiPoc.AccountingImport.Services.SoftipMop;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.SoftipMop;

public sealed class SoftipMopRealPdfDiagnosticTests(
    ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void Import_real_softip_general_ledger()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var importer =
            new SoftipMopPdfGeneralLedgerImporter();

        Assert.True(
            importer.CanImport(pdfPath, "Softip-MOP"));

        var result = importer.Import(pdfPath);

        _output.WriteLine($"File: {result.SourceFileName}");
        _output.WriteLine($"IČO: {result.Ico}");
        _output.WriteLine($"Fiscal year: {result.FiscalYear}");
        _output.WriteLine($"Through month: {result.ThroughMonth}");
        _output.WriteLine($"Canonical rows: {result.Rows.Count}");
        _output.WriteLine(
            $"Validations: {result.ImportReport.ValidationResults.Count}");
        _output.WriteLine(
            $"Diagnostics: {result.ImportReport.Diagnostics.Count}");

        Assert.Equal("31715362", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(12, result.ThroughMonth);
        Assert.Equal("12/2025", result.PeriodHeader);
        Assert.Equal("PDF", result.TechnicalType);
        Assert.Equal("Softip-MOP", result.AccountingFormat);

        Assert.Equal(248, result.Rows.Count);
        Assert.Equal(
            248,
            result.ImportReport.ValidationResults.Count);

        Assert.True(result.ImportReport.IsValid);
        Assert.Empty(result.ImportReport.Diagnostics);

        Assert.All(
            result.ImportReport.ValidationResults,
            validation => Assert.True(
                validation.IsValid,
                $"{validation.Scope}: " +
                $"difference {validation.Difference}"));

        var first = result.Rows[0];
        var last = result.Rows[^1];

        Assert.Equal("01110", first.AccountCode);
        Assert.Equal("ZRIADOVACIE VYDAVKY", first.AccountName);

        Assert.Equal("75290", last.AccountCode);
        Assert.Equal("ZUCTOVACI UCET - 752", last.AccountName);

        var account04210 = Assert.Single(
            result.Rows,
            row => row.AccountCode == "04210");

        Assert.Equal(36520.12m, account04210.OpeningDebit);
        Assert.Equal(0m, account04210.OpeningCredit);

        Assert.Equal(
            604.77m,
            account04210.PeriodDebitTurnover);

        Assert.Equal(
            1678.65m,
            account04210.PeriodCreditTurnover);

        Assert.Equal(
            34362.69m,
            account04210.AnnualDebitTurnover);

        Assert.Equal(
            47655.87m,
            account04210.AnnualCreditTurnover);

        Assert.Equal(23226.94m, account04210.ClosingDebit);
        Assert.Equal(0m, account04210.ClosingCredit);

        var negativeAccount = Assert.Single(
            result.Rows,
            row => row.AccountCode == "75210");

        Assert.Equal(0m, negativeAccount.OpeningDebit);
        Assert.Equal(
            2276102.77m,
            negativeAccount.OpeningCredit);

        Assert.Equal(0m, negativeAccount.ClosingDebit);
        Assert.Equal(
            2276102.77m,
            negativeAccount.ClosingCredit);

        _output.WriteLine("");
        _output.WriteLine(
            $"First: {first.AccountCode} {first.AccountName}");
        _output.WriteLine(
            $"Last:  {last.AccountCode} {last.AccountName}");

        _output.WriteLine("");
        _output.WriteLine("Account 04210:");
        _output.WriteLine(
            $"  Opening debit: {account04210.OpeningDebit}");
        _output.WriteLine(
            $"  Monthly debit: {account04210.PeriodDebitTurnover}");
        _output.WriteLine(
            $"  Monthly credit: {account04210.PeriodCreditTurnover}");
        _output.WriteLine(
            $"  Annual debit: {account04210.AnnualDebitTurnover}");
        _output.WriteLine(
            $"  Annual credit: {account04210.AnnualCreditTurnover}");
        _output.WriteLine(
            $"  Closing debit: {account04210.ClosingDebit}");
    }
}