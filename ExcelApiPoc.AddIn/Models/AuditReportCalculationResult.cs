using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AuditReportCalculationResult
    {
        public List<AuditReportRowCalculation> Rows { get; } = new List<AuditReportRowCalculation>();
        public List<AuditAnalyticalMappingRequirement> AnalyticalRequirements
        { get; } = new List<AuditAnalyticalMappingRequirement>();
        public List<AccountSummary> UnmappedAccounts { get; } = new List<AccountSummary>();
        public int AutomaticRuleCount { get; set; }
        public int AutomaticRulesWithValues { get; set; }
        public bool IsComplete => AnalyticalRequirements.Count == 0;
    }

    internal sealed class AuditReportRowCalculation
    {
        public int TableErpId { get; set; }
        public int RowNumber { get; set; }
        public decimal PrimaryValue { get; set; }
        public decimal SecondaryValue { get; set; }
        public decimal CalculatedValue { get; set; }
    }

    internal sealed class AuditAnalyticalMappingRequirement
    {
        public int TableErpId { get; set; }
        public int ReportRowNumber { get; set; }
        public string SyntheticAccountCode { get; set; }
        public string AccountTitle { get; set; }
        public string BalanceSide { get; set; }
        public string ValueSource { get; set; }
        public bool IncludeInPrimary { get; set; }
        public bool IncludeInSecondary { get; set; }
        public string[] CandidateAccountCodes { get; set; }
        public decimal CandidateValue { get; set; }
    }
}