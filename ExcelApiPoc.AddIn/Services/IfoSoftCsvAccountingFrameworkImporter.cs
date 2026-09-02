using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AddIn.Services
{
    internal sealed class IfoSoftCsvAccountingFrameworkImporter
    {
        private static readonly string[] ExpectedHeaders =
        {
            "Syn", "Ana", "Nazov uctu", "Typ", "Pods", "Dan", "Saldo", "DPH"
        };

        public AccountingFrameworkImport Import(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("An accounting-framework file is required.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The accounting-framework file was not found.", filePath);

            var result = new AccountingFrameworkImport
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = CalculateSha256(filePath),
                TechnicalType = "CSV",
                AccountingFormat = "IfoSoft",
                ImportedAtUtc = DateTime.UtcNow
            };

            using (IEnumerator<CsvRecord> records = ReadCsvRecords(filePath).GetEnumerator())
            {
                CsvRecord entity = ReadRequired(records, "entity-information record");
                CsvRecord title = ReadRequired(records, "accounting-framework title");
                CsvRecord header = ReadRequired(records, "accounting-framework header");
                ParseEntity(entity, result);
                ParseTitle(title, result);
                ValidateHeader(header);

                int sequence = 0;
                while (records.MoveNext())
                {
                    CsvRecord source = records.Current;
                    if (source.Fields.Length == 1 && string.IsNullOrWhiteSpace(source.Fields[0]))
                        continue;
                    if (source.Fields.Length != ExpectedHeaders.Length)
                        throw new InvalidDataException(source.Location + ": expected 8 fields, but found " + source.Fields.Length + ".");

                    sequence++;
                    string sourceSynthetic = source.Fields[0];
                    string sourceAnalytical = source.Fields[1];
                    string synthetic = Normalize(sourceSynthetic, result);
                    string analytical = Normalize(sourceAnalytical, result);
                    string name = Normalize(source.Fields[2], result);

                    result.Rows.Add(new AccountingFrameworkRow
                    {
                        SequenceNumber = sequence,
                        SourceRecordNumber = source.StartLineNumber,
                        SourceSyntheticCode = sourceSynthetic,
                        SourceAnalyticalCode = sourceAnalytical,
                        SyntheticCode = synthetic,
                        AnalyticalCode = analytical,
                        AccountCode = CreateAccountCode(synthetic, analytical),
                        AccountName = name,
                        Type = Normalize(source.Fields[3], result),
                        SubsidiaryFlag = Normalize(source.Fields[4], result),
                        TaxFlag = Normalize(source.Fields[5], result),
                        BalanceFlag = Normalize(source.Fields[6], result),
                        VatFlag = Normalize(source.Fields[7], result),
                        RowKind = Classify(synthetic, analytical)
                    });
                }
            }

            return result;
        }

        private static string CreateAccountCode(string synthetic, string analytical)
        {
            if (string.IsNullOrWhiteSpace(synthetic) || analytical == "****") return string.Empty;
            return synthetic + analytical;
        }

        private static AccountingFrameworkRowKind Classify(string synthetic, string analytical)
        {
            if (string.IsNullOrWhiteSpace(synthetic)) return AccountingFrameworkRowKind.Empty;
            if (analytical == "****") return AccountingFrameworkRowKind.GroupHeading;
            return string.IsNullOrWhiteSpace(analytical)
                ? AccountingFrameworkRowKind.SyntheticAccount
                : AccountingFrameworkRowKind.AnalyticalAccount;
        }

        private static string Normalize(string value, AccountingFrameworkImport result)
        {
            string normalized = JournalTextNormalizer.NormalizeText(value, out bool changed).Trim();
            if (changed || !string.Equals(value ?? string.Empty, normalized, StringComparison.Ordinal))
                result.NormalizedTextFieldCount++;
            return normalized;
        }

        private static void ParseEntity(CsvRecord record, AccountingFrameworkImport result)
        {
            string value = FindSingleValue(record);
            Match match = Regex.Match(value, @"^(?<ico>\d{8})(?:\s+(?<name>.*))?$");
            if (!match.Success)
                throw new InvalidDataException(record.Location + ": IČO was not found in the IfoSoft entity-information record.");
            result.Ico = match.Groups["ico"].Value;
            result.CompanyName = match.Groups["name"].Value.Trim();
        }

        private static void ParseTitle(CsvRecord record, AccountingFrameworkImport result)
        {
            string value = FindSingleValue(record);
            Match match = Regex.Match(value, @"^(?:Uctovny|Účtovný)\s+rozvrh\s+(?<year>\d{4})$", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new InvalidDataException(record.Location + ": expected an IfoSoft accounting-framework title, but found '" + value + "'.");
            result.FiscalYear = int.Parse(match.Groups["year"].Value);
        }

        private static string FindSingleValue(CsvRecord record)
        {
            string found = null;
            foreach (string field in record.Fields)
            {
                if (string.IsNullOrWhiteSpace(field)) continue;
                if (found != null)
                    throw new InvalidDataException(record.Location + ": expected one information value.");
                found = field.Trim();
            }
            return found ?? string.Empty;
        }

        private static void ValidateHeader(CsvRecord record)
        {
            if (record.Fields.Length != ExpectedHeaders.Length)
                throw new InvalidDataException(record.Location + ": invalid accounting-framework header.");
            for (int i = 0; i < ExpectedHeaders.Length; i++)
                if (!string.Equals(record.Fields[i].Trim(), ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException(record.Location + ": expected column '" + ExpectedHeaders[i] + "' at position " + (i + 1) + ".");
        }

        private static CsvRecord ReadRequired(IEnumerator<CsvRecord> records, string description)
        {
            if (!records.MoveNext()) throw new InvalidDataException("The IfoSoft " + description + " is missing.");
            return records.Current;
        }

        private static string CalculateSha256(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static IEnumerable<CsvRecord> ReadCsvRecords(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.GetEncoding(1250), true))
            {
                int lineNumber = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lineNumber++;
                    yield return new CsvRecord { Fields = ParseFields(line), StartLineNumber = lineNumber };
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
                    if (quoted && i + 1 < record.Length && record[i + 1] == '"') { field.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (c == ';' && !quoted) { fields.Add(field.ToString()); field.Clear(); }
                else field.Append(c);
            }
            if (quoted) throw new InvalidDataException("Line contains an unterminated quoted field.");
            fields.Add(field.ToString());
            return fields.ToArray();
        }

        private sealed class CsvRecord
        {
            public string[] Fields { get; set; }
            public int StartLineNumber { get; set; }
            public string Location => "Line " + StartLineNumber;
        }
    }
}
