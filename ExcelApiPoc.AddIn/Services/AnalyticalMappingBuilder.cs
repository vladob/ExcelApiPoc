using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AnalyticalMappingBuilder
    {
        private const string ExcludedCaption = "EXCLUDED";
        internal const string HeuristicSource = "Heuristic";
        internal const string AuditorSource = "Auditor";

        private static readonly HashSet<string> IgnoredWords =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "account", "ucet", "účet", "ucty", "účty",
                "ostatne", "ostatné", "ostatny", "ostatný",
                "ine", "iné", "other", "row"
            };

        public static AnalyticalMappingData Build(
            IReadOnlyList<AccountSummary> accounts,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculationResult)
        {
            return Build(
                accounts,
                package,
                calculationResult,
                Array.Empty<AnalyticalMappingExistingState>());
        }

        public static AnalyticalMappingData Build(
            IReadOnlyList<AccountSummary> accounts,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculationResult,
            IReadOnlyList<AnalyticalMappingExistingState> existingStates)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));
            if (package == null)
                throw new ArgumentNullException(nameof(package));
            if (calculationResult == null)
                throw new ArgumentNullException(nameof(calculationResult));
            if (existingStates == null)
                throw new ArgumentNullException(nameof(existingStates));

            var result = new AnalyticalMappingData();
            AuditReportMappingRuleDefinitionResponse[] analyticalRules =
                (package.ReportMappingRules ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>())
                    .Where(rule => rule.RequiresAnalyticalMapping)
                    .ToArray();

            var rulesBySyntheticCode = analyticalRules
                .GroupBy(rule => rule.Account3, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray(),
                    StringComparer.Ordinal);

            var candidateAccountCodes = new HashSet<string>(
                calculationResult.AnalyticalRequirements
                    .SelectMany(requirement =>
                        requirement.CandidateAccountCodes ?? Array.Empty<string>()),
                StringComparer.Ordinal);

            foreach (AnalyticalMappingExistingState state in existingStates)
            {
                if (state != null && !string.IsNullOrWhiteSpace(state.AccountCode))
                    candidateAccountCodes.Add(state.AccountCode);
            }

            Dictionary<string, AccountSummary> accountsByCode = accounts
                .Where(account => account != null &&
                                  !string.IsNullOrWhiteSpace(account.AccountCode))
                .GroupBy(account => account.AccountCode, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);

            Dictionary<string, AnalyticalMappingExistingState> statesByAccount =
                existingStates
                    .Where(state => state != null &&
                                    !string.IsNullOrWhiteSpace(state.AccountCode))
                    .GroupBy(state => state.AccountCode, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First(),
                        StringComparer.Ordinal);

            foreach (AccountSummary account in accounts
                .Where(account => candidateAccountCodes.Contains(account.AccountCode))
                .OrderBy(account => account.SyntheticAccountCode)
                .ThenBy(account => account.AccountCode))
            {
                if (!rulesBySyntheticCode.TryGetValue(
                        account.SyntheticAccountCode,
                        out AuditReportMappingRuleDefinitionResponse[] rules))
                {
                    continue;
                }

                var row = new AnalyticalMappingRow
                {
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    SyntheticAccountCode = account.SyntheticAccountCode,
                    SyntheticAccountName = rules
                        .Select(rule => rule.AccountTitle)
                        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
                        ?? string.Empty,
                    DebitBalance = account.DebitBalance,
                    CreditBalance = account.CreditBalance,
                    NetBalance = account.NetBalance,
                    ValidationRangeName =
                        CreateValidationRangeName(account.SyntheticAccountCode)
                };

                if (statesByAccount.TryGetValue(
                        account.AccountCode,
                        out AnalyticalMappingExistingState existing) &&
                    string.Equals(
                        existing.MappingSource,
                        AuditorSource,
                        StringComparison.OrdinalIgnoreCase))
                {
                    row.MappedTo = existing.MappedTo;
                    row.MappingSource = AuditorSource;
                }

                result.Rows.Add(row);
            }

            string[] requiredSyntheticCodes = result.Rows
                .Select(row => row.SyntheticAccountCode)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            foreach (string syntheticCode in requiredSyntheticCodes)
            {
                string validationRangeName =
                    CreateValidationRangeName(syntheticCode);

                result.Options.Add(new AnalyticalMappingOption
                {
                    SyntheticAccountCode = syntheticCode,
                    OptionKey = ExcludedCaption,
                    DisplayCaption = ExcludedCaption,
                    SortOrder = 0,
                    ValidationRangeName = validationRangeName
                });

                foreach (AuditReportMappingRuleDefinitionResponse rule in
                    rulesBySyntheticCode[syntheticCode]
                        .GroupBy(
                            rule => rule.TableErpId + ":" + rule.ReportRowNumber,
                            StringComparer.Ordinal)
                        .Select(group => group.First())
                        .OrderBy(rule => rule.TableErpId)
                        .ThenBy(rule => rule.ReportRowNumber))
                {
                    result.Options.Add(new AnalyticalMappingOption
                    {
                        SyntheticAccountCode = syntheticCode,
                        OptionKey = rule.TableErpId + "/" + rule.ReportRowNumber,
                        DisplayCaption = BuildCaption(
                            package,
                            rule.TableErpId,
                            rule.ReportRowNumber),
                        TableErpId = rule.TableErpId,
                        ReportRowNumber = rule.ReportRowNumber,
                        SortOrder = rule.ReportRowNumber,
                        ValidationRangeName = validationRangeName
                    });
                }
            }

            ValidateOptionCaptions(result.Options);
            ApplyHeuristicSuggestions(
                result,
                accountsByCode,
                calculationResult.AnalyticalRequirements,
                existingStates);
            return result;
        }

        private static void ApplyHeuristicSuggestions(
            AnalyticalMappingData data,
            IReadOnlyDictionary<string, AccountSummary> accountsByCode,
            IReadOnlyList<AuditAnalyticalMappingRequirement> requirements,
            IReadOnlyList<AnalyticalMappingExistingState> existingStates)
        {
            Dictionary<string, AnalyticalMappingOption> optionsByTarget =
                data.Options
                    .Where(option => option.TableErpId.HasValue &&
                                     option.ReportRowNumber.HasValue)
                    .ToDictionary(
                        option => CreateTargetKey(
                            option.SyntheticAccountCode,
                            option.TableErpId.Value,
                            option.ReportRowNumber.Value),
                        option => option,
                        StringComparer.Ordinal);

            Dictionary<string, AnalyticalMappingExistingState[]> auditorStatesBySynthetic =
                existingStates
                    .Where(state => state != null &&
                                    string.Equals(
                                        state.MappingSource,
                                        AuditorSource,
                                        StringComparison.OrdinalIgnoreCase) &&
                                    !string.IsNullOrWhiteSpace(state.MappedTo) &&
                                    !string.Equals(
                                        state.MappedTo,
                                        ExcludedCaption,
                                        StringComparison.OrdinalIgnoreCase))
                    .GroupBy(
                        state => state.SyntheticAccountCode ?? string.Empty,
                        StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .OrderBy(state => state.AccountCode, StringComparer.Ordinal)
                            .ToArray(),
                        StringComparer.Ordinal);

            foreach (AnalyticalMappingRow row in data.Rows)
            {
                AuditAnalyticalMappingRequirement[] activeRequirements =
                    requirements
                        .Where(requirement =>
                            string.Equals(
                                requirement.SyntheticAccountCode,
                                row.SyntheticAccountCode,
                                StringComparison.Ordinal) &&
                            (requirement.CandidateAccountCodes ?? Array.Empty<string>())
                                .Contains(row.AccountCode, StringComparer.Ordinal))
                        .GroupBy(
                            requirement => requirement.TableErpId + ":" +
                                           requirement.ReportRowNumber,
                            StringComparer.Ordinal)
                        .Select(group => group.First())
                        .ToArray();

                if (activeRequirements.Length == 0)
                    continue;

                var candidates = new List<HeuristicCandidate>();
                foreach (AuditAnalyticalMappingRequirement requirement in
                    activeRequirements)
                {
                    string targetKey = CreateTargetKey(
                        row.SyntheticAccountCode,
                        requirement.TableErpId,
                        requirement.ReportRowNumber);

                    if (!optionsByTarget.TryGetValue(
                            targetKey,
                            out AnalyticalMappingOption option))
                    {
                        continue;
                    }

                    candidates.Add(ScoreCandidate(
                        row,
                        option,
                        activeRequirements.Length,
                        accountsByCode,
                        auditorStatesBySynthetic));
                }

                if (candidates.Count == 0)
                    continue;

                List<HeuristicCandidate> ranked = candidates
                    .OrderByDescending(candidate => candidate.Score)
                    .ThenBy(candidate => candidate.Option.SortOrder)
                    .ToList();

                HeuristicCandidate best = ranked[0];
                int secondScore = ranked.Count > 1 ? ranked[1].Score : -1;
                int gap = best.Score - secondScore;

                if (ranked.Count > 1 && gap == 0)
                {
                    row.SuggestionConfidence = "Ambiguous";
                    row.SuggestionReason =
                        "Several permitted targets have the same heuristic score (" +
                        best.Score + "). Auditor review is required.";
                    continue;
                }

                row.SuggestedMapping = best.Option.DisplayCaption;
                if (best.Score >= 75 && gap >= 20)
                    row.SuggestionConfidence = "High";
                else if (best.Score >= 55 && gap >= 10)
                    row.SuggestionConfidence = "Medium";
                else
                    row.SuggestionConfidence = "Low";

                row.SuggestionReason =
                    "Score " + best.Score + ". " +
                    string.Join("; ", best.Reasons.ToArray());

                if (!string.Equals(
                        row.MappingSource,
                        AuditorSource,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        row.SuggestionConfidence,
                        "High",
                        StringComparison.Ordinal))
                {
                    row.MappedTo = row.SuggestedMapping;
                    row.MappingSource = HeuristicSource;
                }
            }
        }

        private static HeuristicCandidate ScoreCandidate(
            AnalyticalMappingRow row,
            AnalyticalMappingOption option,
            int activeCandidateCount,
            IReadOnlyDictionary<string, AccountSummary> accountsByCode,
            IReadOnlyDictionary<string, AnalyticalMappingExistingState[]>
                auditorStatesBySynthetic)
        {
            var candidate = new HeuristicCandidate
            {
                Option = option
            };

            if (activeCandidateCount == 1)
            {
                candidate.Score = 100;
                candidate.Reasons.Add(
                    "only active permitted target for the account's current balance/value source");
                return candidate;
            }

            candidate.Score = 35;
            candidate.Reasons.Add(
                "active permitted target for synthetic account " +
                row.SyntheticAccountCode);

            string accountText = row.AccountName ?? string.Empty;
            if (accountsByCode.TryGetValue(
                    row.AccountCode,
                    out AccountSummary account))
            {
                accountText = string.Join(
                    " ",
                    new[]
                    {
                        account.AccountName,
                        account.EntityAccountName,
                        account.FrameworkAccountName
                    }.Where(value => !string.IsNullOrWhiteSpace(value)));
            }

            HashSet<string> accountTokens = Tokenize(accountText);
            HashSet<string> targetTokens = Tokenize(option.DisplayCaption);
            string[] overlap = accountTokens
                .Intersect(targetTokens, StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (overlap.Length > 0)
            {
                int tokenScore = Math.Min(45, overlap.Length * 15);
                candidate.Score += tokenScore;
                candidate.Reasons.Add(
                    "name match: " + string.Join(", ", overlap));
            }

            string normalizedAccountText = NormalizePhrase(accountText);
            string normalizedTargetText = NormalizePhrase(option.DisplayCaption);
            if (!string.IsNullOrWhiteSpace(normalizedAccountText) &&
                !string.IsNullOrWhiteSpace(normalizedTargetText) &&
                (normalizedAccountText.Contains(normalizedTargetText) ||
                 normalizedTargetText.Contains(normalizedAccountText)))
            {
                candidate.Score += 20;
                candidate.Reasons.Add("strong caption/name containment");
            }

            if (auditorStatesBySynthetic.TryGetValue(
                    row.SyntheticAccountCode,
                    out AnalyticalMappingExistingState[] auditorStates))
            {
                AnalyticalMappingExistingState previous = null;
                AnalyticalMappingExistingState next = null;

                foreach (AnalyticalMappingExistingState state in auditorStates)
                {
                    int compare = string.Compare(
                        state.AccountCode,
                        row.AccountCode,
                        StringComparison.Ordinal);
                    if (compare < 0)
                        previous = state;
                    else if (compare > 0)
                    {
                        next = state;
                        break;
                    }
                }

                if (previous != null &&
                    string.Equals(
                        previous.MappedTo,
                        option.DisplayCaption,
                        StringComparison.OrdinalIgnoreCase))
                {
                    candidate.Score += 20;
                    candidate.Reasons.Add(
                        "previous auditor-mapped neighboring account " +
                        previous.AccountCode + " uses this target");
                }

                if (next != null &&
                    string.Equals(
                        next.MappedTo,
                        option.DisplayCaption,
                        StringComparison.OrdinalIgnoreCase))
                {
                    candidate.Score += 20;
                    candidate.Reasons.Add(
                        "next auditor-mapped neighboring account " +
                        next.AccountCode + " uses this target");
                }
            }

            return candidate;
        }

        private static HashSet<string> Tokenize(string value)
        {
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var current = new StringBuilder();

            Action flush = () =>
            {
                if (current.Length < 3)
                {
                    current.Clear();
                    return;
                }

                string token = current.ToString().ToLowerInvariant();
                current.Clear();
                if (!IgnoredWords.Contains(token))
                    tokens.Add(token);
            };

            foreach (char character in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(character))
                    current.Append(character);
                else
                    flush();
            }
            flush();
            return tokens;
        }

        private static string NormalizePhrase(string value)
        {
            return string.Join(
                " ",
                Tokenize(value)
                    .OrderBy(token => token, StringComparer.OrdinalIgnoreCase)
                    .ToArray());
        }

        private static string CreateTargetKey(
            string syntheticCode,
            int tableErpId,
            int reportRowNumber)
        {
            return syntheticCode + "\u001f" + tableErpId + "/" + reportRowNumber;
        }

        private static void ValidateOptionCaptions(
            IEnumerable<AnalyticalMappingOption> options)
        {
            IGrouping<string, AnalyticalMappingOption> duplicate = options
                .GroupBy(
                    option => option.SyntheticAccountCode + "\u001f" +
                              option.DisplayCaption,
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicate != null)
            {
                AnalyticalMappingOption option = duplicate.First();
                throw new InvalidOperationException(
                    "Synthetic account " + option.SyntheticAccountCode +
                    " contains duplicate mapping caption '" +
                    option.DisplayCaption + "'.");
            }
        }

        private static string BuildCaption(
            AuditTemplatePackageResponse package,
            int tableErpId,
            int rowNumber)
        {
            AuditReportTableDefinitionResponse table =
                (package.Template?.Tables ??
                 Array.Empty<AuditReportTableDefinitionResponse>())
                    .SingleOrDefault(item => item.TableErpId == tableErpId)
                ?? throw new InvalidOperationException(
                    "Mapping references unknown report table " + tableErpId + ".");

            var rowsByNumber =
                (table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                    .Where(reportRow => reportRow.RowNumber.HasValue)
                    .ToDictionary(reportRow => reportRow.RowNumber.Value);

            if (!rowsByNumber.TryGetValue(
                    rowNumber,
                    out AuditReportRowDefinitionResponse row))
            {
                throw new InvalidOperationException(
                    "Mapping references unknown report row " +
                    tableErpId + "/" + rowNumber + ".");
            }

            if (!string.IsNullOrWhiteSpace(row.MappingCaptionSk))
                return row.MappingCaptionSk.Trim();

            string leafCaption = FirstNonempty(
                row.TextSk,
                row.CategorySk,
                "Row " + rowNumber);
            return "r." + rowNumber + " - " + leafCaption;
        }

        private static string FirstNonempty(
            string first,
            string second,
            string fallback)
        {
            if (!string.IsNullOrWhiteSpace(first))
                return first.Trim();
            if (!string.IsNullOrWhiteSpace(second))
                return second.Trim();
            return fallback;
        }

        private static string CreateValidationRangeName(
            string syntheticAccountCode)
        {
            if (string.IsNullOrWhiteSpace(syntheticAccountCode))
            {
                throw new InvalidOperationException(
                    "An analytical account does not contain a synthetic account code.");
            }

            char[] safeCharacters = syntheticAccountCode
                .Select(character =>
                    char.IsLetterOrDigit(character) || character == '_'
                        ? character
                        : '_')
                .ToArray();
            return "_AU_" + new string(safeCharacters);
        }

        private sealed class HeuristicCandidate
        {
            public AnalyticalMappingOption Option { get; set; }
            public int Score { get; set; }
            public List<string> Reasons { get; } = new List<string>();
        }
    }
}
