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
                    $"Tables '{MappingOptionsTableName}' and '{AnalyticalMappingsTableName}' " +
                    "must either both exist or both be absent.");
            }

            IReadOnlyList<IDictionary<string, object>> optionRows =
                AuditWorkbookTableReader.ReadRows(
                    workbook,
                    MappingOptionsTableName);

            var options = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            foreach (IDictionary<string, object> optionRow in optionRows)
            {
                string syntheticCode = AuditWorkbookTableReader.GetString(optionRow, "SyntheticAccountCode");
                string caption = AuditWorkbookTableReader.GetString(optionRow, "DisplayCaption");
                string key = CreateOptionKey(syntheticCode, caption);
                if (options.ContainsKey(key))
                    throw new InvalidOperationException($"Duplicate mapping option '{caption}' for account {syntheticCode}.");

                options.Add(key, optionRow);
            }

            IReadOnlyList<IDictionary<string, object>> mappingRows =
                AuditWorkbookTableReader.ReadRows(
                    workbook,
                    AnalyticalMappingsTableName);

            var result = new AnalyticalMappingSelectionReadResult();
            foreach (IDictionary<string, object> mappingRow in mappingRows)
            {
                string accountCode = AuditWorkbookTableReader.GetString(mappingRow, "AccountCode");
                string syntheticCode = AuditWorkbookTableReader.GetString(mappingRow, "SyntheticAccountCode");
                string caption = AuditWorkbookTableReader.GetString(mappingRow, "MappedTo").Trim();
                if (string.IsNullOrWhiteSpace(caption))
                {
                    result.UnresolvedAccountCodes.Add(accountCode);
                    continue;
                }

                if (!options.TryGetValue(CreateOptionKey(syntheticCode, caption), out IDictionary<string, object> option))
                {
                    throw new InvalidOperationException(
                        $"Analytical account {accountCode} contains invalid mapping '{caption}'.");
                }
                bool excluded = string.Equals(caption, ExcludedCaption, StringComparison.OrdinalIgnoreCase);
                var selection = new AnalyticalMappingSelection
                {
                    AccountCode = accountCode,
                    SyntheticAccountCode = syntheticCode,
                    IsExcluded = excluded,
                    TableErpId = AuditWorkbookTableReader.GetNullableInt32(option, "TableErpId"),
                    ReportRowNumber = AuditWorkbookTableReader.GetNullableInt32(option, "ReportRowNumber")
                };
                if (!excluded && (!selection.TableErpId.HasValue || !selection.ReportRowNumber.HasValue))
                    throw new InvalidOperationException($"Mapping '{caption}' does not identify a report row.");

                result.Selections.Add(selection);

                if (excluded)
                    result.ExcludedCount++;
                else
                    result.MappedCount++;
            }

            return result;
        }
        private static string CreateOptionKey(string syntheticCode, string caption)
        {
            return syntheticCode + "\u001f" + caption;
        }
    }
}
