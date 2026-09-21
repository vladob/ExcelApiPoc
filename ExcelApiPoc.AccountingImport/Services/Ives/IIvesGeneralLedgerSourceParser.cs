namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal interface IIvesGeneralLedgerSourceParser
    {
        string TechnicalType { get; }

        bool CanParse(string filePath);

        IvesGeneralLedgerParseResult Parse(string filePath);
    }
}
