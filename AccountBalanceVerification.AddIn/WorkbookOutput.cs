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
    internal static partial class WorkbookOutput
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
            string formula = "='" + sheet.Name.Replace("'", "''") + "'!" + address;
            try { AddName(names, name, formula, false); }
            finally { Release(names); }
        }
        private static void AddName(Excel.Names names, string name, string formula, bool visible = true)
        {
            try { Release(names.Add(name, formula, Visible: visible)); }
            catch (Exception ex) {
                throw new InvalidOperationException("Definovaný názov: " + name + "\nVzorec: " + formula + "\n" + ex.Message, ex);
            }
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
            Text(sheet, "A7", "Vyplňte mená a výber tlače. Podpisové časti sa prepočítajú pomocou vzorcov.");
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
            UpgradeParameters(book, sheet);
            Text(sheet, "P2", "Zdroj"); Text(sheet, "Q2", package.Source);
            Text(sheet, "P3", "Načítané (UTC)"); Text(sheet, "Q3", package.RetrievedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            Text(sheet, "P5", "Správy a upozornenia");
            int r = 6; foreach (var message in warnings.Distinct()) Text(sheet, "P" + r++, message);
            var a = sheet.Range["A:C"]; try { a.ColumnWidth = 29; } finally { Release(a); }
            var b = sheet.Range["B:B"]; try { b.ColumnWidth = 36; } finally { Release(b); }
            var e = sheet.Range["P:Q"]; try { e.ColumnWidth = 55; e.WrapText = true; } finally { Release(e); }
        }
        private static string[] Roles() => (ConnectionSettings.Load().SignatureRoles ?? PrintPlan.DefaultRoles)
            .Concat(new[] { "Predsedníčka", "Členka" }).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().DefaultIfEmpty("Člen").ToArray();
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
                for (int row = 1; row <= data.GetLength(0); row++) {
                    var person = new Signatory { Role = Convert.ToString(data[row, 1]), Name = Convert.ToString(data[row, 2]) };
                    if (data.GetLength(1) == 3) person.Placement = Convert.ToString(data[row, 3]);
                    else {
                        person.OnTitle = ReadFlag(data[row, 3], person.Name);
                        person.OnAccounts = ReadFlag(data[row, 4], person.Name);
                        person.DecreeRole = Convert.ToString(data[row, 5]).Trim();
                        person.RecapRole = Convert.ToString(data[row, 6]).Trim();
                    }
                    result.Add(person);
                }
                return result;
            } finally { Release(body); Release(table); Release(tables); }
        }
        private sealed class OutputSheet
        {
            public Excel.Worksheet Sheet; public string Code; public bool Title; public bool Recap;
        }
        private static List<OutputSheet> Sheets(Excel.Workbook book, out Excel.Worksheet parameters)
        {
            parameters = null; var result = new List<OutputSheet>(); var sheets = book.Worksheets;
            try {
                for (int i = 1; i <= sheets.Count; i++) {
                    var sheet = (Excel.Worksheet)sheets[i]; string role = Property(sheet, RoleKey);
                    if (role == "Parameters") { if (parameters != null) { Release(sheet); throw new InvalidOperationException("Duplicitný hárok Parametre."); } parameters = sheet; }
                    else if (role == "Title" || role == "Account" || role == "Recap") result.Add(new OutputSheet { Sheet = sheet, Title = role == "Title", Recap = role == "Recap", Code = Property(sheet, CodeKey) });
                    else Release(sheet);
                }
                if (parameters == null || result.Count(x => x.Title) != 1 || result.Count(x => x.Recap) > 1 || result.Where(x => !x.Title && !x.Recap).GroupBy(x => x.Code).Any(g => g.Count() > 1))
                    throw new InvalidOperationException("Otvorte inventarizačný zošit vytvorený touto verziou doplnku. Chýbajú alebo sú duplicitné interné údaje hárkov.");
                return result.OrderBy(x => x.Title ? 0 : x.Recap ? 2 : 1).ThenBy(x => x.Code, StringComparer.Ordinal).ToList();
            } catch { foreach (var item in result) Release(item.Sheet); Release(parameters); parameters = null; throw; }
            finally { Release(sheets); }
        }
        internal static bool OutputAvailable(Excel.Workbook book, out bool decrees, out bool recap)
        {
            decrees = false; recap = false;
            if (book == null) return false;
            Excel.Worksheet parameters = null; List<OutputSheet> outputs = null;
            try {
                outputs = Sheets(book, out parameters);
                if (!HasName(book, "_ABV_Start_Title") || !outputs.Any(x => !x.Title && !x.Recap)) return false;
                var sheets = book.Worksheets;
                try {
                    for (int i = 1; i <= sheets.Count; i++) {
                        var sheet = (Excel.Worksheet)sheets[i];
                        try {
                            if (Property(sheet, RoleKey) == "Decree") {
                                var first = sheet.Range["A1"];
                                try { decrees = Convert.ToBoolean(first.HasFormula); } finally { Release(first); }
                            }
                        } finally { Release(sheet); }
                    }
                } finally { Release(sheets); }
                recap = outputs.Any(x => x.Recap) && HasName(book, "_ABV_RecapEnd");
                return true;
            } catch { return false; }
            finally {
                if (outputs != null) foreach (var output in outputs) Release(output.Sheet);
                Release(parameters);
            }
        }
        private static void Formula(Excel.Worksheet sheet, string address, string formula)
        {
            var cell = sheet.Range[address];
            try { cell.NumberFormat = "General"; ((dynamic)cell).Formula2 = formula; }
            catch (Exception ex) { throw new InvalidOperationException("Vzorec: " + sheet.Name + "!" + address + " = " + formula + "\n" + ex.Message, ex); }
            finally { Release(cell); }
        }
        private static string PeopleMask(bool title) =>
            "IFERROR((LEN(TRIM(ABV_People[Meno]))>0)*(ABV_People[" + (title ? "Prvá" : "Účty") + "]=TRUE),0)";
        private static string DateFormula(string key) =>
            "IF(" + key + "=\"\",\"\",TEXT(" + key + ",\"d.m.yyyy\"))";

        // Written once. Excel recalculates these spills without any SheetChange handler.
        internal static void InitializeFormulas(Excel.Workbook book)
        {
            Excel.Worksheet parameters; var sheets = Sheets(book, out parameters);
            var app = book.Application;
            bool communication = app.PrintCommunication;
            Exception originalFailure = null;
            try {
                app.PrintCommunication = false;
                if (!HasName(book, "P_EntityName")) {
                string[] readable = { "Location", "ProcessDate", "InventoryBeginning", "InventoryEnd" };
                for (int i = 0; i < readable.Length; i++) Anchor(book, parameters, readable[i], "$B$" + (i + 2));
                // Store calculations through Formula2; defined names only reference cells.
                // Names.Add rejects the full structured-reference count formula on some Excel installations.
                Text(parameters, "M1", "Počet podpisov – účty");
                Text(parameters, "N1", "Počet podpisov – titulná strana");
                Formula(parameters, "M2", "=SUMPRODUCT(" + PeopleMask(false) + ")");
                Formula(parameters, "N2", "=SUMPRODUCT(" + PeopleMask(true) + ")");
                Anchor(book, parameters, "ABV_AccountPeopleCount", "$M$2");
                Anchor(book, parameters, "ABV_TitlePeopleCount", "$N$2");
                }
                int reservedPeopleRows = Math.Max(1, People(parameters).Count);
                foreach (var item in sheets) {
                    if (item.Recap) continue;
                    var sheet = item.Sheet; var key = item.Title ? "Title" : item.Code;
                    var anchor = NamedRange(book, "_ABV_Start_" + key); int row;
                    try { row = anchor.Row; } finally { Release(anchor); }
                    Text(sheet, "A" + row, "Funkcia"); Text(sheet, "B" + row, "Meno"); Text(sheet, "E" + row, "Podpis");
                    string mask = PeopleMask(item.Title);
                    string count = item.Title ? "ABV_TitlePeopleCount" : "ABV_AccountPeopleCount";
                    int body = row + 1;
                    Formula(sheet, "A" + body, "=FILTER(ABV_People[Funkcia]," + mask + ",\"\")");
                    Formula(sheet, "B" + body, "=FILTER(ABV_People[Meno]," + mask + ",\"\")");
                    Formula(sheet, "E" + body, "=IF(" + count + "=0,\"\",IF(SEQUENCE(MAX(1," + count + ")),\"........................................\"))");
                    // Reserve the existing people-table capacity. Insert worksheet rows
                    // above the footer for more people or free-form auditor notes.
                    int footerRow = row + reservedPeopleRows + 2;
                    Formula(sheet, "A" + footerRow, "=IF(Location=\"\",\"\",Location)");
                    Formula(sheet, "E" + footerRow, "=IF(ProcessDate>0,\"dňa: \"&TEXT(ProcessDate,\"d.m.yyyy\"),\"dňa: __________________\")");
                    Anchor(book, sheet, "_ABV_Footer_" + key, "$A$" + footerRow);
                    // Format future spill rows in one operation; no per-person COM calls.
                    var space = sheet.Range["A" + row, "G" + (row + 1002)];
                    try { space.WrapText = false; space.RowHeight = 32; } finally { Release(space); }
                    var roles = sheet.Range["A" + row, "A" + (row + 1002)];
                    try { roles.WrapText = true; } finally { Release(roles); }
                    var footerLine = sheet.Range["A" + footerRow, "G" + footerRow];
                    try { footerLine.WrapText = false; } finally { Release(footerLine); }
                    if (item.Title) {
                        if (HasName(book, "P_EntityName")) {
                            Formula(sheet, "B3", "=P_EntityName"); Formula(sheet, "B4", "=P_ICO"); Formula(sheet, "B5", "=P_FiscalYear");
                        }
                        Formula(sheet, "B6", "=" + DateFormula("InventoryBeginning"));
                        Formula(sheet, "B7", "=" + DateFormula("InventoryEnd"));
                    } else {
                        if (HasName(book, "P_EntityName")) Formula(sheet, "A2", "=P_EntityName&\" | \"&P_FiscalYear");
                        Formula(sheet, "A3", "=\"Deň začatia inventúry: \"&" + DateFormula("InventoryBeginning") +
                        "&\"    Deň skončenia inventúry: \"&" + DateFormula("InventoryEnd"));
                    }
                    var setup = sheet.PageSetup;
                    try {
                        setup.Zoom = false; setup.FitToPagesWide = 1; setup.FitToPagesTall = false;
                        setup.Orientation = Excel.XlPageOrientation.xlPortrait;
                        setup.PaperSize = Excel.XlPaperSize.xlPaperA4;
                        setup.LeftMargin = 18; setup.RightMargin = 18;
                        setup.PrintTitleRows = item.Title ? "" : "$1:$4";
                        setup.CenterFooter = "Strana &P / &N";
                    } finally { Release(setup); }
                    // Use a literal print range rather than a formula-valued Print_Area.
                    // Ribbon output sizes its temporary copies to the current people count.
                    var printSetup = sheet.PageSetup;
                    try { printSetup.PrintArea = "$A$1:$G$" + footerRow; }
                    finally { Release(printSetup); }
                }
            } catch (Exception ex) { originalFailure = ex; throw; }
            finally {
                try { app.PrintCommunication = communication; }
                catch (Exception ex) {
                    var flushFailure = new InvalidOperationException("Excel odmietol uložiť nastavenia tlače pri obnovení Application.PrintCommunication.\n" + ex.Message, ex);
                    if (originalFailure == null) throw flushFailure;
                    throw new AggregateException(originalFailure.Message + "\n" + flushFailure.Message, originalFailure, flushFailure);
                }
                finally { foreach (var item in sheets) Release(item.Sheet); Release(parameters); }
            }
        }
        private static void ValidateForOutput(Excel.Workbook book, List<OutputSheet> sheets, Excel.Worksheet parameters)
        {
            book.Application.Calculate();
            var people = People(parameters);
            if (sheets.Any(x => !x.Recap)) {
                PrintPlan.Select(people, true); PrintPlan.Select(people, false);
                Read(book, "Date", true); Read(book, "StartDate", true); Read(book, "EndDate", true);
            }
            foreach (var item in sheets) {
                item.Sheet.Calculate();
                if (item.Recap) { ValidateRecap(book, item.Sheet, people); continue; }
                var anchor = NamedRange(book, "_ABV_Start_" + (item.Title ? "Title" : item.Code));
                int row; try { row = anchor.Row; } finally { Release(anchor); }
                var formulaAnchor = item.Sheet.Range["E" + (row + 1)];
                try {
                    if (!Convert.ToBoolean(formulaAnchor.HasFormula))
                        throw new InvalidOperationException("Vytvorte nový zošit vo verzii 0.1.7 alebo novšej, ktorá používa vzorce pre podpisy.");
                } finally { Release(formulaAnchor); }
                foreach (string column in new[] { "A", "B", "E" })
                    if (Convert.ToBoolean(item.Sheet.Evaluate("ISERROR(" + column + (row + 1) + ")")))
                        throw new InvalidOperationException("Skontrolujte podpisovú časť na hárku " + item.Sheet.Name +
                            ". Uvoľnite bunky pre vzorce alebo použite aktuálny Microsoft 365 Excel.");
            }
        }
        private static void ExportPdfWithRetry(Excel.Workbook output, string filename)
        {
            while (true) {
                try {
                    output.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, filename,
                        IgnorePrintAreas: false, OpenAfterPublish: true);
                    return;
                } catch (Exception ex) when (ex is COMException || ex is System.IO.IOException || ex is UnauthorizedAccessException) {
                    var choice = MessageBox.Show(
                        "PDF sa nepodarilo uložiť:\n" + filename +
                        "\n\nAk je tento súbor otvorený v prehliadači PDF, zatvorte ho. Skontrolujte tiež oprávnenie na zápis do priečinka." +
                        "\n\nOpakovať: znovu vybrať názov súboru a skúsiť uloženie. Môžete ponechať rovnaký názov alebo zadať iný." +
                        "\nZrušiť: ukončiť export." + "\n\nPodrobnosti: " + ex.Message,
                        "Inventarizácia – export PDF", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                    if (choice != DialogResult.Retry) return;
                    using (var dialog = new SaveFileDialog {
                        Filter = "PDF (*.pdf)|*.pdf", DefaultExt = "pdf", AddExtension = true,
                        OverwritePrompt = true, FileName = System.IO.Path.GetFileName(filename),
                        InitialDirectory = System.IO.Path.GetDirectoryName(filename)
                    }) {
                        if (dialog.ShowDialog() != DialogResult.OK) return;
                        filename = dialog.FileName;
                        PdfExportLocation.Remember(System.IO.Path.GetDirectoryName(filename));
                    }
                }
            }
        }
        internal static void Output(bool pdf, bool recapOnly)
        {
            var app = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook source = null, output = null;
            Excel.Worksheet parameters = null; List<OutputSheet> sheets = null;
            object active = null; bool oldScreen = app.ScreenUpdating, oldAlerts = app.DisplayAlerts;
            try {
                source = app.ActiveWorkbook; active = app.ActiveSheet;
                if (source == null) throw new InvalidOperationException("Najprv otvorte inventarizačný zošit.");
                sheets = Sheets(source, out parameters); // Reject unrelated workbooks before prompting.
                var selected = sheets.Where(x => x.Recap == recapOnly).ToList();
                foreach (var excluded in sheets.Where(x => x.Recap != recapOnly)) Release(excluded.Sheet);
                sheets = selected;
                if (sheets.Count == 0) throw new InvalidOperationException("Vybraný dokument nie je v tomto zošite dostupný.");
                string filename = null;
                if (pdf) using (var dialog = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", DefaultExt = "pdf", AddExtension = true, OverwritePrompt = true, InitialDirectory = PdfExportLocation.InitialDirectory, FileName = recapOnly ? "Rekapitulacia.pdf" : "Inventarizacia.pdf" }) {
                    if (dialog.ShowDialog() != DialogResult.OK) return; filename = dialog.FileName;
                        PdfExportLocation.Remember(System.IO.Path.GetDirectoryName(filename));
                }
                ValidateForOutput(source, sheets, parameters);
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
                var people = People(parameters);
                for (int i = 0; i < sheets.Count; i++) {
                    var sourceSheet = sheets[i]; var copy = (Excel.Worksheet)output.Worksheets[i + 1];
                    if (sourceSheet.Recap) {
                        try { PrepareRecapCopy(source, sourceSheet.Sheet, copy, people); app.CutCopyMode = (Excel.XlCutCopyMode)0; } finally { Release(copy); }
                        continue;
                    }
                    var anchor = NamedRange(source, "_ABV_Start_" + (sourceSheet.Title ? "Title" : sourceSheet.Code));
                    int last; try { last = anchor.Row + Math.Max(1, PrintPlan.Select(people, sourceSheet.Title).Count) + 2; } finally { Release(anchor); }
                    Excel.Range footer = null;
                    try {
                        footer = NamedRange(source, "_ABV_Footer_" + (sourceSheet.Title ? "Title" : sourceSheet.Code));
                        last = Math.Max(last, footer.Row);
                        // Omit reserved signature slots and spacing only in the output
                        // copy. Keep any auditor notes entered between signatures and footer.
                        var start = NamedRange(source, "_ABV_Start_" + (sourceSheet.Title ? "Title" : sourceSheet.Code));
                        int firstUnused;
                        try { firstUnused = start.Row + PrintPlan.Select(people, sourceSheet.Title).Count + 1; }
                        finally { Release(start); }
                        for (int r = firstUnused; r < footer.Row; r++) {
                            var line = copy.Range["A" + r + ":G" + r];
                            try {
                                var values = (object[,])line.Value2;
                                bool empty = true;
                                foreach (object value in values) if (!string.IsNullOrEmpty(Convert.ToString(value))) { empty = false; break; }
                                if (empty) {
                                    var rows = line.EntireRow;
                                    try { rows.Hidden = true; } finally { Release(rows); }
                                }
                            } finally { Release(line); }
                        }
                    }
                    catch (COMException) { /* Older workbooks have a spill footer. */ }
                    finally { Release(footer); }
                    var original = sourceSheet.Sheet.Range["A1:G" + last];
                    var destination = copy.Range["A1:G" + last];
                    try {
                        // Clear spill anchors before replacing the temporary copy with values.
                        destination.ClearContents();
                        original.Copy();
                        destination.PasteSpecial(Excel.XlPasteType.xlPasteValues);
                        app.CutCopyMode = (Excel.XlCutCopyMode)0;
                        var setup = copy.PageSetup;
                        try { setup.PrintArea = "$A$1:$G$" + last; } finally { Release(setup); }
                    } finally { Release(destination); Release(original); Release(copy); }
                }
                app.ScreenUpdating = true; app.DisplayAlerts = oldAlerts;
                if (pdf) ExportPdfWithRetry(output, filename);
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
