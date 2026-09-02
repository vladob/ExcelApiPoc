using System;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class RegisterUzReportListRow
    {
        public string Action { get; set; }

        public string TargetWorksheetName { get; set; }

        public int? FiscalYear { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public string ReportType { get; set; }

        public string ParentType { get; set; }

        public string TemplateName { get; set; }

        public string TableName { get; set; }

        public long? TemplateId { get; set; }

        public long? TemplateTableId { get; set; }

        public long? FinancialStatementId { get; set; }

        public long? AnnualReportId { get; set; }

        public long FinancialReportId { get; set; }

        public int ReportOrdinal { get; set; }

        public long FinancialReportTableId { get; set; }

        public int TableOrdinal { get; set; }

        public string CurrencyCode { get; set; }

        public string DataAvailability { get; set; }

        public DateTime? SubmissionDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        internal DateTime PeriodToSortValue { get; set; }

        internal DateTime PeriodFromSortValue { get; set; }
    }
}
