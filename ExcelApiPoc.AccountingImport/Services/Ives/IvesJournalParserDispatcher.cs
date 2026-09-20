using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal static class IvesJournalParserDispatcher
    {
        private static readonly IReadOnlyList<IIvesJournalSourceParser> Parsers =
            new IIvesJournalSourceParser[]
            {
                new IvesExcelJournalParser(),
                new IvesXmlJournalParser()
            };

        public static bool CanParse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            foreach (IIvesJournalSourceParser parser in Parsers)
            {
                if (parser.CanParse(filePath))
                    return true;
            }

            return false;
        }

        public static IIvesJournalSourceParser Select(string filePath)
        {
            IIvesJournalSourceParser selected = null;

            foreach (IIvesJournalSourceParser parser in Parsers)
            {
                if (!parser.CanParse(filePath))
                    continue;

                if (selected != null)
                {
                    throw new InvalidOperationException(
                        "More than one IVES accounting-journal parser recognizes file '" +
                        Path.GetFileName(filePath) +
                        "'. Parser selection is ambiguous.");
                }

                selected = parser;
            }

            if (selected == null)
            {
                throw new InvalidDataException(
                    "No IVES accounting-journal parser recognizes file '" +
                    Path.GetFileName(filePath) + "'.");
            }

            return selected;
        }
    }
}
