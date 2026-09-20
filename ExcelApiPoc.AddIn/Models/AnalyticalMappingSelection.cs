using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AnalyticalMappingSelection
    {
        public string AccountCode { get; set; }
        public string SyntheticAccountCode { get; set; }
        public bool IsExcluded { get; set; }
        public int? TableErpId { get; set; }
        public int? ReportRowNumber { get; set; }
        public string MappingSource { get; set; }
        public string MappedTo { get; set; }
    }

    internal sealed class AnalyticalMappingExistingState
    {
        public string AccountCode { get; set; }
        public string SyntheticAccountCode { get; set; }
        public string MappedTo { get; set; }
        public string MappingSource { get; set; }
        public string SuggestedMapping { get; set; }
        public string SuggestionConfidence { get; set; }
        public string SuggestionReason { get; set; }
    }

    internal sealed class AnalyticalMappingSelectionReadResult
    {
        public List<AnalyticalMappingSelection> Selections { get; } = new List<AnalyticalMappingSelection>();

        public List<AnalyticalMappingExistingState> States { get; } = new List<AnalyticalMappingExistingState>();

        public List<string> UnresolvedAccountCodes { get; } = new List<string>();

        public int ExcludedCount { get; set; }
        public int MappedCount { get; set; }
    }
}
