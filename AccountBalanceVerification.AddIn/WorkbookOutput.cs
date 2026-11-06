using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AccountBalanceVerification.Core;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace AccountBalanceVerification.AddIn
{
    internal static class WorkbookOutput
    {
        private const string RoleKey = "ABVRole", CodeKey = "ABVCode";
        internal static void Release(object value) { if (value != null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value); }
        internal static void Text(Excel.Worksheet sheet, string address, string value)
        {
            var range = sheet.Range[address];
            try { range.NumberFormat = "@"; range.Value2 = value ?? ""; }
            finally { Release(range); }
        }
        internal static void Tag(Excel.Worksheet sheet, string role, string code = "")
        {
            var properties = sheet.CustomProperties;
            try { Release(properties.Add(RoleKey, role)); if (code.Length > 0) Release(properties.Add(CodeKey, code)); }
            finally { Release(properties); }
        }
        internal static string Property(Excel.Worksheet sheet, string key)
        {
            var properties = sheet.CustomProperties;
            try {
                for (int i = 1; i <= properties.Count; i++) {
                    var property = properties.Item[i];
                    try { if (property.Name == key) return Convert.ToString(property.Value); }
                    finally { Release(property); }
                }
                return "";
            } finally { Release(properties); }
        }
        internal static void Anchor(Excel.Workbook book, Excel.Worksheet sheet, string name, string address)
        {
            var names = book.Names;
            try { Release(names.Add(name, "='" + sheet.Name.Replace("'", "''") + "'!" + address, Visible: false)); }
            finally { Release(names); }
        }
        private static Excel.Range NamedRange(Excel.Workbook book, string name)
        {
            var names = book.Names;
            Excel.Name item = null;
            try { item = names.Item(name); return item.RefersToRange; }
            finally { Release(item); Release(names); }
        }
        internal static void SignatureAnchor(Excel.Workbook book, Excel.Worksheet sheet, string key, int row)
        {
            Anchor(book, sheet, "_ABV_Start_" + key, "$A$" + row);
            Anchor(book, sheet, "_ABV_Output_" + key, "$A$" + row + ":$G$" + row);
        }
        internal static void CreateParameters(Excel.Workbook book, Excel.Worksheet sheet, IEnumerable<string> warnings, VerificationPackage package)
        {
            Tag(sheet, "Parameters"); sheet.Name = "Parametre";
            Text(sheet, "A1", "Parametre a správy");
            string[] labels = { "V (lokalita)", "Dňa", "Deň začatia inventúry", "Deň skončenia inventúry" };
            string[] keys = { "Location", "Date", "StartDate", "EndDate" };
            for (int i = 0; i < labels.Length; i++) {
                Text(sheet, "A" + (i + 2), labels[i]);
                var cell = sheet.Range["B" + (i + 2)];
                try { cell.NumberFormat = i == 0 ? "@" : "d.m.yyyy"; }
                finally { Release(cell); }
                Anchor(book, sheet, "_ABV_" + keys[i], "$B$" + (i + 2));
            }
            Text(sheet, "A7", "Vyplňte mená a výber tlače. Doplnok priebežne aktualizuje podpisové časti.");
            Text(sheet, "A8", "Ďalšie osoby pridajte rozšírením tabuľky. Prázdne mená sa netlačia.");
            Text(sheet, "A10", "Funkcia"); Text(sheet, "B10", "Meno"); Text(sheet, "C10", "Vytlačiť na strane");
            var roles = Roles();
            int count = Math.Max(5, roles.Length);
            for (int i = 0; i < count; i++) {
                Text(sheet, "A" + (11 + i), i < roles.Length ? roles[i] : "");
                Text(sheet, "B" + (11 + i), ""); Text(sheet, "C" + (11 + i), PrintPlan.All);
            }
            var area = sheet.Range["A10", "C" + (10 + count)];
            var tables = sheet.ListObjects;
            try { var table = tables.Add(Excel.XlListObjectSourceType.xlSrcRange, area, Type.Missing, Excel.XlYesNoGuess.xlYes); try { table.Name = "ABV_People"; } finally { Release(table); } }
            finally { Release(tables); Release(area); }
            SetLists(book, sheet);
            var fields = sheet.Range["A11:B1010"]; try { fields.NumberFormat = "@"; } finally { Release(fields); }
            Text(sheet, "E2", "Zdroj"); Text(sheet, "F2", package.Source);
            Text(sheet, "E3", "Načítané (UTC)"); Text(sheet, "F3", package.RetrievedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            Text(sheet, "E5", "Správy a upozornenia");
            int r = 6; foreach (var message in warnings.Distinct()) Text(sheet, "E" + r++, message);
            var a = sheet.Range["A:C"]; try { a.ColumnWidth = 29; } finally { Release(a); }
            var b = sheet.Range["B:B"]; try { b.ColumnWidth = 36; } finally { Release(b); }
            var e = sheet.Range["E:F"]; try { e.ColumnWidth = 55; e.WrapText = true; } finally { Release(e); }
        }
        private static string[] Roles() => (ConnectionSettings.Load().SignatureRoles ?? PrintPlan.DefaultRoles)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().DefaultIfEmpty("Člen").ToArray();
        private static void SetLists(Excel.Workbook book, Excel.Worksheet sheet)
        {
            var roles = Roles();
            for (int i = 0; i < roles.Length; i++) Text(sheet, "J" + (i + 1), roles[i]);
            for (int i = 0; i < PrintPlan.Placements.Length; i++) Text(sheet, "K" + (i + 1), PrintPlan.Placements[i]);
            Anchor(book, sheet, "_ABV_Roles", "$J$1:$J$" + roles.Length);
            Anchor(book, sheet, "_ABV_Placements", "$K$1:$K$3");
            Validate(sheet, "A11:A1010", "=_ABV_Roles"); Validate(sheet, "C11:C1010", "=_ABV_Placements");
            // Keep validation sources visible on Parametre. Hiding columns can fail
            // in Excel; this administrative sheet is excluded from ribbon output.
        }
        private static void Validate(Excel.Worksheet sheet, string address, string source)
        {
            var range = sheet.Range[address]; var validation = range.Validation;
            try { validation.Delete(); validation.Add(Excel.XlDVType.xlValidateList, Excel.XlDVAlertStyle.xlValidAlertStop, Formula1: source); validation.InCellDropdown = true; validation.IgnoreBlank = true; validation.ShowError = true; }
            finally { Release(validation); Release(range); }
        }
        private static string Read(Excel.Workbook book, string key, bool date = false)
        {
            var range = NamedRange(book, "_ABV_" + key);
            try {
                var value = range.Value2;
                if (value == null) return "";
                if (date && value is double serial) return DateTime.FromOADate(serial).ToString("d.M.yyyy");
                string text = Convert.ToString(value).Trim();
                if (date && text.Length > 0) {
                    if (!DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed) &&
                        !DateTime.TryParse(text, CultureInfo.GetCultureInfo("sk-SK"), DateTimeStyles.None, out parsed))
                        throw new InvalidOperationException("Neplatný dátum na hárku Parametre: " + text);
                    return parsed.ToString("d.M.yyyy");
                }
                return text;
            } finally { Release(range); }
        }
        private static List<Signatory> People(Excel.Worksheet parameters)
        {
            var tables = parameters.ListObjects; Excel.ListObject table = null; Excel.Range body = null;
            try {
                table = tables["ABV_People"]; body = table.DataBodyRange;
                var result = new List<Signatory>(); if (body == null) return result;
                var data = (object[,])body.Value2;
                for (int row = 1; row <= data.GetLength(0); row++) result.Add(new Signatory {
                    Role = Convert.ToString(data[row, 1]), Name = Convert.ToString(data[row, 2]), Placement = Convert.ToString(data[row, 3]) });
                return result;
            } finally { Release(body); Release(table); Release(tables); }
        }
        private sealed class OutputSheet
        {
            public Excel.Worksheet Sheet; public string Code; public bool Title;
        }
        private static List<OutputSheet> Sheets(Excel.Workbook book, out Excel.Worksheet parameters)
        {
            parameters = null; var result = new List<OutputSheet>(); var sheets = book.Worksheets;
            try {
                for (int i = 1; i <= sheets.Count; i++) {
                    var sheet = (Excel.Worksheet)sheets[i]; string role = Property(sheet, RoleKey);
                    if (role == "Parameters") { if (parameters != null) { Release(sheet); throw new InvalidOperationException("Duplicitný hárok Parametre."); } parameters = sheet; }
                    else if (role == "Title" || role == "Account") result.Add(new OutputSheet { Sheet = sheet, Title = role == "Title", Code = Property(sheet, CodeKey) });
                    else Release(sheet);
                }
                if (parameters == null || result.Count(x => x.Title) != 1 || result.Where(x => !x.Title).GroupBy(x => x.Code).Any(g => g.Count() > 1))
                    throw new InvalidOperationException("Otvorte inventarizačný zošit vytvorený touto verziou doplnku. Chýbajú alebo sú duplicitné interné údaje hárkov.");
                return result.OrderByDescending(x => x.Title).ThenBy(x => x.Code, StringComparer.Ordinal).ToList();
            } catch { foreach (var item in result) Release(item.Sheet); Release(parameters); parameters = null; throw; }
            finally { Release(sheets); }
        }
        internal static void Refresh(Excel.Workbook book)
        {
            var app = book.Application;
            bool events = app.EnableEvents, screen = app.ScreenUpdating;
            try {
                app.EnableEvents = false; app.ScreenUpdating = false;
                RefreshCore(book);
            } finally { app.EnableEvents = events; app.ScreenUpdating = screen; }
        }
        private static void RefreshCore(Excel.Workbook book)
        {
            Excel.Worksheet parameters; var sheets = Sheets(book, out parameters);
            try {
                SetLists(book, parameters);
                var people = People(parameters);
                // Validate all inputs before changing output areas.
                PrintPlan.Select(people, true); PrintPlan.Select(people, false);
                var location = Read(book, "Location"); var date = Read(book, "Date", true);
                var start = Read(book, "StartDate", true); var end = Read(book, "EndDate", true);
                foreach (var item in sheets) {
                    var sheet = item.Sheet; var key = item.Title ? "Title" : item.Code;
                    var anchor = NamedRange(book, "_ABV_Start_" + key); int row;
                    try { row = anchor.Row; } finally { Release(anchor); }
                    var old = NamedRange(book, "_ABV_Output_" + key);
                    try { old.UnMerge(); old.ClearContents(); old.WrapText = false; old.RowHeight = 15; } finally { Release(old); }
                    int first = row;
                    Text(sheet, "A" + row, "Funkcia"); Text(sheet, "B" + row, "Meno"); Text(sheet, "E" + row++, "Podpis");
                    foreach (var person in PrintPlan.Select(people, item.Title)) {
                        Text(sheet, "A" + row, person.Role); Text(sheet, "B" + row, person.Name); Text(sheet, "E" + row, "........................................");
                        var line = sheet.Range["A" + row, "G" + row]; try { line.RowHeight = 42; line.WrapText = false; } finally { Release(line); }
                        var role = sheet.Range["A" + row];
                        try { role.WrapText = true; } finally { Release(role); }
                        row++;
                    }
                    row++; Text(sheet, "A" + row, location); Text(sheet, "E" + row, date.Length > 0 ? "dňa " + date : "dňa: __________________");
                    Anchor(book, sheet, "_ABV_Output_" + key, "$A$" + first + ":$G$" + row);
                    if (item.Title) { Text(sheet, "B6", start); Text(sheet, "B7", end); }
                    else Text(sheet, "A3", "Deň začatia inventúry: " + start + "    Deň skončenia inventúry: " + end);
                    var setup = sheet.PageSetup;
                    try {
                        setup.PrintArea = "$A$1:$G$" + row;
                        setup.Zoom = false; setup.FitToPagesWide = 1; setup.FitToPagesTall = false;
                        setup.Orientation = Excel.XlPageOrientation.xlPortrait;
                        setup.PaperSize = Excel.XlPaperSize.xlPaperA4;
                        setup.LeftMargin = 18; setup.RightMargin = 18;
                        setup.PrintTitleRows = item.Title ? "" : "$1:$4";
                        setup.CenterFooter = "Strana &P / &N";
                    } finally { Release(setup); }
                }
            } finally { foreach (var item in sheets) Release(item.Sheet); Release(parameters); }
        }
        internal static void Output(bool pdf)
        {
            var app = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook source = null, output = null;
            Excel.Worksheet parameters = null; List<OutputSheet> sheets = null;
            object active = null; bool oldScreen = app.ScreenUpdating, oldAlerts = app.DisplayAlerts;
            try {
                source = app.ActiveWorkbook; active = app.ActiveSheet;
                if (source == null) throw new InvalidOperationException("Najprv otvorte inventarizačný zošit.");
                sheets = Sheets(source, out parameters); // Reject unrelated workbooks before prompting.
                string filename = null;
                if (pdf) using (var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", DefaultExt = "pdf", AddExtension = true, OverwritePrompt = true, FileName = "Inventarizacia.pdf" }) {
                    if (dialog.ShowDialog() != DialogResult.OK) return; filename = dialog.FileName;
                }
                Refresh(source);
                app.ScreenUpdating = false; app.DisplayAlerts = false;
                // A temporary workbook gives both actions exactly the same ordered selection,
                // without leaving worksheets grouped or changing source tab order.
                foreach (var item in sheets) {
                    if (output == null) { item.Sheet.Copy(); output = app.ActiveWorkbook; }
                    else {
                        var copies = output.Worksheets; var last = (Excel.Worksheet)copies[copies.Count];
                        try { item.Sheet.Copy(After: last); } finally { Release(last); Release(copies); }
                    }
                }
                app.ScreenUpdating = true; app.DisplayAlerts = oldAlerts;
                if (pdf) output.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, filename, IgnorePrintAreas: false, OpenAfterPublish: true);
                else output.PrintPreview(EnableChanges: true);
            } catch (Exception ex) { MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally {
                try {
                    if (output != null) { try { output.Close(SaveChanges: false); } finally { Release(output); } }
                    if (source != null) { source.Activate(); if (active is Excel.Worksheet sheet) sheet.Activate(); }
                } finally {
                    app.ScreenUpdating = oldScreen; app.DisplayAlerts = oldAlerts;
                    if (sheets != null) foreach (var item in sheets) Release(item.Sheet);
                    Release(parameters); Release(active); Release(source);
                }
            }
        }
    }
}
