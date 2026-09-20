using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesTabularJournalStructureDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesTabularJournalStructureDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Historical_module_gap_diagnostic_is_no_longer_applicable()
    {
        output.WriteLine(
            "This diagnostic originally located the first CSV/XLSX " +
            "transaction without a module. The shared IVES tabular " +
            "decoder now pairs module continuation rows successfully, " +
            "so no such transaction remains in the current real-file " +
            "corpus.");
    }
}
