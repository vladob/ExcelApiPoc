using ExcelApiPoc.AccountingImport.Models.Reporting;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesJournalValidator
    {
        private const decimal AmountTolerance = 0.01m;

        public ImportReport Validate(IvesJournalParseResult source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var report = new ImportReport
            {
                AccountingFormat = "IVES",
                ImportType = "Journal",
                SourceFileName = source.SourceFileName
            };

            AddRecordCounts(source, report);
            ValidateSourceStructure(source, report);
            ValidateTransactions(source, report);

            JournalTotals totals = CalculateTotals(source.TransactionRows);
            AddAmountValidation(
                report,
                "IVES.JOURNAL.BALANCE",
                "Journal",
                "Debit turnover equals credit turnover.",
                totals.Debit,
                totals.Credit,
                source,
                null,
                "Transactions");

            ValidateReportTotal(source, totals, report);
            return report;
        }

        private static void AddRecordCounts(
            IvesJournalParseResult source,
            ImportReport report)
        {
            report.RecordCounts["Transactions"] = source.TransactionRows.Count;
            report.RecordCounts["Modules"] = source.ModuleRows.Count;
            report.RecordCounts["ReportTotals"] = source.ReportTotalRows.Count;
            report.RecordCounts["Unclassified"] = source.UnclassifiedRows.Count;
        }

        private static void ValidateSourceStructure(
            IvesJournalParseResult source,
            ImportReport report)
        {
            if (source.TransactionRows.Count == 0)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.TRANSACTIONS.MISSING",
                    "The journal contains no transaction records.",
                    source,
                    null,
                    "Transactions");
            }

            foreach (IvesJournalSourceRow row in source.UnclassifiedRows)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.ROW.UNCLASSIFIED",
                    "A nonblank source row could not be classified.",
                    source,
                    row,
                    "Unclassified");
            }
        }

        private static void ValidateTransactions(
            IvesJournalParseResult source,
            ImportReport report)
        {
            Dictionary<int, IvesJournalSourceRow> modulesBySourceRow = null;

            if (source.ModuleRows.Count > 0)
            {
                modulesBySourceRow =
                    new Dictionary<int, IvesJournalSourceRow>();

                for (int index = 0; index < source.ModuleRows.Count; index++)
                {
                    IvesJournalSourceRow module = source.ModuleRows[index];

                    if (module.SequenceNumber != index + 1)
                    {
                        AddDiagnostic(
                            report,
                            "IVES.JOURNAL.MODULE.SEQUENCE",
                            "Module sequence numbers must be contiguous and one-based.",
                            source,
                            module,
                            "Modules");
                    }

                    if (string.IsNullOrWhiteSpace(module.Module))
                    {
                        AddDiagnostic(
                            report,
                            "IVES.JOURNAL.MODULE.VALUE_MISSING",
                            "A module record has no module value.",
                            source,
                            module,
                            "Modules");
                    }

                    if (!modulesBySourceRow.ContainsKey(module.SourceRowNumber))
                    {
                        modulesBySourceRow.Add(module.SourceRowNumber, module);
                    }
                    else
                    {
                        AddDiagnostic(
                            report,
                            "IVES.JOURNAL.MODULE.DUPLICATE_SOURCE_ROW",
                            "More than one module record references source row " +
                            module.SourceRowNumber + ".",
                            source,
                            module,
                            "Modules");
                    }
                }

                if (source.TransactionRows.Count != source.ModuleRows.Count)
                {
                    AddDiagnostic(
                        report,
                        "IVES.JOURNAL.MODULE.COUNT",
                        "Every transaction must have exactly one module evidence record " +
                        "when the source parser exposes separate module records. " +
                        "Transactions: " + source.TransactionRows.Count +
                        ", modules: " + source.ModuleRows.Count + ".",
                        source,
                        null,
                        "Modules");
                }
            }

            for (int index = 0; index < source.TransactionRows.Count; index++)
            {
                IvesJournalSourceRow transaction = source.TransactionRows[index];

                if (transaction.SequenceNumber != index + 1)
                {
                    AddDiagnostic(
                        report,
                        "IVES.JOURNAL.TRANSACTION.SEQUENCE",
                        "Transaction sequence numbers must be contiguous and one-based.",
                        source,
                        transaction,
                        "Transactions");
                }

                ValidateTransactionFields(source, transaction, report);

                if (modulesBySourceRow != null)
                {
                    ValidateModulePair(
                        source,
                        transaction,
                        modulesBySourceRow,
                        report);
                }
            }
        }

        private static void ValidateTransactionFields(
            IvesJournalParseResult source,
            IvesJournalSourceRow transaction,
            ImportReport report)
        {
            if (!transaction.PostingDate.HasValue)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.DATE.MISSING",
                    "A transaction has no posting date.",
                    source,
                    transaction,
                    "Transactions");
            }
            else if (!source.PeriodStart.HasValue || !source.PeriodEnd.HasValue ||
                     transaction.PostingDate.Value < source.PeriodStart.Value ||
                     transaction.PostingDate.Value > source.PeriodEnd.Value)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.DATE.OUTSIDE_PERIOD",
                    "Transaction date " +
                    transaction.PostingDate.Value.ToString("yyyy-MM-dd") +
                    " is outside the declared journal period.",
                    source,
                    transaction,
                    "Transactions");
            }

            if (string.IsNullOrWhiteSpace(transaction.DocumentNumber))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.DOCUMENT_NUMBER.MISSING",
                    "A transaction has no document number.",
                    source,
                    transaction,
                    "Transactions");
            }

            if (string.IsNullOrWhiteSpace(transaction.DebitCompositeAccount) &&
                string.IsNullOrWhiteSpace(transaction.CreditCompositeAccount))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.ACCOUNT.MISSING",
                    "A transaction has neither a debit nor a credit account.",
                    source,
                    transaction,
                    "Transactions");
            }

            if (!transaction.Amount.HasValue)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.AMOUNT.MISSING",
                    "A transaction has no amount.",
                    source,
                    transaction,
                    "Transactions");
            }

            if (string.IsNullOrWhiteSpace(transaction.Module))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.MODULE.VALUE_MISSING",
                    "A transaction has no module value.",
                    source,
                    transaction,
                    "Transactions");
            }

            if (!string.Equals(transaction.Currency, "€", StringComparison.Ordinal))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.CURRENCY.UNSUPPORTED",
                    "Expected currency '€', but found '" +
                    (transaction.Currency ?? string.Empty) + "'.",
                    source,
                    transaction,
                    "Transactions");
            }
        }

        private static void ValidateModulePair(
            IvesJournalParseResult source,
            IvesJournalSourceRow transaction,
            IDictionary<int, IvesJournalSourceRow> modulesBySourceRow,
            ImportReport report)
        {
            int expectedModuleSourceRow = transaction.SourceRowNumber + 1;
            if (transaction.RelatedSourceRowNumber != expectedModuleSourceRow ||
                !modulesBySourceRow.TryGetValue(
                    expectedModuleSourceRow,
                    out IvesJournalSourceRow module))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.MODULE.PAIR_MISSING",
                    "The transaction is not paired with a module on the immediately " +
                    "following source row.",
                    source,
                    transaction,
                    "Transactions");
                return;
            }

            if (module.RelatedSourceRowNumber != transaction.SourceRowNumber ||
                !string.Equals(module.Module, transaction.Module, StringComparison.Ordinal))
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.MODULE.PAIR_INCONSISTENT",
                    "The transaction and module records contain inconsistent pairing data.",
                    source,
                    transaction,
                    "Transactions");
            }
        }

        private static JournalTotals CalculateTotals(
            IEnumerable<IvesJournalSourceRow> transactions)
        {
            var totals = new JournalTotals();

            foreach (IvesJournalSourceRow transaction in transactions)
            {
                decimal amount = transaction.Amount.GetValueOrDefault();
                totals.Displayed += amount;

                if (!string.IsNullOrWhiteSpace(transaction.DebitCompositeAccount))
                {
                    totals.Debit += amount;
                }

                if (!string.IsNullOrWhiteSpace(transaction.CreditCompositeAccount))
                {
                    totals.Credit += amount;
                }
            }

            return totals;
        }

        private static void ValidateReportTotal(
            IvesJournalParseResult source,
            JournalTotals totals,
            ImportReport report)
        {
            if (source.ReportTotalRows.Count != 1)
            {
                AddDiagnostic(
                    report,
                    "IVES.JOURNAL.REPORT_TOTAL.COUNT",
                    "Expected exactly one report-total record, but found " +
                    source.ReportTotalRows.Count + ".",
                    source,
                    source.ReportTotalRows.Count == 0 ? null : source.ReportTotalRows[0],
                    "ReportTotals");
                return;
            }

            IvesJournalSourceRow footer = source.ReportTotalRows[0];
            if (footer.ReportedAmounts.Count == 1)
            {
                AddAmountValidation(
                    report,
                    "IVES.JOURNAL.REPORT_TOTAL.DISPLAYED",
                    "Report",
                    "The sum of displayed transaction amounts equals the report total.",
                    totals.Displayed,
                    footer.ReportedAmounts[0],
                    source,
                    footer,
                    "ReportTotals");
                return;
            }

            if (footer.ReportedAmounts.Count == 2)
            {
                AddAmountValidation(
                    report,
                    "IVES.JOURNAL.REPORT_TOTAL.DEBIT",
                    "Report",
                    "Calculated debit turnover equals the reported debit total.",
                    totals.Debit,
                    footer.ReportedAmounts[0],
                    source,
                    footer,
                    "ReportTotals");
                AddAmountValidation(
                    report,
                    "IVES.JOURNAL.REPORT_TOTAL.CREDIT",
                    "Report",
                    "Calculated credit turnover equals the reported credit total.",
                    totals.Credit,
                    footer.ReportedAmounts[1],
                    source,
                    footer,
                    "ReportTotals");
                return;
            }

            AddDiagnostic(
                report,
                "IVES.JOURNAL.REPORT_TOTAL.SHAPE",
                "The report-total row must contain either one displayed-amount total " +
                "or separate debit and credit totals. Found " +
                footer.ReportedAmounts.Count + " numeric values.",
                source,
                footer,
                "ReportTotals");
        }

        private static void AddAmountValidation(
            ImportReport report,
            string code,
            string scope,
            string description,
            decimal expected,
            decimal actual,
            IvesJournalParseResult source,
            IvesJournalSourceRow row,
            string recordSet)
        {
            decimal difference = actual - expected;
            bool valid = Math.Abs(difference) <= AmountTolerance;
            SourceProvenance provenance = Provenance(source, row, recordSet);

            report.ValidationResults.Add(new ImportValidationResult
            {
                Code = code,
                Scope = scope,
                Description = description,
                IsValid = valid,
                ExpectedAmount = expected,
                ActualAmount = actual,
                Difference = difference,
                Tolerance = AmountTolerance,
                Source = provenance
            });

            if (!valid)
            {
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = code,
                    Severity = ImportDiagnosticSeverity.Error,
                    Message = description + " Scope: '" + scope +
                        "'. Expected " + expected + ", actual " + actual +
                        ", difference " + difference + ".",
                    Source = provenance
                });
            }
        }

        private static void AddDiagnostic(
            ImportReport report,
            string code,
            string message,
            IvesJournalParseResult source,
            IvesJournalSourceRow row,
            string recordSet)
        {
            report.Diagnostics.Add(new ImportDiagnostic
            {
                Code = code,
                Severity = ImportDiagnosticSeverity.Error,
                Message = message,
                Source = Provenance(source, row, recordSet)
            });
        }

        private static SourceProvenance Provenance(
            IvesJournalParseResult source,
            IvesJournalSourceRow row,
            string recordSet)
        {
            return new SourceProvenance
            {
                SourceFileName = source.SourceFileName,
                WorksheetName = source.WorksheetName,
                RecordSet = recordSet,
                SequenceNumber = row == null ? (int?)null : row.SequenceNumber,
                SourceRowNumber = row == null ? (int?)null : row.SourceRowNumber
            };
        }

        private sealed class JournalTotals
        {
            public decimal Displayed { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }
    }
}
