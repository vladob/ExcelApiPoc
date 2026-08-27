using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookWriter
    {
        public static Excel.Workbook CreateWorkbook(JournalImport journalImport)
        {
            Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook workbook = application.Workbooks.Add();

            try
            {
                Excel.Worksheet journalWorksheet = JournalWorksheetWriter.AddWorksheet(workbook, journalImport);
                ImportMetadataWorksheetWriter.AddWorksheet(workbook, journalImport);
                journalWorksheet.Activate();
                return workbook;
            }
            catch
            {
                workbook.Close( SaveChanges: false);
                throw;
            }
        }
    }
}