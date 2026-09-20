using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Services.Ives;
using ExcelDataReader;
using System.Text;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesTabularJournalStructureDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesTabularJournalStructureDiagnosticTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Show_csv_rows_around_first_transaction_without_module()
    {
        string? path = Environment.GetEnvironmentVariable(
            "IVES_JOURNAL_CSV_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine("IVES_JOURNAL_CSV_TEST_FILE is not set.");
            return;
        }

        IvesJournalParseResult parsed =
            new IvesCsvJournalParser().Parse(path);

        IvesJournalSourceRow? row = parsed.TransactionRows
            .FirstOrDefault(item => string.IsNullOrWhiteSpace(item.Module));

        Assert.NotNull(row);

        Encoding encoding = DetectEncoding(path);
        string[] lines = File.ReadAllLines(path, encoding);

        output.WriteLine(
            "First transaction without module: sequence={0}, sourceRow={1}, document={2}",
            row!.SequenceNumber,
            row.SourceRowNumber,
            row.DocumentNumber);

        int start = Math.Max(1, row.SourceRowNumber);
        int end = Math.Min(lines.Length, row.SourceRowNumber + 2);

        for (int lineNumber = start; lineNumber <= end; lineNumber++)
        {
            string[] values = ParseCsvLine(lines[lineNumber - 1]);
            DumpValues("CSV", lineNumber, values);
        }
    }

    [Fact]
    public void Show_xlsx_rows_around_first_transaction_without_module()
    {
        string? path = Environment.GetEnvironmentVariable(
            "IVES_JOURNAL_XLSX_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine("IVES_JOURNAL_XLSX_TEST_FILE is not set.");
            return;
        }

        IvesJournalParseResult parsed =
            new IvesXlsxJournalParser().Parse(path);

        IvesJournalSourceRow? row = parsed.TransactionRows
            .FirstOrDefault(item => string.IsNullOrWhiteSpace(item.Module));

        Assert.NotNull(row);

        output.WriteLine(
            "First transaction without module: sequence={0}, sourceRow={1}, document={2}",
            row!.SequenceNumber,
            row.SourceRowNumber,
            row.DocumentNumber);

        using IExcelDataReader reader = ExcelWorkbookReader.Open(path);

        int currentRow = 0;
        int start = Math.Max(1, row.SourceRowNumber);
        int end = row.SourceRowNumber + 2;

        while (reader.Read())
        {
            currentRow++;

            if (currentRow < start)
                continue;

            if (currentRow > end)
                break;

            string[] values = new string[reader.FieldCount];

            for (int column = 0; column < reader.FieldCount; column++)
            {
                values[column] =
                    ExcelWorkbookReader.GetText(reader, column);
            }

            DumpValues("XLSX", currentRow, values);
        }
    }

    private void DumpValues(
        string source,
        int rowNumber,
        string[] values)
    {
        output.WriteLine("");
        output.WriteLine("{0} physical row {1}", source, rowNumber);

        for (int index = 0; index < values.Length; index++)
        {
            if (!string.IsNullOrWhiteSpace(values[index]))
            {
                output.WriteLine(
                    "  [{0}] = <{1}>",
                    index,
                    values[index]);
            }
        }
    }

    private static Encoding DetectEncoding(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var utf8 = new UTF8Encoding(false, true);

        try
        {
            File.ReadAllText(path, utf8);
            return new UTF8Encoding(false);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1250);
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        char delimiter = CountOutsideQuotes(line, ';') >
                         CountOutsideQuotes(line, ',')
            ? ';'
            : ',';

        var values = new List<string>();
        var value = new StringBuilder();
        bool quoted = false;

        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];

            if (character == '"')
            {
                if (quoted &&
                    index + 1 < line.Length &&
                    line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }

                continue;
            }

            if (!quoted && character == delimiter)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(character);
        }

        values.Add(value.ToString());
        return values.ToArray();
    }

    private static int CountOutsideQuotes(
        string line,
        char delimiter)
    {
        int count = 0;
        bool quoted = false;

        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];

            if (character == '"')
            {
                if (quoted &&
                    index + 1 < line.Length &&
                    line[index + 1] == '"')
                {
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (!quoted && character == delimiter)
            {
                count++;
            }
        }

        return count;
    }
}
