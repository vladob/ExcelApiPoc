using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftDubravkaCsvLedgerDiagnosticTests
{
    [Fact]
    public void Dubravka_2025_malformed_ledger_reports_filename_and_line()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_CSV_GL_DUBRAVKA_2025_MALFORMED_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing original malformed ledger: " + path);
        var error = Assert.Throws<InvalidDataException>(() =>
            new IfoSoftCsvGeneralLedgerImporter().Import(path));
        Assert.Contains(Path.GetFileName(path), error.Message);
        Assert.Contains("line 13:", error.Message);
        Assert.Contains("unterminated quoted field", error.Message);
        Assert.IsType<InvalidDataException>(error.InnerException);
    }
}
