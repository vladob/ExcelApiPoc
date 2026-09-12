namespace ExcelApiPoc.AccountingImport.Models.Reporting
{
    public enum ImportDiagnosticSeverity
    {
        Information,
        Warning,
        Error
    }

    public sealed class ImportDiagnostic
    {
        public string Code { get; set; }
        public ImportDiagnosticSeverity Severity { get; set; }
        public string Message { get; set; }
        public SourceProvenance Source { get; set; }
    }
}
