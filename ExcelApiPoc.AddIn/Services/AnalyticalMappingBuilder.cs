using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingBuilder
    {
        private const string ExcludedCaption = "EXCLUDED";

        public static AnalyticalMappingData Build(IReadOnlyList<AccountSummary> accounts, AuditTemplatePackageResponse package, AuditReportCalculationResult calculationResult)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (calculationResult == null)
                throw new ArgumentNullException(nameof(calculationResult));

            var result = new AnalyticalMappingData();
            var candidateAccountCodes = new HashSet<string>(calculationResult.AnalyticalRequirements.SelectMany(requirement => requirement.CandidateAccountCodes ?? Array.Empty<string>()), StringComparer.Ordinal);
            AuditReportMappingRuleDefinitionResponse[] analyticalRules = (package.ReportMappingRules ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>()).Where(rule => rule.RequiresAnalyticalMapping).ToArray();
            var rulesBySyntheticCode = analyticalRules.GroupBy(rule => rule.Account3, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

            foreach (AccountSummary account in accounts.Where(account => candidateAccountCodes.Contains(account.AccountCode)).OrderBy(account => account.SyntheticAccountCode).ThenBy(account => account.AccountCode))
            {
                if (!rulesBySyntheticCode.TryGetValue(account.SyntheticAccountCode, out AuditReportMappingRuleDefinitionResponse[] rules))
                {
                    throw new InvalidOperationException($"Analytical account {account.AccountCode} does not " + "have a corresponding mapping rule.");
                }

                result.Rows.Add(new AnalyticalMappingRow 
                {
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        SyntheticAccountCode = account.SyntheticAccountCode,
                        SyntheticAccountName = rules.Select(rule => rule.AccountTitle).FirstOrDefault(value =>!string.IsNullOrWhiteSpace(value)) ??string.Empty,
                        DebitBalance = account.DebitBalance,
                        CreditBalance = account.CreditBalance,
                        NetBalance = account.NetBalance,
                        ValidationRangeName = CreateValidationRangeName(account.SyntheticAccountCode)
                });
            }
            string[] requiredSyntheticCodes = result.Rows .Select(row => row.SyntheticAccountCode) .Distinct(StringComparer.Ordinal) .OrderBy(value => value, StringComparer.Ordinal) .ToArray();

            foreach (string syntheticCode in requiredSyntheticCodes)
            {
                string validationRangeName = CreateValidationRangeName(syntheticCode);

                result.Options.Add(new AnalyticalMappingOption
                {
                        SyntheticAccountCode = syntheticCode,
                        OptionKey = ExcludedCaption,
                        DisplayCaption = ExcludedCaption,
                        SortOrder = 0,
                        ValidationRangeName = validationRangeName
                });

                foreach (AuditReportMappingRuleDefinitionResponse rule in rulesBySyntheticCode[syntheticCode].GroupBy(rule => $"{rule.TableErpId}:{rule.ReportRowNumber}",StringComparer.Ordinal).Select(group => group.First()).OrderBy(rule => rule.TableErpId).ThenBy(rule => rule.ReportRowNumber))
                {
                    result.Options.Add(new AnalyticalMappingOption
                    {
                        SyntheticAccountCode = syntheticCode,
                        OptionKey = $"{rule.TableErpId}/{rule.ReportRowNumber}",
                        DisplayCaption = BuildCaption(package, rule.TableErpId, rule.ReportRowNumber),
                        TableErpId = rule.TableErpId,
                        ReportRowNumber = rule.ReportRowNumber,
                        SortOrder = rule.ReportRowNumber,
                        ValidationRangeName = validationRangeName
                    });
                }
            }
            ValidateOptionCaptions(result.Options);
            return result;
        }

        private static void ValidateOptionCaptions(IEnumerable<AnalyticalMappingOption> options)
        {
            IGrouping<string, AnalyticalMappingOption> duplicate = options
                .GroupBy(option => option.SyntheticAccountCode + "\u001f" + option.DisplayCaption, StringComparer.OrdinalIgnoreCase) .FirstOrDefault(group => group.Count() > 1);

            if (duplicate != null)
            {
                AnalyticalMappingOption option = duplicate.First();
                throw new InvalidOperationException($"Synthetic account {option.SyntheticAccountCode} " + $"contains duplicate mapping caption " + $"'{option.DisplayCaption}'.");
            }
        }

        private static string BuildCaption(AuditTemplatePackageResponse package, int tableErpId, int rowNumber)
        {
            AuditReportTableDefinitionResponse table =
                (package.Template?.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>()).SingleOrDefault(item => item.TableErpId == tableErpId)
                ?? throw new InvalidOperationException($"Mapping references unknown report table {tableErpId}.");

            var rowsByNumber = (table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>()).Where(reportRow => reportRow.RowNumber.HasValue).ToDictionary(reportRow => reportRow.RowNumber.Value);

            if (!rowsByNumber.TryGetValue(rowNumber, out AuditReportRowDefinitionResponse row))
            {
                throw new InvalidOperationException($"Mapping references unknown report row " + $"{tableErpId}/{rowNumber}.");
            }

            if (!string.IsNullOrWhiteSpace(row.MappingCaptionSk))
                return row.MappingCaptionSk.Trim();

            string leafCaption = FirstNonempty(row.TextSk, row.CategorySk, $"Row {rowNumber}");
            return $"r.{rowNumber} - {leafCaption}";
        }

        private static string FirstNonempty(string first, string second, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(first))
                return first.Trim();

            if (!string.IsNullOrWhiteSpace(second))
                return second.Trim();

            return fallback;
        }

        private static string CreateValidationRangeName(string syntheticAccountCode)
        {
            if (string.IsNullOrWhiteSpace(syntheticAccountCode))
            {
                throw new InvalidOperationException("An analytical account does not contain a synthetic " + "account code.");
            }
            char[] safeCharacters = syntheticAccountCode.Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_').ToArray();
            return "_AU_" + new string(safeCharacters);
        }
    }
}
