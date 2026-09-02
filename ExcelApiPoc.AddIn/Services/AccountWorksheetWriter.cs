using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountWorksheetWriter
    {
        private const string WorksheetName = "Accounts";
        private const string TableName = "AccountRows";
        private const int HeaderRow = 4;
        private const int FirstColumn = 1;

        private static readonly string[] Headers =
        {
            "AccountCode",
            "AccountName",
            "AccountNameSource",
            "EntityAccountName",
            "SyntheticAccountCode",
            "FrameworkAccountCode",
            "FrameworkAccountName",
            "IsFrameworkMatch",
            "DebitEntryCount",
            "DebitTurnover",
            "CreditEntryCount",
            "CreditTurnover",
            "NetBalance",
            "DebitBalance",
            "CreditBalance",
            "GeneralLedgerAccountName",
            "AccountNameComparisonStatus",
            "JournalLedgerOpeningDebit",
            "JournalLedgerOpeningCredit",
            "JournalLedgerDebitTurnover",
            "JournalLedgerCreditTurnover",
            "JournalLedgerClosingBalance",
            "LedgerOpeningDebit",
            "LedgerOpeningCredit",
            "LedgerDebitTurnover",
            "LedgerCreditTurnover",
            "LedgerClosingDebit",
            "LedgerClosingCredit",
            "OpeningDebitDifference",
            "OpeningCreditDifference",
            "DebitTurnoverDifference",
            "CreditTurnoverDifference",
            "ClosingBalanceDifference",
            "LedgerReconciliationStatus"
        };

        private static readonly int[] TextColumns =
        {
            1, // AccountCode
            2, // AccountName
            3, // AccountNameSource
            4, // EntityAccountName
            5, // SyntheticAccountCode
            6, // FrameworkAccountCode
            7, // FrameworkAccountName
            16, // GeneralLedgerAccountName
            17, // AccountNameComparisonStatus
            34  // LedgerReconciliationStatus
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook,IReadOnlyList<AccountSummary> accounts)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (accounts.Count == 0)
                throw new InvalidOperationException("The account summary does not contain any accounts.");

            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            object previousActiveSheet = application.ActiveSheet;
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            worksheet.Name = WorksheetName;

            int lastRow = HeaderRow + accounts.Count;
            int lastColumn = Headers.Length;

            Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, FirstColumn];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, lastColumn];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            Excel.Range firstDataCell = (Excel.Range)worksheet.Cells[HeaderRow + 1, FirstColumn];
            Excel.Range dataRange = worksheet.Range[firstDataCell, lastCell];

            ApplyDataFormats(dataRange);

            // One COM assignment for headers and all account rows.
            tableRange.Value2 = CreateValues(accounts);

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            AddSubtotalFormulas(worksheet);
            ApplyWorksheetLayout(worksheet, table, dataRange);

            worksheet.Activate();

            Excel.Window window = application.ActiveWindow;
            window.SplitRow = HeaderRow;
            window.SplitColumn = 1;
            window.FreezePanes = true;

            if (previousActiveSheet is Excel.Worksheet previousWorksheet)
                previousWorksheet.Activate();

            return worksheet;
        }

        private static object[,] CreateValues(IReadOnlyList<AccountSummary> accounts)
        {
            int rowCount = accounts.Count + 1;
            int columnCount = Headers.Length;

            var values = new object[rowCount, columnCount];

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            for (int rowIndex = 0; rowIndex < accounts.Count; rowIndex++)
            {
                AccountSummary account = accounts[rowIndex];
                int targetRow = rowIndex + 1;

                values[targetRow, 0] = account.AccountCode;
                values[targetRow, 1] = account.AccountName;
                values[targetRow, 2] = account.AccountNameSource;
                values[targetRow, 3] = account.EntityAccountName;
                values[targetRow, 4] = account.SyntheticAccountCode;
                values[targetRow, 5] = account.FrameworkAccountCode;
                values[targetRow, 6] = account.FrameworkAccountName;
                values[targetRow, 7] = account.IsFrameworkMatch.HasValue ? (object)account.IsFrameworkMatch.Value : null;
                values[targetRow, 8] = account.DebitEntryCount;
                values[targetRow, 9] = ToExcelNumber(account.DebitTurnover);
                values[targetRow, 10] = account.CreditEntryCount;
                values[targetRow, 11] = ToExcelNumber(account.CreditTurnover);
                values[targetRow, 12] = ToExcelNumber(account.NetBalance);
                values[targetRow, 13] = ToExcelNumber(account.DebitBalance);
                values[targetRow, 14] = ToExcelNumber(account.CreditBalance);
                values[targetRow, 15] = account.GeneralLedgerAccountName;
                values[targetRow, 16] = account.AccountNameComparisonStatus;
                values[targetRow, 17] = ToExcelNumber(account.JournalLedgerOpeningDebit);
                values[targetRow, 18] = ToExcelNumber(account.JournalLedgerOpeningCredit);
                values[targetRow, 19] = ToExcelNumber(account.JournalLedgerDebitTurnover);
                values[targetRow, 20] = ToExcelNumber(account.JournalLedgerCreditTurnover);
                values[targetRow, 21] = ToExcelNumber(account.JournalLedgerClosingBalance);
                values[targetRow, 22] = ToExcelNumber(account.LedgerOpeningDebit);
                values[targetRow, 23] = ToExcelNumber(account.LedgerOpeningCredit);
                values[targetRow, 24] = ToExcelNumber(account.LedgerDebitTurnover);
                values[targetRow, 25] = ToExcelNumber(account.LedgerCreditTurnover);
                values[targetRow, 26] = ToExcelNumber(account.LedgerClosingDebit);
                values[targetRow, 27] = ToExcelNumber(account.LedgerClosingCredit);
                values[targetRow, 28] = ToExcelNumber(account.OpeningDebitDifference);
                values[targetRow, 29] = ToExcelNumber(account.OpeningCreditDifference);
                values[targetRow, 30] = ToExcelNumber(account.DebitTurnoverDifference);
                values[targetRow, 31] = ToExcelNumber(account.CreditTurnoverDifference);
                values[targetRow, 32] = ToExcelNumber(account.ClosingBalanceDifference);
                values[targetRow, 33] = account.LedgerReconciliationStatus;
            }

            return values;
        }

        private static object ToExcelNumber(decimal value)
        {
            return (double)value;
        }

        private static void ApplyDataFormats(Excel.Range dataRange)
        {
            foreach (int columnNumber in TextColumns)
            {
                Excel.Range column = (Excel.Range)dataRange.Columns[columnNumber];

                // Preserve analytical and synthetic account identifiers.
                column.NumberFormat = "@";
            }
            Excel.Range debitEntryCountColumn = (Excel.Range)dataRange.Columns[9];
            Excel.Range creditEntryCountColumn = (Excel.Range)dataRange.Columns[11];

            debitEntryCountColumn.NumberFormat = "0";
            creditEntryCountColumn.NumberFormat = "0";

            for (int columnNumber = 10; columnNumber <= 15; columnNumber++)
            {
                if (columnNumber == 11)
                    continue;

                Excel.Range amountColumn = (Excel.Range)dataRange.Columns[columnNumber];
                amountColumn.NumberFormat = "#,##0.00;[Red]-#,##0.00";
            }
            for (int columnNumber = 18; columnNumber <= 33; columnNumber++)
                ((Excel.Range)dataRange.Columns[columnNumber]).NumberFormat =
                    "#,##0.00;[Red]-#,##0.00";
        }

        private static void AddSubtotalFormulas(Excel.Worksheet worksheet)
        {
            SetSubtotalFormula(worksheet, 1, "=SUBTOTAL(3,AccountRows[AccountCode])", "0");
            SetSubtotalFormula(worksheet, 9, "=SUBTOTAL(109,AccountRows[DebitEntryCount])", "#,##0");
            SetSubtotalFormula(worksheet, 10, "=SUBTOTAL(109,AccountRows[DebitTurnover])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 11, "=SUBTOTAL(109,AccountRows[CreditEntryCount])", "#,##0");
            SetSubtotalFormula(worksheet, 12, "=SUBTOTAL(109,AccountRows[CreditTurnover])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 13, "=SUBTOTAL(109,AccountRows[NetBalance])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 14, "=SUBTOTAL(109,AccountRows[DebitBalance])", "#,##0.00;[Red]-#,##0.00");
            SetSubtotalFormula(worksheet, 15, "=SUBTOTAL(109,AccountRows[CreditBalance])", "#,##0.00;[Red]-#,##0.00");
            for (int columnNumber = 18; columnNumber <= 33; columnNumber++)
                SetSubtotalFormula(
                    worksheet,
                    columnNumber,
                    "=SUBTOTAL(109,AccountRows[" + Headers[columnNumber - 1] + "])",
                    "#,##0.00;[Red]-#,##0.00");
        }

        private static void SetSubtotalFormula(Excel.Worksheet worksheet, int columnNumber, string formula, string numberFormat)
        {
            Excel.Range cell = (Excel.Range)worksheet.Cells[3, columnNumber];
            cell.Formula = formula;
            cell.NumberFormat = numberFormat;
            cell.Font.Bold = true;
        }

        private static void ApplyWorksheetLayout(Excel.Worksheet worksheet, Excel.ListObject table, Excel.Range dataRange)
        {
            Excel.Range headerRange = table.HeaderRowRange;
            headerRange.WrapText = false;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            headerRange.RowHeight = 20;
            headerRange.Columns.AutoFit();

            for (int columnNumber = 1; columnNumber <= Headers.Length; columnNumber++)
            {
                Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
                double currentWidth = Convert.ToDouble(column.ColumnWidth);
                column.ColumnWidth = Math.Min(currentWidth + 2, 40);
            }

            SetMinimumColumnWidth(worksheet, 1, 14);
            SetMinimumColumnWidth(worksheet, 2, 30);
            SetMinimumColumnWidth(worksheet, 3, 22);
            SetMinimumColumnWidth(worksheet, 4, 30);
            SetMinimumColumnWidth(worksheet, 5, 20);
            SetMinimumColumnWidth(worksheet, 6, 22);
            SetMinimumColumnWidth(worksheet, 7, 30);
            SetMinimumColumnWidth(worksheet, 16, 30);
            SetMinimumColumnWidth(worksheet, 17, 24);
            SetMinimumColumnWidth(worksheet, 34, 24);

            Excel.Range accountCodeColumn = (Excel.Range)dataRange.Columns[1];
            Excel.Range accountNameColumn = (Excel.Range)dataRange.Columns[2];
            accountCodeColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
            accountNameColumn.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
        }

        private static void SetMinimumColumnWidth(Excel.Worksheet worksheet, int columnNumber, double minimumWidth)
        {
            Excel.Range column = (Excel.Range)worksheet.Columns[columnNumber];
            double currentWidth = Convert.ToDouble(column.ColumnWidth);
            if (currentWidth < minimumWidth) column.ColumnWidth = minimumWidth;
        }
    }
}
