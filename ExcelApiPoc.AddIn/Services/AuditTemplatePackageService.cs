using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditTemplatePackageService
    {
        private const int ContractVersion = 2;
        private static readonly HttpClient HttpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};

        public static AuditTemplatePackageLoadResult Load(AuditReportContext reportContext)
        {
            if (reportContext == null)
                throw new ArgumentNullException(nameof(reportContext));

            string json;
            string source;
            string apiFailureMessage = null;
            bool downloadedFromApi = false;

            string cachePath = TemplateMetadataCache.GetPackagePath( reportContext.TemplateErpId, ContractVersion);
            string relativePath = $"api/v1/templates/" + $"{reportContext.TemplateErpId}/package";

            try
            {
                json = HttpClient.GetStringAsync(SettingsService.BuildApiUri(relativePath)).GetAwaiter().GetResult();
                source = "API";
                downloadedFromApi = true;
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is TaskCanceledException)
            {
                apiFailureMessage = exception.Message;
                json = TemplateMetadataCache.LoadPackage(reportContext.TemplateErpId, ContractVersion);
                source = "Local cache";
            }

            AuditTemplatePackageResponse package = JsonConvert.DeserializeObject<AuditTemplatePackageResponse>(json) ?? throw new InvalidOperationException( "The API returned an empty template package.");
            Validate(package,reportContext);

            if (downloadedFromApi)
            {
                cachePath = TemplateMetadataCache.SavePackage(reportContext.TemplateErpId, ContractVersion, json);
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

            DateTime applicableDate = new DateTime(reportContext.FiscalYear, 12, 31);
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
    }

    internal sealed class AuditTemplatePackageLoadResult
    {
        public AuditTemplatePackageResponse Package { get; set; }
        public string Source { get; set; }
        public string CachePath { get; set; }
        public string ApiFailureMessage { get; set; }
    }
}