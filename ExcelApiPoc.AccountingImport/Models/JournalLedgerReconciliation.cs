using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models
{
    public enum OpeningBalanceSource
    {
        Journal = 0,
        GeneralLedger = 1
    }

    public sealed class JournalLedgerAccountReconciliation
    {
        public string AccountCode { get; set; }
        public string GeneralLedgerAccountName { get; set; }

        public bool HasJournalActivity { get; set; }
        public bool HasGeneralLedgerAccount { get; set; }

        public decimal JournalOpeningDebit { get; set; }
        public decimal JournalOpeningCredit { get; set; }
        public decimal JournalDebitTurnover { get; set; }
        public decimal JournalCreditTurnover { get; set; }

        public decimal LedgerOpeningDebit { get; set; }
        public decimal LedgerOpeningCredit { get; set; }
        public decimal LedgerDebitTurnover { get; set; }
        public decimal LedgerCreditTurnover { get; set; }
        public decimal LedgerClosingDebit { get; set; }
        public decimal LedgerClosingCredit { get; set; }

        public decimal EffectiveOpeningDebit { get; set; }
        public decimal EffectiveOpeningCredit { get; set; }

        public decimal CalculatedClosingDebit { get; set; }
        public decimal CalculatedClosingCredit { get; set; }

        public decimal OpeningDebitDifference { get; set; }
        public decimal OpeningCreditDifference { get; set; }
        public decimal DebitTurnoverDifference { get; set; }
        public decimal CreditTurnoverDifference { get; set; }
        public decimal ClosingBalanceDifference { get; set; }

        public bool IsReconciled { get; set; }
    }

    public sealed class JournalLedgerReconciliationResult
    {
        public bool JournalContainsOpeningRecords { get; set; }
        public bool JournalContainsClosingRecords { get; set; }

        public OpeningBalanceSource OpeningBalanceSource
        {
            get;
            set;
        }

        public int JournalAccountCount { get; set; }
        public int LedgerAccountCount { get; set; }
        public int JournalOnlyAccountCount { get; set; }
        public int LedgerOnlyAccountCount { get; set; }
        public int ReconciledAccountCount { get; set; }
        public int DifferentAccountCount { get; set; }

        public decimal OpeningDebitDifference { get; set; }
        public decimal OpeningCreditDifference { get; set; }
        public decimal DebitTurnoverDifference { get; set; }
        public decimal CreditTurnoverDifference { get; set; }
        public decimal ClosingBalanceDifference { get; set; }

        public List<JournalLedgerAccountReconciliation>
            Accounts
        { get; } =
                new List<JournalLedgerAccountReconciliation>();

        public bool IsReconciled =>
            DifferentAccountCount == 0;
    }
}