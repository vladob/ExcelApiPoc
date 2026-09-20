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
                DateTime firstDate = exceptionalRows.Min(row => row.PostingDate);
                DateTime lastDate = exceptionalRows.Max(row => row.PostingDate);

                journal.ImportReport.Diagnostics.Add(
                    new ImportDiagnostic
                    {
                        Code = DiagnosticCode,
                        Severity = ImportDiagnosticSeverity.Warning,
                        Message = exceptionalRows.Count +
                            " accounting-journal row(s) contain posting dates outside fiscal year " +
                            fiscalYear +
                            ". They were imported and excluded from calculation pending auditor review. " +
                            "Date range: " + firstDate.ToString("yyyy-MM-dd") +
                            " to " + lastDate.ToString("yyyy-MM-dd") + ".",
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
