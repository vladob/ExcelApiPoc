using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditCalculationPackageSelectionService
    {
        private const int ContractVersion = 1;

        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(120)
            };

        public static AuditCalculationPackageSelectionResponse Load(
            string ico,
            int fiscalYear)
        {
            if (string.IsNullOrWhiteSpace(ico))
            {
                throw new ArgumentException(
                    "IČO is required.",
                    nameof(ico));
            }

            string normalizedIco = ico.Trim();
            string relativePath =
                $"api/v1/accounting-entities/" +
                $"{Uri.EscapeDataString(normalizedIco)}/calculation-package" +
                $"?fiscalYear={fiscalYear}";

            using (HttpResponseMessage response = HttpClient
                .GetAsync(SettingsService.BuildApiUri(relativePath))
                .GetAwaiter()
                .GetResult())
            {
                string json = response.Content
                    .ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"Calculation-package API returned " +
                        $"{(int)response.StatusCode}: " +
                        ReadApiMessage(json, response.ReasonPhrase));
                }

                AuditCalculationPackageSelectionResponse selection =
                    JsonConvert.DeserializeObject
                        <AuditCalculationPackageSelectionResponse>(json);

                Validate(selection, normalizedIco, fiscalYear);
                return selection;
            }
        }

        private static void Validate(
            AuditCalculationPackageSelectionResponse selection,
            string expectedIco,
            int expectedFiscalYear)
        {
            if (selection == null)
            {
                throw new InvalidOperationException(
                    "The API returned an empty calculation-package selection.");
            }

            if (selection.ContractVersion != ContractVersion)
            {
                throw new InvalidOperationException(
                    $"Unsupported calculation-package selection contract " +
                    $"version {selection.ContractVersion}.");
            }

            if (!string.Equals(
                    selection.Ico,
                    expectedIco,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected calculation package for IČO '{expectedIco}', " +
                    $"but received '{selection.Ico}'.");
            }

            if (selection.FiscalYear != expectedFiscalYear)
            {
                throw new InvalidOperationException(
                    $"Expected calculation package for fiscal year " +
                    $"{expectedFiscalYear}, but received {selection.FiscalYear}.");
            }

            if (selection.FinancialReportId <= 0 ||
                selection.RegisterUzTemplateId <= 0)
            {
                throw new InvalidOperationException(
                    "The calculation-package selection does not identify " +
                    "a financial report and template.");
            }

            if (selection.CalculationPackage == null ||
                string.IsNullOrWhiteSpace(
                    selection.CalculationPackage.FrameworkCode))
            {
                throw new InvalidOperationException(
                    "The calculation-package selection does not contain " +
                    "an accounting framework.");
            }

            if (selection.CalculationPackage.Template == null ||
                selection.CalculationPackage.Template.TemplateErpId !=
                    selection.RegisterUzTemplateId)
            {
                throw new InvalidOperationException(
                    "The selected RegisterUZ template and calculation package " +
                    "are inconsistent.");
            }
        }

        private static string ReadApiMessage(
            string json,
            string fallback)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    string message =
                        JObject.Parse(json).Value<string>("message");

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                }
                catch (JsonException)
                {
                }
            }

            return string.IsNullOrWhiteSpace(fallback)
                ? "No error details were returned."
                : fallback;
        }
    }
}
