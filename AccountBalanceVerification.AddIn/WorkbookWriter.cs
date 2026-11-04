using System;
using System.Linq;
using System.Runtime.InteropServices;
using AccountBalanceVerification.Core;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;
namespace AccountBalanceVerification.AddIn
{
    internal static class WorkbookWriter
    {
        public static string Create(VerificationPackage package, bool includeEmpty)
        {
            var sheets = WorkbookPlan.Build(package, includeEmpty);
            var app = (Excel.Application)ExcelDnaUtil.Application;
            bool previousScreen = app.ScreenUpdating;
            Excel.Workbook book = null;
            try
            {
                app.ScreenUpdating = false;
                book = app.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                var intro = (Excel.Worksheet)book.Worksheets[1];
                try
                {
                    intro.Name = "Úvod";
                    Text(intro, "A1", "Inventarizácia účtov");
                    string[] labels = { "Účtovná jednotka", "IČO", "Účtovný rok", "Dátum inventarizácie", "Vypracoval", "Zodpovedná osoba", "Zdroj", "Načítané (UTC)", "Poznámka" };
                    object[] values = { package.EntityName, package.Ico, package.FiscalYear.ToString(), "", "", "", package.Source, package.RetrievedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"), "" };
                    for (int i = 0; i < labels.Length; i++) { Text(intro, "A" + (i + 3), labels[i]); Text(intro, "B" + (i + 3), Convert.ToString(values[i])); }
                    PrepareTextInput(intro, "B3:B4"); PrepareTextInput(intro, "B6:B8"); PrepareTextInput(intro, "B11");
                    var warnings = package.Warnings.ToList();
                    warnings.AddRange(sheets.Where(s => !package.OfficialAccounts.Any(a => a.AccountLevel == 3 && a.AccountCode == s.Code)).Select(s => "Chýba oficiálny názov účtu " + s.Code));
                    warnings.AddRange(package.Rows.Where(x => x.OpeningBalance + x.MovementsDebit - x.MovementsCredit != x.ClosingBalance).Select(x => "Nesúhlasí výpočet konečného zostatku: " + x.AccountNo));
                    for (int i = 0; i < warnings.Count; i++) Text(intro, "A" + (14 + i), warnings[i]);
                    Width(intro, "A:A", 48); Width(intro, "B:B", 70);
                }
                finally { Marshal.ReleaseComObject(intro); }
                foreach (var plan in sheets)
                {
                    var sheet = (Excel.Worksheet)book.Worksheets.Add(After: book.Worksheets[book.Worksheets.Count]);
                    try
                    {
                        sheet.Name = plan.Code;
                        Text(sheet, "A1", plan.Code + " – " + plan.Name);
                        Text(sheet, "A2", package.EntityName + " | " + package.FiscalYear);
                        string[] captions = { "Účet", "Názov účtu", "Začiatočný stav", "Obrat MD", "Obrat Dal", "Konečný zostatok", "Poznámka" };
                        for (int c = 0; c < captions.Length; c++) Text(sheet, ((char)('A' + c)).ToString() + "4", captions[c]);
                        int row = 5;
                        foreach (var item in plan.Rows)
                        {
                            Text(sheet, "A" + row, item.AccountNo); Text(sheet, "B" + row, item.AccountName);
                            var amounts = sheet.Range["C" + row, "F" + row];
                            try { amounts.Value2 = new object[,] { { (double)item.OpeningBalance, (double)item.MovementsDebit, (double)item.MovementsCredit, (double)item.ClosingBalance } }; amounts.NumberFormat = "#,##0.00;-#,##0.00"; }
                            finally { Marshal.ReleaseComObject(amounts); }
                            Text(sheet, "G" + row, ""); PrepareTextInput(sheet, "G" + row); row++;
                        }
                        if (plan.Rows.Count > 0)
                        {
                            var range = sheet.Range["A4", "G" + (row - 1)];
                            try
                            {
                                var table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, range, Type.Missing, Excel.XlYesNoGuess.xlYes);
                                try
                                {
                                    table.Name = "ABV_" + plan.Code;
                                    table.ShowTotals = true;
                                    var columns = table.ListColumns;
                                    try
                                    {
                                        for (int c = 3; c <= 6; c++)
                                        {
                                            var column = columns[c];
                                            try { column.TotalsCalculation = Excel.XlTotalsCalculation.xlTotalsCalculationSum; }
                                            finally { Marshal.ReleaseComObject(column); }
                                        }
                                    }
                                    finally { Marshal.ReleaseComObject(columns); }
                                    var totals = table.TotalsRowRange;
                                    try { row = totals.Row; }
                                    finally { Marshal.ReleaseComObject(totals); }
                                    Text(sheet, "A" + row, "Total");
                                    Text(sheet, "B" + row, "Spolu (zobrazené účty)");
                                    var totalAmounts = sheet.Range["C" + row, "F" + row];
                                    try { totalAmounts.NumberFormat = "#,##0.00"; }
                                    finally { Marshal.ReleaseComObject(totalAmounts); }
                                }
                                finally { Marshal.ReleaseComObject(table); }
                            }
                            finally { Marshal.ReleaseComObject(range); }
                        }
                        else { Text(sheet, "A5", "Bez zostatkov a obratov."); row = 6; }
                        Text(sheet, "A" + (row + 2), "Poznámka k syntetickému účtu"); Text(sheet, "B" + (row + 3), ""); PrepareTextInput(sheet, "B" + (row + 3) + ":G" + (row + 5));
                        Text(sheet, "A" + (row + 7), "Zodpovedná osoba"); PrepareTextInput(sheet, "B" + (row + 7));
                        Text(sheet, "A" + (row + 8), "Dátum / podpis"); PrepareTextInput(sheet, "B" + (row + 8) + ":D" + (row + 8));
                        Width(sheet, "A:A", 16); Width(sheet, "B:B", 55); Width(sheet, "C:F", 19); Width(sheet, "G:G", 60);
                        sheet.Activate(); app.ActiveWindow.SplitRow = 4; app.ActiveWindow.FreezePanes = true;
                        sheet.EnableSelection = Excel.XlEnableSelection.xlNoRestrictions;
                    }
                    finally { Marshal.ReleaseComObject(sheet); }
                }
                var first = (Excel.Worksheet)book.Worksheets[1]; first.Activate(); Marshal.ReleaseComObject(first);
                book.Saved = false;
                return book.Name;
            }
            catch { if (book != null) book.Close(false); throw; }
            finally { app.ScreenUpdating = previousScreen; if (book != null) Marshal.ReleaseComObject(book); }
        }
        // Text formatting prevents source names/comments beginning with '=' being interpreted as formulas.
        private static void Text(Excel.Worksheet sheet, string address, string value) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.Value2 = value ?? ""; } finally { Marshal.ReleaseComObject(cell); } }
        private static void PrepareTextInput(Excel.Worksheet sheet, string address) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.WrapText = true; } finally { Marshal.ReleaseComObject(cell); } }
        private static void Width(Excel.Worksheet sheet, string address, double width) { var range = sheet.Range[address]; try { range.ColumnWidth = width; } finally { Marshal.ReleaseComObject(range); } }
    }
}
