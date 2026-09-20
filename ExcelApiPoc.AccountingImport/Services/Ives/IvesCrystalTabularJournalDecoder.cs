using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesCrystalTabularJournalDecoder
    {
        private static readonly Regex IcoPattern = new Regex(
            @"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public IvesJournalParseResult Decode(
            string filePath,
            IEnumerable<IvesCrystalTabularRow> rows)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A source file is required.", nameof(filePath));

            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            var result = new IvesJournalParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath
            };

            AccountingFileNameMetadata metadata = null;
            if (AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out AccountingFileNameMetadata parsedMetadata))
            {
                if (parsedMetadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingJournal)
                {
                    throw new InvalidDataException(
                        "Filename '" + fileName +
                        "' does not identify an accounting journal.");
                }

                metadata = parsedMetadata;
                result.FiscalYear = metadata.FiscalYear;
                result.PeriodStart = new DateTime(metadata.FiscalYear, 1, 1);
                result.PeriodEnd = new DateTime(metadata.FiscalYear, 12, 31);
            }

            string contentIco = null;
            var footerAmounts = new List<decimal>();
            decimal? previousFooter = null;
            int sourceRowCount = 0;
            int sequence = 0;

            foreach (IvesCrystalTabularRow sourceRow in rows)
            {
                sourceRowCount++;

                if (contentIco == null)
                    contentIco = FindIco(sourceRow.Values);

                if (TryDecodeTransaction(
                        sourceRow,
                        metadata?.FiscalYear,
                        ++sequence,
                        fileName,
                        out IvesJournalSourceRow transaction))
                {
                    result.TransactionRows.Add(transaction);
                }
                else
                {
                    sequence--;
                }

                if (TryFindFooterAmount(sourceRow.Values, out decimal footerAmount))
                {
                    if (!previousFooter.HasValue ||
                        previousFooter.Value != footerAmount)
                    {
                        footerAmounts.Add(footerAmount);
                        previousFooter = footerAmount;
                    }
                }
            }

            result.SourceRowCount = sourceRowCount;
            result.Ico = contentIco ?? metadata?.Ico;

            if (result.TransactionRows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES Crystal tabular accounting-journal export contains no transaction rows.");
            }

            if (metadata == null)
            {
                int fiscalYear = ResolveFiscalYearFromTransactions(result);
                result.FiscalYear = fiscalYear;
                result.PeriodStart = new DateTime(fiscalYear, 1, 1);
                result.PeriodEnd = new DateTime(fiscalYear, 12, 31);
            }

            if (footerAmounts.Count > 0)
            {
                var total = new IvesJournalSourceRow
                {
                    SequenceNumber = 1,
                    SourceRowNumber = sourceRowCount + 1,
                    Kind = IvesJournalRowKind.ReportTotal,
                    SourceLocation = fileName +
                        ", Crystal group footers (" +
                        footerAmounts.Count + ")"
                };
                total.ReportedAmounts.Add(footerAmounts.Sum());
                result.ReportTotalRows.Add(total);
            }

            return result;
        }

        private static bool TryDecodeTransaction(
            IvesCrystalTabularRow sourceRow,
            int? expectedFiscalYear,
            int sequenceNumber,
            string sourceFileName,
            out IvesJournalSourceRow transaction)
        {
            transaction = null;
            string[] values = sourceRow.Values ?? Array.Empty<string>();

            for (int index = 0; index + 6 < values.Length; index++)
            {
                if (!TryParsePostingDate(values[index], out DateTime postingDate))
                    continue;

                if (expectedFiscalYear.HasValue &&
                    postingDate.Year != expectedFiscalYear.Value)
                {
                    continue;
                }

                string documentNumber = Normalize(values[index + 1]);
                string debitAccount = Normalize(values[index + 2]);
                string creditAccount = Normalize(values[index + 3]);

                if (documentNumber == null ||
                    (debitAccount == null && creditAccount == null) ||
                    !TryParseAmount(values[index + 4], out decimal amount) ||
                    !string.Equals(
                        Normalize(values[index + 5]),
                        "€",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                string module = index + 11 < values.Length
                    ? Normalize(values[index + 11])
                    : null;

                if (module == null)
                {
                    module = FindModule(values, index + 7, Math.Min(values.Length, index + 14));
                }

                transaction = new IvesJournalSourceRow
                {
                    SequenceNumber = sequenceNumber,
                    SourceRowNumber = sourceRow.SourceRecordNumber,
                    Kind = IvesJournalRowKind.Transaction,
                    PostingDate = postingDate,
                    DocumentNumber = documentNumber,
                    DebitCompositeAccount = debitAccount,
                    CreditCompositeAccount = creditAccount,
                    Amount = amount,
                    Currency = "€",
                    Text = Normalize(values[index + 6]),
                    Module = module,
                    SourceLocation = string.IsNullOrWhiteSpace(sourceRow.SourceLocation)
                        ? sourceFileName + ", record " + sourceRow.SourceRecordNumber
                        : sourceRow.SourceLocation
                };

                return true;
            }

            return false;
        }

        private static string FindModule(
            string[] values,
            int startIndex,
            int endExclusive)
        {
            for (int index = startIndex; index < endExclusive; index++)
            {
                string value = Normalize(values[index]);
                if (value == null)
                    continue;

                if (string.Equals(value, "UCT", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "FAK", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "POK", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "BAN", StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }

        private static string FindIco(string[] values)
        {
            foreach (string value in values ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                Match match = IcoPattern.Match(value);
                if (match.Success)
                    return match.Groups["ico"].Value;
            }

            return null;
        }

        private static bool TryFindFooterAmount(
            string[] values,
            out decimal amount)
        {
            amount = 0m;
            if (values == null)
                return false;

            for (int index = 0; index < values.Length; index++)
            {
                string value = Normalize(values[index]);
                if (!string.Equals(
                        value,
                        "Spolu :",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        value,
                        "Spolu:",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int candidate = index + 1;
                     candidate < Math.Min(values.Length, index + 5);
                     candidate++)
                {
                    if (TryParseAmount(values[candidate], out amount))
                        return true;
                }
            }

            return false;
        }

        private static bool TryParsePostingDate(
            string value,
            out DateTime date)
        {
            string normalized = Normalize(value);
            date = default(DateTime);

            if (normalized == null)
                return false;

            string[] formats =
            {
                "dd.MM.yyyy",
                "d.M.yyyy",
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ss"
            };

            return DateTime.TryParseExact(
                normalized,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static bool TryParseAmount(
            string value,
            out decimal amount)
        {
            string normalized = Normalize(value);
            amount = 0m;

            if (normalized == null)
                return false;

            if (decimal.TryParse(
                    normalized,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out amount))
            {
                return true;
            }

            return decimal.TryParse(
                normalized,
                NumberStyles.Number | NumberStyles.AllowLeadingSign,
                CultureInfo.GetCultureInfo("sk-SK"),
                out amount);
        }

        private static int ResolveFiscalYearFromTransactions(
            IvesJournalParseResult result)
        {
            int fiscalYear = result.TransactionRows[0].PostingDate.Value.Year;

            if (result.TransactionRows.Any(
                    row => row.PostingDate.Value.Year != fiscalYear))
            {
                throw new InvalidDataException(
                    "The IVES Crystal tabular accounting-journal export contains " +
                    "transactions from more than one fiscal year and its filename " +
                    "does not identify the fiscal year.");
            }

            return fiscalYear;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }

    internal sealed class IvesCrystalTabularRow
    {
        public int SourceRecordNumber { get; set; }
        public string SourceLocation { get; set; }
        public string[] Values { get; set; }
    }
}
