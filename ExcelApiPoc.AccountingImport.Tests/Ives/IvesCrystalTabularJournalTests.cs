using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesCrystalTabularJournalTests
{
    [Fact]
    public void CsvParser_MapsFlattenedCrystalRowsThroughSharedDecoder()
    {
        string path = CreateSyntheticCsv();

        try
        {
            IvesJournalParseResult parsed =
                new IvesCsvJournalParser().Parse(path);

            Assert.Equal("00999999", parsed.Ico);
            Assert.Equal(2025, parsed.FiscalYear);
            Assert.Equal(2, parsed.TransactionRows.Count);
            Assert.Single(parsed.ReportTotalRows);
            Assert.Equal(
                150m,
                parsed.ReportTotalRows[0].ReportedAmounts.Single());

            IvesJournalSourceRow first = parsed.TransactionRows[0];
            Assert.Equal(new DateTime(2025, 1, 2), first.PostingDate);
            Assert.Equal("TEST001", first.DocumentNumber);
            Assert.Equal("518.100", first.DebitCompositeAccount);
            Assert.Equal("321.200", first.CreditCompositeAccount);
            Assert.Equal(100m, first.Amount);
            Assert.Equal("DOD", first.Module);

            JournalImport imported =
                new IvesJournalImporter().Import(path);

            Assert.Equal("CSV", imported.TechnicalType);
            Assert.True(imported.ImportReport.IsValid);
            Assert.Equal(2, imported.Rows.Count);
        }
        finally
        {
            DeleteSynthetic(path);
        }
    }


    [Fact]
    public void SharedDecoder_PairsModuleFromFollowingPhysicalRow()
    {
        string fileName = "U_DENNIK_00999999_2025.csv";
        string path = Path.Combine(Path.GetTempPath(), fileName);

        var rows = new[]
        {
            Row(
                1,
                fileName,
                "2025-01-02", "TEST001", "518.100", "321.200",
                "100.00", "€", "Synthetic first"),
            Row(
                2,
                fileName,
                "", "", "", "", "", "", "DOD", "4", "/ 250001")
        };

        IvesJournalParseResult parsed =
            new IvesCrystalTabularJournalDecoder().Decode(path, rows);

        IvesJournalSourceRow transaction =
            Assert.Single(parsed.TransactionRows);

        Assert.Equal("DOD", transaction.Module);
        Assert.Equal(2, transaction.RelatedSourceRowNumber);
    }

    [Fact]
    public void SharedDecoder_AcceptsRowsProducedBySpreadsheetReader()
    {
        string fileName = "U_DENNIK_00999999_2025.xlsx";
        string path = Path.Combine(Path.GetTempPath(), fileName);

        var rows = new[]
        {
            Row(
                1,
                fileName,
                "Obec Test", "IČO:00999999", "Účtovný denník",
                "Dátum", "Čís. dokladu", "Účet MD", "Účet D", "Suma", "Mena", "Text",
                "2025-01-02", "TEST001", "518.100", "321.200", "100.00", "€", "Synthetic first",
                "", "", "", "", "DOD", "4", "/ 250001",
                "Spolu :", "150.00", "€"),
            Row(
                2,
                fileName,
                "Obec Test", "IČO:00999999", "Účtovný denník",
                "Dátum", "Čís. dokladu", "Účet MD", "Účet D", "Suma", "Mena", "Text",
                "2025-01-03", "TEST002", "321.200", "518.100", "50.00", "€", "Synthetic second",
                "", "", "", "", "DOD", "4", "/ 250002",
                "Spolu :", "150.00", "€")
        };

        IvesJournalParseResult parsed =
            new IvesCrystalTabularJournalDecoder().Decode(path, rows);

        Assert.Equal(2, parsed.TransactionRows.Count);
        Assert.Equal("00999999", parsed.Ico);
        Assert.Equal(150m, parsed.ReportTotalRows.Single().ReportedAmounts.Single());
        Assert.Equal("DOD", parsed.TransactionRows[1].Module);
    }

    private static IvesCrystalTabularRow Row(
        int number,
        string fileName,
        params string[] values)
    {
        return new IvesCrystalTabularRow
        {
            SourceRecordNumber = number,
            SourceLocation = fileName + ", row " + number,
            Values = values
        };
    }

    private static string CreateSyntheticCsv()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "U_DENNIK_00999999_2025.csv");

        string[] rows =
        {
            CsvRow(
                "Obec Test", "IČO:00999999", "Účtovný denník",
                "Dátum", "Čís. dokladu", "Účet MD", "Účet D", "Suma", "Mena", "Text",
                "02.01.2025", "TEST001", "518.100", "321.200", "100,00", "€", "Synthetic first",
                "", "", "", "", "DOD", "4", "/ 250001",
                "Spolu :", "100,00", "€"),
            CsvRow(
                "Obec Test", "IČO:00999999", "Účtovný denník",
                "Dátum", "Čís. dokladu", "Účet MD", "Účet D", "Suma", "Mena", "Text",
                "03.01.2025", "TEST002", "321.200", "518.100", "50,00", "€", "Synthetic second",
                "", "", "", "", "UCT", "", "",
                "Spolu :", "50,00", "€")
        };

        File.WriteAllLines(
            path,
            rows,
            new UTF8Encoding(false));

        return path;
    }

    private static string CsvRow(params string[] values)
    {
        return string.Join(
            ",",
            values.Select(value =>
                "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\""));
    }

    private static void DeleteSynthetic(string path)
    {
        string directory = Path.GetDirectoryName(path)!;

        if (File.Exists(path))
            File.Delete(path);

        if (Directory.Exists(directory))
            Directory.Delete(directory);
    }
}
