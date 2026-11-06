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
            bool previousScreen = app.ScreenUpdating, previousEvents = app.EnableEvents;
            Exception originalFailure = null;
            try
            {
                app.ScreenUpdating = false; app.EnableEvents = false;
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
                    var title = intro.Range["A1:G1"]; try { title.Font.Bold = true; title.Font.Size = 16; title.RowHeight = 32; } finally { Marshal.ReleaseComObject(title); }
                    var entityLine = intro.Range["B3:G3"]; try { entityLine.WrapText = false; entityLine.RowHeight = 42; } finally { Marshal.ReleaseComObject(entityLine); }
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
                            try { heading.WrapText = false; heading.RowHeight = h == 1 ? 28 : 24; if (h == 1) heading.Font.Bold = true; }
                            finally { Marshal.ReleaseComObject(heading); }
                        }
                        string[] captions = { "Účet", "Názov účtu", "Začiatočný stav", "Obrat MD", "Obrat Dal", "Konečný zostatok", "Poznámka" };
                        for (int c = 0; c < captions.Length; c++) Text(sheet, ((char)('A' + c)).ToString() + "4", captions[c]);
                        int row = 5;
                        if (plan.Rows.Count > 0) {
                            var values = new object[plan.Rows.Count, 7];
                            for (int i = 0; i < plan.Rows.Count; i++) {
                                var item = plan.Rows[i];
                                values[i, 0] = item.AccountNo; values[i, 1] = item.AccountName;
                                values[i, 2] = (double)item.OpeningBalance; values[i, 3] = (double)item.MovementsDebit;
                                values[i, 4] = (double)item.MovementsCredit; values[i, 5] = (double)item.ClosingBalance;
                                values[i, 6] = "";
                            }
                            row += plan.Rows.Count;
                            var data = sheet.Range["A5:G" + (row - 1)];
                            var amounts = sheet.Range["C5:F" + (row - 1)];
                            try {
                                data.NumberFormat = "@";
                                amounts.NumberFormat = "#,##0.00;-#,##0.00";
                                data.Value2 = values;
                            } finally { Marshal.ReleaseComObject(amounts); Marshal.ReleaseComObject(data); }
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
                                    table.TableStyle = "TableStyleLight8";
                                    table.ShowTableStyleRowStripes = false;
                                    table.ShowTableStyleColumnStripes = false;
                                    var all = table.Range; var header = table.HeaderRowRange;
                                    var body = table.DataBodyRange; var summary = table.TotalsRowRange;
                                    try {
                                        all.Font.Color = 0; all.Interior.Color = 16777215;
                                        body.Font.Bold = false; body.WrapText = true;
                                        header.Font.Color = 16777215; header.Interior.Color = 0;
                                        header.Font.Bold = true; header.WrapText = true; header.RowHeight = 32;
                                        summary.Font.Bold = true; summary.WrapText = true;
                                    } finally { Marshal.ReleaseComObject(summary); Marshal.ReleaseComObject(body); Marshal.ReleaseComObject(header); Marshal.ReleaseComObject(all); }
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
                                    Text(sheet, "A" + row, "");
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
                        Width(sheet, "A:A", 12); Width(sheet, "B:B", 28); Width(sheet, "C:F", 15); Width(sheet, "G:G", 24);
                        var detailRows = sheet.Range["A5:G" + row];
                        try { detailRows.Rows.AutoFit(); } finally { Marshal.ReleaseComObject(detailRows); }
                        sheet.Activate(); app.ActiveWindow.SplitRow = 4; app.ActiveWindow.FreezePanes = true;
                        sheet.EnableSelection = Excel.XlEnableSelection.xlNoRestrictions;
                    }
                    finally { Marshal.ReleaseComObject(sheet); }
                }
                var warnings = package.Warnings.ToList();
                warnings.AddRange(sheets.Where(s => !package.OfficialAccounts.Any(a => a.AccountLevel == 3 && a.AccountCode == s.Code && !string.IsNullOrWhiteSpace(a.AccountName))).Select(s => "Chýba názov pre syntetický účet " + s.Code + ". Zadajte názov do bunky A1 na hárku " + s.Code + "."));
                warnings.AddRange(package.Rows.Where(x => x.OpeningBalance + x.MovementsDebit - x.MovementsCredit != x.ClosingBalance).Select(x => "Nesúhlasí výpočet konečného zostatku: " + x.AccountNo));
                var parameters = (Excel.Worksheet)book.Worksheets.Add(Before: book.Worksheets[1]);
                try { WorkbookOutput.CreateParameters(book, parameters, warnings, package); }
                finally { Marshal.ReleaseComObject(parameters); }
                WorkbookOutput.InitializeFormulas(book);
                var first = (Excel.Worksheet)book.Worksheets[1]; first.Activate(); Marshal.ReleaseComObject(first);
                book.Saved = false;
            }
            catch (Exception ex) { originalFailure = ex; throw; }
            finally {
                try { app.EnableEvents = previousEvents; }
                catch (Exception ex) {
                    if (originalFailure == null) throw new InvalidOperationException("Obnovenie Application.EnableEvents: " + ex.Message, ex);
                    System.Diagnostics.Trace.WriteLine(ex);
                }
                finally {
                    try { app.ScreenUpdating = previousScreen; }
                    catch (Exception ex) {
                        if (originalFailure == null) throw new InvalidOperationException("Obnovenie Application.ScreenUpdating: " + ex.Message, ex);
                        System.Diagnostics.Trace.WriteLine(ex);
                    }
                }
            }
        }
        // Text formatting prevents source names/comments beginning with '=' being interpreted as formulas.
        private static void Text(Excel.Worksheet sheet, string address, string value) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.Value2 = value ?? ""; } finally { Marshal.ReleaseComObject(cell); } }
        private static void PrepareTextInput(Excel.Worksheet sheet, string address) { var cell = sheet.Range[address]; try { cell.NumberFormat = "@"; cell.WrapText = true; } finally { Marshal.ReleaseComObject(cell); } }
        private static void Width(Excel.Worksheet sheet, string address, double width) { var range = sheet.Range[address]; try { range.ColumnWidth = width; } finally { Marshal.ReleaseComObject(range); } }
    }
}
