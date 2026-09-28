using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AddIn.Services;
using System.Text;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftPreviouslyImportedFileTests(ITestOutputHelper output)
{
    [Fact]
    public void Every_listed_file_still_parses()
    {
        string? root = Environment.GetEnvironmentVariable("IFOSOFT_IMPORT_REGRESSION_ROOT");
        Assert.True(!string.IsNullOrWhiteSpace(root),
            "Set IFOSOFT_IMPORT_REGRESSION_ROOT to run the local IfoSoft file list.");

        string manifest = Environment.GetEnvironmentVariable("IFOSOFT_IMPORT_REGRESSION_LIST") ??
            Path.Combine(AppContext.BaseDirectory, "TestData", "IfoSoft", "PreviouslyImportedPaths.txt");
        Assert.True(File.Exists(manifest), "Missing IfoSoft regression list: " + manifest);
        Assert.True(Directory.Exists(root), "Missing IfoSoft regression root: " + root);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        string[] entries = File.ReadAllLines(manifest).Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(entries);
        Assert.Equal(entries.Length, entries.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var failures = new List<string>();
        int passed = 0;
        foreach (string entry in entries)
        {
            string path = Path.IsPathRooted(entry) ? entry :
                Path.Combine(root, entry.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                Assert.True(File.Exists(path), "File not found: " + path);
                int rows = Parse(path);
                Assert.True(rows > 0, "Importer returned no rows: " + path);
                passed++;
                output.WriteLine("OK " + entry + " (" + rows + " rows)");
            }
            catch (Exception ex)
            {
                failures.Add(entry + ": " + ex.GetType().Name + ": " + ex.Message);
                output.WriteLine("FAIL " + failures[^1]);
            }
        }

        Assert.True(failures.Count == 0,
            $"{passed} of {entries.Length} files parsed; {failures.Count} failed:\n" +
            string.Join("\n", failures));
    }

    private static int Parse(string path)
    {
        string name = Path.GetFileName(path);
        string ext = Path.GetExtension(path);
        if (name.StartsWith("U_DENNIK_", StringComparison.OrdinalIgnoreCase))
        {
            if (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftXmlJournalImporter().Import(path).Rows.Count;
            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftPdfDennik1JournalImporter().Import(path).Rows.Count;
            if (ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftCsvJournalImporter().Import(path).Rows.Count;
        }
        if (name.StartsWith("HL_KNIHA_", StringComparison.OrdinalIgnoreCase))
        {
            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftPdfGeneralLedgerImporter().Import(path).Rows.Count;
            if (ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftCsvGeneralLedgerImporter().Import(path).Rows.Count;
        }
        if (name.StartsWith("UCT_ROZVRH_", StringComparison.OrdinalIgnoreCase))
        {
            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftPdfAccountingFrameworkImporter().Import(path).Rows.Count;
            if (ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return new IfoSoftCsvAccountingFrameworkImporter().Import(path).Rows.Count;
        }
        throw new NotSupportedException("No IfoSoft regression parser for " + name);
    }
}
