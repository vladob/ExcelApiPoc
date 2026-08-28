using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditReportCalculationService
    {
        public static AuditReportCalculationResult Calculate(IReadOnlyList<AccountSummary> accounts, AuditTemplatePackageResponse package)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (package.Template == null)
            {
                throw new InvalidOperationException("The template package does not contain a template.");
            }

            var result = new AuditReportCalculationResult();
            Dictionary<string, List<AccountSummary>> accountsBySyntheticCode = CreateAccountIndex(accounts);
            Dictionary<string, AuditReportRowCalculation> rowsByKey = CreateReportRows(package, result);
            AuditReportMappingRuleDefinitionResponse[] mappingRules = package.ReportMappingRules ?? Array.Empty <AuditReportMappingRuleDefinitionResponse>();
            var mappedSyntheticCodes = new HashSet<string>(StringComparer.Ordinal);
            var primaryTables = new HashSet<int>();

            foreach (AuditReportMappingRuleDefinitionResponse rule in mappingRules)
            {
                ValidateRule(rule);
                mappedSyntheticCodes.Add(rule.Account3);
                if (rule.IncludeInBrutto) primaryTables.Add(rule.TableErpId);
                string rowKey = CreateRowKey(rule.TableErpId, rule.ReportRowNumber);

                if (!rowsByKey.TryGetValue(rowKey, out AuditReportRowCalculation targetRow))
                {
                    throw new InvalidOperationException($"Mapping rule references unknown report row " + $"{rowKey}.");
                }

                List<AccountSummary> candidateAccounts;

                if (!accountsBySyntheticCode.TryGetValue(rule.Account3, out candidateAccounts))
                {
                    candidateAccounts =new List<AccountSummary>();
                }

                if (rule.RequiresAnalyticalMapping)
                {
                    AddAnalyticalRequirement(result, rule, candidateAccounts);
                    continue;
                }

                result.AutomaticRuleCount++;

                decimal value = candidateAccounts.Sum(account => ResolveValue(account, rule.Side, rule.ValueSource));
                if (value != 0)
                    result.AutomaticRulesWithValues++;

                AddMappedValue(targetRow, rule, value);
            }

            AddUnmappedAccounts(result, accounts, mappedSyntheticCodes);
            ApplyCalculationPlan(package, rowsByKey);

            foreach (AuditReportRowCalculation row in result.Rows)
            {
                row.CalculatedValue = primaryTables.Contains(row.TableErpId) ? row.PrimaryValue - row.SecondaryValue : row.SecondaryValue;
            }

            return result;
        }

        private static Dictionary<string, List<AccountSummary>> CreateAccountIndex(IReadOnlyList<AccountSummary> accounts)
        {
            var index = new Dictionary<string, List<AccountSummary>>( StringComparer.Ordinal);

            foreach (AccountSummary account in accounts)
            {
                if (string.IsNullOrWhiteSpace(account.SyntheticAccountCode))
                {
                    continue;
                }

                if (!index.TryGetValue(account.SyntheticAccountCode, out List<AccountSummary> accountList))
                {
                    accountList = new List<AccountSummary>();
                    index.Add(account.SyntheticAccountCode, accountList);
                }
                accountList.Add(account);
            }
            return index;
        }

        private static Dictionary<string, AuditReportRowCalculation> CreateReportRows(AuditTemplatePackageResponse package, AuditReportCalculationResult result)
        {
            var index = new Dictionary<string, AuditReportRowCalculation>(StringComparer.Ordinal);

            AuditReportTableDefinitionResponse[] tables = package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>();

            foreach (AuditReportTableDefinitionResponse table in tables)
            {
                AuditReportRowDefinitionResponse[] rows = table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>();

                foreach (AuditReportRowDefinitionResponse row in rows)
                {
                    if (!row.RowNumber.HasValue)
                        continue;

                    var calculation = new AuditReportRowCalculation{TableErpId = table.TableErpId, RowNumber = row.RowNumber.Value};
                    string rowKey = CreateRowKey(calculation.TableErpId, calculation.RowNumber);
                    if (index.ContainsKey(rowKey))
                    {
                        throw new InvalidOperationException( $"Duplicate report row '{rowKey}'.");
                    }

                    index.Add(rowKey, calculation);
                    result.Rows.Add(calculation);
                }
            }
            return index;
        }

        private static void ValidateRule(AuditReportMappingRuleDefinitionResponse rule)
        {
            if (rule == null)
            {
                throw new InvalidOperationException("The package contains an empty mapping rule.");
            }

            if (string.IsNullOrWhiteSpace(rule.Account3))
            {
                throw new InvalidOperationException("A mapping rule does not contain an account.");
            }

            if (rule.IncludeInBrutto == rule.IncludeInCorrection)
            {
                throw new InvalidOperationException($"Mapping rule for account {rule.Account3}, " + $"row {rule.ReportRowNumber}, must select " + $"exactly one result value.");
            }
        }

        private static void AddMappedValue(AuditReportRowCalculation targetRow, AuditReportMappingRuleDefinitionResponse rule, decimal value)
        {
            if (rule.IncludeInBrutto)
                targetRow.PrimaryValue += value;

            if (rule.IncludeInCorrection)
                targetRow.SecondaryValue += value;
        }

        private static void AddAnalyticalRequirement(AuditReportCalculationResult result, AuditReportMappingRuleDefinitionResponse rule, IEnumerable<AccountSummary> candidates)
        {
            AccountSummary[] nonzeroCandidates = candidates.Where(account => ResolveValue(account, rule.Side, rule.ValueSource) != 0).ToArray();

            if (nonzeroCandidates.Length == 0)
                return;

            result.AnalyticalRequirements.Add(new AuditAnalyticalMappingRequirement
                {
                    TableErpId = rule.TableErpId,
                    ReportRowNumber = rule.ReportRowNumber,
                    SyntheticAccountCode = rule.Account3,
                    AccountTitle = rule.AccountTitle,
                    BalanceSide = rule.Side,
                    ValueSource = rule.ValueSource,
                    IncludeInPrimary = rule.IncludeInBrutto,
                    IncludeInSecondary = rule.IncludeInCorrection,
                    CandidateAccountCodes = nonzeroCandidates.Select(account => account.AccountCode).ToArray(),
                    CandidateValue = nonzeroCandidates.Sum(account => ResolveValue(account, rule.Side, rule.ValueSource))
                });
        }

        private static decimal ResolveValue(AccountSummary account, string balanceSide, string valueSource)
        {
            if (string.Equals(valueSource, "ClosingDebit", StringComparison.OrdinalIgnoreCase))
            {
                return account.DebitBalance;
            }

            if (string.Equals(valueSource, "ClosingCredit", StringComparison.OrdinalIgnoreCase))
            {
                return account.CreditBalance;
            }

            if (string.Equals(valueSource, "ClosingNetto", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(balanceSide, "Assets", StringComparison.OrdinalIgnoreCase))
                {
                    return account.NetBalance;
                }

                if (string.Equals(balanceSide, "Liabilities", StringComparison.OrdinalIgnoreCase))
                {
                    return -account.NetBalance;
                }

                throw new InvalidOperationException($"Unsupported balance side '{balanceSide}'.");
            }
            throw new InvalidOperationException($"Unsupported value source '{valueSource}'.");
        }

        private static void ApplyCalculationPlan(AuditTemplatePackageResponse package, IDictionary<string, AuditReportRowCalculation> rowsByKey)
        {
            AuditCalculationDependencyDefinitionResponse[] plan = package.CalculationPlan ?? Array.Empty<AuditCalculationDependencyDefinitionResponse>();

            foreach (IGrouping<int, AuditCalculationDependencyDefinitionResponse> level in plan.OrderBy(item => item.CalculationLevel).GroupBy(item => item.CalculationLevel))
            {
                var calculatedTargets = new List<AuditReportRowCalculation>();
                foreach (IGrouping<string, AuditCalculationDependencyDefinitionResponse> target in level.GroupBy(item => CreateRowKey(item.TargetTableErpId, item.TargetRowNumber)))
                {
                    if (!rowsByKey.TryGetValue(target.Key, out AuditReportRowCalculation targetRow))
                    {
                        throw new InvalidOperationException($"Calculation plan references unknown " + $"target row {target.Key}.");
                    }

                    decimal primaryValue = 0;
                    decimal secondaryValue = 0;

                    foreach (AuditCalculationDependencyDefinitionResponse dependency in target)
                    {
                        string sourceKey = CreateRowKey(dependency.SourceTableErpId, dependency.SourceRowNumber);
                        if (!rowsByKey.TryGetValue(sourceKey, out AuditReportRowCalculation sourceRow))
                        {
                            throw new InvalidOperationException($"Calculation plan references unknown " + $"source row {sourceKey}.");
                        }

                        primaryValue += dependency.Coefficient * sourceRow.PrimaryValue;
                        secondaryValue += dependency.Coefficient * sourceRow.SecondaryValue;
                    }

                    calculatedTargets.Add(new AuditReportRowCalculation
                        {
                            TableErpId = targetRow.TableErpId,
                            RowNumber = targetRow.RowNumber,
                            PrimaryValue = primaryValue,
                            SecondaryValue = secondaryValue
                        });
                }

                foreach (AuditReportRowCalculation calculated in calculatedTargets)
                {
                    string targetKey = CreateRowKey(calculated.TableErpId, calculated.RowNumber);
                    AuditReportRowCalculation targetRow = rowsByKey[targetKey];
                    targetRow.PrimaryValue = calculated.PrimaryValue;
                    targetRow.SecondaryValue = calculated.SecondaryValue;
                }
            }
        }

        private static void AddUnmappedAccounts(AuditReportCalculationResult result, IEnumerable<AccountSummary> accounts, ISet<string> mappedSyntheticCodes)
        {
            foreach (AccountSummary account in accounts)
            {
                if (account.DebitBalance == 0 && account.CreditBalance == 0)
                {
                    continue;
                }

                if (!mappedSyntheticCodes.Contains(account.SyntheticAccountCode))
                {
                    result.UnmappedAccounts.Add(account);
                }
            }
        }

        private static string CreateRowKey(int tableErpId, int rowNumber)
        {
            return $"{tableErpId}:{rowNumber}";
        }
    }
}