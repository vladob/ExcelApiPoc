using ExcelApiPoc.AccountingImport.Models;

namespace ExcelApiPoc.AccountingImport.Services
{
    public interface IJournalImporter
    {
        bool CanImport(string filePath,string accountingFormat);

        JournalImport Import(string filePath);
    }
}