using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    public sealed class SoftipMopExcelJournalImporter : IJournalImporter
    {
        public bool CanImport(string filePath, string accountingFormat)
        {
            if (!string.Equals(accountingFormat, "Softip-MOP", StringComparison.OrdinalIgnoreCase) ||
                !TryDetect(filePath, out int _))
            {
                return false;
            }

            return true;
        }

        public static bool TryDetect(string filePath, out int fiscalYear)
        {
            fiscalYear = 0;

            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath) ||
                !string.Equals(Path.GetExtension(filePath), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
                {
                    if (reader.ResultsCount != 1 || !reader.Read()) return false;
                    if (!SoftipMopJournalColumnLayout.TryDiscover(
                            ReadHeaders(reader),
                            out SoftipMopJournalColumnLayout layout,
                            out string _))
                    {
                        return false;
                    }

                    while (reader.Read())
                    {
                        string period = (ExcelWorkbookReader.GetText(
                            reader,
                            layout.AccountingPeriod) ?? string.Empty).Trim();

                        if (period.Length == 0)
                        {
                            continue;
                        }

                        if (period.Length != 6 ||
                            !int.TryParse(
                                period.Substring(0, 4),
                                NumberStyles.None,
                                CultureInfo.InvariantCulture,
                                out fiscalYear) ||
                            !int.TryParse(
                                period.Substring(4, 2),
                                NumberStyles.None,
                                CultureInfo.InvariantCulture,
                                out int month) ||
                            month < 1 || month > 12)
                        {
                            fiscalYear = 0;
                            return false;
                        }

                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                fiscalYear = 0;
                return false;
            }
        }

        public JournalImport Import(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The accounting journal was not found.", filePath);

            var result = new JournalImport
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = CalculateSha256(filePath),
                TechnicalType = "Excel",
                AccountingFormat = "Softip-MOP",
                ImportedAtUtc = DateTime.UtcNow
            };
            var report = new ImportReport
            {
                AccountingFormat = "Softip-MOP",
                ImportType = "AccountingJournal",
                SourceFileName = result.SourceFileName
            };
            result.ImportReport = report;

            decimal debitTotal = 0m;
            decimal creditTotal = 0m;
            int sourceRows = 0;
            int zeroAmountRows = 0;
            int outsidePeriodRows = 0;
            int outsideFiscalYearRows = 0;
            string accountingPeriod = null;
            string worksheetName;

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                if (reader.ResultsCount != 1)
                    throw new InvalidDataException("The Softip-MOP journal must contain exactly one worksheet.");

                worksheetName = reader.Name ?? "(unnamed)";
                if (!reader.Read())
                    throw new InvalidDataException("The Softip-MOP journal is empty.");

                if (!SoftipMopJournalColumnLayout.TryDiscover(
                        ReadHeaders(reader), out SoftipMopJournalColumnLayout layout, out string layoutError))
                {
                    throw new InvalidDataException(
                        "The selected workbook is not a recognized Softip-MOP accounting journal: " +
                        layoutError);
                }

                int sourceRowNumber = 1;
                while (reader.Read())
                {
                    sourceRowNumber++;
                    if (IsBlankRow(reader)) continue;
                    sourceRows++;

                    string location = result.SourceFileName + ", worksheet " +
                        worksheetName + ", row " + sourceRowNumber;
                    string rowPeriod = RequiredText(reader, layout.AccountingPeriod, location, "Účt.mes.");
                    int fiscalYear = ParsePeriod(rowPeriod, location);

                    if (accountingPeriod == null)
                    {
                        accountingPeriod = rowPeriod;
                        result.FiscalYear = fiscalYear;
                    }
                    else if (!string.Equals(accountingPeriod, rowPeriod, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException(
                            location + ": accounting period '" + rowPeriod +
                            "' differs from period '" + accountingPeriod + "' in the first data row.");
                    }

                    DateTime postingDate = ParseDate(reader.GetValue(layout.PostingDate), location);
                    if (postingDate.Year != fiscalYear) outsideFiscalYearRows++;

                    if (postingDate.Month != int.Parse(rowPeriod.Substring(4, 2), CultureInfo.InvariantCulture))
                        outsidePeriodRows++;

                    decimal debit = ParseAmount(reader.GetValue(layout.DebitAmount), location, "Má dať");
                    decimal credit = ParseAmount(reader.GetValue(layout.CreditAmount), location, "DAL");
                    debitTotal += debit;
                    creditTotal += credit;

                    if (debit == 0m && credit == 0m)
                    {
                        zeroAmountRows++;
                        continue;
                    }

                    string account = RequiredText(reader, layout.Account, location, "Účet");
                    account = AccountCodeNormalizer.Normalize(account);
                    if (account.Length == 0)
                        throw new InvalidDataException(location + ": account is empty after normalization.");

                    bool rowNormalized = false;
                    string description = NormalizeText(
                        ExcelWorkbookReader.GetText(reader, layout.Description), result, ref rowNormalized);
                    string documentType = NormalizeText(
                        ExcelWorkbookReader.GetText(reader, layout.DocumentType), result, ref rowNormalized);
                    string documentNumber = NormalizeText(
                        ExcelWorkbookReader.GetText(reader, layout.DocumentNumber), result, ref rowNormalized);
                    string costCenter = layout.CostCenter.HasValue
                        ? NormalizeText(ExcelWorkbookReader.GetText(reader, layout.CostCenter.Value), result, ref rowNormalized)
                        : null;

                    var row = new JournalRow
                    {
                        SequenceNumber = result.Rows.Count + 1,
                        SourceRecordNumber = sourceRows,
                        SourceStartLineNumber = sourceRowNumber,
                        SourceEndLineNumber = sourceRowNumber,
                        SourceLocation = location,
                        TextNormalizationApplied = rowNormalized,
                        DocumentType = documentType,
                        DocumentNumber = documentNumber,
                        PostingDate = postingDate,
                        Description = description,
                        RecordKind = JournalRecordKind.Normal
                    };

                    if (debit != 0m)
                    {
                        row.DebitAccount = account;
                        row.DebitAmount = debit;
                        row.DebitCostCenter = costCenter;
                    }
                    if (credit != 0m)
                    {
                        row.CreditAccount = account;
                        row.CreditAmount = credit;
                        row.CreditCostCenter = costCenter;
                    }

                    JournalImportCapacity.EnsureCanAppend(
                        result.Rows.Count,
                        1,
                        result.SourceFileName);
                    result.Rows.Add(row);
                }
            }

            if (sourceRows == 0)
                throw new InvalidDataException("The Softip-MOP journal does not contain any data rows.");
            if (result.Rows.Count == 0)
                throw new InvalidDataException("The Softip-MOP journal does not contain any non-zero postings.");

            report.RecordCounts["SourceRows"] = sourceRows;
            report.RecordCounts["Transactions"] = result.Rows.Count;
            report.RecordCounts["ZeroAmountRows"] = zeroAmountRows;
            report.RecordCounts["PostingDatesOutsideAccountingMonth"] = outsidePeriodRows;
            report.RecordCounts["PostingDatesOutsideFiscalYear"] = outsideFiscalYearRows;
            AddBalanceValidation(report, debitTotal, creditTotal, result.SourceFileName, worksheetName);

            if (zeroAmountRows > 0)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_ZERO_AMOUNT_ROWS",
                    Severity = ImportDiagnosticSeverity.Information,
                    Message = zeroAmountRows.ToString("N0") +
                        " source rows contain neither a debit nor a credit amount and were not mapped to canonical postings.",
                    Source = Provenance(result.SourceFileName, worksheetName, "ZeroAmountRows")
                });
            }
            if (outsidePeriodRows > 0)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_POSTING_DATE_OUTSIDE_MONTH",
                    Severity = ImportDiagnosticSeverity.Information,
                    Message = outsidePeriodRows.ToString("N0") +
                        " rows have a posting date outside accounting period " + accountingPeriod +
                        "; the accounting period remains authoritative.",
                    Source = Provenance(result.SourceFileName, worksheetName, "PostingDates")
                });
            }
            if (outsideFiscalYearRows > 0)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_POSTING_DATE_OUTSIDE_FISCAL_YEAR",
                    Severity = ImportDiagnosticSeverity.Information,
                    Message = outsideFiscalYearRows.ToString("N0") +
                        " rows have a posting date outside fiscal year " + result.FiscalYear +
                        "; accounting period " + accountingPeriod + " remains authoritative.",
                    Source = Provenance(result.SourceFileName, worksheetName, "PostingDates")
                });
            }

            if (!report.IsValid)
                throw new InvalidDataException("Softip-MOP accounting-journal validation failed: debit and credit totals differ.");

            return result;
        }

        private static void AddBalanceValidation(
            ImportReport report, decimal debit, decimal credit, string fileName, string worksheetName)
        {
            decimal difference = debit - credit;
            decimal tolerance = 0.01m;
            report.ValidationResults.Add(new ImportValidationResult
            {
                Code = "SOFTIP_MOP_DEBIT_CREDIT_BALANCE",
                Scope = "AccountingJournal",
                Description = "Debit and credit totals agree.",
                IsValid = Math.Abs(difference) <= tolerance,
                ExpectedAmount = debit,
                ActualAmount = credit,
                Difference = difference,
                Tolerance = tolerance,
                Source = Provenance(fileName, worksheetName, "AllSourceRows")
            });

            if (Math.Abs(difference) > tolerance)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_DEBIT_CREDIT_DIFFERENCE",
                    Severity = ImportDiagnosticSeverity.Error,
                    Message = "Debit total " + debit.ToString("N2") +
                        " differs from credit total " + credit.ToString("N2") + ".",
                    Source = Provenance(fileName, worksheetName, "AllSourceRows")
                });
            }
        }

        private static SourceProvenance Provenance(string fileName, string worksheetName, string recordSet)
        {
            return new SourceProvenance
            {
                SourceFileName = fileName,
                WorksheetName = worksheetName,
                RecordSet = recordSet
            };
        }

        private static IReadOnlyList<string> ReadHeaders(IExcelDataReader reader)
        {
            var headers = new List<string>(reader.FieldCount);
            for (int index = 0; index < reader.FieldCount; index++)
                headers.Add(ExcelWorkbookReader.GetText(reader, index));
            return headers;
        }

        private static bool IsBlankRow(IExcelDataReader reader)
        {
            for (int index = 0; index < reader.FieldCount; index++)
            {
                object value = reader.GetValue(index);
                if (value != null && value != DBNull.Value &&
                    !string.IsNullOrWhiteSpace(value.ToString())) return false;
            }
            return true;
        }

        private static string RequiredText(
            IExcelDataReader reader, int column, string location, string field)
        {
            string value = (ExcelWorkbookReader.GetText(reader, column) ?? string.Empty).Trim();
            if (value.Length == 0)
                throw new InvalidDataException(location + ": required field '" + field + "' is empty.");
            return value;
        }

        private static int ParsePeriod(string value, string location)
        {
            if (value.Length != 6 ||
                !int.TryParse(value.Substring(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out int year) ||
                !int.TryParse(value.Substring(4, 2), NumberStyles.None, CultureInfo.InvariantCulture, out int month) ||
                month < 1 || month > 12)
            {
                throw new InvalidDataException(location + ": '" + value + "' is not a valid accounting period YYYYMM.");
            }
            return year;
        }

        private static DateTime ParseDate(object value, string location)
        {
            if (value is DateTime date) return date.Date;
            if (value is double serial)
            {
                try { return DateTime.FromOADate(serial).Date; }
                catch (ArgumentException) { }
            }

            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            string[] formats = { "yyyy-MM-dd", "d.M.yyyy", "dd.MM.yyyy" };
            if (DateTime.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsed)) return parsed.Date;

            throw new InvalidDataException(location + ": '" + text + "' is not a valid posting date.");
        }

        private static decimal ParseAmount(object value, string location, string field)
        {
            if (value == null || value == DBNull.Value) return 0m;
            try
            {
                if (value is decimal decimalValue) return decimalValue;
                if (value is double doubleValue) return Convert.ToDecimal(doubleValue);
                if (value is float floatValue) return Convert.ToDecimal(floatValue);
                if (value is int intValue) return intValue;
                if (value is long longValue) return longValue;
            }
            catch (OverflowException) { }

            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            if (decimal.TryParse(text.Trim(), NumberStyles.Number,
                    CultureInfo.InvariantCulture, out decimal invariant)) return invariant;
            if (decimal.TryParse(text.Trim(), NumberStyles.Number,
                    CultureInfo.GetCultureInfo("sk-SK"), out decimal slovak)) return slovak;

            throw new InvalidDataException(location + ": '" + text + "' is not a valid " + field + " amount.");
        }

        private static string NormalizeText(
            string value, JournalImport result, ref bool rowNormalized)
        {
            string normalized = JournalTextNormalizer.NormalizeText(value, out bool changed);
            if (changed)
            {
                rowNormalized = true;
                result.NormalizedTextFieldCount++;
            }
            return normalized.Length == 0 ? null : normalized;
        }

        private static string CalculateSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }
    }
}
