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
            return Calculate(
                accounts,
                package,
                Array.Empty<AnalyticalMappingSelection>());
        }

        public static AuditReportCalculationResult Calculate(
            IReadOnlyList<AccountSummary> accounts,
            AuditTemplatePackageResponse package,
            IReadOnlyList<AnalyticalMappingSelection> analyticalSelections)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (analyticalSelections == null)
                throw new ArgumentNullException(nameof(analyticalSelections));

            if (package.Template == null)
            {
                throw new InvalidOperationException("The template package does not contain a template.");
            }

            var result = new AuditReportCalculationResult();
            Dictionary<string, List<AccountSummary>> accountsBySyntheticCode = CreateAccountIndex(accounts);

            // Account-side value sources are authoritative; legacy rule.Side is not.
            Dictionary<string, AuditAccountGroupDefinitionResponse> accountGroupsByCode =
                CreateAccountGroupIndex(package);
            Dictionary<string, AuditReportRowCalculation> rowsByKey = CreateReportRows(package, result);
            AuditReportMappingRuleDefinitionResponse[] mappingRules = package.ReportMappingRules ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>();
            Dictionary<string, AnalyticalMappingSelection> selectionsByAccount =
                ValidateAnalyticalSelections(accounts, mappingRules, analyticalSelections);

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
                    candidateAccounts = new List<AccountSummary>();
                }

                if (rule.RequiresAnalyticalMapping)
                {
                    AddAnalyticalRuleResult(
                        result,
                        targetRow,
                        rule,
                        candidateAccounts,
                        selectionsByAccount,
                        accountGroupsByCode);

                    continue;
                }

                result.AutomaticRuleCount++;

                decimal value = candidateAccounts.Sum(account =>
                    ResolveRuleValue(account, rule, accountGroupsByCode));
                if (value != 0)
                    result.AutomaticRulesWithValues++;

                AddMappedValue(targetRow, rule, value);
            }

            AddUnmappedAccounts(result, accounts, mappedSyntheticCodes);

            foreach (AuditReportRowCalculation row in result.Rows)
            {
                row.CalculatedValue = primaryTables.Contains(row.TableErpId)
                    ? row.PrimaryValue - row.SecondaryValue
                    : row.SecondaryValue;
            }

            ApplyCalculationPlan(package, rowsByKey);

            return result;
        }

        private static Dictionary<string, AuditAccountGroupDefinitionResponse>
            CreateAccountGroupIndex(AuditTemplatePackageResponse package)
        {
            var index = new Dictionary<string, AuditAccountGroupDefinitionResponse>(
                StringComparer.Ordinal);

            foreach (AuditAccountGroupDefinitionResponse accountGroup in
                package.AccountGroups ?? Array.Empty<AuditAccountGroupDefinitionResponse>())
            {
                if (accountGroup == null ||
                    string.IsNullOrWhiteSpace(accountGroup.Account))
                {
                    throw new InvalidOperationException(
                        "The package contains an invalid account-group definition.");
                }

                if (index.ContainsKey(accountGroup.Account))
                {
                    throw new InvalidOperationException(
                        $"The package contains duplicate account group " +
                        $"{accountGroup.Account}.");
                }

                index.Add(accountGroup.Account, accountGroup);
            }

            return index;
        }

        private static Dictionary<string, List<AccountSummary>> CreateAccountIndex(IReadOnlyList<AccountSummary> accounts)
        {
            var index = new Dictionary<string, List<AccountSummary>>(StringComparer.Ordinal);

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

                    var calculation = new AuditReportRowCalculation { TableErpId = table.TableErpId, RowNumber = row.RowNumber.Value };
                    string rowKey = CreateRowKey(calculation.TableErpId, calculation.RowNumber);
                    if (index.ContainsKey(rowKey))
                    {
                        throw new InvalidOperationException($"Duplicate report row '{rowKey}'.");
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

        private static void AddAnalyticalRuleResult(
            AuditReportCalculationResult result,
            AuditReportRowCalculation targetRow,
            AuditReportMappingRuleDefinitionResponse rule,
            IEnumerable<AccountSummary> candidates,
            IReadOnlyDictionary<string, AnalyticalMappingSelection> selectionsByAccount,
            IReadOnlyDictionary<string, AuditAccountGroupDefinitionResponse> accountGroupsByCode)
        {
            AccountSummary[] nonzeroCandidates = candidates
                .Where(account => ResolveRuleValue(
                    account,
                    rule,
                    accountGroupsByCode) != 0)
                .ToArray();

            if (nonzeroCandidates.Length == 0)
                return;

            var unresolved = new List<AccountSummary>();
            decimal mappedValue = 0;

            foreach (AccountSummary account in nonzeroCandidates)
            {
                if (!selectionsByAccount.TryGetValue(account.AccountCode, out AnalyticalMappingSelection selection))
                {
                    unresolved.Add(account);
                    continue;
                }

                if (selection.IsExcluded)
                    continue;

                if (selection.TableErpId == rule.TableErpId &&
                    selection.ReportRowNumber == rule.ReportRowNumber)
                {
                    mappedValue += ResolveRuleValue(
                        account,
                        rule,
                        accountGroupsByCode);
                }
            }

            AddMappedValue(targetRow, rule, mappedValue);

            if (unresolved.Count > 0)
            {
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
                    CandidateAccountCodes = unresolved.Select(account => account.AccountCode).ToArray(),
                    CandidateValue = unresolved.Sum(account =>
                        ResolveRuleValue(account, rule, accountGroupsByCode))
                });
            }
        }

        private static decimal ResolveRuleValue(
            AccountSummary account,
            AuditReportMappingRuleDefinitionResponse rule,
            IReadOnlyDictionary<string, AuditAccountGroupDefinitionResponse> accountGroupsByCode)
        {
            if (!accountGroupsByCode.TryGetValue(
                    rule.Account3,
                    out AuditAccountGroupDefinitionResponse accountGroup))
            {
                throw new InvalidOperationException(
                    $"Mapping rule references unknown account group " +
                    $"{rule.Account3}.");
            }

            string balanceSide;
            string valueSource;

            if (rule.IncludeInBrutto)
            {
                balanceSide = "Assets";
                valueSource = accountGroup.AssetsValueSource;
            }
            else
            {
                balanceSide = "Liabilities";
                valueSource = accountGroup.LiabilitiesValueSource;
            }

            if (string.IsNullOrWhiteSpace(valueSource))
                return 0;

            return ResolveValue(account, balanceSide, valueSource);
        }

        private static Dictionary<string, AnalyticalMappingSelection> ValidateAnalyticalSelections(
            IEnumerable<AccountSummary> accounts,
            IEnumerable<AuditReportMappingRuleDefinitionResponse> mappingRules,
            IEnumerable<AnalyticalMappingSelection> selections)
        {
            var accountsByCode = accounts.ToDictionary(
                account => account.AccountCode,
                StringComparer.Ordinal);

            var permittedTargets = new HashSet<string>(
                mappingRules
                    .Where(rule => rule.RequiresAnalyticalMapping)
                    .Select(rule => rule.Account3 + "\u001f" +
                        CreateRowKey(rule.TableErpId, rule.ReportRowNumber)),
                StringComparer.Ordinal);

            var result = new Dictionary<string, AnalyticalMappingSelection>(StringComparer.Ordinal);

            foreach (AnalyticalMappingSelection selection in selections)
            {
                if (selection == null || string.IsNullOrWhiteSpace(selection.AccountCode))
                    throw new InvalidOperationException("An analytical mapping selection does not contain an account code.");

                if (!accountsByCode.TryGetValue(selection.AccountCode, out AccountSummary account))
                    throw new InvalidOperationException($"Analytical mapping references unknown account {selection.AccountCode}.");

                if (!string.Equals(account.SyntheticAccountCode, selection.SyntheticAccountCode, StringComparison.Ordinal))
                    throw new InvalidOperationException($"Analytical account {selection.AccountCode} has an invalid synthetic account.");

                if (result.ContainsKey(selection.AccountCode))
                    throw new InvalidOperationException($"Analytical account {selection.AccountCode} is mapped more than once.");

                if (selection.IsExcluded)
                {
                    if (selection.TableErpId.HasValue || selection.ReportRowNumber.HasValue)
                        throw new InvalidOperationException($"Excluded account {selection.AccountCode} contains a report-row target.");
                }
                else
                {
                    if (!selection.TableErpId.HasValue || !selection.ReportRowNumber.HasValue)
                        throw new InvalidOperationException($"Analytical account {selection.AccountCode} does not contain a report-row target.");

                    string permittedTarget = selection.SyntheticAccountCode + "\u001f" +
                        CreateRowKey(selection.TableErpId.Value, selection.ReportRowNumber.Value);

                    if (!permittedTargets.Contains(permittedTarget))
                    {
                        throw new InvalidOperationException(
                            $"Report row {selection.TableErpId}/{selection.ReportRowNumber} " +
                            $"is not valid for analytical account {selection.AccountCode}.");
                    }
                }

                result.Add(selection.AccountCode, selection);
            }

            return result;
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
                    decimal calculatedValue = 0;

                    foreach (AuditCalculationDependencyDefinitionResponse dependency in target)
                    {
                        string sourceKey = CreateRowKey(dependency.SourceTableErpId, dependency.SourceRowNumber);
                        if (!rowsByKey.TryGetValue(sourceKey, out AuditReportRowCalculation sourceRow))
                        {
                            throw new InvalidOperationException($"Calculation plan references unknown " + $"source row {sourceKey}.");
                        }

                        primaryValue += dependency.Coefficient * sourceRow.PrimaryValue;
                        secondaryValue += dependency.Coefficient * sourceRow.SecondaryValue;
                        calculatedValue += dependency.Coefficient * sourceRow.CalculatedValue;
                    }

                    calculatedTargets.Add(new AuditReportRowCalculation
                    {
                        TableErpId = targetRow.TableErpId,
                        RowNumber = targetRow.RowNumber,
                        PrimaryValue = primaryValue,
                        SecondaryValue = secondaryValue,
                        CalculatedValue = calculatedValue
                    });
                }

                foreach (AuditReportRowCalculation calculated in calculatedTargets)
                {
                    string targetKey = CreateRowKey(calculated.TableErpId, calculated.RowNumber);
                    AuditReportRowCalculation targetRow = rowsByKey[targetKey];
                    targetRow.PrimaryValue = calculated.PrimaryValue;
                    targetRow.SecondaryValue = calculated.SecondaryValue;
                    targetRow.CalculatedValue = calculated.CalculatedValue;
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
