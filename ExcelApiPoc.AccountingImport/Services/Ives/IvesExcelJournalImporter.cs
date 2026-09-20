using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    public sealed class IvesExcelJournalImporter : IJournalImporter
    {
        public const int MaximumJournalRows = 500_000;

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
                       "U_DENNIK_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public JournalImport Import(string filePath)
        {
            IvesJournalParseResult source =
                new IvesExcelJournalParser().Parse(filePath);
            ImportReport report = new IvesJournalValidator().Validate(source);

            if (!report.IsValid)
            {
                throw CreateValidationException(report);
            }

            if (source.TransactionRows.Count > MaximumJournalRows)
            {
                throw new InvalidDataException(
                    "The IVES accounting journal contains more than " +
                    MaximumJournalRows.ToString("N0") +
                    " transaction records. This file size is not supported.");
            }

            AccountingFileNameMetadata fileNameMetadata = null;
            if (AccountingFileNameMetadataParser.TryParse(
                    source.SourceFileName,
                    out AccountingFileNameMetadata parsedMetadata))
            {
                if (parsedMetadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingJournal)
                {
                    throw new InvalidDataException(
                        "Filename '" + source.SourceFileName +
                        "' does not identify an accounting journal.");
                }

                fileNameMetadata = parsedMetadata;
            }

            var result = new JournalImport
            {
                SourceFileName = source.SourceFileName,
                SourceFilePath = source.SourceFilePath,
                SourceFileHash = CalculateSha256(source.SourceFilePath),
                TechnicalType = "Excel",
                AccountingFormat = "IVES",
                Ico = AccountingFileNameMetadataParser.ResolveIco(
                    source.SourceFileName,
                    source.Ico,
                    fileNameMetadata),
                FiscalYear = AccountingFileNameMetadataParser.ResolveFiscalYear(
                    source.SourceFileName,
                    source.FiscalYear,
                    fileNameMetadata),
                ExportStage = fileNameMetadata?.ExportStage,
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = report
            };

            foreach (IvesJournalSourceRow sourceRow in source.TransactionRows)
            {
                result.Rows.Add(MapCanonicalRow(result, source, sourceRow));
            }

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting journal does not contain any " +
                    "transaction records.");
            }

            return result;
        }

        private static JournalRow MapCanonicalRow(
            JournalImport result,
            IvesJournalParseResult source,
            IvesJournalSourceRow sourceRow)
        {
            bool rowNormalized = false;
            string documentNumber = NormalizeText(
                sourceRow.DocumentNumber,
                result,
                ref rowNormalized);
            string description = NormalizeText(
                sourceRow.Text,
                result,
                ref rowNormalized);
            string debitAccount = NormalizeAccount(
                sourceRow.DebitCompositeAccount);
            string creditAccount = NormalizeAccount(
                sourceRow.CreditCompositeAccount);
            decimal amount = sourceRow.Amount.GetValueOrDefault();

            return new JournalRow
            {
                SequenceNumber = sourceRow.SequenceNumber,
                SourceRecordNumber = sourceRow.SourceRowNumber,
                SourceStartLineNumber = sourceRow.SourceRowNumber,
                SourceEndLineNumber = sourceRow.RelatedSourceRowNumber,
                SourceLocation = Location(source, sourceRow),
                TextNormalizationApplied = rowNormalized,
                DocumentType = null,
                DocumentNumber = documentNumber,
                PostingDate = sourceRow.PostingDate.Value,
                Description = description,
                RecordKind = JournalRecordKind.Normal,
                DebitAccount = debitAccount,
                DebitAmount = debitAccount == null ? (decimal?)null : amount,
                CreditAccount = creditAccount,
                CreditAmount = creditAccount == null ? (decimal?)null : amount
            };
        }

        private static string NormalizeAccount(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            string normalized = AccountCodeNormalizer.Normalize(value);
            return normalized.Length == 0 ? null : normalized;
        }

        private static string NormalizeText(
            string value,
            JournalImport result,
            ref bool rowNormalized)
        {
            string normalized = JournalTextNormalizer.NormalizeText(
                value,
                out bool changed);

            if (changed)
            {
                rowNormalized = true;
                result.NormalizedTextFieldCount++;
            }

            return normalized;
        }

        private static string Location(
            IvesJournalParseResult source,
            IvesJournalSourceRow row)
        {
            return source.SourceFileName + ", worksheet " +
                (source.WorksheetName ?? "(unnamed)") + ", rows " +
                row.SourceRowNumber + "-" +
                row.RelatedSourceRowNumber.GetValueOrDefault(
                    row.SourceRowNumber);
        }

        private static InvalidDataException CreateValidationException(
            ImportReport report)
        {
            foreach (ImportDiagnostic diagnostic in report.Diagnostics)
            {
                if (diagnostic.Severity == ImportDiagnosticSeverity.Error)
                {
                    return new InvalidDataException(
                        "IVES accounting-journal validation failed: " +
                        diagnostic.Message);
                }
            }

            return new InvalidDataException(
                "IVES accounting-journal validation failed.");
        }

        private static string CalculateSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
            }
        }
    }
}
