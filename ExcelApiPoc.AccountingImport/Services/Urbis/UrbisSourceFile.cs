using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Urbis
{
    internal enum UrbisDocumentKind
    {
        AccountingJournal,
        GeneralLedger
    }

    internal sealed class UrbisSourceFile
    {
        public string FilePath { get; private set; }

        public string FileName { get; private set; }

        public UrbisDocumentKind DocumentKind { get; private set; }

        public string Ico { get; private set; }

        public int FiscalYear { get; private set; }

        public int? ExportStage { get; private set; }

        public string Extension { get; private set; }

        public static UrbisSourceFile Parse(
            string filePath,
            UrbisDocumentKind expectedDocumentKind)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "An Urbis source file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The Urbis source file was not found.",
                    filePath);
            }

            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath);

            if (!string.Equals(
                    extension,
                    ".xls",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    extension,
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The Urbis filename '" + fileName +
                    "' must use .xls or .xlsx format.");
            }

            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out AccountingFileNameMetadata metadata) ||
                (metadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingJournal &&
                 metadata.DocumentKind !=
                    AccountingSourceDocumentKind.GeneralLedger))
            {
                throw new InvalidDataException(
                    "The Urbis filename '" + fileName + "' is invalid. " +
                    "Expected 'U_DENNIK_<IČO>_<YYYY>[<stage>].xls[x]' or " +
                    "'HL_KNIHA_<IČO>_<YYYY>[<stage>].xls[x]'.");
            }

            UrbisDocumentKind detectedDocumentKind =
                metadata.DocumentKind ==
                    AccountingSourceDocumentKind.AccountingJournal
                    ? UrbisDocumentKind.AccountingJournal
                    : UrbisDocumentKind.GeneralLedger;

            if (detectedDocumentKind != expectedDocumentKind)
            {
                throw new InvalidDataException(
                    "The Urbis file '" + fileName + "' is a " +
                    Describe(detectedDocumentKind) +
                    ", but a " +
                    Describe(expectedDocumentKind) +
                    " was expected.");
            }

            return new UrbisSourceFile
            {
                FilePath = Path.GetFullPath(filePath),
                FileName = fileName,
                DocumentKind = detectedDocumentKind,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                ExportStage = metadata.ExportStage,
                Extension = extension.ToLowerInvariant()
            };
        }

        private static string Describe(UrbisDocumentKind documentKind)
        {
            return documentKind == UrbisDocumentKind.AccountingJournal
                ? "accounting journal"
                : "general ledger";
        }
    }
}
