using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesGeneralLedgerParseResult
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string WorksheetName { get; set; }
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int SourceRowCount { get; set; }
        public List<IvesGeneralLedgerSourceRow> StructuralRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> AccountRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> DocumentRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> DocumentSummaryRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> SyntheticAccountRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> SyntheticSubtotalRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> ReportTotalRows { get; } = new List<IvesGeneralLedgerSourceRow>();
        public List<IvesGeneralLedgerSourceRow> UnclassifiedRows { get; } = new List<IvesGeneralLedgerSourceRow>();
    }
}
