using System;

namespace ExcelApiPoc.AddIn.Models
{
    public sealed class AuditTemplateMetadataResponse
    {
        public int TemplateErpId { get; set; }

        public string Name { get; set; }

        public string MfSpecification { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public AuditTableMetadataResponse[] Tables { get; set; }
    }

    public sealed class AuditTableMetadataResponse
    {
        public int TableErpId { get; set; }

        public string NameSk { get; set; }

        public string NameEn { get; set; }

        public int NumberOfColumns { get; set; }

        public int NumberOfDataColumns { get; set; }

        public bool DontHaveRowNumbers { get; set; }
    }
}