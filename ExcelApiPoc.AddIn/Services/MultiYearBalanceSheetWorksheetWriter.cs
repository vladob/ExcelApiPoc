using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class MultiYearBalanceSheetWorksheetWriter
    {
        private const string WorksheetName = "Multi-year Balance Sheet";
        private const int TitleRow = 1;
        private const int EntityRow = 2;
        private const int TemplateRow = 3;
        private const int HeaderRow = 5;
        private const int FirstDataRow = HeaderRow + 1;
        private const int FixedColumnCount = 4;
        private const int HasDataColumn = 4;

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            AccountingEntityPackageEnvelope package,
            RegisterUzFinancialReportSelection auditedReport)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            MultiYearBalanceSheet balanceSheet =
                MultiYearBalanceSheetBuilder.Build(package, auditedReport);

            Excel.Application application =
                (Excel.Application)ExcelDnaUtil.Application;
            object previousActiveSheet = application.ActiveSheet;

            Excel.Worksheet worksheet =
                (Excel.Worksheet)workbook.Worksheets.Add(
                    After: workbook.Worksheets[workbook.Worksheets.Count]);
            worksheet.Name = WorksheetName;
            worksheet.Visible = Excel.XlSheetVisibility.xlSheetVisible;

            int totalColumns =
                               FixedColumnCount + balanceSheet.FiscalYears.Count;
            int lastRow = FirstDataRow + balanceSheet.Rows.Count - 1;

            Excel.Range renderedRange = worksheet.Range[
                worksheet.Cells[TitleRow, 1],
                worksheet.Cells[lastRow, totalColumns]];
            renderedRange.Value2 = CreateValues(
                balanceSheet,
                totalColumns,
                lastRow);

            CreateTable(
                workbook,
                worksheet,
                lastRow,
                totalColumns);

            ApplyFormatting(
                worksheet,
                balanceSheet,
                totalColumns,
                lastRow);
            ApplyColumnWidths(
                worksheet,
                balanceSheet.FiscalYears.Count);
            ApplyPrintSetup(
                worksheet,
                totalColumns,
                lastRow);

            worksheet.Activate();
            Excel.Window window = application.ActiveWindow;
            window.SplitRow = HeaderRow;
            window.SplitColumn = FixedColumnCount;
            window.FreezePanes = true;

            if (previousActiveSheet is Excel.Worksheet previousWorksheet)
                previousWorksheet.Activate();

            return worksheet;
        }

        private static object[,] CreateValues(
            MultiYearBalanceSheet balanceSheet,
            int totalColumns,
            int lastRow)
        {
            var values = new object[lastRow, totalColumns];

            values[TitleRow - 1, 0] = WorksheetName;
            values[EntityRow - 1, 0] =
                balanceSheet.Entity.Name + " | IČO: " +
                balanceSheet.Entity.Ico;
            values[TemplateRow - 1, 0] =
                "RegisterUZ template 690 | " + balanceSheet.TemplateName;

            values[HeaderRow - 1, 0] = "Designation";
            values[HeaderRow - 1, 1] = "Description";
            values[HeaderRow - 1, 2] = "Row number";
            values[HeaderRow - 1, HasDataColumn - 1] = "Has data";

            for (int yearIndex = 0;
                 yearIndex < balanceSheet.FiscalYears.Count;
                 yearIndex++)
            {
                values[HeaderRow - 1, FixedColumnCount + yearIndex] =
                    balanceSheet.FiscalYears[yearIndex];
            }

            for (int rowIndex = 0;
                 rowIndex < balanceSheet.Rows.Count;
                 rowIndex++)
            {
                MultiYearBalanceSheetRow row = balanceSheet.Rows[rowIndex];
                int targetRow = FirstDataRow - 1 + rowIndex;

                values[targetRow, 0] = row.Designation;
                values[targetRow, 1] = row.Description;
                values[targetRow, 2] = row.RowNumber;
                values[targetRow, HasDataColumn - 1] = row.HasData.HasValue
                    ? (object)row.HasData.Value
                    : null;

                for (int yearIndex = 0;
                     yearIndex < balanceSheet.FiscalYears.Count;
                     yearIndex++)
                {
                    decimal value;
                    if (row.ValuesByFiscalYear.TryGetValue(
                            balanceSheet.FiscalYears[yearIndex],
                            out value))
                    {
                        values[targetRow, FixedColumnCount + yearIndex] =
                            (double)value;
                    }
                }
            }

            return values;
        }

        private static void ApplyFormatting(
            Excel.Worksheet worksheet,
            MultiYearBalanceSheet balanceSheet,
            int totalColumns,
            int lastRow)
        {
            Excel.Range rendered = worksheet.Range[
                worksheet.Cells[TitleRow, 1],
                worksheet.Cells[lastRow, totalColumns]];
            rendered.Font.Name = "Aptos Narrow";
            rendered.Font.Size = 10;

            Excel.Range title = worksheet.Range[
                worksheet.Cells[TitleRow, 1],
                worksheet.Cells[TitleRow, totalColumns]];
            title.Font.Bold = true;
            title.Font.Size = 14;

            Excel.Range header = worksheet.Range[
                worksheet.Cells[HeaderRow, 1],
                worksheet.Cells[HeaderRow, totalColumns]];
            header.Font.Bold = true;
            header.WrapText = true;
            header.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            header.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            header.Interior.Color = 15773696;
            ApplyBorders(header);

            Excel.Range data = worksheet.Range[
                worksheet.Cells[FirstDataRow, 1],
                worksheet.Cells[lastRow, totalColumns]];
            data.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            ApplyBorders(data);

            Excel.Range descriptive = worksheet.Range[
                worksheet.Cells[FirstDataRow, 1],
                worksheet.Cells[lastRow, 3]];
            descriptive.WrapText = true;

            ((Excel.Range)worksheet.Range[
                worksheet.Cells[FirstDataRow, 1],
                worksheet.Cells[lastRow, 1]])
                .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ((Excel.Range)worksheet.Range[
                worksheet.Cells[FirstDataRow, 3],
                worksheet.Cells[lastRow, 3]])
                .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            ((Excel.Range)worksheet.Range[
                worksheet.Cells[FirstDataRow, HasDataColumn],
                worksheet.Cells[lastRow, HasDataColumn]])
                .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            if (balanceSheet.FiscalYears.Count > 0)
            {
                Excel.Range numeric = worksheet.Range[
                    worksheet.Cells[FirstDataRow, FixedColumnCount + 1],
                    worksheet.Cells[lastRow, totalColumns]];
                numeric.NumberFormat = "#,##0;[Red]-#,##0;–";
                numeric.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            }

            for (int rowIndex = 0;
                 rowIndex < balanceSheet.Rows.Count;
                 rowIndex++)
            {
                if (!balanceSheet.Rows[rowIndex].IsSumRow)
                    continue;

                Excel.Range sumRow = worksheet.Range[
                    worksheet.Cells[FirstDataRow + rowIndex, 1],
                    worksheet.Cells[FirstDataRow + rowIndex, totalColumns]];
                sumRow.Font.Bold = true;
            }

            data.Rows.RowHeight = 15;
            header.Rows.AutoFit();
        }

        private static void CreateTable(
            Excel.Workbook workbook,
            Excel.Worksheet worksheet,
            int lastRow,
            int totalColumns)
        {
            Excel.Range tableRange = worksheet.Range[
                worksheet.Cells[HeaderRow, 1],
                worksheet.Cells[lastRow, totalColumns]];

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);

            table.Name = ExcelTableNameHelper.CreateUniqueName(
                workbook,
                "MultiYearBalanceSheet");
            table.TableStyle = "TableStyleMedium2";
        }

        private static void ApplyBorders(Excel.Range range)
        {
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Borders.Weight = Excel.XlBorderWeight.xlThin;
            range.Borders.Color = 12566463;
        }

        private static void ApplyColumnWidths(
            Excel.Worksheet worksheet,
            int fiscalYearCount)
        {
            ((Excel.Range)worksheet.Columns[1]).ColumnWidth = 12;
            ((Excel.Range)worksheet.Columns[2]).ColumnWidth = 70;
            ((Excel.Range)worksheet.Columns[3]).ColumnWidth = 12;
            ((Excel.Range)worksheet.Columns[HasDataColumn]).ColumnWidth = 6;

            for (int yearIndex = 0; yearIndex < fiscalYearCount; yearIndex++)
            {
                ((Excel.Range)worksheet.Columns[
                    FixedColumnCount + yearIndex + 1]).ColumnWidth = 14;
            }
        }

        private static void ApplyPrintSetup(
            Excel.Worksheet worksheet,
            int totalColumns,
            int lastRow)
        {
            Excel.Range printRange = worksheet.Range[
                worksheet.Cells[TitleRow, 1],
                worksheet.Cells[lastRow, totalColumns]];

            worksheet.PageSetup.PrintArea = printRange.Address;
            worksheet.PageSetup.PrintTitleRows = "$1:$5";
            worksheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperA4;
            worksheet.PageSetup.Orientation =
                Excel.XlPageOrientation.xlLandscape;
            worksheet.PageSetup.Zoom = false;
            worksheet.PageSetup.FitToPagesWide = 1;
            worksheet.PageSetup.FitToPagesTall = false;
            worksheet.PageSetup.CenterHorizontally = true;
            worksheet.PageSetup.LeftMargin =
                worksheet.Application.CentimetersToPoints(0.7);
            worksheet.PageSetup.RightMargin =
                worksheet.Application.CentimetersToPoints(0.7);
            worksheet.PageSetup.TopMargin =
                worksheet.Application.CentimetersToPoints(1.0);
            worksheet.PageSetup.BottomMargin =
                worksheet.Application.CentimetersToPoints(1.0);
        }
    }
}
