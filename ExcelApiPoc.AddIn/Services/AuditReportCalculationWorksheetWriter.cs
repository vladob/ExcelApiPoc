using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditReportCalculationWorksheetWriter
    {
        private const string WorksheetName = "Calculation Results";
        private const string TableName = "CalculatedReportRows";
        private const int HeaderRow = 4;
        private static readonly string[] Headers = { "TableErpId", "ReportTable", "RowNumber", "RowCaption", "IsSumRow", "PrimaryValue", "SecondaryValue", "CalculatedValue" };
        private static readonly string[] ValueColumns = { "PrimaryValue", "SecondaryValue", "CalculatedValue" };

        public static Excel.Worksheet Write(Excel.Workbook workbook, AuditTemplatePackageResponse package, AuditReportCalculationResult calculation)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));

            Excel.ListObject table = FindTable(workbook);
            if (table != null)
            {
                Refresh(table, calculation);
                return (Excel.Worksheet)table.Parent;
            }
            return Create(workbook, package, calculation);
        }

        private static void Refresh(Excel.ListObject table, AuditReportCalculationResult calculation)
        {
            Excel.Worksheet sheet = (Excel.Worksheet)table.Parent;
            if (!string.Equals(sheet.Name, WorksheetName, StringComparison.OrdinalIgnoreCase))
                throw Invalid($"Table '{TableName}' must be on worksheet '{WorksheetName}', but it is on '{sheet.Name}'.");
            if (table.DataBodyRange == null)
                throw Invalid($"Table '{TableName}' contains no report rows.");

            Dictionary<string, int> columns = GetColumns(table);
            Dictionary<string, AuditReportRowCalculation> calculated = IndexCalculatedRows(calculation.Rows);
            Dictionary<string, int> current = ValidateCurrentRows(table, columns, calculated);
            object[,] previous = ReadValues(table, columns);

            try
            {
                foreach (KeyValuePair<string, int> item in current)
                {
                    AuditReportRowCalculation row = calculated[item.Key];
                    int index = item.Value;
                    ((Excel.Range)table.DataBodyRange.Cells[index, columns["PrimaryValue"]]).Value2 = (double)row.PrimaryValue;
                    ((Excel.Range)table.DataBodyRange.Cells[index, columns["SecondaryValue"]]).Value2 = (double)row.SecondaryValue;
                    ((Excel.Range)table.DataBodyRange.Cells[index, columns["CalculatedValue"]]).Value2 = (double)row.CalculatedValue;
                }
            }
            catch (Exception writeException)
            {
                try { RestoreValues(table, columns, previous); }
                catch (Exception rollbackException)
                {
                    throw new InvalidOperationException("Refreshing failed and the previous calculated values could not be fully restored.", new AggregateException(writeException, rollbackException));
                }
                throw new InvalidOperationException("Refreshing failed. The previous calculated values were restored.", writeException);
            }
        }

        private static Dictionary<string, int> GetColumns(Excel.ListObject table)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 1; i <= table.ListColumns.Count; i++) result.Add(table.ListColumns[i].Name, i);
            foreach (string header in Headers)
                if (!result.ContainsKey(header)) throw Invalid($"Table '{TableName}' does not contain required column '{header}'.");
            return result;
        }

        private static Dictionary<string, AuditReportRowCalculation> IndexCalculatedRows(IEnumerable<AuditReportRowCalculation> rows)
        {
            var result = new Dictionary<string, AuditReportRowCalculation>(StringComparer.Ordinal);
            foreach (AuditReportRowCalculation row in rows)
            {
                string key = Key(row.TableErpId, row.RowNumber);
                if (result.ContainsKey(key)) throw Invalid($"Calculation contains duplicate report row '{key}'.");
                result.Add(key, row);
            }
            return result;
        }

        private static Dictionary<string, int> ValidateCurrentRows(Excel.ListObject table, IReadOnlyDictionary<string, int> columns, IReadOnlyDictionary<string, AuditReportRowCalculation> calculated)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            int count = table.DataBodyRange.Rows.Count;
            object[,] tableIds = ToArray(((Excel.Range)table.DataBodyRange.Columns[columns["TableErpId"]]).Value2, count);
            object[,] rowNumbers = ToArray(((Excel.Range)table.DataBodyRange.Columns[columns["RowNumber"]]).Value2, count);
            for (int i = 1; i <= count; i++)
            {
                string key = Key(ReadInt(tableIds[i, 1], "TableErpId", i), ReadInt(rowNumbers[i, 1], "RowNumber", i));
                if (result.ContainsKey(key)) throw Invalid($"Table '{TableName}' contains duplicate report row '{key}'.");
                if (!calculated.ContainsKey(key)) throw Invalid($"Table '{TableName}' contains unexpected report row '{key}'.");
                result.Add(key, i);
            }
            string missing = calculated.Keys.FirstOrDefault(key => !result.ContainsKey(key));
            if (missing != null) throw Invalid($"Table '{TableName}' is missing report row '{missing}'.");
            return result;
        }

        private static int ReadInt(object value, string column, int row)
        {
            try
            {
                if (value == null || string.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture))) throw new FormatException();
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception) when (exception is FormatException || exception is InvalidCastException || exception is OverflowException)
            {
                throw new InvalidOperationException($"Table '{TableName}', data row {row}, column '{column}' is not a valid integer. No cells were changed.", exception);
            }
        }

        private static object[,] ReadValues(Excel.ListObject table, IReadOnlyDictionary<string, int> columns)
        {
            int count = table.DataBodyRange.Rows.Count;
            var result = new object[count + 1, ValueColumns.Length + 1];
            for (int c = 0; c < ValueColumns.Length; c++)
            {
                object[,] values = ToArray(((Excel.Range)table.DataBodyRange.Columns[columns[ValueColumns[c]]]).Value2, count);
                for (int r = 1; r <= count; r++) result[r, c + 1] = values[r, 1];
            }
            return result;
        }

        private static void RestoreValues(Excel.ListObject table, IReadOnlyDictionary<string, int> columns, object[,] previous)
        {
            int count = table.DataBodyRange.Rows.Count;
            for (int c = 0; c < ValueColumns.Length; c++)
            {
                var values = new object[count, 1];
                for (int r = 1; r <= count; r++) values[r - 1, 0] = previous[r, c + 1];
                ((Excel.Range)table.DataBodyRange.Columns[columns[ValueColumns[c]]]).Value2 = values;
            }
        }

        private static Excel.Worksheet Create(Excel.Workbook workbook, AuditTemplatePackageResponse package, AuditReportCalculationResult calculation)
        {
            Excel.Worksheet sheet = GetOrCreateSheet(workbook, out bool created);
            try
            {
                Dictionary<string, AuditReportRowDefinitionResponse> definitions = DefinitionIndex(package);
                Dictionary<int, string> names = (package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>()).ToDictionary(x => x.TableErpId, x => x.NameSk);
                AuditReportRowCalculation[] rows = calculation.Rows.OrderBy(x => x.TableErpId).ThenBy(x => x.RowNumber).ToArray();
                Excel.Range range = sheet.Range[sheet.Cells[HeaderRow, 1], sheet.Cells[HeaderRow + rows.Length, Headers.Length]];
                if (Convert.ToDouble(sheet.Application.WorksheetFunction.CountA(range)) != 0)
                    throw Invalid($"Worksheet '{WorksheetName}' already contains content in the range reserved for table '{TableName}'.");

                var values = new object[rows.Length + 1, Headers.Length];
                for (int c = 0; c < Headers.Length; c++) values[0, c] = Headers[c];
                for (int r = 0; r < rows.Length; r++)
                {
                    AuditReportRowCalculation row = rows[r];
                    string key = Key(row.TableErpId, row.RowNumber);
                    if (!definitions.TryGetValue(key, out AuditReportRowDefinitionResponse definition)) throw Invalid($"Missing definition for calculated report row {key}.");
                    values[r + 1, 0] = row.TableErpId; values[r + 1, 1] = names[row.TableErpId]; values[r + 1, 2] = row.RowNumber;
                    values[r + 1, 3] = definition.TextSk; values[r + 1, 4] = definition.IsSumRow;
                    values[r + 1, 5] = (double)row.PrimaryValue; values[r + 1, 6] = (double)row.SecondaryValue; values[r + 1, 7] = (double)row.CalculatedValue;
                }
                range.Value2 = values;
                Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, range, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
                table.Name = TableName; table.TableStyle = "TableStyleMedium2";
                Excel.Range countCell = (Excel.Range)sheet.Cells[3, 1]; countCell.Formula = "=SUBTOTAL(3,CalculatedReportRows[RowNumber])"; countCell.NumberFormat = "0"; countCell.Font.Bold = true;
                Excel.Range body = table.DataBodyRange; ((Excel.Range)body.Columns[1]).NumberFormat = "0"; ((Excel.Range)body.Columns[3]).NumberFormat = "0";
                for (int c = 6; c <= 8; c++) ((Excel.Range)body.Columns[c]).NumberFormat = "#,##0.00;[Red]-#,##0.00";
                double[] widths = { 14, 22, 14, 70, 12, 18, 18, 18 }; for (int c = 1; c <= widths.Length; c++) ((Excel.Range)sheet.Columns[c]).ColumnWidth = widths[c - 1];
                sheet.Visible = Excel.XlSheetVisibility.xlSheetVisible; sheet.Activate();
                Excel.Window window = workbook.Application.ActiveWindow; window.SplitRow = HeaderRow; window.SplitColumn = 0; window.FreezePanes = true;
                return sheet;
            }
            catch { if (created) sheet.Delete(); throw; }
        }

        private static Excel.ListObject FindTable(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                foreach (Excel.ListObject table in sheet.ListObjects)
                    if (string.Equals(table.Name, TableName, StringComparison.OrdinalIgnoreCase)) return table;
            return null;
        }

        private static Excel.Worksheet GetOrCreateSheet(Excel.Workbook workbook, out bool created)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (string.Equals(sheet.Name, WorksheetName, StringComparison.OrdinalIgnoreCase)) { created = false; return sheet; }
            Excel.Worksheet last = (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];
            Excel.Worksheet result = (Excel.Worksheet)workbook.Worksheets.Add(After: last); result.Name = WorksheetName; created = true; return result;
        }

        private static Dictionary<string, AuditReportRowDefinitionResponse> DefinitionIndex(AuditTemplatePackageResponse package)
        {
            var result = new Dictionary<string, AuditReportRowDefinitionResponse>(StringComparer.Ordinal);
            foreach (AuditReportTableDefinitionResponse table in package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
                foreach (AuditReportRowDefinitionResponse row in table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                    if (row.RowNumber.HasValue) result.Add(Key(table.TableErpId, row.RowNumber.Value), row);
            return result;
        }

        private static object[,] ToArray(object value, int rowCount)
        {
            if (value is object[,] array) return array;
            var result = new object[rowCount + 1, 2]; result[1, 1] = value; return result;
        }

        private static InvalidOperationException Invalid(string message) { return new InvalidOperationException(message + " No cells were changed."); }
        private static string Key(int tableErpId, int rowNumber) { return tableErpId + ":" + rowNumber; }
    }
}
