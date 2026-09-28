using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftXmlJournalImporterTests
{
    [Fact]
    public void Imports_kolonica_xml_with_invalid_description_characters()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_XML_JOURNAL_KOLONICA_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_XML_JOURNAL_KOLONICA_TEST_FILE to U_DENNIK_00323161_2022.xml.");
        Assert.True(new IfoSoftXmlJournalImporter().CanImport(path!, "IfoSoft"));
        JournalImport import = new IfoSoftXmlJournalImporter().Import(path!);
        Assert.Equal("00323161", import.Ico);
        Assert.Equal(2022, import.FiscalYear);
        Assert.Equal(4260741.64m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(4260741.64m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(import.ImportReport.Diagnostics, d =>
            d.Code == "IFOSOFT_XML_DESCRIPTION_NORMALIZED" && d.Message.Contains("4 invalid XML characters"));
        Assert.Contains(import.Rows, row => row.Description.Contains("odvod& do VŠZP"));
    }

    [Theory]
    [InlineData(2023, 854, "412470.07")]
    [InlineData(2024, 875, "419767.53")]
    public void Imports_paired_entries_and_checks_export_total(int year, int count, string totalText)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_PDF_GL_AF_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root), "Set IFOSOFT_PDF_GL_AF_TEST_DIR to the IfoSoft sample folder.");
        string path = Path.Combine(root!, $"U_DENNIK_00322857_{year}.xml");
        Assert.True(File.Exists(path), "Missing XML sample: " + path);
        Assert.True(AccountingJournalDetectionService.TryDetect(path, out JournalDetectionResult detection));
        Assert.Equal("IfoSoft", detection.AccountingFormat);
        Assert.Equal("XML", detection.TechnicalType);
        Assert.True(new IfoSoftXmlJournalImporter().CanImport(path, "IfoSoft"));
        Assert.False(new IfoSoftXmlJournalImporter().CanImport(path, "IVES"));

        JournalImport result = new IfoSoftXmlJournalImporter().Import(path);
        Assert.Equal("00322857", result.Ico);
        Assert.Equal(year, result.FiscalYear);
        Assert.Equal(count, result.Rows.Count);
        decimal total = decimal.Parse(totalText, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(total, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(total, result.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(result.Rows, row => row.RecordKind == JournalRecordKind.Opening);
        Assert.Contains(result.Rows, row => row.DebitAccount == "357ŽP" || row.CreditAccount == "357ŽP");
    }

    [Fact]
    public void Rejects_a_broken_pair_even_when_the_document_is_well_formed()
    {
        string path = Path.Combine(Path.GetTempPath(), "U_DENNIK_00322857_2023.xml");
        try
        {
            File.WriteAllText(path, "<uctovny_vykaz><typDoc>UCT_VETA</typDoc><obdobie><rok>2023</rok></obdobie>" +
                "<identifikacia><identifikator><ico>00322857</ico></identifikator></identifikacia><vety>" +
                "<veta><ucDok>1</ucDok><ucSuv>221</ucSuv><ucPripDat>01.02.2023</ucPripDat><rok>2023</rok><mes>02</mes><md>10,00</md></veta>" +
                "<veta><ucDok>1</ucDok><ucSuv>321</ucSuv><ucPripDat>01.02.2023</ucPripDat><rok>2023</rok><mes>02</mes><dal>9,00</dal></veta>" +
                "</vety><sucetKontrola>12,00</sucetKontrola></uctovny_vykaz>");
            var exception = Assert.Throws<InvalidDataException>(() => new IfoSoftXmlJournalImporter().Import(path));
            Assert.Contains("control total", exception.Message);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(2022, "3688552.01", 7707)]
    [InlineData(2023, "4848547.91", 8501)]
    public void Imports_multi_line_Hankovce_entries(int year, string totalText, int sourceRecords)
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_XML_JOURNAL_TEST_DIR");
        Assert.True(!string.IsNullOrWhiteSpace(root), "Set IFOSOFT_XML_JOURNAL_TEST_DIR to the Hankovce sample folder.");
        string path = Path.Combine(root!, $"U_DENNIK_00322962_{year}.xml");
        Assert.True(File.Exists(path), "Missing XML sample: " + path);
        Assert.True(AccountingJournalDetectionService.TryDetect(path, out JournalDetectionResult detection));
        Assert.Equal(year, detection.FiscalYear);

        JournalImport result = new IfoSoftXmlJournalImporter().Import(path);
        decimal total = decimal.Parse(totalText, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("00322962", result.Ico);
        Assert.Equal(total, result.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(total, result.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.True(result.Rows.Count > sourceRecords / 2);
        Assert.True(result.Rows.Count < sourceRecords);
        Assert.Contains(result.Rows, row => row.DebitAmount.HasValue && !row.CreditAmount.HasValue);
        Assert.Contains(result.Rows, row => row.CreditAmount.HasValue && !row.DebitAmount.HasValue);
        if (year == 2022)
            Assert.Contains(result.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_XML_EXPORT_PERIOD_DIFFERS");
        else
            Assert.Contains(result.Rows, row => row.DebitAccount == "518162" && row.TextNormalizationApplied);
    }

    [Fact]
    public void Imports_hudcovce_accounts_with_hyphens_and_next_year_postings()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_XML_JOURNAL_HUDCOVCE_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_XML_JOURNAL_HUDCOVCE_TEST_FILE to U_DENNIK_00323012_2023.xml.");

        JournalImport import = new IfoSoftXmlJournalImporter().Import(path!);
        Assert.Equal("00323012", import.Ico);
        Assert.Equal(2023, import.FiscalYear);
        Assert.Contains(import.Rows, row => row.DebitAccount == "042MŠ-U" && row.DebitAmount == 200m);
        Assert.Contains(import.Rows, row => row.PostingDate.Year == 2024);
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_XML_NEXT_YEAR_POSTINGS");
        Assert.Equal(4059859.81m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(4059859.81m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
    }
}
