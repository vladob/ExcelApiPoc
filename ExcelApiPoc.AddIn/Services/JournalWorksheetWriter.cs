using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class JournalWorksheetWriter
    {
        private const string WorksheetName = "Accounting Journal";
        private const string TableName = "JournalRows";
        private const int HeaderRow = 4;
        private const int FirstColumn = 1;

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

            "SourceRecordNumber",
            "SourceStartLineNumber",
            "SourceEndLineNumber",
            "SourceLocation",
            "TextNormalizationApplied"
        };

        private static readonly int[] TextColumns =
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

            23  // SourceLocation
        };

        public static Excel.Workbook CreateWorkbook(JournalImport journalImport)
        {
            if (journalImport == null)
            {
                throw new ArgumentNullException(nameof(journalImport));
            }

            if (journalImport.Rows.Count == 0)
            {
                throw new InvalidOperationException("The canonical journal does not contain any rows.");
            }

            object[,] values = CreateValues(journalImport);
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook workbook = application.Workbooks.Add();

            try
            {
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
                worksheet.Name = WorksheetName;

                int lastRow = HeaderRow + journalImport.Rows.Count;
                int lastColumn = Headers.Length;
                Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
                Excel.Range lastCell = (Excel.Range)worksheet.Cells[ lastRow, lastColumn];
                Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

                Excel.Range firstDataCell =(Excel.Range)worksheet.Cells[HeaderRow + 1, FirstColumn];
                Excel.Range dataRange = worksheet.Range[firstDataCell, lastCell];
                ApplyDataFormats(dataRange);

                // One COM assignment for the complete journal.
                tableRange.Value2 = values;

                Excel.ListObject table = worksheet.ListObjects.Add( Excel.XlListObjectSourceType.xlSrcRange,
                        tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
                table.Name = TableName;
                table.TableStyle = "TableStyleMedium2";
                ApplyWorksheetLayout(worksheet, table, dataRange);

                Excel.Range countCell = (Excel.Range)worksheet.Cells[3, 1];
                countCell.Formula = "=SUBTOTAL(3,JournalRows[SequenceNumber])";
                countCell.Font.Bold = true;
                countCell.NumberFormat = "#,##0";
                worksheet.Activate();

                Excel.Window window = application.ActiveWindow;

                window.SplitRow = 4;
                window.SplitColumn = 1;
                window.FreezePanes = true;

                return workbook;
            }
            catch
            {
                workbook.Close( SaveChanges: false);
                throw;
            }
        }

        private static object[,] CreateValues(JournalImport journalImport)
        {
            int rowCount = journalImport.Rows.Count + 1;
            int columnCount = Headers.Length;
            var values = new object[rowCount, columnCount];

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            for (int rowIndex = 0; rowIndex < journalImport.Rows.Count; rowIndex++)
            {
                JournalRow row = journalImport.Rows[rowIndex];
                int targetRow = rowIndex + 1;
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
                values[targetRow, 19] = row.SourceRecordNumber;
                values[targetRow, 20] = row.SourceStartLineNumber.HasValue ? (object)row.SourceStartLineNumber.Value : null;
                values[targetRow, 21] = row.SourceEndLineNumber.HasValue ? (object)row.SourceEndLineNumber.Value : null;
                values[targetRow, 22] = row.SourceLocation;
                values[targetRow, 23] = row.TextNormalizationApplied;
            }
            return values;
        }

        private static object ToExcelNumber(decimal? value)
        {
            return value.HasValue ? (object)(double)value.Value : null;
        }

        private static void ApplyDataFormats(Excel.Range dataRange)
        {
            foreach (int columnNumber in TextColumns)
            {
                Excel.Range column = (Excel.Range)dataRange.Columns[columnNumber];

                // Must be applied before assigning values,
                // otherwise numeric-looking identifiers may be converted.
                column.NumberFormat = "@";
            }

            Excel.Range sequenceColumn =(Excel.Range)dataRange.Columns[1];
            Excel.Range postingDateColumn = (Excel.Range)dataRange.Columns[2];
            Excel.Range debitAmountColumn = (Excel.Range)dataRange.Columns[7];
            Excel.Range creditAmountColumn = (Excel.Range)dataRange.Columns[14];
            Excel.Range sourceRecordColumn = (Excel.Range)dataRange.Columns[20];
            Excel.Range sourceStartLineColumn = (Excel.Range)dataRange.Columns[21];
            Excel.Range sourceEndLineColumn = (Excel.Range)dataRange.Columns[22];

            sequenceColumn.NumberFormat = "0";
            postingDateColumn.NumberFormat = "yyyy-mm-dd";
            debitAmountColumn.NumberFormat ="#,##0.00;[Red]-#,##0.00";
            creditAmountColumn.NumberFormat ="#,##0.00;[Red]-#,##0.00";
            sourceRecordColumn.NumberFormat = "0";
            sourceStartLineColumn.NumberFormat = "0";
            sourceEndLineColumn.NumberFormat = "0";
        }

        private static void ApplyWorksheetLayout(Excel.Worksheet worksheet, Excel.ListObject table, Excel.Range dataRange)
        {
            Excel.Range headerRange = table.HeaderRowRange;

            headerRange.WrapText = false;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            headerRange.RowHeight = 20;
            Excel.Range visibleHeaderRange = worksheet.Range["A4:S4"];

            // Fit only according to the 19 visible headers.
            visibleHeaderRange.Columns.AutoFit();

            // Add room for the table filter buttons.
            for (int columnNumber = 1; columnNumber <= 19; columnNumber++)
            {
                Excel.Range column = (Excel.Range)worksheet.Columns[ columnNumber];
                double currentWidth = Convert.ToDouble( column.ColumnWidth);
                column.ColumnWidth = Math.Min(currentWidth + 2, 40);
            }

            SetMinimumColumnWidth(worksheet, 2, 12); // PostingDate
            SetMinimumColumnWidth(worksheet, 4, 16); // DocumentNumber
            SetMinimumColumnWidth(worksheet, 5, 40); // Description
            SetMinimumColumnWidth(worksheet, 7, 14); // DebitAmount
            SetMinimumColumnWidth(worksheet, 14, 14); // CreditAmount

            Excel.Range sequenceColumn = (Excel.Range)dataRange.Columns[1];
            Excel.Range postingDateColumn = (Excel.Range)dataRange.Columns[2];
            Excel.Range descriptionColumn = (Excel.Range)dataRange.Columns[5];
            Excel.Range debitAmountColumn = (Excel.Range)dataRange.Columns[7];
            Excel.Range creditAmountColumn = (Excel.Range)dataRange.Columns[14];
            sequenceColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            postingDateColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            descriptionColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            debitAmountColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            creditAmountColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

            // Preserve row-level source traceability in the table,
            // but keep technical columns out of the auditor's default view.
            Excel.Range technicalColumns = worksheet.Range["T:X"];
            technicalColumns.EntireColumn.Hidden = true;
        }

        private static void SetMinimumColumnWidth(Excel.Worksheet worksheet, int columnNumber, double minimumWidth)
        {
            Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
            double currentWidth = Convert.ToDouble(column.ColumnWidth);
            if (currentWidth < minimumWidth)
            {
                column.ColumnWidth =minimumWidth;
            }
        }
    }
}