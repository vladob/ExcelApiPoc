using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Common
{
    public static class JournalWorksheetDataProjector
    {
        public const int DefaultChunkSize = 20_000;
        public const int ColumnCount = 26;

        private static readonly string[] Headers =
        {
            "SequenceNumber",
            "PostingDate",
            "DocumentType",
            "DocumentNumber",
            "Description",
            "DebitAccount",
            "DebitAmount",
            "DebitSection",
            "DebitItem",
            "DebitFundingSource",
            "DebitCostCenter",
            "DebitOrder",
            "CreditAccount",
            "CreditAmount",
            "CreditSection",
            "CreditItem",
            "CreditFundingSource",
            "CreditCostCenter",
            "CreditOrder",
            "RecordKind",
            "UsedForReportCalculation",
            "SourceRecordNumber",
            "SourceStartLineNumber",
            "SourceEndLineNumber",
            "SourceLocation",
            "TextNormalizationApplied"
        };

        public static object[,] CreateHeaderValues()
        {
            var values = new object[1, ColumnCount];
            for (int columnIndex = 0; columnIndex < ColumnCount; columnIndex++)
                values[0, columnIndex] = Headers[columnIndex];
            return values;
        }

        public static object[,] CreateDataValues(
            IReadOnlyList<JournalRow> rows,
            int startIndex,
            int rowCount)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (startIndex < 0 || startIndex > rows.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (rowCount < 0 || rowCount > rows.Count - startIndex)
                throw new ArgumentOutOfRangeException(nameof(rowCount));

            var values = new object[rowCount, ColumnCount];
            for (int targetRow = 0; targetRow < rowCount; targetRow++)
            {
                JournalRow row = rows[startIndex + targetRow] ?? throw new InvalidOperationException(
                        "The canonical journal contains a null row at index " +
                        (startIndex + targetRow) + ".");
                values[targetRow, 0] = row.SequenceNumber;
                values[targetRow, 1] = row.PostingDate;
                values[targetRow, 2] = row.DocumentType;
                values[targetRow, 3] = row.DocumentNumber;
                values[targetRow, 4] = row.Description;
                values[targetRow, 5] = row.DebitAccount;
                values[targetRow, 6] = ToExcelNumber(row.DebitAmount);
                values[targetRow, 7] = row.DebitSection;
                values[targetRow, 8] = row.DebitItem;
                values[targetRow, 9] = row.DebitFundingSource;
                values[targetRow, 10] = row.DebitCostCenter;
                values[targetRow, 11] = row.DebitOrder;
                values[targetRow, 12] = row.CreditAccount;
                values[targetRow, 13] = ToExcelNumber(row.CreditAmount);
                values[targetRow, 14] = row.CreditSection;
                values[targetRow, 15] = row.CreditItem;
                values[targetRow, 16] = row.CreditFundingSource;
                values[targetRow, 17] = row.CreditCostCenter;
                values[targetRow, 18] = row.CreditOrder;
                values[targetRow, 19] = row.RecordKind.ToString();
                values[targetRow, 20] = row.UsedForReportCalculation;
                values[targetRow, 21] = row.SourceRecordNumber;
                values[targetRow, 22] = row.SourceStartLineNumber.HasValue
                    ? (object)row.SourceStartLineNumber.Value
                    : null;
                values[targetRow, 23] = row.SourceEndLineNumber.HasValue
                    ? (object)row.SourceEndLineNumber.Value
                    : null;
                values[targetRow, 24] = row.SourceLocation;
                values[targetRow, 25] = row.TextNormalizationApplied;
            }
            return values;
        }

        public static IReadOnlyList<JournalWorksheetChunk> PlanChunks(
            int rowCount,
            int chunkSize = DefaultChunkSize)
        {
            if (rowCount < 0) throw new ArgumentOutOfRangeException(nameof(rowCount));
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));

            var chunks = new List<JournalWorksheetChunk>();
            int startIndex = 0;
            while (startIndex < rowCount)
            {
                int currentRowCount = Math.Min(chunkSize, rowCount - startIndex);
                chunks.Add(new JournalWorksheetChunk(
                    startIndex,
                    currentRowCount));
                startIndex += currentRowCount;
            }
            return chunks;
        }

        private static object ToExcelNumber(decimal? value)
        {
            return value.HasValue ? (object)(double)value.Value : null;
        }
    }

    public sealed class JournalWorksheetChunk
    {
        public JournalWorksheetChunk(int startIndex, int rowCount)
        {
            StartIndex = startIndex;
            RowCount = rowCount;
        }

        public int StartIndex { get; }
        public int RowCount { get; }
    }
}
