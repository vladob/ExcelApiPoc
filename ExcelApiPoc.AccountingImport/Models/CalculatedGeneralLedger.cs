using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class CalculatedGeneralLedger
    {
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public bool JournalContainsOpeningRecords { get; set; }
        public bool JournalContainsClosingRecords { get; set; }
        public int SourceRecordCount { get; set; }

        public List<CalculatedGeneralLedgerRow> Rows { get; } =
            new List<CalculatedGeneralLedgerRow>();
    }

    public sealed class CalculatedGeneralLedgerRow
    {
        public string AccountCode { get; set; }
        public int OpeningDebitEntryCount { get; set; }
        public int OpeningCreditEntryCount { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public int DebitEntryCount { get; set; }
        public int CreditEntryCount { get; set; }
        public decimal DebitTurnover { get; set; }
        public decimal CreditTurnover { get; set; }
        public decimal? ClosingDebit { get; set; }
        public decimal? ClosingCredit { get; set; }
    }
}
