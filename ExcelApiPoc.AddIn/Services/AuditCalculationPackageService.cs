using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditCalculationPackageService
    {
        private const int MaximumErrorBodyBytes = 16 * 1024;
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public static AuditCalculationPackageResponse Load(string ico, int fiscalYear)
        {
            string relativePath = $"api/v1/accounting-entities/{Uri.EscapeDataString(ico)}/calculation-package" +
                $"?fiscalYear={fiscalYear}";
            using (HttpResponseMessage response = HttpClient.GetAsync(
                       SettingsService.BuildApiUri(relativePath)).GetAwaiter().GetResult())
            {
                string json = ReadBounded(response.Content);
                if (!response.IsSuccessStatusCode)
                {
                    string message = null;
                    try { message = JObject.Parse(json ?? string.Empty).Value<string>("message"); }
                    catch (JsonException) { }
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                        ? $"Calculation-package API returned {(int)response.StatusCode} {response.ReasonPhrase}."
                        : message);
                }

                AuditCalculationPackageResponse result =
                    JsonConvert.DeserializeObject<AuditCalculationPackageResponse>(json) ??
                    throw new InvalidOperationException("The API returned an empty calculation package.");
                AuditCalculationPackageValidator.Validate(result, fiscalYear);
                return result;
            }
        }

        private static string ReadBounded(HttpContent content)
        {
            if (content == null || content.Headers.ContentLength > MaximumErrorBodyBytes)
                return null;
            using (Stream stream = content.ReadAsStreamAsync().GetAwaiter().GetResult())
            {
                var buffer = new byte[MaximumErrorBodyBytes + 1];
                int count = 0;
                while (count < buffer.Length)
                {
                    int read = stream.Read(buffer, count, buffer.Length - count);
                    if (read == 0)
                        break;
                    count += read;
                }
                return count > MaximumErrorBodyBytes ? null :
                    new UTF8Encoding(false, true).GetString(buffer, 0, count);
            }
        }
    }
}
