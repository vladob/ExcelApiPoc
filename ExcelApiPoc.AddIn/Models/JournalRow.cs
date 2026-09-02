using System;

namespace ExcelApiPoc.AddIn.Models
{
    internal enum JournalRecordKind
    {
        Normal = 0,
        Opening = 1,
        Closing = 2
    }

    internal sealed class JournalRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRecordNumber { get; set; }
        public int? SourceStartLineNumber { get; set; }
        public int? SourceEndLineNumber { get; set; }
        public string SourceLocation { get; set; }
        public bool TextNormalizationApplied { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime PostingDate { get; set; }
        public string Description { get; set; }
        public JournalRecordKind RecordKind { get; set; }
        public bool UsedForReportCalculation
        {
            get { return RecordKind != JournalRecordKind.Closing; }
        }
        public string DebitAccount { get; set; }
        public decimal? DebitAmount { get; set; }
        public string DebitSection { get; set; }
        public string DebitItem { get; set; }
        public string DebitFundingSource { get; set; }
        public string DebitCostCenter { get; set; }
        public string DebitOrder { get; set; }
        public string CreditAccount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string CreditSection { get; set; }
        public string CreditItem { get; set; }
        public string CreditFundingSource { get; set; }
        public string CreditCostCenter { get; set; }
        public string CreditOrder { get; set; }
    }
}