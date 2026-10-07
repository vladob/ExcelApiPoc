using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal sealed class SignificanceMapping
    {
        public string TableKey { get; set; }
        public int RowNumber { get; set; }
        public string Source { get; set; }
    }

    internal sealed class SignificanceEvidence
    {
        public long ReportId { get; set; }
        public string TableKey { get; set; }
        public int RowNumber { get; set; }
        public int RowOrdinal { get; set; }
        public int DataColumnOrdinal { get; set; }
        public decimal Value { get; set; }
    }

    internal sealed class SignificanceValue
    {
        public List<SignificanceEvidence> Evidence { get; } = new List<SignificanceEvidence>();
        public string Problem { get; set; }
        public decimal? Value => Problem == null && Evidence.Count > 0
            ? (decimal?)Evidence.Sum(x => x.Value) : null;
    }

    // Resolves printed row numbers through the official template before reading
    // zero-based data ordinals. Never substitutes imported/calculated GL values.
    internal static class PerformanceMaterialityResolver
    {
        public static SignificanceValue Resolve(AccountingEntityPackageEnvelope package,
            IEnumerable<SignificanceMapping> mappings, string source, int year,
            long? selectedStatementId = null, FinancialStatementDto scope = null)
        {
            var result = new SignificanceValue();
            if (package == null) return Missing(result, "RegisterUZ data is unavailable.");
            var statements = package.FinancialStatements.Where(x =>
                x.Statement.PeriodFrom == year + "-01" && x.Statement.PeriodTo == year + "-12" &&
                (selectedStatementId.HasValue ||
                 ((x.Statement.IsConsolidated == true) == (scope?.IsConsolidated == true) &&
                  (x.Statement.IsConsolidatedCentralGovernment == true) == (scope?.IsConsolidatedCentralGovernment == true) &&
                  (x.Statement.IsSummaryPublicAdministration == true) == (scope?.IsSummaryPublicAdministration == true)))).ToArray();
            FinancialStatementEnvelope statement;
            if (selectedStatementId.HasValue)
                statement = statements.SingleOrDefault(x => x.Statement.Id == selectedStatementId.Value);
            else
            {
                var approved = statements.Where(x => x.Statement.ApprovalDate.HasValue).ToArray();
                statement = approved.Length == 1 ? approved[0] : statements
                    .OrderByDescending(x => x.Statement.SubmissionDate)
                    .ThenByDescending(x => x.Statement.Id).FirstOrDefault();
            }
            if (statement == null) return Missing(result, "No matching annual statement for " + year + ".");

            var candidates = new List<SignificanceValue>();
            foreach (var report in statement.FinancialReports)
            {
                var template = report.Template?.Template;
                if (template == null) continue;
                var applicable = mappings.Where(x => string.Equals(x.Source.Trim(), source.Trim(),
                    StringComparison.OrdinalIgnoreCase) && x.TableKey.StartsWith(template.TemplateErpId + "_",
                    StringComparison.Ordinal)).ToArray();
                if (applicable.Length == 0) continue;
                var candidate = new SignificanceValue();
                candidates.Add(candidate);
                // RegisterUZ may omit this optional field (including templates 690/727).
                // Preserve the official amounts just as the reference and multi-year sheets do.
                // An explicitly different currency still requires a conversion policy.
                if (!string.IsNullOrWhiteSpace(report.Report.CurrencyCode) &&
                    !string.Equals(report.Report.CurrencyCode.Trim(), "EUR", StringComparison.OrdinalIgnoreCase))
                {
                    Missing(candidate, "RegisterUZ report " + report.Report.Id + " specifies unsupported currency " +
                        report.Report.CurrencyCode + ".");
                    continue;
                }
                foreach (var mapping in applicable)
                {
                    int tableOrdinal;
                    if (!int.TryParse(mapping.TableKey.Substring(mapping.TableKey.IndexOf('_') + 1), out tableOrdinal))
                    { Missing(candidate, "Invalid mapping table key: " + mapping.TableKey); break; }
                    // ListOfTablesFull uses one-based table numbers; RegisterUZ ordinals are zero-based.
                    tableOrdinal--;
                    var tables = (template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
                        .Where(x => x.TableOrdinal == tableOrdinal).ToArray();
                    if (tables.Length != 1)
                    { Missing(candidate, "Missing or ambiguous template table " + mapping.TableKey); break; }
                    var rows = (tables[0].Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                        .Where(x => x.RowNumber == mapping.RowNumber).ToArray();
                    if (rows.Length != 1)
                    { Missing(candidate, "Missing or ambiguous printed row " + mapping.TableKey + "/" + mapping.RowNumber); break; }
                    int column;
                    try { column = MultiYearBalanceSheetBuilder.ResolveCurrentPeriodDataColumnOrdinal(tables[0]); }
                    catch (InvalidOperationException ex) { Missing(candidate, ex.Message); break; }
                    var values = report.Tables.Where(x => x.Table.TableOrdinal == tableOrdinal)
                        .SelectMany(x => x.Values).Where(x => x.RowOrdinal == rows[0].RowOrdinal &&
                            x.DataColumnOrdinal == column).ToArray();
                    if (values.Length != 1)
                    { Missing(candidate, "Missing or duplicate official value " + mapping.TableKey + "/" + mapping.RowNumber); break; }
                    if (candidate.Evidence.Any(x => x.TableKey == mapping.TableKey && x.RowNumber == mapping.RowNumber))
                    { Missing(candidate, "Duplicate metadata mapping " + mapping.TableKey + "/" + mapping.RowNumber); break; }
                    candidate.Evidence.Add(new SignificanceEvidence { ReportId = report.Report.Id,
                        TableKey = mapping.TableKey, RowNumber = mapping.RowNumber,
                        RowOrdinal = rows[0].RowOrdinal, DataColumnOrdinal = column,
                        Value = values[0].NumericValue });
                }
            }
            if (candidates.Count == 0) return Missing(result, "No mapping for " + source.Trim() + " in statement " + statement.Statement.Id + ".");
            if (candidates.Count > 1) return Missing(result, "Multiple reports map " + source.Trim() + " in statement " + statement.Statement.Id + "; review the source selection.");
            return candidates[0];
        }

        private static SignificanceValue Missing(SignificanceValue result, string message)
        {
            result.Problem = message;
            return result;
        }
    }
}
