using ExcelApiPoc.AccountingImport.Models;

namespace ExcelApiPoc.AccountingImport.Services
{
    public interface IGeneralLedgerImporter
    {
        bool CanImport(
            string filePath,
            string accountingFormat);

        GeneralLedgerImport Import(string filePath);
    }
}