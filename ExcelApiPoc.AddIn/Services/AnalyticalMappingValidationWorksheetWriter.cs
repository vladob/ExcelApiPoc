using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingValidationWorksheetWriter
    {
        internal const string WorksheetName = "__Validation";
        private const string TableName = "__AnalyticalMappingOptions";
        private const string DateExceptionTableName = "__JournalDateExceptionResolutions";
        internal const string DateExceptionValidationRangeName = "__JournalDateExceptionResolutionOptions";

        private static readonly string[] Headers =
        {
            "SyntheticAccountCode", "OptionKey", "DisplayCaption", "TableErpId",
            "ReportRowNumber", "SortOrder", "ValidationRangeName"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            IReadOnlyList<AnalyticalMappingOption> options)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Count == 0)
                throw new InvalidOperationException("The analytical mapping does not contain validation options.");

            Excel.Worksheet worksheet = GetOrCreateWorksheet(workbook);
            if (!ContainsTable(worksheet, TableName))
                WriteAnalyticalOptions(workbook, worksheet, options);

            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;
            return worksheet;
        }

        public static Excel.Worksheet ReplaceAnalyticalMappingOptions(
            Excel.Workbook workbook,
            IReadOnlyList<AnalyticalMappingOption> options)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (options == null) throw new ArgumentNullException(nameof(options));

            Excel.Worksheet worksheet = GetOrCreateWorksheet(workbook);
            Excel.ListObject existing = FindTable(worksheet, TableName);
            if (existing != null)
            {
                Excel.Range validationNames = null;
                try
                {
                    validationNames = existing.ListColumns["ValidationRangeName"].DataBodyRange;
                }
                catch
                {
                    validationNames = null;
                }

                if (validationNames != null)
                {
                    object values = validationNames.Value2;
                    if (values is object[,] array)
                    {
                        var names = new HashSet<string>(StringComparer.Ordinal);
                        for (int row = 1; row <= array.GetLength(0); row++)
                        {
                            string name = Convert.ToString(array[row, 1]);
                            if (!string.IsNullOrWhiteSpace(name))
                                names.Add(name);
                        }
                        foreach (string name in names)
                            DeleteWorkbookNameIfPresent(workbook, name);
                    }
                    else
                    {
                        string name = Convert.ToString(values);
                        if (!string.IsNullOrWhiteSpace(name))
                            DeleteWorkbookNameIfPresent(workbook, name);
                    }
                }

                Excel.Range oldRange = existing.Range;
                existing.Unlist();
                oldRange.Clear();
            }

            if (options.Count > 0)
                WriteAnalyticalOptions(workbook, worksheet, options);

            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;
            return worksheet;
        }

        public static void EnsureJournalDateExceptionOptions(Excel.Workbook workbook)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));

            Excel.Worksheet worksheet = GetOrCreateWorksheet(workbook);

            if (!ContainsTable(worksheet, DateExceptionTableName))
            {
                const int firstColumn = 9; // I
                object[,] values =
                {
                    { "Key", "DisplayCaption" },
                    { "Excluded", "Row Excluded" },
                    { "OriginalIncluded", "Original Date Included" },
                    { "ModifiedIncluded", "Modified Date Included" }
                };

                Excel.Range firstCell = (Excel.Range)worksheet.Cells[1, firstColumn];
                Excel.Range lastCell = (Excel.Range)worksheet.Cells[4, firstColumn + 1];
                Excel.Range range = worksheet.Range[firstCell, lastCell];
                range.NumberFormat = "@";
                range.Value2 = values;

                Excel.ListObject table = worksheet.ListObjects.Add(
                    Excel.XlListObjectSourceType.xlSrcRange,
                    range,
                    Type.Missing,
                    Excel.XlYesNoGuess.xlYes,
                    Type.Missing);
                table.Name = DateExceptionTableName;
                table.TableStyle = "TableStyleMedium2";
            }

            Excel.ListObject optionTable = FindTable(worksheet, DateExceptionTableName);
            Excel.Range captions = optionTable.ListColumns["DisplayCaption"].DataBodyRange;
            string address = captions.get_Address(
                true, true, Excel.XlReferenceStyle.xlA1, false, Type.Missing);

            DeleteWorkbookNameIfPresent(workbook, DateExceptionValidationRangeName);
            workbook.Names.Add(
                Name: DateExceptionValidationRangeName,
                RefersTo: $"='{WorksheetName}'!{address}");

            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;
        }

        private static Excel.Worksheet GetOrCreateWorksheet(Excel.Workbook workbook)
        {
            Excel.Worksheet worksheet = FindWorksheet(workbook, WorksheetName);
            if (worksheet != null)
                return worksheet;

            Excel.Worksheet lastWorksheet =
                (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];
            worksheet = (Excel.Worksheet)workbook.Worksheets.Add(After: lastWorksheet);
            worksheet.Name = WorksheetName;
            return worksheet;
        }

        private static void WriteAnalyticalOptions(
            Excel.Workbook workbook,
            Excel.Worksheet worksheet,
            IReadOnlyList<AnalyticalMappingOption> options)
        {
            int lastRow = options.Count + 1;
            int lastColumn = Headers.Length;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[1, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, lastColumn];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            Excel.Range textRange = worksheet.Range[
                (Excel.Range)worksheet.Cells[2, 1],
                (Excel.Range)worksheet.Cells[lastRow, 3]];

            textRange.NumberFormat = "@";
            tableRange.Value2 = CreateValues(options);

            Excel.ListObject table = worksheet.ListObjects.Add(
                Excel.XlListObjectSourceType.xlSrcRange,
                tableRange,
                Type.Missing,
                Excel.XlYesNoGuess.xlYes,
                Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            AddValidationNames(workbook, worksheet, options);
        }

        private static object[,] CreateValues(IReadOnlyList<AnalyticalMappingOption> options)
        {
            var values = new object[options.Count + 1, Headers.Length];
            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
                values[0, columnIndex] = Headers[columnIndex];

            for (int optionIndex = 0; optionIndex < options.Count; optionIndex++)
            {
                AnalyticalMappingOption option = options[optionIndex];
                int rowIndex = optionIndex + 1;
                values[rowIndex, 0] = option.SyntheticAccountCode;
                values[rowIndex, 1] = option.OptionKey;
                values[rowIndex, 2] = option.DisplayCaption;
                values[rowIndex, 3] = option.TableErpId.HasValue ? (object)option.TableErpId.Value : null;
                values[rowIndex, 4] = option.ReportRowNumber.HasValue ? (object)option.ReportRowNumber.Value : null;
                values[rowIndex, 5] = option.SortOrder;
                values[rowIndex, 6] = option.ValidationRangeName;
            }
            return values;
        }

        private static void AddValidationNames(
            Excel.Workbook workbook,
            Excel.Worksheet worksheet,
            IReadOnlyList<AnalyticalMappingOption> options)
        {
            int firstOptionRow = 2;
            foreach (IGrouping<string, AnalyticalMappingOption> group in
                options.GroupBy(option => option.ValidationRangeName, StringComparer.Ordinal))
            {
                int optionCount = group.Count();
                Excel.Range firstCaptionCell = (Excel.Range)worksheet.Cells[firstOptionRow, 3];
                Excel.Range lastCaptionCell =
                    (Excel.Range)worksheet.Cells[firstOptionRow + optionCount - 1, 3];
                Excel.Range captionRange = worksheet.Range[firstCaptionCell, lastCaptionCell];
                string address = captionRange.get_Address(
                    true, true, Excel.XlReferenceStyle.xlA1, false, Type.Missing);
                DeleteWorkbookNameIfPresent(workbook, group.Key);
                workbook.Names.Add(Name: group.Key, RefersTo: $"='{WorksheetName}'!{address}");
                firstOptionRow += optionCount;
            }
        }

        private static Excel.Worksheet FindWorksheet(Excel.Workbook workbook, string name)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
                if (string.Equals(worksheet.Name, name, StringComparison.OrdinalIgnoreCase))
                    return worksheet;
            return null;
        }

        private static bool ContainsTable(Excel.Worksheet worksheet, string tableName)
        {
            return FindTable(worksheet, tableName) != null;
        }

        private static Excel.ListObject FindTable(Excel.Worksheet worksheet, string tableName)
        {
            foreach (Excel.ListObject table in worksheet.ListObjects)
                if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                    return table;
            return null;
        }

        private static void DeleteWorkbookNameIfPresent(Excel.Workbook workbook, string name)
        {
            try
            {
                workbook.Names.Item(name, Type.Missing, Type.Missing).Delete();
            }
            catch
            {
                // Name is absent.
            }
        }
    }
}
