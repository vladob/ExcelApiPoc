namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal interface IIvesJournalSourceParser
    {
        string TechnicalType { get; }

        bool CanParse(string filePath);

        IvesJournalParseResult Parse(string filePath);
    }
}
