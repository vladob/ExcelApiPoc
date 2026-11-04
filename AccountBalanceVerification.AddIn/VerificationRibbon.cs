using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelDna.Integration.CustomUI;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;
namespace AccountBalanceVerification.AddIn
{
    [ComVisible(true)]
    public sealed class VerificationRibbon : ExcelRibbon
    {
        public override string GetCustomUI(string ribbonId) =>
            "<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'><ribbon><tabs><tab id='abvTab' label='Inventarizácia'><group id='abvCreate' label='Zošit'><button id='abvNew' label='Vytvoriť inventarizáciu' size='large' onAction='CreateWorkbook'/></group></tab></tabs></ribbon></customUI>";
        public void CreateWorkbook(IRibbonControl control)
        {
            try
            {
                string workbookName;
                using (var form = new CreateWorkbookForm())
                {
                    form.ShowDialog();
                    workbookName = form.CreatedWorkbookName;
                }
                // Run after the modal dialog and ribbon callback have returned,
                // so Excel has finished restoring focus to the launching window.
                if (!string.IsNullOrEmpty(workbookName))
                    ExcelAsyncUtil.QueueAsMacro(() => ActivateCreatedWorkbook(workbookName));
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        private static void ActivateCreatedWorkbook(string workbookName)
        {
            Excel.Workbooks books = null;
            Excel.Workbook book = null;
            Excel.Windows windows = null;
            Excel.Window window = null;
            try
            {
                var app = (Excel.Application)ExcelDnaUtil.Application;
                books = app.Workbooks;
                book = books[workbookName];
                book.Activate();
                windows = book.Windows;
                if (windows.Count > 0)
                {
                    window = windows[1];
                    window.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zošit bol vytvorený, ale nepodarilo sa ho aktivovať: " + ex.Message,
                    "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (window != null) Marshal.ReleaseComObject(window);
                if (windows != null) Marshal.ReleaseComObject(windows);
                if (book != null) Marshal.ReleaseComObject(book);
                if (books != null) Marshal.ReleaseComObject(books);
            }
        }
    }
}
