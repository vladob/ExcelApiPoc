using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzAttachmentsWorksheetWriter
    {
        private const string WorksheetName = "RegisterUZ Attachments";
        private const string TableName = "RegisterUzAttachments";
        private const int HeaderRow = 4;

        private static readonly string[] Headers =
        {
            "Action",
            "FileName",
            "FiscalYear",
            "PeriodFrom",
            "PeriodTo",
            "OwnerType",
            "ParentType",
            "ReportType",
            "TemplateName",
            "MimeType",
            "FileSizeBytes",
            "PageCount",
            "LanguageCode",
            "Url",
            "TemplateId",
            "FinancialStatementId",
            "AnnualReportId",
            "FinancialReportId",
            "ReportOrdinal",
            "AttachmentId",
            "AttachmentOrdinal",
            "DataAvailability",
            "SubmissionDate"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            AccountingEntityPackageEnvelope package)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            IReadOnlyList<RegisterUzAttachmentListRow> rows =
                RegisterUzAttachmentsWorksheetBuilder.Build(package);

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

            ApplyHyperlinks(worksheet, rows);
            ApplyFormats(table, rows.Count);
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
            IReadOnlyList<RegisterUzAttachmentListRow> rows)
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
                RegisterUzAttachmentListRow row = rows[rowIndex];
                int targetRow = rowIndex + 1;

                values[targetRow, 0] = row.Action;
                values[targetRow, 1] = row.FileName;
                values[targetRow, 2] = ToExcelNumber(row.FiscalYear);
                values[targetRow, 3] = row.PeriodFrom;
                values[targetRow, 4] = row.PeriodTo;
                values[targetRow, 5] = row.OwnerType;
                values[targetRow, 6] = row.ParentType;
                values[targetRow, 7] = row.ReportType;
                values[targetRow, 8] = row.TemplateName;
                values[targetRow, 9] = row.MimeType;
                values[targetRow, 10] = ToExcelNumber(row.FileSizeBytes);
                values[targetRow, 11] = ToExcelNumber(row.PageCount);
                values[targetRow, 12] = row.LanguageCode;
                values[targetRow, 13] = row.Url;
                values[targetRow, 14] = ToExcelNumber(row.TemplateId);
                values[targetRow, 15] = ToExcelNumber(row.FinancialStatementId);
                values[targetRow, 16] = ToExcelNumber(row.AnnualReportId);
                values[targetRow, 17] = ToExcelNumber(row.FinancialReportId);
                values[targetRow, 18] = ToExcelNumber(row.ReportOrdinal);
                values[targetRow, 19] = (double)row.AttachmentId;
                values[targetRow, 20] = row.AttachmentOrdinal;
                values[targetRow, 21] = row.DataAvailability;
                values[targetRow, 22] = row.SubmissionDate.HasValue
                    ? (object)row.SubmissionDate.Value
                    : null;
            }

            return values;
        }

        private static object ToExcelNumber(long? value)
        {
            return value.HasValue ? (object)(double)value.Value : null;
        }

        private static object ToExcelNumber(int? value)
        {
            return value.HasValue ? (object)value.Value : null;
        }

        private static void ApplyHyperlinks(
            Excel.Worksheet worksheet,
            IReadOnlyList<RegisterUzAttachmentListRow> rows)
        {
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                RegisterUzAttachmentListRow row = rows[rowIndex];
                Excel.Range actionCell =
                    (Excel.Range)worksheet.Cells[
                        HeaderRow + 1 + rowIndex,
                        1];

                worksheet.Hyperlinks.Add(
                    actionCell,
                    row.Url,
                    Type.Missing,
                    "Open attachment in RegisterUZ",
                    row.Action);
            }
        }

        private static void ApplyFormats(
            Excel.ListObject table,
            int rowCount)
        {
            if (rowCount == 0 || table.DataBodyRange == null)
                return;

            Excel.Range dataRange = table.DataBodyRange;
            ((Excel.Range)dataRange.Columns[4]).NumberFormat = "@";
            ((Excel.Range)dataRange.Columns[5]).NumberFormat = "@";

            ((Excel.Range)dataRange.Columns[11]).NumberFormat = "0";
            ((Excel.Range)dataRange.Columns[12]).NumberFormat = "0";

            for (int columnNumber = 15; columnNumber <= 21; columnNumber++)
            {
                ((Excel.Range)dataRange.Columns[columnNumber]).NumberFormat = "0";
            }

            ((Excel.Range)dataRange.Columns[23]).NumberFormat = "yyyy-mm-dd";
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
            SetColumnWidth(worksheet, 2, 48);
            SetColumnWidth(worksheet, 3, 11);
            SetColumnWidth(worksheet, 4, 12);
            SetColumnWidth(worksheet, 5, 12);
            SetColumnWidth(worksheet, 6, 20);
            SetColumnWidth(worksheet, 7, 20);
            SetColumnWidth(worksheet, 8, 20);
            SetColumnWidth(worksheet, 9, 34);
            SetColumnWidth(worksheet, 10, 20);
            SetColumnWidth(worksheet, 11, 18);
            SetColumnWidth(worksheet, 12, 12);
            SetColumnWidth(worksheet, 13, 14);
            SetColumnWidth(worksheet, 14, 60);

            for (int columnNumber = 15;
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
