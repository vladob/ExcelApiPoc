using ExcelApiPoc.AddIn.Forms;
using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using ExcelDna.Integration;
using ExcelDna.Integration.CustomUI;
using Microsoft.Office.Interop.Excel;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
            <button
                id='buttonOpenRegisterUzReport'
                label='Open RegisterUZ Report'
                size='large'
                imageMso='FileOpen'
                onAction='OnOpenRegisterUzReport'/>
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

            Excel.Application application =
                (Excel.Application)ExcelDnaUtil.Application;

            Excel.Workbook auditWorkbook =
                application.Workbooks.Add();

            auditWorkbook.Activate();

            using (var dialog = new CreateAuditWorkbookForm(auditWorkbook))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    auditWorkbook.Close(SaveChanges: false);
                }
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

        public void OnOpenRegisterUzReport(IRibbonControl control)
        {
            _ = control;

            try
            {
                RegisterUzReportRenderingService.RenderSelectedReportTable();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"RegisterUZ report rendering failed.\n\n{exception.Message}",
                    "Open RegisterUZ Report",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnGetAccountingEntityPackage(IRibbonControl control)
        {
            try
            {
                // string ico = "36206075"; // CONSULTING, s.r.o.
                // string ico = "00325554"; // Obec Oreské
                 string ico = "00312011"; // Obec Svinná
                // string ico = "36601837"; // BOJKUN spol. s r.o.
                /*

                AccountingEntityPackageDto package = AccountingEntityPackageApiClient.GetPackage(ico);

                                MessageBox.Show(
                                    BuildAccountingEntityPackageSummary(package),
                                    "Accounting Entity Package",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                */


                string summary = AccountingEntityPackageApiClient.GetEnvelopeSummary(ico);

                MessageBox.Show(
                    summary,
                    "Accounting Entity Graph",
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
            catch (AccountingEntityPackageAmbiguousException ex)
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
