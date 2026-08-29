using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookWriter
    {

        public static Excel.Workbook CreateWorkbook(JournalImport journalImport, IReadOnlyList<AccountSummary> accountSummaries, 
            AccountFrameworkLoadResult frameworkLoad, AnalyticalMappingData analyticalMapping,
            AuditTemplatePackageResponse templatePackage, AuditReportContext reportContext, AuditTemplatePackageLoadResult templatePackageLoad)
        {
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook workbook = application.Workbooks.Add();

            try
            {
                Excel.Worksheet journalWorksheet = JournalWorksheetWriter.AddWorksheet(workbook, journalImport);

                AccountWorksheetWriter.AddWorksheet(workbook,accountSummaries);
                if (analyticalMapping != null && analyticalMapping.Rows.Count > 0)
                {
                    AnalyticalMappingValidationWorksheetWriter.AddWorksheet(workbook, analyticalMapping.Options);
                    AnalyticalMappingWorksheetWriter.AddWorksheet(workbook, analyticalMapping.Rows);
                }
                AuditCalculationPackageWorksheetWriter.AddWorksheet(workbook, templatePackage, reportContext, templatePackageLoad);

                ImportMetadataWorksheetWriter.AddWorksheet(
                    workbook,
                    journalImport,
                    frameworkLoad,
                    templatePackage,
                    reportContext,
                    templatePackageLoad);

                journalWorksheet.Activate();

                return workbook;
            }
            catch
            {
                workbook.Close(SaveChanges: false);
                throw;
            }
        }
    }
}
