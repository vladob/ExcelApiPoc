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
            Excel.Workbooks books = null;
            Excel.Workbook book = null;
            bool completed = false;
            try
            {
                var app = (Excel.Application)ExcelDnaUtil.Application;
                books = app.Workbooks;
                book = books.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                // Match the existing audit add-in: activate before constructing/showing
                // the modal dialog, so Excel restores focus to this new workbook.
                book.Activate();
                using (var form = new CreateWorkbookForm(book))
                    completed = form.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    if (book != null && !completed) book.Close(SaveChanges: false);
                }
                finally
                {
                    if (book != null) Marshal.ReleaseComObject(book);
                    if (books != null) Marshal.ReleaseComObject(books);
                }
            }
        }
    }
}
