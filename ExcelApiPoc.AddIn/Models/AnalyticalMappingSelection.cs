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
    }

    internal sealed class AnalyticalMappingSelectionReadResult
    {
        public List<AnalyticalMappingSelection> Selections { get; } = new List<AnalyticalMappingSelection>();

        public List<string> UnresolvedAccountCodes { get; } = new List<string>();

        public int ExcludedCount { get; set; }
        public int MappedCount { get; set; }
    }
}
