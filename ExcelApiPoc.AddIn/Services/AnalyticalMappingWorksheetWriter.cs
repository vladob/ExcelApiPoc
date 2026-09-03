using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingWorksheetWriter
    {
        private const string WorksheetName = "Analytical Mapping";
        private const string TableName = "AnalyticalMappings";
        private const int HeaderRow = 4;

        private static readonly string[] Headers =
        {
            "AccountCode",
            "AccountName",
            "SyntheticAccountCode",
            "SyntheticAccountName",
            "DebitBalance",
            "CreditBalance",
            "NetBalance",
            "MappedTo",
            "MappingStatus"
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook,IReadOnlyList<AnalyticalMappingRow> mappings)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (mappings == null)
                throw new ArgumentNullException(nameof(mappings));

            if (mappings.Count == 0)
            {
                throw new InvalidOperationException(
                    "The analytical mapping does not contain any accounts.");
            }

            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            object previousActiveSheet = application.ActiveSheet;
            Excel.Worksheet lastWorksheet = (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets.Add(After: lastWorksheet);
            worksheet.Name = WorksheetName;

            int lastRow = HeaderRow + mappings.Count;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, Headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            Excel.Range firstDataCell = (Excel.Range)worksheet.Cells[HeaderRow + 1, 1];
            Excel.Range dataRange = worksheet.Range[firstDataCell, lastCell];

            ApplyDataFormats(dataRange);
            tableRange.Value2 = CreateValues(mappings);

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            ApplyMappingValidation(worksheet, mappings);
            ApplyStatusFormula(table);
            AddSubtotalFormulas(worksheet);
            ApplyWorksheetLayout(worksheet, table);

            worksheet.Activate();
            Excel.Window window = application.ActiveWindow;
            window.SplitRow = HeaderRow;
            window.SplitColumn = 1;
            window.FreezePanes = true;

            if (previousActiveSheet is Excel.Worksheet previousWorksheet)
                previousWorksheet.Activate();

            return worksheet;
        }

        private static object[,] CreateValues(IReadOnlyList<AnalyticalMappingRow> mappings)
        {
            var values = new object[mappings.Count + 1, Headers.Length];

            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            for (int mappingIndex = 0; mappingIndex < mappings.Count; mappingIndex++)
            {
                AnalyticalMappingRow mapping = mappings[mappingIndex];
                int rowIndex = mappingIndex + 1;

                values[rowIndex, 0] = mapping.AccountCode;
                values[rowIndex, 1] = mapping.AccountName;
                values[rowIndex, 2] = mapping.SyntheticAccountCode;
                values[rowIndex, 3] = mapping.SyntheticAccountName;
                values[rowIndex, 4] = (double)mapping.DebitBalance;
                values[rowIndex, 5] = (double)mapping.CreditBalance;
                values[rowIndex, 6] = (double)mapping.NetBalance;
                values[rowIndex, 7] = string.Empty;
                values[rowIndex, 8] = string.Empty;
            }

            return values;
        }

        private static void ApplyDataFormats(Excel.Range dataRange)
        {
            for (int columnNumber = 1; columnNumber <= 4; columnNumber++)
            {
                Excel.Range column = (Excel.Range)dataRange.Columns[columnNumber];
                column.NumberFormat = "@";
            }

            Excel.Range mappedToColumn = (Excel.Range)dataRange.Columns[8];
            mappedToColumn.NumberFormat = "@";

            for (int columnNumber = 5; columnNumber <= 7; columnNumber++)
            {
                Excel.Range amountColumn = (Excel.Range)dataRange.Columns[columnNumber];
                amountColumn.NumberFormat = "#,##0.00;[Red]-#,##0.00";
            }
        }

        private static void ApplyMappingValidation(Excel.Worksheet worksheet, IReadOnlyList<AnalyticalMappingRow> mappings)
        {
            for (int mappingIndex = 0; mappingIndex < mappings.Count; mappingIndex++)
            {
                AnalyticalMappingRow mapping = mappings[mappingIndex];
                Excel.Range cell = (Excel.Range)worksheet.Cells[HeaderRow + 1 + mappingIndex, 8];
                cell.Validation.Delete();
                cell.Validation.Add(Excel.XlDVType.xlValidateList, Excel.XlDVAlertStyle.xlValidAlertStop, Excel.XlFormatConditionOperator.xlBetween, "=" + mapping.ValidationRangeName, Type.Missing);
                cell.Validation.IgnoreBlank = true;
                cell.Validation.InCellDropdown = true;
                cell.Validation.ShowInput = true;
                cell.Validation.InputTitle = "Report-row mapping";
                cell.Validation.InputMessage = "Select one report row or EXCLUDED.";
                cell.Validation.ShowError = true;
                cell.Validation.ErrorTitle = "Invalid mapping";
                cell.Validation.ErrorMessage = "Select a value from the available mapping list.";
            }
        }

        private static void ApplyStatusFormula(Excel.ListObject table)
        {
            Excel.ListColumn statusColumn = table.ListColumns["MappingStatus"];
            Excel.Range statusRange = statusColumn.DataBodyRange;
            statusRange.Formula = "=IF([@MappedTo]=\"\",\"Unresolved\"," + "IF([@MappedTo]=\"EXCLUDED\",\"Excluded\",\"Mapped\"))";
        }

        private static void AddSubtotalFormulas(Excel.Worksheet worksheet)
        {
            SetSubtotalFormula(worksheet, 1, "=SUBTOTAL(3,AnalyticalMappings[AccountCode])", "0");
            SetSubtotalFormula(worksheet, 5, "=SUBTOTAL(109,AnalyticalMappings[DebitBalance])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 6, "=SUBTOTAL(109,AnalyticalMappings[CreditBalance])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 7, "=SUBTOTAL(109,AnalyticalMappings[NetBalance])", "#,##0.00;[Red]-#,##0.00");
        }

        private static void SetSubtotalFormula(Excel.Worksheet worksheet, int columnNumber, string formula, string numberFormat)
        {
            Excel.Range cell =(Excel.Range)worksheet.Cells[3, columnNumber];
            cell.Formula = formula;
            cell.NumberFormat = numberFormat;
            cell.Font.Bold = true;
        }

        private static void ApplyWorksheetLayout(Excel.Worksheet worksheet, Excel.ListObject table)
        {
            Excel.Range headerRange = table.HeaderRowRange;
            headerRange.WrapText = false;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            headerRange.RowHeight = 20;

            SetColumnWidth(worksheet, 1, 16);
            SetColumnWidth(worksheet, 2, 28);
            SetColumnWidth(worksheet, 3, 22);
            SetColumnWidth(worksheet, 4, 32);
            SetColumnWidth(worksheet, 5, 16);
            SetColumnWidth(worksheet, 6, 16);
            SetColumnWidth(worksheet, 7, 16);
            SetColumnWidth(worksheet, 8, 75);
            SetColumnWidth(worksheet, 9, 16);

            Excel.Range mappedToRange = table.ListColumns["MappedTo"].DataBodyRange;

            // Pale-yellow fill indicating auditor-editable input cells.
            // Avoid Excel built-in style names because they are localized.
            mappedToRange.Interior.Color = 13434879;
            mappedToRange.Locked = false;
        }

        private static void SetColumnWidth(Excel.Worksheet worksheet, int columnNumber, double width)
        {
            Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
            column.ColumnWidth = width;
        }
    }
}
