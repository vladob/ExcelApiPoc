using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookWriter
    {
        public static Excel.Workbook CreateWorkbook(
            Excel.Workbook workbook,
            JournalImport journalImport,
            IReadOnlyList<AccountSummary> accountSummaries,
            CalculatedGeneralLedger calculatedGeneralLedger,
            JournalLedgerReconciliationResult journalLedgerReconciliation,
            AccountFrameworkLoadResult frameworkLoad,
            AnalyticalMappingData analyticalMapping,
            AuditTemplatePackageResponse templatePackage,
            AuditReportContext reportContext,
            AuditTemplatePackageLoadResult templatePackageLoad,
            RegisterUzFinancialReportSelection registerUzReportSelection,
            AccountingEntityPackageEnvelope accountingEntityPackage,
            AccountingFrameworkImport accountingFrameworkImport,
            GeneralLedgerImport generalLedgerImport)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            using (new ExcelApplicationStateScope(workbook.Application))
            {
                AddSourceDataWorksheets(
                    workbook,
                    journalImport,
                    accountSummaries,
                    accountingFrameworkImport,
                    generalLedgerImport);

                CalculatedGeneralLedgerComparisonWorksheetWriter.AddWorksheet(
                    workbook,
                    calculatedGeneralLedger,
                    journalLedgerReconciliation,
                    accountSummaries);

                if (analyticalMapping != null &&
                    analyticalMapping.Rows.Count > 0)
                {
                    AnalyticalMappingValidationWorksheetWriter.AddWorksheet(
                        workbook,
                        analyticalMapping.Options);

                    AnalyticalMappingWorksheetWriter.AddWorksheet(
                        workbook,
                        analyticalMapping.Rows);
                }

                AuditCalculationPackageWorksheetWriter.AddWorksheet(
                    workbook,
                    templatePackage,
                    reportContext,
                    templatePackageLoad);

                RegisterUzReferenceWorksheetWriter.AddWorksheet(
                    workbook,
                    registerUzReportSelection);

                RegisterUzReportsWorksheetWriter.AddWorksheet(
                    workbook,
                    accountingEntityPackage);

                MultiYearBalanceSheetWorksheetWriter.AddWorksheets(
                    workbook,
                    accountingEntityPackage);

                RegisterUzAttachmentsWorksheetWriter.AddWorksheet(
                    workbook,
                    accountingEntityPackage);

                ImportMetadataWorksheetWriter.AddWorksheet(
                    workbook,
                    journalImport,
                    frameworkLoad,
                    templatePackage,
                    reportContext,
                    templatePackageLoad,
                    accountingFrameworkImport,
                    generalLedgerImport);

                AuditWorkbookWorksheetLayout.Apply(workbook);
                AuditWorkbookIdentity.Stamp(workbook);
            }

            JournalDateExceptionWarning.Show(journalImport);
            return workbook;
        }

        public static Excel.Workbook CreateWithoutCalculation(
            Excel.Workbook workbook,
            JournalImport journalImport,
            IReadOnlyList<AccountSummary> accountSummaries,
            CalculatedGeneralLedger calculatedGeneralLedger,
            JournalLedgerReconciliationResult journalLedgerReconciliation,
            AccountingFrameworkImport accountingFrameworkImport,
            GeneralLedgerImport generalLedgerImport,
            AccountingEntityPackageEnvelope accountingEntityPackage,
            Exception calculationFailure)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            using (new ExcelApplicationStateScope(workbook.Application))
            {
                AddSourceDataWorksheets(
                    workbook,
                    journalImport,
                    accountSummaries,
                    accountingFrameworkImport,
                    generalLedgerImport);

                CalculatedGeneralLedgerComparisonWorksheetWriter.AddWorksheet(
                    workbook,
                    calculatedGeneralLedger,
                    journalLedgerReconciliation,
                    accountSummaries);

                if (accountingEntityPackage != null)
                {
                    RegisterUzReportsWorksheetWriter.AddWorksheet(
                        workbook,
                        accountingEntityPackage);

                    MultiYearBalanceSheetWorksheetWriter.AddWorksheets(
                        workbook,
                        accountingEntityPackage);

                    RegisterUzAttachmentsWorksheetWriter.AddWorksheet(
                        workbook,
                        accountingEntityPackage);
                }

                ImportMetadataWorksheetWriter.AddWithoutCalculation(
                    workbook,
                    journalImport,
                    accountingFrameworkImport,
                    generalLedgerImport);

                NoCalculationReportWorksheetWriter.AddWorksheet(
                    workbook,
                    journalImport,
                    accountingFrameworkImport,
                    generalLedgerImport,
                    accountingEntityPackage,
                    calculationFailure);

                AuditWorkbookWorksheetLayout.Apply(workbook);
                AuditWorkbookIdentity.Stamp(workbook);
            }

            JournalDateExceptionWarning.Show(journalImport);
            return workbook;
        }

        private static Excel.Worksheet AddSourceDataWorksheets(
            Excel.Workbook workbook,
            JournalImport journalImport,
            IReadOnlyList<AccountSummary> accountSummaries,
            AccountingFrameworkImport accountingFrameworkImport,
            GeneralLedgerImport generalLedgerImport)
        {
            Excel.Worksheet journalWorksheet =
                JournalWorksheetWriter.AddWorksheet(
                    workbook,
                    journalImport);

            AccountWorksheetWriter.AddWorksheet(
                workbook,
                accountSummaries);

            if (accountingFrameworkImport != null)
            {
                AccountingFrameworkWorksheetWriter.AddWorksheet(
                    workbook,
                    accountingFrameworkImport);
            }

            if (generalLedgerImport != null)
            {
                GeneralLedgerWorksheetWriter.AddWorksheet(
                    workbook,
                    generalLedgerImport);
            }

            return journalWorksheet;
        }
    }

    internal static class AuditWorkbookIdentity
    {
        internal const string WorkbookKindProperty = "ExcelApiPoc.WorkbookKind";
        internal const string SchemaVersionProperty = "ExcelApiPoc.SchemaVersion";
        internal const string AddInVersionProperty = "ExcelApiPoc.AddInVersion";
        internal const string WorkbookIdProperty = "ExcelApiPoc.WorkbookId";

        internal const string AuditWorkbookKind = "AuditWorkbook";
        internal const string CurrentSchemaVersion = "1";

        private const int OfficeStringPropertyType = 4;

        public static void Stamp(Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            string workbookId = GetProperty(workbook, WorkbookIdProperty);
            if (string.IsNullOrWhiteSpace(workbookId))
                workbookId = Guid.NewGuid().ToString("D");

            SetProperty(workbook, WorkbookKindProperty, AuditWorkbookKind);
            SetProperty(workbook, SchemaVersionProperty, CurrentSchemaVersion);
            SetProperty(
                workbook,
                AddInVersionProperty,
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
            SetProperty(workbook, WorkbookIdProperty, workbookId);
        }

        public static bool IsAuditWorkbook(Excel.Workbook workbook)
        {
            if (workbook == null)
                return false;

            return string.Equals(
                       GetProperty(workbook, WorkbookKindProperty),
                       AuditWorkbookKind,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       GetProperty(workbook, SchemaVersionProperty),
                       CurrentSchemaVersion,
                       StringComparison.Ordinal);
        }

        private static string GetProperty(
            Excel.Workbook workbook,
            string propertyName)
        {
            try
            {
                dynamic properties = workbook.CustomDocumentProperties;
                dynamic property = properties[propertyName];
                object value = property.Value;
                return Convert.ToString(value);
            }
            catch
            {
                return null;
            }
        }

        private static void SetProperty(
            Excel.Workbook workbook,
            string propertyName,
            string value)
        {
            dynamic properties = workbook.CustomDocumentProperties;

            try
            {
                dynamic property = properties[propertyName];
                property.Value = value;
            }
            catch
            {
                properties.Add(
                    propertyName,
                    false,
                    OfficeStringPropertyType,
                    value);
            }
        }
    }

    internal static class JournalDateExceptionWarning
    {
        public static void Show(JournalImport journalImport)
        {
            if (journalImport == null)
                return;

            JournalRow[] exceptionalRows = journalImport.Rows
                .Where(row => row.DateExceptionResolution.HasValue)
                .ToArray();

            if (exceptionalRows.Length == 0)
                return;

            DateTime firstDate = exceptionalRows.Min(row => row.PostingDate);
            DateTime lastDate = exceptionalRows.Max(row => row.PostingDate);
            string language = SettingsService.NormalizeUiLanguage(
                SettingsService.Load().UiLanguage);

            bool slovak = string.Equals(
                language,
                SettingsService.SlovakUiLanguage,
                StringComparison.Ordinal);

            string title = slovak
                ? "Dátumy mimo účtovného roka"
                : "Dates outside fiscal year";

            string message = slovak
                ? exceptionalRows.Length.ToString("N0") +
                  " riadkov účtovného denníka obsahuje dátum mimo účtovného roka " +
                  journalImport.FiscalYear +
                  ".\r\n\r\nRiadky boli importované, ale automaticky vylúčené z výpočtu. " +
                  "Skontrolujte ich v hárku Accounting Journal. Pôvodné dátumy zostali zachované; " +
                  "audítor ich môže neskôr potvrdiť alebo zadať opravený dátum.\r\n\r\n" +
                  "Rozsah problémových dátumov: " +
                  firstDate.ToString("yyyy-MM-dd") + " – " +
                  lastDate.ToString("yyyy-MM-dd") + "."
                : exceptionalRows.Length.ToString("N0") +
                  " accounting-journal row(s) contain posting dates outside fiscal year " +
                  journalImport.FiscalYear +
                  ".\r\n\r\nThe rows were imported but automatically excluded from calculation. " +
                  "Review them in the Accounting Journal worksheet. The original dates are preserved; " +
                  "the auditor can later accept the original date or enter a corrected date.\r\n\r\n" +
                  "Problematic date range: " +
                  firstDate.ToString("yyyy-MM-dd") + " – " +
                  lastDate.ToString("yyyy-MM-dd") + ".";

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
