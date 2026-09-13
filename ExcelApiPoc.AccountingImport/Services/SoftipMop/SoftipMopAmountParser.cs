using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    internal static class SoftipMopAmountParser
    {
        public static decimal Parse(string value, string context)
        {
            string source = value ?? string.Empty;
            var compact = new StringBuilder(source.Length);
            foreach (char character in source.Trim())
                if (!char.IsWhiteSpace(character))
                    compact.Append(character == ',' ? '.' : character);

            string normalized = compact.ToString();
            if (normalized.Length == 0)
                throw Invalid(source, context);

            decimal result;
            if (!decimal.TryParse(
                    normalized,
                    NumberStyles.AllowLeadingSign |
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out result))
                throw Invalid(source, context);

            return result;
        }

        private static InvalidDataException Invalid(
            string value,
            string context)
        {
            return new InvalidDataException(
                context + ": '" + value +
                "' is not a valid Softip-MOP amount.");
        }
    }
}
