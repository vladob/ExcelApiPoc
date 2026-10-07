using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;

namespace ExcelApiPoc.AccountingImport.Tests.Urbis;

public sealed class UrbisMultipleJournalTests
{
    [Fact]
    public void Sobrance_2025_original_journals_combine_with_source_provenance()
    {
        string? root = Environment.GetEnvironmentVariable("URBIS_SOBRANCE_2025_TEST_DIR");
        if (string.IsNullOrWhiteSpace(root)) return;
        var request = new AccountingImportRequest { AccountingFormat = "Urbis", ExpectedIco = "00325791", ExpectedFiscalYear = 2025 };
        request.JournalFilePaths.AddRange(new[] { "účtovný denník 1.Q..xls", "účtovný denník 4.-52025.xls", "účtovný denník 62025.xls", "účtovný denník 3.Q..xls", "účtovný denník 4.Q..xls" }.Select(n => Path.Combine(root, n)));
        request.GeneralLedgerFilePath = Path.Combine(root, "hlavná kniha 2025.xls");
        var result = AccountingImportCoordinator.CreateDefault().Import(request);
        Assert.Equal(29582, result.Journal.Rows.Count);
        Assert.Equal(36628672.90m, result.Journal.Rows.Sum(r => r.DebitAmount ?? 0));
        Assert.Equal(36628672.90m, result.Journal.Rows.Sum(r => r.CreditAmount ?? 0));
        Assert.Equal(5, result.Journal.Rows.Select(r => r.SourceFields["SourceFile"]).Distinct().Count());
        Assert.NotNull(result.JournalLedgerReconciliation);
        Assert.Equal(556, result.GeneralLedger.Rows.Count);
        Assert.Equal(0m, result.JournalLedgerReconciliation.DebitTurnoverDifference);
        Assert.Equal(0m, result.JournalLedgerReconciliation.CreditTurnoverDifference);
    }
}
