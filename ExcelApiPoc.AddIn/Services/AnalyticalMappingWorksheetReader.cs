using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
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
            bool hasOptionsTable =
                AuditWorkbookTableReader.ContainsTable(
                    workbook,
                    MappingOptionsTableName);

            bool hasMappingsTable =
                AuditWorkbookTableReader.ContainsTable(
                    workbook,
                    AnalyticalMappingsTableName);

            if (!hasOptionsTable && !hasMappingsTable)
                return new AnalyticalMappingSelectionReadResult();

            if (hasOptionsTable != hasMappingsTable)
            {
                throw new InvalidOperationException(
                    "The workbook contains an incomplete analytical mapping definition. " +
                    "Tables '" + MappingOptionsTableName + "' and '" +
                    AnalyticalMappingsTableName +
                    "' must either both exist or both be absent.");
            }

            IReadOnlyList<IDictionary<string, object>> optionRows =
                AuditWorkbookTableReader.ReadRows(
                    workbook,
                    MappingOptionsTableName);

            var options = new Dictionary<string, IDictionary<string, object>>(
                StringComparer.OrdinalIgnoreCase);
            foreach (IDictionary<string, object> optionRow in optionRows)
            {
                string syntheticCode =
                    AuditWorkbookTableReader.GetString(
                        optionRow,
                        "SyntheticAccountCode");
                string caption =
                    AuditWorkbookTableReader.GetString(
                        optionRow,
                        "DisplayCaption");
                string key = CreateOptionKey(syntheticCode, caption);
                if (options.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        "Duplicate mapping option '" + caption +
                        "' for account " + syntheticCode + ".");
                }
                options.Add(key, optionRow);
            }

            IReadOnlyList<IDictionary<string, object>> mappingRows =
                AuditWorkbookTableReader.ReadRows(
                    workbook,
                    AnalyticalMappingsTableName);

            var result = new AnalyticalMappingSelectionReadResult();
            foreach (IDictionary<string, object> mappingRow in mappingRows)
            {
                string accountCode =
                    AuditWorkbookTableReader.GetString(mappingRow, "AccountCode");
                string syntheticCode =
                    AuditWorkbookTableReader.GetString(
                        mappingRow,
                        "SyntheticAccountCode");
                string caption =
                    AuditWorkbookTableReader.GetString(mappingRow, "MappedTo").Trim();
                string storedSource = GetOptionalString(
                    mappingRow,
                    "MappingSource");
                string suggestedMapping = GetOptionalString(
                    mappingRow,
                    "SuggestedMapping");
                string suggestionConfidence = GetOptionalString(
                    mappingRow,
                    "SuggestionConfidence");
                string suggestionReason = GetOptionalString(
                    mappingRow,
                    "SuggestionReason");

                string effectiveSource = ResolveMappingSource(
                    caption,
                    storedSource,
                    suggestedMapping);

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

                if (!options.TryGetValue(
                        CreateOptionKey(syntheticCode, caption),
                        out IDictionary<string, object> option))
                {
                    throw new InvalidOperationException(
                        "Analytical account " + accountCode +
                        " contains invalid mapping '" + caption + "'.");
                }

                bool excluded = string.Equals(
                    caption,
                    ExcludedCaption,
                    StringComparison.OrdinalIgnoreCase);

                var selection = new AnalyticalMappingSelection
                {
                    AccountCode = accountCode,
                    SyntheticAccountCode = syntheticCode,
                    IsExcluded = excluded,
                    TableErpId = AuditWorkbookTableReader.GetNullableInt32(
                        option,
                        "TableErpId"),
                    ReportRowNumber =
                        AuditWorkbookTableReader.GetNullableInt32(
                            option,
                            "ReportRowNumber"),
                    MappingSource = effectiveSource,
                    MappedTo = caption
                };

                if (!excluded &&
                    (!selection.TableErpId.HasValue ||
                     !selection.ReportRowNumber.HasValue))
                {
                    throw new InvalidOperationException(
                        "Mapping '" + caption +
                        "' does not identify a report row.");
                }

                result.Selections.Add(selection);

                if (excluded)
                    result.ExcludedCount++;
                else
                    result.MappedCount++;
            }

            return result;
        }

        private static string ResolveMappingSource(
            string mappedTo,
            string storedSource,
            string suggestedMapping)
        {
            if (string.Equals(
                    storedSource,
                    AnalyticalMappingBuilder.AuditorSource,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AnalyticalMappingBuilder.AuditorSource;
            }

            if (string.Equals(
                    storedSource,
                    AnalyticalMappingBuilder.HeuristicSource,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(mappedTo) &&
                    string.Equals(
                        mappedTo,
                        suggestedMapping,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return AnalyticalMappingBuilder.HeuristicSource;
                }

                // Changing or clearing a heuristic preselection is an auditor action.
                return AnalyticalMappingBuilder.AuditorSource;
            }

            // Workbooks created before provenance tracking, or a manual choice made
            // from a non-preselected suggestion, are treated as auditor-owned.
            return string.IsNullOrWhiteSpace(mappedTo)
                ? string.Empty
                : AnalyticalMappingBuilder.AuditorSource;
        }

        private static string GetOptionalString(
            IDictionary<string, object> row,
            string columnName)
        {
            return row.ContainsKey(columnName)
                ? AuditWorkbookTableReader.GetString(row, columnName).Trim()
                : string.Empty;
        }

        private static string CreateOptionKey(
            string syntheticCode,
            string caption)
        {
            return syntheticCode + "\u001f" + caption;
        }
    }
}
