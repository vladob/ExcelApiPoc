namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal interface IIvesAccountingFrameworkSourceParser
    {
        string TechnicalType { get; }

        bool CanParse(string filePath);

        IvesAccountingFrameworkParseResult Parse(string filePath);
    }
}
