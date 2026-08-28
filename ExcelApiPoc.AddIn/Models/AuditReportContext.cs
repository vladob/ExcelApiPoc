namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AuditReportContext
    {
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public int TemplateErpId { get; set; }
        public string SelectionSource { get; set; }
        public string RegisterUzReportId { get; set; }
    }
}