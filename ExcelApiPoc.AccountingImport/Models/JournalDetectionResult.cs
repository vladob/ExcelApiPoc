namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class JournalDetectionResult
    {
        public string TechnicalType { get; set; }
        public string AccountingFormat { get; set; }
        public string Ico { get; set; }
        public int? FiscalYear { get; set; }
        public string CompanyName { get; set; }
    }
}