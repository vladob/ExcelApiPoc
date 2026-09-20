using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXlsxJournalParser : IIvesJournalSourceParser
    {
        public string TechnicalType => "Excel";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".xlsx",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesJournalParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            var rows = new List<IvesCrystalTabularRow>();
            string worksheetName;

            using (IExcelDataReader reader = ExcelWorkbookReader.Open(filePath))
            {
                worksheetName =
                    ExcelWorkbookReader.ValidateSingleWorksheet(
                        reader,
                        Path.GetFileName(filePath));

                int sourceRowNumber = 0;
                while (reader.Read())
                {
                    sourceRowNumber++;
                    var values = new string[reader.FieldCount];

                    for (int column = 0;
                         column < reader.FieldCount;
                         column++)
                    {
                        values[column] =
                            ExcelWorkbookReader.GetText(reader, column);
                    }

                    rows.Add(new IvesCrystalTabularRow
                    {
                        SourceRecordNumber = sourceRowNumber,
                        SourceLocation = Path.GetFileName(filePath) +
                            ", worksheet " +
                            (worksheetName ?? "(unnamed)") +
                            ", row " + sourceRowNumber,
                        Values = values
                    });
                }
            }

            IvesJournalParseResult result =
                new IvesCrystalTabularJournalDecoder().Decode(
                    filePath,
                    rows);

            result.WorksheetName = worksheetName;
            return result;
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A source file is required.", nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES accounting-journal XLSX file was not found.",
                    filePath);
            }
        }
    }
}
