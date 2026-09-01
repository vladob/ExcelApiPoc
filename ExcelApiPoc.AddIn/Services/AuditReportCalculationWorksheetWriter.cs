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

        private static readonly string[] Headers =
        {
            "TableErpId",
            "ReportTable",
            "RowNumber",
            "RowCaption",
            "IsSumRow",
            "CalculatedValue1",
            "CalculatedValue2",
            "CalculatedValue3",
            "RegisterUzValue1",
            "RegisterUzValue2",
            "RegisterUzValue3",
            "Difference1",
            "Difference2",
            "Difference3"
        };

        private static readonly string[] ValueColumns =
        {
            "CalculatedValue1",
            "CalculatedValue2",
            "CalculatedValue3",
            "RegisterUzValue1",
            "RegisterUzValue2",
            "RegisterUzValue3",
            "Difference1",
            "Difference2",
            "Difference3"
        };

        public static Excel.Worksheet Write(
            Excel.Workbook workbook,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculation)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));

            AuditReportReconciliationResult reconciliation =
                AuditReportReconciliationService.Reconcile(
                    workbook,
                    package,
                    calculation);

            Excel.ListObject table = FindTable(workbook);

            if (table != null)
            {
                Refresh(table, calculation, reconciliation);
                return (Excel.Worksheet)table.Parent;
            }

            return Create(workbook, package, calculation, reconciliation);
        }

        private static void Refresh(
            Excel.ListObject table,
            AuditReportCalculationResult calculation,
            AuditReportReconciliationResult reconciliation)
        {
            Excel.Worksheet sheet = (Excel.Worksheet)table.Parent;

            if (!string.Equals(
                    sheet.Name,
                    WorksheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw Invalid(
                    $"Table '{TableName}' must be on worksheet '{WorksheetName}', " +
                    $"but it is on '{sheet.Name}'.");
            }

            if (table.DataBodyRange == null)
                throw Invalid($"Table '{TableName}' contains no report rows.");

            Dictionary<string, int> columns = GetColumns(table);

            Dictionary<string, AuditReportRowCalculation> calculated =
                IndexCalculatedRows(calculation.Rows);

            Dictionary<string, AuditReportRowReconciliation> reconciled =
                IndexReconciledRows(reconciliation.Rows);

            Dictionary<string, int> current =
                ValidateCurrentRows(table, columns, calculated, reconciled);

            object[,] previous = ReadValues(table, columns);

            try
            {
                foreach (KeyValuePair<string, int> item in current)
                {
                    AuditReportRowCalculation calculatedRow =
                        calculated[item.Key];

                    AuditReportRowReconciliation reconciliationRow =
                        reconciled[item.Key];

                    int rowIndex = item.Value;

                    WriteNullableDecimal(
                        table,
                        rowIndex,
                        columns["CalculatedValue1"],
                        calculatedRow.PrimaryValue);

                    WriteNullableDecimal(
                        table,
                        rowIndex,
                        columns["CalculatedValue2"],
                        calculatedRow.SecondaryValue);

                    WriteNullableDecimal(
                        table,
                        rowIndex,
                        columns["CalculatedValue3"],
                        calculatedRow.CalculatedValue);

                    for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                    {
                        WriteNullableDecimal(
                            table,
                            rowIndex,
                            columns[
                                "RegisterUzValue" +
                                (slotIndex + 1).ToString(
                                    CultureInfo.InvariantCulture)],
                            reconciliationRow.RegisterUzValues[slotIndex]);

                        WriteNullableDecimal(
                            table,
                            rowIndex,
                            columns[
                                "Difference" +
                                (slotIndex + 1).ToString(
                                    CultureInfo.InvariantCulture)],
                            reconciliationRow.Differences[slotIndex]);
                    }
                }
            }
            catch (Exception writeException)
            {
                try
                {
                    RestoreValues(table, columns, previous);
                }
                catch (Exception rollbackException)
                {
                    throw new InvalidOperationException(
                        "Refreshing failed and the previous calculated and " +
                        "reconciliation values could not be fully restored.",
                        new AggregateException(
                            writeException,
                            rollbackException));
                }

                throw new InvalidOperationException(
                    "Refreshing failed. The previous calculated and " +
                    "reconciliation values were restored.",
                    writeException);
            }
        }

        private static Dictionary<string, int> GetColumns(Excel.ListObject table)
        {
            var result =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int columnIndex = 1;
                 columnIndex <= table.ListColumns.Count;
                 columnIndex++)
            {
                result.Add(
                    table.ListColumns[columnIndex].Name,
                    columnIndex);
            }

            foreach (string header in Headers)
            {
                if (!result.ContainsKey(header))
                {
                    throw Invalid(
                        $"Table '{TableName}' does not contain required column '{header}'.");
                }
            }

            return result;
        }

        private static Dictionary<string, AuditReportRowCalculation>
            IndexCalculatedRows(IEnumerable<AuditReportRowCalculation> rows)
        {
            var result =
                new Dictionary<string, AuditReportRowCalculation>(StringComparer.Ordinal);

            foreach (AuditReportRowCalculation row in rows)
            {
                string key = Key(row.TableErpId, row.RowNumber);

                if (result.ContainsKey(key))
                    throw Invalid(
                        $"Calculation contains duplicate report row '{key}'.");

                result.Add(key, row);
            }

            return result;
        }

        private static Dictionary<string, AuditReportRowReconciliation>
            IndexReconciledRows(IEnumerable<AuditReportRowReconciliation> rows)
        {
            var result =
                new Dictionary<string, AuditReportRowReconciliation>(StringComparer.Ordinal);

            foreach (AuditReportRowReconciliation row in rows)
            {
                string key = Key(row.TableErpId, row.RowNumber);

                if (result.ContainsKey(key))
                    throw Invalid(
                        $"Reconciliation contains duplicate report row '{key}'.");

                result.Add(key, row);
            }

            return result;
        }

        private static Dictionary<string, int> ValidateCurrentRows(
            Excel.ListObject table,
            IReadOnlyDictionary<string, int> columns,
            IReadOnlyDictionary<string, AuditReportRowCalculation> calculated,
            IReadOnlyDictionary<string, AuditReportRowReconciliation> reconciled)
        {
            var result =
                new Dictionary<string, int>(StringComparer.Ordinal);

            int count = table.DataBodyRange.Rows.Count;

            object[,] tableIds =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["TableErpId"]]).Value2,
                    count);

            object[,] rowNumbers =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["RowNumber"]]).Value2,
                    count);

            for (int rowIndex = 1; rowIndex <= count; rowIndex++)
            {
                string key =
                    Key(
                        ReadInt(
                            tableIds[rowIndex, 1],
                            "TableErpId",
                            rowIndex),
                        ReadInt(
                            rowNumbers[rowIndex, 1],
                            "RowNumber",
                            rowIndex));

                if (result.ContainsKey(key))
                    throw Invalid(
                        $"Table '{TableName}' contains duplicate report row '{key}'.");

                if (!calculated.ContainsKey(key))
                    throw Invalid(
                        $"Table '{TableName}' contains unexpected report row '{key}'.");

                if (!reconciled.ContainsKey(key))
                    throw Invalid(
                        $"Reconciliation is missing report row '{key}'.");

                result.Add(key, rowIndex);
            }

            string missingCalculation =
                calculated.Keys.FirstOrDefault(
                    key => !result.ContainsKey(key));

            if (missingCalculation != null)
                throw Invalid(
                    $"Table '{TableName}' is missing report row '{missingCalculation}'.");

            string missingReconciliation =
                reconciled.Keys.FirstOrDefault(
                    key => !result.ContainsKey(key));

            if (missingReconciliation != null)
                throw Invalid(
                    $"Table '{TableName}' is missing reconciled report row " +
                    $"'{missingReconciliation}'.");

            return result;
        }

        private static int ReadInt(object value, string column, int row)
        {
            try
            {
                if (value == null ||
                    string.IsNullOrWhiteSpace(
                        Convert.ToString(
                            value,
                            CultureInfo.InvariantCulture)))
                {
                    throw new FormatException();
                }

                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
                when (
                    exception is FormatException ||
                    exception is InvalidCastException ||
                    exception is OverflowException)
            {
                throw new InvalidOperationException(
                    $"Table '{TableName}', data row {row}, column '{column}' " +
                    $"is not a valid integer. No cells were changed.",
                    exception);
            }
        }

        private static object[,] ReadValues(
            Excel.ListObject table,
            IReadOnlyDictionary<string, int> columns)
        {
            int count = table.DataBodyRange.Rows.Count;

            var result =
                new object[count + 1, ValueColumns.Length + 1];

            for (int columnIndex = 0;
                 columnIndex < ValueColumns.Length;
                 columnIndex++)
            {
                object[,] values =
                    ToArray(
                        ((Excel.Range)table.DataBodyRange.Columns[
                            columns[ValueColumns[columnIndex]]]).Value2,
                        count);

                for (int rowIndex = 1; rowIndex <= count; rowIndex++)
                {
                    result[rowIndex, columnIndex + 1] =
                        values[rowIndex, 1];
                }
            }

            return result;
        }

        private static void RestoreValues(
            Excel.ListObject table,
            IReadOnlyDictionary<string, int> columns,
            object[,] previous)
        {
            int count = table.DataBodyRange.Rows.Count;

            for (int columnIndex = 0;
                 columnIndex < ValueColumns.Length;
                 columnIndex++)
            {
                var values = new object[count, 1];

                for (int rowIndex = 1; rowIndex <= count; rowIndex++)
                {
                    values[rowIndex - 1, 0] =
                        previous[rowIndex, columnIndex + 1];
                }

                ((Excel.Range)table.DataBodyRange.Columns[
                    columns[ValueColumns[columnIndex]]]).Value2 =
                    values;
            }
        }

        private static Excel.Worksheet Create(
            Excel.Workbook workbook,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculation,
            AuditReportReconciliationResult reconciliation)
        {
            Excel.Worksheet sheet =
                GetOrCreateSheet(workbook, out bool created);

            try
            {
                Dictionary<string, AuditReportRowDefinitionResponse> definitions =
                    DefinitionIndex(package);

                Dictionary<int, string> names =
                    (package.Template.Tables ??
                        Array.Empty<AuditReportTableDefinitionResponse>())
                    .ToDictionary(
                        templateTable => templateTable.TableErpId,
                        templateTable => templateTable.NameSk);

                Dictionary<string, AuditReportRowReconciliation> reconciled =
                    IndexReconciledRows(reconciliation.Rows);

                AuditReportRowCalculation[] rows =
                    calculation.Rows
                    .OrderBy(row => row.TableErpId)
                    .ThenBy(row => row.RowNumber)
                    .ToArray();

                Excel.Range range =
                    sheet.Range[
                        sheet.Cells[HeaderRow, 1],
                        sheet.Cells[
                            HeaderRow + rows.Length,
                            Headers.Length]];

                if (Convert.ToDouble(
                        sheet.Application.WorksheetFunction.CountA(range)) != 0)
                {
                    throw Invalid(
                        $"Worksheet '{WorksheetName}' already contains content " +
                        $"in the range reserved for table '{TableName}'.");
                }

                var values =
                    new object[rows.Length + 1, Headers.Length];

                for (int columnIndex = 0;
                     columnIndex < Headers.Length;
                     columnIndex++)
                {
                    values[0, columnIndex] = Headers[columnIndex];
                }

                for (int rowIndex = 0;
                     rowIndex < rows.Length;
                     rowIndex++)
                {
                    AuditReportRowCalculation row = rows[rowIndex];

                    string key = Key(row.TableErpId, row.RowNumber);

                    if (!definitions.TryGetValue(
                            key,
                            out AuditReportRowDefinitionResponse definition))
                    {
                        throw Invalid(
                            $"Missing definition for calculated report row {key}.");
                    }

                    if (!reconciled.TryGetValue(
                            key,
                            out AuditReportRowReconciliation reconciliationRow))
                    {
                        throw Invalid(
                            $"Missing reconciliation for calculated report row {key}.");
                    }

                    values[rowIndex + 1, 0] = row.TableErpId;
                    values[rowIndex + 1, 1] = names[row.TableErpId];
                    values[rowIndex + 1, 2] = row.RowNumber;
                    values[rowIndex + 1, 3] = definition.TextSk;
                    values[rowIndex + 1, 4] = definition.IsSumRow;

                    values[rowIndex + 1, 5] = (double)row.PrimaryValue;
                    values[rowIndex + 1, 6] = (double)row.SecondaryValue;
                    values[rowIndex + 1, 7] = (double)row.CalculatedValue;

                    for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                    {
                        decimal? registerUzValue =
                            reconciliationRow.RegisterUzValues[slotIndex];

                        decimal? difference =
                            reconciliationRow.Differences[slotIndex];

                        values[rowIndex + 1, 8 + slotIndex] =
                            registerUzValue.HasValue
                                ? (object)(double)registerUzValue.Value
                                : null;

                        values[rowIndex + 1, 11 + slotIndex] =
                            difference.HasValue
                                ? (object)(double)difference.Value
                                : null;
                    }
                }

                range.Value2 = values;

                Excel.ListObject table =
                    sheet.ListObjects.Add(
                        Excel.XlListObjectSourceType.xlSrcRange,
                        range,
                        Type.Missing,
                        Excel.XlYesNoGuess.xlYes,
                        Type.Missing);

                table.Name = TableName;
                table.TableStyle = "TableStyleMedium2";

                Excel.Range countCell =
                    (Excel.Range)sheet.Cells[3, 1];

                countCell.Formula =
                    "=SUBTOTAL(3,CalculatedReportRows[RowNumber])";

                countCell.NumberFormat = "0";
                countCell.Font.Bold = true;

                Excel.Range body = table.DataBodyRange;

                ((Excel.Range)body.Columns[1]).NumberFormat = "0";
                ((Excel.Range)body.Columns[3]).NumberFormat = "0";

                for (int columnIndex = 6;
                     columnIndex <= 14;
                     columnIndex++)
                {
                    ((Excel.Range)body.Columns[columnIndex]).NumberFormat =
                        "#,##0.00;[Red]-#,##0.00";
                }

                double[] widths =
                {
                    14, 22, 14, 70, 12,
                    18, 18, 18,
                    18, 18, 18,
                    18, 18, 18
                };

                for (int columnIndex = 1;
                     columnIndex <= widths.Length;
                     columnIndex++)
                {
                    ((Excel.Range)sheet.Columns[columnIndex]).ColumnWidth =
                        widths[columnIndex - 1];
                }

                sheet.Visible = Excel.XlSheetVisibility.xlSheetVisible;
                sheet.Activate();

                Excel.Window window = workbook.Application.ActiveWindow;
                window.SplitRow = HeaderRow;
                window.SplitColumn = 0;
                window.FreezePanes = true;

                return sheet;
            }
            catch
            {
                if (created) sheet.Delete();
                throw;
            }
        }

        private static void WriteNullableDecimal(
            Excel.ListObject table,
            int rowIndex,
            int columnIndex,
            decimal? value)
        {
            Excel.Range cell =
                (Excel.Range)table.DataBodyRange.Cells[rowIndex, columnIndex];

            if (value.HasValue)
                cell.Value2 = (double)value.Value;
            else
                cell.ClearContents();
        }

        private static Excel.ListObject FindTable(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in sheet.ListObjects)
                {
                    if (string.Equals(
                            table.Name,
                            TableName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return table;
                    }
                }
            }

            return null;
        }

        private static Excel.Worksheet GetOrCreateSheet(
            Excel.Workbook workbook,
            out bool created)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
            {
                if (string.Equals(
                        sheet.Name,
                        WorksheetName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    created = false;
                    return sheet;
                }
            }

            Excel.Worksheet last =
                (Excel.Worksheet)workbook.Worksheets[
                    workbook.Worksheets.Count];

            Excel.Worksheet result =
                (Excel.Worksheet)workbook.Worksheets.Add(After: last);

            result.Name = WorksheetName;
            created = true;
            return result;
        }

        private static Dictionary<string, AuditReportRowDefinitionResponse>
            DefinitionIndex(AuditTemplatePackageResponse package)
        {
            var result =
                new Dictionary<string, AuditReportRowDefinitionResponse>(
                    StringComparer.Ordinal);

            foreach (AuditReportTableDefinitionResponse table in
                package.Template.Tables ??
                Array.Empty<AuditReportTableDefinitionResponse>())
            {
                foreach (AuditReportRowDefinitionResponse row in
                    table.Rows ??
                    Array.Empty<AuditReportRowDefinitionResponse>())
                {
                    if (!row.RowNumber.HasValue) continue;

                    string key =
                        Key(table.TableErpId, row.RowNumber.Value);

                    if (result.ContainsKey(key))
                    {
                        throw Invalid(
                            $"Template contains duplicate report row '{key}'.");
                    }

                    result.Add(key, row);
                }
            }

            return result;
        }

        private static object[,] ToArray(object value, int rowCount)
        {
            if (value is object[,] array) return array;

            var result = new object[rowCount + 1, 2];
            result[1, 1] = value;
            return result;
        }

        private static InvalidOperationException Invalid(string message)
        {
            return new InvalidOperationException(
                message + " No cells were changed.");
        }

        private static string Key(int tableErpId, int rowNumber)
        {
            return tableErpId + ":" + rowNumber;
        }
    }
}
