using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountFrameworkService
    {
        private const int ContractVersion = 1;
        private static readonly HttpClient HttpClient = new HttpClient{Timeout = TimeSpan.FromSeconds(10)};

        public static AccountFrameworkLoadResult Load(string frameworkCode, int fiscalYear)
        {
            string json;
            string source;
            string apiFailureMessage = null;
            bool downloadedFromApi = false;
            string cachePath = AccountFrameworkCache.GetPath(frameworkCode, fiscalYear, ContractVersion);
            string relativePath = $"api/v1/account-frameworks/" + $"{Uri.EscapeDataString(frameworkCode)}/applicable" + $"?fiscalYear={fiscalYear}";

            try
            {
                json = HttpClient.GetStringAsync(SettingsService.BuildApiUri(relativePath)).GetAwaiter().GetResult();
                source = "IIS API";
                downloadedFromApi = true;
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException)
            {
                apiFailureMessage = exception.Message;
                json = AccountFrameworkCache.Load(frameworkCode, fiscalYear, ContractVersion);
                source = "Local cache";
            }

            ApplicableAccountFrameworkResponse framework = JsonConvert.DeserializeObject<ApplicableAccountFrameworkResponse>(json) ?? throw new InvalidOperationException("The API returned an empty account framework.");
            Validate(framework,frameworkCode,fiscalYear);

            if (downloadedFromApi)
            {
                cachePath = AccountFrameworkCache.Save(frameworkCode, fiscalYear, ContractVersion, json);
            }

            return new AccountFrameworkLoadResult
            {
                Framework = framework,
                Source = source,
                CachePath = cachePath,
                ApiFailureMessage = apiFailureMessage
            };
        }

        private static void Validate(ApplicableAccountFrameworkResponse framework, string expectedFrameworkCode, int fiscalYear)
        {
            if (!string.Equals(framework.FrameworkCode, expectedFrameworkCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected framework '{expectedFrameworkCode}', " + $"but received '{framework.FrameworkCode}'.");
            }

            DateTime applicableDate = new DateTime(fiscalYear, 12, 31);
            if (framework.ValidFrom.Date > applicableDate || framework.ValidTo.HasValue && framework.ValidTo.Value.Date < applicableDate)
            {
                throw new InvalidOperationException($"Framework version '{framework.VersionCode}' " + $"is not applicable on {applicableDate:yyyy-MM-dd}.");
            }

            AccountDefinitionMetadataResponse[] definitions = framework.Definitions ?? Array.Empty<AccountDefinitionMetadataResponse>();
            if (definitions.Length == 0)
            {
                throw new InvalidOperationException( "The framework does not contain any definitions.");
            }

            var definitionKeys = new HashSet<string>(StringComparer.Ordinal);
            int syntheticAccountCount = 0;

            foreach (AccountDefinitionMetadataResponse definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace( definition.AccountCode) || string.IsNullOrWhiteSpace( definition.OfficialName))
                {
                    throw new InvalidOperationException("The framework contains an incomplete definition.");
                }

                string definitionKey = $"{definition.AccountLevel}:" + $"{definition.AccountCode}";

                if (!definitionKeys.Add(definitionKey))
                {
                    throw new InvalidOperationException($"Duplicate framework definition " + $"'{definitionKey}'.");
                }

                if (definition.AccountLevel == 3)
                    syntheticAccountCount++;
            }

            if (syntheticAccountCount == 0)
            {
                throw new InvalidOperationException("The framework does not contain synthetic accounts.");
            }
        }
    }

    internal sealed class AccountFrameworkLoadResult
    {
        public ApplicableAccountFrameworkResponse Framework{get; set;}
        public string Source { get; set; }
        public string CachePath { get; set; }
        public string ApiFailureMessage { get; set; }
    }
}