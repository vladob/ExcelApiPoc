using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountingFrameworkWorksheetWriter
    {
        private const string WorksheetName = "Accounting Framework";
        private const string TableName = "AccountingFrameworkRows";
        private const int HeaderRow = 4;
        private static readonly string[] Headers =
        {
            "SequenceNumber", "SourceRecordNumber", "RowKind", "SourceSyntheticCode",
            "SourceAnalyticalCode", "SyntheticCode", "AnalyticalCode", "AccountCode",
            "AccountName", "Type", "SubsidiaryFlag", "TaxFlag", "BalanceFlag", "VatFlag"
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook, AccountingFrameworkImport import)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (import == null) throw new ArgumentNullException(nameof(import));
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            object previous = application.ActiveSheet;
            Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            sheet.Name = WorksheetName;
            int lastRow = HeaderRow + import.Rows.Count;
            Excel.Range range = sheet.Range[sheet.Cells[HeaderRow, 1], sheet.Cells[lastRow, Headers.Length]];
            Excel.Range data = sheet.Range[sheet.Cells[HeaderRow + 1, 1], sheet.Cells[lastRow, Headers.Length]];
            for (int column = 3; column <= Headers.Length; column++) ((Excel.Range)data.Columns[column]).NumberFormat = "@";
            range.Value2 = CreateValues(import);
            Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, range, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            AddSubtotalCount(sheet);
            table.HeaderRowRange.Columns.AutoFit();
            ((Excel.Range)sheet.Columns[9]).ColumnWidth = 45;
            sheet.Activate();
            application.ActiveWindow.SplitRow = HeaderRow;
            application.ActiveWindow.SplitColumn = 0;
            application.ActiveWindow.FreezePanes = true;
            if (previous is Excel.Worksheet previousSheet) previousSheet.Activate();
            return sheet;
        }

        private static void AddSubtotalCount(Excel.Worksheet worksheet)
        {
            Excel.Range countCell = (Excel.Range)worksheet.Cells[3, 1];
            countCell.Formula =
                "=SUBTOTAL(3,AccountingFrameworkRows[SequenceNumber])";
            countCell.NumberFormat = "0";
            countCell.Font.Bold = true;
        }

        private static object[,] CreateValues(AccountingFrameworkImport import)
        {
            var values = new object[import.Rows.Count + 1, Headers.Length];
            for (int c = 0; c < Headers.Length; c++) values[0, c] = Headers[c];
            for (int r = 0; r < import.Rows.Count; r++)
            {
                AccountingFrameworkRow row = import.Rows[r];
                int x = r + 1;
                values[x, 0] = row.SequenceNumber; values[x, 1] = row.SourceRecordNumber;
                values[x, 2] = row.RowKind.ToString(); values[x, 3] = row.SourceSyntheticCode;
                values[x, 4] = row.SourceAnalyticalCode; values[x, 5] = row.SyntheticCode;
                values[x, 6] = row.AnalyticalCode; values[x, 7] = row.AccountCode;
                values[x, 8] = row.AccountName; values[x, 9] = row.Type;
                values[x, 10] = row.SubsidiaryFlag; values[x, 11] = row.TaxFlag;
                values[x, 12] = row.BalanceFlag; values[x, 13] = row.VatFlag;
            }
            return values;
        }
    }
}
