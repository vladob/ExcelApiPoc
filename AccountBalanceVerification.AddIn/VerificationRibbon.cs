using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelDna.Integration.CustomUI;
namespace AccountBalanceVerification.AddIn
{
    [ComVisible(true)]
    public sealed class VerificationRibbon : ExcelRibbon
    {
        public override string GetCustomUI(string ribbonId) =>
            "<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'><ribbon><tabs><tab id='abvTab' label='Inventarizácia'><group id='abvCreate' label='Zošit'><button id='abvNew' label='Vytvoriť inventarizáciu' size='large' onAction='CreateWorkbook'/></group></tab></tabs></ribbon></customUI>";
        public void CreateWorkbook(IRibbonControl control)
        {
            try { using (var form = new CreateWorkbookForm()) form.ShowDialog(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
