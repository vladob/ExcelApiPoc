using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzReportsWorksheetWriter
    {
        private const string WorksheetName = "RegisterUZ Reports";
        private const string TableName = "RegisterUzReports";
        private const int HeaderRow = 4;

        private static readonly string[] Headers =
        {
            "Action",
            "TargetWorksheetName",
            "FiscalYear",
            "PeriodFrom",
            "PeriodTo",
            "ReportType",
            "ParentType",
            "TemplateName",
            "TableName",
            "TemplateId",
            "TemplateTableId",
            "FinancialStatementId",
            "AnnualReportId",
            "FinancialReportId",
            "ReportOrdinal",
            "FinancialReportTableId",
            "TableOrdinal",
            "CurrencyCode",
            "DataAvailability",
            "SubmissionDate",
            "CompletionDate"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            AccountingEntityPackageEnvelope package)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            IReadOnlyList<RegisterUzReportListRow> rows =
                RegisterUzReportsWorksheetBuilder.Build(package);

            Excel.Application application =
                (Excel.Application)ExcelDnaUtil.Application;
            object previousActiveSheet = application.ActiveSheet;

            Excel.Worksheet worksheet =
                (Excel.Worksheet)workbook.Worksheets.Add(
                    After: workbook.Worksheets[workbook.Worksheets.Count]);
            worksheet.Name = WorksheetName;

            int lastRow = HeaderRow + rows.Count;
            Excel.Range firstCell =
                (Excel.Range)worksheet.Cells[HeaderRow, 1];
            Excel.Range lastCell =
                (Excel.Range)worksheet.Cells[lastRow, Headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            tableRange.Value2 = CreateValues(rows);

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            ApplyFormats(worksheet, table, rows.Count);
            ApplyLayout(worksheet, table);

            worksheet.Activate();
            Excel.Window window = application.ActiveWindow;
            window.SplitRow = HeaderRow;
            window.SplitColumn = 2;
            window.FreezePanes = true;

            if (previousActiveSheet is Excel.Worksheet previousWorksheet)
                previousWorksheet.Activate();

            return worksheet;
        }

        private static object[,] CreateValues(
            IReadOnlyList<RegisterUzReportListRow> rows)
        {
            var values = new object[rows.Count + 1, Headers.Length];

            for (int columnIndex = 0;
                 columnIndex < Headers.Length;
                 columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                RegisterUzReportListRow row = rows[rowIndex];
                int targetRow = rowIndex + 1;

                values[targetRow, 0] = row.Action;
                values[targetRow, 1] = row.TargetWorksheetName;
                values[targetRow, 2] = row.FiscalYear.HasValue
                    ? (object)row.FiscalYear.Value
                    : null;
                values[targetRow, 3] = row.PeriodFrom;
                values[targetRow, 4] = row.PeriodTo;
                values[targetRow, 5] = row.ReportType;
                values[targetRow, 6] = row.ParentType;
                values[targetRow, 7] = row.TemplateName;
                values[targetRow, 8] = row.TableName;
                values[targetRow, 9] = ToExcelNumber(row.TemplateId);
                values[targetRow, 10] = ToExcelNumber(row.TemplateTableId);
                values[targetRow, 11] = ToExcelNumber(row.FinancialStatementId);
                values[targetRow, 12] = ToExcelNumber(row.AnnualReportId);
                values[targetRow, 13] = (double)row.FinancialReportId;
                values[targetRow, 14] = row.ReportOrdinal;
                values[targetRow, 15] = (double)row.FinancialReportTableId;
                values[targetRow, 16] = row.TableOrdinal;
                values[targetRow, 17] = row.CurrencyCode;
                values[targetRow, 18] = row.DataAvailability;
                values[targetRow, 19] = row.SubmissionDate.HasValue
                    ? (object)row.SubmissionDate.Value
                    : null;
                values[targetRow, 20] = row.CompletionDate.HasValue
                    ? (object)row.CompletionDate.Value
                    : null;
            }

            return values;
        }

        private static object ToExcelNumber(long? value)
        {
            return value.HasValue ? (object)(double)value.Value : null;
        }

        private static void ApplyFormats(
            Excel.Worksheet worksheet,
            Excel.ListObject table,
            int rowCount)
        {
            if (rowCount == 0 || table.DataBodyRange == null)
                return;

            Excel.Range dataRange = table.DataBodyRange;

            ((Excel.Range)dataRange.Columns[4]).NumberFormat = "@";
            ((Excel.Range)dataRange.Columns[5]).NumberFormat = "@";

            for (int columnNumber = 10; columnNumber <= 17; columnNumber++)
            {
                ((Excel.Range)dataRange.Columns[columnNumber]).NumberFormat = "0";
            }

            ((Excel.Range)dataRange.Columns[20]).NumberFormat = "yyyy-mm-dd";
            ((Excel.Range)dataRange.Columns[21]).NumberFormat = "yyyy-mm-dd";
        }

        private static void ApplyLayout(
            Excel.Worksheet worksheet,
            Excel.ListObject table)
        {
            Excel.Range header = table.HeaderRowRange;
            header.WrapText = false;
            header.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            header.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            header.RowHeight = 20;

            SetColumnWidth(worksheet, 1, 12);
            SetColumnWidth(worksheet, 2, 31);
            SetColumnWidth(worksheet, 3, 11);
            SetColumnWidth(worksheet, 4, 12);
            SetColumnWidth(worksheet, 5, 12);
            SetColumnWidth(worksheet, 6, 18);
            SetColumnWidth(worksheet, 7, 20);
            SetColumnWidth(worksheet, 8, 34);
            SetColumnWidth(worksheet, 9, 34);

            for (int columnNumber = 10;
                 columnNumber <= Headers.Length;
                 columnNumber++)
            {
                SetColumnWidth(worksheet, columnNumber, 18);
            }
        }

        private static void SetColumnWidth(
            Excel.Worksheet worksheet,
            int columnNumber,
            double width)
        {
            Excel.Range column =
                (Excel.Range)worksheet.Columns[columnNumber];
            column.ColumnWidth = width;
        }
    }
}
