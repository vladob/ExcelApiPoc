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
    internal sealed class IfoSoftCsvJournalImporter : IJournalImporter
    {
        public const int MaximumJournalRows = 500_000;

        private static readonly string[] ExpectedHeaders =
        {
            "DD",
            "CisloD",
            "Datum",
            "Popis operacie",
            "Ucet_MD",
            "Pol_MD",
            "Zdr_MD",
            "Str_MD",
            "Zak_MD",
            "Suma_MD",
            "Ucet_D",
            "Odd_D",
            "Pol_D",
            "Zdr_D",
            "Str_D",
            "Zak_D",
            "Suma_D"
        };

        public bool CanImport(string filePath,string accountingFormat)
        {
            return
                string.Equals(accountingFormat, "IfoSoft", StringComparison.OrdinalIgnoreCase) &&
                IfoSoftCsvJournalDetector.TryDetect(filePath, out JournalDetectionResult _);
        }

        public JournalImport Import(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The accounting journal was not found.",filePath);
            }

            if (!IfoSoftCsvJournalDetector.TryDetect(filePath, out JournalDetectionResult detection))
            {
                throw new InvalidDataException("The selected file is not a recognized " + "IfoSoft accounting journal.");
            }

            var journalImport = new JournalImport
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = CalculateSha256(filePath),
                TechnicalType = "CSV",
                AccountingFormat = "IfoSoft",
                Ico = detection.Ico,
                CompanyName = detection.CompanyName,
                FiscalYear = detection.FiscalYear ?? 0,
                ImportedAtUtc = DateTime.UtcNow
            };

            using (IEnumerator<CsvRecord> records = ReadCsvRecords(filePath).GetEnumerator())
            {
                CsvRecord entityRecord = ReadRequiredRecord( records, "entity-information record");
                CsvRecord titleRecord = ReadRequiredRecord( records, "journal-title record");
                CsvRecord headerRecord = ReadRequiredRecord( records, "journal header");

                ValidateEntityRecord(entityRecord,journalImport);
                ValidateTitleRecord(titleRecord);
                ValidateHeaderRecord(headerRecord);

                int sequenceNumber = 0;
                int sourceRecordNumber = 0;

                while (records.MoveNext())
                {
                    CsvRecord sourceRecord = records.Current;

                    if (IsEmptyRecord(sourceRecord))
                    {
                        continue;
                    }
                    sourceRecordNumber++;

                    if (sourceRecord.Fields.Length != ExpectedHeaders.Length)
                    {
                        throw new InvalidDataException($"{sourceRecord.Location}: expected " + $"{ExpectedHeaders.Length} fields, but found " + $"{sourceRecord.Fields.Length}.");
                    }
                    sequenceNumber++;

                    if (sequenceNumber > MaximumJournalRows)
                    {
                        throw new InvalidDataException( $"The accounting journal contains more than " + $"{MaximumJournalRows:N0} records. " + "This file size is not currently supported.");
                    }
                    JournalRow row = MapJournalRow(sourceRecord, sourceRecordNumber, sequenceNumber, journalImport);
                    journalImport.Rows.Add(row);
                }
            }

            if (journalImport.Rows.Count == 0)
            {
                throw new InvalidDataException("The accounting journal does not contain any records.");
            }
            return journalImport;
        }

        private static JournalRow MapJournalRow(CsvRecord source,int sourceRecordNumber,int sequenceNumber,JournalImport journalImport)
        {
            string[] fields = source.Fields;
            bool rowNormalized = false;
            DateTime postingDate = ParseDate(fields[2], source.Location);
            if (journalImport.FiscalYear == 0)
            {
                journalImport.FiscalYear = postingDate.Year;
            }

            var row = new JournalRow
            {
                SequenceNumber = sequenceNumber,
                SourceRecordNumber = sourceRecordNumber,
                SourceStartLineNumber = source.StartLineNumber,
                SourceEndLineNumber = source.EndLineNumber,
                SourceLocation = source.Location,
                DocumentType = Normalize(fields[0], journalImport, ref rowNormalized),
                DocumentNumber = Normalize(fields[1], journalImport, ref rowNormalized),
                PostingDate = postingDate,
                Description = Normalize(fields[3], journalImport, ref rowNormalized),
                DebitAccount = Normalize(fields[4], journalImport, ref rowNormalized),
                DebitItem = Normalize(fields[5], journalImport, ref rowNormalized),
                DebitFundingSource = Normalize(fields[6], journalImport, ref rowNormalized),
                DebitCostCenter = Normalize(fields[7], journalImport, ref rowNormalized),
                DebitOrder = Normalize(fields[8], journalImport, ref rowNormalized),
                DebitAmount = ParseNullableDecimal( fields[9], source.Location, "debit amount"),
                CreditAccount = Normalize(fields[10], journalImport, ref rowNormalized),
                CreditSection =Normalize(fields[11], journalImport, ref rowNormalized),
                CreditItem = Normalize(fields[12], journalImport, ref rowNormalized),
                CreditFundingSource = Normalize(fields[13], journalImport, ref rowNormalized),
                CreditCostCenter = Normalize(fields[14], journalImport, ref rowNormalized),
                CreditOrder = Normalize(fields[15], journalImport, ref rowNormalized),
                CreditAmount = ParseNullableDecimal(fields[16], source.Location, "credit amount")
            };
            row.TextNormalizationApplied = rowNormalized;
            row.RecordKind = ClassifyRecord(row);
            return row;
        }

        private static JournalRecordKind ClassifyRecord(JournalRow row)
        {
            if (IsOpeningRecord(row))
                return JournalRecordKind.Opening;

            if (IsClosingRecord(row))
                return JournalRecordKind.Closing;

            return JournalRecordKind.Normal;
        }

        private static bool IsOpeningRecord(JournalRow row)
        {
            return
                row.PostingDate.Month == 1 &&
                row.PostingDate.Day == 1 &&
                (IsSyntheticAccount(row.DebitAccount, "701") ||
                 IsSyntheticAccount(row.CreditAccount, "701"));
        }

        private static bool IsClosingRecord(JournalRow row)
        {
            if (row.PostingDate.Month != 12 ||
                row.PostingDate.Day != 31)
            {
                return false;
            }

            return
                IsIfoSoftClosingOperation(row) ||
                IsClosingAccount(row.DebitAccount) ||
                IsClosingAccount(row.CreditAccount);
        }

        private static bool IsIfoSoftClosingOperation(JournalRow row)
        {
            if (!string.Equals(row.DocumentType, "ID", StringComparison.OrdinalIgnoreCase))
                return false;

            string description = row.Description ?? string.Empty;
            switch (row.DocumentNumber)
            {
                case "999990":
                    return description.StartsWith(
                        "Uzatvorenie účtov - PRÍJEM/VÝDAJ za",
                        StringComparison.OrdinalIgnoreCase);
                case "999998":
                    return description.StartsWith(
                        "Výsledok hospodárenia za",
                        StringComparison.OrdinalIgnoreCase);
                case "999999":
                    return description.StartsWith(
                        "Uzatvorenie účtovných kníh",
                        StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        private static bool IsClosingAccount(string account)
        {
            return
                IsSyntheticAccount(account, "702") ||
                IsSyntheticAccount(account, "710");
        }

        private static bool IsSyntheticAccount(
            string account,
            string syntheticAccount)
        {
            if (string.IsNullOrWhiteSpace(account) ||
                account.Length < 3)
            {
                return false;
            }

            return string.Equals(
                account.Substring(0, 3),
                syntheticAccount,
                StringComparison.Ordinal);
        }

        private static string Normalize(string value, JournalImport journalImport, ref bool rowNormalized)
        {
            string normalized = JournalTextNormalizer.NormalizeText(value, out bool changed);
            if (changed)
            {
                rowNormalized = true;
                journalImport.NormalizedTextFieldCount++;
            }
            return normalized;
        }

        private static DateTime ParseDate(string value, string sourceLocation)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (!DateTime.TryParseExact(normalized, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                throw new InvalidDataException($"{sourceLocation}: '{normalized}' is not " + "a valid IfoSoft posting date.");
            }
            return result;
        }

        private static decimal? ParseNullableDecimal(string value,string sourceLocation,string fieldDescription)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }
            var slovakCulture = CultureInfo.GetCultureInfo("sk-SK");
            if (!decimal.TryParse(normalized, NumberStyles.Number, slovakCulture, out decimal result))
            {
                throw new InvalidDataException($"{sourceLocation}: '{normalized}' is not a " + $"valid {fieldDescription}.");
            }
            return result;
        }

        private static void ValidateEntityRecord(CsvRecord record, JournalImport journalImport)
        {
            if (record.Fields.Length != 1)
            {
                throw new InvalidDataException($"{record.Location}: invalid IfoSoft " + "entity-information record.");
            }
            string value = record.Fields[0].Trim();
            Match match = Regex.Match(value, @"^(?<ico>\d{8})(?:\s+(?<name>.*))?$");

            if (!match.Success)
            {
                throw new InvalidDataException($"{record.Location}: IČO was not found in " + "the IfoSoft entity-information record.");
            }
            journalImport.Ico = match.Groups["ico"].Value;

            if (match.Groups["name"].Success)
            {
                journalImport.CompanyName = match.Groups["name"].Value.Trim();
            }
        }

        private static void ValidateTitleRecord(CsvRecord record)
        {
            if (record.Fields.Length != 1)
            {
                throw new InvalidDataException($"{record.Location}: invalid IfoSoft " + "journal-title record.");
            }
            string title = record.Fields[0].Trim();
            if (!string.Equals(title, "Uctovny dennik", StringComparison.OrdinalIgnoreCase) && !string.Equals(title, "Účtovný denník", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"{record.Location}: expected the IfoSoft " + "journal title, but found '" + title + "'.");
            }
        }

        private static void ValidateHeaderRecord(CsvRecord record)
        {
            if (record.Fields.Length != ExpectedHeaders.Length)
            {
                throw new InvalidDataException($"{record.Location}: expected " + $"{ExpectedHeaders.Length} header fields, " + $"but found {record.Fields.Length}.");
            }

            for (int index = 0; index < ExpectedHeaders.Length; index++)
            {
                if (!string.Equals(record.Fields[index].Trim(), ExpectedHeaders[index], StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"{record.Location}: expected column " + $"'{ExpectedHeaders[index]}' at position " + $"{index + 1}, but found " + $"'{record.Fields[index]}'.");
                }
            }
        }

        private static bool IsEmptyRecord(CsvRecord record)
        {
            return record.Fields.Length == 1 && string.IsNullOrWhiteSpace(record.Fields[0]);
        }

        private static CsvRecord ReadRequiredRecord(IEnumerator<CsvRecord> records,string description)
        {
            if (!records.MoveNext())
            {
                throw new InvalidDataException($"The IfoSoft {description} is missing.");
            }
            return records.Current;
        }

        private static string CalculateSha256(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        private static IEnumerable<CsvRecord> ReadCsvRecords(string filePath)
        {
            using (var reader = new StreamReader(filePath,Encoding.GetEncoding(1250),true))
            {
                var recordBuilder = new StringBuilder();
                int physicalLineNumber = 0;
                int recordStartLineNumber = 0;
                bool insideQuotes = false;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    physicalLineNumber++;

                    if (recordBuilder.Length == 0)
                    {
                        recordStartLineNumber = physicalLineNumber;
                    }
                    else
                    {
                        recordBuilder.Append("\r\n");
                    }
                    recordBuilder.Append(line);

                    UpdateQuoteState(line, ref insideQuotes);

                    if (!insideQuotes)
                    {
                        yield return CreateCsvRecord(recordBuilder.ToString(), recordStartLineNumber, physicalLineNumber);
                        recordBuilder.Clear();
                    }
                }

                if (insideQuotes)
                {
                    throw new InvalidDataException($"CSV record beginning at line " + $"{recordStartLineNumber} has an " + "unterminated quoted field.");
                }

                if (recordBuilder.Length > 0)
                {
                    yield return CreateCsvRecord(recordBuilder.ToString(), recordStartLineNumber, physicalLineNumber);
                }
            }
        }

        private static void UpdateQuoteState(string line, ref bool insideQuotes)
        {
            for (int index = 0; index < line.Length; index++)
            {
                if (line[index] != '"')
                {
                    continue;
                }
                if (insideQuotes &&index + 1 < line.Length && line[index + 1] == '"')
                {
                    index++;
                    continue;
                }
                insideQuotes = !insideQuotes;
            }
        }

        private static CsvRecord CreateCsvRecord(string rawRecord, int startLineNumber, int endLineNumber)
        {
            return new CsvRecord
            {
                Fields = ParseFields(rawRecord),
                StartLineNumber = startLineNumber,
                EndLineNumber = endLineNumber
            };
        }

        private static string[] ParseFields(string rawRecord)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool insideQuotes = false;

            for (int index = 0; index < rawRecord.Length; index++)
            {
                char character = rawRecord[index];
                if (character == '"')
                {
                    if (insideQuotes && index + 1 < rawRecord.Length && rawRecord[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                    continue;
                }
                if (character == ';' && !insideQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }
                if ((character == '\r' || character == '\n') && !insideQuotes)
                {
                    continue;
                }
                field.Append(character);
            }

            if (insideQuotes)
            {
                throw new InvalidDataException("The CSV record contains an unterminated " + "quoted field.");
            }
            fields.Add(field.ToString());
            return fields.ToArray();
        }

        private sealed class CsvRecord
        {
            public string[] Fields { get; set; }
            public int StartLineNumber { get; set; }
            public int EndLineNumber { get; set; }
            public string Location => StartLineNumber == EndLineNumber ? $"Line {StartLineNumber}" : $"Lines {StartLineNumber}–{EndLineNumber}";
        }
    }
}
