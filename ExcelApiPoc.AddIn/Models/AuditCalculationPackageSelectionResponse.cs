using System;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AuditCalculationPackageSelectionResponse
    {
        public int ContractVersion { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public long RegisterUzEntityId { get; set; }
        public string EntityName { get; set; }
        public string LegalFormCode { get; set; }
        public long FinancialStatementId { get; set; }
        public long FinancialReportId { get; set; }
        public int RegisterUzTemplateId { get; set; }
        public AuditTemplatePackageResponse CalculationPackage { get; set; }
    }
}
