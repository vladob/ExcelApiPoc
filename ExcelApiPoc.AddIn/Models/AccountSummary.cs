namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AccountSummary
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNameSource { get; set; }
        public string EntityAccountName { get; set; }
        public string SyntheticAccountCode { get; set; }
        public string FrameworkAccountCode { get; set; }
        public string FrameworkAccountName { get; set; }
        public bool? IsFrameworkMatch { get; set; }
        public string GeneralLedgerAccountName { get; set; }
        public string AccountNameComparisonStatus { get; set; }
        public int DebitEntryCount { get; set; }
        public decimal DebitTurnover { get; set; }
        public int CreditEntryCount { get; set; }
        public decimal CreditTurnover { get; set; }
        public decimal NetBalance => DebitTurnover - CreditTurnover;
        public decimal DebitBalance => NetBalance > 0 ? NetBalance : 0;
        public decimal CreditBalance => NetBalance < 0 ? -NetBalance : 0;
        public decimal JournalLedgerOpeningDebit { get; set; }
        public decimal JournalLedgerOpeningCredit { get; set; }
        public decimal JournalLedgerDebitTurnover { get; set; }
        public decimal JournalLedgerCreditTurnover { get; set; }
        public decimal JournalLedgerClosingBalance =>
            JournalLedgerOpeningDebit - JournalLedgerOpeningCredit +
            JournalLedgerDebitTurnover - JournalLedgerCreditTurnover;
        public decimal LedgerOpeningDebit { get; set; }
        public decimal LedgerOpeningCredit { get; set; }
        public decimal LedgerDebitTurnover { get; set; }
        public decimal LedgerCreditTurnover { get; set; }
        public decimal LedgerClosingDebit { get; set; }
        public decimal LedgerClosingCredit { get; set; }
        public decimal OpeningDebitDifference => JournalLedgerOpeningDebit - LedgerOpeningDebit;
        public decimal OpeningCreditDifference => JournalLedgerOpeningCredit - LedgerOpeningCredit;
        public decimal DebitTurnoverDifference => JournalLedgerDebitTurnover - LedgerDebitTurnover;
        public decimal CreditTurnoverDifference => JournalLedgerCreditTurnover - LedgerCreditTurnover;
        public decimal ClosingBalanceDifference =>
            JournalLedgerClosingBalance - (LedgerClosingDebit - LedgerClosingCredit);
        public string LedgerReconciliationStatus { get; set; }
    }
}
