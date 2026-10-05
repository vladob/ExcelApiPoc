using ExcelApiPoc.AccountingImport.Services.Layouts;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
namespace PdfLayoutEngine.SimpleTests;
public sealed class EmptyAccountTests
{
    [Theory]
    [InlineData("UCT_ROZVRH_00325805_2023.pdf")]
    [InlineData("UCT_ROZVRH_00325805_2024.pdf")]
    [InlineData("UCT_ROZVRH_00325805_2025.pdf")]
    public void Named_placeholder_is_diagnostic_only(string name)
    {
        var root=Environment.GetEnvironmentVariable("IFOSOFT_AF_PLACEHOLDER_TEST_ROOT");if(root==null)return;
        using var importer=new CompactLayoutImporter(Path.Combine(root,name),Path.Combine(AppContext.BaseDirectory,"Compact"));
        var r=importer.Examine(ImportLevel.Normalize);
        Assert.Equal("completed",r.Status);
        var placeholder=Assert.Single(r.Rows,x=>x.Kind=="EmptyAccount"&&x.Name=="Prázdny účet");
        Assert.Equal(1,placeholder.Page);
        var af=Assert.IsType<AccountingFrameworkImport>(importer.Canonical);
        Assert.NotEmpty(af.Rows);
        Assert.DoesNotContain(af.Rows,x=>x.AccountName=="Prázdny účet");
        Assert.All(af.Rows,x=>Assert.False(string.IsNullOrWhiteSpace(x.SyntheticCode)));
    }
}
