using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzReportWorksheetWriter
    {
        private const int TitleRow = 1;
        private const int EntityRow = 2;
        private const int PeriodRow = 3;
        private const int HeaderFirstRow = 5;

        public static Excel.Worksheet Write(
            Excel.Workbook workbook,
            string worksheetName,
            AccountingEntityDto entity,
            FinancialReportEnvelope report,
            FinancialReportTableEnvelope reportTable,
            AuditReportTableDefinitionResponse templateTable)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (string.IsNullOrWhiteSpace(worksheetName))
                throw new ArgumentException("Worksheet name is required.", nameof(worksheetName));

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (reportTable == null)
                throw new ArgumentNullException(nameof(reportTable));

            if (templateTable == null)
                throw new ArgumentNullException(nameof(templateTable));

            ValidateWorksheetName(worksheetName);

            int totalColumns = GetTotalColumnCount(templateTable);
            int dataColumnCount = GetDataColumnCount(templateTable);
            int descriptiveColumnCount = totalColumns - dataColumnCount;
            int headerRowCount = GetHeaderRowCount(templateTable);

            AuditReportRowDefinitionResponse[] rows =
                (templateTable.Rows ??
                    Array.Empty<AuditReportRowDefinitionResponse>())
                .OrderBy(x => x.RowOrdinal)
                .ToArray();

            ValidateTemplate(
                templateTable,
                rows,
                totalColumns,
                dataColumnCount,
                headerRowCount);

            Dictionary<string, decimal> values =
                BuildValueIndex(
                    reportTable,
                    rows,
                    dataColumnCount);

            Excel.Worksheet worksheet =
                FindWorksheet(workbook, worksheetName);
            bool created = worksheet == null;
            double[] previousWidths = null;

            if (created)
            {
                Excel.Worksheet activeWorksheet =
                    workbook.Application.ActiveSheet as Excel.Worksheet;

                worksheet = activeWorksheet == null
                    ? (Excel.Worksheet)workbook.Worksheets.Add(
                        After: workbook.Worksheets[workbook.Worksheets.Count])
                    : (Excel.Worksheet)workbook.Worksheets.Add(
                        After: activeWorksheet);

                worksheet.Name = worksheetName;
            }
            else
            {
                previousWidths = CaptureColumnWidths(
                    worksheet,
                    totalColumns);

                Excel.Range usedRange = worksheet.UsedRange;
                usedRange.UnMerge();
                usedRange.Clear();
            }

            int firstDataRow = HeaderFirstRow + headerRowCount;
            int lastRow = firstDataRow + rows.Length - 1;
            int renderedLastRow = Math.Max(lastRow, HeaderFirstRow + headerRowCount - 1);

            Excel.Range renderedRange =
                worksheet.Range[
                    worksheet.Cells[TitleRow, 1],
                    worksheet.Cells[renderedLastRow, totalColumns]];

            renderedRange.Value2 = BuildValues(
                entity,
                report,
                reportTable,
                templateTable,
                rows,
                values,
                totalColumns,
                dataColumnCount,
                descriptiveColumnCount,
                headerRowCount,
                renderedLastRow);

            MergeHeaders(
                worksheet,
                templateTable,
                totalColumns,
                headerRowCount);

            ApplyFormatting(
                worksheet,
                templateTable,
                rows,
                totalColumns,
                dataColumnCount,
                descriptiveColumnCount,
                headerRowCount,
                firstDataRow,
                renderedLastRow);

            if (previousWidths == null)
            {
                ApplyDefaultColumnWidths(
                    worksheet,
                    totalColumns,
                    descriptiveColumnCount);
            }
            else
            {
                RestoreColumnWidths(worksheet, previousWidths);
            }

            ApplyPrintSetup(
                worksheet,
                totalColumns,
                renderedLastRow);

            worksheet.Activate();
            Excel.Window window = workbook.Application.ActiveWindow;
            window.SplitRow = firstDataRow - 1;
            window.SplitColumn = descriptiveColumnCount;
            window.FreezePanes = true;

            return worksheet;
        }

        private static object[,] BuildValues(
            AccountingEntityDto entity,
            FinancialReportEnvelope report,
            FinancialReportTableEnvelope reportTable,
            AuditReportTableDefinitionResponse templateTable,
            IReadOnlyList<AuditReportRowDefinitionResponse> rows,
            IReadOnlyDictionary<string, decimal> valueIndex,
            int totalColumns,
            int dataColumnCount,
            int descriptiveColumnCount,
            int headerRowCount,
            int renderedLastRow)
        {
            var result = new object[renderedLastRow, totalColumns];

            string templateName =
                report.Template?.Template?.Name ?? string.Empty;
            string tableName = FirstNonEmpty(
                reportTable.Table.NameSk,
                templateTable.NameSk,
                reportTable.Table.NameEn,
                templateTable.NameEn);

            result[TitleRow - 1, 0] =
                FirstNonEmpty(templateName, "RegisterUZ report") +
                " — " + tableName;

            result[EntityRow - 1, 0] =
                entity.Name + " | IČO: " + entity.Ico;

            result[PeriodRow - 1, 0] =
                "Obdobie: " + GetPeriodFrom(report) +
                " – " + GetPeriodTo(report) +
                " | Mena: " + (report.Report.CurrencyCode ?? string.Empty);

            foreach (AuditReportHeaderDefinitionResponse header
                     in templateTable.Headers ??
                        Array.Empty<AuditReportHeaderDefinitionResponse>())
            {
                int targetRow = HeaderFirstRow + header.RowPosition - 2;
                int targetColumn = header.ColumnPosition - 1;
                result[targetRow, targetColumn] =
                    ResolveHeaderText(header.TextSk, report);
            }

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                AuditReportRowDefinitionResponse row = rows[rowIndex];
                int targetRow = HeaderFirstRow + headerRowCount + rowIndex - 1;

                WriteDescriptiveValues(
                    result,
                    targetRow,
                    row,
                    descriptiveColumnCount);

                for (int dataColumnOrdinal = 0;
                     dataColumnOrdinal < dataColumnCount;
                     dataColumnOrdinal++)
                {
                    if (valueIndex.TryGetValue(
                            ValueKey(row.RowOrdinal, dataColumnOrdinal),
                            out decimal value))
                    {
                        result[
                            targetRow,
                            descriptiveColumnCount + dataColumnOrdinal] =
                            (double)value;
                    }
                }
            }

            return result;
        }

        private static void WriteDescriptiveValues(
            object[,] values,
            int targetRow,
            AuditReportRowDefinitionResponse row,
            int descriptiveColumnCount)
        {
            if (descriptiveColumnCount >= 3)
            {
                values[targetRow, 0] = row.Designation;
                values[targetRow, 1] = row.TextSk;
                values[targetRow, 2] = row.RowNumber.HasValue
                    ? (object)row.RowNumber.Value
                    : null;
                return;
            }

            if (descriptiveColumnCount == 2)
            {
                values[targetRow, 0] = row.Designation;
                values[targetRow, 1] = row.TextSk;
                return;
            }

            if (descriptiveColumnCount == 1)
            {
                values[targetRow, 0] =
                    FirstNonEmpty(row.TextSk, row.Designation);
            }
        }

        private static Dictionary<string, decimal> BuildValueIndex(
            FinancialReportTableEnvelope reportTable,
            IReadOnlyCollection<AuditReportRowDefinitionResponse> rows,
            int dataColumnCount)
        {
            var rowOrdinals =
                new HashSet<int>(rows.Select(x => x.RowOrdinal));
            var result =
                new Dictionary<string, decimal>(StringComparer.Ordinal);

            foreach (FinancialReportValueDto value in reportTable.Values)
            {
                if (!rowOrdinals.Contains(value.RowOrdinal))
                {
                    throw new InvalidOperationException(
                        $"RegisterUZ table {reportTable.Table.Id} contains value for " +
                        $"unknown row ordinal {value.RowOrdinal}.");
                }

                if (value.DataColumnOrdinal < 0 ||
                    value.DataColumnOrdinal >= dataColumnCount)
                {
                    throw new InvalidOperationException(
                        $"RegisterUZ table {reportTable.Table.Id} contains data-column " +
                        $"ordinal {value.DataColumnOrdinal}, but template table " +
                        $"{reportTable.Table.TemplateTableId} defines " +
                        $"{dataColumnCount} data columns.");
                }

                string key =
                    ValueKey(value.RowOrdinal, value.DataColumnOrdinal);

                if (result.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        $"RegisterUZ table {reportTable.Table.Id} contains duplicate " +
                        $"value for row {value.RowOrdinal}, data column " +
                        $"{value.DataColumnOrdinal}.");
                }

                result.Add(key, value.NumericValue);
            }

            return result;
        }

        private static void ValidateTemplate(
            AuditReportTableDefinitionResponse templateTable,
            IReadOnlyList<AuditReportRowDefinitionResponse> rows,
            int totalColumns,
            int dataColumnCount,
            int headerRowCount)
        {
            if (dataColumnCount > totalColumns)
            {
                throw new InvalidOperationException(
                    $"Template table {templateTable.TableErpId} defines more data " +
                    "columns than total columns.");
            }

            var rowOrdinals = new HashSet<int>();

            foreach (AuditReportRowDefinitionResponse row in rows)
            {
                if (!rowOrdinals.Add(row.RowOrdinal))
                {
                    throw new InvalidOperationException(
                        $"Template table {templateTable.TableErpId} contains duplicate " +
                        $"row ordinal {row.RowOrdinal}.");
                }
            }

            var occupied = new bool[headerRowCount, totalColumns];

            foreach (AuditReportHeaderDefinitionResponse header
                     in templateTable.Headers ??
                        Array.Empty<AuditReportHeaderDefinitionResponse>())
            {
                if (header.RowPosition < 1 ||
                    header.ColumnPosition < 1 ||
                    header.RowSpan < 1 ||
                    header.ColumnSpan < 1 ||
                    header.RowPosition + header.RowSpan - 1 > headerRowCount ||
                    header.ColumnPosition + header.ColumnSpan - 1 > totalColumns)
                {
                    throw new InvalidOperationException(
                        $"Template table {templateTable.TableErpId} contains an invalid " +
                        "header position or span.");
                }

                for (int row = header.RowPosition - 1;
                     row < header.RowPosition - 1 + header.RowSpan;
                     row++)
                {
                    for (int column = header.ColumnPosition - 1;
                         column < header.ColumnPosition - 1 + header.ColumnSpan;
                         column++)
                    {
                        if (occupied[row, column])
                        {
                            throw new InvalidOperationException(
                                $"Template table {templateTable.TableErpId} contains " +
                                "overlapping header definitions.");
                        }

                        occupied[row, column] = true;
                    }
                }
            }
        }

        private static void MergeHeaders(
            Excel.Worksheet worksheet,
            AuditReportTableDefinitionResponse templateTable,
            int totalColumns,
            int headerRowCount)
        {
            foreach (AuditReportHeaderDefinitionResponse header
                     in templateTable.Headers ??
                        Array.Empty<AuditReportHeaderDefinitionResponse>())
            {
                if (header.RowSpan == 1 && header.ColumnSpan == 1)
                    continue;

                int firstRow = HeaderFirstRow + header.RowPosition - 1;
                int firstColumn = header.ColumnPosition;
                int lastRow = firstRow + header.RowSpan - 1;
                int lastColumn = firstColumn + header.ColumnSpan - 1;

                worksheet.Range[
                    worksheet.Cells[firstRow, firstColumn],
                    worksheet.Cells[lastRow, lastColumn]].Merge();
            }
        }

        private static void ApplyFormatting(
            Excel.Worksheet worksheet,
            AuditReportTableDefinitionResponse templateTable,
            IReadOnlyList<AuditReportRowDefinitionResponse> rows,
            int totalColumns,
            int dataColumnCount,
            int descriptiveColumnCount,
            int headerRowCount,
            int firstDataRow,
            int renderedLastRow)
        {
            Excel.Range rendered =
                worksheet.Range[
                    worksheet.Cells[TitleRow, 1],
                    worksheet.Cells[renderedLastRow, totalColumns]];
            rendered.Font.Name = "Aptos Narrow";
            rendered.Font.Size = 10;

            Excel.Range title =
                worksheet.Range[
                    worksheet.Cells[TitleRow, 1],
                    worksheet.Cells[TitleRow, totalColumns]];
            title.Font.Bold = true;
            title.Font.Size = 14;

            Excel.Range header =
                worksheet.Range[
                    worksheet.Cells[HeaderFirstRow, 1],
                    worksheet.Cells[HeaderFirstRow + headerRowCount - 1, totalColumns]];
            header.Font.Bold = true;
            header.WrapText = true;
            header.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            header.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            header.Interior.Color = 15773696;
            ApplyBorders(header);

            if (rows.Count > 0)
            {
                Excel.Range data =
                    worksheet.Range[
                        worksheet.Cells[firstDataRow, 1],
                        worksheet.Cells[renderedLastRow, totalColumns]];
                data.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                ApplyBorders(data);

                if (descriptiveColumnCount > 0)
                {
                    Excel.Range descriptive =
                        worksheet.Range[
                            worksheet.Cells[firstDataRow, 1],
                            worksheet.Cells[renderedLastRow, descriptiveColumnCount]];
                    descriptive.WrapText = true;
                }

                if (descriptiveColumnCount >= 1)
                {
                    ((Excel.Range)worksheet.Range[
                        worksheet.Cells[firstDataRow, 1],
                        worksheet.Cells[renderedLastRow, 1]])
                        .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                if (descriptiveColumnCount >= 3)
                {
                    ((Excel.Range)worksheet.Range[
                        worksheet.Cells[firstDataRow, 3],
                        worksheet.Cells[renderedLastRow, 3]])
                        .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                }

                if (dataColumnCount > 0)
                {
                    Excel.Range numeric =
                        worksheet.Range[
                            worksheet.Cells[
                                firstDataRow,
                                descriptiveColumnCount + 1],
                            worksheet.Cells[renderedLastRow, totalColumns]];
                    numeric.NumberFormat = "#,##0;[Red]-#,##0;–";
                    numeric.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                }

                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    if (!rows[rowIndex].IsSumRow)
                        continue;

                    Excel.Range sumRow =
                        worksheet.Range[
                            worksheet.Cells[firstDataRow + rowIndex, 1],
                            worksheet.Cells[firstDataRow + rowIndex, totalColumns]];
                    sumRow.Font.Bold = true;
                }

                data.Rows.AutoFit();
            }

            header.Rows.AutoFit();
        }

        private static void ApplyBorders(Excel.Range range)
        {
            range.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Borders.Weight = Excel.XlBorderWeight.xlThin;
            range.Borders.Color = 12566463;
        }

        private static void ApplyDefaultColumnWidths(
            Excel.Worksheet worksheet,
            int totalColumns,
            int descriptiveColumnCount)
        {
            for (int column = 1; column <= totalColumns; column++)
            {
                double width = 16;

                if (column == 1 && descriptiveColumnCount > 0)
                    width = 12;
                else if (column == 2 && descriptiveColumnCount > 1)
                    width = 70;
                else if (column == 3 && descriptiveColumnCount > 2)
                    width = 12;

                ((Excel.Range)worksheet.Columns[column]).ColumnWidth = width;
            }
        }

        private static double[] CaptureColumnWidths(
            Excel.Worksheet worksheet,
            int columnCount)
        {
            var result = new double[columnCount];

            for (int column = 1; column <= columnCount; column++)
            {
                result[column - 1] = Convert.ToDouble(
                    ((Excel.Range)worksheet.Columns[column]).ColumnWidth,
                    CultureInfo.InvariantCulture);
            }

            return result;
        }

        private static void RestoreColumnWidths(
            Excel.Worksheet worksheet,
            IReadOnlyList<double> widths)
        {
            for (int column = 1; column <= widths.Count; column++)
            {
                ((Excel.Range)worksheet.Columns[column]).ColumnWidth =
                    widths[column - 1];
            }
        }

        private static void ApplyPrintSetup(
            Excel.Worksheet worksheet,
            int totalColumns,
            int lastRow)
        {
            Excel.Range printRange =
                worksheet.Range[
                    worksheet.Cells[TitleRow, 1],
                    worksheet.Cells[lastRow, totalColumns]];

            worksheet.PageSetup.PrintArea = printRange.Address;
            worksheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperA4;
            worksheet.PageSetup.Orientation = Excel.XlPageOrientation.xlPortrait;
            worksheet.PageSetup.Zoom = false;
            worksheet.PageSetup.FitToPagesWide = 1;
            worksheet.PageSetup.FitToPagesTall = false;
            worksheet.PageSetup.CenterHorizontally = true;
            worksheet.PageSetup.LeftMargin =
                worksheet.Application.CentimetersToPoints(0.7);
            worksheet.PageSetup.RightMargin =
                worksheet.Application.CentimetersToPoints(0.7);
            worksheet.PageSetup.TopMargin =
                worksheet.Application.CentimetersToPoints(1.0);
            worksheet.PageSetup.BottomMargin =
                worksheet.Application.CentimetersToPoints(1.0);
        }

        private static int GetTotalColumnCount(
            AuditReportTableDefinitionResponse table)
        {
            int result = table.NumberOfColumns ?? 0;

            foreach (AuditReportHeaderDefinitionResponse header
                     in table.Headers ??
                        Array.Empty<AuditReportHeaderDefinitionResponse>())
            {
                result = Math.Max(
                    result,
                    header.ColumnPosition + header.ColumnSpan - 1);
            }

            if (result < 1)
            {
                throw new InvalidOperationException(
                    $"Template table {table.TableErpId} does not define any columns.");
            }

            return result;
        }

        private static int GetDataColumnCount(
            AuditReportTableDefinitionResponse table)
        {
            if (!table.NumberOfDataColumns.HasValue ||
                table.NumberOfDataColumns.Value < 1)
            {
                throw new InvalidOperationException(
                    $"Template table {table.TableErpId} does not define a valid " +
                    "number of data columns.");
            }

            return table.NumberOfDataColumns.Value;
        }

        private static int GetHeaderRowCount(
            AuditReportTableDefinitionResponse table)
        {
            AuditReportHeaderDefinitionResponse[] headers =
                table.Headers ??
                Array.Empty<AuditReportHeaderDefinitionResponse>();

            if (headers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Template table {table.TableErpId} does not contain headers.");
            }

            return headers.Max(
                x => x.RowPosition + x.RowSpan - 1);
        }

        private static string ResolveHeaderText(
            string text,
            FinancialReportEnvelope report)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            int? fiscalYear = GetFiscalYear(report);

            if (!fiscalYear.HasValue)
                return text.Trim();

            return text.Trim()
                .Replace(
                    "20xx-1",
                    (fiscalYear.Value - 1).ToString(
                        CultureInfo.InvariantCulture))
                .Replace(
                    "20xx",
                    fiscalYear.Value.ToString(
                        CultureInfo.InvariantCulture));
        }

        private static int? GetFiscalYear(FinancialReportEnvelope report)
        {
            string periodTo = GetPeriodTo(report);
            DateTime date;

            return DateTime.TryParse(
                periodTo,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date)
                    ? (int?)date.Year
                    : null;
        }

        private static string GetPeriodFrom(FinancialReportEnvelope report)
        {
            return FirstNonEmpty(
                report.TitlePage?.PeriodFrom,
                report.FinancialStatement?.PeriodFrom,
                report.AnnualReport?.PeriodFrom);
        }

        private static string GetPeriodTo(FinancialReportEnvelope report)
        {
            return FirstNonEmpty(
                report.TitlePage?.PeriodTo,
                report.FinancialStatement?.PeriodTo,
                report.AnnualReport?.PeriodTo);
        }

        private static string ValueKey(
            int rowOrdinal,
            int dataColumnOrdinal)
        {
            return rowOrdinal.ToString(CultureInfo.InvariantCulture) +
                ":" +
                dataColumnOrdinal.ToString(CultureInfo.InvariantCulture);
        }

        private static Excel.Worksheet FindWorksheet(
            Excel.Workbook workbook,
            string name)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                if (string.Equals(
                        worksheet.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return worksheet;
                }
            }

            return null;
        }

        private static void ValidateWorksheetName(string name)
        {
            if (name.Length > 31 ||
                name.IndexOfAny(new[] { '[', ']', ':', '*', '?', '/', '\\' }) >= 0)
            {
                throw new InvalidOperationException(
                    $"'{name}' is not a valid Excel worksheet name.");
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }
    }
}
