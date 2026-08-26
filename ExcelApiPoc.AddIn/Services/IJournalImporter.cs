using ExcelApiPoc.AddIn.Models;

namespace ExcelApiPoc.AddIn.Services
{
    internal interface IJournalImporter
    {
        bool CanImport(string filePath,string accountingFormat);

        JournalImport Import(string filePath);
    }
}