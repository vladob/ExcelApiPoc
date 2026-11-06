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
        public static void Populate(Excel.Workbook book, VerificationPackage package, bool includeEmpty)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            var sheets = WorkbookPlan.Build(package, includeEmpty);
            var app = (Excel.Application)ExcelDnaUtil.Application;
            bool previousScreen = app.ScreenUpdating;
            try
            {
                app.ScreenUpdating = false;
                var intro = (Excel.Worksheet)book.Worksheets[1];
                try
                {
                    intro.Name = "Titulná strana";
                    WorkbookOutput.Tag(intro, "Title");
                    Text(intro, "A1", "Inventarizácia účtov");
                    string[] labels = { "Účtovná jednotka", "IČO", "Účtovný rok", "Deň začatia inventúry", "Deň skončenia inventúry" };
                    string[] values = { package.EntityName, package.Ico, package.FiscalYear.ToString(), "", "" };
                    for (int i = 0; i < labels.Length; i++) { Text(intro, "A" + (i + 3), labels[i]); Text(intro, "B" + (i + 3), values[i]); }
                    Width(intro, "A:A", 30); Width(intro, "B:D", 12); Width(intro, "E:G", 10);
                    for (int r = 3; r <= 7; r++) { var merged = intro.Range["B" + r, "G" + r]; try { merged.Merge(); } finally { Marshal.ReleaseComObject(merged); } }
                    var title = intro.Range["A1:G1"]; try { title.Merge(); title.Font.Bold = true; title.Font.Size = 16; title.RowHeight = 32; } finally { Marshal.ReleaseComObject(title); }
                    var entityLine = intro.Range["B3:G3"]; try { entityLine.WrapText = true; entityLine.RowHeight = 42; } finally { Marshal.ReleaseComObject(entityLine); }
                    WorkbookOutput.SignatureAnchor(book, intro, "Title", 11);
                }
                finally { Marshal.ReleaseComObject(intro); }
                foreach (var plan in sheets)
                {
                    var sheet = (Excel.Worksheet)book.Worksheets.Add(After: book.Worksheets[book.Worksheets.Count]);
                    try
                    {
                        sheet.Name = plan.Code;
                        WorkbookOutput.Tag(sheet, "Account", plan.Code);
                        Text(sheet, "A1", plan.Code + " – " + plan.Name);
                        Text(sheet, "A2", package.EntityName + " | " + package.FiscalYear);
                        for (int h = 1; h <= 3; h++) {
                            var heading = sheet.Range["A" + h, "G" + h];
                            try { heading.Merge(); heading.WrapText = true; heading.RowHeight = h == 1 ? 28 : 24; if (h == 1) heading.Font.Bold = true; }
                            finally { Marshal.ReleaseComObject(heading); }
                        }
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
                        WorkbookOutput.SignatureAnchor(book, sheet, plan.Code, row + 7);
                        Width(sheet, "A:A", 16); Width(sheet, "B:B", 55); Width(sheet, "C:F", 19); Width(sheet, "G:G", 60);
                        sheet.Activate(); app.ActiveWindow.SplitRow = 4; app.ActiveWindow.FreezePanes = true;
                        sheet.EnableSelection = Excel.XlEnableSelection.xlNoRestrictions;
                    }
                    finally { Marshal.ReleaseComObject(sheet); }
                }
                var warnings = package.Warnings.ToList();
                warnings.AddRange(sheets.Where(s => !package.OfficialAccounts.Any(a => a.AccountLevel == 3 && a.AccountCode == s.Code && !string.IsNullOrWhiteSpace(a.AccountName))).Select(s => "Chýba názov syntetického účtu " + s.Code + ". Doplňte účtový rozvrh alebo skontrolujte zdrojové údaje."));
                warnings.AddRange(package.Rows.Where(x => x.OpeningBalance + x.MovementsDebit - x.MovementsCredit != x.ClosingBalance).Select(x => "Nesúhlasí výpočet konečného zostatku: " + x.AccountNo));
                var parameters = (Excel.Worksheet)book.Worksheets.Add(After: book.Worksheets[book.Worksheets.Count]);
                try { WorkbookOutput.CreateParameters(book, parameters, warnings, package); }
                finally { Marshal.ReleaseComObject(parameters); }
                WorkbookOutput.Refresh(book);
                var first = (Excel.Worksheet)book.Worksheets[1]; first.Activate(); Marshal.ReleaseComObject(first);
                book.Saved = false;
            }
            finally { app.ScreenUpdating = previousScreen; }
        }
        // Text formatting prevents source names/comments beginning with '=' being interpreted as formulas.
        private static void Text(Excel.Worksheet sheet, string address, string value) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.Value2 = value ?? ""; } finally { Marshal.ReleaseComObject(cell); } }
        private static void PrepareTextInput(Excel.Worksheet sheet, string address) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.WrapText = true; } finally { Marshal.ReleaseComObject(cell); } }
        private static void Width(Excel.Worksheet sheet, string address, double width) { var range = sheet.Range[address]; try { range.ColumnWidth = width; } finally { Marshal.ReleaseComObject(range); } }
    }
}
