using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    internal sealed class MkSoftSourceFile
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string Ico { get; private set; }
        public int FiscalYear { get; private set; }
        public int? ExportStage { get; private set; }

        public static MkSoftSourceFile ParseGeneralLedger(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A MkSoft general-ledger file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The MkSoft general-ledger file was not found.",
                    filePath);
            }

            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);

            if (!string.Equals(
                    extension,
                    ".xls",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The MkSoft filename '" + fileName +
                    "' must use the original .xls format.");
            }

            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out AccountingFileNameMetadata metadata) ||
                metadata.DocumentKind !=
                    AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "The MkSoft filename '" + fileName +
                    "' is invalid. Expected " +
                    "'HL_KNIHA_<IČO>_<YYYY>[<stage>].xls'.");
            }

            return new MkSoftSourceFile
            {
                FilePath = Path.GetFullPath(filePath),
                FileName = fileName,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                ExportStage = metadata.ExportStage
            };
        }
    }
}
