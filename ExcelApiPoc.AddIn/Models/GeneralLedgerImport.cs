using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class GeneralLedgerImport
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileHash { get; set; }
        public string TechnicalType { get; set; }
        public string AccountingFormat { get; set; }
        public string Ico { get; set; }
        public string CompanyName { get; set; }
        public int FiscalYear { get; set; }
        public int ThroughMonth { get; set; }
        public string PeriodHeader { get; set; }
        public DateTime ImportedAtUtc { get; set; }
        public int NormalizedTextFieldCount { get; set; }
        public List<GeneralLedgerRow> Rows { get; } = new List<GeneralLedgerRow>();
    }

    internal sealed class GeneralLedgerRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRecordNumber { get; set; }
        public string SyntheticCode { get; set; }
        public string AnalyticalCode { get; set; }
        public string AccountCode { get; set; }
        public string Type { get; set; }
        public string P { get; set; }
        public string Section { get; set; }
        public string Item { get; set; }
        public string FundingSource { get; set; }
        public string Program { get; set; }
        public string CostCenter { get; set; }
        public string Order { get; set; }
        public string AccountName { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal AnnualDebitTurnover { get; set; }
        public decimal AnnualCreditTurnover { get; set; }
        public decimal PeriodDebitTurnover { get; set; }
        public decimal PeriodCreditTurnover { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
        public decimal Plan { get; set; }
    }

    internal sealed class GeneralLedgerReconciliationResult
    {
        public int JournalAccountCount { get; set; }
        public int LedgerAccountCount { get; set; }
        public int MatchedAccountCount { get; set; }
        public int JournalOnlyAccountCount { get; set; }
        public int LedgerOnlyAccountCount { get; set; }
        public int ReconciledAccountCount { get; set; }
        public int DifferentAccountCount { get; set; }
        public decimal OpeningDebitDifference { get; set; }
        public decimal OpeningCreditDifference { get; set; }
        public decimal DebitTurnoverDifference { get; set; }
        public decimal CreditTurnoverDifference { get; set; }
        public decimal ClosingBalanceDifference { get; set; }
        public bool IsReconciled =>
            JournalOnlyAccountCount == 0 && LedgerOnlyAccountCount == 0 &&
            DifferentAccountCount == 0;
    }
}
