using ExcelApiPoc.AccountingImport.Services.Layouts;
namespace PdfLayoutEngine.SimpleTests;

public sealed class ImportMetadataResolverTests
{
    [Fact]
    public void Ledger_contents_beat_journal_filename_and_values_resolve_independently()
    {
        var value = ImportMetadataResolver.Resolve(new[] {
            new ImportMetadataCandidate { Path = "U_DENNIK_11111111_2023.csv", Ico = "22222222" },
            new ImportMetadataCandidate { Path = "HL_KNIHA_33333333_2022.csv", Ico = "44444444", FiscalYear = 2024 },
            new ImportMetadataCandidate { Ico = "55555555", FiscalYear = 2025 }
        });
        Assert.Equal("22222222", value.Ico);
        Assert.Equal(2024, value.FiscalYear);
    }

    [Fact]
    public void Framework_contents_beat_all_filenames()
    {
        var value = ImportMetadataResolver.Resolve(new[] {
            new ImportMetadataCandidate { Path = "U_DENNIK_11111111_2023.csv" },
            new ImportMetadataCandidate { Path = "HL_KNIHA_22222222_2022.csv" },
            new ImportMetadataCandidate { Ico = "33333333", FiscalYear = 2024 }
        });
        Assert.Equal("33333333", value.Ico);
        Assert.Equal(2024, value.FiscalYear);
    }

    [Theory]
    [InlineData("U_DENNIK_00323217_202512.DBF", "00323217", 2025)]
    [InlineData("U_DENNIK_00322792_202414.CSV", "00322792", 2024)]
    public void Journal_filename_is_fallback(string path, string ico, int year)
    {
        var value = ImportMetadataResolver.Resolve(new[] {
            new ImportMetadataCandidate { Path = path },
            new ImportMetadataCandidate { Path = "HL_KNIHA_11111111_2023.csv" }
        });
        Assert.Equal(ico, value.Ico);
        Assert.Equal(year, value.FiscalYear);
    }

    [Fact]
    public void Unavailable_values_remain_unknown()
    {
        var value = ImportMetadataResolver.Resolve(new[] { new ImportMetadataCandidate { Path = "journal.csv" } });
        Assert.Null(value.Ico);
        Assert.Null(value.FiscalYear);
    }
}
