using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Ives;
using ExcelApiPoc.AccountingImport.Services.MkSoft;
using ExcelApiPoc.AccountingImport.Services.SoftipMop;
using ExcelApiPoc.AccountingImport.Services.Urbis;
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

            if (TryDetectIves(filePath, out result))
            {
                return true;
            }

            if (TryDetectMkSoft(filePath, out result))
            {
                return true;
            }

            if (SoftipMopExcelJournalImporter.TryDetect(
                    filePath,
                    out int softipMopFiscalYear))
            {
                result = new JournalDetectionResult
                {
                    TechnicalType = "Excel",
                    AccountingFormat = "Softip-MOP",
                    FiscalYear = softipMopFiscalYear
                };

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

        private static bool TryDetectMkSoft(
            string filePath,
            out JournalDetectionResult result)
        {
            result = null;

            try
            {
                MkSoftSourceFile sourceFile =
                    MkSoftSourceFile.ParseAccountingJournal(filePath);

                MkSoftWorkbookInspection inspection =
                    MkSoftWorkbookInspector.Inspect(filePath);

                if (inspection.Kind !=
                    MkSoftWorkbookKind.AccountingJournal)
                {
                    return false;
                }

                result = new JournalDetectionResult
                {
                    TechnicalType = "Excel",
                    AccountingFormat = "MkSoft",
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

        private static bool TryDetectIves(
            string filePath,
            out JournalDetectionResult result)
        {
            result = null;

            if (!IvesJournalParserDispatcher.CanParse(filePath))
                return false;

            try
            {
                IIvesJournalSourceParser parser =
                    IvesJournalParserDispatcher.Select(filePath);
                IvesJournalParseResult source =
                    parser.Parse(filePath);

                result = new JournalDetectionResult
                {
                    TechnicalType = parser.TechnicalType,
                    AccountingFormat = "IVES",
                    Ico = source.Ico,
                    FiscalYear = source.FiscalYear
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
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
