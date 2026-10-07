using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;

public sealed class IvesExcelLayoutTests
{
    static string[][] Journal()
    {
        var rows = Enumerable.Range(0, 21).Select(_ => Enumerable.Repeat("", 30).ToArray()).ToArray();
        rows[1][1] = "Test entity"; rows[2][22] = "IČO:00325961";
        rows[4][1] = "Účtovný denník"; rows[7][1] = "Dátum vytvorenia   od: 1.1.2024, do: 31.12.2024";
        rows[11][1] = "Dátum"; rows[11][4] = "Čís. dokladu"; rows[11][24] = "Suma"; rows[13][27] = "Modul";
        rows[18][1] = "2024-01-02"; rows[18][4] = "B1"; rows[18][8] = "211. 1 ."; rows[18][17] = "321. 2 .";
        rows[18][23] = "12.34"; rows[18][27] = "Test operation"; rows[19][27] = "UCT";
        rows[20][1] = "Spolu :"; rows[20][23] = "12.34";
        return rows;
    }
    static string[][] Ledger()
    {
        var rows = Enumerable.Range(0, 19).Select(_ => Enumerable.Repeat("", 37).ToArray()).ToArray();
        rows[1][1] = "Test entity"; rows[2][23] = "IČO:00325961"; rows[3][1] = "Hlavná kniha";
        rows[6][1] = "Dátum od: 1.1.2024, Dátum do: 31.12.2024"; rows[10][20] = "Obraty do DATA";
        rows[12][1] = "Dátum"; rows[12][16] = "Počiatočný stav"; rows[12][36] = "Nový zostatok";
        rows[16][1] = "-"; rows[16][3] = "-"; rows[16][7] = "211. 1 ."; rows[16][11] = "Cash";
        foreach (int column in new[] { 16, 18, 21, 26, 31, 35 }) rows[17][column] = "0";
        rows[17][16] = "10"; rows[17][26] = "12.34"; rows[17][35] = "22.34";
        rows[18][1] = "C e l k o m";
        foreach (int column in new[] { 16, 18, 21, 31, 35 }) rows[18][column] = rows[17][column];
        rows[18][27] = "12.34";
        return rows;
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Inline_ledger_variant_validates_its_printed_total(bool corruptTotal)
    {
        var rows = Enumerable.Range(0, 17).Select(_ => Enumerable.Repeat("", 28).ToArray()).ToArray();
        rows[1][1] = "Test entity"; rows[2][21] = "IČO:00322881";
        rows[4][1] = "Hlavná kniha";
        rows[7][1] = "Dátum od: 1.1.2024, Dátum do: 31.12.2024";
        foreach (var pair in new Dictionary<int, string> { [1] = "Dátum", [3] = "Doklad", [17] = "Starý zostatok", [22] = "Obrat MD", [24] = "Obrat DAL", [27] = "Nový zostatok" }) rows[11][pair.Key] = pair.Value;
        rows[14][1] = "Hlavná činnosť"; rows[14][10] = "EUR";
        rows[15][1] = rows[15][3] = "-"; rows[15][8] = "211.1."; rows[15][12] = "Cash";
        rows[15][15] = "10"; rows[15][19] = "12.34"; rows[15][23] = "0"; rows[15][26] = "22.34";
        rows[16][1] = "C e l k o m"; rows[16][13] = "10"; rows[16][19] = corruptTotal ? "13.34" : "12.34";
        rows[16][23] = "0"; rows[16][26] = "22.34";
        var path = UrbisLayoutTests.Workbook(rows);
        try
        {
            using var importer = new CompactLayoutImporter(path, StagedImportRuntime.ProducerDirectory("IVES"));
            var result = importer.Examine(ImportLevel.Normalize);
            if (corruptTotal)
            {
                Assert.Equal("invalid", result.Status); Assert.Null(importer.Canonical);
                Assert.Contains(result.Issues, i => i.Code == "printedTotalMismatch");
            }
            else
            {
                Assert.Equal("completed", result.Status);
                var row = Assert.Single(Assert.IsType<GeneralLedgerImport>(importer.Canonical).Rows);
                Assert.Equal("211.1.", row.AccountCode); Assert.Equal(12.34m, row.AnnualDebitTurnover);
            }
        }
        finally { File.Delete(path); }
    }

    static void Invalid(string[][] rows, string code)
    {
        var path = UrbisLayoutTests.Workbook(rows);
        try
        {
            using var importer = new CompactLayoutImporter(path, StagedImportRuntime.ProducerDirectory("IVES"));
            var result = importer.Examine(ImportLevel.Normalize);
            Assert.Equal("invalid", result.Status); Assert.Null(importer.Canonical);
            Assert.Contains(result.Issues, i => i.Code == code);
        }
        finally { File.Delete(path); }
    }
    [Fact] public void Journal_detects_content_and_keeps_continuation_provenance()
    {
        var path = UrbisLayoutTests.Workbook(Journal());
        try
        {
            Assert.True(StagedImportRuntime.TryDetectJournal(path, out var detection));
            Assert.Equal("IVES", detection.AccountingFormat); Assert.Equal("00325961", detection.Ico); Assert.Equal(2024, detection.FiscalYear);
            using var importer = new CompactLayoutImporter(path, StagedImportRuntime.CatalogueDirectory);
            Assert.Empty(importer.Examine(ImportLevel.Identify).Rows);
            Assert.Equal("completed", importer.Examine(ImportLevel.Normalize).Status);
            var row = Assert.Single(Assert.IsType<JournalImport>(importer.Canonical).Rows);
            Assert.Equal("211.1.", row.DebitAccount); Assert.Equal(12.34m, row.DebitAmount);
            Assert.Equal("UCT", row.SourceFields["continuation:20:source:28"]);
        }
        finally { File.Delete(path); }
    }
    [Fact] public void Ledger_joins_amount_row_and_uses_report_total_column()
    {
        var path = UrbisLayoutTests.Workbook(Ledger());
        try
        {
            var gl = StagedImportRuntime.Import<GeneralLedgerImport>(path, "IVES", "GL");
            Assert.Equal(2024, gl.FiscalYear); var row = Assert.Single(gl.Rows);
            Assert.Equal("211.1.", row.AccountCode); Assert.Equal(22.34m, row.SourceClosingNet);
            Assert.Equal(12.34m, row.AnnualDebitTurnover);
            Assert.Equal("worksheet row 18", row.SourceDimensions["AmountSourceLocation"]);
        }
        finally { File.Delete(path); }
    }
    [Fact] public void Missing_amount_row_is_not_silently_discarded() => Invalid(Ledger().Where((_, i) => i != 17).ToArray(), "missingAmounts");
    [Fact] public void Selected_year_cannot_replace_explicit_report_year()
    {
        var path = UrbisLayoutTests.Workbook(Journal());
        try { Assert.Equal(2024, StagedImportRuntime.Import<JournalImport>(path, "IVES", "AJ", 2025).FiscalYear); }
        finally { File.Delete(path); }
    }
    [Fact] public void Unknown_row_is_not_silently_discarded()
    {
        var rows = Journal(); rows[19][1] = "Unexpected row"; Invalid(rows, "unresolvedRow");
    }
    [Fact] public void Extra_populated_cell_is_not_silently_discarded()
    {
        var rows = Journal(); rows[18][6] = "extra"; Invalid(rows, "unexpectedCells");
    }
    [Fact] public void Invalid_amount_blocks_import()
    {
        var rows = Journal(); rows[18][23] = "bad"; Invalid(rows, "invalidAmount");
    }
    [Fact] public void Changed_journal_total_blocks_import()
    {
        var rows = Journal(); rows[20][23] = "13.34"; Invalid(rows, "printedTotalMismatch");
    }
    [Fact] public void Changed_ledger_balance_blocks_import()
    {
        var rows = Ledger(); rows[17][35] = rows[18][35] = "23.34"; Invalid(rows, "balanceMismatch");
    }
    [Fact] public void Multiple_worksheets_are_rejected()
    {
        var path = UrbisLayoutTests.Workbook(Journal(), 2);
        try { Assert.Throws<InvalidDataException>(() => StagedImportRuntime.Import<JournalImport>(path, "IVES", "AJ")); }
        finally { File.Delete(path); }
    }
    [Fact] public void Framework_xlsx_with_xls_extension_has_no_invented_year()
    {
        var rows = new[] { Enumerable.Repeat("", 20).ToArray(), Enumerable.Repeat("", 20).ToArray() };
        foreach (var pair in new Dictionary<int, string> { [0] = "Id_uctu", [1] = "Druh_cinnosti", [4] = "SU", [5] = "aU", [6] = "aU_Text", [7] = "Nazov_uctu", [13] = "Platnost_od" }) rows[0][pair.Key] = pair.Value;
        rows[1][0] = "1"; rows[1][4] = "211"; rows[1][6] = "211. 1 ."; rows[1][7] = "Cash"; rows[1][13] = "1.1.2009";
        var original = UrbisLayoutTests.Workbook(rows); var path = Path.ChangeExtension(original, ".xls"); File.Move(original, path);
        try
        {
            var af = StagedImportRuntime.Import<AccountingFrameworkImport>(path, "IVES", "AF");
            Assert.Equal(0, af.FiscalYear); Assert.Null(af.Ico); Assert.Equal("211.1.", Assert.Single(af.Rows).AccountCode);
        }
        finally { File.Delete(path); }
    }

    // Optional private corpus; no client exports are committed to the repository.
    [Theory]
    [InlineData(2024, 3098, 425)]
    [InlineData(2025, 3008, 433)]
    public void Vojnatina_runtime_reconciles_every_account(int year, int journals, int accounts)
    {
        string? root = Environment.GetEnvironmentVariable("IVES_VOJNATINA_TEST_DIR");
        if (string.IsNullOrWhiteSpace(root)) return;
        var package = AccountingImportCoordinator.CreateDefault().Import(new AccountingImportRequest {
            AccountingFormat = "IVES", ExpectedIco = "00325961", ExpectedFiscalYear = year,
            JournalFilePath = Path.Combine(root, $"U_DENNIK_00325961_{year}.xls"),
            GeneralLedgerFilePath = Path.Combine(root, $"HL_KNIHA_00325961_{year}.xls") });
        Assert.Equal(journals, package.Journal.Rows.Count); Assert.Equal(accounts, package.GeneralLedger.Rows.Count);
        Assert.Equal(0, package.JournalLedgerReconciliation.DifferentAccountCount);
    }

    [Fact] public void Vojnatina_2024_excel_matches_pdf_journal_and_ledger_values()
    {
        string? excel = Environment.GetEnvironmentVariable("IVES_VOJNATINA_TEST_DIR");
        string? pdf = Environment.GetEnvironmentVariable("IVES_VOJNATINA_PDF_DIR");
        if (string.IsNullOrWhiteSpace(excel) || string.IsNullOrWhiteSpace(pdf)) return;
        T Load<T>(string root, string prefix, string extension, string category) where T : class =>
            StagedImportRuntime.Import<T>(Path.Combine(root, prefix + "_00325961_2024." + extension), "IVES", category);
        var ajExcel = Load<JournalImport>(excel, "U_DENNIK", "xls", "AJ");
        var ajPdf = Load<JournalImport>(pdf, "U_DENNIK", "pdf", "AJ");
        Assert.Equal(ajPdf.Rows.Select(r => (r.PostingDate, r.DocumentNumber, r.DebitAccount, r.CreditAccount, r.DebitAmount, r.CreditAmount)),
            ajExcel.Rows.Select(r => (r.PostingDate, r.DocumentNumber, r.DebitAccount, r.CreditAccount, r.DebitAmount, r.CreditAmount)));
        var glExcel = Load<GeneralLedgerImport>(excel, "HL_KNIHA", "xls", "GL");
        var glPdf = Load<GeneralLedgerImport>(pdf, "HL_KNIHA", "pdf", "GL");
        Assert.Equal(glPdf.Rows.OrderBy(r => r.AccountCode).Select(r => (r.AccountCode, r.OpeningDebit, r.OpeningCredit, r.AnnualDebitTurnover, r.AnnualCreditTurnover, r.PeriodDebitTurnover, r.PeriodCreditTurnover, r.ClosingDebit, r.ClosingCredit)),
            glExcel.Rows.OrderBy(r => r.AccountCode).Select(r => (r.AccountCode, r.OpeningDebit, r.OpeningCredit, r.AnnualDebitTurnover, r.AnnualCreditTurnover, r.PeriodDebitTurnover, r.PeriodCreditTurnover, r.ClosingDebit, r.ClosingCredit)));
    }
}
