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
            ((Excel.Range)sheet.Cells[5, 1]).Value2 = journal.CompanyName;
            ((Excel.Range)sheet.Cells[6, 1]).Value2 = "IČO: " + journal.Ico + "    Fiscal year: " + journal.FiscalYear;
            ((Excel.Range)sheet.Cells[7, 1]).Formula =
                "=IFERROR(MID(CELL(\"filename\",A1),FIND(\"]\",CELL(\"filename\",A1))+1,255),\"Unsaved workbook\")";
            ((Excel.Range)sheet.Cells[7, 2]).Value2 = "Created by add-in: " +
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ((Excel.Range)sheet.Cells[8, 1]).Value2 = "Accounting journal: " + System.IO.Path.GetFileName(journal.SourceFileName);
            if (ledger != null)
                ((Excel.Range)sheet.Cells[9, 1]).Value2 = "General ledger: " + System.IO.Path.GetFileName(ledger.SourceFileName);
            else
                ((Excel.Range)sheet.Cells[9, 1]).Value2 = "General ledger calculated from " + journal.Rows.Count + " journal records.";
            int outsideYear = journal.Rows.Count(x => x.PostingDate.Year != journal.FiscalYear);
            if (outsideYear > 0)
                ((Excel.Range)sheet.Cells[10, 1]).Value2 = outsideYear + " journal records have dates outside the fiscal year; review their resolutions.";
            ((Excel.Range)sheet.Range["A8:A10"]).Font.Size = 9;
            ((Excel.Range)sheet.Cells[12, 1]).Value2 = "Worksheets";
            ((Excel.Range)sheet.Cells[11, 1]).Value2 = "Without the active add-in, this is a standard Excel workbook. Workbook controls and navigation updates are the user's responsibility.";
            ((Excel.Range)sheet.Cells[11, 1]).Font.Size = 9;
            ((Excel.Range)sheet.Cells[4, 1]).Font.Bold = true;
            ((Excel.Range)sheet.Cells[12, 1]).Font.Bold = true;
            sheet.Columns[1].ColumnWidth = 68;
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

            navigation.Range["A13:A1000"].Hyperlinks.Delete();
            navigation.Range["A13:A1000"].ClearContents();
            int row = 13;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
            {
                if (sheet.Name == Name || sheet.Visible != Excel.XlSheetVisibility.xlSheetVisible) continue;
                navigation.Hyperlinks.Add(navigation.Cells[row++, 1], "", "'" + sheet.Name.Replace("'", "''") + "'!A1", Type.Missing, sheet.Name);
            }
        }

        public static void RefreshIfChanged(Excel.Workbook workbook)
        {
            Excel.Worksheet navigation = null;
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == Name) navigation = sheet;
            if (navigation == null) return;

            var names = new List<string>();
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name != Name && sheet.Visible == Excel.XlSheetVisibility.xlSheetVisible)
                    names.Add(sheet.Name);
            for (int i = 0; i < names.Count; i++)
                if (!string.Equals(Convert.ToString(((Excel.Range)navigation.Cells[13 + i, 1]).Value2),
                    names[i], StringComparison.Ordinal))
                {
                    Refresh(workbook);
                    return;
                }
            if (((Excel.Range)navigation.Cells[13 + names.Count, 1]).Value2 != null)
                Refresh(workbook);
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
