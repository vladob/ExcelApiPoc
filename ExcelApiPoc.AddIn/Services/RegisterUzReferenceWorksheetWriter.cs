using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzReferenceWorksheetWriter
    {
        private const string WorksheetName = "__RegisterUzReference";
        private const string ReportTablesTableName = "__RegisterUzReportTables";
        private const string ReportValuesTableName = "__RegisterUzReportValues";

        private const int MaximumDataColumns = 8;

        private static readonly string[] ReportTableHeaders =
        {
            "FinancialStatementId",
            "FinancialReportId",
            "TemplateId",
            "PeriodFrom",
            "PeriodTo",
            "TableId",
            "TemplateTableId",
            "TableOrdinal",
            "TableNameSk"
        };

        private static readonly string[] ReportValueHeaders =
        {
            "TableId",
            "RowOrdinal",
            "NumericValue1",
            "NumericValue2",
            "NumericValue3",
            "NumericValue4",
            "NumericValue5",
            "NumericValue6",
            "NumericValue7",
            "NumericValue8"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            RegisterUzFinancialReportSelection selection)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook));
            }

            if (selection == null)
            {
                throw new ArgumentNullException(nameof(selection));
            }

            if (selection.Statement?.Statement == null)
            {
                throw new InvalidOperationException(
                    "RegisterUZ report selection does not contain a financial statement.");
            }

            if (selection.Report?.Report == null)
            {
                throw new InvalidOperationException(
                    "RegisterUZ report selection does not contain a financial report.");
            }

            FinancialStatementDto statement =
                selection.Statement.Statement;

            FinancialReportEnvelope reportEnvelope =
                selection.Report;

            FinancialReportDto report =
                reportEnvelope.Report;

            Excel.Worksheet worksheet =
                AddWorksheet(workbook);

            int nextRow = 1;

            nextRow =
                WriteReportTables(
                    worksheet,
                    statement,
                    reportEnvelope,
                    nextRow);

            nextRow += 2;

            WriteReportValues(
                worksheet,
                reportEnvelope,
                nextRow);

            worksheet.Visible =
                Excel.XlSheetVisibility.xlSheetHidden;

            return worksheet;
        }

        private static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook)
        {
            Excel.Worksheet lastWorksheet =
                (Excel.Worksheet)workbook.Worksheets[
                    workbook.Worksheets.Count];

            Excel.Worksheet worksheet =
                (Excel.Worksheet)workbook.Worksheets.Add(
                    After: lastWorksheet);

            worksheet.Name =
                WorksheetName;

            return worksheet;
        }

        private static int WriteReportTables(
            Excel.Worksheet worksheet,
            FinancialStatementDto statement,
            FinancialReportEnvelope reportEnvelope,
            int firstRow)
        {
            FinancialReportDto report =
                reportEnvelope.Report;

            int dataRowCount =
                reportEnvelope.Tables.Count;

            object[,] values =
                new object[
                    dataRowCount + 1,
                    ReportTableHeaders.Length];

            for (int columnIndex = 0;
                 columnIndex < ReportTableHeaders.Length;
                 columnIndex++)
            {
                values[0, columnIndex] =
                    ReportTableHeaders[columnIndex];
            }

            for (int rowIndex = 0;
                 rowIndex < dataRowCount;
                 rowIndex++)
            {
                FinancialReportTableDto table =
                    reportEnvelope.Tables[rowIndex].Table;

                values[rowIndex + 1, 0] =
                    statement.Id;

                values[rowIndex + 1, 1] =
                    report.Id;

                values[rowIndex + 1, 2] =
                    report.TemplateId.HasValue
                        ? (object)report.TemplateId.Value
                        : null;

                values[rowIndex + 1, 3] =
                    statement.PeriodFrom;

                values[rowIndex + 1, 4] =
                    statement.PeriodTo;

                values[rowIndex + 1, 5] =
                    table.Id;

                values[rowIndex + 1, 6] =
                    table.TemplateTableId.HasValue
                        ? (object)table.TemplateTableId.Value
                        : null;

                values[rowIndex + 1, 7] =
                    table.TableOrdinal;

                values[rowIndex + 1, 8] =
                    table.NameSk;
            }

            int lastRow =
                firstRow + dataRowCount;

            Excel.Range firstCell =
                (Excel.Range)worksheet.Cells[
                    firstRow,
                    1];

            Excel.Range lastCell =
                (Excel.Range)worksheet.Cells[
                    lastRow,
                    ReportTableHeaders.Length];

            Excel.Range tableRange =
                worksheet.Range[
                    firstCell,
                    lastCell];

            if (dataRowCount > 0)
            {
                Excel.Range periodFromRange =
                    worksheet.Range[
                        worksheet.Cells[firstRow + 1, 4],
                        worksheet.Cells[lastRow, 4]];

                Excel.Range periodToRange =
                    worksheet.Range[
                        worksheet.Cells[firstRow + 1, 5],
                        worksheet.Cells[lastRow, 5]];

                Excel.Range tableNameRange =
                    worksheet.Range[
                        worksheet.Cells[firstRow + 1, 9],
                        worksheet.Cells[lastRow, 9]];

                periodFromRange.NumberFormat = "@";
                periodToRange.NumberFormat = "@";
                tableNameRange.NumberFormat = "@";
            }

            tableRange.Value2 =
                values;

            Excel.ListObject excelTable =
                worksheet.ListObjects.Add(
                    Excel.XlListObjectSourceType.xlSrcRange,
                    tableRange,
                    Type.Missing,
                    Excel.XlYesNoGuess.xlYes,
                    Type.Missing);

            excelTable.Name =
                ReportTablesTableName;

            excelTable.TableStyle =
                "TableStyleMedium2";

            return lastRow + 1;
        }

        private static void WriteReportValues(
            Excel.Worksheet worksheet,
            FinancialReportEnvelope reportEnvelope,
            int firstRow)
        {
            List<RegisterUzReportValueRow> rows =
                BuildReportValueRows(
                    reportEnvelope);

            object[,] values =
                new object[
                    rows.Count + 1,
                    ReportValueHeaders.Length];

            for (int columnIndex = 0;
                 columnIndex < ReportValueHeaders.Length;
                 columnIndex++)
            {
                values[0, columnIndex] =
                    ReportValueHeaders[columnIndex];
            }

            for (int rowIndex = 0;
                 rowIndex < rows.Count;
                 rowIndex++)
            {
                RegisterUzReportValueRow row =
                    rows[rowIndex];

                values[rowIndex + 1, 0] =
                    row.TableId;

                values[rowIndex + 1, 1] =
                    row.RowOrdinal;

                for (int dataColumnIndex = 0;
                     dataColumnIndex < MaximumDataColumns;
                     dataColumnIndex++)
                {
                    values[rowIndex + 1, dataColumnIndex + 2] =
                        row.NumericValues[dataColumnIndex].HasValue
                            ? (object)row.NumericValues[dataColumnIndex].Value
                            : null;
                }
            }

            int lastRow =
                firstRow + rows.Count;

            Excel.Range firstCell =
                (Excel.Range)worksheet.Cells[
                    firstRow,
                    1];

            Excel.Range lastCell =
                (Excel.Range)worksheet.Cells[
                    lastRow,
                    ReportValueHeaders.Length];

            Excel.Range tableRange =
                worksheet.Range[
                    firstCell,
                    lastCell];

            tableRange.Value2 =
                values;

            Excel.ListObject excelTable =
                worksheet.ListObjects.Add(
                    Excel.XlListObjectSourceType.xlSrcRange,
                    tableRange,
                    Type.Missing,
                    Excel.XlYesNoGuess.xlYes,
                    Type.Missing);

            excelTable.Name =
                ReportValuesTableName;

            excelTable.TableStyle =
                "TableStyleMedium2";
        }

        private static List<RegisterUzReportValueRow>
            BuildReportValueRows(
                FinancialReportEnvelope reportEnvelope)
        {
            var rows =
                new List<RegisterUzReportValueRow>();

            foreach (FinancialReportTableEnvelope tableEnvelope
                     in reportEnvelope.Tables)
            {
                FinancialReportTableDto table =
                    tableEnvelope.Table;

                var rowsByOrdinal =
                    new SortedDictionary<int, RegisterUzReportValueRow>();

                foreach (FinancialReportValueDto value
                         in tableEnvelope.Values)
                {
                    if (value.DataColumnOrdinal < 0 ||
                        value.DataColumnOrdinal >= MaximumDataColumns)
                    {
                        throw new InvalidOperationException(
                            $"RegisterUZ table {table.Id} contains data-column ordinal " +
                            $"{value.DataColumnOrdinal}, but the workbook reference schema " +
                            $"supports only {MaximumDataColumns} data columns.");
                    }

                    RegisterUzReportValueRow row;

                    if (!rowsByOrdinal.TryGetValue(
                            value.RowOrdinal,
                            out row))
                    {
                        row =
                            new RegisterUzReportValueRow(
                                table.Id,
                                value.RowOrdinal);

                        rowsByOrdinal.Add(
                            value.RowOrdinal,
                            row);
                    }

                    if (row.NumericValues[value.DataColumnOrdinal].HasValue)
                    {
                        throw new InvalidOperationException(
                            $"RegisterUZ table {table.Id}, row {value.RowOrdinal}, " +
                            $"data column {value.DataColumnOrdinal} contains duplicate values.");
                    }

                    row.NumericValues[value.DataColumnOrdinal] =
                        value.NumericValue;
                }

                foreach (RegisterUzReportValueRow row
                         in rowsByOrdinal.Values)
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        private sealed class RegisterUzReportValueRow
        {
            public RegisterUzReportValueRow(
                long tableId,
                int rowOrdinal)
            {
                TableId =
                    tableId;

                RowOrdinal =
                    rowOrdinal;

                NumericValues =
                    new decimal?[MaximumDataColumns];
            }

            public long TableId { get; }

            public int RowOrdinal { get; }

            public decimal?[] NumericValues { get; }
        }
    }
}
