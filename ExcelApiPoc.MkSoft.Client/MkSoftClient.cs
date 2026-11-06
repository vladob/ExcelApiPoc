using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AccountBalanceVerification.Core;
namespace ExcelApiPoc.MkSoft.Client
{
    public sealed class MkSoftClient
    {
        private readonly HttpClient http;
        private readonly Uri baseUri;
        private readonly string apiKey;
        public MkSoftClient(HttpClient http, string baseUrl, string apiKey)
        {
            this.http = http ?? throw new ArgumentNullException(nameof(http));
            baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
            if (baseUri.Scheme != "https" && baseUri.Scheme != "http") throw new ArgumentException("HTTP(S) URL required.");
            this.apiKey = apiKey;
        }
        public async Task<System.Collections.Generic.List<MkSoftEntity>> FindEntitiesAsync(int fiscalYear, int? clientErpId, string ico, CancellationToken ct)
        {
            var path = "api/v1/mksoft/entities?fiscalYear=" + fiscalYear;
            if (clientErpId.HasValue) path += "&clientErpId=" + clientErpId.Value;
            if (!string.IsNullOrWhiteSpace(ico)) path += "&ico=" + Uri.EscapeDataString(ico.Trim());
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path)))
            {
                request.Headers.Add("X-Api-Key", apiKey);
                using (var response = await http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException("MkSoft API: " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return JsonConvert.DeserializeObject<System.Collections.Generic.List<MkSoftEntity>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                        ?? throw new InvalidOperationException("Empty entity response.");
                }
            }
        }
        public async Task<System.Collections.Generic.List<OfficialAccount>> GetAccountingFrameworkAsync(int clientErpId, int fiscalYear, CancellationToken ct)
        {
            var path = $"api/v1/mksoft/clients/{clientErpId}/years/{fiscalYear}/accounting-framework";
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path)))
            {
                request.Headers.Add("X-Api-Key", apiKey);
                using (var response = await http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException("MkSoft POSNOVA API: " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return JsonConvert.DeserializeObject<System.Collections.Generic.List<OfficialAccount>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                        ?? throw new InvalidOperationException("Missing POSNOVA response.");
                }
            }
        }
        public async Task ApplyFrameworkAsync(VerificationPackage package, string frameworkCode, CancellationToken ct)
        {
            var path = "api/v1/account-frameworks/" + Uri.EscapeDataString(frameworkCode)
                + "/applicable?fiscalYear=" + package.FiscalYear;
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path)))
            {
                if (!string.IsNullOrWhiteSpace(apiKey)) request.Headers.Add("X-Api-Key", apiKey);
                using (var response = await http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Account framework API: " + (int)response.StatusCode);
                    var framework = JsonConvert.DeserializeObject<FrameworkResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    if (framework == null || framework.Definitions == null) throw new InvalidOperationException("Missing account framework.");
                    package.AccountFrameworkVersionId = framework.FrameworkVersionId;
                    var fallbackNames = package.OfficialAccounts.ToArray();
                    package.OfficialAccounts.Clear();
                    foreach (var item in framework.Definitions)
                        if (WorkbookPlan.InScope(item.AccountCode)) package.OfficialAccounts.Add(new OfficialAccount {
                            AccountCode = item.AccountCode, AccountName = item.OfficialName,
                            AccountLevel = item.AccountLevel,
                            ParentAccountCode = item.AccountCode.Length > 1 ? item.AccountCode.Substring(0, item.AccountCode.Length - 1) : null
                        });
                    foreach (var fallback in fallbackNames)
                        if (!package.OfficialAccounts.Any(x => x.AccountCode == fallback.AccountCode)) package.OfficialAccounts.Add(fallback);
                }
            }
        }
        private sealed class FrameworkResponse
        {
            public int FrameworkVersionId { get; set; }
            public System.Collections.Generic.List<FrameworkDefinition> Definitions { get; set; }
        }
        private sealed class FrameworkDefinition
        {
            public int AccountLevel { get; set; }
            public string AccountCode { get; set; }
            public string OfficialName { get; set; }
        }
        public async Task<VerificationPackage> GetAsync(int clientErpId, int fiscalYear, bool includeEmptyAccounts, CancellationToken ct)
        {
            var path = $"api/v1/mksoft/clients/{clientErpId}/years/{fiscalYear}/general-ledger?includeEmptyAccounts={includeEmptyAccounts.ToString().ToLowerInvariant()}";
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path)))
            {
                request.Headers.Add("X-Api-Key", apiKey);
                using (var response = await http.SendAsync(request, ct).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException("MkSoft API: " + (int)response.StatusCode + " " + response.ReasonPhrase);
                    return JsonConvert.DeserializeObject<VerificationPackage>(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                        ?? throw new InvalidOperationException("Empty API response.");
                }
            }
        }
    }
}
