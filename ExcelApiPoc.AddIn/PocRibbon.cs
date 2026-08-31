using ExcelApiPoc.AddIn.Forms;
using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using ExcelDna.Integration;
using ExcelDna.Integration.CustomUI;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq;

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
            <button
                id=""btnGetAccountingEntityPackage""
                label=""Get Entity Package""
                onAction=""OnGetAccountingEntityPackage""
                size=""large"" />
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

        public void OnGetAccountingEntityPackage(IRibbonControl control)
        {
            try
            {
                AccountingEntityPackageDto package = AccountingEntityPackageApiClient.GetPackage("23451234");

                MessageBox.Show(
                    BuildAccountingEntityPackageSummary(package),
                    "Accounting Entity Package",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (AccountingEntityPackageNotFoundException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Accounting Entity Package",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Accounting Entity Package",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string BuildAccountingEntityPackageSummary(
            AccountingEntityPackageDto package)
        {
            int statementCount = package.FinancialStatements?.Count ?? 0;

            int reportCount =
                package.FinancialStatements?
                    .Sum(statement =>
                        statement.FinancialReports?.Count ?? 0)
                ?? 0;

            int tableCount =
                package.FinancialStatements?
                    .Sum(statement =>
                        statement.FinancialReports?
                            .Sum(report =>
                                report.Tables?.Count ?? 0)
                        ?? 0)
                ?? 0;

            int valueCount =
                package.FinancialStatements?
                    .Sum(statement =>
                        statement.FinancialReports?
                            .Sum(report =>
                                report.Tables?
                                    .Sum(table =>
                                        table.Values?.Count ?? 0)
                                ?? 0)
                        ?? 0)
                ?? 0;
            return
                $"IČO: {package.Entity?.Ico}\r\n" +
                $"Name: {package.Entity?.Name}\r\n" +
                $"Statements: {statementCount}\r\n" +
                $"Tables: {tableCount}\r\n" +
                $"Values: {valueCount}\r\n" +
                $"Generated: {package.GeneratedAtUtc:u}";
        }
    }
}
