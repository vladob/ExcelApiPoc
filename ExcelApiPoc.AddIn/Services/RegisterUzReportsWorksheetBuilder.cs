using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzReportsWorksheetBuilder
    {
        private const string CreateAction = "Create";
        private const int MaximumWorksheetNameLength = 31;

        public static IReadOnlyList<RegisterUzReportListRow> Build(
            AccountingEntityPackageEnvelope package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var rows = new List<RegisterUzReportListRow>();

            foreach (FinancialStatementEnvelope statement
                     in package.FinancialStatements)
            {
                for (int reportIndex = 0;
                     reportIndex < statement.FinancialReports.Count;
                     reportIndex++)
                {
                    AddReportRows(
                        rows,
                        statement.FinancialReports[reportIndex],
                        reportIndex + 1);
                }
            }

            foreach (AnnualReportEnvelope annualReport
                     in package.AnnualReports)
            {
                for (int reportIndex = 0;
                     reportIndex < annualReport.FinancialReports.Count;
                     reportIndex++)
                {
                    AddReportRows(
                        rows,
                        annualReport.FinancialReports[reportIndex],
                        reportIndex + 1);
                }
            }

            return rows
                .OrderByDescending(x => x.FiscalYear ?? int.MinValue)
                .ThenByDescending(x => x.PeriodToSortValue)
                .ThenByDescending(x => x.PeriodFromSortValue)
                .ThenByDescending(x => x.FinancialReportId)
                .ThenBy(x => x.TableOrdinal)
                .ThenBy(x => x.FinancialReportTableId)
                .ToList();
        }

        private static void AddReportRows(
            ICollection<RegisterUzReportListRow> rows,
            FinancialReportEnvelope reportEnvelope,
            int reportOrdinal)
        {
            FinancialReportDto report = reportEnvelope.Report;
            FinancialReportTitlePageDto titlePage = report.TitlePage;

            string periodFrom = FirstNonEmpty(
                titlePage?.PeriodFrom,
                reportEnvelope.FinancialStatement?.PeriodFrom,
                reportEnvelope.AnnualReport?.PeriodFrom);

            string periodTo = FirstNonEmpty(
                titlePage?.PeriodTo,
                reportEnvelope.FinancialStatement?.PeriodTo,
                reportEnvelope.AnnualReport?.PeriodTo);

            DateTime periodFromDate = ParseDate(periodFrom);
            DateTime periodToDate = ParseDate(periodTo);
            int? fiscalYear = GetFiscalYear(periodToDate, periodFromDate);

            string reportType = FirstNonEmpty(
                titlePage?.ReportType,
                reportEnvelope.FinancialStatement?.StatementType,
                reportEnvelope.AnnualReport?.AnnualReportType);

            string parentType = reportEnvelope.BelongsToFinancialStatement
                ? "Financial statement"
                : "Annual report";

            string templateName =
                reportEnvelope.Template?.Template?.Name ?? string.Empty;

            foreach (FinancialReportTableEnvelope tableEnvelope
                     in reportEnvelope.Tables)
            {
                FinancialReportTableDto table = tableEnvelope.Table;
                string tableName = FirstNonEmpty(
                    table.NameSk,
                    GetTemplateTableName(reportEnvelope.Template, table),
                    table.NameEn,
                    "Table " + table.TableOrdinal.ToString(CultureInfo.InvariantCulture));

                rows.Add(
                    new RegisterUzReportListRow
                    {
                        Action = CreateAction,
                        TargetWorksheetName = BuildWorksheetName(
                            fiscalYear,
                            tableName,
                            table.Id),
                        FiscalYear = fiscalYear,
                        PeriodFrom = periodFrom,
                        PeriodTo = periodTo,
                        ReportType = reportType,
                        ParentType = parentType,
                        TemplateName = templateName,
                        TableName = tableName,
                        TemplateId = report.TemplateId,
                        TemplateTableId = table.TemplateTableId,
                        FinancialStatementId =
                            reportEnvelope.FinancialStatement?.Id,
                        AnnualReportId = reportEnvelope.AnnualReport?.Id,
                        FinancialReportId = report.Id,
                        ReportOrdinal = reportOrdinal,
                        FinancialReportTableId = table.Id,
                        TableOrdinal = table.TableOrdinal,
                        CurrencyCode = report.CurrencyCode,
                        DataAvailability = report.DataAvailability,
                        SubmissionDate = FirstDate(
                            reportEnvelope.FinancialStatement?.SubmissionDate,
                            reportEnvelope.AnnualReport?.SubmissionDate),
                        CompletionDate = titlePage?.CompletionDate,
                        PeriodToSortValue = periodToDate,
                        PeriodFromSortValue = periodFromDate
                    });
            }
        }

        private static string GetTemplateTableName(
            AuditTemplatePackageResponse template,
            FinancialReportTableDto reportTable)
        {
            AuditReportTableDefinitionResponse[] tables =
                template?.Template?.Tables;

            if (tables == null)
                return string.Empty;

            AuditReportTableDefinitionResponse match = null;

            if (reportTable.TemplateTableId.HasValue)
            {
                match = tables.FirstOrDefault(
                    x => x.TableErpId == reportTable.TemplateTableId.Value);
            }

            if (match == null)
            {
                match = tables.FirstOrDefault(
                    x => x.TableOrdinal == reportTable.TableOrdinal);
            }

            return FirstNonEmpty(match?.NameSk, match?.NameEn);
        }

        private static string BuildWorksheetName(
            int? fiscalYear,
            string tableName,
            long tableId)
        {
            string year = fiscalYear.HasValue
                ? fiscalYear.Value.ToString(CultureInfo.InvariantCulture)
                : "Unknown";

            string suffix = " " + tableId.ToString(CultureInfo.InvariantCulture);
            string prefix = CleanWorksheetName("RUZ " + year + " " + tableName);
            int availablePrefixLength =
                MaximumWorksheetNameLength - suffix.Length;

            if (availablePrefixLength < 1)
            {
                return CleanWorksheetName(
                    ("RUZ" + suffix).Substring(
                        0,
                        Math.Min(
                            MaximumWorksheetNameLength,
                            ("RUZ" + suffix).Length)));
            }

            if (prefix.Length > availablePrefixLength)
                prefix = prefix.Substring(0, availablePrefixLength).TrimEnd();

            return prefix + suffix;
        }

        private static string CleanWorksheetName(string value)
        {
            var result = new StringBuilder(value.Length);

            foreach (char character in value)
            {
                switch (character)
                {
                    case '[':
                    case ']':
                    case ':':
                    case '*':
                    case '?':
                    case '/':
                    case '\\':
                        result.Append(' ');
                        break;
                    default:
                        result.Append(character);
                        break;
                }
            }

            return string.Join(
                " ",
                result.ToString()
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static int? GetFiscalYear(
            DateTime periodTo,
            DateTime periodFrom)
        {
            if (periodTo != DateTime.MinValue)
                return periodTo.Year;

            if (periodFrom != DateTime.MinValue)
                return periodFrom.Year;

            return null;
        }

        private static DateTime ParseDate(string value)
        {
            DateTime result;
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result)
                    ? result
                    : DateTime.MinValue;
        }

        private static DateTime? FirstDate(
            DateTime? first,
            DateTime? second)
        {
            return first ?? second;
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
