using System;
using System.Linq;
using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;

namespace ExcelApiPoc.AccountingImport.Services
{
    public static class AccountingFrameworkYearValidator
    {
        public static void Validate(AccountingFrameworkImport framework, int selectedFiscalYear)
        {
            if (framework == null) throw new ArgumentNullException(nameof(framework));
            // AF is supporting account metadata. An absent year is acceptable;
            // a stated different year is advisory. Preserve the source year.
            if (framework.FiscalYear == 0 || framework.FiscalYear == selectedFiscalYear) return;
            if (framework.ImportReport == null)
                framework.ImportReport = new ImportReport
                {
                    SourceFileName = framework.SourceFileName,
                    AccountingFormat = framework.AccountingFormat,
                    ImportType = "AF"
                };
            const string code = "AF_FISCAL_YEAR_MISMATCH";
            if (!framework.ImportReport.Diagnostics.Any(d => d.Code == code))
                framework.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = code,
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = "The accounting framework states fiscal year " + framework.FiscalYear +
                        ", which differs from the selected fiscal year " + selectedFiscalYear + "."
                });
        }
    }
}
