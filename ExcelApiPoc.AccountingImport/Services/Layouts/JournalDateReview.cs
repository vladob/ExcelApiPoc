using System;
using ExcelApiPoc.AccountingImport.Models;
namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    // Shared initial selection policy. SourceFields always retain original date text.
    public static class JournalDateReview
    {
        public static void Apply(JournalRow row, int fiscalYear, bool dateParsed, DateTime? accountingDate = null)
        {
            if(!dateParsed || row.PostingDate.Year < 1900 || fiscalYear == 0 ||
               row.PostingDate.Year != fiscalYear || (accountingDate.HasValue && accountingDate.Value.Year != fiscalYear))
                row.DateExceptionResolution = JournalDateExceptionResolution.Excluded;
        }
    }
}
