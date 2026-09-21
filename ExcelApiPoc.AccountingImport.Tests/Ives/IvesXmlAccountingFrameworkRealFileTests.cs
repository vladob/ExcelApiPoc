using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXmlAccountingFrameworkRealFileTests
{
    private readonly ITestOutputHelper output;

    public IvesXmlAccountingFrameworkRealFileTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Parse_real_xml_from_environment_variable()
    {
        const string variable = "IVES_ACCOUNTING_FRAMEWORK_XML_TEST_FILE";
        string? path = Environment.GetEnvironmentVariable(variable);

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            output.WriteLine(variable + " is not set to an existing file. Diagnostic skipped.");
            return;
        }

        IvesAccountingFrameworkParseResult result =
            new IvesXmlAccountingFrameworkParser().Parse(path);

        Assert.Equal("00325465", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(3203, result.SourceRowCount);
        Assert.Equal(3203, result.Rows.Count);

        IvesAccountingFrameworkSourceRow first = result.Rows[0];
        Assert.Equal("021.1    .      .    .       .   .š.", first.SourceAccountCode);
        Assert.Equal("021.1.....š.", first.AccountCode);
        Assert.Equal("Budovy", first.AccountName);
        Assert.Equal("H", first.ActivityCode);
        Assert.Equal("A", first.Type);
        Assert.Equal("EUR", first.Currency);
        Assert.Equal(new DateTime(2012, 9, 1), first.ValidFrom);
        Assert.Null(first.ValidTo);

        IvesAccountingFrameworkSourceRow last = result.Rows[result.Rows.Count - 1];
        Assert.Equal("799.9......", last.AccountCode);
        Assert.Equal("P", last.ActivityCode);
        Assert.Equal("X", last.Type);
        Assert.Equal("EUR", last.Currency);
        Assert.Equal(new DateTime(2008, 1, 1), last.ValidFrom);
        Assert.Null(last.ValidTo);

        output.WriteLine("IVES XML accounting-framework parser verified.");
        output.WriteLine("Rows          : " + result.Rows.Count);
        output.WriteLine("First account : " + first.AccountCode);
        output.WriteLine("Last account  : " + last.AccountCode);
    }
}
