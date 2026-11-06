using System;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace AccountBalanceVerification.AddIn
{
    // Excel raises SheetChange for edits, pastes and table row additions/deletions.
    // Keep this handler alive for the lifetime of the XLL.
    public sealed class VerificationAddIn : IExcelAddIn
    {
        private Excel.Application application;
        private bool updating;

        public void AutoOpen()
        {
            application = (Excel.Application)ExcelDnaUtil.Application;
            application.SheetChange += OnSheetChange;
        }

        public void AutoClose()
        {
            if (application != null) application.SheetChange -= OnSheetChange;
            application = null;
        }

        private void OnSheetChange(object changedSheet, Excel.Range target)
        {
            if (updating || !(changedSheet is Excel.Worksheet sheet)) return;
            Excel.Workbook book = null;
            bool events = application.EnableEvents;
            try
            {
                if (WorkbookOutput.Property(sheet, "ABVRole") != "Parameters") return;
                updating = true;
                application.EnableEvents = false;
                book = (Excel.Workbook)sheet.Parent;
                WorkbookOutput.Refresh(book);
                WorkbookOutput.Text(sheet, "A9", "Podpisové časti sú aktualizované.");
            }
            catch (Exception ex)
            {
                // A newly added person may be incomplete while the user is typing.
                // Keep the previous output and explain the pending refresh without
                // interrupting each edit. Print/PDF still validates and blocks it.
                try { WorkbookOutput.Text(sheet, "A9", "Podpisy neboli aktualizované: " + ex.Message); }
                catch { System.Diagnostics.Trace.WriteLine(ex); }
            }
            finally
            {
                application.EnableEvents = events;
                updating = false;
                WorkbookOutput.Release(book);
            }
        }
    }
}
