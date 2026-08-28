namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AccountSummary
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string SyntheticAccountCode { get; set; }
        public string FrameworkAccountCode { get; set; }
        public string FrameworkAccountName { get; set; }
        public bool? IsFrameworkMatch { get; set; }
        public int DebitEntryCount { get; set; }
        public decimal DebitTurnover { get; set; }
        public int CreditEntryCount { get; set; }
        public decimal CreditTurnover { get; set; }
        public decimal NetBalance => DebitTurnover - CreditTurnover;
        public decimal DebitBalance => NetBalance > 0 ? NetBalance : 0;
        public decimal CreditBalance => NetBalance < 0 ? -NetBalance : 0;
    }
}