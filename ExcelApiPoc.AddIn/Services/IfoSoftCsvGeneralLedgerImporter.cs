using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AddIn.Services
{
    internal sealed class IfoSoftCsvGeneralLedgerImporter
    {
        private static readonly string[] FixedHeaders =
        {
            "Syn", "Ana", "Typ", "P", "Odd", "Polozka", "KZdroja", "Program",
            "Stred", "Zakaz", "Nazov uctu", "Poc_M", "Poc_D", "Roc_M", "Roc_D"
        };

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

            using (IEnumerator<CsvRecord> records = ReadCsvRecords(filePath).GetEnumerator())
            {
                ParseEntity(ReadRequired(records, "entity-information record"), result);
                ParseTitle(ReadRequired(records, "general-ledger title"), result);
                ValidateHeader(ReadRequired(records, "general-ledger header"), result);
                int sequence = 0;
                while (records.MoveNext())
                {
                    CsvRecord source = records.Current;
                    if (source.Fields.Length == 1 && string.IsNullOrWhiteSpace(source.Fields[0])) continue;
                    if (source.Fields.Length != 20)
                        throw new InvalidDataException(source.Location + ": expected 20 fields, but found " + source.Fields.Length + ".");
                    sequence++;
                    string synthetic = Normalize(source.Fields[0], result);
                    string analytical = Normalize(source.Fields[1], result);
                    result.Rows.Add(new GeneralLedgerRow
                    {
                        SequenceNumber = sequence, SourceRecordNumber = source.StartLineNumber,
                        SyntheticCode = synthetic, AnalyticalCode = analytical,
                        AccountCode = AccountCodeNormalizer.Normalize(synthetic + analytical),
                        Type = Normalize(source.Fields[2], result), P = Normalize(source.Fields[3], result),
                        Section = Normalize(source.Fields[4], result), Item = Normalize(source.Fields[5], result),
                        FundingSource = Normalize(source.Fields[6], result), Program = Normalize(source.Fields[7], result),
                        CostCenter = Normalize(source.Fields[8], result), Order = Normalize(source.Fields[9], result),
                        AccountName = Normalize(source.Fields[10], result),
                        OpeningDebit = ParseAmount(source.Fields[11], source.Location),
                        OpeningCredit = ParseAmount(source.Fields[12], source.Location),
                        AnnualDebitTurnover = ParseAmount(source.Fields[13], source.Location),
                        AnnualCreditTurnover = ParseAmount(source.Fields[14], source.Location),
                        PeriodDebitTurnover = ParseAmount(source.Fields[15], source.Location),
                        PeriodCreditTurnover = ParseAmount(source.Fields[16], source.Location),
                        ClosingDebit = ParseAmount(source.Fields[17], source.Location),
                        ClosingCredit = ParseAmount(source.Fields[18], source.Location),
                        Plan = ParseAmount(source.Fields[19], source.Location)
                    });
                }
            }
            if (result.Rows.Count == 0) throw new InvalidDataException("The general ledger does not contain any rows.");
            return result;
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
            Match periodMatch = Regex.Match(period, @"^(?<month>\d{1,2})/(?<year>\d{4})$");
            if (!periodMatch.Success ||
                int.Parse(periodMatch.Groups["month"].Value, CultureInfo.InvariantCulture) != result.ThroughMonth ||
                int.Parse(periodMatch.Groups["year"].Value, CultureInfo.InvariantCulture) != result.FiscalYear ||
                !string.Equals(record.Fields[16].Trim(), period, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.Fields[17].Trim(), "Kon_M", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.Fields[18].Trim(), "Kon_D", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(record.Fields[19].Trim(), "Plan", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(record.Location + ": invalid period or closing-balance columns.");
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
            using (var reader = new StreamReader(path, Encoding.GetEncoding(1250), true))
            {
                int line = 0;
                while (!reader.EndOfStream) { line++; yield return new CsvRecord { Fields = ParseFields(reader.ReadLine()), StartLineNumber = line }; }
            }
        }

        private static string[] ParseFields(string record)
        {
            var fields = new List<string>(); var field = new StringBuilder(); bool quoted = false;
            for (int i = 0; i < record.Length; i++)
            {
                char c = record[i];
                if (c == '"') { if (quoted && i + 1 < record.Length && record[i + 1] == '"') { field.Append('"'); i++; } else quoted = !quoted; }
                else if (c == ';' && !quoted) { fields.Add(field.ToString()); field.Clear(); }
                else field.Append(c);
            }
            if (quoted) throw new InvalidDataException("Line contains an unterminated quoted field.");
            fields.Add(field.ToString()); return fields.ToArray();
        }

        private sealed class CsvRecord
        {
            public string[] Fields { get; set; }
            public int StartLineNumber { get; set; }
            public string Location => "Line " + StartLineNumber;
        }
    }
}
