using ExcelApiPoc.AddIn.Models;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class IfoSoftJournalPreflightValidator
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

        public static JournalPreflightResult Validate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Accounting journal path is empty.",nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The accounting journal was not found.",filePath);
            }

            var result = new JournalPreflightResult();

            using (var reader = new StreamReader(filePath,Encoding.GetEncoding(1250),true))
            {
                // Entity and report-title rows
                reader.ReadLine();
                reader.ReadLine();

                string headerLine = reader.ReadLine();
                ValidateHeaders(headerLine);
                int fileLineNumber = 3;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    fileLineNumber++;

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    result.SourceRows++;

                    string[] values = SplitRow(line);

                    if (values.Length != ExpectedHeaders.Length)
                    {
                        AddError(result, $"Line {fileLineNumber}: expected " + $"{ExpectedHeaders.Length} columns, " + $"but found {values.Length}.");
                        continue;
                    }

                    if (!TryParseDate(values[2], out DateTime journalDate))
                    {
                        AddError(result, $"Line {fileLineNumber}: invalid date " + $"'{CleanValue(values[2])}'.");
                        continue;
                    }
                    result.ValidRows++;

                    if (result.ValidRows > MaximumJournalRows)
                    {
                        throw new InvalidDataException( $"The accounting journal contains more than " + $"{MaximumJournalRows:N0} valid records. " + "This file size is not currently supported.");
                    }
                    UpdateDateRange(result,journalDate);
                }
            }
            result.RejectedRows = result.SourceRows - result.ValidRows;
            return result;
        }

        private static void ValidateHeaders(string headerLine)
        {
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidDataException("The IfoSoft journal header is missing.");
            }
            string[] headers = SplitRow(headerLine);

            if (headers.Length != ExpectedHeaders.Length)
            {
                throw new InvalidDataException(
                    $"The IfoSoft journal must contain " +
                    $"{ExpectedHeaders.Length} columns, but the header " +
                    $"contains {headers.Length}.");
            }

            for (int index = 0; index < ExpectedHeaders.Length; index++)
            {
                if (!string.Equals(headers[index], ExpectedHeaders[index], StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        $"Unexpected column {index + 1}. " +
                        $"Expected '{ExpectedHeaders[index]}', " +
                        $"but found '{headers[index]}'.");
                }
            }
        }

        private static string[] SplitRow(string line)
        {
            string[] values = line.Split(';');
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = CleanValue(values[index]);
            }
            return values;
        }

        private static string CleanValue(string value)
        {
            return (value ?? string.Empty).Trim().Trim('"').Trim();
        }

        private static bool TryParseDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(CleanValue(value), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static void UpdateDateRange(JournalPreflightResult result, DateTime date)
        {
            if (!result.DateFrom.HasValue || date < result.DateFrom.Value)
            {
                result.DateFrom = date;
            }

            if (!result.DateTo.HasValue || date > result.DateTo.Value)
            {
                result.DateTo = date;
            }
        }

        private static void AddError(JournalPreflightResult result, string message)
        {
            // Retain only representative errors so the message
            // does not become enormous.
            if (result.Errors.Count < 10)
            {
                result.Errors.Add(message);
            }
        }
    }
}