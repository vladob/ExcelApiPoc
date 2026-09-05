using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class MultiYearBalanceSheetBuilder
    {
        private const int FirstSupportedFiscalYear = 2014;
        private const int AssetsTableOrdinal = 0;
        private const int LiabilitiesTableOrdinal = 1;
        private const int AssetsFirstRowNumber = 1;
        private const int AssetsLastRowNumber = 114;
        private const int LiabilitiesFirstRowNumber = 115;
        private const int LiabilitiesLastRowNumber = 183;

        // RegisterUZ data-column ordinals are zero-based. These correspond to
        // official balance-sheet columns 3 (assets Netto) and 5 (liabilities).
        private const int AssetsNetDataColumnOrdinal = 2;
        private const int LiabilitiesNetDataColumnOrdinal = 0;

        public static MultiYearBalanceSheet Build(
            AccountingEntityPackageEnvelope package,
            RegisterUzFinancialReportSelection auditedReport)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));
            if (auditedReport == null)
                throw new ArgumentNullException(nameof(auditedReport));

            int auditedFiscalYear = GetStatementFiscalYear(auditedReport.Statement.Statement);
            List<YearlyReport> reports = SelectYearlyReports(
                package, auditedReport.TemplateErpId, auditedFiscalYear);

            if (reports.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No compatible RegisterUZ financial report with template {auditedReport.TemplateErpId} was found.");
            }

            YearlyReport newest = reports[0];
            AuditTemplatePackageResponse template = newest.Report.Template;

            AuditReportTableDefinitionResponse assetsTemplate =
                GetTemplateTable(template, AssetsTableOrdinal);
            AuditReportTableDefinitionResponse liabilitiesTemplate =
                GetTemplateTable(template, LiabilitiesTableOrdinal);

            List<LayoutRow> layout = new List<LayoutRow>();
            layout.AddRange(BuildLayoutRows(
                assetsTemplate,
                AssetsTableOrdinal,
                AssetsFirstRowNumber,
                AssetsLastRowNumber,
                AssetsNetDataColumnOrdinal));
            layout.AddRange(BuildLayoutRows(
                liabilitiesTemplate,
                LiabilitiesTableOrdinal,
                LiabilitiesFirstRowNumber,
                LiabilitiesLastRowNumber,
                LiabilitiesNetDataColumnOrdinal));

            ValidateContinuousRowNumbers(layout);

            var valuesByReport = new Dictionary<int, IReadOnlyDictionary<string, decimal>>();

            foreach (YearlyReport report in reports)
            {
                valuesByReport.Add(
                    report.FiscalYear,
                    BuildValueIndex(report.Report));
            }

            List<MultiYearBalanceSheetRow> rows =
                new List<MultiYearBalanceSheetRow>(layout.Count);

            foreach (LayoutRow layoutRow in layout)
            {
                var valuesByYear = new Dictionary<int, decimal>();

                foreach (YearlyReport report in reports)
                {
                    decimal value;
                    if (valuesByReport[report.FiscalYear].TryGetValue(
                            ValueKey(
                                layoutRow.TableOrdinal,
                                layoutRow.RowOrdinal,
                                layoutRow.DataColumnOrdinal),
                            out value))
                    {
                        valuesByYear.Add(report.FiscalYear, value);
                    }
                }

                rows.Add(
                    new MultiYearBalanceSheetRow
                    {
                        Designation = layoutRow.TemplateRow.Designation,
                        Description = layoutRow.TemplateRow.TextSk,
                        RowNumber = layoutRow.TemplateRow.RowNumber.Value,
                        IsSumRow = layoutRow.TemplateRow.IsSumRow,
                        HasData = GetHasDataValue(valuesByYear.Values),
                        ValuesByFiscalYear = valuesByYear
                    });
            }

            return new MultiYearBalanceSheet
            {
                Entity = package.Entity,
                TemplateName = template.Template?.Name ?? string.Empty,
                FiscalYears = reports.Select(x => x.FiscalYear).ToArray(),
                Rows = rows
            };
        }

        private static int? GetHasDataValue(
            IEnumerable<decimal> values)
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

        private static List<YearlyReport> SelectYearlyReports(
            AccountingEntityPackageEnvelope package,
            int auditedTemplateErpId,
            int auditedFiscalYear)
        {
            var candidates = new List<YearlyReport>();

            foreach (FinancialReportEnvelope report in package.ReportsById.Values)
            {
                // Until compatibility groups are configured, a template is
                // explicitly compatible only with itself.
                if (report.Report.TemplateId != auditedTemplateErpId ||
                    !report.HasTemplate)
                {
                    continue;
                }

                int fiscalYear;
                if (!TryGetFiscalYear(report, out fiscalYear))
                    continue;

                if (fiscalYear < FirstSupportedFiscalYear || fiscalYear > auditedFiscalYear)
                    continue;

                candidates.Add(
                    new YearlyReport
                    {
                        FiscalYear = fiscalYear,
                        Report = report,
                        SortDate = GetReportSortDate(report)
                    });
            }

            return candidates
                .GroupBy(x => x.FiscalYear)
                .Select(group => group
                    .OrderByDescending(x => x.SortDate)
                    .ThenByDescending(x => x.Report.Report.Id)
                    .First())
                .OrderByDescending(x => x.FiscalYear)
                .ToList();
        }

        private static int GetStatementFiscalYear(FinancialStatementDto statement)
        {
            if (statement == null || string.IsNullOrWhiteSpace(statement.PeriodTo) ||
                statement.PeriodTo.Length < 4 ||
                !int.TryParse(statement.PeriodTo.Substring(0, 4), NumberStyles.None,
                    CultureInfo.InvariantCulture, out int fiscalYear))
            {
                throw new InvalidOperationException("The selected financial statement has no valid fiscal year.");
            }

            return fiscalYear;
        }

        private static IEnumerable<LayoutRow> BuildLayoutRows(
            AuditReportTableDefinitionResponse table,
            int tableOrdinal,
            int firstRowNumber,
            int lastRowNumber,
            int dataColumnOrdinal)
        {
            AuditReportRowDefinitionResponse[] rows = table.Rows ??
                Array.Empty<AuditReportRowDefinitionResponse>();

            foreach (AuditReportRowDefinitionResponse row
                     in rows.OrderBy(x => x.RowOrdinal))
            {
                if (!row.RowNumber.HasValue ||
                    row.RowNumber.Value < firstRowNumber ||
                    row.RowNumber.Value > lastRowNumber)
                {
                    continue;
                }

                yield return new LayoutRow
                {
                    TableOrdinal = tableOrdinal,
                    RowOrdinal = row.RowOrdinal,
                    DataColumnOrdinal = dataColumnOrdinal,
                    TemplateRow = row
                };
            }
        }

        private static void ValidateContinuousRowNumbers(
            IReadOnlyList<LayoutRow> rows)
        {
            int expectedCount = LiabilitiesLastRowNumber;

            if (rows.Count != expectedCount)
            {
                throw new InvalidOperationException(
                    $"Template 690 balance-sheet layout contains {rows.Count} rows; " +
                    $"{expectedCount} rows were expected.");
            }

            for (int index = 0; index < rows.Count; index++)
            {
                int expectedRowNumber = index + 1;
                if (rows[index].TemplateRow.RowNumber != expectedRowNumber)
                {
                    throw new InvalidOperationException(
                        "Template 690 balance-sheet row order is not continuous at " +
                        $"row number {expectedRowNumber}.");
                }
            }
        }

        private static AuditReportTableDefinitionResponse GetTemplateTable(
            AuditTemplatePackageResponse template,
            int tableOrdinal)
        {
            AuditReportTableDefinitionResponse[] matches =
                (template.Template?.Tables ??
                    Array.Empty<AuditReportTableDefinitionResponse>())
                .Where(x => x.TableOrdinal == tableOrdinal)
                .ToArray();

            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Template 690 must contain exactly one table with ordinal " +
                    $"{tableOrdinal}; found {matches.Length}.");
            }

            return matches[0];
        }

        private static IReadOnlyDictionary<string, decimal> BuildValueIndex(
            FinancialReportEnvelope report)
        {
            var result = new Dictionary<string, decimal>(StringComparer.Ordinal);

            foreach (FinancialReportTableEnvelope table in report.Tables)
            {
                int tableOrdinal = table.Table.TableOrdinal;
                if (tableOrdinal != AssetsTableOrdinal &&
                    tableOrdinal != LiabilitiesTableOrdinal)
                {
                    continue;
                }

                foreach (FinancialReportValueDto value in table.Values)
                {
                    string key = ValueKey(
                        tableOrdinal,
                        value.RowOrdinal,
                        value.DataColumnOrdinal);

                    if (result.ContainsKey(key))
                    {
                        throw new InvalidOperationException(
                            $"RegisterUZ report {report.Report.Id} contains duplicate " +
                            $"value for table {tableOrdinal}, row {value.RowOrdinal}, " +
                            $"data column {value.DataColumnOrdinal}.");
                    }

                    result.Add(key, value.NumericValue);
                }
            }

            return result;
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
            {
                if (date.HasValue && date.Value > result)
                    result = date.Value;
            }

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
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private sealed class YearlyReport
        {
            public int FiscalYear { get; set; }

            public FinancialReportEnvelope Report { get; set; }

            public DateTime SortDate { get; set; }
        }

        private sealed class LayoutRow
        {
            public int TableOrdinal { get; set; }

            public int RowOrdinal { get; set; }

            public int DataColumnOrdinal { get; set; }

            public AuditReportRowDefinitionResponse TemplateRow { get; set; }
        }
    }
}
