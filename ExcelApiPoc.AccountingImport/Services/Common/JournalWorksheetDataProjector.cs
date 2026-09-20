using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Common
{
    public static class JournalWorksheetDataProjector
    {
        public const int DefaultChunkSize = 20_000;
        public const int ColumnCount = 26;
        public const int DateExceptionColumnCount = 2;

        private static readonly string[] BaseHeaders =
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

        public static int GetColumnCount(bool includeDateExceptionColumns)
        {
            return ColumnCount +
                (includeDateExceptionColumns ? DateExceptionColumnCount : 0);
        }

        public static object[,] CreateHeaderValues()
        {
            return CreateHeaderValues(false);
        }

        public static object[,] CreateHeaderValues(
            bool includeDateExceptionColumns)
        {
            int columnCount = GetColumnCount(includeDateExceptionColumns);
            var values = new object[1, columnCount];

            values[0, 0] = BaseHeaders[0];
            values[0, 1] = BaseHeaders[1];

            int targetIndex = 2;
            if (includeDateExceptionColumns)
            {
                values[0, targetIndex++] = "DateExceptionResolution";
                values[0, targetIndex++] = "CorrectedPostingDate";
            }

            for (int sourceIndex = 2;
                 sourceIndex < BaseHeaders.Length;
                 sourceIndex++)
            {
                values[0, targetIndex++] = BaseHeaders[sourceIndex];
            }

            return values;
        }

        public static object[,] CreateDataValues(
            IReadOnlyList<JournalRow> rows,
            int startIndex,
            int rowCount)
        {
            return CreateDataValues(
                rows,
                startIndex,
                rowCount,
                false);
        }

        public static object[,] CreateDataValues(
            IReadOnlyList<JournalRow> rows,
            int startIndex,
            int rowCount,
            bool includeDateExceptionColumns)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (startIndex < 0 || startIndex > rows.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (rowCount < 0 || rowCount > rows.Count - startIndex)
                throw new ArgumentOutOfRangeException(nameof(rowCount));

            int columnCount = GetColumnCount(includeDateExceptionColumns);
            var values = new object[rowCount, columnCount];

            for (int targetRow = 0; targetRow < rowCount; targetRow++)
            {
                JournalRow row = rows[startIndex + targetRow] ??
                    throw new InvalidOperationException(
                        "The canonical journal contains a null row at index " +
                        (startIndex + targetRow) + ".");

                values[targetRow, 0] = row.SequenceNumber;
                values[targetRow, 1] = row.PostingDate;

                int targetColumn = 2;
                if (includeDateExceptionColumns)
                {
                    values[targetRow, targetColumn++] =
                        row.DateExceptionResolution.HasValue
                            ? (object)row.DateExceptionResolution.Value.ToString()
                            : null;
                    values[targetRow, targetColumn++] =
                        row.CorrectedPostingDate.HasValue
                            ? (object)row.CorrectedPostingDate.Value
                            : null;
                }

                values[targetRow, targetColumn++] = row.DocumentType;
                values[targetRow, targetColumn++] = row.DocumentNumber;
                values[targetRow, targetColumn++] = row.Description;
                values[targetRow, targetColumn++] = row.DebitAccount;
                values[targetRow, targetColumn++] = ToExcelNumber(row.DebitAmount);
                values[targetRow, targetColumn++] = row.DebitSection;
                values[targetRow, targetColumn++] = row.DebitItem;
                values[targetRow, targetColumn++] = row.DebitFundingSource;
                values[targetRow, targetColumn++] = row.DebitCostCenter;
                values[targetRow, targetColumn++] = row.DebitOrder;
                values[targetRow, targetColumn++] = row.CreditAccount;
                values[targetRow, targetColumn++] = ToExcelNumber(row.CreditAmount);
                values[targetRow, targetColumn++] = row.CreditSection;
                values[targetRow, targetColumn++] = row.CreditItem;
                values[targetRow, targetColumn++] = row.CreditFundingSource;
                values[targetRow, targetColumn++] = row.CreditCostCenter;
                values[targetRow, targetColumn++] = row.CreditOrder;
                values[targetRow, targetColumn++] = row.RecordKind.ToString();
                values[targetRow, targetColumn++] = row.UsedForReportCalculation;
                values[targetRow, targetColumn++] = row.SourceRecordNumber;
                values[targetRow, targetColumn++] =
                    row.SourceStartLineNumber.HasValue
                        ? (object)row.SourceStartLineNumber.Value
                        : null;
                values[targetRow, targetColumn++] =
                    row.SourceEndLineNumber.HasValue
                        ? (object)row.SourceEndLineNumber.Value
                        : null;
                values[targetRow, targetColumn++] = row.SourceLocation;
                values[targetRow, targetColumn] = row.TextNormalizationApplied;
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
