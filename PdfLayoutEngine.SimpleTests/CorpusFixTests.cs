using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public sealed class CorpusFixTests
{
    // Optional real-file regressions; source PDFs are not committed.
    [Theory]
    [InlineData("U_DENNIK_00325121_2025(1).pdf",6633,0)]
    [InlineData("U_DENNIK_00323195_2025(1).pdf",2142,11)]
    public void Journal_records_are_retained(string file,int count,int excluded)
    {
        var root=Environment.GetEnvironmentVariable("IFOSOFT_FIX2_TEST_ROOT");if(root==null)return;
        using var importer=new CompactLayoutImporter(Path.Combine(root,file),Path.Combine(AppContext.BaseDirectory,"Compact"));
        var result=importer.Examine(ImportLevel.Normalize);Assert.Equal("completed",result.Status);
        var journal=Assert.IsType<JournalImport>(importer.Canonical);Assert.Equal(count,journal.Rows.Count);
        Assert.Equal(excluded,journal.Rows.Count(r=>!r.UsedForReportCalculation));
    }
    [Fact]public void Blank_page_stops_import_and_retry_does_not_bypass_failure()
    {
        var root=Environment.GetEnvironmentVariable("IFOSOFT_FIX2_TEST_ROOT");if(root==null)return;
        using var importer=new CompactLayoutImporter(Path.Combine(root,"HL_KNIHA_00325287_2022_a.pdf"),Path.Combine(AppContext.BaseDirectory,"Compact"));
        for(int n=0;n<2;n++){
            var result=importer.Examine(ImportLevel.Normalize);Assert.Equal("invalid",result.Status);Assert.Equal(2,result.DecodedPageCount);
            Assert.Contains(result.Issues,i=>i.Code=="criticalPageContentMissing"&&i.Page==2);Assert.Null(importer.Canonical);
        }
    }
    [Fact]public void Account_plan_excerpt_preserves_coverage_warning()
    {
        var root=Environment.GetEnvironmentVariable("IFOSOFT_FIX2_TEST_ROOT");if(root==null)return;
        using var importer=new CompactLayoutImporter(Path.Combine(root,"UCT_ROZVRH_00325503_2022_b.pdf"),Path.Combine(AppContext.BaseDirectory,"Compact"));
        var result=importer.Examine(ImportLevel.Normalize);Assert.Equal("completed",result.Status);
        Assert.Contains(result.Issues,i=>i.Code=="partialReport"&&i.Severity=="warning");Assert.Equal(26,Assert.IsType<AccountingFrameworkImport>(importer.Canonical).Rows.Count);
    }
}
