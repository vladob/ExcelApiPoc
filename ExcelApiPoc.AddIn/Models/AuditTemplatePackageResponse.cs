using System;

namespace ExcelApiPoc.AddIn.Models
{
    public sealed class AuditTemplatePackageResponse
    {
        public int ContractVersion { get; set; }

        public DateTime GeneratedAtUtc { get; set; }

        public AuditTemplateDefinitionResponse Template { get; set; }

        public AuditAccountGroupDefinitionResponse[] AccountGroups { get; set; }

        public AuditReportMappingRuleDefinitionResponse[] ReportMappingRules
        {
            get;
            set;
        }
    }

    public sealed class AuditTemplateDefinitionResponse
    {
        public int TemplateErpId { get; set; }

        public string Name { get; set; }

        public string MfSpecification { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public string AccountingModel { get; set; }

        public AuditReportTableDefinitionResponse[] Tables { get; set; }
    }

    public sealed class AuditReportTableDefinitionResponse
    {
        public int TableErpId { get; set; }

        public string NameSk { get; set; }

        public string NameEn { get; set; }

        public int? NumberOfColumns { get; set; }

        public int? NumberOfDataColumns { get; set; }

        public bool DontHaveRowNumbers { get; set; }

        public AuditReportHeaderDefinitionResponse[] Headers { get; set; }

        public AuditReportRowDefinitionResponse[] Rows { get; set; }
    }

    public sealed class AuditReportHeaderDefinitionResponse
    {
        public string TextSk { get; set; }

        public string TextEn { get; set; }

        public int RowPosition { get; set; }

        public int ColumnPosition { get; set; }

        public int RowSpan { get; set; }

        public int ColumnSpan { get; set; }
    }

    public sealed class AuditReportRowDefinitionResponse
    {
        public int? RowNumber { get; set; }

        public string Designation { get; set; }

        public string TextSk { get; set; }

        public string TextEn { get; set; }

        public bool IsSumRow { get; set; }

        public string CategorySk { get; set; }
    }

    public sealed class AuditAccountGroupDefinitionResponse
    {
        public string Account { get; set; }

        public string Title { get; set; }

        public string Legend { get; set; }

        public string AssetsValueSource { get; set; }

        public string LiabilitiesValueSource { get; set; }
    }

    public sealed class AuditReportMappingRuleDefinitionResponse
    {
        public int TableErpId { get; set; }

        public string Account3 { get; set; }

        public int ReportRowNumber { get; set; }

        public string AccountTitle { get; set; }

        public bool RequiresAnalyticalMapping { get; set; }

        public bool IncludeInBrutto { get; set; }

        public bool IncludeInCorrection { get; set; }

        public string Side { get; set; }

        public string ValueSource { get; set; }
    }

}