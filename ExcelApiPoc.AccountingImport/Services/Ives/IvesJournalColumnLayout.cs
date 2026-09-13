namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesJournalColumnLayout
    {
        public int ColumnCount { get; set; }
        public int HeaderRowNumber { get; set; }
        public int DateColumn { get; set; }
        public int DocumentNumberColumn { get; set; }
        public int DebitAccountColumn { get; set; }
        public int CreditAccountColumn { get; set; }
        public int AmountColumn { get; set; }
        public int CurrencyColumn { get; set; }
        public int TextColumn { get; set; }
    }
}
