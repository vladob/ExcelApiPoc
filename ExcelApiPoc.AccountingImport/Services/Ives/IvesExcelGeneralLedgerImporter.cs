using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    public sealed class IvesExcelGeneralLedgerImporter : IGeneralLedgerImporter
    {
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
                       "HL_KNIHA_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public GeneralLedgerImport Import(string filePath)
        {
            var parser = new IvesExcelGeneralLedgerParser();
            IvesGeneralLedgerParseResult source = parser.Parse(filePath);
            ImportReport report =
                new IvesGeneralLedgerValidator().Validate(source);

            if (!report.IsValid)
            {
                throw CreateValidationException(report);
            }

            var result = new GeneralLedgerImport
            {
                SourceFileName = source.SourceFileName,
                SourceFilePath = source.SourceFilePath,
                SourceFileHash = CalculateSha256(source.SourceFilePath),
                TechnicalType = "Excel",
                AccountingFormat = "IVES",
                Ico = source.Ico,
                FiscalYear = source.FiscalYear,
                ThroughMonth = source.PeriodEnd.Value.Month,
                PeriodHeader =
                    source.PeriodEnd.Value.Month.ToString(
                        CultureInfo.InvariantCulture) +
                    "/" +
                    source.FiscalYear.ToString(
                        CultureInfo.InvariantCulture),
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = report
            };

            foreach (IvesGeneralLedgerSourceRow sourceRow in source.AccountRows)
            {
                AddCanonicalRow(result, sourceRow);
            }

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES general ledger does not contain any account records.");
            }

            return result;
        }

        private static void AddCanonicalRow(
            GeneralLedgerImport result,
            IvesGeneralLedgerSourceRow sourceRow)
        {
            string accountCode =
                AccountCodeNormalizer.Normalize(sourceRow.AccountCode);

            int separatorIndex = accountCode.IndexOf('.');
            string syntheticCode = separatorIndex < 0
                ? accountCode
                : accountCode.Substring(0, separatorIndex);

            if (syntheticCode.Length != 3 || accountCode.Length < 3)
            {
                throw new InvalidDataException(
                    "IVES source row " + sourceRow.SourceRowNumber +
                    " contains invalid account code '" +
                    sourceRow.AccountCode + "'.");
            }

            decimal openingBalance =
                sourceRow.OpeningBalance.GetValueOrDefault();
            decimal closingBalance =
                sourceRow.ClosingBalance.GetValueOrDefault();
            decimal debitTurnover =
                sourceRow.DebitTurnover.GetValueOrDefault();
            decimal creditTurnover =
                sourceRow.CreditTurnover.GetValueOrDefault();

            result.Rows.Add(new GeneralLedgerRow
            {
                SequenceNumber = sourceRow.SequenceNumber,
                SourceRecordNumber = sourceRow.SourceRowNumber,
                SyntheticCode = syntheticCode,
                AnalyticalCode = accountCode.Substring(3),
                AccountCode = accountCode,
                AccountName = sourceRow.Text ?? string.Empty,
                OpeningDebit = openingBalance > 0m
                    ? openingBalance
                    : 0m,
                OpeningCredit = openingBalance < 0m
                    ? -openingBalance
                    : 0m,
                AnnualDebitTurnover = debitTurnover,
                AnnualCreditTurnover = creditTurnover,
                PeriodDebitTurnover = debitTurnover,
                PeriodCreditTurnover = creditTurnover,
                ClosingDebit = closingBalance > 0m
                    ? closingBalance
                    : 0m,
                ClosingCredit = closingBalance < 0m
                    ? -closingBalance
                    : 0m,
                Plan = 0m
            });
        }

        private static InvalidDataException CreateValidationException(
            ImportReport report)
        {
            foreach (ImportDiagnostic diagnostic in report.Diagnostics)
            {
                if (diagnostic.Severity == ImportDiagnosticSeverity.Error)
                {
                    return new InvalidDataException(
                        "IVES general-ledger validation failed: " +
                        diagnostic.Message +
                        FormatSourceLocation(diagnostic));
                }
            }

            return new InvalidDataException(
                "IVES general-ledger validation failed.");
        }

        private static string FormatSourceLocation(
            ImportDiagnostic diagnostic)
        {
            if (diagnostic.Source == null ||
                !diagnostic.Source.SourceRowNumber.HasValue)
            {
                return string.Empty;
            }

            return " Source: worksheet '" +
                (diagnostic.Source.WorksheetName ?? "(unnamed)") +
                "', row " +
                diagnostic.Source.SourceRowNumber.Value.ToString(
                    CultureInfo.InvariantCulture) +
                ".";
        }

        private static string CalculateSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(
                    sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
            }
        }
    }
}
