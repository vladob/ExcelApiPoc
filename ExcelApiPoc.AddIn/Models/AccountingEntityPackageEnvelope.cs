using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AccountingEntityPackageEnvelope
    {
        public AccountingEntityPackageEnvelope(
            AccountingEntityPackageDto package,
            IReadOnlyList<FinancialStatementEnvelope> financialStatements,
            IReadOnlyList<AnnualReportEnvelope> annualReports,
            IReadOnlyDictionary<long, FinancialReportEnvelope> reportsById,
            IReadOnlyDictionary<long, FinancialReportTableEnvelope> tablesById,
            IReadOnlyDictionary<long, AuditTemplatePackageResponse> templatesById)
        {
            Package = package;
            FinancialStatements = financialStatements;
            AnnualReports = annualReports;
            ReportsById = reportsById;
            TablesById = tablesById;
            TemplatesById = templatesById;
        }

        public IReadOnlyDictionary<long, FinancialReportEnvelope> ReportsById { get; }

        public IReadOnlyDictionary<long, FinancialReportTableEnvelope> TablesById { get; }

        public IReadOnlyDictionary<long, AuditTemplatePackageResponse> TemplatesById { get; }

        public AccountingEntityPackageDto Package { get; }

        public AccountingEntityDto Entity => Package.Entity;

        public IReadOnlyList<FinancialStatementEnvelope> FinancialStatements { get; }

        public IReadOnlyList<AnnualReportEnvelope> AnnualReports { get; }

        public IReadOnlyList<AuditTemplatePackageResponse> Templates => Package.Templates;

        public IReadOnlyList<long> MissingTemplateIds => Package.MissingTemplateIds;

        public int FinancialReportCount => ReportsById.Count;

        public int FinancialReportTableCount => TablesById.Count;

        public int FinancialStatementReportCount
        {
            get
            {
                int count = 0;

                foreach (FinancialStatementEnvelope statement
                         in FinancialStatements)
                {
                    count += statement.FinancialReports.Count;
                }

                return count;
            }
        }

        public int AnnualReportFinancialReportCount
        {
            get
            {
                int count = 0;

                foreach (AnnualReportEnvelope annualReport
                         in AnnualReports)
                {
                    count += annualReport.FinancialReports.Count;
                }

                return count;
            }
        }

        public int UnresolvedTemplateReportCount
        {
            get
            {
                int count = 0;

                foreach (FinancialReportEnvelope report
                         in ReportsById.Values)
                {
                    if (report.Report.TemplateId.HasValue &&
                        !report.HasTemplate)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int AnnualReportAttachmentCount
        {
            get
            {
                int count = 0;

                foreach (AnnualReportEnvelope annualReport
                         in AnnualReports)
                {
                    count += annualReport.Attachments.Count;
                }

                return count;
            }
        }

        public int FinancialReportAttachmentCount
        {
            get
            {
                int count = 0;

                foreach (FinancialReportEnvelope report
                         in ReportsById.Values)
                {
                    count += report.Attachments.Count;
                }

                return count;
            }
        }

    }

    internal sealed class FinancialStatementEnvelope
    {
        public FinancialStatementEnvelope(FinancialStatementDto statement, IReadOnlyList<FinancialReportEnvelope> financialReports)
        {
            Statement = statement;
            FinancialReports = financialReports;
        }

        public FinancialStatementDto Statement { get; }

        public IReadOnlyList<FinancialReportEnvelope> FinancialReports { get; }
    }

    internal sealed class AnnualReportEnvelope
    {
        public AnnualReportEnvelope(AnnualReportDto annualReport, IReadOnlyList<FinancialReportEnvelope> financialReports)
        {
            AnnualReport = annualReport;
            FinancialReports = financialReports;
        }

        public AnnualReportDto AnnualReport { get; }

        public IReadOnlyList<AnnualReportAttachmentDto> Attachments =>
            AnnualReport.Attachments;

        public IReadOnlyList<FinancialReportEnvelope> FinancialReports { get; }
    }

    internal sealed class FinancialReportEnvelope
    {
        public FinancialReportEnvelope(FinancialReportDto report, FinancialStatementDto financialStatement, AnnualReportDto annualReport, AuditTemplatePackageResponse template, IReadOnlyList<FinancialReportTableEnvelope> tables)
        {
            Report = report;
            FinancialStatement = financialStatement;
            AnnualReport = annualReport;
            Template = template;
            Tables = tables;
        }

        public FinancialReportDto Report { get; }

        public FinancialStatementDto FinancialStatement { get; }

        public AnnualReportDto AnnualReport { get; }

        public bool BelongsToFinancialStatement => FinancialStatement != null;

        public bool BelongsToAnnualReport => AnnualReport != null;

        public AuditTemplatePackageResponse Template { get; }

        public bool HasTemplate => Template != null;

        public FinancialReportTitlePageDto TitlePage => Report.TitlePage;

        public IReadOnlyList<FinancialReportAttachmentDto> Attachments => Report.Attachments;

        public IReadOnlyList<FinancialReportTableEnvelope> Tables { get; }
    }

    internal sealed class FinancialReportTableEnvelope
    {
        public FinancialReportTableEnvelope(FinancialReportTableDto table, FinancialReportDto financialReport)
        {
            Table = table;
            FinancialReport = financialReport;
        }

        public FinancialReportTableDto Table { get; }

        public FinancialReportDto FinancialReport { get; }

        public IReadOnlyList<FinancialReportValueDto> Values => Table.Values;
    }
}