using System;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesGeneralLedgerSourceRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRowNumber { get; set; }
        public IvesGeneralLedgerRowKind Kind { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string DocumentNumber { get; set; }
        public string AccountCode { get; set; }
        public string Text { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? DebitTurnover { get; set; }
        public decimal? CreditTurnover { get; set; }
        public decimal? ClosingBalance { get; set; }
    }
}
