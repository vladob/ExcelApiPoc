using ExcelApiPoc.AccountingImport.Models;
using System;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    public sealed class IvesExcelGeneralLedgerImporter : IGeneralLedgerImporter
    {
        public bool CanImport(string filePath, string accountingFormat)
        {
            return string.Equals(
                       accountingFormat,
                       "IVES",
                       StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".xls",
                       StringComparison.OrdinalIgnoreCase) &&
                   Path.GetFileName(filePath).StartsWith(
                       "HL_KNIHA_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public GeneralLedgerImport Import(string filePath)
        {
            return new IvesGeneralLedgerImporter().Import(filePath);
        }
    }
}
