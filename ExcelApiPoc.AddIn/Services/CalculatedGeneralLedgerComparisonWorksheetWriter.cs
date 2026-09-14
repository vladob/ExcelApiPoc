using ExcelApiPoc.AccountingImport.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class CalculatedGeneralLedgerComparisonWorksheetWriter
    {
        private const string WorksheetName = "GL Comparison";
        private const string TableName = "GeneralLedgerComparisonRows";
        private const int HeaderRow = 4;
        private const decimal Tolerance = 0.01m;

        private static readonly string[] Headers =
        {
            "AccountCode",
            "OpeningAvailableFromJournal",
            "CalculatedOpeningDebit",
            "CalculatedOpeningCredit",
            "CalculatedDebitTurnover",
            "CalculatedCreditTurnover",
            "CalculatedClosingDebit",
            "CalculatedClosingCredit",
            "GeneralLedgerAccountName",
            "GeneralLedgerOpeningDebit",
            "GeneralLedgerOpeningCredit",
            "GeneralLedgerDebitTurnover",
            "GeneralLedgerCreditTurnover",
            "GeneralLedgerClosingDebit",
            "GeneralLedgerClosingCredit",
            "OpeningDebitDifference",
            "OpeningCreditDifference",
            "DebitTurnoverDifference",
            "CreditTurnoverDifference",
            "ClosingDebitDifference",
            "ClosingCreditDifference",
            "Status"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            CalculatedGeneralLedger calculated,
            JournalLedgerReconciliationResult reconciliation)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (calculated == null)
                throw new ArgumentNullException(nameof(calculated));

            Excel.Application application =
                (Excel.Application)ExcelDnaUtil.Application;
            object previous = application.ActiveSheet;

            Excel.Worksheet sheet =
                (Excel.Worksheet)workbook.Worksheets.Add(
                    After: workbook.Worksheets[workbook.Worksheets.Count]);

            sheet.Name = WorksheetName;

            List<Row> rows = BuildRows(calculated, reconciliation);
            int lastRow = HeaderRow + rows.Count;

            Excel.Range first = (Excel.Range)sheet.Cells[HeaderRow, 1];
            Excel.Range last =
                (Excel.Range)sheet.Cells[lastRow, Headers.Length];
            Excel.Range range = sheet.Range[first, last];

            if (rows.Count > 0)
            {
                Excel.Range firstData =
                    (Excel.Range)sheet.Cells[HeaderRow + 1, 1];
                Excel.Range data = sheet.Range[firstData, last];

                ((Excel.Range)data.Columns[1]).NumberFormat = "@";

                for (int column = 3; column <= 21; column++)
                {
                    ((Excel.Range)data.Columns[column]).NumberFormat =
                        "#,##0.00;[Red]-#,##0.00";
                }
            }

            range.Value2 = CreateValues(rows);

            Excel.ListObject table = sheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                range,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            AddSubtotals(sheet);
            table.HeaderRowRange.WrapText = true;
            table.HeaderRowRange.Columns.AutoFit();

            for (int column = 1; column <= Headers.Length; column++)
            {
                Excel.Range target = (Excel.Range)sheet.Columns[column];
                target.ColumnWidth = Math.Min(
                    Convert.ToDouble(target.ColumnWidth) + 2,
                    column == 9 ? 40 : 24);
            }

            sheet.Activate();
            application.ActiveWindow.SplitRow = HeaderRow;
            application.ActiveWindow.SplitColumn = 2;
            application.ActiveWindow.FreezePanes = true;

            if (previous is Excel.Worksheet previousSheet)
                previousSheet.Activate();

            return sheet;
        }

        private static List<Row> BuildRows(
            CalculatedGeneralLedger calculated,
            JournalLedgerReconciliationResult reconciliation)
        {
            var calculatedByCode = calculated.Rows.ToDictionary(
                row => row.AccountCode,
                StringComparer.Ordinal);

            var reconciledByCode =
                reconciliation == null
                    ? new Dictionary<string, JournalLedgerAccountReconciliation>(
                        StringComparer.Ordinal)
                    : reconciliation.Accounts.ToDictionary(
                        row => row.AccountCode,
                        StringComparer.Ordinal);

            IEnumerable<string> accountCodes =
                calculatedByCode.Keys
                    .Concat(reconciledByCode.Keys)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(code => code, StringComparer.Ordinal);

            var result = new List<Row>();

            foreach (string accountCode in accountCodes)
            {
                calculatedByCode.TryGetValue(
                    accountCode,
                    out CalculatedGeneralLedgerRow journal);
                reconciledByCode.TryGetValue(
                    accountCode,
                    out JournalLedgerAccountReconciliation ledger);

                result.Add(
                    CreateRow(
                        accountCode,
                        calculated.JournalContainsOpeningRecords,
                        journal,
                        ledger,
                        reconciliation != null));
            }

            return result;
        }

        private static Row CreateRow(
            string accountCode,
            bool openingAvailable,
            CalculatedGeneralLedgerRow journal,
            JournalLedgerAccountReconciliation ledger,
            bool hasImportedLedger)
        {
            var row = new Row
            {
                AccountCode = accountCode,
                OpeningAvailable = openingAvailable && journal != null
            };

            if (journal != null)
            {
                row.CalculatedOpeningDebit = journal.OpeningDebit;
                row.CalculatedOpeningCredit = journal.OpeningCredit;
                row.CalculatedDebitTurnover = journal.DebitTurnover;
                row.CalculatedCreditTurnover = journal.CreditTurnover;
                row.CalculatedClosingDebit = journal.ClosingDebit;
                row.CalculatedClosingCredit = journal.ClosingCredit;
            }

            if (ledger != null && ledger.HasGeneralLedgerAccount)
            {
                row.HasImportedLedgerAccount = true;
                row.GeneralLedgerAccountName =
                    ledger.GeneralLedgerAccountName;
                row.GeneralLedgerOpeningDebit =
                    ledger.LedgerOpeningDebit;
                row.GeneralLedgerOpeningCredit =
                    ledger.LedgerOpeningCredit;
                row.GeneralLedgerDebitTurnover =
                    ledger.LedgerDebitTurnover;
                row.GeneralLedgerCreditTurnover =
                    ledger.LedgerCreditTurnover;
                row.GeneralLedgerClosingDebit =
                    ledger.LedgerClosingDebit;
                row.GeneralLedgerClosingCredit =
                    ledger.LedgerClosingCredit;
            }

            if (!hasImportedLedger)
            {
                row.Status = "No imported general ledger";
                return row;
            }

            if (journal == null)
            {
                row.Status = "General-ledger only";
                return row;
            }

            if (!row.HasImportedLedgerAccount)
            {
                row.Status = "Journal only";
                return row;
            }

            row.DebitTurnoverDifference =
                row.CalculatedDebitTurnover -
                row.GeneralLedgerDebitTurnover;
            row.CreditTurnoverDifference =
                row.CalculatedCreditTurnover -
                row.GeneralLedgerCreditTurnover;

            if (!row.OpeningAvailable)
            {
                row.Status =
                    IsZero(row.DebitTurnoverDifference.Value) &&
                    IsZero(row.CreditTurnoverDifference.Value)
                        ? "Turnover reconciled; opening unavailable"
                        : "Different; opening unavailable";
                return row;
            }

            row.OpeningDebitDifference =
                row.CalculatedOpeningDebit -
                row.GeneralLedgerOpeningDebit;
            row.OpeningCreditDifference =
                row.CalculatedOpeningCredit -
                row.GeneralLedgerOpeningCredit;
            row.ClosingDebitDifference =
                row.CalculatedClosingDebit -
                row.GeneralLedgerClosingDebit;
            row.ClosingCreditDifference =
                row.CalculatedClosingCredit -
                row.GeneralLedgerClosingCredit;

            row.Status =
                IsZero(row.OpeningDebitDifference.Value) &&
                IsZero(row.OpeningCreditDifference.Value) &&
                IsZero(row.DebitTurnoverDifference.Value) &&
                IsZero(row.CreditTurnoverDifference.Value) &&
                IsZero(row.ClosingDebitDifference.Value) &&
                IsZero(row.ClosingCreditDifference.Value)
                    ? "Reconciled"
                    : "Different";

            return row;
        }

        private static object[,] CreateValues(IReadOnlyList<Row> rows)
        {
            var values = new object[rows.Count + 1, Headers.Length];

            for (int column = 0; column < Headers.Length; column++)
                values[0, column] = Headers[column];

            for (int index = 0; index < rows.Count; index++)
            {
                Row row = rows[index];
                int target = index + 1;

                values[target, 0] = row.AccountCode;
                values[target, 1] = row.OpeningAvailable;
                values[target, 2] = Number(row.CalculatedOpeningDebit);
                values[target, 3] = Number(row.CalculatedOpeningCredit);
                values[target, 4] = Number(row.CalculatedDebitTurnover);
                values[target, 5] = Number(row.CalculatedCreditTurnover);
                values[target, 6] = Number(row.CalculatedClosingDebit);
                values[target, 7] = Number(row.CalculatedClosingCredit);
                values[target, 8] = row.GeneralLedgerAccountName;
                values[target, 9] = Number(row.GeneralLedgerOpeningDebit);
                values[target, 10] = Number(row.GeneralLedgerOpeningCredit);
                values[target, 11] = Number(row.GeneralLedgerDebitTurnover);
                values[target, 12] = Number(row.GeneralLedgerCreditTurnover);
                values[target, 13] = Number(row.GeneralLedgerClosingDebit);
                values[target, 14] = Number(row.GeneralLedgerClosingCredit);
                values[target, 15] = Number(row.OpeningDebitDifference);
                values[target, 16] = Number(row.OpeningCreditDifference);
                values[target, 17] = Number(row.DebitTurnoverDifference);
                values[target, 18] = Number(row.CreditTurnoverDifference);
                values[target, 19] = Number(row.ClosingDebitDifference);
                values[target, 20] = Number(row.ClosingCreditDifference);
                values[target, 21] = row.Status;
            }

            return values;
        }

        private static object Number(decimal? value)
        {
            return value.HasValue ? (object)(double)value.Value : null;
        }

        private static bool IsZero(decimal value)
        {
            return Math.Abs(value) <= Tolerance;
        }

        private static void AddSubtotals(Excel.Worksheet sheet)
        {
            SetSubtotal(
                sheet,
                1,
                "=SUBTOTAL(3,GeneralLedgerComparisonRows[AccountCode])",
                "0");

            for (int column = 3; column <= 21; column++)
            {
                SetSubtotal(
                    sheet,
                    column,
                    "=SUBTOTAL(109,GeneralLedgerComparisonRows[" +
                        Headers[column - 1] + "])",
                    "#,##0.00;[Red]-#,##0.00");
            }
        }

        private static void SetSubtotal(
            Excel.Worksheet sheet,
            int column,
            string formula,
            string format)
        {
            Excel.Range cell = (Excel.Range)sheet.Cells[3, column];
            cell.Formula = formula;
            cell.NumberFormat = format;
            cell.Font.Bold = true;
        }

        private sealed class Row
        {
            public string AccountCode;
            public bool OpeningAvailable;
            public bool HasImportedLedgerAccount;
            public decimal? CalculatedOpeningDebit;
            public decimal? CalculatedOpeningCredit;
            public decimal? CalculatedDebitTurnover;
            public decimal? CalculatedCreditTurnover;
            public decimal? CalculatedClosingDebit;
            public decimal? CalculatedClosingCredit;
            public string GeneralLedgerAccountName;
            public decimal? GeneralLedgerOpeningDebit;
            public decimal? GeneralLedgerOpeningCredit;
            public decimal? GeneralLedgerDebitTurnover;
            public decimal? GeneralLedgerCreditTurnover;
            public decimal? GeneralLedgerClosingDebit;
            public decimal? GeneralLedgerClosingCredit;
            public decimal? OpeningDebitDifference;
            public decimal? OpeningCreditDifference;
            public decimal? DebitTurnoverDifference;
            public decimal? CreditTurnoverDifference;
            public decimal? ClosingDebitDifference;
            public decimal? ClosingCreditDifference;
            public string Status;
        }
    }
}
