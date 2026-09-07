using ExcelApiPoc.AccountingImport.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services
{
    public sealed class UrbisExcelGeneralLedgerImporter : IGeneralLedgerImporter
    {
        private const decimal AmountTolerance = 0.01m;

        public bool CanImport(
            string filePath,
            string accountingFormat)
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
                    UrbisDocumentKind.GeneralLedger);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public GeneralLedgerImport Import(string filePath)
        {
            UrbisSourceFile sourceFile = UrbisSourceFile.Parse(
                filePath,
                UrbisDocumentKind.GeneralLedger);

            if (sourceFile.ExportStage != 12)
            {
                throw new InvalidDataException(
                    "Urbis general ledger '" +
                    sourceFile.FileName +
                    "' has export stage " +
                    sourceFile.ExportStage +
                    ". The first Urbis importer version supports " +
                    "only stage 12 general ledgers.");
            }

            var result = new GeneralLedgerImport
            {
                SourceFileName = sourceFile.FileName,
                SourceFilePath = sourceFile.FilePath,
                SourceFileHash =
                    CalculateSha256(sourceFile.FilePath),
                TechnicalType = "Excel",
                AccountingFormat = "Urbis",
                Ico = sourceFile.Ico,
                FiscalYear = sourceFile.FiscalYear,
                ExportStage = sourceFile.ExportStage,
                ThroughMonth = 12,
                PeriodHeader =
                    "12/" +
                    sourceFile.FiscalYear.ToString(
                        CultureInfo.InvariantCulture),
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
                    "The Urbis general ledger does not contain " +
                    "any account rows.");
            }

            return result;
        }

        private static void ImportWorksheet(
            IExcelDataReader reader,
            string worksheetName,
            UrbisSourceFile sourceFile,
            GeneralLedgerImport result)
        {
            int sourceRowNumber = 0;

            if (!reader.Read())
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        1) +
                    ": the first header row is missing.");
            }

            sourceRowNumber++;
            ValidateFirstHeaderRow(
                reader,
                sourceFile,
                worksheetName,
                sourceRowNumber);

            if (!reader.Read())
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        2) +
                    ": the second header row is missing.");
            }

            sourceRowNumber++;
            ValidateSecondHeaderRow(
                reader,
                sourceFile,
                worksheetName,
                sourceRowNumber);

            var accountCodes =
                new HashSet<string>(StringComparer.Ordinal);

            int sequenceNumber = 0;

            while (reader.Read())
            {
                sourceRowNumber++;

                if (reader.FieldCount != 9)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": expected 9 columns, but found " +
                        reader.FieldCount + ".");
                }

                string sourceAccount =
                    ExcelWorkbookReader.GetUrbisText(reader, 0, sourceFile.Extension);
                string sourceName =
                    ExcelWorkbookReader.GetUrbisText(reader, 1, sourceFile.Extension);
                object openingValue = reader.GetValue(2);
                object debitTurnoverValue = reader.GetValue(3);
                object closingValue = reader.GetValue(4);
                object creditTurnoverValue = reader.GetValue(5);
                string tag =
                    ExcelWorkbookReader.GetUrbisText(reader, 6, sourceFile.Extension);

                string groupCode =
                ExcelWorkbookReader.GetUrbisText(reader, 7, sourceFile.Extension);

                string groupName =
                    ExcelWorkbookReader.GetUrbisText(reader, 8, sourceFile.Extension);


                if (IsEntireRowBlank(
                        sourceAccount,
                        sourceName,
                        openingValue,
                        debitTurnoverValue,
                        closingValue,
                        creditTurnoverValue,
                        tag,
                        groupCode,
                        groupName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sourceAccount))
                {
                    ValidateGroupOrSummaryRow(
                        sourceFile.FileName,
                        worksheetName,
                        sourceRowNumber,
                        sourceName,
                        tag,
                        groupCode);

                    continue;
                }

                if (!string.IsNullOrWhiteSpace(groupCode) ||
                    !string.IsNullOrWhiteSpace(groupName))
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": an account row also contains group " +
                        "summary columns.");
                }

                string accountCode =
                    AccountCodeNormalizer.Normalize(
                        sourceAccount.Trim());

                if (accountCode.Length < 3)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": account code '" +
                        accountCode +
                        "' contains fewer than three characters.");
                }

                if (!accountCodes.Add(accountCode))
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": duplicate account code '" +
                        accountCode + "'.");
                }

                decimal signedOpening = ParseAmount(
                    openingValue,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "opening balance");

                decimal debitTurnover = ParseAmount(
                    debitTurnoverValue,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "debit turnover");

                decimal signedClosing = ParseAmount(
                    closingValue,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "closing balance");

                decimal creditTurnover = ParseAmount(
                    creditTurnoverValue,
                    sourceFile.FileName,
                    worksheetName,
                    sourceRowNumber,
                    "credit turnover");

                decimal calculatedClosing =
                    signedOpening +
                    debitTurnover -
                    creditTurnover;

                if (Math.Abs(
                        calculatedClosing - signedClosing) >
                    AmountTolerance)
                {
                    throw new InvalidDataException(
                        Location(
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber) +
                        ": account '" +
                        accountCode +
                        "' has opening balance " +
                        FormatAmount(signedOpening) +
                        ", debit turnover " +
                        FormatAmount(debitTurnover) +
                        ", credit turnover " +
                        FormatAmount(creditTurnover) +
                        " and closing balance " +
                        FormatAmount(signedClosing) +
                        ". The calculated closing balance is " +
                        FormatAmount(calculatedClosing) + ".");
                }

                bool nameNormalized = false;
                string accountName =
                    NormalizeText(
                        sourceName,
                        result,
                        ref nameNormalized);

                string normalizedTag =
                    NormalizeText(
                        tag,
                        result,
                        ref nameNormalized);

                sequenceNumber++;

                result.Rows.Add(
                    new GeneralLedgerRow
                    {
                        SequenceNumber = sequenceNumber,
                        SourceRecordNumber = sourceRowNumber,
                        SyntheticCode =
                            accountCode.Substring(0, 3),
                        AnalyticalCode =
                            accountCode.Length > 3
                                ? accountCode.Substring(3)
                                : string.Empty,
                        AccountCode = accountCode,
                        Type = normalizedTag,
                        AccountName = accountName,

                        OpeningDebit =
                            signedOpening > 0m
                                ? signedOpening
                                : 0m,
                        OpeningCredit =
                            signedOpening < 0m
                                ? -signedOpening
                                : 0m,

                        AnnualDebitTurnover =
                            debitTurnover,
                        AnnualCreditTurnover =
                            creditTurnover,
                        PeriodDebitTurnover =
                            debitTurnover,
                        PeriodCreditTurnover =
                            creditTurnover,

                        ClosingDebit =
                            signedClosing > 0m
                                ? signedClosing
                                : 0m,
                        ClosingCredit =
                            signedClosing < 0m
                                ? -signedClosing
                                : 0m,

                        Plan = 0m
                    });
            }
        }

        private static void ValidateFirstHeaderRow(
            IExcelDataReader reader,
            UrbisSourceFile sourceFile,
            string worksheetName,
            int rowNumber)
        {
            ValidateFieldCount(
                reader,
                sourceFile.FileName,
                worksheetName,
                rowNumber);

            ValidateHeaderValue(reader, 0,
                "Účet",
                sourceFile,
                worksheetName,
                rowNumber);

            ValidateHeaderValue(
                reader,
                1,
                "Popis",
                sourceFile,
                worksheetName,
                rowNumber);

            ValidateHeaderValue(
                reader,
                2,
                "Počiatočný stav",
                sourceFile,
                worksheetName,
                rowNumber);

            string turnoverHeader = NormalizeSpaces(
                ExcelWorkbookReader.GetUrbisText(reader, 3, sourceFile.Extension));

            string expectedTurnoverHeader =
                "Obraty od 1. 1." +
                sourceFile.FiscalYear +
                " do 31.12." +
                sourceFile.FiscalYear;

            if (!string.Equals(
                    turnoverHeader,
                    expectedTurnoverHeader,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        rowNumber) +
                    ": expected turnover header '" +
                    expectedTurnoverHeader +
                    "', but found '" +
                    turnoverHeader + "'.");
            }

            ValidateHeaderValue(
                reader,
                4,
                "Konečný stav",
                sourceFile,
                worksheetName,
                rowNumber);
        }

        private static void ValidateSecondHeaderRow(
            IExcelDataReader reader,
            UrbisSourceFile sourceFile,
            string worksheetName,
            int rowNumber)
        {
            ValidateFieldCount(
                reader,
                sourceFile.FileName,
                worksheetName,
                rowNumber);

            string expectedOpeningDate =
                "k 1. 1." +
                sourceFile.FiscalYear;

            string actualOpeningDate = NormalizeSpaces(
                ExcelWorkbookReader.GetUrbisText(reader, 2, sourceFile.Extension));

            if (!string.Equals(
                    actualOpeningDate,
                    expectedOpeningDate,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        rowNumber) +
                    ": expected opening-balance header '" +
                    expectedOpeningDate +
                    "', but found '" +
                    actualOpeningDate + "'.");
            }

            ValidateHeaderValue(
                reader,
                3,
                "Má dať",
                sourceFile,
                worksheetName,
                rowNumber);

            string expectedClosingDate =
                "k 31.12." +
                sourceFile.FiscalYear;

            string actualClosingDate = NormalizeSpaces(
                ExcelWorkbookReader.GetUrbisText(reader, 4, sourceFile.Extension));


            if (!string.Equals(
                    actualClosingDate,
                    expectedClosingDate,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        rowNumber) +
                    ": expected closing-balance header '" +
                    expectedClosingDate +
                    "', but found '" +
                    actualClosingDate + "'.");
            }

            ValidateHeaderValue(
                reader,
                5,
                "Dal",
                sourceFile,
                worksheetName,
                rowNumber);
        }

        private static void ValidateFieldCount(
            IExcelDataReader reader,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            if (reader.FieldCount != 9)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        rowNumber) +
                    ": expected 9 columns, but found " +
                    reader.FieldCount + ".");
            }
        }

        private static void ValidateHeaderValue(
            IExcelDataReader reader,
            int columnIndex,
            string expected,
            UrbisSourceFile sourceFile,
            string worksheetName,
            int rowNumber)
        {
            string actual = NormalizeSpaces(
                ExcelWorkbookReader.GetUrbisText(
                    reader,
                    columnIndex,
                    sourceFile.Extension));

            if (!string.Equals(
                    actual,
                    expected,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    Location(
                        sourceFile.FileName,
                        worksheetName,
                        rowNumber) +
                    ": expected '" +
                    expected +
                    "' in column " +
                    (columnIndex + 1) +
                    ", but found '" +
                    actual + "'.");
            }
        }

        private static void ValidateGroupOrSummaryRow(
            string fileName,
            string worksheetName,
            int rowNumber,
            string accountName,
            string tag,
            string groupCode)
        {
            if (!string.IsNullOrWhiteSpace(accountName) ||
                !string.IsNullOrWhiteSpace(tag))
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        rowNumber) +
                    ": an unrecognized non-account row was found.");
            }

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        rowNumber) +
                    ": a non-account row has no group or summary label.");
            }

            // Synthetic totals and report summaries are intentionally
            // not imported as canonical account rows.
        }

        private static decimal ParseAmount(
            object value,
            string fileName,
            string worksheetName,
            int rowNumber,
            string description)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0m;
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
                    CultureInfo.InvariantCulture) ??
                string.Empty;

            normalized = normalized
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty)
                .Replace("*", string.Empty);

            if (normalized.Length == 0 ||
                normalized == "-")
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
                    Location(
                        fileName,
                        worksheetName,
                        rowNumber) +
                    ": '" +
                    normalized +
                    "' is not a valid Urbis " +
                    description + ".");
            }

            return result;
        }

        private static string NormalizeText(
            string value,
            GeneralLedgerImport result,
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

        private static string NormalizeSpaces(string value)
        {
            return Regex.Replace(
                (value ?? string.Empty).Trim(),
                @"\s+",
                " ");
        }

        private static bool IsEntireRowBlank(
            string account,
            string accountName,
            object opening,
            object debitTurnover,
            object closing,
            object creditTurnover,
            string tag,
            string groupCode,
            string groupName)
        {
            return
                string.IsNullOrWhiteSpace(account) &&
                string.IsNullOrWhiteSpace(accountName) &&
                IsBlank(opening) &&
                IsBlank(debitTurnover) &&
                IsBlank(closing) &&
                IsBlank(creditTurnover) &&
                string.IsNullOrWhiteSpace(tag) &&
                string.IsNullOrWhiteSpace(groupCode) &&
                string.IsNullOrWhiteSpace(groupName);
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

        private static string FormatAmount(decimal value)
        {
            return value.ToString(
                "0.00",
                CultureInfo.InvariantCulture);
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