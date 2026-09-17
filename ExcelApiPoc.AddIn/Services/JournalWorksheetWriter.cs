using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class JournalWorksheetWriter
    {
        private const string WorksheetName = "Accounting Journal";
        private const string TableName = "JournalRows";
        private const int HeaderRow = JournalImportCapacity.JournalHeaderRow;
        private const int FirstColumn = 1;

        private static readonly int[] BaseTextColumns =
        {
            3,  // DocumentType
            4,  // DocumentNumber
            5,  // Description
            6,  // DebitAccount
            8,  // DebitSection
            9,  // DebitItem
            10, // DebitFundingSource
            11, // DebitCostCenter
            12, // DebitOrder
            13, // CreditAccount
            15, // CreditSection
            16, // CreditItem
            17, // CreditFundingSource
            18, // CreditCostCenter
            19, // CreditOrder
            20, // RecordKind
            25  // SourceLocation
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            JournalImport journalImport)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (journalImport == null)
                throw new ArgumentNullException(nameof(journalImport));

            if (journalImport.Rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "The canonical journal does not contain any rows.");
            }

            JournalImportCapacity.EnsureCanAppend(
                0,
                journalImport.Rows.Count,
                journalImport.SourceFileName);

            bool includeDateExceptionColumns =
                journalImport.Rows.Any(
                    row => row.DateExceptionResolution.HasValue);
            int lastColumn =
                JournalWorksheetDataProjector.GetColumnCount(
                    includeDateExceptionColumns);

            Excel.Worksheet worksheet =
                (Excel.Worksheet)workbook.Worksheets[1];
            worksheet.Name = WorksheetName;

            int lastRow = HeaderRow + journalImport.Rows.Count;
            Excel.Range firstCell =
                (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
            Excel.Range lastCell =
                (Excel.Range)worksheet.Cells[lastRow, lastColumn];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            Excel.Range firstDataCell =
                (Excel.Range)worksheet.Cells[HeaderRow + 1, FirstColumn];
            Excel.Range dataRange = worksheet.Range[firstDataCell, lastCell];
            ApplyDataFormats(dataRange, includeDateExceptionColumns);

            WriteHeader(
                worksheet,
                lastColumn,
                includeDateExceptionColumns);
            WriteDataChunks(
                worksheet,
                journalImport,
                lastColumn,
                includeDateExceptionColumns);

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            ApplyWorksheetLayout(
                worksheet,
                table,
                dataRange,
                includeDateExceptionColumns);

            Excel.Range countCell = (Excel.Range)worksheet.Cells[3, 1];
            countCell.Formula = "=SUBTOTAL(3,JournalRows[SequenceNumber])";
            countCell.Font.Bold = true;
            countCell.NumberFormat = "#,##0";
            worksheet.Activate();

            Excel.Application application = workbook.Application;
            Excel.Window window = application.ActiveWindow;

            window.SplitRow = 4;
            window.SplitColumn = 1;
            window.FreezePanes = true;

            return worksheet;
        }

        private static void WriteHeader(
            Excel.Worksheet worksheet,
            int lastColumn,
            bool includeDateExceptionColumns)
        {
            Excel.Range firstCell =
                (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
            Excel.Range lastCell =
                (Excel.Range)worksheet.Cells[HeaderRow, lastColumn];
            Excel.Range headerRange = worksheet.Range[firstCell, lastCell];
            headerRange.Value2 =
                JournalWorksheetDataProjector.CreateHeaderValues(
                    includeDateExceptionColumns);
        }

        private static void WriteDataChunks(
            Excel.Worksheet worksheet,
            JournalImport journalImport,
            int lastColumn,
            bool includeDateExceptionColumns)
        {
            foreach (JournalWorksheetChunk chunk in
                JournalWorksheetDataProjector.PlanChunks(
                    journalImport.Rows.Count))
            {
                int firstWorksheetRow =
                    HeaderRow + 1 + chunk.StartIndex;
                int lastWorksheetRow =
                    firstWorksheetRow + chunk.RowCount - 1;
                Excel.Range firstCell =
                    (Excel.Range)worksheet.Cells[
                        firstWorksheetRow,
                        FirstColumn];
                Excel.Range lastCell =
                    (Excel.Range)worksheet.Cells[
                        lastWorksheetRow,
                        lastColumn];
                Excel.Range targetRange =
                    worksheet.Range[firstCell, lastCell];

                targetRange.Value2 =
                    JournalWorksheetDataProjector.CreateDataValues(
                        journalImport.Rows,
                        chunk.StartIndex,
                        chunk.RowCount,
                        includeDateExceptionColumns);
            }
        }

        private static void ApplyDataFormats(
            Excel.Range dataRange,
            bool includeDateExceptionColumns)
        {
            int offset = includeDateExceptionColumns ? 2 : 0;

            foreach (int baseColumnNumber in BaseTextColumns)
            {
                int columnNumber = baseColumnNumber + offset;
                Excel.Range column =
                    (Excel.Range)dataRange.Columns[columnNumber];

                // Must be applied before assigning values,
                // otherwise numeric-looking identifiers may be converted.
                column.NumberFormat = "@";
            }

            if (includeDateExceptionColumns)
            {
                Excel.Range resolutionColumn =
                    (Excel.Range)dataRange.Columns[3];
                Excel.Range correctedDateColumn =
                    (Excel.Range)dataRange.Columns[4];
                resolutionColumn.NumberFormat = "@";
                correctedDateColumn.NumberFormat = "yyyy-mm-dd";
            }

            Excel.Range sequenceColumn =
                (Excel.Range)dataRange.Columns[1];
            Excel.Range postingDateColumn =
                (Excel.Range)dataRange.Columns[2];
            Excel.Range debitAmountColumn =
                (Excel.Range)dataRange.Columns[7 + offset];
            Excel.Range creditAmountColumn =
                (Excel.Range)dataRange.Columns[14 + offset];
            Excel.Range sourceRecordColumn =
                (Excel.Range)dataRange.Columns[22 + offset];
            Excel.Range sourceStartLineColumn =
                (Excel.Range)dataRange.Columns[23 + offset];
            Excel.Range sourceEndLineColumn =
                (Excel.Range)dataRange.Columns[24 + offset];

            sequenceColumn.NumberFormat = "0";
            postingDateColumn.NumberFormat = "yyyy-mm-dd";
            debitAmountColumn.NumberFormat = "#,##0.00;[Red]-#,##0.00";
            creditAmountColumn.NumberFormat = "#,##0.00;[Red]-#,##0.00";
            sourceRecordColumn.NumberFormat = "0";
            sourceStartLineColumn.NumberFormat = "0";
            sourceEndLineColumn.NumberFormat = "0";
        }

        private static void ApplyWorksheetLayout(
            Excel.Worksheet worksheet,
            Excel.ListObject table,
            Excel.Range dataRange,
            bool includeDateExceptionColumns)
        {
            int offset = includeDateExceptionColumns ? 2 : 0;
            Excel.Range headerRange = table.HeaderRowRange;

            headerRange.WrapText = false;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            headerRange.RowHeight = 20;

            Excel.Range firstVisibleHeader =
                (Excel.Range)worksheet.Cells[HeaderRow, 1];
            Excel.Range lastVisibleHeader =
                (Excel.Range)worksheet.Cells[HeaderRow, 19 + offset];
            Excel.Range visibleHeaderRange =
                worksheet.Range[firstVisibleHeader, lastVisibleHeader];

            // Fit only according to visible business columns.
            visibleHeaderRange.Columns.AutoFit();

            for (int columnNumber = 1;
                 columnNumber <= 19 + offset;
                 columnNumber++)
            {
                Excel.Range column =
                    (Excel.Range)worksheet.Columns[columnNumber];
                double currentWidth = Convert.ToDouble(column.ColumnWidth);
                column.ColumnWidth = Math.Min(currentWidth + 2, 40);
            }

            SetMinimumColumnWidth(worksheet, 2, 12); // PostingDate
            if (includeDateExceptionColumns)
            {
                SetMinimumColumnWidth(worksheet, 3, 22); // Resolution
                SetMinimumColumnWidth(worksheet, 4, 18); // Corrected date
            }
            SetMinimumColumnWidth(worksheet, 4 + offset, 16); // DocumentNumber
            SetMinimumColumnWidth(worksheet, 5 + offset, 40); // Description
            SetMinimumColumnWidth(worksheet, 7 + offset, 14); // DebitAmount
            SetMinimumColumnWidth(worksheet, 14 + offset, 14); // CreditAmount

            Excel.Range sequenceColumn =
                (Excel.Range)dataRange.Columns[1];
            Excel.Range postingDateColumn =
                (Excel.Range)dataRange.Columns[2];
            Excel.Range descriptionColumn =
                (Excel.Range)dataRange.Columns[5 + offset];
            Excel.Range debitAmountColumn =
                (Excel.Range)dataRange.Columns[7 + offset];
            Excel.Range creditAmountColumn =
                (Excel.Range)dataRange.Columns[14 + offset];

            sequenceColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            postingDateColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            descriptionColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            debitAmountColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            creditAmountColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

            if (includeDateExceptionColumns)
            {
                Excel.Range resolutionColumn =
                    (Excel.Range)dataRange.Columns[3];
                Excel.Range correctedDateColumn =
                    (Excel.Range)dataRange.Columns[4];
                resolutionColumn.HorizontalAlignment =
                    Excel.XlHAlign.xlHAlignCenter;
                correctedDateColumn.HorizontalAlignment =
                    Excel.XlHAlign.xlHAlignCenter;
            }

            // Preserve row-level source traceability in the table,
            // but keep technical columns out of the auditor's default view.
            Excel.Range firstTechnicalColumn =
                (Excel.Range)worksheet.Columns[22 + offset];
            Excel.Range lastTechnicalColumn =
                (Excel.Range)worksheet.Columns[26 + offset];
            Excel.Range technicalColumns =
                worksheet.Range[firstTechnicalColumn, lastTechnicalColumn];
            technicalColumns.EntireColumn.Hidden = true;
        }

        private static void SetMinimumColumnWidth(
            Excel.Worksheet worksheet,
            int columnNumber,
            double minimumWidth)
        {
            Excel.Range column =
                (Excel.Range)worksheet.Columns[columnNumber];
            double currentWidth = Convert.ToDouble(column.ColumnWidth);
            if (currentWidth < minimumWidth)
                column.ColumnWidth = minimumWidth;
        }
    }
}
