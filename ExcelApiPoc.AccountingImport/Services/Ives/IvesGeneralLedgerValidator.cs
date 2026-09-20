using ExcelApiPoc.AccountingImport.Models.Reporting;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesGeneralLedgerValidator
    {
        private const decimal AmountTolerance = 0.01m;

        public ImportReport Validate(IvesGeneralLedgerParseResult source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var report = new ImportReport
            {
                AccountingFormat = "IVES",
                ImportType = "GeneralLedger",
                SourceFileName = source.SourceFileName
            };

            AddRecordCounts(source, report);
            AddSourceStructureDiagnostics(source, report);

            if (source.Activities.Count == 0)
            {
                AddDiagnostic(report, "IVES.ACTIVITY.NONE",
                    "The general ledger contains no semantic activity.",
                    source, null, "Activities");
                return report;
            }

            foreach (IvesGeneralLedgerActivity activity in source.Activities)
            {
                Dictionary<string, IvesGeneralLedgerSourceRow> accounts =
                    IndexAccounts(source, activity, report);

                ValidateAccountBalances(source, activity, report);
                ValidateDocuments(source, activity, accounts, report);

                Dictionary<string, IvesGeneralLedgerSourceRow> summaries =
                    IndexSyntheticSummaries(source, activity, report);

                ValidateSyntheticSummaries(source, activity, summaries, report);
                ValidateReportTotal(source, activity, report);
            }

            return report;
        }

        private static void AddSourceStructureDiagnostics(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            foreach (IvesGeneralLedgerSourceRow row in source.UnclassifiedRows)
            {
                AddDiagnostic(report, "IVES.ROW.UNCLASSIFIED",
                    "A nonblank source row could not be classified.",
                    source, row, "Unclassified");
            }

            foreach (IvesGeneralLedgerActivity activity in source.Activities)
            {
                foreach (IvesGeneralLedgerSourceRow row in activity.DocumentSummaryRows)
                {
                    report.Diagnostics.Add(new ImportDiagnostic
                    {
                        Code = "IVES.DOCUMENT_SUMMARY.NOT_VALIDATED",
                        Severity = ImportDiagnosticSeverity.Warning,
                        Message = "The document-summary row was retained, but its " +
                            "amounts were not included in financial validation because " +
                            "no original XLS containing this row kind is available yet.",
                        Source = Provenance(source, row, "DocumentSummaries")
                    });
                }
            }
        }

        private static void AddRecordCounts(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            int accounts = 0;
            int documents = 0;
            int documentSummaries = 0;
            int syntheticSummaries = 0;
            int reportTotals = 0;

            foreach (IvesGeneralLedgerActivity activity in source.Activities)
            {
                accounts += activity.AccountRows.Count;
                documents += activity.DocumentRows.Count;
                documentSummaries += activity.DocumentSummaryRows.Count;
                syntheticSummaries += activity.SyntheticSummaryRows.Count;
                reportTotals += activity.ReportTotalRows.Count;
            }

            report.RecordCounts["Activities"] = source.Activities.Count;
            report.RecordCounts["Accounts"] = accounts;
            report.RecordCounts["Documents"] = documents;
            report.RecordCounts["DocumentSummaries"] = documentSummaries;
            report.RecordCounts["SyntheticSummaries"] = syntheticSummaries;
            report.RecordCounts["ReportTotals"] = reportTotals;
            report.RecordCounts["Unclassified"] = source.UnclassifiedRows.Count;
        }

        private static Dictionary<string, IvesGeneralLedgerSourceRow> IndexAccounts(
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerActivity activity,
            ImportReport report)
        {
            var result = new Dictionary<string, IvesGeneralLedgerSourceRow>(
                StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow row in activity.AccountRows)
            {
                if (string.IsNullOrWhiteSpace(row.AccountCode))
                {
                    AddDiagnostic(report, "IVES.ACCOUNT.CODE_MISSING",
                        "An account record has no account code.",
                        source, row, "Accounts");
                    continue;
                }

                if (!result.ContainsKey(row.AccountCode))
                {
                    result.Add(row.AccountCode, row);
                }
                else
                {
                    AddDiagnostic(report, "IVES.ACCOUNT.DUPLICATE",
                        "Duplicate account record '" + row.AccountCode +
                        "' within activity '" + ActivityName(activity) + "'.",
                        source, row, "Accounts");
                }
            }

            return result;
        }

        private static void ValidateAccountBalances(
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerActivity activity,
            ImportReport report)
        {
            foreach (IvesGeneralLedgerSourceRow account in activity.AccountRows)
            {
                decimal expected = Value(account.OpeningBalance) +
                    Value(account.DebitTurnover) - Value(account.CreditTurnover);

                AddAmountValidation(report, "IVES.ACCOUNT.BALANCE",
                    account.AccountCode,
                    "Closing balance equals opening balance plus debit turnover minus credit turnover.",
                    expected, Value(account.ClosingBalance),
                    source, account, "Accounts");
            }
        }

        private static void ValidateDocuments(
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerActivity activity,
            IDictionary<string, IvesGeneralLedgerSourceRow> accounts,
            ImportReport report)
        {
            var totals = new Dictionary<string, AmountSet>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow document in activity.DocumentRows)
            {
                if (string.IsNullOrWhiteSpace(document.AccountCode))
                {
                    AddDiagnostic(report, "IVES.DOCUMENT.ACCOUNT_MISSING",
                        "A document record has no account code.",
                        source, document, "Documents");
                    continue;
                }

                if (!totals.TryGetValue(document.AccountCode, out AmountSet amountSet))
                {
                    amountSet = new AmountSet();
                    totals.Add(document.AccountCode, amountSet);
                }

                amountSet.Debit += Value(document.DebitTurnover);
                amountSet.Credit += Value(document.CreditTurnover);
            }

            foreach (KeyValuePair<string, AmountSet> item in totals)
            {
                if (!accounts.ContainsKey(item.Key))
                {
                    IvesGeneralLedgerSourceRow firstDocument =
                        FindFirst(activity.DocumentRows, item.Key);
                    AddDiagnostic(report, "IVES.DOCUMENT.ACCOUNT_NOT_FOUND",
                        "Documents reference account '" + item.Key +
                        "', but no account record exists in activity '" +
                        ActivityName(activity) + "'.",
                        source, firstDocument, "Documents");
                }
            }

            foreach (IvesGeneralLedgerSourceRow account in activity.AccountRows)
            {
                totals.TryGetValue(
                    account.AccountCode ?? string.Empty,
                    out AmountSet documentTotal);
                documentTotal = documentTotal ?? new AmountSet();

                AddAmountValidation(report, "IVES.DOCUMENTS.DEBIT",
                    account.AccountCode,
                    "Document debit turnover equals account debit turnover.",
                    documentTotal.Debit, Value(account.DebitTurnover),
                    source, account, "Accounts");

                AddAmountValidation(report, "IVES.DOCUMENTS.CREDIT",
                    account.AccountCode,
                    "Document credit turnover equals account credit turnover.",
                    documentTotal.Credit, Value(account.CreditTurnover),
                    source, account, "Accounts");
            }
        }

        private static Dictionary<string, IvesGeneralLedgerSourceRow>
            IndexSyntheticSummaries(
                IvesGeneralLedgerParseResult source,
                IvesGeneralLedgerActivity activity,
                ImportReport report)
        {
            var result = new Dictionary<string, IvesGeneralLedgerSourceRow>(
                StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow row in activity.SyntheticSummaryRows)
            {
                if (string.IsNullOrWhiteSpace(row.AccountCode))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.CODE_MISSING",
                        "A synthetic summary has no synthetic account code.",
                        source, row, "SyntheticSummaries");
                }
                else if (!result.ContainsKey(row.AccountCode))
                {
                    result.Add(row.AccountCode, row);
                }
                else
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.DUPLICATE",
                        "Duplicate synthetic summary '" + row.AccountCode +
                        "' within activity '" + ActivityName(activity) + "'.",
                        source, row, "SyntheticSummaries");
                }
            }

            return result;
        }

        private static void ValidateSyntheticSummaries(
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerActivity activity,
            IDictionary<string, IvesGeneralLedgerSourceRow> summaries,
            ImportReport report)
        {
            var accountTotals = new Dictionary<string, AmountSet>(
                StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow account in activity.AccountRows)
            {
                string syntheticCode = GetSyntheticCode(account.AccountCode);

                if (syntheticCode == null)
                {
                    AddDiagnostic(report, "IVES.ACCOUNT.SYNTHETIC_CODE_INVALID",
                        "Account code '" + account.AccountCode +
                        "' has no recognizable synthetic account.",
                        source, account, "Accounts");
                    continue;
                }

                if (!accountTotals.TryGetValue(
                    syntheticCode, out AmountSet amountSet))
                {
                    amountSet = new AmountSet();
                    accountTotals.Add(syntheticCode, amountSet);
                }

                amountSet.Add(account);
            }

            foreach (KeyValuePair<string, AmountSet> item in accountTotals)
            {
                if (!summaries.TryGetValue(
                    item.Key, out IvesGeneralLedgerSourceRow summary))
                {
                    IvesGeneralLedgerSourceRow firstAccount =
                        FindFirstBySyntheticCode(
                            activity.AccountRows, item.Key);

                    AddDiagnostic(report, "IVES.SYNTHETIC.SUMMARY_NOT_FOUND",
                        "Accounts exist for synthetic account '" + item.Key +
                        "', but its summary is missing in activity '" +
                        ActivityName(activity) + "'.",
                        source, firstAccount, "Accounts");
                    continue;
                }

                AddSyntheticValidations(
                    source, report, item.Key, item.Value, summary);
            }

            foreach (KeyValuePair<string, IvesGeneralLedgerSourceRow> item
                in summaries)
            {
                if (!accountTotals.ContainsKey(item.Key))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.ACCOUNTS_NOT_FOUND",
                        "Synthetic summary '" + item.Key +
                        "' has no account records in activity '" +
                        ActivityName(activity) + "'.",
                        source, item.Value, "SyntheticSummaries");
                }
            }
        }

        private static void AddSyntheticValidations(
            IvesGeneralLedgerParseResult source,
            ImportReport report,
            string code,
            AmountSet accounts,
            IvesGeneralLedgerSourceRow summary)
        {
            AddAmountValidation(report, "IVES.SYNTHETIC.OPENING", code,
                "Account opening balances equal the synthetic summary.",
                accounts.Opening, Value(summary.OpeningBalance),
                source, summary, "SyntheticSummaries");

            AddAmountValidation(report, "IVES.SYNTHETIC.DEBIT", code,
                "Account debit turnovers equal the synthetic summary.",
                accounts.Debit, Value(summary.DebitTurnover),
                source, summary, "SyntheticSummaries");

            AddAmountValidation(report, "IVES.SYNTHETIC.CREDIT", code,
                "Account credit turnovers equal the synthetic summary.",
                accounts.Credit, Value(summary.CreditTurnover),
                source, summary, "SyntheticSummaries");

            AddAmountValidation(report, "IVES.SYNTHETIC.CLOSING", code,
                "Account closing balances equal the synthetic summary.",
                accounts.Closing, Value(summary.ClosingBalance),
                source, summary, "SyntheticSummaries");
        }

        private static void ValidateReportTotal(
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerActivity activity,
            ImportReport report)
        {
            if (activity.ReportTotalRows.Count != 1)
            {
                AddDiagnostic(report, "IVES.REPORT_TOTAL.COUNT",
                    "Expected exactly one report-total record in activity '" +
                    ActivityName(activity) + "', but found " +
                    activity.ReportTotalRows.Count + ".",
                    source,
                    activity.ReportTotalRows.Count == 0
                        ? null
                        : activity.ReportTotalRows[0],
                    "ReportTotals");
                return;
            }

            var summarySum = new AmountSet();
            foreach (IvesGeneralLedgerSourceRow summary
                in activity.SyntheticSummaryRows)
            {
                summarySum.Add(summary);
            }

            IvesGeneralLedgerSourceRow total = activity.ReportTotalRows[0];
            string scope = string.IsNullOrWhiteSpace(activity.Name)
                ? "Report"
                : activity.Name;

            AddAmountValidation(report, "IVES.REPORT_TOTAL.OPENING", scope,
                "Synthetic opening summaries equal the activity report total.",
                summarySum.Opening, Value(total.OpeningBalance),
                source, total, "ReportTotals");

            AddAmountValidation(report, "IVES.REPORT_TOTAL.DEBIT", scope,
                "Synthetic debit summaries equal the activity report total.",
                summarySum.Debit, Value(total.DebitTurnover),
                source, total, "ReportTotals");

            AddAmountValidation(report, "IVES.REPORT_TOTAL.CREDIT", scope,
                "Synthetic credit summaries equal the activity report total.",
                summarySum.Credit, Value(total.CreditTurnover),
                source, total, "ReportTotals");

            AddAmountValidation(report, "IVES.REPORT_TOTAL.CLOSING", scope,
                "Synthetic closing summaries equal the activity report total.",
                summarySum.Closing, Value(total.ClosingBalance),
                source, total, "ReportTotals");
        }

        private static void AddAmountValidation(
            ImportReport report,
            string code,
            string scope,
            string description,
            decimal expected,
            decimal actual,
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerSourceRow row,
            string recordSet)
        {
            decimal difference = actual - expected;
            bool valid = Math.Abs(difference) <= AmountTolerance;
            SourceProvenance provenance = Provenance(
                source, row, recordSet);

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
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerSourceRow row,
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
            IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerSourceRow row,
            string recordSet)
        {
            return new SourceProvenance
            {
                SourceFileName = source.SourceFileName,
                WorksheetName = source.WorksheetName,
                RecordSet = recordSet,
                SequenceNumber = row == null
                    ? (int?)null
                    : row.SequenceNumber,
                SourceRowNumber = row == null
                    ? (int?)null
                    : row.SourceRowNumber
            };
        }

        private static string ActivityName(IvesGeneralLedgerActivity activity)
        {
            return string.IsNullOrWhiteSpace(activity.Name)
                ? "(default)"
                : activity.Name;
        }

        private static string GetSyntheticCode(string accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode))
                return null;

            int dot = accountCode.IndexOf('.');
            string code = (dot < 0
                ? accountCode
                : accountCode.Substring(0, dot)).Trim();

            return code.Length == 3 ? code : null;
        }

        private static IvesGeneralLedgerSourceRow FindFirst(
            IEnumerable<IvesGeneralLedgerSourceRow> rows,
            string accountCode)
        {
            foreach (IvesGeneralLedgerSourceRow row in rows)
            {
                if (string.Equals(
                    row.AccountCode, accountCode, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private static IvesGeneralLedgerSourceRow FindFirstBySyntheticCode(
            IEnumerable<IvesGeneralLedgerSourceRow> rows,
            string syntheticCode)
        {
            foreach (IvesGeneralLedgerSourceRow row in rows)
            {
                if (string.Equals(
                    GetSyntheticCode(row.AccountCode),
                    syntheticCode,
                    StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private static decimal Value(decimal? value)
        {
            return value.GetValueOrDefault();
        }

        private sealed class AmountSet
        {
            public decimal Opening { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Closing { get; set; }

            public void Add(IvesGeneralLedgerSourceRow row)
            {
                Opening += Value(row.OpeningBalance);
                Debit += Value(row.DebitTurnover);
                Credit += Value(row.CreditTurnover);
                Closing += Value(row.ClosingBalance);
            }
        }
    }
}
