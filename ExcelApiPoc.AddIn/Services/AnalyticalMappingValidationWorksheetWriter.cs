using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingValidationWorksheetWriter
    {
        private const string WorksheetName = "__Validation";
        private const string TableName = "__AnalyticalMappingOptions";

        private static readonly string[] Headers =
        {
            "SyntheticAccountCode",
            "OptionKey",
            "DisplayCaption",
            "TableErpId",
            "ReportRowNumber",
            "SortOrder",
            "ValidationRangeName"
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook,IReadOnlyList<AnalyticalMappingOption> options)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Count == 0)
            {
                throw new InvalidOperationException("The analytical mapping does not contain validation " + "options.");
            }

            Excel.Worksheet lastWorksheet = (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets.Add(After: lastWorksheet);
            worksheet.Name = WorksheetName;

            int lastRow = options.Count + 1;
            int lastColumn = Headers.Length;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[1, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, lastColumn];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];
            Excel.Range textRange = worksheet.Range[(Excel.Range)worksheet.Cells[2, 1], (Excel.Range)worksheet.Cells[lastRow, 3]];

            textRange.NumberFormat = "@";
            tableRange.Value2 = CreateValues(options);

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);

            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";

            AddValidationNames(workbook, worksheet, options);

            // Users may unhide this worksheet to inspect the exact options.
            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;

            return worksheet;
        }

        private static object[,] CreateValues(IReadOnlyList<AnalyticalMappingOption> options)
        {
            var values = new object[options.Count + 1, Headers.Length];

            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            for (int optionIndex = 0;optionIndex < options.Count;optionIndex++)
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

        private static void AddValidationNames(Excel.Workbook workbook, Excel.Worksheet worksheet, IReadOnlyList<AnalyticalMappingOption> options)
        {
            int firstOptionRow = 2;

            foreach (IGrouping<string, AnalyticalMappingOption> group in options.GroupBy(option => option.ValidationRangeName, StringComparer.Ordinal))
            {
                int optionCount = group.Count();
                Excel.Range firstCaptionCell = (Excel.Range)worksheet.Cells[firstOptionRow, 3];
                Excel.Range lastCaptionCell = (Excel.Range)worksheet.Cells[firstOptionRow + optionCount - 1, 3];
                Excel.Range captionRange = worksheet.Range[firstCaptionCell, lastCaptionCell];

                string address = captionRange.get_Address(true, true, Excel.XlReferenceStyle.xlA1, false, Type.Missing);

                workbook.Names.Add(Name: group.Key, RefersTo: $"='{WorksheetName}'!{address}");
                firstOptionRow += optionCount;
            }
        }
    }
}
