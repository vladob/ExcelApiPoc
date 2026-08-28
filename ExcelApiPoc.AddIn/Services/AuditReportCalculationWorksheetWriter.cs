using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditReportCalculationWorksheetWriter
    {
        private const string WorksheetName = "Calculation Results";
        private const string TableName = "CalculatedReportRows";
        private const int HeaderRow = 4;

        private static readonly string[] Headers =
        {
            "TableErpId", "ReportTable", "RowNumber", "RowCaption", "IsSumRow",
            "PrimaryValue", "SecondaryValue", "CalculatedValue"
        };

        public static Excel.Worksheet Write(Excel.Workbook workbook, AuditTemplatePackageResponse package, AuditReportCalculationResult calculation)
        {
            Excel.Worksheet worksheet = GetOrCreateWorksheet(workbook);
            ClearWorksheet(worksheet);

            Dictionary<string, AuditReportRowDefinitionResponse> definitions = CreateDefinitionIndex(package);
            Dictionary<int, string> tableNames = (package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>()).ToDictionary(reportTable => reportTable.TableErpId, reportTable => reportTable.NameSk);
            AuditReportRowCalculation[] rows = calculation.Rows.OrderBy(row => row.TableErpId).ThenBy(row => row.RowNumber).ToArray();

            var values = new object[rows.Length + 1, Headers.Length];

            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
                values[0, columnIndex] = Headers[columnIndex];

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                AuditReportRowCalculation calculationRow = rows[rowIndex];
                string key = CreateRowKey(calculationRow.TableErpId, calculationRow.RowNumber);

                if (!definitions.TryGetValue(key, out AuditReportRowDefinitionResponse definition))
                    throw new InvalidOperationException($"Missing definition for calculated report row {key}.");

                values[rowIndex + 1, 0] = calculationRow.TableErpId;
                values[rowIndex + 1, 1] = tableNames[calculationRow.TableErpId];
                values[rowIndex + 1, 2] = calculationRow.RowNumber;
                values[rowIndex + 1, 3] = definition.TextSk;
                values[rowIndex + 1, 4] = definition.IsSumRow;
                values[rowIndex + 1, 5] = (double)calculationRow.PrimaryValue;
                values[rowIndex + 1, 6] = (double)calculationRow.SecondaryValue;
                values[rowIndex + 1, 7] = (double)calculationRow.CalculatedValue;
            }

            int lastRow = HeaderRow + rows.Length;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, Headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            tableRange.Value2 = values;

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            Excel.Range countCell = (Excel.Range)worksheet.Cells[3, 1];
            countCell.Formula = "=SUBTOTAL(3,CalculatedReportRows[RowNumber])";
            countCell.NumberFormat = "0";
            countCell.Font.Bold = true;

            Excel.Range dataRange = table.DataBodyRange;
            ((Excel.Range)dataRange.Columns[1]).NumberFormat = "0";
            ((Excel.Range)dataRange.Columns[3]).NumberFormat = "0";

            for (int columnNumber = 6; columnNumber <= 8; columnNumber++)
                ((Excel.Range)dataRange.Columns[columnNumber]).NumberFormat = "#,##0.00;[Red]-#,##0.00";

            ((Excel.Range)worksheet.Columns[1]).ColumnWidth = 14;
            ((Excel.Range)worksheet.Columns[2]).ColumnWidth = 22;
            ((Excel.Range)worksheet.Columns[3]).ColumnWidth = 14;
            ((Excel.Range)worksheet.Columns[4]).ColumnWidth = 70;
            ((Excel.Range)worksheet.Columns[5]).ColumnWidth = 12;

            for (int columnNumber = 6; columnNumber <= 8; columnNumber++)
                ((Excel.Range)worksheet.Columns[columnNumber]).ColumnWidth = 18;

            worksheet.Visible = Excel.XlSheetVisibility.xlSheetVisible;
            worksheet.Activate();

            Excel.Window window = workbook.Application.ActiveWindow;
            window.SplitRow = HeaderRow;
            window.SplitColumn = 0;
            window.FreezePanes = true;

            return worksheet;
        }

        private static Excel.Worksheet GetOrCreateWorksheet(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                if (string.Equals(worksheet.Name, WorksheetName, StringComparison.OrdinalIgnoreCase))
                    return worksheet;
            }

            Excel.Worksheet lastWorksheet =
                (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];

            Excel.Worksheet created =
                (Excel.Worksheet)workbook.Worksheets.Add(After: lastWorksheet);

            created.Name = WorksheetName;
            return created;
        }

        private static void ClearWorksheet(Excel.Worksheet worksheet)
        {
            for (int index = worksheet.ListObjects.Count; index >= 1; index--)
                worksheet.ListObjects[index].Delete();

            worksheet.Cells.Clear();
        }

        private static Dictionary<string, AuditReportRowDefinitionResponse> CreateDefinitionIndex(AuditTemplatePackageResponse package)
        {
            var result = new Dictionary<string, AuditReportRowDefinitionResponse>(StringComparer.Ordinal);

            foreach (AuditReportTableDefinitionResponse table in
                package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
            {
                foreach (AuditReportRowDefinitionResponse row in
                    table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                {
                    if (row.RowNumber.HasValue)
                        result.Add(CreateRowKey(table.TableErpId, row.RowNumber.Value), row);
                }
            }
            return result;
        }

        private static string CreateRowKey(int tableErpId, int rowNumber)
        {
            return tableErpId + ":" + rowNumber;
        }
    }
}
