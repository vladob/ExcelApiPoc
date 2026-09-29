using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AccountingImport.Services.Common
{
    public static class JournalDateExceptionService
    {
        public const string DiagnosticCode = "AJ_DATE_OUTSIDE_FISCAL_YEAR";

        public static int Apply(JournalImport journal, int fiscalYear)
        {
            if (journal == null)
                throw new ArgumentNullException(nameof(journal));

            if (fiscalYear < 1900 || fiscalYear > 9999)
                throw new ArgumentOutOfRangeException(nameof(fiscalYear));

            List<JournalRow> exceptionalRows = journal.Rows
                .Where(row => row.PostingDate.Year != fiscalYear)
                .ToList();

            foreach (JournalRow row in exceptionalRows)
            {
                if (!row.DateExceptionResolution.HasValue)
                {
                    row.DateExceptionResolution =
                        JournalDateExceptionResolution.Excluded;
                }
            }

            if (exceptionalRows.Count == 0 || journal.ImportReport == null)
                return exceptionalRows.Count;

            if (!journal.ImportReport.Diagnostics.Any(
                    diagnostic => string.Equals(
                        diagnostic.Code,
                        DiagnosticCode,
                        StringComparison.Ordinal)))
            {
                List<JournalRow> datedRows = exceptionalRows
                    .Where(row => row.PostingDate != DateTime.MinValue).ToList();
                string range = datedRows.Count == 0 ? string.Empty :
                    " Date range: " + datedRows.Min(row => row.PostingDate).ToString("yyyy-MM-dd") +
                    " to " + datedRows.Max(row => row.PostingDate).ToString("yyyy-MM-dd") + ".";

                journal.ImportReport.Diagnostics.Add(
                    new ImportDiagnostic
                    {
                        Code = DiagnosticCode,
                        Severity = ImportDiagnosticSeverity.Warning,
                        Message = exceptionalRows.Count +
                            " accounting-journal row(s) have missing or out-of-range posting dates for fiscal year " +
                            fiscalYear +
                            ". They were imported and excluded from calculation pending auditor review. " +
                            range,
                        Source = new SourceProvenance
                        {
                            SourceFileName = journal.SourceFileName,
                            RecordSet = "JournalRows"
                        }
                    });
            }

            return exceptionalRows.Count;
        }
    }
}
