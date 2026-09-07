namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class AccountingImportPackage
    {
        public string AccountingFormat { get; set; }
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public int? ExportStage { get; set; }

        public JournalImport Journal { get; set; }
        public GeneralLedgerImport GeneralLedger { get; set; }

        public bool HasGeneralLedger =>
            GeneralLedger != null;
    }
}