using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class AccountingImportRequest
    {
        public string AccountingFormat { get; set; }
        public string JournalFilePath { get; set; }
        public List<string> JournalFilePaths { get; } = new List<string>();
        public string GeneralLedgerFilePath { get; set; }
        public string ExpectedIco { get; set; }
        public int ExpectedFiscalYear { get; set; }
    }
}
