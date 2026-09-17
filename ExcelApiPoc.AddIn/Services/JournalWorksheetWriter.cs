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

        internal const string ExcludedCaption = "Row Excluded";
        internal const string OriginalIncludedCaption = "Original Date Included";
        internal const string ModifiedIncludedCaption = "Modified Date Included";

        private static readonly int[] BaseTextColumns =
        {
            3, 4, 5, 6, 8, 9, 10, 11, 12,
            13, 15, 16, 17, 18, 19, 20, 25
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
                throw new InvalidOperationException("The canonical journal does not contain any rows.");

            JournalImportCapacity.EnsureCanAppend(0, journalImport.Rows.Count, journalImport.SourceFileName);

            bool includeDateExceptionColumns =
                journalImport.Rows.Any(row => row.DateExceptionResolution.HasValue);
            int lastColumn =
                JournalWorksheetDataProjector.GetColumnCount(includeDateExceptionColumns);

            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[1];
            worksheet.Name = WorksheetName;

            int lastRow = HeaderRow + journalImport.Rows.Count;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, lastColumn];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            Excel.Range firstDataCell = (Excel.Range)worksheet.Cells[HeaderRow + 1, FirstColumn];
            Excel.Range dataRange = worksheet.Range[firstDataCell, lastCell];
            ApplyDataFormats(dataRange, includeDateExceptionColumns);

            WriteHeader(worksheet, lastColumn, includeDateExceptionColumns);
            WriteDataChunks(worksheet, journalImport, lastColumn, includeDateExceptionColumns);

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            ApplyWorksheetLayout(worksheet, table, dataRange, includeDateExceptionColumns);

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

            ApplyDateExceptionValidation(workbook, journalImport);
            return worksheet;
        }

        public static void ApplyDateExceptionValidation(
            Excel.Workbook workbook,
            JournalImport journalImport)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (journalImport == null)
                throw new ArgumentNullException(nameof(journalImport));

            if (!journalImport.Rows.Any(row => row.DateExceptionResolution.HasValue))
                return;

            AnalyticalMappingValidationWorksheetWriter.EnsureJournalDateExceptionOptions(workbook);
            Excel.ListObject table = FindTable(workbook, TableName);
            Excel.Range resolutionColumn = table.ListColumns["DateExceptionResolution"].DataBodyRange;

            try
            {
                resolutionColumn.Validation.Delete();
            }
            catch
            {
                // The column has no validation yet.
            }

            for (int index = 0; index < journalImport.Rows.Count; index++)
            {
                JournalRow row = journalImport.Rows[index];
                if (!row.DateExceptionResolution.HasValue)
                    continue;

                Excel.Range cell = (Excel.Range)resolutionColumn.Cells[index + 1, 1];
                cell.Value2 = ToResolutionCaption(row.DateExceptionResolution.Value);
                cell.Validation.Add(
                    Excel.XlDVType.xlValidateList,
                    Excel.XlDVAlertStyle.xlValidAlertStop,
                    Excel.XlFormatConditionOperator.xlBetween,
                    "=" + AnalyticalMappingValidationWorksheetWriter.DateExceptionValidationRangeName,
                    Type.Missing);
                cell.Validation.IgnoreBlank = false;
                cell.Validation.InCellDropdown = true;
                cell.Validation.ShowError = true;
            }
        }

        public static void SynchronizeDerivedColumns(
            Excel.Workbook workbook,
            JournalImport journalImport)
        {
            Excel.ListObject table = FindTable(workbook, TableName);
            Excel.Range usedColumn = table.ListColumns["UsedForReportCalculation"].DataBodyRange;
            Excel.Range resolutionColumn = null;

            try
            {
                resolutionColumn = table.ListColumns["DateExceptionResolution"].DataBodyRange;
            }
            catch
            {
                // Clean journals do not have date-exception columns.
            }

            for (int index = 0; index < journalImport.Rows.Count; index++)
            {
                JournalRow row = journalImport.Rows[index];
                ((Excel.Range)usedColumn.Cells[index + 1, 1]).Value2 =
                    row.UsedForReportCalculation;

                if (resolutionColumn != null && row.DateExceptionResolution.HasValue)
                {
                    ((Excel.Range)resolutionColumn.Cells[index + 1, 1]).Value2 =
                        ToResolutionCaption(row.DateExceptionResolution.Value);
                }
            }
        }

        internal static string ToResolutionCaption(JournalDateExceptionResolution resolution)
        {
            switch (resolution)
            {
                case JournalDateExceptionResolution.Excluded:
                    return ExcludedCaption;
                case JournalDateExceptionResolution.OriginalIncluded:
                    return OriginalIncludedCaption;
                case JournalDateExceptionResolution.ModifiedIncluded:
                    return ModifiedIncludedCaption;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution));
            }
        }

        internal static bool TryParseResolutionCaption(
            string value,
            out JournalDateExceptionResolution resolution)
        {
            string text = (value ?? string.Empty).Trim();
            if (string.Equals(text, ExcludedCaption, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "Excluded", StringComparison.OrdinalIgnoreCase))
            {
                resolution = JournalDateExceptionResolution.Excluded;
                return true;
            }
            if (string.Equals(text, OriginalIncludedCaption, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "OriginalIncluded", StringComparison.OrdinalIgnoreCase))
            {
                resolution = JournalDateExceptionResolution.OriginalIncluded;
                return true;
            }
            if (string.Equals(text, ModifiedIncludedCaption, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(text, "ModifiedIncluded", StringComparison.OrdinalIgnoreCase))
            {
                resolution = JournalDateExceptionResolution.ModifiedIncluded;
                return true;
            }

            resolution = default(JournalDateExceptionResolution);
            return false;
        }

        private static void WriteHeader(
            Excel.Worksheet worksheet,
            int lastColumn,
            bool includeDateExceptionColumns)
        {
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[HeaderRow, lastColumn];
            Excel.Range headerRange = worksheet.Range[firstCell, lastCell];
            headerRange.Value2 = JournalWorksheetDataProjector.CreateHeaderValues(includeDateExceptionColumns);
        }

        private static void WriteDataChunks(
            Excel.Worksheet worksheet,
            JournalImport journalImport,
            int lastColumn,
            bool includeDateExceptionColumns)
        {
            foreach (JournalWorksheetChunk chunk in JournalWorksheetDataProjector.PlanChunks(journalImport.Rows.Count))
            {
                int firstWorksheetRow = HeaderRow + 1 + chunk.StartIndex;
                int lastWorksheetRow = firstWorksheetRow + chunk.RowCount - 1;
                Excel.Range firstCell = (Excel.Range)worksheet.Cells[firstWorksheetRow, FirstColumn];
                Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastWorksheetRow, lastColumn];
                Excel.Range targetRange = worksheet.Range[firstCell, lastCell];
                targetRange.Value2 = JournalWorksheetDataProjector.CreateDataValues(
                    journalImport.Rows,
                    chunk.StartIndex,
                    chunk.RowCount,
                    includeDateExceptionColumns);
            }
        }

        private static void ApplyDataFormats(Excel.Range dataRange, bool includeDateExceptionColumns)
        {
            int offset = includeDateExceptionColumns ? 2 : 0;
            foreach (int baseColumnNumber in BaseTextColumns)
            {
                int columnNumber = baseColumnNumber + offset;
                ((Excel.Range)dataRange.Columns[columnNumber]).NumberFormat = "@";
            }

            if (includeDateExceptionColumns)
            {
                ((Excel.Range)dataRange.Columns[3]).NumberFormat = "@";
                ((Excel.Range)dataRange.Columns[4]).NumberFormat = "yyyy-mm-dd";
            }

            ((Excel.Range)dataRange.Columns[1]).NumberFormat = "0";
            ((Excel.Range)dataRange.Columns[2]).NumberFormat = "yyyy-mm-dd";
            ((Excel.Range)dataRange.Columns[7 + offset]).NumberFormat = "#,##0.00;[Red]-#,##0.00";
            ((Excel.Range)dataRange.Columns[14 + offset]).NumberFormat = "#,##0.00;[Red]-#,##0.00";
            ((Excel.Range)dataRange.Columns[22 + offset]).NumberFormat = "0";
            ((Excel.Range)dataRange.Columns[23 + offset]).NumberFormat = "0";
            ((Excel.Range)dataRange.Columns[24 + offset]).NumberFormat = "0";
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

            Excel.Range firstVisibleHeader = (Excel.Range)worksheet.Cells[HeaderRow, 1];
            Excel.Range lastVisibleHeader = (Excel.Range)worksheet.Cells[HeaderRow, 19 + offset];
            worksheet.Range[firstVisibleHeader, lastVisibleHeader].Columns.AutoFit();

            for (int columnNumber = 1; columnNumber <= 19 + offset; columnNumber++)
            {
                Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
                double currentWidth = Convert.ToDouble(column.ColumnWidth);
                column.ColumnWidth = Math.Min(currentWidth + 2, 40);
            }

            SetMinimumColumnWidth(worksheet, 2, 12);
            if (includeDateExceptionColumns)
            {
                SetMinimumColumnWidth(worksheet, 3, 24);
                SetMinimumColumnWidth(worksheet, 4, 18);
            }
            SetMinimumColumnWidth(worksheet, 4 + offset, 16);
            SetMinimumColumnWidth(worksheet, 5 + offset, 40);
            SetMinimumColumnWidth(worksheet, 7 + offset, 14);
            SetMinimumColumnWidth(worksheet, 14 + offset, 14);

            ((Excel.Range)dataRange.Columns[1]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            ((Excel.Range)dataRange.Columns[2]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ((Excel.Range)dataRange.Columns[5 + offset]).HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            ((Excel.Range)dataRange.Columns[7 + offset]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            ((Excel.Range)dataRange.Columns[14 + offset]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

            if (includeDateExceptionColumns)
            {
                ((Excel.Range)dataRange.Columns[3]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                ((Excel.Range)dataRange.Columns[4]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            }

            Excel.Range firstTechnicalColumn = (Excel.Range)worksheet.Columns[22 + offset];
            Excel.Range lastTechnicalColumn = (Excel.Range)worksheet.Columns[26 + offset];
            worksheet.Range[firstTechnicalColumn, lastTechnicalColumn].EntireColumn.Hidden = true;
        }

        private static void SetMinimumColumnWidth(
            Excel.Worksheet worksheet,
            int columnNumber,
            double minimumWidth)
        {
            Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
            double currentWidth = Convert.ToDouble(column.ColumnWidth);
            if (currentWidth < minimumWidth)
                column.ColumnWidth = minimumWidth;
        }

        private static Excel.ListObject FindTable(Excel.Workbook workbook, string tableName)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                        return table;
                }
            }
            throw new InvalidOperationException("The workbook does not contain table '" + tableName + "'.");
        }
    }
}
