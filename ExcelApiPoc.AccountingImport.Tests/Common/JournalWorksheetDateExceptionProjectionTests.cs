using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

public sealed class JournalWorksheetDateExceptionProjectionTests
{
    [Fact]
    public void MissingPostingDate_is_blank_and_excluded_in_the_journal_worksheet()
    {
        var row = new JournalRow
        {
            PostingDate = DateTime.MinValue,
            DateExceptionResolution = JournalDateExceptionResolution.Excluded,
            DebitAccount = "357REF", DebitAmount = 126.05m
        };
        object[,] values = JournalWorksheetDataProjector.CreateDataValues(new[] { row }, 0, 1, true);
        Assert.Null(values[0, 1]);
        Assert.Equal("Excluded", values[0, 2]);
        Assert.Equal(false, values[0, 22]);
        Assert.Equal(126.05d, values[0, 8]);
    }

    [Fact]
    public void DateExceptionColumns_AreInsertedOnlyWhenRequested()
    {
        object[,] cleanHeaders =
            JournalWorksheetDataProjector.CreateHeaderValues(false);
        object[,] exceptionHeaders =
            JournalWorksheetDataProjector.CreateHeaderValues(true);

        Assert.Equal(26, cleanHeaders.GetLength(1));
        Assert.Equal(28, exceptionHeaders.GetLength(1));
        Assert.Equal("PostingDate", exceptionHeaders[0, 1]);
        Assert.Equal("DateExceptionResolution", exceptionHeaders[0, 2]);
        Assert.Equal("CorrectedPostingDate", exceptionHeaders[0, 3]);
        Assert.Equal("DocumentType", exceptionHeaders[0, 4]);
        Assert.Equal("TextNormalizationApplied", exceptionHeaders[0, 27]);
    }

    [Fact]
    public void DateExceptionColumns_ProjectResolutionAndCorrection()
    {
        var row = new JournalRow
        {
            SequenceNumber = 7,
            PostingDate = new DateTime(3025, 3, 12),
            DateExceptionResolution =
                JournalDateExceptionResolution.ModifiedIncluded,
            CorrectedPostingDate = new DateTime(2025, 3, 12),
            DocumentType = "DD",
            DocumentNumber = "123",
            RecordKind = JournalRecordKind.Normal
        };

        object[,] values =
            JournalWorksheetDataProjector.CreateDataValues(
                new[] { row },
                0,
                1,
                true);

        Assert.Equal(28, values.GetLength(1));
        Assert.Equal(new DateTime(3025, 3, 12), values[0, 1]);
        Assert.Equal("ModifiedIncluded", values[0, 2]);
        Assert.Equal(new DateTime(2025, 3, 12), values[0, 3]);
        Assert.Equal("DD", values[0, 4]);
        Assert.Equal("123", values[0, 5]);
        Assert.Equal(true, values[0, 22]);
    }
}
