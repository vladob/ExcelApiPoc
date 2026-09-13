using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesJournalSourceRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRowNumber { get; set; }
        public int? RelatedSourceRowNumber { get; set; }
        public IvesJournalRowKind Kind { get; set; }
        public DateTime? PostingDate { get; set; }
        public string DocumentNumber { get; set; }
        public string DebitCompositeAccount { get; set; }
        public string CreditCompositeAccount { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string Text { get; set; }
        public string Module { get; set; }

        public List<decimal> ReportedAmounts { get; } =
            new List<decimal>();
    }
}
