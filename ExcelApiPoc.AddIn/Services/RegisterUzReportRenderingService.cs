using ExcelApiPoc.AddIn.Models;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzReportRenderingService
    {
        private const string ReportsTableName = "RegisterUzReports";
        private const string ImportSourcesTableName = "__ImportSources";

        public static Excel.Worksheet RenderSelectedReportTable()
        {
            Excel.Application application =
                (Excel.Application)ExcelDnaUtil.Application;
            Excel.Workbook workbook = application.ActiveWorkbook;

            if (workbook == null)
                throw new InvalidOperationException("No active workbook was found.");

            SelectedReportTable selection = ReadSelection(application);
            string ico = ReadIco(workbook);

            AccountingEntityPackageEnvelope package =
                AccountingEntityPackageApiClient.GetEnvelope(ico);

            if (!package.ReportsById.TryGetValue(
                    selection.FinancialReportId,
                    out FinancialReportEnvelope report))
            {
                throw new InvalidOperationException(
                    $"Financial report {selection.FinancialReportId} is no longer present " +
                    $"in the RegisterUZ package for IČO {ico}.");
            }

            FinancialReportTableEnvelope reportTable =
                report.Tables.FirstOrDefault(
                    x => x.Table.Id == selection.FinancialReportTableId);

            if (reportTable == null)
            {
                throw new InvalidOperationException(
                    $"Financial-report table {selection.FinancialReportTableId} is no " +
                    $"longer present in report {selection.FinancialReportId}.");
            }

            if (!report.HasTemplate)
            {
                throw new InvalidOperationException(
                    $"Template {report.Report.TemplateId} required by financial report " +
                    $"{report.Report.Id} is not available in the RegisterUZ package.");
            }

            AuditReportTableDefinitionResponse templateTable =
                FindTemplateTable(report.Template, reportTable.Table);

            Excel.Worksheet worksheet =
                RegisterUzReportWorksheetWriter.Write(
                    workbook,
                    selection.TargetWorksheetName,
                    package.Entity,
                    report,
                    reportTable,
                    templateTable);

            selection.ActionCell.Value2 = "Open";
            worksheet.Activate();
            return worksheet;
        }

        private static SelectedReportTable ReadSelection(
            Excel.Application application)
        {
            Excel.Worksheet worksheet =
                application.ActiveSheet as Excel.Worksheet;

            if (worksheet == null)
            {
                throw new InvalidOperationException(
                    "Select a row in the RegisterUZ Reports table first.");
            }

            Excel.ListObject table = null;

            foreach (Excel.ListObject candidate in worksheet.ListObjects)
            {
                if (string.Equals(
                        candidate.Name,
                        ReportsTableName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    table = candidate;
                    break;
                }
            }

            if (table == null || table.DataBodyRange == null)
            {
                throw new InvalidOperationException(
                    "Select a data row in the RegisterUZ Reports table first.");
            }

            Excel.Range activeCell = application.ActiveCell as Excel.Range;

            if (activeCell == null ||
                activeCell.Row < table.DataBodyRange.Row ||
                activeCell.Row >= table.DataBodyRange.Row +
                    table.DataBodyRange.Rows.Count ||
                activeCell.Column < table.Range.Column ||
                activeCell.Column >= table.Range.Column + table.Range.Columns.Count)
            {
                throw new InvalidOperationException(
                    "Select a data row in the RegisterUZ Reports table first.");
            }

            int rowIndex =
                activeCell.Row - table.DataBodyRange.Row + 1;

            return new SelectedReportTable
            {
                TargetWorksheetName = ReadRequiredString(
                    table,
                    rowIndex,
                    "TargetWorksheetName"),
                FinancialReportId = ReadRequiredInt64(
                    table,
                    rowIndex,
                    "FinancialReportId"),
                FinancialReportTableId = ReadRequiredInt64(
                    table,
                    rowIndex,
                    "FinancialReportTableId"),
                ActionCell = GetCell(
                    table,
                    rowIndex,
                    "Action")
            };
        }

        private static string ReadIco(Excel.Workbook workbook)
        {
            IReadOnlyList<IDictionary<string, object>> rows =
                AuditWorkbookTableReader.ReadRows(
                    workbook,
                    ImportSourcesTableName);

            if (rows.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Workbook table '{ImportSourcesTableName}' must contain exactly " +
                    "one import-source row.");
            }

            string ico = AuditWorkbookTableReader.GetString(rows[0], "Ico");

            if (string.IsNullOrWhiteSpace(ico))
            {
                throw new InvalidOperationException(
                    $"Workbook table '{ImportSourcesTableName}' does not contain IČO.");
            }

            return ico.Trim();
        }

        private static AuditReportTableDefinitionResponse FindTemplateTable(
            AuditTemplatePackageResponse template,
            FinancialReportTableDto reportTable)
        {
            AuditReportTableDefinitionResponse[] tables =
                template.Template?.Tables ??
                Array.Empty<AuditReportTableDefinitionResponse>();

            AuditReportTableDefinitionResponse result = null;

            if (reportTable.TemplateTableId.HasValue)
            {
                result = tables.FirstOrDefault(
                    x => x.TableErpId == reportTable.TemplateTableId.Value);
            }

            if (result == null)
            {
                result = tables.FirstOrDefault(
                    x => x.TableOrdinal == reportTable.TableOrdinal);
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Template {template.Template?.TemplateErpId} does not contain a " +
                    $"definition for report table {reportTable.Id} " +
                    $"(template table {reportTable.TemplateTableId}, ordinal " +
                    $"{reportTable.TableOrdinal}).");
            }

            return result;
        }

        private static Excel.Range GetCell(
            Excel.ListObject table,
            int rowIndex,
            string columnName)
        {
            Excel.ListColumn column;

            try
            {
                column = table.ListColumns[columnName];
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Table '{ReportsTableName}' does not contain required column " +
                    $"'{columnName}'.",
                    exception);
            }

            return (Excel.Range)column.DataBodyRange.Cells[rowIndex, 1];
        }

        private static string ReadRequiredString(
            Excel.ListObject table,
            int rowIndex,
            string columnName)
        {
            object value = GetCell(table, rowIndex, columnName).Value2;
            string result = Convert.ToString(
                value,
                CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new InvalidOperationException(
                    $"Selected RegisterUZ report row contains no '{columnName}'.");
            }

            return result.Trim();
        }

        private static long ReadRequiredInt64(
            Excel.ListObject table,
            int rowIndex,
            string columnName)
        {
            object value = GetCell(table, rowIndex, columnName).Value2;

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

                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
                when (
                    exception is FormatException ||
                    exception is InvalidCastException ||
                    exception is OverflowException)
            {
                throw new InvalidOperationException(
                    $"Selected RegisterUZ report row contains an invalid " +
                    $"'{columnName}'.",
                    exception);
            }
        }

        private sealed class SelectedReportTable
        {
            public string TargetWorksheetName { get; set; }

            public long FinancialReportId { get; set; }

            public long FinancialReportTableId { get; set; }

            public Excel.Range ActionCell { get; set; }
        }
    }
}
