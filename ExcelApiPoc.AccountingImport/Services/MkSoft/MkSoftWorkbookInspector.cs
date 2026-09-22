using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    public static class MkSoftWorkbookInspector
    {
        public const int GeneralLedgerFieldCount = 41;
        public const int AccountingJournalFieldCount = 251;

        private static readonly string[] GeneralLedgerRequiredHeaders =
        {
            "ucet",
            "nazov",
            "ucet3",
            "nazov3",
            "ucet1",
            "nazov1",
            "stredisko",
            "zakazka",
            "kodobratu",
            "pocstavm",
            "pocstavd",
            "obratym",
            "obratyd",
            "obratysm",
            "obratysd",
            "zostatokm",
            "zostatokd"
        };

        private static readonly string[] AccountingJournalRequiredHeaders =
        {
            "id",
            "pc",
            "typ",
            "x_doklad",
            "datum",
            "datumpokl",
            "text",
            "firmaid",
            "specialkod",
            "id2",
            "dokladid",
            "pc1",
            "ucetm",
            "ucetd",
            "ciastka1m",
            "ciastka1d",
            "ciastka2m",
            "ciastka2d",
            "ciastkam",
            "ciastkad",
            "ciastkacmm",
            "ciastkacmd",
            "menam",
            "menad",
            "strediskom",
            "strediskod",
            "zakazkam",
            "zakazkad",
            "kodobratum",
            "kodobratud",
            "popis",
            "autogen"
        };

        public static MkSoftWorkbookInspection Inspect(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A MkSoft file path is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("MkSoft workbook was not found.", filePath);

            string extension = Path.GetExtension(filePath);

            if (!string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "MkSoft spreadsheet import currently accepts original .xls files only.");
            }

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                if (reader.ResultsCount != 1)
                {
                    throw new InvalidDataException(
                        "MkSoft workbook '" + Path.GetFileName(filePath) +
                        "' must contain exactly one worksheet; found " +
                        reader.ResultsCount + ".");
                }

                string worksheetName = reader.Name;

                if (!reader.Read())
                {
                    throw new InvalidDataException(
                        "MkSoft workbook '" + Path.GetFileName(filePath) +
                        "' does not contain a header row.");
                }

                var headers = new string[reader.FieldCount];

                for (int column = 0; column < reader.FieldCount; column++)
                {
                    headers[column] =
                        (ExcelWorkbookReader.GetText(reader, column) ?? string.Empty)
                        .Trim();
                }

                MkSoftWorkbookKind kind = ClassifyHeaders(headers);

                if (kind == MkSoftWorkbookKind.Unknown)
                {
                    throw new InvalidDataException(
                        "Workbook '" + Path.GetFileName(filePath) +
                        "' does not match a supported MkSoft XLS signature. " +
                        "Found " + reader.FieldCount + " columns.");
                }

                int dataRowCount = 0;

                while (reader.Read())
                {
                    if (HasAnyValue(reader))
                        dataRowCount++;
                }

                return new MkSoftWorkbookInspection
                {
                    SourceFileName = Path.GetFileName(filePath),
                    WorksheetName = worksheetName,
                    Kind = kind,
                    FieldCount = headers.Length,
                    DataRowCount = dataRowCount,
                    Headers = headers
                };
            }
        }

        public static MkSoftWorkbookKind ClassifyHeaders(
            IReadOnlyList<string> headers)
        {
            if (MatchesSignature(
                    headers,
                    GeneralLedgerFieldCount,
                    GeneralLedgerRequiredHeaders))
            {
                return MkSoftWorkbookKind.GeneralLedger;
            }

            if (MatchesSignature(
                    headers,
                    AccountingJournalFieldCount,
                    AccountingJournalRequiredHeaders))
            {
                return MkSoftWorkbookKind.AccountingJournal;
            }

            return MkSoftWorkbookKind.Unknown;
        }

        private static bool MatchesSignature(
            IReadOnlyList<string> headers,
            int expectedFieldCount,
            IReadOnlyList<string> requiredHeaders)
        {
            if (headers == null || headers.Count != expectedFieldCount)
                return false;

            foreach (string requiredHeader in requiredHeaders)
            {
                int matches = 0;

                for (int index = 0; index < headers.Count; index++)
                {
                    if (string.Equals(
                            (headers[index] ?? string.Empty).Trim(),
                            requiredHeader,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        matches++;
                    }
                }

                if (matches != 1)
                    return false;
            }

            return true;
        }

        private static bool HasAnyValue(IExcelDataReader reader)
        {
            for (int column = 0; column < reader.FieldCount; column++)
            {
                object value = reader.GetValue(column);

                if (value == null || value == DBNull.Value)
                    continue;

                if (value is string text)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                        return true;

                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
