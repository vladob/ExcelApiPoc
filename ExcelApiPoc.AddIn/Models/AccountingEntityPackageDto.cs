using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AccountingEntityPackageDto
    {
        public int ContractVersion { get; set; }

        public DateTimeOffset GeneratedAtUtc { get; set; }

        public AccountingEntityDto Entity { get; set; }

        public List<FinancialStatementDto> FinancialStatements { get; set; }

        public List<AnnualReportDto> AnnualReports { get; set; }

        public List<AuditTemplatePackageResponse> Templates { get; set; }

        public List<long> MissingTemplateIds { get; set; }
    }

    internal sealed class AccountingEntityDto
    {
        public long Id { get; set; }

        public string Ico { get; set; }

        public string Dic { get; set; }

        public string Sid { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string PostalCode { get; set; }

        public DateTime? EstablishedDate { get; set; }

        public DateTime? CancellationDate { get; set; }

        public string LegalFormCode { get; set; }

        public string SkNaceCode { get; set; }

        public string OrganizationSizeCode { get; set; }

        public string OwnershipTypeCode { get; set; }

        public string RegionCode { get; set; }

        public string DistrictCode { get; set; }

        public string RegisteredOfficeCode { get; set; }

        public bool? IsConsolidated { get; set; }
    }

    internal sealed class FinancialStatementDto
    {
        public long Id { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public DateTime? SubmissionDate { get; set; }

        public DateTime? PreparationDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? AssemblyDate { get; set; }

        public DateTime? AuditorReportAttachmentDate { get; set; }

        public string FundName { get; set; }

        public string LeiCode { get; set; }

        public bool? IsConsolidated { get; set; }

        public bool? IsConsolidatedCentralGovernment { get; set; }

        public bool? IsSummaryPublicAdministration { get; set; }

        public string StatementType { get; set; }

        public List<FinancialReportDto> FinancialReports { get; set; }
    }

    internal sealed class AnnualReportDto
    {
        public long Id { get; set; }

        public string EntityNameAtSubmission { get; set; }

        public string AnnualReportType { get; set; }

        public string FundName { get; set; }

        public string LeiCode { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public DateTime? SubmissionDate { get; set; }

        public DateTime? AssemblyDate { get; set; }

        public List<AnnualReportAttachmentDto> Attachments { get; set; }

        public List<FinancialReportDto> FinancialReports { get; set; }
    }

    internal sealed class AnnualReportAttachmentDto
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        public string LanguageCode { get; set; }
    }

    internal sealed class FinancialReportDto
    {
        public long Id { get; set; }

        public long? TemplateId { get; set; }

        public string CurrencyCode { get; set; }

        public string TaxOfficeCode { get; set; }

        public string DataAvailability { get; set; }

        public FinancialReportTitlePageDto TitlePage { get; set; }

        public List<FinancialReportAttachmentDto> Attachments { get; set; }

        public List<FinancialReportTableDto> Tables { get; set; }
    }

    internal sealed class FinancialReportTitlePageDto
    {
        public string EntityName { get; set; }

        public string Ico { get; set; }

        public string Dic { get; set; }

        public string Sid { get; set; }

        public string Address { get; set; }

        public string LegalFormCode { get; set; }

        public string SkNaceCode { get; set; }

        public string ReportType { get; set; }

        public bool? IsConsolidated { get; set; }

        public bool? IsConsolidatedCentralGovernment { get; set; }

        public bool? IsSummaryPublicAdministration { get; set; }

        public string EntityType { get; set; }

        public string CommercialRegister { get; set; }

        public string FundName { get; set; }

        public string LeiCode { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public string PreviousPeriodFrom { get; set; }

        public string PreviousPeriodTo { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? PreparationDate { get; set; }

        public DateTime? AssemblyDate { get; set; }

        public DateTime? AuditorReportAttachmentDate { get; set; }
    }

    internal sealed class FinancialReportAttachmentDto
    {
        public long Id { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        public int? PageCount { get; set; }

        public string LanguageCode { get; set; }
    }

    internal sealed class FinancialReportTableDto
    {
        public long Id { get; set; }

        public long? TemplateTableId { get; set; }

        public int TableOrdinal { get; set; }

        public string NameSk { get; set; }

        public string NameEn { get; set; }

        public List<FinancialReportValueDto> Values { get; set; }
    }

    internal sealed class FinancialReportValueDto
    {
        public int ValueOrdinal { get; set; }

        public int RowOrdinal { get; set; }

        public int DataColumnOrdinal { get; set; }

        public decimal NumericValue { get; set; }

        public string SourceValue { get; set; }
    }
}