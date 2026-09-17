using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingWorksheetReader
    {
        private const string ExcludedCaption = "EXCLUDED";
        private const string MappingOptionsTableName = "__AnalyticalMappingOptions";
        private const string AnalyticalMappingsTableName = "AnalyticalMappings";

        public static AnalyticalMappingSelectionReadResult Read(Excel.Workbook workbook)
        {
            bool hasOptionsTable = AuditWorkbookTableReader.ContainsTable(workbook, MappingOptionsTableName);
            bool hasMappingsTable = AuditWorkbookTableReader.ContainsTable(workbook, AnalyticalMappingsTableName);

            if (!hasOptionsTable && !hasMappingsTable)
                return new AnalyticalMappingSelectionReadResult();

            if (hasOptionsTable != hasMappingsTable)
                throw new InvalidOperationException(
                    "The workbook contains an incomplete analytical mapping definition. " +
                    "Tables '" + MappingOptionsTableName + "' and '" + AnalyticalMappingsTableName +
                    "' must either both exist or both be absent.");

            IReadOnlyList<IDictionary<string, object>> optionRows = AuditWorkbookTableReader.ReadRows(workbook, MappingOptionsTableName);
            var options = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            foreach (IDictionary<string, object> optionRow in optionRows)
            {
                string syntheticCode = AuditWorkbookTableReader.GetString(optionRow, "SyntheticAccountCode");
                string caption = AuditWorkbookTableReader.GetString(optionRow, "DisplayCaption");
                string key = CreateOptionKey(syntheticCode, caption);
                if (options.ContainsKey(key))
                    throw new InvalidOperationException("Duplicate mapping option '" + caption + "' for account " + syntheticCode + ".");
                options.Add(key, optionRow);
            }

            IReadOnlyList<IDictionary<string, object>> mappingRows = AuditWorkbookTableReader.ReadRows(workbook, AnalyticalMappingsTableName);
            var result = new AnalyticalMappingSelectionReadResult();
            foreach (IDictionary<string, object> mappingRow in mappingRows)
            {
                string accountCode = AuditWorkbookTableReader.GetString(mappingRow, "AccountCode");
                string syntheticCode = AuditWorkbookTableReader.GetString(mappingRow, "SyntheticAccountCode");
                string caption = AuditWorkbookTableReader.GetString(mappingRow, "MappedTo").Trim();
                string storedSource = GetOptionalString(mappingRow, "MappingSource");
                string suggestedMapping = GetOptionalString(mappingRow, "SuggestedMapping");
                string suggestionConfidence = GetOptionalString(mappingRow, "SuggestionConfidence");
                string suggestionReason = GetOptionalString(mappingRow, "SuggestionReason");
                string effectiveSource = ResolveMappingSource(caption, storedSource, suggestedMapping);

                result.States.Add(new AnalyticalMappingExistingState
                {
                    AccountCode = accountCode,
                    SyntheticAccountCode = syntheticCode,
                    MappedTo = caption,
                    MappingSource = effectiveSource,
                    SuggestedMapping = suggestedMapping,
                    SuggestionConfidence = suggestionConfidence,
                    SuggestionReason = suggestionReason
                });

                if (string.IsNullOrWhiteSpace(caption))
                {
                    result.UnresolvedAccountCodes.Add(accountCode);
                    continue;
                }

                if (!options.TryGetValue(CreateOptionKey(syntheticCode, caption), out IDictionary<string, object> option))
                    throw new InvalidOperationException("Analytical account " + accountCode + " contains invalid mapping '" + caption + "'.");

                bool excluded = string.Equals(caption, ExcludedCaption, StringComparison.OrdinalIgnoreCase);
                var selection = new AnalyticalMappingSelection
                {
                    AccountCode = accountCode,
                    SyntheticAccountCode = syntheticCode,
                    IsExcluded = excluded,
                    TableErpId = AuditWorkbookTableReader.GetNullableInt32(option, "TableErpId"),
                    ReportRowNumber = AuditWorkbookTableReader.GetNullableInt32(option, "ReportRowNumber"),
                    MappingSource = effectiveSource,
                    MappedTo = caption
                };

                if (!excluded && (!selection.TableErpId.HasValue || !selection.ReportRowNumber.HasValue))
                    throw new InvalidOperationException("Mapping '" + caption + "' does not identify a report row.");

                result.Selections.Add(selection);
                if (excluded) result.ExcludedCount++;
                else result.MappedCount++;
            }

            return result;
        }

        private static string ResolveMappingSource(string mappedTo, string storedSource, string suggestedMapping)
        {
            if (string.Equals(storedSource, AnalyticalMappingBuilder.AuditorSource, StringComparison.OrdinalIgnoreCase))
                return AnalyticalMappingBuilder.AuditorSource;

            if (string.Equals(storedSource, AnalyticalMappingBuilder.HeuristicSource, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(mappedTo) && string.Equals(mappedTo, suggestedMapping, StringComparison.OrdinalIgnoreCase))
                    return AnalyticalMappingBuilder.HeuristicSource;
                return AnalyticalMappingBuilder.AuditorSource;
            }

            return string.IsNullOrWhiteSpace(mappedTo)
                ? string.Empty
                : AnalyticalMappingBuilder.AuditorSource;
        }

        private static string GetOptionalString(IDictionary<string, object> row, string columnName)
        {
            return row.ContainsKey(columnName)
                ? AuditWorkbookTableReader.GetString(row, columnName).Trim()
                : string.Empty;
        }

        private static string CreateOptionKey(string syntheticCode, string caption)
        {
            return syntheticCode + "\u001f" + caption;
        }
    }

    internal static class AnalyticalMappingHeuristicRefreshService
    {
        public static AuditWorkbookRecalculationResult RefreshAndRecalculate(Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            AnalyticalMappingSelectionReadResult existing = AnalyticalMappingWorksheetReader.Read(workbook);
            AuditTemplatePackageResponse package = AuditCalculationPackageWorksheetReader.Read(workbook);
            IReadOnlyList<AccountSummary> accounts = AccountWorksheetReader.Read(workbook);

            AuditReportCalculationResult heuristicBase = AuditReportCalculationService.Calculate(
                accounts,
                package,
                Array.Empty<AnalyticalMappingSelection>());

            AnalyticalMappingData mapping = AnalyticalMappingBuilder.Build(
                accounts,
                package,
                heuristicBase,
                existing.States);

            AuditReportReconciliationResult reconciliation = AuditReportReconciliationService.Reconcile(
                workbook,
                package,
                heuristicBase);

            AnalyticalMappingResidueService.Apply(
                mapping,
                package,
                heuristicBase.AnalyticalRequirements,
                reconciliation,
                existing.States);

            AnalyticalMappingValidationWorksheetWriter.ReplaceAnalyticalMappingOptions(workbook, mapping.Options);
            AnalyticalMappingWorksheetWriter.ReplaceWorksheet(workbook, mapping.Rows);

            return AuditWorkbookRecalculationService.Recalculate(workbook);
        }
    }

    internal static class AnalyticalMappingResidueService
    {
        private const decimal ExactTolerance = 1.00m;

        public static void Apply(
            AnalyticalMappingData mapping,
            AuditTemplatePackageResponse package,
            IReadOnlyList<AuditAnalyticalMappingRequirement> requirements,
            AuditReportReconciliationResult reconciliation,
            IReadOnlyList<AnalyticalMappingExistingState> existingStates)
        {
            if (mapping == null || reconciliation == null)
                return;

            var reconciliationByRow = reconciliation.Rows.ToDictionary(
                row => CreateRowKey(row.TableErpId, row.RowNumber),
                StringComparer.Ordinal);

            var optionsByRow = mapping.Options
                .Where(option => option.TableErpId.HasValue && option.ReportRowNumber.HasValue)
                .ToDictionary(
                    option => CreateTargetKey(option.SyntheticAccountCode, option.TableErpId.Value, option.ReportRowNumber.Value),
                    option => option,
                    StringComparer.Ordinal);

            var existingByAccount = existingStates
                .Where(state => state != null && !string.IsNullOrWhiteSpace(state.AccountCode))
                .GroupBy(state => state.AccountCode, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var primaryTables = new HashSet<int>(
                (package.ReportMappingRules ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>())
                    .Where(rule => rule.IncludeInBrutto)
                    .Select(rule => rule.TableErpId));

            foreach (AnalyticalMappingRow row in mapping.Rows)
            {
                var candidates = new List<ResidueCandidate>();

                foreach (AuditAnalyticalMappingRequirement requirement in requirements.Where(requirement =>
                    string.Equals(requirement.SyntheticAccountCode, row.SyntheticAccountCode, StringComparison.Ordinal) &&
                    (requirement.CandidateAccountCodes ?? Array.Empty<string>()).Contains(row.AccountCode, StringComparer.Ordinal)))
                {
                    string optionKey = CreateTargetKey(
                        row.SyntheticAccountCode,
                        requirement.TableErpId,
                        requirement.ReportRowNumber);

                    if (!optionsByRow.TryGetValue(optionKey, out AnalyticalMappingOption option))
                        continue;

                    if (!reconciliationByRow.TryGetValue(
                            CreateRowKey(requirement.TableErpId, requirement.ReportRowNumber),
                            out AuditReportRowReconciliation reportRow))
                        continue;

                    if (!TryGetResidue(
                            requirement,
                            reportRow,
                            primaryTables.Contains(requirement.TableErpId),
                            out decimal before,
                            out decimal after))
                        continue;

                    decimal beforeAbs = Math.Abs(before);
                    decimal afterAbs = Math.Abs(after);
                    if (beforeAbs <= ExactTolerance || afterAbs >= beforeAbs)
                        continue;

                    decimal improvement = beforeAbs == 0 ? 0 : (beforeAbs - afterAbs) / beforeAbs;
                    int score = Score(improvement, afterAbs);
                    if (score <= 0)
                        continue;

                    if (string.Equals(row.SuggestedMapping, option.DisplayCaption, StringComparison.OrdinalIgnoreCase))
                        score += 5;

                    candidates.Add(new ResidueCandidate
                    {
                        Option = option,
                        Score = score,
                        Before = before,
                        After = after,
                        CandidateTotal = requirement.CandidateValue
                    });
                }

                if (candidates.Count == 0)
                    continue;

                ResidueCandidate[] ranked = candidates
                    .OrderByDescending(candidate => candidate.Score)
                    .ThenBy(candidate => Math.Abs(candidate.After))
                    .ThenBy(candidate => candidate.Option.SortOrder)
                    .ToArray();

                ResidueCandidate best = ranked[0];
                int secondScore = ranked.Length > 1 ? ranked[1].Score : -1;
                int gap = best.Score - secondScore;

                string confidence;
                if (best.Score >= 95 && gap >= 15)
                    confidence = "High";
                else if (best.Score >= 70 && gap >= 10)
                    confidence = "Medium";
                else
                    confidence = "Low";

                row.SuggestedMapping = best.Option.DisplayCaption;
                row.SuggestionConfidence = confidence;
                row.SuggestionReason =
                    "Residue score " + best.Score + ". Target difference " +
                    best.Before.ToString("0.00") + " would become " +
                    best.After.ToString("0.00") + " if the current unresolved candidate total " +
                    best.CandidateTotal.ToString("0.00") + " were assigned here.";

                bool auditorOwned = existingByAccount.TryGetValue(row.AccountCode, out AnalyticalMappingExistingState existing) &&
                    string.Equals(existing.MappingSource, AnalyticalMappingBuilder.AuditorSource, StringComparison.OrdinalIgnoreCase);

                if (!auditorOwned && string.Equals(confidence, "High", StringComparison.Ordinal))
                {
                    row.MappedTo = best.Option.DisplayCaption;
                    row.MappingSource = AnalyticalMappingBuilder.HeuristicSource;
                }
                else if (!auditorOwned && string.Equals(row.MappingSource, AnalyticalMappingBuilder.HeuristicSource, StringComparison.OrdinalIgnoreCase) &&
                         !string.Equals(confidence, "High", StringComparison.Ordinal))
                {
                    row.MappedTo = string.Empty;
                    row.MappingSource = string.Empty;
                }
            }
        }

        private static bool TryGetResidue(
            AuditAnalyticalMappingRequirement requirement,
            AuditReportRowReconciliation reconciliation,
            bool primaryTable,
            out decimal before,
            out decimal after)
        {
            before = 0;
            after = 0;
            int slot;
            decimal contribution;

            if (requirement.IncludeInPrimary && reconciliation.Differences[0].HasValue)
            {
                slot = 0;
                contribution = requirement.CandidateValue;
            }
            else if (requirement.IncludeInSecondary && reconciliation.Differences[1].HasValue)
            {
                slot = 1;
                contribution = requirement.CandidateValue;
            }
            else if (reconciliation.Differences[2].HasValue)
            {
                slot = 2;
                contribution = requirement.IncludeInSecondary && primaryTable
                    ? -requirement.CandidateValue
                    : requirement.CandidateValue;
            }
            else
            {
                return false;
            }

            before = reconciliation.Differences[slot].Value;
            after = before + contribution;
            return true;
        }

        private static int Score(decimal improvement, decimal afterAbs)
        {
            if (afterAbs <= ExactTolerance) return 100;
            if (improvement >= 0.95m) return 90;
            if (improvement >= 0.80m) return 80;
            if (improvement >= 0.60m) return 70;
            if (improvement >= 0.35m) return 55;
            if (improvement >= 0.20m) return 40;
            return 0;
        }

        private static string CreateRowKey(int tableErpId, int rowNumber)
        {
            return tableErpId + ":" + rowNumber;
        }

        private static string CreateTargetKey(string syntheticAccountCode, int tableErpId, int rowNumber)
        {
            return syntheticAccountCode + "\u001f" + tableErpId + "/" + rowNumber;
        }

        private sealed class ResidueCandidate
        {
            public AnalyticalMappingOption Option { get; set; }
            public int Score { get; set; }
            public decimal Before { get; set; }
            public decimal After { get; set; }
            public decimal CandidateTotal { get; set; }
        }
    }
}
