using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftXmlJournalImporterTests
{
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
            Assert.Contains("XML records 1 and 2", exception.Message);
        }
        finally { File.Delete(path); }
    }
}
