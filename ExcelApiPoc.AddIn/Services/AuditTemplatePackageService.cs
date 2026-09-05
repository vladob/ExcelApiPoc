using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditTemplatePackageService
    {
        private const int ContractVersion = 4;
        private const int MaximumErrorBodyBytes = 16 * 1024;
        private const int MaximumDisplayedMessageCharacters = 1000;
        private static readonly HttpClient HttpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};

        public static AuditTemplatePackageLoadResult Load(AuditReportContext reportContext)
        {
            if (reportContext == null)
                throw new ArgumentNullException(nameof(reportContext));

            string json;
            string source;
            string apiFailureMessage = null;
            bool downloadedFromApi = false;

            string cachePath = TemplateMetadataCache.GetPackageV4Path(
                reportContext.TemplateErpId, reportContext.FrameworkCode, reportContext.FiscalYear);
            string relativePath = $"api/v2/templates/{reportContext.TemplateErpId}/package" +
                $"?frameworkCode={Uri.EscapeDataString(reportContext.FrameworkCode)}" +
                $"&fiscalYear={reportContext.FiscalYear}";

            try
            {
                using (HttpResponseMessage response = HttpClient
                    .GetAsync(SettingsService.BuildApiUri(relativePath))
                    .GetAwaiter()
                    .GetResult())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        source = "API";
                        downloadedFromApi = true;
                    }
                    else
                    {
                        string errorBody = ReadBoundedErrorBody(response.Content);
                        string safeMessage = ReadSafeApiMessage(errorBody, response.ReasonPhrase);

                        if (response.StatusCode == HttpStatusCode.BadRequest ||
                            response.StatusCode == HttpStatusCode.NotFound ||
                            response.StatusCode == HttpStatusCode.Conflict)
                        {
                            throw new InvalidOperationException(
                                $"Template-package API returned {(int)response.StatusCode}: {safeMessage}");
                        }

                        if ((int)response.StatusCode >= 500)
                        {
                            apiFailureMessage =
                                $"Template-package API returned {(int)response.StatusCode}: {safeMessage}";
                            json = LoadValidatedCache(reportContext, apiFailureMessage);
                            source = "Local cache";
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Template-package API returned unexpected status {(int)response.StatusCode}: {safeMessage}");
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException)
            {
                apiFailureMessage = exception.Message;
                json = LoadValidatedCache(reportContext, apiFailureMessage);
                source = "Local cache";
            }

            AuditTemplatePackageResponse package = JsonConvert.DeserializeObject<AuditTemplatePackageResponse>(json) ?? throw new InvalidOperationException( "The API returned an empty template package.");
            Validate(package,reportContext);

            if (downloadedFromApi)
            {
                cachePath = TemplateMetadataCache.SavePackageV4(
                    reportContext.TemplateErpId, reportContext.FrameworkCode, reportContext.FiscalYear, json);
            }

            return new AuditTemplatePackageLoadResult
            {
                Package = package,
                Source = source,
                CachePath = cachePath,
                ApiFailureMessage = apiFailureMessage
            };
        }

        private static void Validate(AuditTemplatePackageResponse package,AuditReportContext reportContext)
        {
            if (package.ContractVersion != ContractVersion)
            {
                throw new InvalidOperationException($"Unsupported template-package contract version " + $"{package.ContractVersion}.");
            }

            if (package.Template == null)
            {
                throw new InvalidOperationException("The package does not contain a template.");
            }

            if (package.Template.TemplateErpId != reportContext.TemplateErpId)
            {
                throw new InvalidOperationException($"Expected template " + $"{reportContext.TemplateErpId}, but received " + $"{package.Template.TemplateErpId}.");
            }

            if (!string.Equals(package.FrameworkCode, reportContext.FrameworkCode, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Expected framework '{reportContext.FrameworkCode}', but received '{package.FrameworkCode}'.");
            }

            if (string.IsNullOrWhiteSpace(package.FrameworkVersionCode))
                throw new InvalidOperationException("The template package does not contain a framework version code.");

            if (string.IsNullOrWhiteSpace(package.CalculationConfigurationCode))
                throw new InvalidOperationException("The template package does not contain a calculation configuration code.");

            DateTime applicableDate = new DateTime(reportContext.FiscalYear, 12, 31);
            if (!package.ApplicableDate.HasValue ||
                package.ApplicableDate.Value.Date != applicableDate ||
                package.ApplicableDate.Value.TimeOfDay != TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    $"Expected template-package applicable date {applicableDate:yyyy-MM-dd} with no time component, but received " +
                    $"{(package.ApplicableDate.HasValue ? package.ApplicableDate.Value.ToString("O") : "no value")}.");
            }
            if (package.Template.ValidFrom.HasValue && package.Template.ValidFrom.Value.Date >applicableDate)
            {
                throw new InvalidOperationException($"Template {reportContext.TemplateErpId} " + $"is not yet valid for fiscal year " + $"{reportContext.FiscalYear}.");
            }

            if (package.Template.ValidTo.HasValue && package.Template.ValidTo.Value.Date < applicableDate)
            {
                throw new InvalidOperationException($"Template {reportContext.TemplateErpId} " + $"is no longer valid for fiscal year " + $"{reportContext.FiscalYear}.");
            }

            AuditReportTableDefinitionResponse[] tables = package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>();
            if (tables.Length == 0)
            {
                throw new InvalidOperationException("The template package does not contain any tables.");
            }

            if (package.ReportMappingRules == null)
            {
                throw new InvalidOperationException("The template package does not contain " + "report-mapping rules.");
            }

            if (package.CalculationPlan == null)
            {
                throw new InvalidOperationException("The template package does not contain " + "a calculation plan.");
            }
        }

        private static string LoadValidatedCache(AuditReportContext reportContext, string apiFailureMessage)
        {
            try
            {
                string json = TemplateMetadataCache.LoadPackageV4(
                    reportContext.TemplateErpId, reportContext.FrameworkCode, reportContext.FiscalYear);
                AuditTemplatePackageResponse package =
                    JsonConvert.DeserializeObject<AuditTemplatePackageResponse>(json) ??
                    throw new InvalidOperationException("The cached template package is empty.");
                Validate(package, reportContext);
                return json;
            }
            catch (Exception cacheException)
            {
                throw new InvalidOperationException(
                    $"The template-package API failed ({BoundMessage(apiFailureMessage)}) and no valid exact cache is available: " +
                    BoundMessage(cacheException.Message),
                    cacheException);
            }
        }

        private static string ReadBoundedErrorBody(HttpContent content)
        {
            if (content == null ||
                content.Headers.ContentLength.HasValue &&
                content.Headers.ContentLength.Value > MaximumErrorBodyBytes)
            {
                return null;
            }

            using (Stream stream = content.ReadAsStreamAsync().GetAwaiter().GetResult())
            {
                var buffer = new byte[MaximumErrorBodyBytes + 1];
                int totalBytesRead = 0;

                while (totalBytesRead < buffer.Length)
                {
                    int bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
                    if (bytesRead == 0)
                        break;
                    totalBytesRead += bytesRead;
                }

                if (totalBytesRead > MaximumErrorBodyBytes)
                    return null;

                try
                {
                    return new UTF8Encoding(false, true).GetString(buffer, 0, totalBytesRead);
                }
                catch (DecoderFallbackException)
                {
                    return null;
                }
            }
        }

        private static string ReadSafeApiMessage(string json, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    string message = JObject.Parse(json).Value<string>("message");
                    if (!string.IsNullOrWhiteSpace(message) &&
                        message.IndexOf('<') < 0 &&
                        message.IndexOf('>') < 0)
                        return BoundMessage(message);
                }
                catch (JsonException)
                {
                }
            }

            return string.IsNullOrWhiteSpace(fallback)
                ? "No error details were returned."
                : BoundMessage(fallback);
        }

        private static string BoundMessage(string message)
        {
            string normalized = (message ?? string.Empty).Trim();
            return normalized.Length <= MaximumDisplayedMessageCharacters
                ? normalized
                : normalized.Substring(0, MaximumDisplayedMessageCharacters - 1) + "…";
        }
    }

    internal sealed class AuditTemplatePackageLoadResult
    {
        public AuditTemplatePackageResponse Package { get; set; }
        public string Source { get; set; }
        public string CachePath { get; set; }
        public string ApiFailureMessage { get; set; }
    }
}
