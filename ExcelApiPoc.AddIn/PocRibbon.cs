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
        private const string RegisterUzReportsTableName = "RegisterUzReports";
        private const string RecalculateGeneralLedgerControlId = "buttonRecalculateGeneralLedger";
        private const string RecalculateAuditReportControlId = "buttonRecalculateAuditReport";
        private const string OpenRegisterUzReportControlId = "buttonOpenRegisterUzReport";

        private IRibbonUI _ribbon;
        private bool _applicationEventsSubscribed;

        public override string GetCustomUI(string ribbonId)
        {
            return @"
<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui' onLoad='OnRibbonLoad'>
  <ribbon>
    <tabs>
      <tab id='tabExcelApiPoc' label='API PoC'>
        <group id='groupAuditWorkbook' label='Audit Workbook'>
            <button id='buttonCreateAuditWorkbook' label='Create Audit Workbook' size='large' imageMso='FileNew' onAction='OnCreateAuditWorkbook'/>
            <button id='buttonSettings' label='Settings' size='large' imageMso='ApplicationOptionsDialog' onAction='OnSettings'/>
            <button id='buttonCreateAccountDetails' label='Create missing account sheets' size='large' imageMso='TableInsert' onAction='OnCreateAccountDetails' getEnabled='GetRecalculateGeneralLedgerEnabled'/>
            <button id='buttonRefreshNavigation' label='Refresh navigation' size='large' imageMso='RefreshAll' onAction='OnRefreshNavigation' getEnabled='GetRecalculateGeneralLedgerEnabled'/>
        </group>
        <group id='groupAnalysis' label='Analysis'>
            <button id='buttonRecalculateGeneralLedger'
                label='Recalculate General Ledger from Accounting Journal'
                size='large'
                imageMso='RefreshAll'
                onAction='OnRecalculateGeneralLedger'
                getEnabled='GetRecalculateGeneralLedgerEnabled'/>
            <button id='buttonRecalculateAuditReport'
                label='Recalculate Report'
                size='large'
                imageMso='CalculateSheet'
                onAction='OnRecalculateAuditReport'
                getEnabled='GetRecalculateAuditReportEnabled'/>
        </group>
        <group id='groupRegisterUz' label='RegisterUZ'>
            <button id='buttonOpenRegisterUzReport' label='Open RegisterUZ Report' size='large' imageMso='FieldChooser' onAction='OnOpenRegisterUzReport' getEnabled='GetOpenRegisterUzReportEnabled'/>
        </group>
      </tab>
    </tabs>
  </ribbon>
</customUI>";
        }

        public void OnRibbonLoad(IRibbonUI ribbon)
        {
            _ribbon = ribbon;
            if (_applicationEventsSubscribed) return;

            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            application.SheetSelectionChange += OnSheetSelectionChange;
            application.SheetActivate += OnSheetActivate;
            application.WorkbookActivate += OnWorkbookActivate;
            application.WorkbookAfterSave += OnWorkbookAfterSave;
            _applicationEventsSubscribed = true;
        }

        public bool GetRecalculateGeneralLedgerEnabled(IRibbonControl control)
        {
            _ = control;
            return IsActiveAuditWorkbook();
        }

        public bool GetRecalculateAuditReportEnabled(IRibbonControl control)
        {
            _ = control;
            return IsActiveAuditWorkbook();
        }

        private static bool IsActiveAuditWorkbook()
        {
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                return AuditWorkbookIdentity.IsAuditWorkbook(application.ActiveWorkbook);
            }
            catch
            {
                return false;
            }
        }

        public bool GetOpenRegisterUzReportEnabled(IRibbonControl control)
        {
            _ = control;
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                if (!AuditWorkbookIdentity.IsAuditWorkbook(application.ActiveWorkbook)) return false;

                Excel.Worksheet worksheet = application.ActiveSheet as Excel.Worksheet;
                if (worksheet == null) return false;

                Excel.ListObject table = null;
                foreach (Excel.ListObject candidate in worksheet.ListObjects)
                {
                    if (string.Equals(candidate.Name, RegisterUzReportsTableName, StringComparison.OrdinalIgnoreCase))
                    {
                        table = candidate;
                        break;
                    }
                }

                Excel.Range data = table?.DataBodyRange;
                Excel.Range activeCell = application.ActiveCell as Excel.Range;
                return data != null && activeCell != null &&
                    activeCell.Row >= data.Row && activeCell.Row < data.Row + data.Rows.Count &&
                    activeCell.Column >= data.Column && activeCell.Column < data.Column + data.Columns.Count;
            }
            catch
            {
                return false;
            }
        }

        private void OnSheetSelectionChange(object sheet, Excel.Range target)
        {
            _ = sheet; _ = target; InvalidateAuditControls();
        }

        private void OnSheetActivate(object sheet)
        {
            _ = sheet;
            InvalidateAuditControls();
            RefreshNavigationIfNeeded();
        }

        private void OnWorkbookActivate(Excel.Workbook workbook)
        {
            _ = workbook;
            InvalidateAuditControls();
            RefreshNavigationIfNeeded();
        }

        private void OnWorkbookAfterSave(Excel.Workbook workbook, bool success)
        {
            if (success && AuditWorkbookIdentity.IsAuditWorkbook(workbook))
                AuditNavigationWorksheet.UpdateWorkbookFileName(workbook);
        }

        private static void RefreshNavigationIfNeeded()
        {
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                if (application.ActiveWorkbook != null && AuditWorkbookIdentity.IsAuditWorkbook(application.ActiveWorkbook))
                    AuditNavigationWorksheet.RefreshIfChanged(application.ActiveWorkbook);
            }
            catch
            {
                // Navigation remains available through the explicit Refresh navigation action.
            }
        }

        private void InvalidateAuditControls()
        {
            _ribbon?.InvalidateControl(RecalculateGeneralLedgerControlId);
            _ribbon?.InvalidateControl(RecalculateAuditReportControlId);
            _ribbon?.InvalidateControl(OpenRegisterUzReportControlId);
        }

        public void OnSettings(IRibbonControl control)
        {
            _ = control;
            using (var dialog = new SettingsForm()) dialog.ShowDialog();
        }

        public void OnCreateAuditWorkbook(IRibbonControl control)
        {
            _ = control;
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook auditWorkbook = application.Workbooks.Add();
            auditWorkbook.Activate();
            using (var dialog = new CreateAuditWorkbookForm(auditWorkbook))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    auditWorkbook.Close(SaveChanges: false);
            }
        }

        public void OnCreateAccountDetails(IRibbonControl control)
        {
            _ = control;
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                Excel.Workbook workbook = application.ActiveWorkbook;
                if (workbook == null || !AuditWorkbookIdentity.IsAuditWorkbook(workbook))
                    throw new InvalidOperationException("Open an audit workbook first.");
                int count = AccountDetailWorksheetWriter.CreateMissing(workbook);
                MessageBox.Show(count + " account sheet(s) created.", "Account details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Account details", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnRefreshNavigation(IRibbonControl control)
        {
            _ = control;
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            if (application.ActiveWorkbook != null && AuditWorkbookIdentity.IsAuditWorkbook(application.ActiveWorkbook))
                AuditNavigationWorksheet.Refresh(application.ActiveWorkbook);
        }

        public void OnRecalculateGeneralLedger(IRibbonControl control)
        {
            _ = control;
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                Excel.Workbook workbook = application.ActiveWorkbook;
                if (workbook == null) throw new InvalidOperationException("No active workbook was found.");
                if (!AuditWorkbookIdentity.IsAuditWorkbook(workbook))
                    throw new InvalidOperationException("The active workbook is not an audit workbook.");

                AuditWorkbookRecalculationResult result;
                using (new ExcelBusyCursor(application))
                using (new ExcelApplicationStateScope(application, disableEvents: false))
                {
                    AuditWorkbookRecalculationService.RecalculateGeneralLedgerFromJournal(workbook);
                    result = AnalyticalMappingHeuristicRefreshService.RefreshAndRecalculate(workbook);
                }
                AuditWorkbookRecalculationDialog.Show(result);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "General-ledger recalculation failed.\n\n" + exception.Message,
                    "Recalculate General Ledger from Accounting Journal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnRecalculateAuditReport(IRibbonControl control)
        {
            _ = control;
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                Excel.Workbook workbook = application.ActiveWorkbook;
                if (workbook == null) throw new InvalidOperationException("No active workbook was found.");
                if (!AuditWorkbookIdentity.IsAuditWorkbook(workbook))
                    throw new InvalidOperationException("The active workbook is not an audit workbook.");

                AuditWorkbookRecalculationResult result;
                using (new ExcelBusyCursor(application))
                using (new ExcelApplicationStateScope(application, disableEvents: false))
                    result = AuditWorkbookRecalculationService.Recalculate(workbook);

                AuditWorkbookRecalculationDialog.Show(result);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Audit report calculation failed.\n\n" + exception.Message,
                    "Audit Report Calculation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnOpenRegisterUzReport(IRibbonControl control)
        {
            _ = control;
            try
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                if (!AuditWorkbookIdentity.IsAuditWorkbook(application.ActiveWorkbook))
                    throw new InvalidOperationException("The active workbook is not an audit workbook.");

                using (new ExcelBusyCursor(application))
                using (new ExcelApplicationStateScope(application, disableEvents: false))
                    RegisterUzReportRenderingService.RenderSelectedReportTable();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "RegisterUZ report rendering failed.\n\n" + exception.Message,
                    "Open RegisterUZ Report",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnGetAccountingEntityPackage(IRibbonControl control)
        {
            try
            {
                string ico = "00312011";
                string summary = AccountingEntityPackageApiClient.GetEnvelopeSummary(ico);
                MessageBox.Show(summary, "Accounting Entity Graph", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (AccountingEntityPackageNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Accounting Entity Package", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (AccountingEntityPackageAmbiguousException ex)
            {
                MessageBox.Show(ex.Message, "Accounting Entity Package", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Accounting Entity Package", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string BuildAccountingEntityPackageSummary(AccountingEntityPackageDto package)
        {
            int statementCount = package.FinancialStatements?.Count ?? 0;
            int reportCount = package.FinancialStatements?.Sum(statement => statement.FinancialReports?.Count ?? 0) ?? 0;
            int tableCount = package.FinancialStatements?.Sum(statement => statement.FinancialReports?.Sum(report => report.Tables?.Count ?? 0) ?? 0) ?? 0;
            int valueCount = package.FinancialStatements?.Sum(statement => statement.FinancialReports?.Sum(report => report.Tables?.Sum(table => table.Values?.Count ?? 0) ?? 0) ?? 0) ?? 0;
            return $"IČO: {package.Entity?.Ico}\r\n" +
                   $"Name: {package.Entity?.Name}\r\n" +
                   $"Statements: {statementCount}\r\n" +
                   $"Tables: {tableCount}\r\n" +
                   $"Values: {valueCount}\r\n" +
                   $"Generated: {package.GeneratedAtUtc:u}";
        }
    }
}
