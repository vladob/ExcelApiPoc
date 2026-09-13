using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    public sealed class SoftipMopMonthlyJournalImporter
    {
        public JournalImport Import(IEnumerable<string> filePaths)
        {
            if (filePaths == null) throw new ArgumentNullException(nameof(filePaths));

            string[] paths = filePaths
                .Select(path => string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path))
                .ToArray();

            if (paths.Length == 0)
                throw new ArgumentException("At least one Softip-MOP journal file is required.", nameof(filePaths));
            if (paths.Any(path => path == null))
                throw new ArgumentException("A Softip-MOP journal file path is empty.", nameof(filePaths));

            SoftipMopJournalFileDescriptor[] files = paths
                .Select(Inspect)
                .OrderBy(file => file.AccountingPeriod, StringComparer.Ordinal)
                .ToArray();

            ValidatePeriods(files);

            string combinedName = files.Length == 1
                ? files[0].FileName
                : files.Length + " Softip-MOP monthly journal files";
            var result = new JournalImport
            {
                SourceFileName = combinedName,
                SourceFilePath = files.Length == 1 ? files[0].FullPath : null,
                TechnicalType = "Excel",
                AccountingFormat = "Softip-MOP",
                FiscalYear = files[0].FiscalYear,
                ImportedAtUtc = DateTime.UtcNow
            };
            var report = new ImportReport
            {
                AccountingFormat = "Softip-MOP",
                ImportType = "AccountingJournal",
                SourceFileName = combinedName
            };
            result.ImportReport = report;

            var hashes = new List<string>(files.Length);
            var sourceHashes = new List<string>(files.Length);
            decimal combinedDebit = 0m;
            decimal combinedCredit = 0m;

            foreach (SoftipMopJournalFileDescriptor file in files)
            {
                JournalImport monthly = new SoftipMopExcelJournalImporter().Import(file.FullPath);
                hashes.Add(file.AccountingPeriod + ":" + monthly.SourceFileHash);
                sourceHashes.Add(monthly.SourceFileHash);
                result.NormalizedTextFieldCount += monthly.NormalizedTextFieldCount;

                foreach (JournalRow row in monthly.Rows)
                {
                    row.SequenceNumber = result.Rows.Count + 1;
                    result.Rows.Add(row);
                }

                MergeReport(report, monthly.ImportReport);
                ImportValidationResult balance = monthly.ImportReport.ValidationResults
                    .Single(validation => validation.Code == "SOFTIP_MOP_DEBIT_CREDIT_BALANCE");
                combinedDebit += balance.ExpectedAmount;
                combinedCredit += balance.ActualAmount;
            }

            result.SourceFileHash = files.Length == 1
                ? sourceHashes[0]
                : CalculateCombinedHash(hashes);
            report.RecordCounts["SourceFiles"] = files.Length;
            report.RecordCounts["AccountingPeriods"] = files.Length;
            AddCombinedBalanceValidation(report, combinedDebit, combinedCredit, combinedName);
            AddMissingPeriodDiagnostic(report, files, combinedName);

            return result;
        }

        internal static void ValidatePeriods(IReadOnlyList<SoftipMopJournalFileDescriptor> files)
        {
            if (files.Count == 0)
                throw new InvalidDataException("No Softip-MOP journal files were inspected.");

            IGrouping<string, SoftipMopJournalFileDescriptor> duplicate = files
                .GroupBy(file => file.AccountingPeriod, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
            {
                throw new InvalidDataException(
                    "Accounting period " + duplicate.Key + " occurs in more than one Softip-MOP journal file: " +
                    string.Join(", ", duplicate.Select(file => file.FileName)) + ".");
            }

            int fiscalYear = files[0].FiscalYear;
            SoftipMopJournalFileDescriptor wrongYear = files
                .FirstOrDefault(file => file.FiscalYear != fiscalYear);
            if (wrongYear != null)
            {
                throw new InvalidDataException(
                    "Softip-MOP journal files contain mixed fiscal years " + fiscalYear +
                    " and " + wrongYear.FiscalYear + ".");
            }
        }

        private static SoftipMopJournalFileDescriptor Inspect(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("The accounting journal was not found.", path);
            if (!string.Equals(Path.GetExtension(path), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Softip-MOP journal files must use the .xlsx format.");

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(path))
            {
                if (reader.ResultsCount != 1)
                    throw new InvalidDataException(
                        "Softip-MOP journal '" + Path.GetFileName(path) + "' must contain exactly one worksheet.");
                if (!reader.Read())
                    throw new InvalidDataException(
                        "Softip-MOP journal '" + Path.GetFileName(path) + "' is empty.");

                var headers = new List<string>(reader.FieldCount);
                for (int index = 0; index < reader.FieldCount; index++)
                    headers.Add(ExcelWorkbookReader.GetText(reader, index));

                if (!SoftipMopJournalColumnLayout.TryDiscover(
                        headers, out SoftipMopJournalColumnLayout layout, out string error))
                {
                    throw new InvalidDataException(
                        "Softip-MOP journal '" + Path.GetFileName(path) + "' is not recognized: " + error);
                }

                int sourceRow = 1;
                while (reader.Read())
                {
                    sourceRow++;
                    string period = (ExcelWorkbookReader.GetText(reader, layout.AccountingPeriod) ?? string.Empty).Trim();
                    if (period.Length == 0) continue;

                    if (period.Length != 6 ||
                        !int.TryParse(period.Substring(0, 4), NumberStyles.None,
                            CultureInfo.InvariantCulture, out int year) ||
                        !int.TryParse(period.Substring(4, 2), NumberStyles.None,
                            CultureInfo.InvariantCulture, out int month) ||
                        month < 1 || month > 12)
                    {
                        throw new InvalidDataException(
                            Path.GetFileName(path) + ", worksheet " + (reader.Name ?? "(unnamed)") +
                            ", row " + sourceRow + ": '" + period +
                            "' is not a valid accounting period YYYYMM.");
                    }

                    return new SoftipMopJournalFileDescriptor(path, period, year, month);
                }
            }

            throw new InvalidDataException(
                "Softip-MOP journal '" + Path.GetFileName(path) + "' does not contain an accounting period.");
        }

        private static void MergeReport(ImportReport target, ImportReport source)
        {
            foreach (KeyValuePair<string, int> count in source.RecordCounts)
            {
                target.RecordCounts.TryGetValue(count.Key, out int current);
                target.RecordCounts[count.Key] = current + count.Value;
            }
            target.ValidationResults.AddRange(source.ValidationResults);
            target.Diagnostics.AddRange(source.Diagnostics);
        }

        private static void AddCombinedBalanceValidation(
            ImportReport report, decimal debit, decimal credit, string sourceName)
        {
            decimal difference = debit - credit;
            decimal tolerance = 0.01m;
            report.ValidationResults.Add(new ImportValidationResult
            {
                Code = "SOFTIP_MOP_COMBINED_DEBIT_CREDIT_BALANCE",
                Scope = "CombinedAccountingJournal",
                Description = "Combined debit and credit totals agree.",
                IsValid = Math.Abs(difference) <= tolerance,
                ExpectedAmount = debit,
                ActualAmount = credit,
                Difference = difference,
                Tolerance = tolerance,
                Source = new SourceProvenance
                {
                    SourceFileName = sourceName,
                    RecordSet = "AllMonthlyFiles"
                }
            });

            if (Math.Abs(difference) > tolerance)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_COMBINED_DEBIT_CREDIT_DIFFERENCE",
                    Severity = ImportDiagnosticSeverity.Error,
                    Message = "Combined debit total " + debit.ToString("N2") +
                        " differs from combined credit total " + credit.ToString("N2") + ".",
                    Source = new SourceProvenance
                    {
                        SourceFileName = sourceName,
                        RecordSet = "AllMonthlyFiles"
                    }
                });
            }
        }

        private static void AddMissingPeriodDiagnostic(
            ImportReport report,
            IReadOnlyList<SoftipMopJournalFileDescriptor> files,
            string sourceName)
        {
            int firstMonth = files.Min(file => file.Month);
            int lastMonth = files.Max(file => file.Month);
            var present = new HashSet<int>(files.Select(file => file.Month));
            int[] missing = Enumerable.Range(firstMonth, lastMonth - firstMonth + 1)
                .Where(month => !present.Contains(month))
                .ToArray();

            report.RecordCounts["MissingAccountingPeriods"] = missing.Length;
            if (missing.Length == 0) return;

            report.Diagnostics.Add(new ImportDiagnostic
            {
                Code = "SOFTIP_MOP_MISSING_ACCOUNTING_PERIODS",
                Severity = ImportDiagnosticSeverity.Warning,
                Message = "The selected Softip-MOP journal files have missing accounting periods between " +
                    files[0].AccountingPeriod + " and " + files[files.Count - 1].AccountingPeriod + ": " +
                    string.Join(", ", missing.Select(month =>
                        files[0].FiscalYear.ToString("0000", CultureInfo.InvariantCulture) +
                        month.ToString("00", CultureInfo.InvariantCulture))) + ".",
                Source = new SourceProvenance
                {
                    SourceFileName = sourceName,
                    RecordSet = "SelectedMonthlyFiles"
                }
            });
        }

        private static string CalculateCombinedHash(IEnumerable<string> values)
        {
            string content = string.Join("\n", values) + "\n";
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }

    internal sealed class SoftipMopJournalFileDescriptor
    {
        public SoftipMopJournalFileDescriptor(
            string fullPath, string accountingPeriod, int fiscalYear, int month)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            AccountingPeriod = accountingPeriod;
            FiscalYear = fiscalYear;
            Month = month;
        }

        public string FullPath { get; }
        public string FileName { get; }
        public string AccountingPeriod { get; }
        public int FiscalYear { get; }
        public int Month { get; }
    }
}
