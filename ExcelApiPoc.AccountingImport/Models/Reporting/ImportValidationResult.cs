namespace ExcelApiPoc.AccountingImport.Models.Reporting
{
    public sealed class ImportValidationResult
    {
        public string Code { get; set; }
        public string Scope { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Difference { get; set; }
        public decimal Tolerance { get; set; }
        public SourceProvenance Source { get; set; }
    }
}
