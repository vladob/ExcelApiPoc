using System.Text;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

internal sealed class TemporaryCsvFile : IDisposable
{
    static TemporaryCsvFile()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public TemporaryCsvFile(IEnumerable<string> lines)
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "ExcelApiPoc-" + Guid.NewGuid().ToString("N") + ".csv");

        File.WriteAllLines(Path, lines, Encoding.GetEncoding(1250));
    }

    public string Path { get; }

    public void Dispose()
    {
        if (File.Exists(Path))
        {
            File.Delete(Path);
        }
    }
}
