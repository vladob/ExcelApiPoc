namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class AccountingImportRequest
    {
        public string AccountingFormat { get; set; }
        public string JournalFilePath { get; set; }
        public string GeneralLedgerFilePath { get; set; }
        public string ExpectedIco { get; set; }
        public int ExpectedFiscalYear { get; set; }
    }
}