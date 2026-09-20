using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesGeneralLedgerActivity
    {
        public string Name { get; set; }
        public string Currency { get; set; }

        public List<IvesGeneralLedgerSourceRow> AccountRows { get; } =
            new List<IvesGeneralLedgerSourceRow>();

        public List<IvesGeneralLedgerSourceRow> DocumentRows { get; } =
            new List<IvesGeneralLedgerSourceRow>();

        public List<IvesGeneralLedgerSourceRow> DocumentSummaryRows { get; } =
            new List<IvesGeneralLedgerSourceRow>();

        public List<IvesGeneralLedgerSourceRow> SyntheticSummaryRows { get; } =
            new List<IvesGeneralLedgerSourceRow>();

        public List<IvesGeneralLedgerSourceRow> ReportTotalRows { get; } =
            new List<IvesGeneralLedgerSourceRow>();
    }
}
