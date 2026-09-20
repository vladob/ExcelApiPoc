using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesCsvJournalParser : IIvesJournalSourceParser
    {
        public string TechnicalType => "CSV";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".csv",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesJournalParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);
            return new IvesCrystalTabularJournalDecoder().Decode(
                filePath,
                ReadRows(filePath));
        }

        private static IEnumerable<IvesCrystalTabularRow> ReadRows(
            string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var reader = new StreamReader(
                filePath,
                Encoding.GetEncoding(1250),
                true))
            {
                string firstLine = reader.ReadLine();
                if (firstLine == null)
                    yield break;

                char delimiter = DetectDelimiter(firstLine);
                int lineNumber = 1;

                yield return new IvesCrystalTabularRow
                {
                    SourceRecordNumber = lineNumber,
                    SourceLocation = Path.GetFileName(filePath) +
                        ", CSV line " + lineNumber,
                    Values = ParseFields(firstLine, delimiter)
                };

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lineNumber++;

                    yield return new IvesCrystalTabularRow
                    {
                        SourceRecordNumber = lineNumber,
                        SourceLocation = Path.GetFileName(filePath) +
                            ", CSV line " + lineNumber,
                        Values = ParseFields(line ?? string.Empty, delimiter)
                    };
                }
            }
        }

        private static char DetectDelimiter(string line)
        {
            int commas = CountDelimiter(line, ',');
            int semicolons = CountDelimiter(line, ';');

            if (commas == 0 && semicolons == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal CSV file does not contain a recognized delimiter.");
            }

            return commas >= semicolons ? ',' : ';';
        }

        private static int CountDelimiter(string line, char delimiter)
        {
            int count = 0;
            bool quoted = false;

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];

                if (character == '"')
                {
                    if (quoted &&
                        index + 1 < line.Length &&
                        line[index + 1] == '"')
                    {
                        index++;
                        continue;
                    }

                    quoted = !quoted;
                }
                else if (!quoted && character == delimiter)
                {
                    count++;
                }
            }

            return count;
        }

        private static string[] ParseFields(
            string line,
            char delimiter)
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];

                if (character == '"')
                {
                    if (quoted &&
                        index + 1 < line.Length &&
                        line[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }

                    continue;
                }

                if (!quoted && character == delimiter)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                field.Append(character);
            }

            if (quoted)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal CSV contains an unterminated quoted field.");
            }

            fields.Add(field.ToString());
            return fields.ToArray();
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A source file is required.", nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES accounting-journal CSV file was not found.",
                    filePath);
            }
        }
    }
}
