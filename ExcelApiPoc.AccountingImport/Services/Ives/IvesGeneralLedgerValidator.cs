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
            {
                throw new ArgumentNullException(nameof(source));
            }

            var report = new ImportReport
            {
                AccountingFormat = "IVES",
                ImportType = "GeneralLedger",
                SourceFileName = source.SourceFileName
            };

            AddRecordCounts(source, report);
            AddSourceStructureDiagnostics(source, report);
            Dictionary<string, IvesGeneralLedgerSourceRow> accounts =
                IndexAccounts(source, report);

            ValidateAccountBalances(source, report);
            ValidateDocuments(source, accounts, report);

            Dictionary<string, IvesGeneralLedgerSourceRow> subtotals =
                IndexSyntheticSubtotals(source, report);

            ValidateSyntheticHeadings(source, subtotals, report);
            ValidateSyntheticSubtotals(source, subtotals, report);
            ValidateReportTotal(source, report);

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

            foreach (IvesGeneralLedgerSourceRow row in source.DocumentSummaryRows)
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

        private static void AddRecordCounts(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            report.RecordCounts["Accounts"] = source.AccountRows.Count;
            report.RecordCounts["Documents"] = source.DocumentRows.Count;
            report.RecordCounts["DocumentSummaries"] = source.DocumentSummaryRows.Count;
            report.RecordCounts["SyntheticAccounts"] = source.SyntheticAccountRows.Count;
            report.RecordCounts["SyntheticSubtotals"] = source.SyntheticSubtotalRows.Count;
            report.RecordCounts["ReportTotals"] = source.ReportTotalRows.Count;
            report.RecordCounts["Unclassified"] = source.UnclassifiedRows.Count;
        }

        private static Dictionary<string, IvesGeneralLedgerSourceRow> IndexAccounts(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            var result = new Dictionary<string, IvesGeneralLedgerSourceRow>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow row in source.AccountRows)
            {
                if (string.IsNullOrWhiteSpace(row.AccountCode))
                {
                    AddDiagnostic(report, "IVES.ACCOUNT.CODE_MISSING",
                        "An account record has no account code.", source, row, "Accounts");
                    continue;
                }

                if (!result.ContainsKey(row.AccountCode))
                {
                    result.Add(row.AccountCode, row);
                }
                else
                {
                    AddDiagnostic(report, "IVES.ACCOUNT.DUPLICATE",
                        "Duplicate account record '" + row.AccountCode + "'.",
                        source, row, "Accounts");
                }
            }

            return result;
        }

        private static void ValidateAccountBalances(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            foreach (IvesGeneralLedgerSourceRow account in source.AccountRows)
            {
                decimal expected = Value(account.OpeningBalance) +
                    Value(account.DebitTurnover) - Value(account.CreditTurnover);

                AddAmountValidation(report, "IVES.ACCOUNT.BALANCE", account.AccountCode,
                    "Closing balance equals opening balance plus debit turnover minus credit turnover.",
                    expected, Value(account.ClosingBalance), source, account, "Accounts");
            }
        }

        private static void ValidateDocuments(
            IvesGeneralLedgerParseResult source,
            IDictionary<string, IvesGeneralLedgerSourceRow> accounts,
            ImportReport report)
        {
            var totals = new Dictionary<string, AmountSet>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow document in source.DocumentRows)
            {
                if (string.IsNullOrWhiteSpace(document.AccountCode))
                {
                    AddDiagnostic(report, "IVES.DOCUMENT.ACCOUNT_MISSING",
                        "A document record has no account code.", source, document, "Documents");
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
                        FindFirst(source.DocumentRows, item.Key);
                    AddDiagnostic(report, "IVES.DOCUMENT.ACCOUNT_NOT_FOUND",
                        "Documents reference account '" + item.Key +
                        "', but no account record exists.",
                        source, firstDocument, "Documents");
                }
            }

            foreach (IvesGeneralLedgerSourceRow account in source.AccountRows)
            {
                totals.TryGetValue(account.AccountCode ?? string.Empty, out AmountSet documentTotal);
                documentTotal = documentTotal ?? new AmountSet();

                AddAmountValidation(report, "IVES.DOCUMENTS.DEBIT", account.AccountCode,
                    "Document debit turnover equals account debit turnover.",
                    documentTotal.Debit, Value(account.DebitTurnover),
                    source, account, "Accounts");
                AddAmountValidation(report, "IVES.DOCUMENTS.CREDIT", account.AccountCode,
                    "Document credit turnover equals account credit turnover.",
                    documentTotal.Credit, Value(account.CreditTurnover),
                    source, account, "Accounts");
            }
        }

        private static Dictionary<string, IvesGeneralLedgerSourceRow> IndexSyntheticSubtotals(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            var result = new Dictionary<string, IvesGeneralLedgerSourceRow>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow row in source.SyntheticSubtotalRows)
            {
                if (string.IsNullOrWhiteSpace(row.AccountCode))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.CODE_MISSING",
                        "A synthetic subtotal has no synthetic account code.",
                        source, row, "SyntheticSubtotals");
                }
                else if (!result.ContainsKey(row.AccountCode))
                {
                    result.Add(row.AccountCode, row);
                }
                else
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.DUPLICATE",
                        "Duplicate synthetic subtotal '" + row.AccountCode + "'.",
                        source, row, "SyntheticSubtotals");
                }
            }

            return result;
        }

        private static void ValidateSyntheticHeadings(
            IvesGeneralLedgerParseResult source,
            IDictionary<string, IvesGeneralLedgerSourceRow> subtotals,
            ImportReport report)
        {
            var headings = new Dictionary<string, IvesGeneralLedgerSourceRow>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow heading in source.SyntheticAccountRows)
            {
                string code = GetSyntheticCode(heading.AccountCode);

                if (code == null)
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.HEADING_CODE_INVALID",
                        "A synthetic-account heading has no recognizable code.",
                        source, heading, "SyntheticAccounts");
                }
                else if (headings.ContainsKey(code))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.HEADING_DUPLICATE",
                        "Duplicate synthetic-account heading '" + code + "'.",
                        source, heading, "SyntheticAccounts");
                }
                else
                {
                    headings.Add(code, heading);
                }
            }

            foreach (KeyValuePair<string, IvesGeneralLedgerSourceRow> heading in headings)
            {
                if (!subtotals.ContainsKey(heading.Key))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.HEADING_WITHOUT_SUBTOTAL",
                        "Synthetic-account heading '" + heading.Key +
                        "' has no subtotal record.",
                        source, heading.Value, "SyntheticAccounts");
                }
            }

            foreach (KeyValuePair<string, IvesGeneralLedgerSourceRow> subtotal in subtotals)
            {
                if (!headings.ContainsKey(subtotal.Key))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.SUBTOTAL_WITHOUT_HEADING",
                        "Synthetic subtotal '" + subtotal.Key +
                        "' has no heading record.",
                        source, subtotal.Value, "SyntheticSubtotals");
                }
            }
        }

        private static void ValidateSyntheticSubtotals(
            IvesGeneralLedgerParseResult source,
            IDictionary<string, IvesGeneralLedgerSourceRow> subtotals,
            ImportReport report)
        {
            var accountTotals = new Dictionary<string, AmountSet>(StringComparer.Ordinal);

            foreach (IvesGeneralLedgerSourceRow account in source.AccountRows)
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

                if (!accountTotals.TryGetValue(syntheticCode, out AmountSet amountSet))
                {
                    amountSet = new AmountSet();
                    accountTotals.Add(syntheticCode, amountSet);
                }

                amountSet.Add(account);
            }

            foreach (KeyValuePair<string, AmountSet> item in accountTotals)
            {
                if (!subtotals.TryGetValue(item.Key, out IvesGeneralLedgerSourceRow subtotal))
                {
                    IvesGeneralLedgerSourceRow firstAccount =
                        FindFirstBySyntheticCode(source.AccountRows, item.Key);
                    AddDiagnostic(report, "IVES.SYNTHETIC.SUBTOTAL_NOT_FOUND",
                        "Accounts exist for synthetic account '" + item.Key +
                        "', but its subtotal is missing.",
                        source, firstAccount, "Accounts");
                    continue;
                }

                AddSyntheticValidations(source, report, item.Key, item.Value, subtotal);
            }

            foreach (KeyValuePair<string, IvesGeneralLedgerSourceRow> item in subtotals)
            {
                if (!accountTotals.ContainsKey(item.Key))
                {
                    AddDiagnostic(report, "IVES.SYNTHETIC.ACCOUNTS_NOT_FOUND",
                        "Synthetic subtotal '" + item.Key +
                        "' has no account records.",
                        source, item.Value, "SyntheticSubtotals");
                }
            }
        }

        private static void AddSyntheticValidations(
            IvesGeneralLedgerParseResult source, ImportReport report, string code,
            AmountSet accounts, IvesGeneralLedgerSourceRow subtotal)
        {
            AddAmountValidation(report, "IVES.SYNTHETIC.OPENING", code,
                "Account opening balances equal the synthetic subtotal.",
                accounts.Opening, Value(subtotal.OpeningBalance),
                source, subtotal, "SyntheticSubtotals");
            AddAmountValidation(report, "IVES.SYNTHETIC.DEBIT", code,
                "Account debit turnovers equal the synthetic subtotal.",
                accounts.Debit, Value(subtotal.DebitTurnover),
                source, subtotal, "SyntheticSubtotals");
            AddAmountValidation(report, "IVES.SYNTHETIC.CREDIT", code,
                "Account credit turnovers equal the synthetic subtotal.",
                accounts.Credit, Value(subtotal.CreditTurnover),
                source, subtotal, "SyntheticSubtotals");
            AddAmountValidation(report, "IVES.SYNTHETIC.CLOSING", code,
                "Account closing balances equal the synthetic subtotal.",
                accounts.Closing, Value(subtotal.ClosingBalance),
                source, subtotal, "SyntheticSubtotals");
        }

        private static void ValidateReportTotal(
            IvesGeneralLedgerParseResult source,
            ImportReport report)
        {
            if (source.ReportTotalRows.Count != 1)
            {
                AddDiagnostic(report, "IVES.REPORT_TOTAL.COUNT",
                    "Expected exactly one report-total record, but found " +
                    source.ReportTotalRows.Count + ".",
                    source,
                    source.ReportTotalRows.Count == 0 ? null : source.ReportTotalRows[0],
                    "ReportTotals");
                return;
            }

            var subtotalSum = new AmountSet();
            foreach (IvesGeneralLedgerSourceRow subtotal in source.SyntheticSubtotalRows)
            {
                subtotalSum.Add(subtotal);
            }

            IvesGeneralLedgerSourceRow total = source.ReportTotalRows[0];
            AddAmountValidation(report, "IVES.REPORT_TOTAL.OPENING", "Report",
                "Synthetic opening subtotals equal the report total.",
                subtotalSum.Opening, Value(total.OpeningBalance),
                source, total, "ReportTotals");
            AddAmountValidation(report, "IVES.REPORT_TOTAL.DEBIT", "Report",
                "Synthetic debit subtotals equal the report total.",
                subtotalSum.Debit, Value(total.DebitTurnover),
                source, total, "ReportTotals");
            AddAmountValidation(report, "IVES.REPORT_TOTAL.CREDIT", "Report",
                "Synthetic credit subtotals equal the report total.",
                subtotalSum.Credit, Value(total.CreditTurnover),
                source, total, "ReportTotals");
            AddAmountValidation(report, "IVES.REPORT_TOTAL.CLOSING", "Report",
                "Synthetic closing subtotals equal the report total.",
                subtotalSum.Closing, Value(total.ClosingBalance),
                source, total, "ReportTotals");
        }

        private static void AddAmountValidation(
            ImportReport report, string code, string scope, string description,
            decimal expected, decimal actual, IvesGeneralLedgerParseResult source,
            IvesGeneralLedgerSourceRow row, string recordSet)
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
            ImportReport report, string code, string message,
            IvesGeneralLedgerParseResult source, IvesGeneralLedgerSourceRow row,
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
                SequenceNumber = row == null ? (int?)null : row.SequenceNumber,
                SourceRowNumber = row == null ? (int?)null : row.SourceRowNumber
            };
        }

        private static string GetSyntheticCode(string accountCode)
        {
            if (string.IsNullOrWhiteSpace(accountCode)) return null;
            int dot = accountCode.IndexOf('.');
            string code = (dot < 0 ? accountCode : accountCode.Substring(0, dot)).Trim();
            return code.Length == 3 ? code : null;
        }

        private static IvesGeneralLedgerSourceRow FindFirst(
            IEnumerable<IvesGeneralLedgerSourceRow> rows, string accountCode)
        {
            foreach (IvesGeneralLedgerSourceRow row in rows)
                if (string.Equals(row.AccountCode, accountCode, StringComparison.Ordinal)) return row;
            return null;
        }

        private static IvesGeneralLedgerSourceRow FindFirstBySyntheticCode(
            IEnumerable<IvesGeneralLedgerSourceRow> rows, string syntheticCode)
        {
            foreach (IvesGeneralLedgerSourceRow row in rows)
                if (string.Equals(GetSyntheticCode(row.AccountCode), syntheticCode,
                    StringComparison.Ordinal)) return row;
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
