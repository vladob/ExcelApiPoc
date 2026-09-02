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
                    GeneralLedgerAccountName = AuditWorkbookTableReader.GetString(row, "GeneralLedgerAccountName"),
                    AccountNameComparisonStatus = AuditWorkbookTableReader.GetString(row, "AccountNameComparisonStatus"),
                    DebitEntryCount = AuditWorkbookTableReader.GetInt32(row, "DebitEntryCount"),
                    DebitTurnover = AuditWorkbookTableReader.GetDecimal(row, "DebitTurnover"),
                    CreditEntryCount = AuditWorkbookTableReader.GetInt32(row, "CreditEntryCount"),
                    CreditTurnover = AuditWorkbookTableReader.GetDecimal(row, "CreditTurnover"),
                    JournalLedgerOpeningDebit = AuditWorkbookTableReader.GetDecimal(row, "JournalLedgerOpeningDebit"),
                    JournalLedgerOpeningCredit = AuditWorkbookTableReader.GetDecimal(row, "JournalLedgerOpeningCredit"),
                    JournalLedgerDebitTurnover = AuditWorkbookTableReader.GetDecimal(row, "JournalLedgerDebitTurnover"),
                    JournalLedgerCreditTurnover = AuditWorkbookTableReader.GetDecimal(row, "JournalLedgerCreditTurnover"),
                    LedgerOpeningDebit = AuditWorkbookTableReader.GetDecimal(row, "LedgerOpeningDebit"),
                    LedgerOpeningCredit = AuditWorkbookTableReader.GetDecimal(row, "LedgerOpeningCredit"),
                    LedgerDebitTurnover = AuditWorkbookTableReader.GetDecimal(row, "LedgerDebitTurnover"),
                    LedgerCreditTurnover = AuditWorkbookTableReader.GetDecimal(row, "LedgerCreditTurnover"),
                    LedgerClosingDebit = AuditWorkbookTableReader.GetDecimal(row, "LedgerClosingDebit"),
                    LedgerClosingCredit = AuditWorkbookTableReader.GetDecimal(row, "LedgerClosingCredit"),
                    LedgerReconciliationStatus = AuditWorkbookTableReader.GetString(row, "LedgerReconciliationStatus")
                });
            }
            return result;
        }
    }
}
