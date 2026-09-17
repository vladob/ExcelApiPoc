using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookRecalculationService
    {
        public static AuditWorkbookRecalculationResult Recalculate(Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            AuditTemplatePackageResponse package = AuditCalculationPackageWorksheetReader.Read(workbook);
            IReadOnlyList<AccountSummary> accounts = AccountWorksheetReader.Read(workbook);
            AnalyticalMappingSelectionReadResult mappingSelections = AnalyticalMappingWorksheetReader.Read(workbook);
            AuditReportCalculationResult calculation = AuditReportCalculationService.Calculate(accounts, package, mappingSelections.Selections);

            Excel.Worksheet calculationWorksheet =
                AuditReportCalculationWorksheetWriter.Write(workbook, package, calculation);
            AuditValidationResultsWorksheetWriter.Write(
                workbook,
                accounts,
                mappingSelections,
                package,
                calculation);

            calculationWorksheet.Activate();

            return new AuditWorkbookRecalculationResult
            {
                Calculation = calculation,
                MappingSelections = mappingSelections,
                AccountCount = accounts.Count,
                MappingRuleCount = package.ReportMappingRules?.Length ?? 0,
                CalculationDependencyCount = package.CalculationPlan?.Length ?? 0
            };
        }

        public static AuditWorkbookRecalculationResult RecalculateGeneralLedgerFromJournal(
            Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            WorkbookImportIdentity identity = WorkbookImportIdentity.Read(workbook);
            JournalImport journal = JournalWorksheetReader.ReadAndValidate(
                workbook,
                identity.Ico,
                identity.FiscalYear,
                identity.AccountingFormat,
                identity.SourceFileName);

            IReadOnlyList<AccountSummary> previousAccounts =
                AccountWorksheetReader.Read(workbook);

            CalculatedGeneralLedger calculated =
                CalculatedGeneralLedgerBuilder.Build(journal);
            List<AccountSummary> accounts =
                JournalAccountSummaryBuilder.Build(journal);

            GeneralLedgerImport ledger =
                GeneralLedgerWorksheetReader.ReadIfPresent(
                    workbook,
                    identity.Ico,
                    identity.FiscalYear,
                    identity.AccountingFormat);

            JournalLedgerReconciliationResult reconciliation = null;
            if (ledger != null)
            {
                reconciliation = JournalLedgerReconciliationService.Reconcile(journal, ledger);
                CanonicalReconciliationAccountSummaryAdapter.Apply(reconciliation, accounts);
            }

            PreserveAccountMetadata(accounts, previousAccounts);
            JournalWorksheetWriter.SynchronizeDerivedColumns(workbook, journal);

            bool previousDisplayAlerts = workbook.Application.DisplayAlerts;
            workbook.Application.DisplayAlerts = false;
            try
            {
                DeleteWorksheetContainingTable(workbook, "AccountRows");
                DeleteWorksheetContainingTable(workbook, "GeneralLedgerComparisonRows");
            }
            finally
            {
                workbook.Application.DisplayAlerts = previousDisplayAlerts;
            }

            AccountWorksheetWriter.AddWorksheet(workbook, accounts);
            CalculatedGeneralLedgerComparisonWorksheetWriter.AddWorksheet(
                workbook,
                calculated,
                reconciliation,
                accounts);

            AuditWorkbookWorksheetLayout.Apply(workbook);
            AuditWorkbookRecalculationResult result = Recalculate(workbook);
            AuditWorkbookWorksheetLayout.Apply(workbook);
            return result;
        }

        private static void PreserveAccountMetadata(
            IList<AccountSummary> accounts,
            IReadOnlyList<AccountSummary> previousAccounts)
        {
            Dictionary<string, AccountSummary> previousByCode =
                previousAccounts
                    .Where(account => !string.IsNullOrWhiteSpace(account.AccountCode))
                    .GroupBy(account => account.AccountCode, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            foreach (AccountSummary account in accounts)
            {
                if (!previousByCode.TryGetValue(account.AccountCode, out AccountSummary previous))
                    continue;

                account.AccountName = previous.AccountName;
                account.AccountNameSource = previous.AccountNameSource;
                account.EntityAccountName = previous.EntityAccountName;
                account.SyntheticAccountCode = previous.SyntheticAccountCode;
                account.FrameworkAccountCode = previous.FrameworkAccountCode;
                account.FrameworkAccountName = previous.FrameworkAccountName;
                account.IsFrameworkMatch = previous.IsFrameworkMatch;
                account.AccountNameComparisonStatus = previous.AccountNameComparisonStatus;

                if (string.IsNullOrWhiteSpace(account.GeneralLedgerAccountName))
                    account.GeneralLedgerAccountName = previous.GeneralLedgerAccountName;
            }
        }

        private static void DeleteWorksheetContainingTable(
            Excel.Workbook workbook,
            string tableName)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
                    {
                        worksheet.Delete();
                        return;
                    }
                }
            }

            throw new InvalidOperationException(
                "The audit workbook does not contain table '" + tableName + "'.");
        }
    }

    internal static class JournalWorksheetReader
    {
        public static JournalImport ReadAndValidate(
            Excel.Workbook workbook,
            string ico,
            int fiscalYear,
            string accountingFormat,
            string sourceFileName)
        {
            IReadOnlyList<IDictionary<string, object>> rows =
                AuditWorkbookTableReader.ReadRows(workbook, "JournalRows");

            var journal = new JournalImport
            {
                Ico = ico,
                FiscalYear = fiscalYear,
                AccountingFormat = accountingFormat,
                SourceFileName = sourceFileName
            };

            foreach (IDictionary<string, object> source in rows)
            {
                var row = new JournalRow
                {
                    SequenceNumber = AuditWorkbookTableReader.GetInt32(source, "SequenceNumber"),
                    PostingDate = AuditWorkbookTableReader.GetDateTime(source, "PostingDate"),
                    DocumentType = AuditWorkbookTableReader.GetString(source, "DocumentType"),
                    DocumentNumber = AuditWorkbookTableReader.GetString(source, "DocumentNumber"),
                    Description = AuditWorkbookTableReader.GetString(source, "Description"),
                    DebitAccount = AuditWorkbookTableReader.GetString(source, "DebitAccount"),
                    DebitAmount = GetNullableDecimal(source, "DebitAmount"),
                    DebitSection = AuditWorkbookTableReader.GetString(source, "DebitSection"),
                    DebitItem = AuditWorkbookTableReader.GetString(source, "DebitItem"),
                    DebitFundingSource = AuditWorkbookTableReader.GetString(source, "DebitFundingSource"),
                    DebitCostCenter = AuditWorkbookTableReader.GetString(source, "DebitCostCenter"),
                    DebitOrder = AuditWorkbookTableReader.GetString(source, "DebitOrder"),
                    CreditAccount = AuditWorkbookTableReader.GetString(source, "CreditAccount"),
                    CreditAmount = GetNullableDecimal(source, "CreditAmount"),
                    CreditSection = AuditWorkbookTableReader.GetString(source, "CreditSection"),
                    CreditItem = AuditWorkbookTableReader.GetString(source, "CreditItem"),
                    CreditFundingSource = AuditWorkbookTableReader.GetString(source, "CreditFundingSource"),
                    CreditCostCenter = AuditWorkbookTableReader.GetString(source, "CreditCostCenter"),
                    CreditOrder = AuditWorkbookTableReader.GetString(source, "CreditOrder"),
                    SourceRecordNumber = AuditWorkbookTableReader.GetInt32(source, "SourceRecordNumber"),
                    SourceStartLineNumber = AuditWorkbookTableReader.GetNullableInt32(source, "SourceStartLineNumber"),
                    SourceEndLineNumber = AuditWorkbookTableReader.GetNullableInt32(source, "SourceEndLineNumber"),
                    SourceLocation = AuditWorkbookTableReader.GetString(source, "SourceLocation"),
                    TextNormalizationApplied = AuditWorkbookTableReader.GetBoolean(source, "TextNormalizationApplied")
                };

                if (!Enum.TryParse(
                        AuditWorkbookTableReader.GetString(source, "RecordKind"),
                        true,
                        out JournalRecordKind recordKind))
                {
                    throw new InvalidOperationException(
                        "Journal row " + row.SequenceNumber + " contains an invalid RecordKind.");
                }
                row.RecordKind = recordKind;

                string resolutionText = source.ContainsKey("DateExceptionResolution")
                    ? AuditWorkbookTableReader.GetString(source, "DateExceptionResolution")
                    : string.Empty;
                DateTime? correctedDate = source.ContainsKey("CorrectedPostingDate")
                    ? AuditWorkbookTableReader.GetNullableDateTime(source, "CorrectedPostingDate")
                    : null;

                ValidateDateException(
                    row,
                    resolutionText,
                    correctedDate,
                    fiscalYear);
                journal.Rows.Add(row);
            }

            return journal;
        }

        private static void ValidateDateException(
            JournalRow row,
            string resolutionText,
            DateTime? correctedDate,
            int fiscalYear)
        {
            bool outsideFiscalYear = row.PostingDate.Year != fiscalYear;
            bool hasResolution = !string.IsNullOrWhiteSpace(resolutionText);

            if (!outsideFiscalYear)
            {
                if (hasResolution || correctedDate.HasValue)
                {
                    throw new InvalidOperationException(
                        "Journal row " + row.SequenceNumber +
                        " is not a date exception and must not contain a date-exception resolution or corrected date.");
                }
                return;
            }

            if (!hasResolution)
            {
                throw new InvalidOperationException(
                    "Journal row " + row.SequenceNumber +
                    " has posting date " + row.PostingDate.ToString("yyyy-MM-dd") +
                    " outside fiscal year " + fiscalYear +
                    " and requires an explicit resolution.");
            }

            if (!JournalWorksheetWriter.TryParseResolutionCaption(
                    resolutionText,
                    out JournalDateExceptionResolution resolution))
            {
                throw new InvalidOperationException(
                    "Journal row " + row.SequenceNumber +
                    " contains an invalid date-exception resolution '" + resolutionText + "'.");
            }

            row.DateExceptionResolution = resolution;
            row.CorrectedPostingDate = correctedDate;

            switch (resolution)
            {
                case JournalDateExceptionResolution.Excluded:
                case JournalDateExceptionResolution.OriginalIncluded:
                    if (correctedDate.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Journal row " + row.SequenceNumber +
                            " must leave CorrectedPostingDate empty when the resolution is '" +
                            JournalWorksheetWriter.ToResolutionCaption(resolution) + "'.");
                    }
                    break;

                case JournalDateExceptionResolution.ModifiedIncluded:
                    if (!correctedDate.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Journal row " + row.SequenceNumber +
                            " requires CorrectedPostingDate when the resolution is '" +
                            JournalWorksheetWriter.ModifiedIncludedCaption + "'.");
                    }
                    if (correctedDate.Value.Year != fiscalYear)
                    {
                        throw new InvalidOperationException(
                            "Journal row " + row.SequenceNumber +
                            " has corrected date " + correctedDate.Value.ToString("yyyy-MM-dd") +
                            ", which is outside fiscal year " + fiscalYear + ".");
                    }
                    break;
            }
        }

        private static decimal? GetNullableDecimal(
            IDictionary<string, object> row,
            string columnName)
        {
            if (!row.TryGetValue(columnName, out object value) ||
                value == null ||
                string.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture)))
            {
                return null;
            }
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
    }

    internal static class GeneralLedgerWorksheetReader
    {
        public static GeneralLedgerImport ReadIfPresent(
            Excel.Workbook workbook,
            string ico,
            int fiscalYear,
            string accountingFormat)
        {
            if (!AuditWorkbookTableReader.ContainsTable(workbook, "GeneralLedgerRows"))
                return null;

            var result = new GeneralLedgerImport
            {
                Ico = ico,
                FiscalYear = fiscalYear,
                AccountingFormat = accountingFormat
            };

            foreach (IDictionary<string, object> source in
                AuditWorkbookTableReader.ReadRows(workbook, "GeneralLedgerRows"))
            {
                result.Rows.Add(new GeneralLedgerRow
                {
                    SequenceNumber = AuditWorkbookTableReader.GetInt32(source, "SequenceNumber"),
                    SourceRecordNumber = AuditWorkbookTableReader.GetInt32(source, "SourceRecordNumber"),
                    SyntheticCode = AuditWorkbookTableReader.GetString(source, "SyntheticCode"),
                    AnalyticalCode = AuditWorkbookTableReader.GetString(source, "AnalyticalCode"),
                    AccountCode = AuditWorkbookTableReader.GetString(source, "AccountCode"),
                    Type = AuditWorkbookTableReader.GetString(source, "Type"),
                    P = AuditWorkbookTableReader.GetString(source, "P"),
                    Section = AuditWorkbookTableReader.GetString(source, "Section"),
                    Item = AuditWorkbookTableReader.GetString(source, "Item"),
                    FundingSource = AuditWorkbookTableReader.GetString(source, "FundingSource"),
                    Program = AuditWorkbookTableReader.GetString(source, "Program"),
                    CostCenter = AuditWorkbookTableReader.GetString(source, "CostCenter"),
                    Order = AuditWorkbookTableReader.GetString(source, "Order"),
                    AccountName = AuditWorkbookTableReader.GetString(source, "AccountName"),
                    OpeningDebit = AuditWorkbookTableReader.GetDecimal(source, "OpeningDebit"),
                    OpeningCredit = AuditWorkbookTableReader.GetDecimal(source, "OpeningCredit"),
                    AnnualDebitTurnover = AuditWorkbookTableReader.GetDecimal(source, "AnnualDebitTurnover"),
                    AnnualCreditTurnover = AuditWorkbookTableReader.GetDecimal(source, "AnnualCreditTurnover"),
                    PeriodDebitTurnover = AuditWorkbookTableReader.GetDecimal(source, "PeriodDebitTurnover"),
                    PeriodCreditTurnover = AuditWorkbookTableReader.GetDecimal(source, "PeriodCreditTurnover"),
                    ClosingDebit = AuditWorkbookTableReader.GetDecimal(source, "ClosingDebit"),
                    ClosingCredit = AuditWorkbookTableReader.GetDecimal(source, "ClosingCredit"),
                    Plan = AuditWorkbookTableReader.GetDecimal(source, "Plan")
                });
            }

            return result;
        }
    }

    internal sealed class WorkbookImportIdentity
    {
        public string Ico { get; private set; }
        public int FiscalYear { get; private set; }
        public string AccountingFormat { get; private set; }
        public string SourceFileName { get; private set; }

        public static WorkbookImportIdentity Read(Excel.Workbook workbook)
        {
            IDictionary<string, object> row =
                AuditWorkbookTableReader.ReadRows(workbook, "__ImportSources").Single();
            return new WorkbookImportIdentity
            {
                Ico = AuditWorkbookTableReader.GetString(row, "Ico"),
                FiscalYear = AuditWorkbookTableReader.GetInt32(row, "SelectedFiscalYear"),
                AccountingFormat = AuditWorkbookTableReader.GetString(row, "AccountingFormat"),
                SourceFileName = AuditWorkbookTableReader.GetString(row, "SourceFileName")
            };
        }
    }
}
