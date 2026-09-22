namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    public sealed class MkSoftGeneralLedgerSourceRow
    {
        public int SourceRowNumber { get; set; }
        public string Account { get; set; }
        public string AccountName { get; set; }
        public string SyntheticAccount { get; set; }
        public string SyntheticAccountName { get; set; }
        public string AccountClass { get; set; }
        public string AccountClassName { get; set; }
        public string CostCenter { get; set; }
        public string Order { get; set; }
        public string TurnoverCode { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal DebitTurnover { get; set; }
        public decimal CreditTurnover { get; set; }
        public decimal SecondaryDebitTurnover { get; set; }
        public decimal SecondaryCreditTurnover { get; set; }
        public decimal GrossClosingDebit { get; set; }
        public decimal GrossClosingCredit { get; set; }
    }
}
