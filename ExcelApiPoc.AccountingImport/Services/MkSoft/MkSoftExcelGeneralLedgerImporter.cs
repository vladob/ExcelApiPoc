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
    public sealed class MkSoftExcelGeneralLedgerImporter :
        IGeneralLedgerImporter
    {
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
                MkSoftSourceFile.ParseGeneralLedger(filePath);

                MkSoftWorkbookInspection inspection =
                    MkSoftWorkbookInspector.Inspect(filePath);

                return inspection.Kind ==
                    MkSoftWorkbookKind.GeneralLedger;
            }
            catch
            {
                return false;
            }
        }

        public GeneralLedgerImport Import(string filePath)
        {
            MkSoftSourceFile sourceFile =
                MkSoftSourceFile.ParseGeneralLedger(filePath);

            var sourceRows = new List<MkSoftGeneralLedgerSourceRow>();
            DateTime? periodFrom = null;
            DateTime? periodTo = null;

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

                int sourceRowNumber = 1;

                while (reader.Read())
                {
                    sourceRowNumber++;

                    if (IsBlankRow(reader))
                        continue;

                    if (reader.FieldCount !=
                        MkSoftWorkbookInspector.GeneralLedgerFieldCount)
                    {
                        throw new InvalidDataException(
                            Location(
                                sourceFile.FileName,
                                worksheetName,
                                sourceRowNumber) +
                            ": expected " +
                            MkSoftWorkbookInspector.GeneralLedgerFieldCount +
                            " columns, but found " +
                            reader.FieldCount + ".");
                    }

                    MkSoftGeneralLedgerSourceRow row =
                        ReadSourceRow(
                            reader,
                            columns,
                            sourceFile.FileName,
                            worksheetName,
                            sourceRowNumber);

                    if (string.IsNullOrWhiteSpace(row.Account))
                    {
                        throw new InvalidDataException(
                            Location(
                                sourceFile.FileName,
                                worksheetName,
                                sourceRowNumber) +
                            ": account code is empty.");
                    }

                    ValidatePeriod(
                        row,
                        sourceFile,
                        sourceFile.FileName,
                        worksheetName,
                        sourceRowNumber);

                    if (!periodFrom.HasValue)
                    {
                        periodFrom = row.PeriodFrom;
                        periodTo = row.PeriodTo;
                    }
                    else if (periodFrom != row.PeriodFrom ||
                             periodTo != row.PeriodTo)
                    {
                        throw new InvalidDataException(
                            Location(
                                sourceFile.FileName,
                                worksheetName,
                                sourceRowNumber) +
                            ": period differs from preceding MkSoft rows.");
                    }

                    ValidateGrossClosing(
                        row,
                        sourceFile.FileName,
                        worksheetName);

                    sourceRows.Add(row);
                }
            }

            if (sourceRows.Count == 0)
            {
                throw new InvalidDataException(
                    "The MkSoft general ledger does not contain " +
                    "any account rows.");
            }

            return MapCanonical(
                sourceFile,
                sourceRows,
                periodTo);
        }

        private static GeneralLedgerImport MapCanonical(
            MkSoftSourceFile sourceFile,
            IReadOnlyList<MkSoftGeneralLedgerSourceRow> sourceRows,
            DateTime? periodTo)
        {
            int throughMonth = periodTo?.Month ?? 12;

            var result = new GeneralLedgerImport
            {
                SourceFileName = sourceFile.FileName,
                SourceFilePath = sourceFile.FilePath,
                SourceFileHash = CalculateSha256(sourceFile.FilePath),
                TechnicalType = "Excel",
                AccountingFormat = "MkSoft",
                Ico = sourceFile.Ico,
                FiscalYear = sourceFile.FiscalYear,
                ExportStage = sourceFile.ExportStage,
                ThroughMonth = throughMonth,
                PeriodHeader =
                    throughMonth.ToString(
                        CultureInfo.InvariantCulture) +
                    "/" +
                    sourceFile.FiscalYear.ToString(
                        CultureInfo.InvariantCulture),
                ImportedAtUtc = DateTime.UtcNow
            };

            var accountCodes =
                new HashSet<string>(StringComparer.Ordinal);

            int sequence = 0;

            foreach (MkSoftGeneralLedgerSourceRow source in sourceRows)
            {
                string accountCode =
                    AccountCodeNormalizer.Normalize(source.Account);

                if (accountCode.Length < 3)
                {
                    throw new InvalidDataException(
                        "MkSoft account '" + accountCode +
                        "' contains fewer than three characters.");
                }

                if (!accountCodes.Add(accountCode))
                {
                    throw new InvalidDataException(
                        "MkSoft general ledger contains duplicate " +
                        "account code '" + accountCode + "'.");
                }

                string accountName =
                    JournalTextNormalizer.NormalizeText(
                        source.AccountName,
                        out bool nameChanged);

                if (nameChanged)
                    result.NormalizedTextFieldCount++;

                string costCenter =
                    JournalTextNormalizer.NormalizeText(
                        source.CostCenter,
                        out bool costCenterChanged);

                if (costCenterChanged)
                    result.NormalizedTextFieldCount++;

                string order =
                    JournalTextNormalizer.NormalizeText(
                        source.Order,
                        out bool orderChanged);

                if (orderChanged)
                    result.NormalizedTextFieldCount++;

                decimal closingBalance =
                    source.GrossClosingDebit -
                    source.GrossClosingCredit;

                sequence++;

                result.Rows.Add(
                    new GeneralLedgerRow
                    {
                        SequenceNumber = sequence,
                        SourceRecordNumber = source.SourceRowNumber,
                        SyntheticCode =
                            accountCode.Substring(0, 3),
                        AnalyticalCode =
                            accountCode.Length > 3
                                ? accountCode.Substring(3)
                                : string.Empty,
                        AccountCode = accountCode,
                        AccountName = accountName,
                        CostCenter = costCenter,
                        Order = order,
                        OpeningDebit = source.OpeningDebit,
                        OpeningCredit = source.OpeningCredit,
                        AnnualDebitTurnover =
                            source.DebitTurnover,
                        AnnualCreditTurnover =
                            source.CreditTurnover,
                        // The supplied whole-year MkSoft sample contains a
                        // second turnover pair with identical values. Its
                        // business distinction is not established yet, so
                        // canonical period turnover uses the primary pair.
                        // The secondary pair remains preserved on the
                        // MkSoft source row for later interpretation.
                        PeriodDebitTurnover =
                            source.DebitTurnover,
                        PeriodCreditTurnover =
                            source.CreditTurnover,
                        ClosingDebit =
                            closingBalance > 0m
                                ? closingBalance
                                : 0m,
                        ClosingCredit =
                            closingBalance < 0m
                                ? -closingBalance
                                : 0m,
                        Plan = 0m
                    });
            }

            return result;
        }

        private static MkSoftGeneralLedgerSourceRow ReadSourceRow(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string fileName,
            string worksheetName,
            int sourceRowNumber)
        {
            return new MkSoftGeneralLedgerSourceRow
            {
                SourceRowNumber = sourceRowNumber,
                Id = Text(reader, columns, "id"),
                PeriodFrom = Date(
                    reader,
                    columns,
                    "datumod",
                    fileName,
                    worksheetName,
                    sourceRowNumber),
                PeriodTo = Date(
                    reader,
                    columns,
                    "datumdo",
                    fileName,
                    worksheetName,
                    sourceRowNumber),
                CostCenter = Text(reader, columns, "stredisko"),
                Order = Text(reader, columns, "zakazka"),
                TurnoverCode = Text(reader, columns, "kodobratu"),
                Account = Text(reader, columns, "ucet"),
                AccountName = Text(reader, columns, "nazov"),
                SyntheticAccount = Text(reader, columns, "ucet3"),
                SyntheticAccountName = Text(reader, columns, "nazov3"),
                AccountClass = Text(reader, columns, "ucet1"),
                AccountClassName = Text(reader, columns, "nazov1"),
                Currency = Text(reader, columns, "mena"),
                OpeningDebit = Amount(
                    reader, columns, "pocstavm",
                    fileName, worksheetName, sourceRowNumber),
                OpeningCredit = Amount(
                    reader, columns, "pocstavd",
                    fileName, worksheetName, sourceRowNumber),
                DebitTurnover = Amount(
                    reader, columns, "obratym",
                    fileName, worksheetName, sourceRowNumber),
                CreditTurnover = Amount(
                    reader, columns, "obratyd",
                    fileName, worksheetName, sourceRowNumber),
                SecondaryDebitTurnover = Amount(
                    reader, columns, "obratysm",
                    fileName, worksheetName, sourceRowNumber),
                SecondaryCreditTurnover = Amount(
                    reader, columns, "obratysd",
                    fileName, worksheetName, sourceRowNumber),
                GrossClosingDebit = Amount(
                    reader, columns, "zostatokm",
                    fileName, worksheetName, sourceRowNumber),
                GrossClosingCredit = Amount(
                    reader, columns, "zostatokd",
                    fileName, worksheetName, sourceRowNumber)
            };
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

                if (header.Length > 0)
                {
                    if (columns.ContainsKey(header))
                    {
                        throw new InvalidDataException(
                            Location(fileName, worksheetName, 1) +
                            ": duplicate header '" + header + "'.");
                    }

                    columns.Add(header, column);
                }
            }

            if (MkSoftWorkbookInspector.ClassifyHeaders(headers) !=
                MkSoftWorkbookKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    Location(fileName, worksheetName, 1) +
                    ": the 41-column header does not match the " +
                    "MkSoft general-ledger signature.");
            }

            return columns;
        }

        private static void ValidatePeriod(
            MkSoftGeneralLedgerSourceRow row,
            MkSoftSourceFile sourceFile,
            string fileName,
            string worksheetName,
            int sourceRowNumber)
        {
            if (!row.PeriodFrom.HasValue ||
                !row.PeriodTo.HasValue)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        sourceRowNumber) +
                    ": datumod and datumdo are required.");
            }

            if (row.PeriodFrom.Value.Year != sourceFile.FiscalYear ||
                row.PeriodTo.Value.Year != sourceFile.FiscalYear ||
                row.PeriodFrom.Value > row.PeriodTo.Value)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        sourceRowNumber) +
                    ": period " +
                    row.PeriodFrom.Value.ToString("yyyy-MM-dd") +
                    " to " +
                    row.PeriodTo.Value.ToString("yyyy-MM-dd") +
                    " is inconsistent with fiscal year " +
                    sourceFile.FiscalYear + ".");
            }
        }

        private static void ValidateGrossClosing(
            MkSoftGeneralLedgerSourceRow row,
            string fileName,
            string worksheetName)
        {
            decimal expectedDebit =
                row.OpeningDebit + row.DebitTurnover;
            decimal expectedCredit =
                row.OpeningCredit + row.CreditTurnover;

            if (Math.Abs(
                    expectedDebit -
                    row.GrossClosingDebit) >
                AmountTolerance ||
                Math.Abs(
                    expectedCredit -
                    row.GrossClosingCredit) >
                AmountTolerance)
            {
                throw new InvalidDataException(
                    Location(
                        fileName,
                        worksheetName,
                        row.SourceRowNumber) +
                    ": account '" + row.Account +
                    "' has inconsistent MkSoft gross closing values.");
            }
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

        private static decimal Amount(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string name,
            string fileName,
            string worksheetName,
            int rowNumber)
        {
            object value = reader.GetValue(columns[name]);

            if (value == null || value == DBNull.Value)
                return 0m;

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
                return 0m;

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
    }
}
