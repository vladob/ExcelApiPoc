using ExcelApiPoc.AddIn.Models;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class IfoSoftCsvJournalDetector
    {
        private const int MaximumDetectionRows = 100;

        public static bool TryDetect(string filePath,out JournalDetectionResult result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || !string.Equals(Path.GetExtension(filePath), ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            using (var reader = new StreamReader(filePath,Encoding.GetEncoding(1250),true))
            {
                string entityLine = reader.ReadLine();
                string titleLine = reader.ReadLine();
                string headerLine = reader.ReadLine();

                if (!IsIfoSoftJournal(titleLine,headerLine))
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

        private static bool IsIfoSoftJournal(string titleLine, string headerLine)
        {
            if (string.IsNullOrWhiteSpace(titleLine) || string.IsNullOrWhiteSpace(headerLine))
            {
                return false;
            }

            string normalizedTitle = titleLine.Trim().Trim('"');

            if (!string.Equals(normalizedTitle,"Uctovny dennik", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedTitle,"Účtovný denník",StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] requiredColumns =
            {
                "\"DD\"",
                "\"CisloD\"",
                "\"Datum\"",
                "\"Ucet_MD\"",
                "\"Suma_MD\"",
                "\"Ucet_D\"",
                "\"Suma_D\""
            };

            foreach (string requiredColumn in requiredColumns)
            {
                if (headerLine.IndexOf(requiredColumn, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static void DetectEntityInformation(string entityLine, JournalDetectionResult result)
        {
            if (string.IsNullOrWhiteSpace(entityLine))
            {
                return;
            }
            string normalized = entityLine.Trim().Trim('"').Trim();
            Match match = Regex.Match(normalized, @"^(?<ico>\d{8})(?:\s+(?<name>.*))?$");

            if (!match.Success)
            {
                return;
            }
            result.Ico = match.Groups["ico"].Value;

            if (match.Groups["name"].Success)
            {
                result.CompanyName = match.Groups["name"].Value.Trim();
            }
        }

        private static void DetectFiscalYear(StreamReader reader, JournalDetectionResult result)
        {
            int inspectedRows = 0;
            while (!reader.EndOfStream && inspectedRows < MaximumDetectionRows)
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

                string dateText = values[2].Trim().Trim('"');

                if (DateTime.TryParseExact(
                        dateText,
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime date))
                {
                    result.FiscalYear = date.Year;
                    return;
                }
            }
        }
    }
}