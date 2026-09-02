using System;
using System.Collections.Generic;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class ExcelTableNameHelper
    {
        public static string CreateUniqueName(
            Excel.Workbook workbook,
            string sourceName)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            string baseName = "tbl" + Sanitize(sourceName);
            if (string.Equals(baseName, "tbl", StringComparison.Ordinal))
                baseName = "tblReport";

            var existingNames =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                    existingNames.Add(table.Name);
            }

            string candidate = baseName;
            int suffix = 2;

            while (existingNames.Contains(candidate))
            {
                candidate = baseName + suffix;
                suffix++;
            }

            return candidate;
        }

        private static string Sanitize(string value)
        {
            var result = new StringBuilder();

            foreach (char character in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                    result.Append(character);
            }

            const int maximumSuffixLength = 20;
            int maximumLength = 255 - maximumSuffixLength;

            if (result.Length > maximumLength)
                result.Length = maximumLength;

            return result.ToString();
        }
    }
}
