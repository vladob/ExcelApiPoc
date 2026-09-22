using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    public sealed class IfoSoftCsvGeneralLedgerImporter : IGeneralLedgerImporter
    {
        private const decimal AmountTolerance = 0.01m;

        private static readonly string[] FixedHeaders =
        {
            "Syn", "Ana", "Typ", "P", "Odd", "Polozka", "KZdroja", "Program",
            "Stred", "Zakaz", "Nazov uctu", "Poc_M", "Poc_D", "Roc_M", "Roc_D"
        };

        private static readonly string[] CompactHeaders =
        {
            "Účet",
            "Názov účtu",
            "Počiatočný stav (MD ? DAL)",
            "Obrat od začiatku roka – MD",
            "Obrat od začiatku roka – DAL",
            "Obrat za posledný mesiac – MD",
            "Obrat za posledný mesiac – DAL",
            "Zostatok účtu (MD ? DAL)",
            "Strana PDF"
        };

        public bool CanImport(
    string filePath,
    string accountingFormat)
        {
            return
                string.Equals(
                    accountingFormat,
                    "IfoSoft",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(filePath) &&
                string.Equals(
                    Path.GetExtension(filePath),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileName(filePath).StartsWith(
                    "HL_KNIHA_",
                    StringComparison.OrdinalIgnoreCase);
        }

        public GeneralLedgerImport Import(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A general-ledger file is required.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The general-ledger file was not found.", filePath);

            var result = new GeneralLedgerImport
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = CalculateSha256(filePath),
                TechnicalType = "CSV", AccountingFormat = "IfoSoft",
                ImportedAtUtc = DateTime.UtcNow
            };

            List<CsvRecord> records =
                ReadCsvRecords(filePath)
                    .Where(record => !IsBlankRecord(record))
                    .ToList();

            if (TryImportCompactLedger(
                    filePath,
                    records,
                    result))
            {
                if (result.Rows.Count == 0)
                {
                    throw new InvalidDataException(
                        "The general ledger does not contain any analytical rows.");
                }

                return result;
            }

            ResolveStandardLayout(
                records,
                result,
                out IReadOnlyList<CsvRecord> dataRecords);

            int sequence = 0;

            foreach (CsvRecord source in dataRecords)
            {
                if (source.Fields.Length != 20)
                {
                    throw new InvalidDataException(
                        source.Location +
                        ": expected 20 fields, but found " +
                        source.Fields.Length + ".");
                }

                sequence++;
                string synthetic = Normalize(source.Fields[0], result);
                string analytical = Normalize(source.Fields[1], result);

                result.Rows.Add(new GeneralLedgerRow
                {
                    SequenceNumber = sequence,
                    SourceRecordNumber = source.StartLineNumber,
                    SyntheticCode = synthetic,
                    AnalyticalCode = analytical,
                    AccountCode =
                        AccountCodeNormalizer.Normalize(
                            synthetic + analytical),
                    Type = Normalize(source.Fields[2], result),
                    P = Normalize(source.Fields[3], result),
                    Section = Normalize(source.Fields[4], result),
                    Item = Normalize(source.Fields[5], result),
                    FundingSource = Normalize(source.Fields[6], result),
                    Program = Normalize(source.Fields[7], result),
                    CostCenter = Normalize(source.Fields[8], result),
                    Order = Normalize(source.Fields[9], result),
                    AccountName = Normalize(source.Fields[10], result),
                    OpeningDebit =
                        ParseAmount(source.Fields[11], source.Location),
                    OpeningCredit =
                        ParseAmount(source.Fields[12], source.Location),
                    AnnualDebitTurnover =
                        ParseAmount(source.Fields[13], source.Location),
                    AnnualCreditTurnover =
                        ParseAmount(source.Fields[14], source.Location),
                    PeriodDebitTurnover =
                        ParseAmount(source.Fields[15], source.Location),
                    PeriodCreditTurnover =
                        ParseAmount(source.Fields[16], source.Location),
                    ClosingDebit =
                        ParseAmount(source.Fields[17], source.Location),
                    ClosingCredit =
                        ParseAmount(source.Fields[18], source.Location),
                    Plan =
                        ParseAmount(source.Fields[19], source.Location)
                });
            }
            if (result.Rows.Count == 0) throw new InvalidDataException("The general ledger does not contain any rows.");
            return result;
        }

        private static void ResolveStandardLayout(
            IReadOnlyList<CsvRecord> records,
            GeneralLedgerImport result,
            out IReadOnlyList<CsvRecord> dataRecords)
        {
            if (records == null || records.Count < 4)
            {
                throw new InvalidDataException(
                    "The IfoSoft general ledger does not contain enough records.");
            }

            if (!LooksLikeHeader(records[2]))
            {
                throw new InvalidDataException(
                    "The IfoSoft general-ledger metadata/header layout " +
                    "was not recognized.");
            }

            ParseEntity(records[0], result);
            ParseTitle(records[1], result);
            ValidateHeader(records[2], result);
            dataRecords = records.Skip(3).ToList();
        }

        private static bool LooksLikeHeader(CsvRecord record)
        {
            if (record == null ||
                record.Fields == null ||
                record.Fields.Length != 20)
            {
                return false;
            }

            return
                string.Equals(
                    record.Fields[0].Trim(),
                    "Syn",
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    record.Fields[1].Trim(),
                    "Ana",
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    record.Fields[10].Trim(),
                    "Nazov uctu",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlankRecord(CsvRecord record)
        {
            return
                record == null ||
                record.Fields == null ||
                record.Fields.All(string.IsNullOrWhiteSpace);
        }

        private static bool TryImportCompactLedger(
            string filePath,
            IReadOnlyList<CsvRecord> records,
            GeneralLedgerImport result)
        {
            int headerIndex = FindCompactHeaderIndex(records);

            if (headerIndex < 0)
                return false;

            if (!AccountingFileNameMetadataParser.TryParse(
                    filePath,
                    out AccountingFileNameMetadata metadata) ||
                metadata.DocumentKind !=
                    AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "The compact IfoSoft general-ledger CSV requires " +
                    "a filename in the form " +
                    "'HL_KNIHA_<IČO>_<YYYY>[<stage>].csv'.");
            }

            result.Ico = metadata.Ico;
            result.FiscalYear = metadata.FiscalYear;
            result.ExportStage = metadata.ExportStage;

            ApplyCompactPreamble(
                records.Take(headerIndex).ToList(),
                result);

            int throughMonth =
                result.ThroughMonth > 0
                    ? result.ThroughMonth
                    : metadata.ExportStage ?? 12;

            if (throughMonth < 1 || throughMonth > 12)
            {
                throw new InvalidDataException(
                    "The compact IfoSoft general ledger contains " +
                    "an invalid accounting period.");
            }

            result.ThroughMonth = throughMonth;
            result.PeriodHeader =
                throughMonth.ToString(
                    CultureInfo.InvariantCulture) +
                "/" +
                result.FiscalYear.ToString(
                    CultureInfo.InvariantCulture);

            int sequence = 0;

            for (int index = headerIndex + 1;
                 index < records.Count;
                 index++)
            {
                CsvRecord source = records[index];

                if (source.Fields.Length != CompactHeaders.Length)
                {
                    throw new InvalidDataException(
                        source.Location +
                        ": expected " +
                        CompactHeaders.Length +
                        " fields in the compact IfoSoft general ledger, " +
                        "but found " +
                        source.Fields.Length + ".");
                }

                string accountCode =
                    AccountCodeNormalizer.Normalize(
                        source.Fields[0]);

                if (!IsCompactAnalyticalAccount(accountCode))
                    continue;

                decimal openingNet =
                    ParseCompactAmount(
                        source.Fields[2],
                        source.Location);

                decimal annualDebit =
                    ParseCompactAmount(
                        source.Fields[3],
                        source.Location);

                decimal annualCredit =
                    ParseCompactAmount(
                        source.Fields[4],
                        source.Location);

                decimal periodDebit =
                    ParseCompactAmount(
                        source.Fields[5],
                        source.Location);

                decimal periodCredit =
                    ParseCompactAmount(
                        source.Fields[6],
                        source.Location);

                decimal closingNet =
                    ParseCompactAmount(
                        source.Fields[7],
                        source.Location);

                decimal calculatedClosing =
                    openingNet +
                    annualDebit -
                    annualCredit;

                if (Math.Abs(
                        calculatedClosing -
                        closingNet) >
                    AmountTolerance)
                {
                    throw new InvalidDataException(
                        source.Location +
                        ": account '" +
                        accountCode +
                        "' has inconsistent opening, turnover, " +
                        "and closing values.");
                }

                string accountName =
                    Normalize(
                        source.Fields[1],
                        result);

                sequence++;

                result.Rows.Add(
                    new GeneralLedgerRow
                    {
                        SequenceNumber = sequence,
                        SourceRecordNumber =
                            source.StartLineNumber,
                        SyntheticCode =
                            accountCode.Substring(0, 3),
                        AnalyticalCode =
                            accountCode.Substring(3),
                        AccountCode = accountCode,
                        AccountName = accountName,
                        OpeningDebit =
                            openingNet > 0m
                                ? openingNet
                                : 0m,
                        OpeningCredit =
                            openingNet < 0m
                                ? -openingNet
                                : 0m,
                        AnnualDebitTurnover =
                            annualDebit,
                        AnnualCreditTurnover =
                            annualCredit,
                        PeriodDebitTurnover =
                            periodDebit,
                        PeriodCreditTurnover =
                            periodCredit,
                        ClosingDebit =
                            closingNet > 0m
                                ? closingNet
                                : 0m,
                        ClosingCredit =
                            closingNet < 0m
                                ? -closingNet
                                : 0m,
                        Plan = 0m
                    });
            }

            return true;
        }

        private static int FindCompactHeaderIndex(
            IReadOnlyList<CsvRecord> records)
        {
            if (records == null)
                return -1;

            for (int index = 0;
                 index < records.Count;
                 index++)
            {
                CsvRecord record = records[index];

                if (record?.Fields == null ||
                    record.Fields.Length !=
                        CompactHeaders.Length)
                {
                    continue;
                }

                bool matches = true;

                for (int column = 0;
                     column < CompactHeaders.Length;
                     column++)
                {
                    if (!string.Equals(
                            record.Fields[column].Trim(),
                            CompactHeaders[column],
                            StringComparison.OrdinalIgnoreCase))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                    return index;
            }

            return -1;
        }

        private static void ApplyCompactPreamble(
            IReadOnlyList<CsvRecord> preamble,
            GeneralLedgerImport result)
        {
            foreach (CsvRecord record in preamble)
            {
                string value =
                    string.Join(
                        " ",
                        record.Fields
                            .Where(
                                field =>
                                    !string.IsNullOrWhiteSpace(field))
                            .Select(field => field.Trim()));

                if (value.Length == 0)
                    continue;

                Match titleMatch = Regex.Match(
                    value,
                    @"^(?<name>.+?)\s+[–-]\s+" +
                    @"(?:Hlavná|Hlavna)\s+kniha\s+" +
                    @"(?<year>\d{4})$",
                    RegexOptions.IgnoreCase);

                if (titleMatch.Success)
                {
                    int year = int.Parse(
                        titleMatch.Groups["year"].Value,
                        CultureInfo.InvariantCulture);

                    if (year != result.FiscalYear)
                    {
                        throw new InvalidDataException(
                            record.Location +
                            ": fiscal year in the compact ledger title " +
                            "does not match the filename.");
                    }

                    result.CompanyName =
                        titleMatch.Groups["name"].Value.Trim();

                    continue;
                }

                Match periodMatch = Regex.Match(
                    value,
                    @"obdobie\s+\d{1,2}/(?<fromYear>\d{4})" +
                    @".*?–\s*(?<month>\d{1,2})/" +
                    @"(?<year>\d{4})",
                    RegexOptions.IgnoreCase);

                if (!periodMatch.Success)
                    continue;

                int periodYear = int.Parse(
                    periodMatch.Groups["year"].Value,
                    CultureInfo.InvariantCulture);

                int fromYear = int.Parse(
                    periodMatch.Groups["fromYear"].Value,
                    CultureInfo.InvariantCulture);

                int month = int.Parse(
                    periodMatch.Groups["month"].Value,
                    CultureInfo.InvariantCulture);

                if (periodYear != result.FiscalYear ||
                    fromYear != result.FiscalYear ||
                    month < 1 ||
                    month > 12)
                {
                    throw new InvalidDataException(
                        record.Location +
                        ": accounting period in the compact ledger " +
                        "does not match the filename.");
                }

                result.ThroughMonth = month;
            }
        }

        private static bool IsCompactAnalyticalAccount(
            string accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode) ||
                accountCode.Length <= 3)
            {
                return false;
            }

            for (int index = 0;
                 index < accountCode.Length;
                 index++)
            {
                if (!char.IsDigit(accountCode[index]))
                    return false;
            }

            return true;
        }

        private static decimal ParseCompactAmount(
            string value,
            string location)
        {
            string normalized =
                (value ?? string.Empty)
                    .Trim()
                    .Replace("\u00A0", string.Empty)
                    .Replace(" ", string.Empty);

            if (normalized.Length == 0)
                return 0m;

            if (!decimal.TryParse(
                    normalized,
                    NumberStyles.Number |
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.GetCultureInfo("sk-SK"),
                    out decimal amount))
            {
                throw new InvalidDataException(
                    location +
                    ": '" +
                    normalized +
                    "' is not a valid amount.");
            }

            return amount;
        }

        private static void ParseEntity(CsvRecord record, GeneralLedgerImport result)
        {
            string value = FindSingleValue(record);
            Match match = Regex.Match(value, @"^(?<ico>\d{8})(?:\s+(?<name>.*))?$");
            if (!match.Success) throw new InvalidDataException(record.Location + ": IČO was not found.");
            result.Ico = match.Groups["ico"].Value;
            result.CompanyName = match.Groups["name"].Value.Trim();
        }

        private static void ParseTitle(CsvRecord record, GeneralLedgerImport result)
        {
            string value = FindSingleValue(record);
            Match match = Regex.Match(value, @"^(?:Hlavna|Hlavná)\s+kniha\s+k\s+(?<month>\d{1,2})/(?<year>\d{4})$", RegexOptions.IgnoreCase);
            if (!match.Success) throw new InvalidDataException(record.Location + ": expected an IfoSoft general-ledger title, but found '" + value + "'.");
            result.ThroughMonth = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
            result.FiscalYear = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
            if (result.ThroughMonth < 1 || result.ThroughMonth > 12)
                throw new InvalidDataException(record.Location + ": invalid ledger month.");
        }

        private static void ValidateHeader(CsvRecord record, GeneralLedgerImport result)
        {
            if (record.Fields.Length != 20) throw new InvalidDataException(record.Location + ": invalid general-ledger header.");
            for (int i = 0; i < FixedHeaders.Length; i++)
                if (!string.Equals(record.Fields[i].Trim(), FixedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(record.Location + ": expected column '" + FixedHeaders[i] + "' at position " + (i + 1) + ".");
            string period = record.Fields[15].Trim();

            Match periodMatch = Regex.Match(
                period,
                @"^(?<month>\d{1,2})/(?<year>\d{4})$");

            if (!periodMatch.Success ||
                int.Parse(
                    periodMatch.Groups["month"].Value,
                    CultureInfo.InvariantCulture) !=
                    result.ThroughMonth ||
                int.Parse(
                    periodMatch.Groups["year"].Value,
                    CultureInfo.InvariantCulture) !=
                    result.FiscalYear ||
                !string.Equals(
                    record.Fields[16].Trim(),
                    period,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    record.Fields[17].Trim(),
                    "Kon_M",
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    record.Fields[18].Trim(),
                    "Kon_D",
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    record.Fields[19].Trim(),
                    "Plan",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    record.Location +
                    ": invalid period or closing-balance columns.");
            }

            result.PeriodHeader = period;
        }

        private static decimal ParseAmount(string value, string location)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0) return 0m;
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("sk-SK"), out decimal amount))
                throw new InvalidDataException(location + ": '" + normalized + "' is not a valid amount.");
            return amount;
        }

        private static string Normalize(string value, GeneralLedgerImport result)
        {
            string normalized = JournalTextNormalizer.NormalizeText(value, out bool changed).Trim();
            if (changed || !string.Equals(value ?? string.Empty, normalized, StringComparison.Ordinal)) result.NormalizedTextFieldCount++;
            return normalized;
        }

        private static string FindSingleValue(CsvRecord record)
        {
            string found = null;
            foreach (string field in record.Fields)
                if (!string.IsNullOrWhiteSpace(field))
                {
                    if (found != null) throw new InvalidDataException(record.Location + ": expected one information value.");
                    found = field.Trim();
                }
            return found ?? string.Empty;
        }

        private static CsvRecord ReadRequired(IEnumerator<CsvRecord> records, string description)
        {
            if (!records.MoveNext()) throw new InvalidDataException("The IfoSoft " + description + " is missing.");
            return records.Current;
        }

        private static string CalculateSha256(string path)
        {
            using (var stream = File.OpenRead(path)) using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static IEnumerable<CsvRecord> ReadCsvRecords(string path)
        {
            using (var reader = new StreamReader(
                       path,
                       Encoding.GetEncoding(1250),
                       true))
            {
                int line = 0;

                while (!reader.EndOfStream)
                {
                    line++;
                    string rawRecord = reader.ReadLine();

                    string[] fields;
                    try
                    {
                        fields = ParseFields(rawRecord);
                    }
                    catch (InvalidDataException)
                    {
                        if (!TryRecoverMalformedLedgerRecord(
                                rawRecord,
                                out fields))
                        {
                            throw new InvalidDataException(
                                "File '" + Path.GetFileName(path) +
                                "', line " + line +
                                ": malformed quoted field could not be " +
                                "resolved to the 20-column IfoSoft " +
                                "general-ledger schema.");
                        }
                    }

                    yield return new CsvRecord
                    {
                        Fields = fields,
                        StartLineNumber = line
                    };
                }
            }
        }

        private static string[] ParseFields(string record)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < record.Length; i++)
            {
                char c = record[i];

                if (c == '"')
                {
                    if (quoted &&
                        i + 1 < record.Length &&
                        record[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (c == ';' && !quoted)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            if (quoted)
            {
                throw new InvalidDataException(
                    "Line contains an unterminated quoted field.");
            }

            fields.Add(field.ToString());
            return fields.ToArray();
        }

        private static bool TryRecoverMalformedLedgerRecord(
            string record,
            out string[] fields)
        {
            fields = null;

            if (string.IsNullOrEmpty(record))
                return false;

            string[] parts = record.Split(';');

            // MkSoft-like recovery is deliberately not used here.
            // IfoSoft GL has a fixed 20-column schema. Columns 1-10
            // and 12-20 are structural; column 11 (Nazov uctu) is
            // free text and may contain malformed quote escaping.
            if (parts.Length < 20)
                return false;

            var recovered = new string[20];

            for (int i = 0; i < 10; i++)
            {
                recovered[i] =
                    NormalizeMalformedField(parts[i]);
            }

            int trailingStart = parts.Length - 9;

            recovered[10] =
                NormalizeMalformedField(
                    string.Join(
                        ";",
                        parts.Skip(10)
                            .Take(trailingStart - 10)));

            for (int i = 0; i < 9; i++)
            {
                recovered[11 + i] =
                    NormalizeMalformedField(
                        parts[trailingStart + i]);
            }

            fields = recovered;
            return true;
        }

        private static string NormalizeMalformedField(string value)
        {
            string normalized =
                (value ?? string.Empty).Trim();

            if (normalized.Length >= 2 &&
                normalized[0] == '"' &&
                normalized[normalized.Length - 1] == '"')
            {
                normalized = normalized.Substring(
                    1,
                    normalized.Length - 2);
            }

            return normalized.Replace("\"\"", "\"");
        }

        private sealed class CsvRecord
        {
            public string[] Fields { get; set; }
            public int StartLineNumber { get; set; }
            public string Location => "Line " + StartLineNumber;
        }
    }
}
