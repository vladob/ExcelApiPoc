using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AccountBalanceVerification.Core;
using Excel = Microsoft.Office.Interop.Excel;

namespace AccountBalanceVerification.AddIn
{
    internal static partial class WorkbookOutput
    {
        private static readonly int InputColor = ColorTranslator.ToOle(Color.FromArgb(242, 242, 210));
        private static void Input(Excel.Worksheet sheet, string address, string format = "@")
        {
            var r = sheet.Range[address];
            try { r.NumberFormat = format; r.Interior.Color = InputColor; r.Font.Color = 0; } finally { Release(r); }
        }
        private static Excel.Worksheet AddSheet(Excel.Workbook book, string name, string role)
        {
            var sheets = book.Worksheets; var last = (Excel.Worksheet)sheets[sheets.Count];
            try { var sheet = (Excel.Worksheet)sheets.Add(After: last); sheet.Name = name; Tag(sheet, role); return sheet; }
            finally { Release(last); Release(sheets); }
        }
        private static bool HasName(Excel.Workbook book, string name)
        {
            Excel.Range r = null;
            try { r = NamedRange(book, name); return true; }
            catch (System.Runtime.InteropServices.COMException) { return false; }
            finally { Release(r); }
        }
        private static void PublicParameter(Excel.Workbook book, Excel.Worksheet sheet, string address, string publicName, params string[] aliases)
        {
            var names = book.Names;
            try { AddName(names, publicName, "='" + sheet.Name.Replace("'", "''") + "'!" + address); }
            finally { Release(names); }
            foreach (string alias in aliases) Anchor(book, sheet, alias, address);
        }
        internal static void CreateParametersV12(Excel.Workbook book, Excel.Worksheet sheet, IEnumerable<string> warnings, VerificationPackage package)
        {
            sheet.Name = "Parametre"; Tag(sheet, "Parameters");
            Text(sheet, "A1", "Parametre inventarizácie");
            string[] left = { "Účtovná jednotka", "IČO", "Účtovný rok", "V (lokalita)", "Dňa", "Deň začatia inventúry", "Deň skončenia inventúry" };
            for (int i = 0; i < left.Length; i++) { Text(sheet, "A" + (i + 2), left[i]); Input(sheet, "B" + (i + 2), i < 2 || i == 3 ? "@" : i == 2 ? "0" : "d.m.yyyy"); }
            Text(sheet, "B2", package.EntityName); Text(sheet, "B3", package.Ico);
            var year = sheet.Range["B4"]; try { year.Value2 = package.FiscalYear; } finally { Release(year); }
            PublicParameter(book, sheet, "$B$2", "P_EntityName", "_ABV_EntityName");
            PublicParameter(book, sheet, "$B$3", "P_ICO"); PublicParameter(book, sheet, "$B$4", "P_FiscalYear");
            PublicParameter(book, sheet, "$B$5", "P_Location", "_ABV_Location", "Location");
            PublicParameter(book, sheet, "$B$6", "P_DocumentDate", "_ABV_Date", "ProcessDate");
            PublicParameter(book, sheet, "$B$7", "P_InventoryStartDate", "_ABV_StartDate", "InventoryBeginning");
            PublicParameter(book, sheet, "$B$8", "P_InventoryEndDate", "_ABV_EndDate", "InventoryEnd");
            string[] publicNames = { "P_AeAddress", "P_DecreeIssueDate", "P_AppointmentEffectiveFrom", "P_InventoryDate", "P_OrderNumber", "P_OrderIssueDate", "P_OrderReceiptDate" };
            for (int i = 0; i < DocumentKeys.Length; i++) {
                Text(sheet, "E" + (i + 2), DocumentLabels[i]);
                Input(sheet, "F" + (i + 2), i == 0 || i == 4 ? "@" : "d.m.yyyy");
                PublicParameter(book, sheet, "$F$" + (i + 2), publicNames[i], "_ABV_" + DocumentKeys[i]);
            }
            Text(sheet, "F2", package.EntityAddress);
            var inventory = sheet.Range["F5"]; try { inventory.Value2 = new DateTime(package.FiscalYear, 12, 31).ToOADate(); } finally { Release(inventory); }
            Text(sheet, "A10", "Svetlé bunky sú upraviteľné. Osoby pridávajte rozšírením tabuľky ABV_People.");
            Text(sheet, "A11", "Dokumenty sa prepočítajú vzorcami. Mapovanie a závery správy overte pred podpisom.");
            string[] headers = { "Funkcia", "Meno", "Prvá", "Účty", "Menovací dekrét", "Rekapitulácia" };
            for (int i = 0; i < headers.Length; i++) Text(sheet, ((char)('A' + i)) + "13", headers[i]);
            var data = new object[5, 6];
            string[] roles = { "Predseda", "Člen", "Člen", "Starosta", "Zodpovedná osoba" };
            for (int i = 0; i < 5; i++) {
                data[i, 0] = roles[i]; data[i, 1] = ""; data[i, 2] = i < 3 || i == 4; data[i, 3] = i < 3;
                data[i, 4] = i < 3 ? DecreePlan.Recipient : ""; data[i, 5] = i < 3 ? DecreePlan.Commission : "";
            }
            var body = sheet.Range["A14:F18"]; try { body.Value2 = data; body.Interior.Color = InputColor; } finally { Release(body); }
            var area = sheet.Range["A13:F18"]; var tables = sheet.ListObjects;
            try {
                var table = tables.Add(Excel.XlListObjectSourceType.xlSrcRange, area, Type.Missing, Excel.XlYesNoGuess.xlYes);
                try { table.Name = "ABV_People"; table.TableStyle = "TableStyleLight1"; table.ShowTableStyleRowStripes = false; }
                finally { Release(table); }
            } finally { Release(tables); Release(area); }
            var support = AddSheet(book, "ABV_Podpora", "Support");
            try {
                var choices = Roles();
                for (int i = 0; i < choices.Length; i++) Text(support, "A" + (i + 1), choices[i]);
                Anchor(book, support, "_ABV_Roles", "$A$1:$A$" + choices.Length);
                Text(support, "B1", DecreePlan.Recipient); Text(support, "B2", DecreePlan.Signer);
                Text(support, "C1", DecreePlan.Commission); Text(support, "C2", DecreePlan.Signer);
                Anchor(book, support, "_ABV_DecreeRoles", "$B$1:$B$2"); Anchor(book, support, "_ABV_RecapRoles", "$C$1:$C$2");
                var bools = support.Range["D1:D2"]; try { bools.Value2 = new object[,] { { true }, { false } }; } finally { Release(bools); }
                Anchor(book, support, "_ABV_BooleanChoices", "$D$1:$D$2");
                Formula(support, "E2", "=SUMPRODUCT(" + PeopleMask(false) + ")"); Formula(support, "F2", "=SUMPRODUCT(" + PeopleMask(true) + ")");
                Anchor(book, support, "ABV_AccountPeopleCount", "$E$2"); Anchor(book, support, "ABV_TitlePeopleCount", "$F$2");
                sheet.Activate(); support.Visible = Excel.XlSheetVisibility.xlSheetHidden;
            } finally { Release(support); }
            ConfigurePeopleInputs(sheet);
            Text(sheet, "A22", "Zdroj"); Text(sheet, "B22", package.Source);
            Text(sheet, "A23", "Načítané (UTC)"); Text(sheet, "B23", package.RetrievedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            Text(sheet, "A25", "Správy a upozornenia");
            int row = 26; foreach (string message in warnings.Distinct()) {
                var note = sheet.Range["A" + row + ":F" + row];
                try { note.Merge(); note.WrapText = true; note.RowHeight = 32; } finally { Release(note); }
                Text(sheet, "A" + row++, message);
            }
            var range = sheet.Range["A:A"]; try { range.ColumnWidth = 30; } finally { Release(range); }
            range = sheet.Range["B:B"]; try { range.ColumnWidth = 36; } finally { Release(range); }
            range = sheet.Range["C:D"]; try { range.ColumnWidth = 9; } finally { Release(range); }
            range = sheet.Range["E:E"]; try { range.ColumnWidth = 31; } finally { Release(range); }
            range = sheet.Range["F:F"]; try { range.ColumnWidth = 43; } finally { Release(range); }
            range = sheet.Range["A2:F8"]; try { range.WrapText = true; range.RowHeight = 32; } finally { Release(range); }
        }
        private static void ConfigurePeopleInputs(Excel.Worksheet sheet)
        {
            var tables = sheet.ListObjects; var table = tables["ABV_People"]; var body = table.DataBodyRange;
            try {
                if (body == null) return;
                var rows = body.Rows; int last;
                try { last = body.Row + rows.Count - 1; } finally { Release(rows); }
                string start = body.Row.ToString();
                Input(sheet, "A" + start + ":B" + last); Input(sheet, "E" + start + ":F" + last);
                Input(sheet, "C" + start + ":D" + last, "General");
                Validate(sheet, "A" + start + ":A" + last, "=_ABV_Roles");
                Validate(sheet, "E" + start + ":E" + last, "=_ABV_DecreeRoles");
                Validate(sheet, "F" + start + ":F" + last, "=_ABV_RecapRoles");
                var flags = sheet.Range["C" + start + ":D" + last];
                try { try { ((dynamic)flags).CellControl.SetCheckbox(); }
                    catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException || ex is Microsoft.CSharp.RuntimeBinder.RuntimeBinderException) {
                        Validate(sheet, "C" + start + ":D" + last, "=_ABV_BooleanChoices");
                    }
                } finally { Release(flags); }
            } finally { Release(body); Release(table); Release(tables); }
        }
    }
}
