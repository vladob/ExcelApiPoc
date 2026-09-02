using ExcelApiPoc.AddIn.Models;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookWriter
    {
        public static Excel.Workbook CreateWorkbook(
            Excel.Workbook workbook,
            JournalImport journalImport,
            IReadOnlyList<AccountSummary> accountSummaries,
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
            Excel.Worksheet journalWorksheet =
                JournalWorksheetWriter.AddWorksheet(workbook, journalImport);

            AccountWorksheetWriter.AddWorksheet(workbook, accountSummaries);

            if (accountingFrameworkImport != null)
                AccountingFrameworkWorksheetWriter.AddWorksheet(
                    workbook, accountingFrameworkImport);

            if (generalLedgerImport != null)
                GeneralLedgerWorksheetWriter.AddWorksheet(
                    workbook, generalLedgerImport);

            if (analyticalMapping != null && analyticalMapping.Rows.Count > 0)
            {
                AnalyticalMappingValidationWorksheetWriter.AddWorksheet(
                    workbook, analyticalMapping.Options);

                AnalyticalMappingWorksheetWriter.AddWorksheet(
                    workbook, analyticalMapping.Rows);
            }

            AuditCalculationPackageWorksheetWriter.AddWorksheet(
                workbook, templatePackage, reportContext, templatePackageLoad);

            RegisterUzReferenceWorksheetWriter.AddWorksheet(
                workbook, registerUzReportSelection);

            RegisterUzReportsWorksheetWriter.AddWorksheet(
                workbook, accountingEntityPackage);

            MultiYearBalanceSheetWorksheetWriter.AddWorksheet(
                workbook, accountingEntityPackage);

            RegisterUzAttachmentsWorksheetWriter.AddWorksheet(
                workbook, accountingEntityPackage);

            ImportMetadataWorksheetWriter.AddWorksheet(
                workbook,
                journalImport,
                frameworkLoad,
                templatePackage,
                reportContext,
                templatePackageLoad,
                accountingFrameworkImport,
                generalLedgerImport);

            journalWorksheet.Activate();
            return workbook;
        }
    }
}
