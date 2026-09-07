using ExcelApiPoc.AccountingImport.Models;
using System;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services
{
    public static class AccountingJournalDetectionService
    {
        public static bool TryDetect(string filePath, out JournalDetectionResult result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            if (IfoSoftCsvJournalDetector.TryDetect(filePath, out result))
            {
                return true;
            }

            try
            {
                UrbisSourceFile sourceFile = UrbisSourceFile.Parse(filePath, UrbisDocumentKind.AccountingJournal);
                result = new JournalDetectionResult
                {
                    TechnicalType = "Excel",
                    AccountingFormat = "Urbis",
                    Ico = sourceFile.Ico,
                    FiscalYear = sourceFile.FiscalYear
                };

                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidDataException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}