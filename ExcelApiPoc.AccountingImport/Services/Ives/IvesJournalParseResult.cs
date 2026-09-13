using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesJournalParseResult
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string WorksheetName { get; set; }
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int SourceRowCount { get; set; }
        public IvesJournalColumnLayout Layout { get; set; }

        public List<IvesJournalSourceRow> StructuralRows { get; } =
            new List<IvesJournalSourceRow>();
        public List<IvesJournalSourceRow> TransactionRows { get; } =
            new List<IvesJournalSourceRow>();
        public List<IvesJournalSourceRow> ModuleRows { get; } =
            new List<IvesJournalSourceRow>();
        public List<IvesJournalSourceRow> ReportTotalRows { get; } =
            new List<IvesJournalSourceRow>();
        public List<IvesJournalSourceRow> UnclassifiedRows { get; } =
            new List<IvesJournalSourceRow>();
    }
}
