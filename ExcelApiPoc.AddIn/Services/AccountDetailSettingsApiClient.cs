using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System;
using System.Net.Http;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountDetailSettingsApiClient
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public static AccountDetailTextSettings GetTexts() => Get<AccountDetailTextSettings>("texts");
        public static AccountDetailLayout GetLayout() => Get<AccountDetailLayout>("layout");

        public static AccountDetailText CreateText(string category, string value, int order) =>
            Send<AccountDetailText>(HttpMethod.Post, "texts", new { categoryCode = category, textSk = value, sortOrder = order });

        public static void UpdateText(string category, int id, string value, int order) =>
            Send<object>(HttpMethod.Put, "texts/" + category + "/" + id, new { textSk = value, sortOrder = order });

        public static void DeleteText(string category, int id) =>
            Send<object>(HttpMethod.Delete, "texts/" + category + "/" + id, null);

        public static void SetDefault(string account, string category, int id) =>
            Send<object>(HttpMethod.Put, "defaults/" + account + "/" + category, new { textId = id });

        public static void DeleteDefault(string account, string category) =>
            Send<object>(HttpMethod.Delete, "defaults/" + account + "/" + category, null);

        private static T Send<T>(HttpMethod method, string path, object body)
        {
            string key = SettingsService.Load().ApiKey;
            if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Enter your API key in Settings.");
            using (var request = new HttpRequestMessage(method, SettingsService.BuildApiUri("api/v1/account-detail/" + path)))
            {
                request.Headers.Add("X-Api-Key", key.Trim());
                if (body != null) request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        string detail = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        throw new InvalidOperationException($"Account detail API returned {(int)response.StatusCode} {response.ReasonPhrase}: {detail}");
                    }
                    if (response.StatusCode == HttpStatusCode.NoContent) return default(T);
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
        }

        private static T Get<T>(string path)
        {
            string key = SettingsService.Load().ApiKey;
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Enter your API key in Settings before creating an audit workbook.");

            using (var request = new HttpRequestMessage(HttpMethod.Get,
                SettingsService.BuildApiUri("api/v1/account-detail/" + path)))
            {
                request.Headers.Add("X-Api-Key", key.Trim());
                using (HttpResponseMessage response = Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    response.EnsureSuccessStatusCode();
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
        }
    }
}
