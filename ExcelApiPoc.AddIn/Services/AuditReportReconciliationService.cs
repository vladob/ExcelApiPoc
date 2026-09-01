using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditReportReconciliationService
    {
        private const string RegisterUzReportTablesName = "__RegisterUzReportTables";
        private const string RegisterUzReportValuesName = "__RegisterUzReportValues";
        private const int CalculationValueCount = 3;
        private const int MaximumRegisterUzDataColumns = 8;

        public static AuditReportReconciliationResult Reconcile(
            Excel.Workbook workbook,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculation)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (package == null || package.Template == null)
                throw new ArgumentNullException(nameof(package));
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));

            Dictionary<int, long> registerUzTableIdsByOrdinal =
                ReadRegisterUzReportTables(workbook);

            Dictionary<string, decimal?[]> registerUzRows =
                ReadRegisterUzReportValues(workbook);

            Dictionary<int, AuditReportTableDefinitionResponse> tablesByErpId =
                (package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
                .ToDictionary(table => table.TableErpId);

            var rowDefinitions =
                new Dictionary<string, AuditReportRowDefinitionResponse>(StringComparer.Ordinal);

            foreach (AuditReportTableDefinitionResponse table in tablesByErpId.Values)
            {
                foreach (AuditReportRowDefinitionResponse row in
                    table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                {
                    if (!row.RowNumber.HasValue) continue;

                    string key = Key(table.TableErpId, row.RowNumber.Value);

                    if (rowDefinitions.ContainsKey(key))
                        throw new InvalidOperationException(
                            $"Template contains duplicate report row '{key}'.");

                    rowDefinitions.Add(key, row);
                }
            }

            var result = new AuditReportReconciliationResult();

            foreach (AuditReportRowCalculation calculatedRow in calculation.Rows)
            {
                if (!tablesByErpId.TryGetValue(
                        calculatedRow.TableErpId,
                        out AuditReportTableDefinitionResponse table))
                {
                    throw new InvalidOperationException(
                        $"Calculation references unknown template table {calculatedRow.TableErpId}.");
                }

                string rowKey = Key(calculatedRow.TableErpId, calculatedRow.RowNumber);

                if (!rowDefinitions.TryGetValue(
                        rowKey,
                        out AuditReportRowDefinitionResponse rowDefinition))
                {
                    throw new InvalidOperationException(
                        $"Calculation references unknown template row '{rowKey}'.");
                }

                if (!table.NumberOfDataColumns.HasValue ||
                    table.NumberOfDataColumns.Value < 1)
                {
                    throw new InvalidOperationException(
                        $"Template table {table.TableErpId} does not define a valid NumberOfDataColumns.");
                }

                int comparableRegisterUzColumns = table.NumberOfDataColumns.Value - 1;

                if (comparableRegisterUzColumns < 0 ||
                    comparableRegisterUzColumns > CalculationValueCount)
                {
                    throw new InvalidOperationException(
                        $"Template table {table.TableErpId} has {table.NumberOfDataColumns.Value} data columns. " +
                        $"The current reconciliation implementation supports at most " +
                        $"{CalculationValueCount} current-year RegisterUZ columns plus one previous-year column.");
                }

                var reconciliation = new AuditReportRowReconciliation
                {
                    TableErpId = calculatedRow.TableErpId,
                    RowNumber = calculatedRow.RowNumber
                };

                if (!registerUzTableIdsByOrdinal.TryGetValue(
                        table.TableOrdinal,
                        out long registerUzTableId))
                {
                    result.Rows.Add(reconciliation);
                    continue;
                }

                string registerUzRowKey =
                    RegisterUzRowKey(registerUzTableId, rowDefinition.RowOrdinal);

                if (!registerUzRows.TryGetValue(
                        registerUzRowKey,
                        out decimal?[] registerUzValues))
                {
                    result.Rows.Add(reconciliation);
                    continue;
                }

                int firstCalculationSlot =
                    CalculationValueCount - comparableRegisterUzColumns;

                decimal[] calculatedValues =
                {
                    calculatedRow.PrimaryValue,
                    calculatedRow.SecondaryValue,
                    calculatedRow.CalculatedValue
                };

                for (int registerUzColumnIndex = 0;
                     registerUzColumnIndex < comparableRegisterUzColumns;
                     registerUzColumnIndex++)
                {
                    int calculationSlot =
                        firstCalculationSlot + registerUzColumnIndex;

                    decimal? registerUzValue =
                        registerUzValues[registerUzColumnIndex];

                    reconciliation.RegisterUzValues[calculationSlot] =
                        registerUzValue;

                    if (registerUzValue.HasValue)
                    {
                        reconciliation.Differences[calculationSlot] =
                            calculatedValues[calculationSlot] - registerUzValue.Value;
                    }
                }

                result.Rows.Add(reconciliation);
            }

            return result;
        }

        private static Dictionary<int, long> ReadRegisterUzReportTables(
            Excel.Workbook workbook)
        {
            Excel.ListObject table =
                FindTable(workbook, RegisterUzReportTablesName);

            Dictionary<string, int> columns =
                GetColumns(table, new[] { "TableId", "TableOrdinal" });

            var result = new Dictionary<int, long>();

            if (table.DataBodyRange == null)
                return result;

            int rowCount = table.DataBodyRange.Rows.Count;

            object[,] tableIds =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["TableId"]]).Value2,
                    rowCount);

            object[,] tableOrdinals =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["TableOrdinal"]]).Value2,
                    rowCount);

            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                long tableId =
                    ReadInt64(
                        tableIds[rowIndex, 1],
                        RegisterUzReportTablesName,
                        "TableId",
                        rowIndex);

                int tableOrdinal =
                    ReadInt32(
                        tableOrdinals[rowIndex, 1],
                        RegisterUzReportTablesName,
                        "TableOrdinal",
                        rowIndex);

                if (result.ContainsKey(tableOrdinal))
                {
                    throw new InvalidOperationException(
                        $"Table '{RegisterUzReportTablesName}' contains duplicate " +
                        $"TableOrdinal {tableOrdinal}.");
                }

                result.Add(tableOrdinal, tableId);
            }

            return result;
        }

        private static Dictionary<string, decimal?[]> ReadRegisterUzReportValues(
            Excel.Workbook workbook)
        {
            Excel.ListObject table =
                FindTable(workbook, RegisterUzReportValuesName);

            var requiredColumns =
                new List<string> { "TableId", "RowOrdinal" };

            for (int columnIndex = 1;
                 columnIndex <= MaximumRegisterUzDataColumns;
                 columnIndex++)
            {
                requiredColumns.Add(
                    "NumericValue" +
                    columnIndex.ToString(CultureInfo.InvariantCulture));
            }

            Dictionary<string, int> columns =
                GetColumns(table, requiredColumns);

            var result =
                new Dictionary<string, decimal?[]>(StringComparer.Ordinal);

            if (table.DataBodyRange == null)
                return result;

            int rowCount = table.DataBodyRange.Rows.Count;

            object[,] tableIds =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["TableId"]]).Value2,
                    rowCount);

            object[,] rowOrdinals =
                ToArray(
                    ((Excel.Range)table.DataBodyRange.Columns[
                        columns["RowOrdinal"]]).Value2,
                    rowCount);

            var numericColumns =
                new object[MaximumRegisterUzDataColumns][,];

            for (int columnIndex = 0;
                 columnIndex < MaximumRegisterUzDataColumns;
                 columnIndex++)
            {
                string columnName =
                    "NumericValue" +
                    (columnIndex + 1).ToString(
                        CultureInfo.InvariantCulture);

                numericColumns[columnIndex] =
                    ToArray(
                        ((Excel.Range)table.DataBodyRange.Columns[
                            columns[columnName]]).Value2,
                        rowCount);
            }

            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                long tableId =
                    ReadInt64(
                        tableIds[rowIndex, 1],
                        RegisterUzReportValuesName,
                        "TableId",
                        rowIndex);

                int rowOrdinal =
                    ReadInt32(
                        rowOrdinals[rowIndex, 1],
                        RegisterUzReportValuesName,
                        "RowOrdinal",
                        rowIndex);

                var numericValues =
                    new decimal?[MaximumRegisterUzDataColumns];

                for (int columnIndex = 0;
                     columnIndex < MaximumRegisterUzDataColumns;
                     columnIndex++)
                {
                    numericValues[columnIndex] =
                        ReadNullableDecimal(
                            numericColumns[columnIndex][rowIndex, 1]);
                }

                string key =
                    RegisterUzRowKey(tableId, rowOrdinal);

                if (result.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        $"Table '{RegisterUzReportValuesName}' contains duplicate " +
                        $"report row '{key}'.");
                }

                result.Add(key, numericValues);
            }

            return result;
        }

        private static Excel.ListObject FindTable(
            Excel.Workbook workbook,
            string tableName)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(
                            table.Name,
                            tableName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return table;
                    }
                }
            }

            throw new InvalidOperationException(
                $"Workbook does not contain required table '{tableName}'.");
        }

        private static Dictionary<string, int> GetColumns(
            Excel.ListObject table,
            IEnumerable<string> requiredColumns)
        {
            var result =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int columnIndex = 1;
                 columnIndex <= table.ListColumns.Count;
                 columnIndex++)
            {
                result.Add(table.ListColumns[columnIndex].Name, columnIndex);
            }

            foreach (string requiredColumn in requiredColumns)
            {
                if (!result.ContainsKey(requiredColumn))
                {
                    throw new InvalidOperationException(
                        $"Table '{table.Name}' does not contain required column '{requiredColumn}'.");
                }
            }

            return result;
        }

        private static int ReadInt32(
            object value,
            string tableName,
            string columnName,
            int rowIndex)
        {
            try
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
                when (
                    exception is FormatException ||
                    exception is InvalidCastException ||
                    exception is OverflowException)
            {
                throw new InvalidOperationException(
                    $"Table '{tableName}', data row {rowIndex}, column '{columnName}' " +
                    $"is not a valid integer.",
                    exception);
            }
        }

        private static long ReadInt64(
            object value,
            string tableName,
            string columnName,
            int rowIndex)
        {
            try
            {
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
                when (
                    exception is FormatException ||
                    exception is InvalidCastException ||
                    exception is OverflowException)
            {
                throw new InvalidOperationException(
                    $"Table '{tableName}', data row {rowIndex}, column '{columnName}' " +
                    $"is not a valid integer.",
                    exception);
            }
        }

        private static decimal? ReadNullableDecimal(object value)
        {
            if (value == null) return null;

            string text = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(text)) return null;

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static object[,] ToArray(
            object value,
            int rowCount)
        {
            if (value is object[,] array)
                return array;

            var result =
                new object[rowCount + 1, 2];

            result[1, 1] =
                value;

            return result;
        }

        private static string Key(int tableErpId, int rowNumber)
        {
            return tableErpId.ToString(CultureInfo.InvariantCulture) +
                   ":" +
                   rowNumber.ToString(CultureInfo.InvariantCulture);
        }

        private static string RegisterUzRowKey(long tableId, int rowOrdinal)
        {
            return tableId.ToString(CultureInfo.InvariantCulture) +
                   ":" +
                   rowOrdinal.ToString(CultureInfo.InvariantCulture);
        }
    }
}
