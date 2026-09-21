using System;
using System.Collections.Generic;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal static class IvesAccountingFrameworkParserDispatcher
    {
        private static readonly IReadOnlyList<IIvesAccountingFrameworkSourceParser> Parsers =
            new IIvesAccountingFrameworkSourceParser[]
            {
                new IvesXlsxAccountingFrameworkParser(),
                new IvesCsvAccountingFrameworkParser(),
                new IvesXmlAccountingFrameworkParser(),
                new IvesPdfAccountingFrameworkParser()
            };

        public static bool CanParse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            foreach (IIvesAccountingFrameworkSourceParser parser in Parsers)
            {
                if (parser.CanParse(filePath))
                    return true;
            }

            return false;
        }

        public static IIvesAccountingFrameworkSourceParser Select(
            string filePath)
        {
            IIvesAccountingFrameworkSourceParser selected = null;

            foreach (IIvesAccountingFrameworkSourceParser parser in Parsers)
            {
                if (!parser.CanParse(filePath))
                    continue;

                if (selected != null)
                {
                    throw new InvalidOperationException(
                        "More than one IVES accounting-framework parser recognizes file '" +
                        Path.GetFileName(filePath) +
                        "'. Parser selection is ambiguous.");
                }

                selected = parser;
            }

            if (selected == null)
            {
                throw new InvalidDataException(
                    "No IVES accounting-framework parser recognizes file '" +
                    Path.GetFileName(filePath) + "'.");
            }

            return selected;
        }
    }
}
