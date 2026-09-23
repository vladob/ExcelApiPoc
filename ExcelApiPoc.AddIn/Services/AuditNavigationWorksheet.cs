using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditNavigationWorksheet
    {
        private const string Name = "Navigation";

        public static void Create(Excel.Workbook workbook, JournalImport journal, GeneralLedgerImport ledger)
        {
            Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets.Add(Before: workbook.Worksheets[1]);
            sheet.Name = Name;
            if (sheet.Index != 1)
                sheet.Move(Before: workbook.Worksheets[1]);
            ((Excel.Range)sheet.Cells[4, 1]).Value2 = "Audit workbook";
            ((Excel.Range)sheet.Cells[5, 1]).Value2 = "Created by add-in: " +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ((Excel.Range)sheet.Cells[6, 1]).Value2 = journal.CompanyName;
            ((Excel.Range)sheet.Cells[7, 1]).Value2 = "IČO: " + journal.Ico + "    Fiscal year: " + journal.FiscalYear;
            UpdateWorkbookFileName(workbook);
            ((Excel.Range)sheet.Cells[9, 1]).Value2 = "Accounting journal: " + System.IO.Path.GetFileName(journal.SourceFileName);
            if (ledger != null)
                ((Excel.Range)sheet.Cells[10, 1]).Value2 = "General ledger: " + System.IO.Path.GetFileName(ledger.SourceFileName);
            else
                ((Excel.Range)sheet.Cells[10, 1]).Value2 = "General ledger calculated from " + journal.Rows.Count + " journal records.";
            int outsideYear = journal.Rows.Count(x => x.PostingDate.Year != journal.FiscalYear);
            if (outsideYear > 0)
                ((Excel.Range)sheet.Cells[11, 1]).Value2 = outsideYear + " journal records have dates outside the fiscal year; review their resolutions.";
            ((Excel.Range)sheet.Range["A9:A11"]).Font.Size = 9;
            ((Excel.Range)sheet.Cells[13, 1]).Value2 = "Worksheets";
            Excel.Range notice = (Excel.Range)sheet.Cells[12, 1];
            notice.Value2 = "Without the active add-in, this is a standard Excel workbook." + Environment.NewLine +
                "Workbook controls and navigation updates are the user's responsibility.";
            notice.WrapText = true;
            notice.Font.Size = 9;
            notice.EntireRow.RowHeight = 30;
            ((Excel.Range)sheet.Cells[4, 1]).Font.Bold = true;
            ((Excel.Range)sheet.Cells[13, 1]).Font.Bold = true;
            sheet.Columns[1].ColumnWidth = 68;
            EnsureWorkbookSignificance(workbook);
            AddReturnLinks(workbook);
            Refresh(workbook);
            sheet.Activate();
        }

        public static bool Exists(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name) return true;
            return false;
        }

        public static void EnsureWorkbookSignificance(Excel.Workbook workbook)
        {
            Excel.Worksheet navigation = null;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name) { navigation = sheet; break; }
            if (navigation == null)
                throw new InvalidOperationException("The audit workbook has no Navigation worksheet.");

            ((Excel.Range)navigation.Cells[5, 4]).Value2 = "Workbook implementation significance:";
            ((Excel.Range)navigation.Cells[5, 4]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
            Excel.Range value = (Excel.Range)navigation.Cells[5, 5];
            if (value.Value2 == null) value.Value2 = 0;
            value.NumberFormat = "#,##0.00 \"€\"";
            navigation.Columns[4].ColumnWidth = 38;
            navigation.Columns[5].ColumnWidth = 18;
            foreach (Excel.Name existing in workbook.Names)
                if (string.Equals(existing.Name, "_WB_significance", StringComparison.OrdinalIgnoreCase)) return;
            workbook.Names.Add(Name: "_WB_significance", RefersTo: "='" + Name + "'!$E$5");
        }

        public static void AddReturnLinks(Excel.Workbook workbook)
        {
            if (!Exists(workbook)) return;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name != Name && sheet.Visible == Excel.XlSheetVisibility.xlSheetVisible)
                    AddReturnLink(sheet);
        }

        public static void Activate(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name)
                {
                    sheet.Activate();
                    return;
                }
        }

        public static void Refresh(Excel.Workbook workbook)
        {
            Excel.Worksheet navigation = null;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name) navigation = sheet;
            if (navigation == null) return;

            UpdateWorkbookFileName(workbook);
            navigation.Range["A14:A1000"].Hyperlinks.Delete();
            navigation.Range["A14:A1000"].ClearContents();
            int row = 14;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
            {
                if (sheet.Name == Name || sheet.Visible != Excel.XlSheetVisibility.xlSheetVisible) continue;
                navigation.Hyperlinks.Add(navigation.Cells[row++, 1], "", "'" + sheet.Name.Replace("'", "''") + "'!A1",
                    Type.Missing, NavigationCaption(sheet));
            }
        }

        public static void RefreshIfChanged(Excel.Workbook workbook)
        {
            Excel.Worksheet navigation = null;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name) navigation = sheet;
            if (navigation == null) return;
            UpdateWorkbookFileName(workbook);

            var names = new List<string>();
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name != Name && sheet.Visible == Excel.XlSheetVisibility.xlSheetVisible)
                    names.Add(NavigationCaption(sheet));
            for (int i = 0; i < names.Count; i++)
                if (!string.Equals(Convert.ToString(((Excel.Range)navigation.Cells[14 + i, 1]).Value2),
                    names[i], StringComparison.Ordinal))
                {
                    Refresh(workbook);
                    return;
                }
            if (((Excel.Range)navigation.Cells[14 + names.Count, 1]).Value2 != null)
                Refresh(workbook);
        }

        private static string NavigationCaption(Excel.Worksheet sheet)
        {
            foreach (Excel.ListObject table in sheet.ListObjects)
                if (table.Name.StartsWith("AccountDetail_", StringComparison.OrdinalIgnoreCase))
                    return "Details for account: " + sheet.Name;
            return sheet.Name;
        }

        public static void UpdateWorkbookFileName(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name)
                {
                    string filename = string.IsNullOrEmpty(workbook.Path) ? "Unsaved workbook" : workbook.Name;
                    Excel.Range cell = (Excel.Range)sheet.Cells[8, 1];
                    if (!string.Equals(Convert.ToString(cell.Value2), filename, StringComparison.Ordinal))
                        cell.Value2 = filename;
                    return;
                }
        }

        public static void AddReturnLink(Excel.Worksheet sheet)
        {
            Excel.Range cell = (Excel.Range)sheet.Cells[1, 1];
            if (cell.Hyperlinks.Count > 0)
            {
                Excel.Hyperlink existing = cell.Hyperlinks[1];
                if (string.Equals(existing.SubAddress, "'" + Name + "'!A1", StringComparison.OrdinalIgnoreCase))
                    existing.TextToDisplay = "Home";
                return;
            }
            if (cell.Value2 == null && cell.HasFormula == false)
                sheet.Hyperlinks.Add(cell, "", "'" + Name + "'!A1", Type.Missing, "Home");
        }
    }
}
