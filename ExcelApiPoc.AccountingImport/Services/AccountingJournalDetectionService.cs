using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Layouts;
namespace ExcelApiPoc.AccountingImport.Services
{
    public static class AccountingJournalDetectionService
    {
        public static bool TryDetect(string filePath, out JournalDetectionResult result)
            => StagedImportRuntime.TryDetectJournal(filePath, out result);
    }
}
