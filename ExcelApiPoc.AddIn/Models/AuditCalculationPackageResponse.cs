namespace ExcelApiPoc.AddIn.Models
{
    public sealed class AuditCalculationPackageResponse
    {
        public int ContractVersion { get; set; }
        public long FinancialStatementId { get; set; }
        public long FinancialReportId { get; set; }
        public int RegisterUzTemplateId { get; set; }
        public string LegalFormValidationWarning { get; set; }
        public AuditTemplatePackageResponse CalculationPackage { get; set; }
    }
}
