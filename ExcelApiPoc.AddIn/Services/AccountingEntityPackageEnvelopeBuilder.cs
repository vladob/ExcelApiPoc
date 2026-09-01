using System;
using System.Collections.Generic;
using ExcelApiPoc.AddIn.Models;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountingEntityPackageEnvelopeBuilder
    {
        public static AccountingEntityPackageEnvelope Build(AccountingEntityPackageDto package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (package.Entity == null)
            {
                throw new InvalidOperationException(
                    "Accounting-entity package does not contain an entity.");
            }

            NormalizeCollections(package);

            Dictionary<long, AuditTemplatePackageResponse> templates =
                BuildTemplateIndex(package.Templates);

            Dictionary<long, FinancialReportEnvelope> reportsById =
                new Dictionary<long, FinancialReportEnvelope>();

            Dictionary<long, FinancialReportTableEnvelope> tablesById =
                new Dictionary<long, FinancialReportTableEnvelope>();

            List<FinancialStatementEnvelope> financialStatements =
                new List<FinancialStatementEnvelope>();

            foreach (FinancialStatementDto statement
                     in package.FinancialStatements)
            {
                if (statement == null)
                {
                    throw new InvalidOperationException(
                        "Accounting-entity package contains a null financial statement.");
                }

                if (statement.FinancialReports == null)
                {
                    statement.FinancialReports =
                        new List<FinancialReportDto>();
                }

                List<FinancialReportEnvelope> reports =
                    new List<FinancialReportEnvelope>();

                foreach (FinancialReportDto report
                         in statement.FinancialReports)
                {
                    FinancialReportEnvelope envelope =
                        BuildFinancialReport(
                            report,
                            statement,
                            null,
                            templates,
                            reportsById,
                            tablesById);

                    reports.Add(envelope);
                }

                financialStatements.Add(
                    new FinancialStatementEnvelope(
                        statement,
                        reports));
            }

            List<AnnualReportEnvelope> annualReports =
                new List<AnnualReportEnvelope>();

            foreach (AnnualReportDto annualReport
                     in package.AnnualReports)
            {
                if (annualReport == null)
                {
                    throw new InvalidOperationException(
                        "Accounting-entity package contains a null annual report.");
                }

                if (annualReport.Attachments == null)
                {
                    annualReport.Attachments =
                        new List<AnnualReportAttachmentDto>();
                }

                if (annualReport.FinancialReports == null)
                {
                    annualReport.FinancialReports =
                        new List<FinancialReportDto>();
                }

                List<FinancialReportEnvelope> reports =
                    new List<FinancialReportEnvelope>();

                foreach (FinancialReportDto report
                         in annualReport.FinancialReports)
                {
                    FinancialReportEnvelope envelope =
                        BuildFinancialReport(
                            report,
                            null,
                            annualReport,
                            templates,
                            reportsById,
                            tablesById);

                    reports.Add(envelope);
                }

                annualReports.Add(
                    new AnnualReportEnvelope(
                        annualReport,
                        reports));
            }

            return new AccountingEntityPackageEnvelope(
                package,
                financialStatements,
                annualReports,
                reportsById,
                tablesById,
                templates);
        }

        private static FinancialReportEnvelope BuildFinancialReport(
            FinancialReportDto report,
            FinancialStatementDto financialStatement,
            AnnualReportDto annualReport,
            IReadOnlyDictionary<long, AuditTemplatePackageResponse> templates,
            IDictionary<long, FinancialReportEnvelope> reportsById,
            IDictionary<long, FinancialReportTableEnvelope> tablesById)
        {
            if (report == null)
            {
                throw new InvalidOperationException(
                    "Accounting-entity package contains a null financial report.");
            }

            if ((financialStatement == null) == (annualReport == null))
            {
                throw new InvalidOperationException(
                    "Financial report must belong to exactly one parent.");
            }

            if (report.Attachments == null)
            {
                report.Attachments =
                    new List<FinancialReportAttachmentDto>();
            }

            if (report.Tables == null)
            {
                report.Tables =
                    new List<FinancialReportTableDto>();
            }

            AuditTemplatePackageResponse template = null;

            if (report.TemplateId.HasValue)
            {
                templates.TryGetValue(
                    report.TemplateId.Value,
                    out template);
            }

            List<FinancialReportTableEnvelope> tables =
                new List<FinancialReportTableEnvelope>();

            foreach (FinancialReportTableDto table
                     in report.Tables)
            {
                if (table == null)
                {
                    throw new InvalidOperationException(
                        $"Financial report {report.Id} contains a null table.");
                }

                if (table.Values == null)
                {
                    table.Values =
                        new List<FinancialReportValueDto>();
                }

                if (tablesById.ContainsKey(table.Id))
                {
                    throw new InvalidOperationException(
                        $"Accounting-entity package contains duplicate financial-report table {table.Id}.");
                }

                FinancialReportTableEnvelope tableEnvelope =
                    new FinancialReportTableEnvelope(
                        table,
                        report);

                tables.Add(tableEnvelope);
                tablesById.Add(table.Id, tableEnvelope);
            }

            FinancialReportEnvelope envelope =
                new FinancialReportEnvelope(
                    report,
                    financialStatement,
                    annualReport,
                    template,
                    tables);

            if (reportsById.ContainsKey(report.Id))
            {
                throw new InvalidOperationException(
                    $"Accounting-entity package contains duplicate financial report {report.Id}.");
            }

            reportsById.Add(report.Id, envelope);

            return envelope;
        }

        private static Dictionary<long, AuditTemplatePackageResponse> BuildTemplateIndex( IEnumerable<AuditTemplatePackageResponse> templates)
        {
            Dictionary<long, AuditTemplatePackageResponse> result =
                new Dictionary<long, AuditTemplatePackageResponse>();

            foreach (AuditTemplatePackageResponse template in templates)
            {
                if (template == null)
                {
                    throw new InvalidOperationException(
                        "Accounting-entity package contains a null template.");
                }

                if (template.Template == null)
                {
                    throw new InvalidOperationException(
                        "Accounting-entity package contains a template package without template metadata.");
                }

                long templateId =
                    template.Template.TemplateErpId;

                if (result.ContainsKey(templateId))
                {
                    throw new InvalidOperationException(
                        $"Accounting-entity package contains duplicate template {templateId}.");
                }

                result.Add(
                    templateId,
                    template);
            }

            return result;
        }

        private static void NormalizeCollections(AccountingEntityPackageDto package)
        {
            if (package.FinancialStatements == null)
            {
                package.FinancialStatements =
                    new List<FinancialStatementDto>();
            }

            if (package.AnnualReports == null)
            {
                package.AnnualReports =
                    new List<AnnualReportDto>();
            }

            if (package.Templates == null)
            {
                package.Templates =
                    new List<AuditTemplatePackageResponse>();
            }

            if (package.MissingTemplateIds == null)
            {
                package.MissingTemplateIds =
                    new List<long>();
            }
        }
    }
}