using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    internal sealed class MkSoftSourceFile
    {
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public AccountingSourceDocumentKind DocumentKind { get; private set; }
        public string Ico { get; private set; }
        public int FiscalYear { get; private set; }
        public int? ExportStage { get; private set; }

        public static MkSoftSourceFile ParseGeneralLedger(string filePath)
        {
            return Parse(
                filePath,
                AccountingSourceDocumentKind.GeneralLedger);
        }

        public static MkSoftSourceFile ParseAccountingJournal(string filePath)
        {
            return Parse(
                filePath,
                AccountingSourceDocumentKind.AccountingJournal);
        }

        private static MkSoftSourceFile Parse(
            string filePath,
            AccountingSourceDocumentKind expectedKind)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A MkSoft source file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The MkSoft source file was not found.",
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
                metadata.DocumentKind != expectedKind)
            {
                throw new InvalidDataException(
                    "The MkSoft filename '" + fileName +
                    "' is invalid. Expected '" +
                    ExpectedPattern(expectedKind) +
                    "'.");
            }

            return new MkSoftSourceFile
            {
                FilePath = Path.GetFullPath(filePath),
                FileName = fileName,
                DocumentKind = metadata.DocumentKind,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                ExportStage = metadata.ExportStage
            };
        }

        private static string ExpectedPattern(
            AccountingSourceDocumentKind kind)
        {
            return kind ==
                AccountingSourceDocumentKind.AccountingJournal
                ? "U_DENNIK_<IČO>_<YYYY>[<stage>].xls"
                : "HL_KNIHA_<IČO>_<YYYY>[<stage>].xls";
        }
    }
}
