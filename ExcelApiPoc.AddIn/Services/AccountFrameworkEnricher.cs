using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountFrameworkEnricher
    {
        public static AccountFrameworkEnrichmentResult Enrich(IReadOnlyList<AccountSummary> accounts, ApplicableAccountFrameworkResponse framework)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (framework == null)
                throw new ArgumentNullException(nameof(framework));

            var syntheticDefinitions = CreateSyntheticDefinitionIndex(framework);
            AccountRangeMetadataResponse[] ranges = framework.Ranges ?? Array.Empty<AccountRangeMetadataResponse>();
            var result = new AccountFrameworkEnrichmentResult();

            foreach (AccountSummary account in accounts)
            {
                ResetFrameworkData(account);
                string syntheticCode = account.SyntheticAccountCode;

                if (string.IsNullOrWhiteSpace(syntheticCode))
                {
                    account.IsFrameworkMatch = false;
                    result.InvalidSyntheticCodeCount++;
                    result.UnmatchedCount++;
                    continue;
                }

                if (syntheticDefinitions.TryGetValue( syntheticCode, out AccountDefinitionMetadataResponse definition))
                {
                    account.FrameworkAccountCode = definition.AccountCode;
                    account.FrameworkAccountName = definition.OfficialName;
                    account.AccountName = definition.OfficialName;
                    account.AccountNameSource = "StatutoryFramework";
                    account.IsFrameworkMatch = true;
                    result.ExactMatchCount++;
                    continue;
                }

                AccountRangeMetadataResponse matchingRange = FindMatchingRange(syntheticCode, ranges);

                if (matchingRange != null)
                {
                    account.FrameworkAccountCode = $"{matchingRange.FromAccountCode}-" + $"{matchingRange.ToAccountCode}";
                    account.FrameworkAccountName = matchingRange.OfficialName;
                    account.AccountName = matchingRange.OfficialName;
                    account.AccountNameSource = "StatutoryFramework";
                    account.IsFrameworkMatch = true;
                    result.RangeMatchCount++;
                    continue;
                }

                account.IsFrameworkMatch = false;
                result.UnmatchedCount++;
            }

            return result;
        }

        private static Dictionary <string, AccountDefinitionMetadataResponse> CreateSyntheticDefinitionIndex(ApplicableAccountFrameworkResponse framework)
        {
            var definitions = new Dictionary <string, AccountDefinitionMetadataResponse>( StringComparer.Ordinal);
            AccountDefinitionMetadataResponse[] sourceDefinitions = framework.Definitions ?? Array.Empty<AccountDefinitionMetadataResponse>();

            foreach (AccountDefinitionMetadataResponse definition in sourceDefinitions)
            {
                if (definition.AccountLevel != 3)
                    continue;
                definitions.Add( definition.AccountCode, definition);
            }

            return definitions;
        }

        private static AccountRangeMetadataResponse FindMatchingRange(string syntheticAccountCode, IEnumerable<AccountRangeMetadataResponse> ranges)
        {
            if (syntheticAccountCode.Length < 2)
                return null;

            string accountGroupCode = syntheticAccountCode.Substring(0, 2);
            foreach (AccountRangeMetadataResponse range in ranges)
            {
                if (range == null || range.AccountLevel != 2 || string.IsNullOrWhiteSpace( range.FromAccountCode) || string.IsNullOrWhiteSpace( range.ToAccountCode))
                {
                    continue;
                }

                if (string.CompareOrdinal(accountGroupCode, range.FromAccountCode) >= 0 && string.CompareOrdinal( accountGroupCode, range.ToAccountCode) <= 0)
                {
                    return range;
                }
            }
            return null;
        }

        private static void ResetFrameworkData(AccountSummary account)
        {
            account.FrameworkAccountCode = string.Empty;
            account.FrameworkAccountName = string.Empty;
            account.AccountName = string.Empty;
            account.AccountNameSource = "Synthetic";
            account.IsFrameworkMatch = null;
        }
    }

    internal sealed class AccountFrameworkEnrichmentResult
    {
        public int ExactMatchCount { get; set; }
        public int RangeMatchCount { get; set; }
        public int UnmatchedCount { get; set; }
        public int InvalidSyntheticCodeCount { get; set; }
        public int MatchedCount => ExactMatchCount + RangeMatchCount;
    }
}
