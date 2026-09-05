using ExcelApiPoc.AddIn.Models;

namespace ExcelApiPoc.AddIn.Services
{
    internal sealed class RegisterUzFinancialReportSelection
    {
        public FinancialStatementEnvelope Statement { get; set; }

        public FinancialReportEnvelope Report { get; set; }

        public int TemplateErpId { get; set; }

        public long RegisterUzReportId { get; set; }
    }
}
