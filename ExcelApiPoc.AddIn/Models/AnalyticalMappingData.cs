using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AnalyticalMappingData
    {
        public List<AnalyticalMappingRow> Rows { get; } = new List<AnalyticalMappingRow>();

        public List<AnalyticalMappingOption> Options { get; } = new List<AnalyticalMappingOption>();
    }

    internal sealed class AnalyticalMappingRow
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string SyntheticAccountCode { get; set; }
        public string SyntheticAccountName { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string ValidationRangeName { get; set; }
    }

    internal sealed class AnalyticalMappingOption
    {
        public string SyntheticAccountCode { get; set; }
        public string OptionKey { get; set; }
        public string DisplayCaption { get; set; }
        public int? TableErpId { get; set; }
        public int? ReportRowNumber { get; set; }
        public int SortOrder { get; set; }
        public string ValidationRangeName { get; set; }
    }
}
