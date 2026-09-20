using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class MultiYearBalanceSheetBuilder
    {
        public static IReadOnlyList<MultiYearBalanceSheet> BuildAll(
            AccountingEntityPackageEnvelope package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var candidates = new List<YearlyReport>();

            foreach (FinancialReportEnvelope report in package.ReportsById.Values)
            {
                if (!report.Report.TemplateId.HasValue ||
                    !report.HasTemplate ||
                    report.Template.Template == null ||
                    !report.Template.Template.CreateMultiYear)
                    continue;

                if (!TryGetFiscalYear(report, out int fiscalYear))
                    continue;

                candidates.Add(new YearlyReport
                {
                    TemplateErpId = report.Template.Template.TemplateErpId,
                    FiscalYear = fiscalYear,
                    Report = report,
                    SortDate = GetReportSortDate(report)
                });
            }

            var result = new List<MultiYearBalanceSheet>();

            foreach (IGrouping<int, YearlyReport> group in candidates
                .GroupBy(x => x.TemplateErpId)
                .OrderBy(x => x.Key))
            {
                List<YearlyReport> reports = group
                    .GroupBy(x => x.FiscalYear)
                    .Select(year => year
                        .OrderByDescending(x => x.SortDate)
                        .ThenByDescending(x => x.Report.Report.Id)
                        .First())
                    .OrderByDescending(x => x.FiscalYear)
                    .ToList();

                if (reports.Count >= 2)
                    result.Add(Build(package, reports));
            }

            return result;
        }

        private static MultiYearBalanceSheet Build(
            AccountingEntityPackageEnvelope package,
            IReadOnlyList<YearlyReport> reports)
        {
            AuditTemplateDefinitionResponse template =
                reports[0].Report.Template.Template;

            if (string.IsNullOrWhiteSpace(template.MultiYearWorksheetName))
                throw new InvalidOperationException(
                    $"Template {template.TemplateErpId} enables multi-year " +
                    "reporting but does not define a worksheet name.");

            var layout = new List<LayoutRow>();

            foreach (AuditReportTableDefinitionResponse table in
                (template.Tables ??
                    Array.Empty<AuditReportTableDefinitionResponse>())
                .OrderBy(x => x.TableOrdinal))
            {
                AuditReportRowDefinitionResponse[] numberedRows =
                    (table.Rows ??
                        Array.Empty<AuditReportRowDefinitionResponse>())
                    .Where(x => x.RowNumber.HasValue)
                    .OrderBy(x => x.RowOrdinal)
                    .ToArray();

                if (numberedRows.Length == 0)
                    continue;

                int valueColumn =
                    ResolveCurrentPeriodDataColumnOrdinal(table);

                foreach (AuditReportRowDefinitionResponse row in numberedRows)
                {
                    layout.Add(new LayoutRow
                    {
                        TableOrdinal = table.TableOrdinal,
                        RowOrdinal = row.RowOrdinal,
                        DataColumnOrdinal = valueColumn,
                        TableName = FirstNonEmpty(
                            table.NameSk,
                            table.NameEn,
                            "Table " + table.TableErpId),
                        TemplateRow = row
                    });
                }
            }

            if (layout.Count == 0)
                throw new InvalidOperationException(
                    $"Template {template.TemplateErpId} does not contain " +
                    "numbered report rows.");

            var reportValues =
                new Dictionary<int, IReadOnlyDictionary<string, decimal>>();

            foreach (YearlyReport report in reports)
                reportValues.Add(
                    report.FiscalYear,
                    BuildValueIndex(report.Report));

            var rows = new List<MultiYearBalanceSheetRow>(layout.Count);

            foreach (LayoutRow layoutRow in layout)
            {
                var values = new Dictionary<int, decimal>();

                foreach (YearlyReport report in reports)
                    if (reportValues[report.FiscalYear].TryGetValue(
                        ValueKey(
                            layoutRow.TableOrdinal,
                            layoutRow.RowOrdinal,
                            layoutRow.DataColumnOrdinal),
                        out decimal value))
                        values.Add(report.FiscalYear, value);

                rows.Add(new MultiYearBalanceSheetRow
                {
                    ReportTable = layoutRow.TableName,
                    Designation = layoutRow.TemplateRow.Designation,
                    Description = layoutRow.TemplateRow.TextSk,
                    RowNumber = layoutRow.TemplateRow.RowNumber.Value,
                    IsSumRow = layoutRow.TemplateRow.IsSumRow,
                    HasData = GetHasDataValue(values.Values),
                    ValuesByFiscalYear = values
                });
            }

            return new MultiYearBalanceSheet
            {
                Entity = package.Entity,
                TemplateErpId = template.TemplateErpId,
                TemplateName = template.Name ?? string.Empty,
                WorksheetName = template.MultiYearWorksheetName.Trim(),
                FiscalYears = reports.Select(x => x.FiscalYear).ToArray(),
                Rows = rows
            };
        }

        private static int ResolveCurrentPeriodDataColumnOrdinal(
            AuditReportTableDefinitionResponse table)
        {
            if (!table.NumberOfColumns.HasValue ||
                !table.NumberOfDataColumns.HasValue ||
                table.NumberOfDataColumns.Value <= 0)
                throw new InvalidOperationException(
                    $"Template table {table.TableErpId} does not define " +
                    "a valid data-column layout.");

            int dataCount = table.NumberOfDataColumns.Value;
            if (dataCount == 1)
                return 0;

            int descriptiveCount =
                table.NumberOfColumns.Value - dataCount;
            var currentCandidates = new List<int>();
            var previousCandidates = new List<int>();

            foreach (AuditReportHeaderDefinitionResponse header in
                table.Headers ??
                    Array.Empty<AuditReportHeaderDefinitionResponse>())
            {
                string text = NormalizeHeaderText(
                    FirstNonEmpty(header.TextSk, header.TextEn));
                bool isCurrent = IsCurrentPeriodHeader(text);
                bool isPrevious = IsPreviousPeriodHeader(text);

                if (!isCurrent && !isPrevious)
                    continue;

                int first = header.ColumnPosition - 1;
                int last = first + Math.Max(header.ColumnSpan, 1) - 1;

                for (int column = first; column <= last; column++)
                {
                    int ordinal = column - descriptiveCount;
                    if (ordinal < 0 || ordinal >= dataCount)
                        continue;

                    if (isCurrent)
                        currentCandidates.Add(ordinal);
                    if (isPrevious)
                        previousCandidates.Add(ordinal);
                }
            }

            if (previousCandidates.Count > 0)
            {
                int firstPreviousColumn = previousCandidates.Min();
                if (firstPreviousColumn > 0)
                    return firstPreviousColumn - 1;
            }

            if (currentCandidates.Count > 0)
                return currentCandidates.Max();

            throw new InvalidOperationException(
                $"Template table {table.TableErpId} has {dataCount} " +
                "data columns, but its current-period value column " +
                "could not be identified from the headers.");
        }

        private static bool IsCurrentPeriodHeader(string text)
        {
            if (IsPreviousPeriodHeader(text))
                return false;

            return text.Contains("BEZNE UCTOVNE OBDOBIE") ||
                   text.Contains("BEZNE OBDOBIE") ||
                   text.Contains("CURRENT ACCOUNTING PERIOD") ||
                   text.Contains("CURRENT PERIOD") ||
                   text.Contains("20XX");
        }

        private static bool IsPreviousPeriodHeader(string text)
        {
            return text.Contains("PREDCHADZAJUCE") ||
                   text.Contains("PREVIOUS ACCOUNTING PERIOD") ||
                   text.Contains("PREVIOUS PERIOD") ||
                   text.Contains("20XX-1") ||
                   text.Contains("20XX - 1");
        }

        private static string NormalizeHeaderText(string value)
        {
            string decomposed =
                (value ?? string.Empty).Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(decomposed.Length);

            foreach (char character in decomposed)
                if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                    UnicodeCategory.NonSpacingMark)
                    result.Append(char.ToUpperInvariant(character));

            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        private static IReadOnlyDictionary<string, decimal> BuildValueIndex(
            FinancialReportEnvelope report)
        {
            var result =
                new Dictionary<string, decimal>(StringComparer.Ordinal);

            foreach (FinancialReportTableEnvelope table in report.Tables)
                foreach (FinancialReportValueDto value in table.Values)
                {
                    string key = ValueKey(
                        table.Table.TableOrdinal,
                        value.RowOrdinal,
                        value.DataColumnOrdinal);

                    if (result.ContainsKey(key))
                        throw new InvalidOperationException(
                            $"RegisterUZ report {report.Report.Id} contains " +
                            $"duplicate value for table " +
                            $"{table.Table.TableOrdinal}, row " +
                            $"{value.RowOrdinal}, data column " +
                            $"{value.DataColumnOrdinal}.");

                    result.Add(key, value.NumericValue);
                }

            return result;
        }

        private static int? GetHasDataValue(IEnumerable<decimal> values)
        {
            bool hasValue = false;
            foreach (decimal value in values)
            {
                hasValue = true;
                if (value != 0m)
                    return 1;
            }

            return hasValue ? (int?)0 : null;
        }

        private static bool TryGetFiscalYear(
            FinancialReportEnvelope report,
            out int fiscalYear)
        {
            string periodTo = FirstNonEmpty(
                report.TitlePage?.PeriodTo,
                report.FinancialStatement?.PeriodTo,
                report.AnnualReport?.PeriodTo);

            fiscalYear = 0;
            return periodTo.Length >= 4 &&
                int.TryParse(
                    periodTo.Substring(0, 4),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out fiscalYear);
        }

        private static DateTime GetReportSortDate(
            FinancialReportEnvelope report)
        {
            DateTime result = DateTime.MinValue;
            DateTime?[] dates =
            {
                report.FinancialStatement?.SubmissionDate,
                report.AnnualReport?.SubmissionDate,
                report.TitlePage?.CompletionDate,
                report.TitlePage?.PreparationDate,
                report.TitlePage?.ApprovalDate
            };

            foreach (DateTime? date in dates)
                if (date.HasValue && date.Value > result)
                    result = date.Value;

            return result;
        }

        private static string ValueKey(
            int tableOrdinal,
            int rowOrdinal,
            int dataColumnOrdinal)
        {
            return tableOrdinal.ToString(CultureInfo.InvariantCulture) + ":" +
                rowOrdinal.ToString(CultureInfo.InvariantCulture) + ":" +
                dataColumnOrdinal.ToString(CultureInfo.InvariantCulture);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();

            return string.Empty;
        }

        private sealed class YearlyReport
        {
            public int TemplateErpId { get; set; }
            public int FiscalYear { get; set; }
            public FinancialReportEnvelope Report { get; set; }
            public DateTime SortDate { get; set; }
        }

        private sealed class LayoutRow
        {
            public int TableOrdinal { get; set; }
            public int RowOrdinal { get; set; }
            public int DataColumnOrdinal { get; set; }
            public string TableName { get; set; }
            public AuditReportRowDefinitionResponse TemplateRow { get; set; }
        }
    }
}
