using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal static class IvesGeneralLedgerParserDispatcher
    {
        private static readonly IReadOnlyList<IIvesGeneralLedgerSourceParser> Parsers =
            new IIvesGeneralLedgerSourceParser[]
            {
                new IvesExcelGeneralLedgerParser(),
                new IvesXlsxGeneralLedgerParser(),
                new IvesCsvGeneralLedgerParser(),
                new IvesXmlGeneralLedgerParser(),
                new IvesPdfGeneralLedgerParser()
            };

        public static bool CanParse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            foreach (IIvesGeneralLedgerSourceParser parser in Parsers)
            {
                if (parser.CanParse(filePath))
                    return true;
            }

            return false;
        }

        public static IIvesGeneralLedgerSourceParser Select(string filePath)
        {
            IIvesGeneralLedgerSourceParser selected = null;

            foreach (IIvesGeneralLedgerSourceParser parser in Parsers)
            {
                if (!parser.CanParse(filePath))
                    continue;

                if (selected != null)
                {
                    throw new InvalidOperationException(
                        "More than one IVES general-ledger parser recognizes file '" +
                        Path.GetFileName(filePath) +
                        "'. Parser selection is ambiguous.");
                }

                selected = parser;
            }

            if (selected == null)
            {
                throw new InvalidDataException(
                    "No IVES general-ledger parser recognizes file '" +
                    Path.GetFileName(filePath) + "'.");
            }

            return selected;
        }
    }
}
