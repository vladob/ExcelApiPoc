namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AuditWorkbookRecalculationResult
    {
        public AuditReportCalculationResult Calculation { get; set; }
        public AnalyticalMappingSelectionReadResult MappingSelections { get; set; }
        public int AccountCount { get; set; }
        public int MappingRuleCount { get; set; }
        public int CalculationDependencyCount { get; set; }
    }
}
