using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AccountBalanceVerification.Core;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace AccountBalanceVerification.AddIn
{
    internal static partial class WorkbookOutput
    {
        private static readonly string[] DocumentKeys = { "EntityAddress", "DecreeDate", "AppointmentDate", "InventoryDate", "OrderNumber", "OrderDate", "OrderReceivedDate" };
        private static readonly string[] DocumentLabels = { "Sídlo účtovnej jednotky", "Dátum vydania dekrétu", "Účinnosť menovania od", "Inventarizácia ku dňu", "Číslo príkazu", "Dátum príkazu", "Dátum prevzatia príkazu" };
        private static bool ReadFlag(object value, string name)
        {
            if (value == null || Convert.ToString(value).Length == 0) return false;
            if (value is bool flag) return flag;
            if (bool.TryParse(Convert.ToString(value), out flag)) return flag;
            throw new InvalidOperationException("Neplatné začiarknutie Prvá/Účty pri osobe: " + name);
        }
        private static void UpgradeParameters(Excel.Workbook book, Excel.Worksheet parameters)
        {
            if (HasName(book, "P_EntityName")) { ConfigurePeopleInputs(parameters); return; }
            var tables = parameters.ListObjects; Excel.ListObject table = null;
            try {
                table = tables["ABV_People"];
                var columns = table.ListColumns; int count;
                try { count = columns.Count; } finally { Release(columns); }
                if (count == 3) {
                    var oldPeople = People(parameters);
                    // Validate all old selections before modifying the table.
                    var converted = oldPeople.Select(x => DecreePlan.Migrate(x.Role, x.Name, x.Placement)).ToList();
                    var old = parameters.Range["E1:F1010"]; var target = parameters.Range["P1:Q1010"];
                    try { old.Cut(target); } finally { Release(target); Release(old); }
                    var range = table.Range; int first, last;
                    try { first = range.Row; last = first + range.Rows.Count - 1; } finally { Release(range); }
                    var expanded = parameters.Range["A" + first + ":F" + last];
                    try { table.Resize(expanded); } finally { Release(expanded); }
                    string[] headers = { "Funkcia", "Meno", "Prvá", "Účty", "Menovací dekrét", "Rekapitulácia" };
                    for (int i = 0; i < headers.Length; i++) Text(parameters, ((char)('A' + i)) + first.ToString(), headers[i]);
                    if (converted.Count > 0) {
                        var data = new object[converted.Count, 4];
                        for (int i = 0; i < converted.Count; i++) {
                            data[i, 0] = converted[i].OnTitle.Value; data[i, 1] = converted[i].OnAccounts.Value;
                            data[i, 2] = converted[i].DecreeRole; data[i, 3] = converted[i].RecapRole;
                        }
                        var body = parameters.Range["C" + (first + 1) + ":F" + last];
                        try { body.NumberFormat = "General"; body.Value2 = data; } finally { Release(body); }
                    }
                    // Replace only signature formulas; retain auditor notes and existing footer positions.
                    Excel.Worksheet ignored; var outputs = Sheets(book, out ignored);
                    try {
                        foreach (var item in outputs) {
                            if (item.Recap) continue;
                            var anchor = NamedRange(book, "_ABV_Start_" + (item.Title ? "Title" : item.Code));
                            int row; try { row = anchor.Row + 1; } finally { Release(anchor); }
                            var cell = item.Sheet.Range["A" + row]; bool exists;
                            try { exists = Convert.ToBoolean(cell.HasFormula); } finally { Release(cell); }
                            if (exists) {
                                Formula(item.Sheet, "A" + row, "=FILTER(ABV_People[Funkcia]," + PeopleMask(item.Title) + ",\"\")");
                                Formula(item.Sheet, "B" + row, "=FILTER(ABV_People[Meno]," + PeopleMask(item.Title) + ",\"\")");
                            }
                        }
                    } finally { foreach (var item in outputs) Release(item.Sheet); Release(ignored); }
                    Formula(parameters, "M2", "=SUMPRODUCT(" + PeopleMask(false) + ")");
                    Formula(parameters, "N2", "=SUMPRODUCT(" + PeopleMask(true) + ")");
                } else if (count != 6) throw new InvalidOperationException("Neznáma štruktúra tabuľky ABV_People.");
                var flagBody = table.DataBodyRange;
                try {
                    if (flagBody != null) {
                        var bodyRows = flagBody.Rows;
                        int lastRow;
                        try { lastRow = flagBody.Row + bodyRows.Count - 1; }
                        finally { Release(bodyRows); }
                        var flags = parameters.Range["C" + flagBody.Row + ":D" + lastRow];
                        var validation = flags.Validation;
                try {
                    validation.Delete();
                    // Native in-cell checkboxes are optional in older Microsoft 365 builds.
                    try { ((dynamic)flags).CellControl.SetCheckbox(); }
                    catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException || ex is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) {
                        Text(parameters, "A9", "Táto verzia Excelu používa pre Prvá/Účty výber TRUE/FALSE namiesto políčok.");
                        var choices = parameters.Range["O1:O2"];
                        try { choices.Value2 = new object[,] { { true }, { false } }; } finally { Release(choices); }
                        Anchor(book, parameters, "_ABV_BooleanChoices", "$O$1:$O$2");
                        validation.Add(Excel.XlDVType.xlValidateList, Formula1: "=_ABV_BooleanChoices");
                        validation.ShowError = true;
                    }
                } finally { Release(validation); Release(flags); }
                    }
                } finally { Release(flagBody); }
                Text(parameters, "K1", DecreePlan.Recipient); Text(parameters, "K2", DecreePlan.Signer);
                Text(parameters, "L1", DecreePlan.Commission); Text(parameters, "L2", DecreePlan.Signer);
                Anchor(book, parameters, "_ABV_DecreeRoles", "$K$1:$K$2");
                Anchor(book, parameters, "_ABV_RecapRoles", "$L$1:$L$2");
                Validate(parameters, "E11:E1010", "=_ABV_DecreeRoles");
                Validate(parameters, "F11:F1010", "=_ABV_RecapRoles");
                var roles = Roles();
                for (int i = 0; i < roles.Length; i++) Text(parameters, "J" + (i + 1), roles[i]);
                Anchor(book, parameters, "_ABV_Roles", "$J$1:$J$" + roles.Length);
                Validate(parameters, "A11:A1010", "=_ABV_Roles");
                Text(parameters, "A7", "Prvá / Účty: podpisy. Menovací dekrét a Rekapitulácia: vyberte úlohu osoby.");
                Text(parameters, "A8", "Dokumenty sa prepočítajú vzorcami. Prázdne mená sa netlačia. Rekapitulácia sa pripravuje.");
                for (int i = 0; i < DocumentKeys.Length; i++) {
                    int row = i + 10;
                    Text(parameters, "H" + row, DocumentLabels[i]);
                    Anchor(book, parameters, "_ABV_" + DocumentKeys[i], "$I$" + row);
                    var cell = parameters.Range["I" + row];
                    try { cell.NumberFormat = i == 0 || i == 4 ? "@" : "d.m.yyyy"; } finally { Release(cell); }
                }
                var widths = parameters.Range["C:D"]; try { widths.ColumnWidth = 10; } finally { Release(widths); }
                widths = parameters.Range["E:F"]; try { widths.ColumnWidth = 24; } finally { Release(widths); }
                widths = parameters.Range["H:I"]; try { widths.ColumnWidth = 32; widths.WrapText = true; } finally { Release(widths); }
            } finally { Release(table); Release(tables); }
        }

        internal static void CreateDecreeSheet(Excel.Workbook book)
        {
            var sheet = GetDecreeSheet(book);
            try { InitializeDecreeFormulas(book, sheet); }
            finally { Release(sheet); }
        }
        private static Excel.Worksheet GetDecreeSheet(Excel.Workbook book)
        {
            var sheets = book.Worksheets;
            try {
                Excel.Worksheet found = null;
                for (int i = 1; i <= sheets.Count; i++) {
                    var sheet = (Excel.Worksheet)sheets[i];
                    if (Property(sheet, RoleKey) == "Decree") {
                        if (found != null) { Release(sheet); Release(found); throw new InvalidOperationException("Duplicitný hárok menovacích dekrétov."); }
                        found = sheet;
                    } else Release(sheet);
                }
                if (found != null) return found;
                var last = (Excel.Worksheet)sheets[sheets.Count];
                try {
                    var result = (Excel.Worksheet)sheets.Add(After: last);
                    try { result.Name = "Menovací dekrét"; Tag(result, "Decree"); return result; }
                    catch { Release(result); throw; }
                } finally { Release(last); }
            } finally { Release(sheets); }
        }
        private const int DecreeRows = 12, DecreeCapacity = 1000;
        private static void InitializeDecreeFormulas(Excel.Workbook book, Excel.Worksheet sheet)
        {
            // A single spill expands/contracts as recipients change. Preformatted rows
            // provide the repeating page layout without worksheet change handlers.
            var existing = sheet.Range["A1"]; bool initialized;
            try { initialized = Convert.ToBoolean(existing.HasFormula); } finally { Release(existing); }
            if (initialized) return;
            var all = sheet.Range["A1:A" + (DecreeCapacity * DecreeRows)];
            try { all.NumberFormat = "General"; all.ColumnWidth = 88; all.WrapText = true;
                all.Font.Name = "Calibri"; all.Font.Size = 12; all.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            } finally { Release(all); }
            double[] heights = { 60, 20, 45, 20, 40, 20, 150, 70, 40, 70, 35, 50 };
            for (int slot = 0; slot < DecreeRows; slot++) {
                // Keep each union address below Excel's address-length limit.
                for (int first = 0; first < DecreeCapacity; first += 20) {
                    var rows = DecreeRowUnion(
                        sheet, first, Math.Min(20, DecreeCapacity - first), slot);
                    try {
                        rows.RowHeight = heights[slot];
                        if (slot == 4) { rows.Font.Size = 18; rows.Font.Bold = true; rows.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter; }
                        if (slot == 9) rows.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    } finally { Release(rows); }
                }
            }
            string mask = "(LEN(TRIM(ABV_People[Meno]))>0)*(ABV_People[Menovací dekrét]=\"Príjemca\")";
            string signerMask = "(LEN(TRIM(ABV_People[Meno]))>0)*(ABV_People[Menovací dekrét]=\"Podpisujúci\")";
            string formula = "=LET(names,FILTER(ABV_People[Meno]," + mask + ",\"\"),roles,FILTER(ABV_People[Funkcia]," + mask + ",\"\")," +
                "n,SUMPRODUCT(" + mask + "),steps,SEQUENCE(MAX(1,n)*12),slot,MOD(steps-1,12)+1,person,INT((steps-1)/12)+1," +
                "role,LOWER(TRIM(INDEX(roles,person))),appointment,IF((role=\"predseda\")+(role=\"predseda komisie\"),\"predsedu\"," +
                "IF((role=\"predsedníčka\")+(role=\"predsedníčka komisie\"),\"predsedníčku\",IF((role=\"členka\")+(role=\"členka komisie\"),\"členku\"," +
                "IF((role=\"člen\")+(role=\"člen komisie\"),\"člena\",\"[vyberte funkciu]\")))),";
            formula += "signer,TEXTJOIN(\"; \",TRUE,FILTER(ABV_People[Meno]&\" – \"&ABV_People[Funkcia]," + signerMask + ",\"[vyberte podpisujúceho]\"))," +
                "IF(n=0,IF(steps=1,\"Vyplňte príjemcov a podpisujúceho na hárku Parametre.\",\"\"),CHOOSE(slot," +
                "\"Názov a sídlo účtovnej jednotky: \"&CHAR(10)&_ABV_EntityName&\", \"&_ABV_EntityAddress,\"\"," +
                "\"Meno a priezvisko: \"&INDEX(names,person),\"\",\"Menovací dekrét\",\"\"," +
                "\"Vymenúvam Vás s účinnosťou od \"&" + DateFormula("_ABV_AppointmentDate") + "&\" za \"&appointment&" +
                "\" inventúrnej komisie, ktorá vykoná riadnu inventúru majetku, záväzkov a rozdielu majetku a záväzkov k \"&" + DateFormula("_ABV_InventoryDate") +
                "&\" v súlade so zákonom č. 431/2002 Z. z. o účtovníctve v znení neskorších predpisov.\",\"\"," +
                "\"V \"&_ABV_Location&\", dňa \"&" + DateFormula("_ABV_DecreeDate") + "," +
                "\"............................................................\"&CHAR(10)&signer,\"\",\"Prevzal: ............................................................\")))";
            Formula(sheet, "A1", formula);
            var setup = sheet.PageSetup;
            try {
                setup.PaperSize = Excel.XlPaperSize.xlPaperA4; setup.Orientation = Excel.XlPageOrientation.xlPortrait;
                setup.Zoom = 100; setup.LeftMargin = 30; setup.RightMargin = 30; setup.TopMargin = 30; setup.BottomMargin = 30;
                setup.PrintArea = "$A$1:$A$12";
                setup.PrintTitleRows = ""; setup.PrintTitleColumns = "";
                setup.LeftHeader = ""; setup.CenterHeader = ""; setup.RightHeader = "";
                setup.LeftFooter = ""; setup.CenterFooter = ""; setup.RightFooter = "";
            } finally { Release(setup); }
        }
        private static string Required(Excel.Workbook book, string key, string label, bool date = false)
        {
            string value = Read(book, key, date);
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Na hárku Parametre vyplňte: " + label + ".");
            return value;
        }
        private static List<Signatory> ValidateDecrees(Excel.Workbook book, Excel.Worksheet parameters, Excel.Worksheet title, Excel.Worksheet sheet)
        {
            var people = People(parameters); var signer = DecreePlan.Validate(people); var recipients = DecreePlan.Recipients(people);
            string address = Required(book, "EntityAddress", DocumentLabels[0]);
            string issue = Required(book, "DecreeDate", DocumentLabels[1], true);
            string effective = Required(book, "AppointmentDate", DocumentLabels[2], true);
            string inventory = Required(book, "InventoryDate", DocumentLabels[3], true);
            string location = Required(book, "Location", "V (lokalita)");
            var entityCell = title.Range["B3"]; string entity;
            try { entity = Convert.ToString(entityCell.Value2); } finally { Release(entityCell); }
            if (string.IsNullOrWhiteSpace(entity)) throw new InvalidOperationException("Na titulnej strane chýba názov účtovnej jednotky.");
            if ((entity + address).Length > 180 || recipients.Any(x => x.Name.Length > 120) ||
                (signer.Name + signer.Role).Length > 160 || location.Length > 80)
                throw new InvalidOperationException("Názov, adresa alebo meno presahuje kapacitu jednostranového dekrétu. Skráťte príslušný text v parametroch.");
            if (recipients.Count > DecreeCapacity)
                throw new InvalidOperationException("Maximálny počet dekrétov je " + DecreeCapacity + ".");
            sheet.Calculate();
            if (Convert.ToBoolean(sheet.Evaluate("SUMPRODUCT(--ISERROR(A1:A" + (recipients.Count * DecreeRows) + "))>0")) ||
                !Convert.ToBoolean(sheet.Evaluate("IFERROR(ROWS(A1#)=" + (recipients.Count * DecreeRows) + ",FALSE)")))
                throw new InvalidOperationException("Skontrolujte vzorec dekrétu a uvoľnite bunky pre jeho výsledky.");
            return recipients;
        }
        // mode: 0 prepare an older workbook, 1 separate print preview, 2 one PDF per recipient.
        internal static void Decrees(int mode)
        {
            var app = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook book = null; Excel.Worksheet parameters = null, sheet = null;
            List<OutputSheet> outputs = null; object active = null;
            bool screen = app.ScreenUpdating;
            try {
                book = app.ActiveWorkbook; active = app.ActiveSheet;
                if (book == null) throw new InvalidOperationException("Najprv otvorte inventarizačný zošit.");
                outputs = Sheets(book, out parameters);
                app.ScreenUpdating = false;
                UpgradeParameters(book, parameters);
                sheet = GetDecreeSheet(book);
                if (!HasName(book, "P_EntityName")) Anchor(book, outputs.Single(x => x.Title).Sheet, "_ABV_EntityName", "$B$3");
                InitializeDecreeFormulas(book, sheet);
                if (mode == 0) { parameters.Activate(); return; }
                var recipients = ValidateDecrees(book, parameters, outputs.Single(x => x.Title).Sheet, sheet);
                app.ScreenUpdating = screen;
                sheet.ResetAllPageBreaks();
                for (int i = 1; i < recipients.Count; i++) {
                    var before = sheet.Range["A" + (i * DecreeRows + 1)]; var breaks = sheet.HPageBreaks;
                    try { Release(breaks.Add(before)); } finally { Release(breaks); Release(before); }
                }
                var fullSetup = sheet.PageSetup;
                try { fullSetup.Zoom = 100; fullSetup.PrintArea = "$A$1:$A$" + (recipients.Count * DecreeRows); }
                finally { Release(fullSetup); }
                if (mode == 1) { sheet.PrintPreview(EnableChanges: false); return; }
                using (var dialog = new FolderBrowserDialog { Description = "Priečinok pre jednotlivé menovacie dekréty", SelectedPath = PdfExportLocation.InitialDirectory }) {
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    // Share the selected parent, not the generated decree subfolder.
                    PdfExportLocation.Remember(dialog.SelectedPath);
                    // Use a new subfolder: never silently overwrite an earlier set of signed documents.
                    string folder = Path.Combine(dialog.SelectedPath, "Menovacie_dekrety_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
                    Directory.CreateDirectory(folder);
                    try {
                        for (int i = 0; i < recipients.Count; i++) {
                            string name = string.Concat(recipients[i].Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
                            if (name.Length > 80) name = name.Substring(0, 80);
                            var setup = sheet.PageSetup;
                            try {
                                setup.Zoom = false; setup.FitToPagesWide = 1; setup.FitToPagesTall = 1;
                                setup.PrintArea = "$A$" + (i * DecreeRows + 1) + ":$A$" + ((i + 1) * DecreeRows);
                                sheet.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF,
                                    Path.Combine(folder, (i + 1).ToString("D3") + "_" + name + ".pdf"), IgnorePrintAreas: false, OpenAfterPublish: false);
                            } finally { Release(setup); }
                        }
                    } catch (Exception ex) { throw new InvalidOperationException("Export sa nedokončil. Čiastočné súbory sú v: " + folder + "\n" + ex.Message, ex); }
                    finally {
                        var setup = sheet.PageSetup;
                        try { setup.Zoom = 100; setup.PrintArea = "$A$1:$A$" + (recipients.Count * DecreeRows); } finally { Release(setup); }
                    }
                    MessageBox.Show("Exportovaných dekrétov: " + recipients.Count + "\n" + folder, "Inventarizácia");
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally {
                app.ScreenUpdating = screen;
                if (mode != 0 && active is Excel.Worksheet original) original.Activate();
                if (outputs != null) foreach (var item in outputs) Release(item.Sheet);
                Release(sheet); Release(parameters); Release(active); Release(book);
            }
        }

        private static Excel.Range DecreeRowUnion(Excel.Worksheet sheet, int first, int count, int slot)
        {
            Excel.Range result = null;
            var app = sheet.Application;
            try
            {
                for (int i = first; i < first + count; i++)
                {
                    var cell = sheet.Range["A" + (i * DecreeRows + slot + 1)];
                    if (result == null)
                    {
                        result = cell;
                        continue;
                    }

                    try
                    {
                        var combined = app.Union(result, cell);
                        Release(result);
                        result = combined;
                    }
                    finally { Release(cell); }
                }
                return result;
            }
            catch
            {
                Release(result);
                throw;
            }
            finally { Release(app); }
        }

    }
}
