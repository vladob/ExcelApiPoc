using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class RegisterUzAttachmentsWorksheetBuilder
    {
        private const string OpenAction = "Open";
        private const string AttachmentUrlPrefix =
            "https://www.registeruz.sk/cruz-public/domain/financialreport/attachment/";

        public static IReadOnlyList<RegisterUzAttachmentListRow> Build(
            AccountingEntityPackageEnvelope package)
        {
            if (package == null)
                throw new ArgumentNullException(nameof(package));

            var rows = new List<RegisterUzAttachmentListRow>();

            foreach (FinancialStatementEnvelope statement
                     in package.FinancialStatements)
            {
                for (int reportIndex = 0;
                     reportIndex < statement.FinancialReports.Count;
                     reportIndex++)
                {
                    AddFinancialReportAttachments(
                        rows,
                        statement.FinancialReports[reportIndex],
                        reportIndex + 1);
                }
            }

            foreach (AnnualReportEnvelope annualReport
                     in package.AnnualReports)
            {
                AddAnnualReportAttachments(rows, annualReport);

                for (int reportIndex = 0;
                     reportIndex < annualReport.FinancialReports.Count;
                     reportIndex++)
                {
                    AddFinancialReportAttachments(
                        rows,
                        annualReport.FinancialReports[reportIndex],
                        reportIndex + 1);
                }
            }

            return rows
                .OrderByDescending(x => x.FiscalYear ?? int.MinValue)
                .ThenByDescending(x => x.PeriodToSortValue)
                .ThenByDescending(x => x.PeriodFromSortValue)
                .ThenBy(x => x.OwnerType)
                .ThenByDescending(x => x.FinancialReportId ?? long.MinValue)
                .ThenByDescending(x => x.AnnualReportId ?? long.MinValue)
                .ThenBy(x => x.AttachmentOrdinal)
                .ThenBy(x => x.AttachmentId)
                .ToList();
        }

        private static void AddFinancialReportAttachments(
            ICollection<RegisterUzAttachmentListRow> rows,
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

            for (int attachmentIndex = 0;
                 attachmentIndex < report.Attachments.Count;
                 attachmentIndex++)
            {
                FinancialReportAttachmentDto attachment =
                    report.Attachments[attachmentIndex];

                rows.Add(
                    new RegisterUzAttachmentListRow
                    {
                        Action = OpenAction,
                        Url = BuildUrl(attachment.Id),
                        FiscalYear = fiscalYear,
                        PeriodFrom = periodFrom,
                        PeriodTo = periodTo,
                        OwnerType = "Financial report",
                        ParentType = parentType,
                        ReportType = reportType,
                        TemplateName = templateName,
                        FileName = attachment.FileName,
                        MimeType = attachment.MimeType,
                        FileSizeBytes = attachment.FileSizeBytes,
                        PageCount = attachment.PageCount,
                        LanguageCode = attachment.LanguageCode,
                        TemplateId = report.TemplateId,
                        FinancialStatementId =
                            reportEnvelope.FinancialStatement?.Id,
                        AnnualReportId = reportEnvelope.AnnualReport?.Id,
                        FinancialReportId = report.Id,
                        ReportOrdinal = reportOrdinal,
                        AttachmentId = attachment.Id,
                        AttachmentOrdinal = attachmentIndex + 1,
                        DataAvailability = report.DataAvailability,
                        SubmissionDate = FirstDate(
                            reportEnvelope.FinancialStatement?.SubmissionDate,
                            reportEnvelope.AnnualReport?.SubmissionDate),
                        PeriodToSortValue = periodToDate,
                        PeriodFromSortValue = periodFromDate
                    });
            }
        }

        private static void AddAnnualReportAttachments(
            ICollection<RegisterUzAttachmentListRow> rows,
            AnnualReportEnvelope annualReportEnvelope)
        {
            AnnualReportDto annualReport =
                annualReportEnvelope.AnnualReport;
            DateTime periodFromDate = ParseDate(annualReport.PeriodFrom);
            DateTime periodToDate = ParseDate(annualReport.PeriodTo);
            int? fiscalYear = GetFiscalYear(periodToDate, periodFromDate);

            for (int attachmentIndex = 0;
                 attachmentIndex < annualReport.Attachments.Count;
                 attachmentIndex++)
            {
                AnnualReportAttachmentDto attachment =
                    annualReport.Attachments[attachmentIndex];

                rows.Add(
                    new RegisterUzAttachmentListRow
                    {
                        Action = OpenAction,
                        Url = BuildUrl(attachment.Id),
                        FiscalYear = fiscalYear,
                        PeriodFrom = annualReport.PeriodFrom,
                        PeriodTo = annualReport.PeriodTo,
                        OwnerType = "Annual report",
                        ParentType = "Annual report",
                        ReportType = annualReport.AnnualReportType,
                        TemplateName = string.Empty,
                        FileName = attachment.FileName,
                        MimeType = attachment.MimeType,
                        FileSizeBytes = attachment.FileSizeBytes,
                        PageCount = null,
                        LanguageCode = attachment.LanguageCode,
                        TemplateId = null,
                        FinancialStatementId = null,
                        AnnualReportId = annualReport.Id,
                        FinancialReportId = null,
                        ReportOrdinal = null,
                        AttachmentId = attachment.Id,
                        AttachmentOrdinal = attachmentIndex + 1,
                        DataAvailability = string.Empty,
                        SubmissionDate = annualReport.SubmissionDate,
                        PeriodToSortValue = periodToDate,
                        PeriodFromSortValue = periodFromDate
                    });
            }
        }

        private static string BuildUrl(long attachmentId)
        {
            return AttachmentUrlPrefix +
                attachmentId.ToString(CultureInfo.InvariantCulture);
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
