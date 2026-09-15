namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal enum IvesGeneralLedgerRowKind
    {
        Blank, Header, Title, SectionTitle, Account, Document,
        DocumentSummary, SyntheticAccount, SyntheticSubtotal,
        ReportTotal, AmountContinuation, Unclassified
    }
}
