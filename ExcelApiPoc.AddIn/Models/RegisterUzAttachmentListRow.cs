using System;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class RegisterUzAttachmentListRow
    {
        public string Action { get; set; }

        public string Url { get; set; }

        public int? FiscalYear { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public string OwnerType { get; set; }

        public string ParentType { get; set; }

        public string ReportType { get; set; }

        public string TemplateName { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        public int? PageCount { get; set; }

        public string LanguageCode { get; set; }

        public long? TemplateId { get; set; }

        public long? FinancialStatementId { get; set; }

        public long? AnnualReportId { get; set; }

        public long? FinancialReportId { get; set; }

        public int? ReportOrdinal { get; set; }

        public long AttachmentId { get; set; }

        public int AttachmentOrdinal { get; set; }

        public string DataAvailability { get; set; }

        public DateTime? SubmissionDate { get; set; }

        internal DateTime PeriodToSortValue { get; set; }

        internal DateTime PeriodFromSortValue { get; set; }
    }
}
