using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

public sealed class JournalWorksheetDataProjectorTests
{
    [Fact]
    public void CreateHeaderValues_PreservesExistingWorksheetSchema()
    {
        object[,] values = JournalWorksheetDataProjector.CreateHeaderValues();

        Assert.Equal(1, values.GetLength(0));
        Assert.Equal(26, values.GetLength(1));
        Assert.Equal("SequenceNumber", values[0, 0]);
        Assert.Equal("DebitAccount", values[0, 5]);
        Assert.Equal("CreditAmount", values[0, 13]);
        Assert.Equal("TextNormalizationApplied", values[0, 25]);
    }

    [Fact]
    public void CreateDataValues_ProjectsRequestedSliceAndAllColumns()
    {
        JournalRow first = CreateRow(1, "FIRST");
        JournalRow second = CreateRow(2, "SECOND");
        JournalRow third = CreateRow(3, "THIRD");

        object[,] values = JournalWorksheetDataProjector.CreateDataValues(
            [first, second, third], 1, 1);

        Assert.Equal(1, values.GetLength(0));
        Assert.Equal(26, values.GetLength(1));
        Assert.Equal(2, values[0, 0]);
        Assert.Equal(new DateTime(2025, 2, 3), values[0, 1]);
        Assert.Equal("DD", values[0, 2]);
        Assert.Equal("SECOND", values[0, 3]);
        Assert.Equal("Description", values[0, 4]);
        Assert.Equal("31110", values[0, 5]);
        Assert.Equal(12.34d, values[0, 6]);
        Assert.Equal("DSection", values[0, 7]);
        Assert.Equal("DItem", values[0, 8]);
        Assert.Equal("DSource", values[0, 9]);
        Assert.Equal("DCenter", values[0, 10]);
        Assert.Equal("DOrder", values[0, 11]);
        Assert.Equal("60410", values[0, 12]);
        Assert.Equal(-12.34d, values[0, 13]);
        Assert.Equal("CSection", values[0, 14]);
        Assert.Equal("CItem", values[0, 15]);
        Assert.Equal("CSource", values[0, 16]);
        Assert.Equal("CCenter", values[0, 17]);
        Assert.Equal("COrder", values[0, 18]);
        Assert.Equal("Closing", values[0, 19]);
        Assert.Equal(false, values[0, 20]);
        Assert.Equal(20, values[0, 21]);
        Assert.Equal(21, values[0, 22]);
        Assert.Equal(22, values[0, 23]);
        Assert.Equal("source.xlsx, row 20", values[0, 24]);
        Assert.Equal(true, values[0, 25]);
    }

    [Fact]
    public void CreateDataValues_PreservesNullAmountsAndSourceLines()
    {
        var row = new JournalRow
        {
            SequenceNumber = 1,
            PostingDate = new DateTime(2025, 1, 1)
        };

        object[,] values = JournalWorksheetDataProjector.CreateDataValues(
            [row], 0, 1);

        Assert.Null(values[0, 6]);
        Assert.Null(values[0, 13]);
        Assert.Null(values[0, 22]);
        Assert.Null(values[0, 23]);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(20_000, 1, 20_000)]
    [InlineData(20_001, 2, 1)]
    [InlineData(791_097, 40, 11_097)]
    [InlineData(1_048_572, 53, 8_572)]
    public void PlanChunks_CoversRowsWithoutAllocatingWorksheetMatrix(
        int rowCount,
        int expectedChunks,
        int expectedLastChunkRows)
    {
        IReadOnlyList<JournalWorksheetChunk> chunks =
            JournalWorksheetDataProjector.PlanChunks(rowCount);

        Assert.Equal(expectedChunks, chunks.Count);
        Assert.Equal(rowCount, chunks.Sum(chunk => chunk.RowCount));
        if (chunks.Count == 0) return;

        int expectedStartIndex = 0;
        foreach (JournalWorksheetChunk chunk in chunks)
        {
            Assert.Equal(expectedStartIndex, chunk.StartIndex);
            Assert.InRange(
                chunk.RowCount,
                1,
                JournalWorksheetDataProjector.DefaultChunkSize);
            expectedStartIndex += chunk.RowCount;
        }

        Assert.Equal(expectedLastChunkRows, chunks[^1].RowCount);
        Assert.Equal(rowCount, expectedStartIndex);
    }

    [Fact]
    public void CreateDataValues_RejectsInvalidSlice()
    {
        JournalRow[] rows = [CreateRow(1, "ONE")];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JournalWorksheetDataProjector.CreateDataValues(rows, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JournalWorksheetDataProjector.CreateDataValues(rows, 0, 2));
    }

    [Theory]
    [InlineData(-1, 20_000)]
    [InlineData(0, 0)]
    public void PlanChunks_RejectsInvalidArguments(
        int rowCount,
        int chunkSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JournalWorksheetDataProjector.PlanChunks(rowCount, chunkSize));
    }

    private static JournalRow CreateRow(int sequenceNumber, string documentNumber)
    {
        return new JournalRow
        {
            SequenceNumber = sequenceNumber,
            PostingDate = new DateTime(2025, 2, 3),
            DocumentType = "DD",
            DocumentNumber = documentNumber,
            Description = "Description",
            DebitAccount = "31110",
            DebitAmount = 12.34m,
            DebitSection = "DSection",
            DebitItem = "DItem",
            DebitFundingSource = "DSource",
            DebitCostCenter = "DCenter",
            DebitOrder = "DOrder",
            CreditAccount = "60410",
            CreditAmount = -12.34m,
            CreditSection = "CSection",
            CreditItem = "CItem",
            CreditFundingSource = "CSource",
            CreditCostCenter = "CCenter",
            CreditOrder = "COrder",
            RecordKind = JournalRecordKind.Closing,
            SourceRecordNumber = 20,
            SourceStartLineNumber = 21,
            SourceEndLineNumber = 22,
            SourceLocation = "source.xlsx, row 20",
            TextNormalizationApplied = true
        };
    }
}
