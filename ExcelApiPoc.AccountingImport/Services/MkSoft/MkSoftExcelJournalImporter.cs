using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    public sealed class MkSoftExcelJournalImporter : IJournalImporter
    {
        public const int MaximumJournalRows = 500_000;

        private const decimal AmountTolerance = 0.01m;

        public bool CanImport(
            string filePath,
            string accountingFormat)
        {
            if (!string.Equals(
                    accountingFormat,
                    "MkSoft",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                MkSoftSourceFile.ParseAccountingJournal(filePath);

                MkSoftWorkbookInspection inspection =
                    MkSoftWorkbookInspector.Inspect(filePath);

                return inspection.Kind ==
                    MkSoftWorkbookKind.AccountingJournal;
            }
            catch
            {
                return false;
            }
        }

        public JournalImport Import(string filePath)
        {
            MkSoftSourceFile sourceFile =
                MkSoftSourceFile.ParseAccountingJournal(filePath);

            var result = new JournalImport
            {
                SourceFileName = sourceFile.FileName,
                SourceFilePath = sourceFile.FilePath,
                SourceFileHash = CalculateSha256(sourceFile.FilePath),
                TechnicalType = "Excel",
                AccountingFormat = "MkSoft",
                Ico = sourceFile.Ico,
                FiscalYear = sourceFile.FiscalYear,
                ExportStage = sourceFile.ExportStage,
                ImportedAtUtc = DateTime.UtcNow
            };

            using (IExcelDataReader reader =
                   ExcelWorkbookReader.Open(sourceFile.FilePath))
            {
                if (reader.ResultsCount != 1)
                {
                    throw new InvalidDataException(
                        "MkSoft workbook '" + sourceFile.FileName +
                        "' must contain exactly one worksheet; found " +
                        reader.ResultsCount + ".");
                }

                string worksheetName = reader.Name;

                if (!reader.Read())
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            1) +
                        ": the header row is missing.");
                }

                Dictionary<string, int> columns =
                    ReadAndValidateHeader(
                        reader,
                        sourceFile.FileName,
                        worksheetName);

                ReadRows(
                    reader,
                    columns,
                    worksheetName,
                    sourceFile,
                    result);
            }

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The MkSoft accounting journal does not contain " +
                    "any movement rows.");
            }

            return result;
        }

        private static void ReadRows(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string worksheetName,
            MkSoftSourceFile sourceFile,
            JournalImport result)
        {
            var movementIds =
                new HashSet<string>(StringComparer.Ordinal);
            var documents =
                new Dictionary<string, DocumentAggregate>(
                    StringComparer.Ordinal);

            int sourceRowNumber = 1;
            int sequenceNumber = 0;

            while (reader.Read())
            {
                sourceRowNumber++;

                if (IsBlankRow(reader))
                    continue;

                if (reader.FieldCount !=
                    MkSoftWorkbookInspector.AccountingJournalFieldCount)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": expected " +
                        MkSoftWorkbookInspector.AccountingJournalFieldCount +
                        " columns, but found " +
                        reader.FieldCount + ".");
                }

                sequenceNumber++;

                if (sequenceNumber > MaximumJournalRows)
                {
                    throw new InvalidDataException(
                        "The MkSoft accounting journal contains more than " +
                        MaximumJournalRows.ToString(
                            "N0",
                            CultureInfo.InvariantCulture) +
                        " movement rows. This file size is not supported.");
                }

                MkSoftJournalSourceRow source =
                    ReadSourceRow(
                        reader,
                        columns,
                        sourceFile.FileName,
                        worksheetName,
                        sourceRowNumber);

                ValidateIdentity(
                    source,
                    movementIds,
                    documents,
                    sourceFile.FileName,
                    worksheetName);

                JournalRow canonical =
                    MapCanonicalRow(
                        source,
                        sourceFile.FileName,
                        worksheetName,
                        sequenceNumber,
                        result);

                result.Rows.Add(canonical);

                DocumentAggregate document =
                    documents[source.DocumentId];

                document.Debit += source.DebitAmount ?? 0m;
                document.Credit += source.CreditAmount ?? 0m;
            }

            foreach (DocumentAggregate document in documents.Values)
            {
                if (Math.Abs(
                        document.Debit -
                        document.Credit) >
                    AmountTolerance)
                {
                    throw new InvalidDataException(
                        "MkSoft document '" +
                        document.DocumentId +
                        "' is not balanced. Debit " +
                        document.Debit.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture) +
                        ", credit " +
                        document.Credit.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture) +
                        ".");
                }
            }
        }

        private static MkSoftJournalSourceRow ReadSourceRow(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string fileName,
            string worksheetName,
            int sourceRowNumber)
        {
            return new MkSoftJournalSourceRow
            {
                SourceRowNumber = sourceRowNumber,

                DocumentId = Text(reader, columns, "id"),
                DocumentSequence = Text(reader, columns, "pc"),
                DocumentType = Text(reader, columns, "typ"),
                DocumentNumber = Text(reader, columns, "x_doklad"),
                PostingDate = Date(
                    reader,
                    columns,
                    "datum",
                    fileName,
                    worksheetName,
                    sourceRowNumber),
                EvidenceDate = Date(
                    reader,
                    columns,
                    "datumpokl",
                    fileName,
                    worksheetName,
                    sourceRowNumber),
                Description = Text(reader, columns, "text"),
                CompanyId = Text(reader, columns, "firmaid"),
                SpecialCode = Text(reader, columns, "specialkod"),

                MovementId = Text(reader, columns, "id2"),
                MovementDocumentId = Text(reader, columns, "dokladid"),
                MovementSequence = Text(reader, columns, "pc1"),
                DebitAccount = Text(reader, columns, "ucetm"),
                CreditAccount = Text(reader, columns, "ucetd"),
                DebitAmount1 = NullableAmount(
                    reader, columns, "ciastka1m",
                    fileName, worksheetName, sourceRowNumber),
                CreditAmount1 = NullableAmount(
                    reader, columns, "ciastka1d",
                    fileName, worksheetName, sourceRowNumber),
                DebitAmount2 = NullableAmount(
                    reader, columns, "ciastka2m",
                    fileName, worksheetName, sourceRowNumber),
                CreditAmount2 = NullableAmount(
                    reader, columns, "ciastka2d",
                    fileName, worksheetName, sourceRowNumber),
                DebitAmount = NullableAmount(
                    reader, columns, "ciastkam",
                    fileName, worksheetName, sourceRowNumber),
                CreditAmount = NullableAmount(
                    reader, columns, "ciastkad",
                    fileName, worksheetName, sourceRowNumber),
                DebitForeignAmount = NullableAmount(
                    reader, columns, "ciastkacmm",
                    fileName, worksheetName, sourceRowNumber),
                CreditForeignAmount = NullableAmount(
                    reader, columns, "ciastkacmd",
                    fileName, worksheetName, sourceRowNumber),
                DebitCurrency = Text(reader, columns, "menam"),
                CreditCurrency = Text(reader, columns, "menad"),
                DebitCostCenter = Text(reader, columns, "strediskom"),
                CreditCostCenter = Text(reader, columns, "strediskod"),
                DebitOrder = Text(reader, columns, "zakazkam"),
                CreditOrder = Text(reader, columns, "zakazkad"),
                DebitTurnoverCode =
                    Text(reader, columns, "kodobratum"),
                CreditTurnoverCode =
                    Text(reader, columns, "kodobratud"),
                MovementDescription =
                    Text(reader, columns, "popis"),
                AutoGenerated =
                    Text(reader, columns, "autogen")
            };
        }

        private static JournalRow MapCanonicalRow(
            MkSoftJournalSourceRow source,
            string fileName,
            string worksheetName,
            int sequenceNumber,
            JournalImport result)
        {
            if (!source.PostingDate.HasValue)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        source.SourceRowNumber) +
                    ": posting date is empty.");
            }

            string debitAccount =
                NormalizeAccount(source.DebitAccount);
            string creditAccount =
                NormalizeAccount(source.CreditAccount);

            if (debitAccount == null &&
                creditAccount == null)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        source.SourceRowNumber) +
                    ": both debit and credit account are empty.");
            }

            bool rowNormalized = false;

            string documentType =
                NormalizeText(
                    source.DocumentType,
                    result,
                    ref rowNormalized);

            string documentNumber =
                NormalizeText(
                    source.DocumentNumber,
                    result,
                    ref rowNormalized);

            string description =
                NormalizeText(
                    source.Description,
                    result,
                    ref rowNormalized);

            string debitCostCenter =
                NormalizeText(
                    source.DebitCostCenter,
                    result,
                    ref rowNormalized);

            string creditCostCenter =
                NormalizeText(
                    source.CreditCostCenter,
                    result,
                    ref rowNormalized);

            string debitOrder =
                NormalizeText(
                    source.DebitOrder,
                    result,
                    ref rowNormalized);

            string creditOrder =
                NormalizeText(
                    source.CreditOrder,
                    result,
                    ref rowNormalized);

            return new JournalRow
            {
                SequenceNumber = sequenceNumber,
                SourceRecordNumber = source.SourceRowNumber,
                SourceStartLineNumber = source.SourceRowNumber,
                SourceEndLineNumber = source.SourceRowNumber,
                SourceLocation =
                    Location(
                        fileName,
                        worksheetName,
                        source.SourceRowNumber),
                TextNormalizationApplied = rowNormalized,
                DocumentType = documentType,
                DocumentNumber = documentNumber,
                PostingDate = source.PostingDate.Value,
                Description = description,
                RecordKind = Classify(source.SpecialCode),
                DebitAccount = debitAccount,
                DebitAmount = source.DebitAmount,
                DebitCostCenter = debitCostCenter,
                DebitOrder = debitOrder,
                CreditAccount = creditAccount,
                CreditAmount = source.CreditAmount,
                CreditCostCenter = creditCostCenter,
                CreditOrder = creditOrder
            };
        }

        private static void ValidateIdentity(
            MkSoftJournalSourceRow source,
            ISet<string> movementIds,
            IDictionary<string, DocumentAggregate> documents,
            string fileName,
            string worksheetName)
        {
            string location =
                Location(
                    fileName,
                    worksheetName,
                    source.SourceRowNumber);

            if (string.IsNullOrWhiteSpace(source.DocumentId))
            {
                throw new InvalidDataException(
                    location + ": document id is empty.");
            }

            if (string.IsNullOrWhiteSpace(source.MovementId))
            {
                throw new InvalidDataException(
                    location + ": movement id is empty.");
            }

            if (!movementIds.Add(source.MovementId))
            {
                throw new InvalidDataException(
                    location +
                    ": duplicate movement id '" +
                    source.MovementId + "'.");
            }

            if (!string.Equals(
                    source.DocumentId,
                    source.MovementDocumentId,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    location +
                    ": movement document id '" +
                    source.MovementDocumentId +
                    "' does not match document id '" +
                    source.DocumentId + "'.");
            }

            if (!documents.TryGetValue(
                    source.DocumentId,
                    out DocumentAggregate document))
            {
                documents.Add(
                    source.DocumentId,
                    new DocumentAggregate
                    {
                        DocumentId = source.DocumentId,
                        DocumentSequence = source.DocumentSequence,
                        DocumentType = source.DocumentType,
                        DocumentNumber = source.DocumentNumber,
                        PostingDate = source.PostingDate,
                        EvidenceDate = source.EvidenceDate,
                        Description = source.Description,
                        CompanyId = source.CompanyId,
                        SpecialCode = source.SpecialCode
                    });

                return;
            }

            ValidateRepeated(
                location,
                "document sequence",
                document.DocumentSequence,
                source.DocumentSequence);

            ValidateRepeated(
                location,
                "document type",
                document.DocumentType,
                source.DocumentType);

            ValidateRepeated(
                location,
                "document number",
                document.DocumentNumber,
                source.DocumentNumber);

            ValidateRepeated(
                location,
                "posting date",
                document.PostingDate,
                source.PostingDate);

            ValidateRepeated(
                location,
                "evidence date",
                document.EvidenceDate,
                source.EvidenceDate);

            ValidateRepeated(
                location,
                "document description",
                document.Description,
                source.Description);

            ValidateRepeated(
                location,
                "company id",
                document.CompanyId,
                source.CompanyId);

            ValidateRepeated(
                location,
                "special code",
                document.SpecialCode,
                source.SpecialCode);
        }

        private static void ValidateRepeated(
            string location,
            string field,
            object expected,
            object actual)
        {
            if (Equals(expected, actual))
                return;

            throw new InvalidDataException(
                location +
                ": repeated " +
                field +
                " differs within the same MkSoft document.");
        }

        private static JournalRecordKind Classify(string specialCode)
        {
            if (string.Equals(
                    specialCode,
                    "PS",
                    StringComparison.OrdinalIgnoreCase))
            {
                return JournalRecordKind.Opening;
            }

            if (string.Equals(
                    specialCode,
                    "UZ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return JournalRecordKind.Closing;
            }

            return JournalRecordKind.Normal;
        }

        private static string NormalizeAccount(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized =
                AccountCodeNormalizer.Normalize(value);

            return normalized.Length == 0
                ? null
                : normalized;
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

        private static Dictionary<string, int> ReadAndValidateHeader(
            IExcelDataReader reader,
            string fileName,
            string worksheetName)
        {
            var headers = new string[reader.FieldCount];
            var columns =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            for (int column = 0; column < reader.FieldCount; column++)
            {
                string header =
                    (ExcelWorkbookReader.GetText(reader, column) ??
                     string.Empty)
                    .Trim();

                headers[column] = header;

                if (header.Length == 0)
                    continue;

                if (columns.ContainsKey(header))
                {
                    throw new InvalidDataException(
                        Location(fileName, worksheetName, 1) +
                        ": duplicate header '" + header + "'.");
                }

                columns.Add(header, column);
            }

            if (MkSoftWorkbookInspector.ClassifyHeaders(headers) !=
                MkSoftWorkbookKind.AccountingJournal)
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, 1) +
                    ": the 251-column header does not match the " +
                    "MkSoft accounting-journal signature.");
            }

            return columns;
        }

        private static string Text(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string name)
        {
            return ExcelWorkbookReader.GetText(
                    reader,
                    columns[name])
                ?.Trim();
        }

        private static decimal? NullableAmount(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string name,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            object value = reader.GetValue(columns[name]);

            if (value == null || value == DBNull.Value)
                return null;

            if (value is decimal decimalValue)
                return decimalValue;

            if (value is double doubleValue)
                return Convert.ToDecimal(doubleValue);

            if (value is float floatValue)
                return Convert.ToDecimal(floatValue);

            if (value is int intValue)
                return intValue;

            if (value is long longValue)
                return longValue;

            string text =
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture)
                ?.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (!decimal.TryParse(
                    text,
                    NumberStyles.Number |
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal amount))
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        rowNumber) +
                    ": field '" + name +
                    "' contains invalid amount '" +
                    text + "'.");
            }

            return amount;
        }

        private static DateTime? Date(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string name,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            object value = reader.GetValue(columns[name]);

            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime date)
                return date.Date;

            if (value is double serial)
            {
                try
                {
                    return DateTime.FromOADate(serial).Date;
                }
                catch (ArgumentException)
                {
                }
            }

            string text =
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture)
                ?.Trim();

            if (DateTime.TryParse(
                    text,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                Location(
                    fileName,
                    worksheetName,
                    rowNumber) +
                ": field '" + name +
                "' contains invalid date '" +
                text + "'.");
        }

        private static bool IsBlankRow(IExcelDataReader reader)
        {
            for (int column = 0; column < reader.FieldCount; column++)
            {
                object value = reader.GetValue(column);

                if (value == null || value == DBNull.Value)
                    continue;

                if (value is string text)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                        return false;

                    continue;
                }

                return false;
            }

            return true;
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

        private sealed class DocumentAggregate
        {
            public string DocumentId;
            public string DocumentSequence;
            public string DocumentType;
            public string DocumentNumber;
            public DateTime? PostingDate;
            public DateTime? EvidenceDate;
            public string Description;
            public string CompanyId;
            public string SpecialCode;
            public decimal Debit;
            public decimal Credit;
        }
    }
}
