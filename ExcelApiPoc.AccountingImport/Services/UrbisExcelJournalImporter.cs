using ExcelApiPoc.AccountingImport.Models;
using ExcelDataReader;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services
{
    public sealed class UrbisExcelJournalImporter : IJournalImporter
    {
        public const int MaximumJournalRows = 500_000;

        private const decimal AmountTolerance = 0.01m;

        private static readonly string[] ExpectedNamedHeaders =
        {
            "Pč",
            "Dátum",
            "Poznámka",
            "Suma",
            "Účet MD",
            "Účet Dal"
        };

        public bool CanImport(string filePath, string accountingFormat)
        {
            if (!string.Equals(
                    accountingFormat,
                    "Urbis",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                UrbisSourceFile.Parse(
                    filePath,
                    UrbisDocumentKind.AccountingJournal);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public JournalImport Import(string filePath)
        {
            UrbisSourceFile sourceFile = UrbisSourceFile.Parse(
                filePath,
                UrbisDocumentKind.AccountingJournal);

            if (sourceFile.ExportStage != 12)
            {
                throw new InvalidDataException(
                    "Urbis accounting journal '" +
                    sourceFile.FileName +
                    "' has export stage " +
                    sourceFile.ExportStage +
                    ". The first Urbis importer version supports only " +
                    "stage 12 journals without opening and closing movements.");
            }

            var result = new JournalImport
            {
                SourceFileName = sourceFile.FileName,
                SourceFilePath = sourceFile.FilePath,
                SourceFileHash = CalculateSha256(sourceFile.FilePath),
                TechnicalType = "Excel",
                AccountingFormat = "Urbis",
                Ico = sourceFile.Ico,
                FiscalYear = sourceFile.FiscalYear,
                ExportStage = sourceFile.ExportStage,
                ImportedAtUtc = DateTime.UtcNow
            };

            using (IExcelDataReader reader =
                   ExcelWorkbookReader.Open(sourceFile.FilePath))
            {
                string worksheetName =
                    ExcelWorkbookReader.ValidateSingleWorksheet(
                        reader,
                        sourceFile.FileName);

                ImportWorksheet(
                    reader,
                    worksheetName,
                    sourceFile,
                    result);
            }

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The Urbis accounting journal does not contain " +
                    "any movement rows.");
            }

            return result;
        }

        private static void ImportWorksheet(
            IExcelDataReader reader,
            string worksheetName,
            UrbisSourceFile sourceFile,
            JournalImport result)
        {
            int sourceRowNumber = 0;

            if (!reader.Read())
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        1) +
                    ": the header row is missing.");
            }

            sourceRowNumber++;
            ValidateHeader(
                reader,
                sourceFile,
                worksheetName,
                sourceRowNumber);

            string currentDocumentId = null;
            decimal currentDocumentDebit = 0m;
            decimal currentDocumentCredit = 0m;
            decimal segmentDebit = 0m;
            decimal segmentCredit = 0m;
            bool lastSegmentTotalFound = false;
            int segmentTotalCount = 0;
            int sequenceNumber = 0;

            while (reader.Read())
            {
                sourceRowNumber++;

                if (reader.FieldCount != 7)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": expected 7 columns, but found " +
                        reader.FieldCount + ".");
                }

                string ordinalText =
                    ExcelWorkbookReader.GetUrbisText(reader, 0, sourceFile.Extension);
                object dateValue = reader.GetValue(1);
                string description =
                    ExcelWorkbookReader.GetUrbisText(reader, 2, sourceFile.Extension);
                object amountValue = reader.GetValue(3);
                string debitAccount =
                    ExcelWorkbookReader.GetUrbisText(reader, 4, sourceFile.Extension);

                string creditAccount =
                    ExcelWorkbookReader.GetUrbisText(reader, 5, sourceFile.Extension);

                string documentInformation =
                    ExcelWorkbookReader.GetUrbisText(reader, 6, sourceFile.Extension);


                if (IsEntireRowBlank(
                        ordinalText,
                        dateValue,
                        description,
                        amountValue,
                        debitAccount,
                        creditAccount,
                        documentInformation))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(documentInformation))
                {
                    ProcessStructuralRow(
                        sourceFile,
                        worksheetName,
                        sourceRowNumber,
                        ordinalText,
                        dateValue,
                        description,
                        amountValue,
                        debitAccount,
                        creditAccount,
                        documentInformation,
                        ref currentDocumentId,
                        ref currentDocumentDebit,
                        ref currentDocumentCredit,
                        ref segmentDebit,
                        ref segmentCredit,
                        ref lastSegmentTotalFound,
                        ref segmentTotalCount);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentDocumentId))
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": a movement row was found before a " +
                        "'Doklad:' row.");
                }

                int sourceOrdinal = ParseOrdinal(
                    ordinalText,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber);

                DateTime postingDate = ParseDate(
                    dateValue,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber);

                if (postingDate.Year != sourceFile.FiscalYear)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": posting date " +
                        postingDate.ToString(
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture) +
                        " does not belong to fiscal year " +
                        sourceFile.FiscalYear + ".");
                }

                decimal amount = ParseAmount(
                    amountValue,
                    false,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "movement amount");

                string normalizedDebitAccount =
                    NormalizeAccount(debitAccount);
                string normalizedCreditAccount =
                    NormalizeAccount(creditAccount);

                bool hasDebitAccount =
                    !string.IsNullOrWhiteSpace(normalizedDebitAccount);

                bool hasCreditAccount =
                    !string.IsNullOrWhiteSpace(normalizedCreditAccount);

                if (!hasDebitAccount && !hasCreditAccount)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": both debit and credit accounts are missing.");
                }

                bool rowNormalized = false;

                string normalizedDescription = NormalizeText(
                    description,
                    result,
                    ref rowNormalized);

                sequenceNumber++;

                if (sequenceNumber > MaximumJournalRows)
                {
                    throw new InvalidDataException(
                        "The Urbis accounting journal contains more than " +
                        MaximumJournalRows.ToString(
                            "N0",
                            CultureInfo.InvariantCulture) +
                        " movement rows. This file size is not supported.");
                }

                lastSegmentTotalFound = false;
                var row = new JournalRow
                {
                    SequenceNumber = sequenceNumber,
                    SourceRecordNumber = sourceOrdinal,
                    SourceStartLineNumber = sourceRowNumber,
                    SourceEndLineNumber = sourceRowNumber,
                    SourceLocation = Location(
                        sourceFile.FileName,
                        worksheetName,
                        sourceRowNumber),
                    DocumentType =
                        GetDocumentType(currentDocumentId),
                    DocumentNumber = currentDocumentId,
                    PostingDate = postingDate,
                    Description = normalizedDescription,
                    RecordKind = JournalRecordKind.Normal,

                    DebitAccount = hasDebitAccount ? normalizedDebitAccount : null,
                    DebitAmount = hasDebitAccount ? (decimal?)amount : null,
                    CreditAccount = hasCreditAccount ? normalizedCreditAccount : null,
                    CreditAmount = hasCreditAccount ? (decimal?)amount : null,

                    TextNormalizationApplied = rowNormalized
                };

                result.Rows.Add(row);

                if (hasDebitAccount)
                {
                    currentDocumentDebit += amount;
                    segmentDebit += amount;
                }

                if (hasCreditAccount)
                {
                    currentDocumentCredit += amount;
                    segmentCredit += amount;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentDocumentId))
            {
                throw new InvalidDataException(
                    "Urbis accounting journal '" +
                    sourceFile.FileName +
                    "' ended before the total row for document '" +
                    currentDocumentId + "'.");
            }

            if (segmentTotalCount == 0)
            {
                throw new InvalidDataException(
                    "Urbis accounting journal '" +
                    sourceFile.FileName +
                    "' does not contain any 'Spolu' row.");
            }

            if (!lastSegmentTotalFound)
            {
                throw new InvalidDataException(
                    "Urbis accounting journal '" +
                    sourceFile.FileName +
                    "' does not end with a validated 'Spolu' row.");
            }
        }

        private static void ProcessStructuralRow(
            UrbisSourceFile sourceFile,
            string worksheetName,
            int sourceRowNumber,
            string ordinalText,
            object dateValue,
            string description,
            object amountValue,
            string debitAccount,
            string creditAccount,
            string documentInformation,
            ref string currentDocumentId,
            ref decimal currentDocumentDebit,
            ref decimal currentDocumentCredit,
            ref decimal segmentDebit,
            ref decimal segmentCredit,
            ref bool lastSegmentTotalFound,
            ref int segmentTotalCount)
        {
            string information = documentInformation.Trim();
            string location = Location(
                sourceFile.FileName,
                worksheetName,
                sourceRowNumber);

            if (information.StartsWith(
                    "Doklad:",
                    StringComparison.OrdinalIgnoreCase))
            {
                ValidateBlank(
                    ordinalText,
                    "ordinal",
                    location);
                ValidateBlank(
                    dateValue,
                    "date",
                    location);
                ValidateBlank(
                    description,
                    "description",
                    location);
                ValidateBlank(
                    amountValue,
                    "amount",
                    location);
                ValidateBlank(
                    debitAccount,
                    "debit account",
                    location);
                ValidateBlank(
                    creditAccount,
                    "credit account",
                    location);

                if (!string.IsNullOrWhiteSpace(currentDocumentId))
                {
                    throw new InvalidDataException(
                        location +
                        ": document '" +
                        currentDocumentId +
                        "' has no 'Spolu za doklad' row.");
                }

                string documentId =
                    information.Substring("Doklad:".Length).Trim();

                if (documentId.Length == 0)
                {
                    throw new InvalidDataException(
                        location +
                        ": document identifier is missing.");
                }

                lastSegmentTotalFound = false;
                currentDocumentId = documentId;
                currentDocumentDebit = 0m;
                currentDocumentCredit = 0m;
                return;
            }

            const string DocumentTotalPrefix =
                "Spolu za doklad";

            if (information.StartsWith(
                    DocumentTotalPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                ValidateBlank(
                    ordinalText,
                    "ordinal",
                    location);
                ValidateBlank(
                    dateValue,
                    "date",
                    location);
                ValidateBlank(
                    description,
                    "description",
                    location);
                ValidateBlank(
                    amountValue,
                    "amount",
                    location);

                if (string.IsNullOrWhiteSpace(currentDocumentId))
                {
                    throw new InvalidDataException(
                        location +
                        ": a document total was found without a " +
                        "preceding 'Doklad:' row.");
                }

                string totalDocumentId =
                    information.Substring(
                        DocumentTotalPrefix.Length).Trim();

                if (!string.Equals(
                        totalDocumentId,
                        currentDocumentId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        location +
                        ": total belongs to document '" +
                        totalDocumentId +
                        "', but the current document is '" +
                        currentDocumentId + "'.");
                }

                decimal expectedDebit = ParseAmount(
                    debitAccount,
                    true,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "document debit total");

                decimal expectedCredit = ParseAmount(
                    creditAccount,
                    true,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "document credit total");

                ValidateTotal(
                    expectedDebit,
                    currentDocumentDebit,
                    "debit",
                    currentDocumentId,
                    location);

                ValidateTotal(
                    expectedCredit,
                    currentDocumentCredit,
                    "credit",
                    currentDocumentId,
                    location);

                currentDocumentId = null;
                currentDocumentDebit = 0m;
                currentDocumentCredit = 0m;
                return;
            }

            if (string.Equals(
                    information,
                    "Spolu",
                    StringComparison.OrdinalIgnoreCase))
            {
                ValidateBlank(
                    ordinalText,
                    "ordinal",
                    location);
                ValidateBlank(
                    dateValue,
                    "date",
                    location);
                ValidateBlank(
                    description,
                    "description",
                    location);
                ValidateBlank(
                    amountValue,
                    "amount",
                    location);

                if (!string.IsNullOrWhiteSpace(currentDocumentId))
                {
                    throw new InvalidDataException(
                        location +
                        ": final journal total was found before the " +
                        "total for document '" +
                        currentDocumentId + "'.");
                }

                decimal expectedDebit = ParseAmount(
                    debitAccount,
                    true,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "journal debit total");

                decimal expectedCredit = ParseAmount(
                    creditAccount,
                    true,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "journal credit total");

                ValidateGrandTotal(
                    expectedDebit,
                    segmentDebit,
                    "debit",
                    location);

                ValidateGrandTotal(
                    expectedCredit,
                    segmentCredit,
                    "credit",
                    location);

                segmentDebit = 0m;
                segmentCredit = 0m;
                lastSegmentTotalFound = true;
                segmentTotalCount++;

                return;

            }

            throw new InvalidDataException(
                location +
                ": unrecognized Urbis structural row '" +
                information + "'.");
        }

        private static void ValidateHeader(
            IExcelDataReader reader,
            UrbisSourceFile sourceFile,
            string worksheetName,
            int rowNumber)
        {
            string location = Location(
                sourceFile.FileName,
                worksheetName,
                rowNumber);

            if (reader.FieldCount != 7)
            {
                throw new InvalidDataException(
                    location +
                    ": expected 7 header columns, but found " +
                    reader.FieldCount + ".");
            }

            for (int index = 0;
                 index < ExpectedNamedHeaders.Length;
                 index++)
            {
                string actual =
                    ExcelWorkbookReader.GetUrbisText(reader, index, sourceFile.Extension);

                if (!string.Equals(
                        (actual ?? string.Empty).Trim(),
                        ExpectedNamedHeaders[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        location +
                        ": expected column '" +
                        ExpectedNamedHeaders[index] +
                        "' at position " +
                        (index + 1) +
                        ", but found '" +
                        (actual ?? string.Empty) +
                        "'.");
                }
            }

            // Urbis leaves the seventh header empty. Power Query may
            // materialize it as Column7, so its name is intentionally ignored.
        }

        private static int ParseOrdinal(
            string value,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            string normalized = (value ?? string.Empty).Trim();


            if (!int.TryParse(
                    normalized,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int result) ||
                result <= 0)
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, rowNumber) +
                    ": '" +
                    normalized +
                    "' is not a valid Urbis movement ordinal.");
            }

            return result;
        }

        private static DateTime ParseDate(
            object value,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            if (value is DateTime date)
            {
                return date.Date;
            }

            string normalized =
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture)?.Trim();

            string[] formats =
            {
                "d.M.yyyy",
                "d.MM.yyyy",
                "dd.M.yyyy",
                "dd.MM.yyyy",
                "yyyy-MM-dd"
            };


            if (!DateTime.TryParseExact(
                    normalized,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result))
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, rowNumber) +
                    ": '" +
                    normalized +
                    "' is not a valid Urbis posting date.");
            }

            return result;
        }

        private static decimal ParseAmount(
            object value,
            bool allowAsterisk,
            string fileName,
            string worksheetName,
            int rowNumber,
            string description)
        {
            if (value == null || value == DBNull.Value)
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, rowNumber) +
                    ": " + description + " is missing.");
            }

            if (value is decimal decimalValue)
            {
                return decimalValue;
            }

            if (value is double doubleValue)
            {
                return Convert.ToDecimal(doubleValue);
            }

            if (value is float floatValue)
            {
                return Convert.ToDecimal(floatValue);
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            string normalized =
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture) ?? string.Empty;

            normalized = normalized
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);

            if (allowAsterisk)
            {
                normalized = normalized.Replace("*", string.Empty);
            }

            if (allowAsterisk && normalized.Length == 0)
            {
                return 0m;
            }


            if (!decimal.TryParse(
                    normalized,
                    NumberStyles.AllowLeadingSign |
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, rowNumber) +
                    ": '" +
                    normalized +
                    "' is not a valid Urbis " +
                    description + ".");
            }

            return result;
        }

        private static string NormalizeAccount(string value)
        {
            return AccountCodeNormalizer.Normalize(
                (value ?? string.Empty).Trim());
        }

        private static string NormalizeText(
            string value,
            JournalImport result,
            ref bool rowNormalized)
        {

            string normalized =
                JournalTextNormalizer.NormalizeText(
                    value,
                    out bool changed);

            if (changed)
            {
                rowNormalized = true;
                result.NormalizedTextFieldCount++;
            }

            return normalized;
        }

        private static string GetDocumentType(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return null;
            }

            int separatorIndex = documentId.IndexOf('/');

            return separatorIndex > 0
                ? documentId.Substring(0, separatorIndex)
                : documentId;
        }

        private static void ValidateTotal(
            decimal expected,
            decimal actual,
            string side,
            string documentId,
            string location)
        {
            if (Math.Abs(expected - actual) <= AmountTolerance)
            {
                return;
            }

            throw new InvalidDataException(
                location +
                ": document '" +
                documentId +
                "' has " +
                side +
                " total " +
                expected.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                ", but its imported movements total " +
                actual.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                ".");
        }

        private static void ValidateGrandTotal(
            decimal expected,
            decimal actual,
            string side,
            string location)
        {
            if (Math.Abs(expected - actual) <= AmountTolerance)
            {
                return;
            }

            throw new InvalidDataException(
                location +
                ": journal segment " +
                side +
                " total is " +
                expected.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                ", but imported movements total " +
                actual.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) +
                ".");
        }

        private static void ValidateBlank(
            object value,
            string fieldName,
            string location)
        {
            if (value == null || value == DBNull.Value)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(
                    Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture)))
            {
                return;
            }

            throw new InvalidDataException(
                location +
                ": " +
                fieldName +
                " must be empty on an Urbis structural row.");
        }

        private static bool IsEntireRowBlank(
            string ordinal,
            object date,
            string description,
            object amount,
            string debitAccount,
            string creditAccount,
            string documentInformation)
        {
            return
                string.IsNullOrWhiteSpace(ordinal) &&
                IsBlank(date) &&
                string.IsNullOrWhiteSpace(description) &&
                IsBlank(amount) &&
                string.IsNullOrWhiteSpace(debitAccount) &&
                string.IsNullOrWhiteSpace(creditAccount) &&
                string.IsNullOrWhiteSpace(documentInformation);
        }

        private static bool IsBlank(object value)
        {
            return
                value == null ||
                value == DBNull.Value ||
                string.IsNullOrWhiteSpace(
                    Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture));
        }

        private static string Location(
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            return
                "File '" +
                fileName +
                "', worksheet '" +
                worksheetName +
                "', row " +
                rowNumber;
        }

        private static string CalculateSha256(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                return BitConverter
                    .ToString(sha256.ComputeHash(stream))
                    .Replace("-", string.Empty);
            }
        }
    }
}