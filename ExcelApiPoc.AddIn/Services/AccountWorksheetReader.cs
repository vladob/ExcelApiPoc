using ExcelApiPoc.AddIn.Models;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountWorksheetReader
    {
        public static IReadOnlyList<AccountSummary> Read(Excel.Workbook workbook)
        {
            IReadOnlyList<IDictionary<string, object>> rows = AuditWorkbookTableReader.ReadRows(workbook, "AccountRows");
            var result = new List<AccountSummary>();

            foreach (IDictionary<string, object> row in rows)
            {
                result.Add(new AccountSummary
                {
                    AccountCode = AuditWorkbookTableReader.GetString(row, "AccountCode"),
                    AccountName = AuditWorkbookTableReader.GetString(row, "AccountName"),
                    AccountNameSource = AuditWorkbookTableReader.GetString(row, "AccountNameSource"),
                    EntityAccountName = AuditWorkbookTableReader.GetString(row, "EntityAccountName"),
                    SyntheticAccountCode = AuditWorkbookTableReader.GetString(row, "SyntheticAccountCode"),
                    FrameworkAccountCode = AuditWorkbookTableReader.GetString(row, "FrameworkAccountCode"),
                    FrameworkAccountName = AuditWorkbookTableReader.GetString(row, "FrameworkAccountName"),
                    DebitEntryCount = AuditWorkbookTableReader.GetInt32(row, "DebitEntryCount"),
                    DebitTurnover = AuditWorkbookTableReader.GetDecimal(row, "DebitTurnover"),
                    CreditEntryCount = AuditWorkbookTableReader.GetInt32(row, "CreditEntryCount"),
                    CreditTurnover = AuditWorkbookTableReader.GetDecimal(row, "CreditTurnover")
                });
            }
            return result;
        }
    }
}
