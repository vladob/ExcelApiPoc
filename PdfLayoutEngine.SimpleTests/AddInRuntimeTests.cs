using System.Text;
using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;

public sealed class AddInRuntimeTests
{
    const string Header = "DD;CisloD;Datum;Popis operacie;Ucet_MD;Pol_MD;Zdr_MD;Str_MD;Zak_MD;Suma_MD;Ucet_D;Odd_D;Pol_D;Zdr_D;Str_D;Zak_D;Suma_D";
    static string Save(string text, string name)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, text, Encoding.GetEncoding(1250));
        return path;
    }
    static string Journal(string description, string amount = "12,34") =>
        "00323110 Test\nUctovny dennik\n" + Header + "\n1;1;01.01.2024;" + description + ";211;;;;;" + amount + ";321;;;;;;" + amount + "\n";
    static void Delete(string path) => Directory.Delete(Path.GetDirectoryName(path)!, true);
    static AccountingImportRequest Request(string path) => new()
    {
        AccountingFormat = "IfoSoft", JournalFilePath = path,
        ExpectedIco = "00323110", ExpectedFiscalYear = 2024
    };

    [Fact] public void Staged_contents_override_misleading_filename()
    {
        var path = Save(Journal("text"), "U_DENNIK_99999999_2023.csv");
        try
        {
            var result = AccountingImportCoordinator.CreateDefault().Import(Request(path));
            Assert.Equal("00323110", result.Journal.Ico);
            Assert.Equal(2024, result.Journal.FiscalYear);
        }
        finally { Delete(path); }
    }

    [Fact] public void Confirmed_year_excludes_only_out_of_year_rows()
    {
        var text = Journal("valid") + "1;2;01.01.3025;bad year;211;;;;;1,00;321;;;;;;1,00\n";
        var path = Save(text, "U_DENNIK_00323110_2024.csv");
        try
        {
            var result = AccountingImportCoordinator.CreateDefault().Import(Request(path));
            Assert.Equal(2, result.Journal.Rows.Count);
            Assert.Single(result.Journal.Rows, r => r.DateExceptionResolution == JournalDateExceptionResolution.Excluded);
        }
        finally { Delete(path); }
    }

    [Fact] public void Legacy_journal_regression_resource_is_available()
    {
        using var resource = typeof(StagedImportRuntime).Assembly.GetManifestResourceStream(
            "ExcelApiPoc.AccountingImport.PdfLayouts.IfoSoft.journal-dennik1.v1.json");
        Assert.NotNull(resource);
    }

    [Fact] public void Runtime_catalogue_is_embedded_and_complete()
    {
        var assembly = typeof(StagedImportRuntime).Assembly;
        var names = assembly.GetManifestResourceNames().Where(n => n.StartsWith("StagedLayouts/", StringComparison.Ordinal)).ToArray();
        Assert.True(names.Length > 40);
        Assert.Equal(names.Length, LayoutFiles.Validate(StagedImportRuntime.CatalogueDirectory));
        foreach (var producer in new[] { "IfoSoft", "IVES", "Urbis", "Softip-MOP", "KROS OMEGA" })
            Assert.NotEmpty(LayoutFiles.All(StagedImportRuntime.ProducerDirectory(producer)));
    }

    [Fact] public void Default_coordinator_imports_staged_general_ledger()
    {
        var journal = Save(Journal("text"), "U_DENNIK_00323110_2024.csv");
        var ledger = Save("00323110 Test\nHlavna kniha k 12/2024\nSyn;Ana;Typ;P;Odd;Polozka;KZdroja;Program;Stred;Zakaz;Nazov uctu;Poc_M;Poc_D;Roc_M;Roc_D;12/2024;12/2024;Kon_M;Kon_D;Plan\n211;;M;;;;;;;;Cash;0;0;12,34;0;12,34;0;12,34;0;0\n321;;P;;;;;;;;Supplier;0;0;0;12,34;0;12,34;0;12,34;0\n", "HL_KNIHA_00323110_2024.csv");
        try
        {
            var request = Request(journal); request.GeneralLedgerFilePath = ledger;
            var package = AccountingImportCoordinator.CreateDefault().Import(request);
            Assert.Equal(2, package.GeneralLedger.Rows.Count);
            Assert.Contains(package.GeneralLedger.ImportReport.Diagnostics, d => d.Code == "STAGED_LAYOUT" && d.Message.Contains("ifosoft-csv-gl"));
        }
        finally { Delete(journal); Delete(ledger); }
    }

    [Fact] public void Default_detection_identifies_staged_xml_from_content()
    {
        var path = Save("<uctovny_vykaz><typDoc>UCT_VETA</typDoc><identifikacia><identifikator><ico>00323110</ico><nazov>Test</nazov></identifikator></identifikacia><obdobie><rok>2024</rok></obdobie><vety><veta><ucSuv>211</ucSuv><ucPripDat>01.01.2024</ucPripDat><rok>2024</rok><mes>01</mes><md>1,00</md><dal>1,00</dal></veta></vety><sucetKontrola>3,00</sucetKontrola></uctovny_vykaz>", "source.xml");
        try
        {
            Assert.True(AccountingJournalDetectionService.TryDetect(path, out var result));
            Assert.Equal("IfoSoft", result.AccountingFormat);
            Assert.Equal("00323110", result.Ico); Assert.Equal(2024, result.FiscalYear);
        }
        finally { Delete(path); }
    }

    [Theory]
    [InlineData("\"Gym ZŠ\"\"", "Gym ZŠ\"")]
    [InlineData("\"Gym ZŠ\"-2.-3.etapa\"", "Gym ZŠ\"-2.-3.etapa")]
    [InlineData("\"Gym \"\"ZŠ\"\"\"", "Gym \"ZŠ\"")]
    [InlineData("\"Gym \"\"A\"\"; main\"", "Gym \"A\"; main")]
    [InlineData("\"two\nlines\"", "two\r\nlines")]
    public void Default_coordinator_uses_staged_parser_and_preserves_source_text(string csv, string expected)
    {
        var path = Save(Journal(csv), "U_DENNIK_00323110_2024.csv");
        try
        {
            var package = AccountingImportCoordinator.CreateDefault().Import(Request(path));
            var row = Assert.Single(package.Journal.Rows);
            Assert.Equal(expected, row.SourceFields["Description"]);
            Assert.Equal(12.34m, row.DebitAmount);
            Assert.Contains(package.Journal.ImportReport.Diagnostics, d => d.Code == "STAGED_LAYOUT" && d.Message.Contains("ifosoft-csv-aj"));
        }
        finally { Delete(path); }
    }

    [Fact] public void Multiple_journals_preserve_provenance_and_combine_amounts()
    {
        var first = Save(Journal("first"), "first.csv");
        var second = Save(Journal("second", "23,45"), "second.csv");
        try
        {
            var request = Request(first); request.JournalFilePaths.AddRange(new[] { first, second });
            var result = AccountingImportCoordinator.CreateDefault().Import(request);
            Assert.Equal(2, result.Journal.Rows.Count);
            Assert.Equal(35.79m, result.Journal.Rows.Sum(r => r.DebitAmount ?? 0));
            Assert.Equal(new[] { 1, 2 }, result.Journal.Rows.Select(r => r.SequenceNumber));
            Assert.Equal(new[] { "first.csv", "second.csv" }, result.Journal.Rows.Select(r => r.SourceFields["SourceFile"]));
            Assert.All(result.Journal.Rows, r => Assert.Contains(r.SourceFields["SourceFile"], r.SourceLocation));
            Assert.Equal(2, result.Journal.ImportReport.RecordCounts["SourceFiles"]);
        }
        finally { Delete(first); Delete(second); }
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("entity")]
    [InlineData("year")]
    [InlineData("amount")]
    public void Multiple_journals_reject_invalid_or_duplicate_part(string problem)
    {
        var first = Save(Journal("first"), "first.csv");
        string text = Journal("first");
        if (problem == "entity") text = text.Replace("00323110", "99999999");
        if (problem == "year") text = text.Replace("2024", "2023");
        if (problem == "amount") text = text.Replace("12,34", "invalid");
        var second = Save(text, "renamed.csv");
        try
        {
            var request = Request(first); request.JournalFilePaths.AddRange(new[] { first, second });
            Assert.Throws<InvalidDataException>(() => AccountingImportCoordinator.CreateDefault().Import(request));
        }
        finally { Delete(first); Delete(second); }
    }

    [Fact] public void Ambiguous_global_detection_does_not_guess_a_producer()
    {
        var path = Save(Journal("text"), "U_DENNIK_00323110_2024.csv");
        try { Assert.False(AccountingJournalDetectionService.TryDetect(path, out var result)); Assert.Null(result); }
        finally { Delete(path); }
    }

    [Fact] public void Invalid_amount_cannot_fall_back_to_legacy_import()
    {
        var path = Save(Journal("text", "wrong"), "U_DENNIK_00323110_2024.csv");
        try { Assert.Throws<InvalidDataException>(() => AccountingImportCoordinator.CreateDefault().Import(Request(path))); }
        finally { Delete(path); }
    }

    [Fact] public void Source_year_mismatch_is_not_overwritten_by_requested_year()
    {
        var path = Save(Journal("text"), "journal.csv");
        try
        {
            var request = Request(path); request.ExpectedFiscalYear = 2025;
            Assert.Throws<InvalidDataException>(() => AccountingImportCoordinator.CreateDefault().Import(request));
        }
        finally { Delete(path); }
    }

    [Fact] public void Malformed_record_reports_file_and_physical_line()
    {
        var path = Save(Journal("\"unfinished"), "journal.csv");
        try
        {
            var error = Assert.Throws<InvalidDataException>(() => StagedImportRuntime.Import<JournalImport>(path, "IfoSoft", "AJ"));
            Assert.Contains("journal.csv", error.Message); Assert.Contains("4", error.Message);
        }
        finally { Delete(path); }
    }

    [Theory]
    [InlineData("\"Gym ZŠ\"\"", "Gym ZŠ\"")]
    [InlineData("\"Gym ZŠ\"-2.-3.etapa\"", "Gym ZŠ\"-2.-3.etapa")]
    [InlineData("\"Gym \"\"ZŠ\"\"\"", "Gym \"ZŠ\"")]
    [InlineData("\"Gym \"\"A\"\"; main\"", "Gym \"A\"; main")]
    public void Framework_uses_staged_name_recovery(string csv, string expected)
    {
        var path = Save("00323110 Test\nUctovny rozvrh 2024\nSyn;Ana;Nazov uctu;Typ;Pods;Dan;Saldo;DPH\n021;915;" + csv + ";M;;;;\n", "accounts.csv");
        try
        {
            var framework = StagedImportRuntime.Import<AccountingFrameworkImport>(path, "IfoSoft", "AF");
            Assert.Equal(expected, Assert.Single(framework.Rows).AccountName);
        }
        finally { Delete(path); }
    }
}
