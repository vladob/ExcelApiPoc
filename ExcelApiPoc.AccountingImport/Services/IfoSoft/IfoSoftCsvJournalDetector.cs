using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    public static class IfoSoftCsvJournalDetector
    {
        private const int MaximumDetectionRows = 100;

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

        private static readonly string[] SupportedDateFormats =
        {
            "d.M.yyyy",
            "dd.MM.yyyy",
            "d.MM.yyyy",
            "dd.M.yyyy"
        };

        public static bool TryDetect(
            string filePath,
            out JournalDetectionResult result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath) ||
                !string.Equals(
                    Path.GetExtension(filePath),
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            using (var reader = new StreamReader(
                       filePath,
                       Encoding.GetEncoding(1250),
                       true))
            {
                string entityLine = reader.ReadLine();
                string titleLine = reader.ReadLine();
                string headerLine = reader.ReadLine();

                if (!IsIfoSoftJournal(titleLine, headerLine))
                {
                    return false;
                }

                result = new JournalDetectionResult
                {
                    TechnicalType = "CSV",
                    AccountingFormat = "IfoSoft"
                };

                DetectEntityInformation(entityLine, result);
                DetectFiscalYear(reader, result);
                return true;
            }
        }

        private static bool IsIfoSoftJournal(
            string titleLine,
            string headerLine)
        {
            if (string.IsNullOrWhiteSpace(titleLine) ||
                string.IsNullOrWhiteSpace(headerLine))
            {
                return false;
            }

            string normalizedTitle =
                NormalizeField(titleLine);

            if (!string.Equals(
                    normalizedTitle,
                    "Uctovny dennik",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    normalizedTitle,
                    "Účtovný denník",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] headers = headerLine
                .Split(';')
                .Select(NormalizeField)
                .ToArray();

            if (headers.Length != ExpectedHeaders.Length)
            {
                return false;
            }

            for (int index = 0; index < ExpectedHeaders.Length; index++)
            {
                if (!string.Equals(
                        headers[index],
                        ExpectedHeaders[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static void DetectEntityInformation(
            string entityLine,
            JournalDetectionResult result)
        {
            if (string.IsNullOrWhiteSpace(entityLine))
            {
                return;
            }

            string normalized =
                NormalizeField(entityLine).Trim();

            Match match = Regex.Match(
                normalized,
                @"^(?<ico>\d{8})(?:\s+(?<name>.*))?$");

            if (!match.Success)
            {
                return;
            }

            result.Ico = match.Groups["ico"].Value;

            if (match.Groups["name"].Success)
            {
                result.CompanyName =
                    match.Groups["name"].Value.Trim();
            }
        }

        private static void DetectFiscalYear(
            StreamReader reader,
            JournalDetectionResult result)
        {
            int inspectedRows = 0;

            while (!reader.EndOfStream &&
                   inspectedRows < MaximumDetectionRows)
            {
                string line = reader.ReadLine();
                inspectedRows++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] values = line.Split(';');

                // Datum is the third IfoSoft column.
                if (values.Length < 3)
                {
                    continue;
                }

                string dateText = NormalizeField(values[2]);

                if (DateTime.TryParseExact(
                        dateText,
                        SupportedDateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime date))
                {
                    result.FiscalYear = date.Year;
                    return;
                }
            }
        }

        private static string NormalizeField(string value)
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

            return normalized
                .Replace("""", """)
                .Trim();
        }
    }
}
