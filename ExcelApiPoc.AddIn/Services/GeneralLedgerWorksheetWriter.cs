using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class GeneralLedgerWorksheetWriter
    {
        private const string WorksheetName = "General Ledger";
        private const string TableName = "GeneralLedgerRows";
        private const int HeaderRow = 4;
        private static readonly string[] Headers =
        {
            "SequenceNumber", "SourceRecordNumber", "SyntheticCode", "AnalyticalCode", "AccountCode",
            "Type", "P", "Section", "Item", "FundingSource", "Program", "CostCenter", "Order", "AccountName",
            "OpeningDebit", "OpeningCredit", "AnnualDebitTurnover", "AnnualCreditTurnover",
            "PeriodDebitTurnover", "PeriodCreditTurnover", "ClosingDebit", "ClosingCredit", "Plan"
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook, GeneralLedgerImport import)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (import == null) throw new ArgumentNullException(nameof(import));
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            object previous = application.ActiveSheet;
            Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            sheet.Name = WorksheetName;
            int lastRow = HeaderRow + import.Rows.Count;
            Excel.Range first = (Excel.Range)sheet.Cells[HeaderRow, 1];
            Excel.Range last = (Excel.Range)sheet.Cells[lastRow, Headers.Length];
            Excel.Range range = sheet.Range[first, last];
            Excel.Range firstData = (Excel.Range)sheet.Cells[HeaderRow + 1, 1];
            Excel.Range data = sheet.Range[firstData, last];
            for (int c = 3; c <= 14; c++) ((Excel.Range)data.Columns[c]).NumberFormat = "@";
            for (int c = 15; c <= 23; c++) ((Excel.Range)data.Columns[c]).NumberFormat = "#,##0.00;[Red]-#,##0.00";
            range.Value2 = CreateValues(import);
            Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, range, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = TableName; table.TableStyle = "TableStyleMedium2";
            AddSubtotals(sheet);
            table.HeaderRowRange.WrapText = false; table.HeaderRowRange.Columns.AutoFit();
            for (int c = 1; c <= Headers.Length; c++)
            {
                Excel.Range column = (Excel.Range)sheet.Columns[c];
                column.ColumnWidth = Math.Min(Convert.ToDouble(column.ColumnWidth) + 2, c == 14 ? 45 : 22);
            }
            sheet.Activate();
            application.ActiveWindow.SplitRow = HeaderRow; application.ActiveWindow.SplitColumn = 5;
            application.ActiveWindow.FreezePanes = true;
            if (previous is Excel.Worksheet previousSheet) previousSheet.Activate();
            return sheet;
        }

        private static object[,] CreateValues(GeneralLedgerImport import)
        {
            var v = new object[import.Rows.Count + 1, Headers.Length];
            for (int c = 0; c < Headers.Length; c++) v[0, c] = Headers[c];
            for (int i = 0; i < import.Rows.Count; i++)
            {
                GeneralLedgerRow r = import.Rows[i]; int x = i + 1;
                v[x,0]=r.SequenceNumber; v[x,1]=r.SourceRecordNumber; v[x,2]=r.SyntheticCode; v[x,3]=r.AnalyticalCode;
                v[x,4]=r.AccountCode; v[x,5]=r.Type; v[x,6]=r.P; v[x,7]=r.Section; v[x,8]=r.Item;
                v[x,9]=r.FundingSource; v[x,10]=r.Program; v[x,11]=r.CostCenter; v[x,12]=r.Order; v[x,13]=r.AccountName;
                v[x,14]=(double)r.OpeningDebit; v[x,15]=(double)r.OpeningCredit; v[x,16]=(double)r.AnnualDebitTurnover;
                v[x,17]=(double)r.AnnualCreditTurnover; v[x,18]=(double)r.PeriodDebitTurnover; v[x,19]=(double)r.PeriodCreditTurnover;
                v[x,20]=(double)r.ClosingDebit; v[x,21]=(double)r.ClosingCredit; v[x,22]=(double)r.Plan;
            }
            return v;
        }

        private static void AddSubtotals(Excel.Worksheet sheet)
        {
            SetSubtotal(sheet, 1, "=SUBTOTAL(3,GeneralLedgerRows[SequenceNumber])", "0");
            for (int c = 15; c <= Headers.Length; c++)
                SetSubtotal(sheet, c, "=SUBTOTAL(109,GeneralLedgerRows[" + Headers[c - 1] + "])", "#,##0.00;[Red]-#,##0.00");
        }

        private static void SetSubtotal(Excel.Worksheet sheet, int column, string formula, string format)
        {
            Excel.Range cell = (Excel.Range)sheet.Cells[3, column];
            cell.Formula = formula; cell.NumberFormat = format; cell.Font.Bold = true;
        }
    }
}
