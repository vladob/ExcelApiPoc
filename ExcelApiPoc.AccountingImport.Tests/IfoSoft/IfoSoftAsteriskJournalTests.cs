using ExcelApiPoc.AccountingImport.Services.IfoSoft;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftAsteriskJournalTests
{
    [Fact]
    public void Xml_2024_preserves_postings_to_asterisk_accounts()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_XML_JOURNAL_00325155_2024_TEST_FILE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), "Missing journal: " + path);
        var result = new IfoSoftXmlJournalImporter().Import(path);
        Assert.Equal(1264.04m, result.Rows.Where(row => row.DebitAccount == "357***")
            .Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1264.04m, result.Rows.Where(row => row.CreditAccount == "693**")
            .Sum(row => row.CreditAmount ?? 0m));
    }
}
