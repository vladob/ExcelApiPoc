namespace ExcelApiPoc.AccountingImport.Models.Reporting
{
    public sealed class SourceProvenance
    {
        public string SourceFileName { get; set; }
        public string WorksheetName { get; set; }
        public string RecordSet { get; set; }
        public int? SequenceNumber { get; set; }
        public int? SourceRowNumber { get; set; }
    }
}
