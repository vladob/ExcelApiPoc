using ExcelDna.Integration;
using ExcelDna.Integration.CustomUI;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelApiPoc.AddIn.Services;
using ExcelApiPoc.AddIn.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn
{
    [ComVisible(true)]
    public class PocRibbon : ExcelRibbon
    {
        public override string GetCustomUI(string ribbonId)
        {
            return @"
<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'>
  <ribbon>
    <tabs>
      <tab id='tabExcelApiPoc' label='API PoC'>
        <group id='groupAuditWorkbook' label='Audit Workbook'>
            <button
                id='buttonCreateAuditWorkbook'
                label='Create Audit Workbook'
                size='large'
                imageMso='FileNew'
                onAction='OnCreateAuditWorkbook'/>
            <button
                id='buttonRecalculateAuditReport'
                label='Recalculate Report'
                size='large'
                imageMso='RefreshAll'
                onAction='OnRecalculateAuditReport'/>
        </group>
        <group id='groupTools' label='Tools'>
            <button
                id='buttonSettings'
                label='Settings'
                size='large'
                imageMso='FileProperties'
                onAction='OnSettings'/>
        </group>
      </tab>
    </tabs>
  </ribbon>
</customUI>";
        }

        public void OnSettings(IRibbonControl control)
        {
            _ = control;

            using (var dialog = new SettingsForm())
            {
                dialog.ShowDialog();
            }
        }

        public void OnCreateAuditWorkbook(IRibbonControl control)
        {
            _ = control;
            using (var dialog = new CreateAuditWorkbookForm())
            {
                dialog.ShowDialog();
            }
        }

        public void OnRecalculateAuditReport(IRibbonControl control)
        {
            _ = control;

            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                Excel.Workbook workbook = application.ActiveWorkbook;

                if (workbook == null)
                    throw new InvalidOperationException("No active workbook was found.");

                AuditWorkbookRecalculationDialog.Show(
                    AuditWorkbookRecalculationService.Recalculate(workbook));
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Audit report calculation failed.\n\n{exception.Message}", "Audit Report Calculation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
