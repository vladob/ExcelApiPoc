using System;
using System.Net;
using System.Net.Http;
using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountingEntityPackageApiClient
    {
        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

        public static AccountingEntityPackageDto GetPackage(string ico)
        {
            if (string.IsNullOrWhiteSpace(ico))
            {
                throw new ArgumentException(
                    "IČO is required.",
                    nameof(ico));
            }

            string normalizedIco = ico.Trim();

            string relativePath =
                $"api/v1/accounting-entities/{Uri.EscapeDataString(normalizedIco)}/package";

            using (HttpResponseMessage response = HttpClient
                .GetAsync(SettingsService.BuildApiUri(relativePath))
                .GetAwaiter()
                .GetResult())
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new AccountingEntityPackageNotFoundException(
                        normalizedIco);
                }

                response.EnsureSuccessStatusCode();

                string json = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                AccountingEntityPackageDto package =
                    JsonConvert.DeserializeObject<AccountingEntityPackageDto>(json);

                if (package == null)
                {
                    throw new InvalidOperationException(
                        $"API returned an empty accounting-entity package for IČO {normalizedIco}.");
                }

                return package;
            }
        }
    }

    internal sealed class AccountingEntityPackageNotFoundException
        : Exception
    {
        public AccountingEntityPackageNotFoundException(string ico)
            : base($"Accounting entity {ico} was not found.")
        {
            Ico = ico;
        }

        public string Ico { get; }
    }
}