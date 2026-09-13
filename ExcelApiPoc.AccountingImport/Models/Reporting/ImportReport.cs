using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models.Reporting
{
    public sealed class ImportReport
    {
        public string AccountingFormat { get; set; }
        public string ImportType { get; set; }
        public string SourceFileName { get; set; }

        public Dictionary<string, int> RecordCounts { get; } =
            new Dictionary<string, int>();

        public List<ImportValidationResult> ValidationResults { get; } =
            new List<ImportValidationResult>();

        public List<ImportDiagnostic> Diagnostics { get; } =
            new List<ImportDiagnostic>();

        public ImportPerformanceMetrics Performance { get; set; }

        public bool IsValid
        {
            get
            {
                foreach (ImportDiagnostic diagnostic in Diagnostics)
                {
                    if (diagnostic.Severity == ImportDiagnosticSeverity.Error)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
