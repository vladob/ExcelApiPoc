using System;
using System.Collections.Generic;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookTableReader
    {
        public static IReadOnlyList<IDictionary<string, object>> ReadRows(Excel.Workbook workbook, string tableName)
        {
            Excel.ListObject table = FindTable(workbook, tableName);
            var rows = new List<IDictionary<string, object>>();
            if (table.DataBodyRange == null)
                return rows;

            object[,] values = ToArray(table.DataBodyRange.Value2, table.DataBodyRange.Rows.Count, table.DataBodyRange.Columns.Count);
            string[] headers = new string[table.ListColumns.Count];

            for (int columnIndex = 1; columnIndex <= table.ListColumns.Count; columnIndex++)
                headers[columnIndex - 1] = table.ListColumns[columnIndex].Name;
            for (int rowIndex = 1; rowIndex <= table.DataBodyRange.Rows.Count; rowIndex++)
            {
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int columnIndex = 1; columnIndex <= headers.Length; columnIndex++)
                    row.Add(headers[columnIndex - 1], values[rowIndex, columnIndex]);
                rows.Add(row);
            }
            return rows;
        }

        public static bool ContainsTable(Excel.Workbook workbook, string tableName)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name is required.", nameof(tableName));

            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public static string GetString(IDictionary<string, object> row, string columnName)
        {
            object value = GetValue(row, columnName);
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }
        public static int GetInt32(IDictionary<string, object> row, string columnName)
        {
            object value = GetRequiredValue(row, columnName);
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        public static int? GetNullableInt32(IDictionary<string, object> row, string columnName)
        {
            object value = GetValue(row, columnName);
            return value == null || string.IsNullOrWhiteSpace(Convert.ToString(value))
                ? (int?)null
                : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
        public static decimal GetDecimal(IDictionary<string, object> row, string columnName)
        {
            return Convert.ToDecimal(GetRequiredValue(row, columnName), CultureInfo.InvariantCulture);
        }

        public static bool GetBoolean(IDictionary<string, object> row, string columnName)
        {
            object value = GetRequiredValue(row, columnName);

            if (value is bool booleanValue)
                return booleanValue;
            string text = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(text, "FALSE", StringComparison.OrdinalIgnoreCase))
                return false;

            return Convert.ToDouble(value, CultureInfo.InvariantCulture) != 0;
        }
        public static DateTime GetDateTime(IDictionary<string, object> row, string columnName)
        {
            object value = GetRequiredValue(row, columnName);

            if (value is DateTime dateTime)
                return dateTime;

            if (value is double serialNumber)
                return DateTime.FromOADate(serialNumber);

            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }
        public static DateTime? GetNullableDateTime(IDictionary<string, object> row, string columnName)
        {
            object value = GetValue(row, columnName);

            if (value == null || string.IsNullOrWhiteSpace(Convert.ToString(value)))
                return null;

            if (value is DateTime dateTime)
                return dateTime;

            if (value is double serialNumber)
                return DateTime.FromOADate(serialNumber);
            return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        private static Excel.ListObject FindTable(Excel.Workbook workbook, string tableName)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                        return table;
                }
            }
            throw new InvalidOperationException($"The active workbook does not contain table '{tableName}'.");
        }
        private static object GetRequiredValue(IDictionary<string, object> row, string columnName)
        {
            object value = GetValue(row, columnName);

            if (value == null || string.IsNullOrWhiteSpace(Convert.ToString(value)))
                throw new InvalidOperationException($"Column '{columnName}' contains an empty required value.");

            return value;
        }
        private static object GetValue(IDictionary<string, object> row, string columnName)
        {
            if (!row.TryGetValue(columnName, out object value))
                throw new InvalidOperationException($"The workbook table does not contain column '{columnName}'.");

            return value;
        }

        private static object[,] ToArray(object value, int rowCount, int columnCount)
        {
            if (value is object[,] array)
                return array;
            var result = new object[rowCount + 1, columnCount + 1];
            result[1, 1] = value;
            return result;
        }
    }
}
