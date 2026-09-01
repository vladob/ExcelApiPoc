using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AuditReportReconciliationResult
    {
        public List<AuditReportRowReconciliation> Rows { get; }
            = new List<AuditReportRowReconciliation>();
    }

    internal sealed class AuditReportRowReconciliation
    {
        public int TableErpId { get; set; }
        public int RowNumber { get; set; }

        public decimal?[] RegisterUzValues { get; }
            = new decimal?[3];

        public decimal?[] Differences { get; }
            = new decimal?[3];
    }
}
