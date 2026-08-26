using ExcelDna.Integration.CustomUI;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System.Text;
using ExcelApiPoc.AddIn.Services;
using System.Threading.Tasks;

namespace ExcelApiPoc.AddIn
{
    [ComVisible(true)]
    public class PocRibbon : ExcelRibbon
    {
        private const string HealthEndpoint = "http://localhost:5080/api/health";
        private const string TemplateMetadataEndpoint = "http://localhost:5080/api/templates/690/metadata";
        private const int PackageTemplateErpId = 690;
        private const int PackageContractVersion = 1;
        private const string TemplatePackageEndpoint = "http://localhost:5080/api/v1/templates/690/package";

        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public override string GetCustomUI(string ribbonId)
        {
            return @"
<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui'>
  <ribbon>
    <tabs>
      <tab id='tabExcelApiPoc' label='API PoC'>
        <group id='groupApiConnection' label='API Connection'>
            <button
                id='buttonCheckApi'
                label='Check API'
                size='large'
                onAction='OnCheckApi'/>
            <button
                id='buttonGetTemplate'
                label='Get Template 690'
                size='large'
                onAction='OnGetTemplate'/>
            <button
                id='buttonGetTemplatePackage'
                label='Get Package 690'
                size='large'
                onAction='OnGetTemplatePackage'/>
        </group>
      </tab>
    </tabs>
  </ribbon>
</customUI>";
        }

        public void OnCheckApi(IRibbonControl control)
        {
            _ = control;

            try
            {
                string response = HttpClient
                    .GetStringAsync(HealthEndpoint)
                    .GetAwaiter()
                    .GetResult();

                MessageBox.Show(
                    response,
                    "API Health Response",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"The API request failed.\n\n{exception.Message}",
                    "API Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnGetTemplate(IRibbonControl control)
        {
            _ = control;

            try
            {
                /*
                string json = HttpClient
                    .GetStringAsync(TemplateMetadataEndpoint)
                    .GetAwaiter()
                    .GetResult();

                string cachePath = TemplateMetadataCache.SaveMetadata(690, json);
                */

                const int templateErpId = 690;

                string json;
                string source;
                string apiFailureMessage = null;

                string cachePath =
                    TemplateMetadataCache.GetMetadataPath(templateErpId);

                try
                {
                    json = HttpClient
                        .GetStringAsync(TemplateMetadataEndpoint)
                        .GetAwaiter()
                        .GetResult();

                    cachePath =
                        TemplateMetadataCache.SaveMetadata(
                            templateErpId,
                            json);

                    source = "IIS API";
                }
                catch (Exception exception)
                    when (exception is HttpRequestException ||
                          exception is TaskCanceledException)
                {
                    apiFailureMessage = exception.Message;

                    json =
                        TemplateMetadataCache.LoadMetadata(
                            templateErpId);

                    source = "Local cache";
                }


                AuditTemplateMetadataResponse template =
                    JsonConvert.DeserializeObject<AuditTemplateMetadataResponse>(json);

                if (template == null)
                {
                    throw new InvalidOperationException(
                        "The API returned an empty template response.");
                }

                var message = new StringBuilder();

                message.AppendLine($"Template ERP ID: {template.TemplateErpId}");
                message.AppendLine($"Name: {template.Name}");
                message.AppendLine($"MF specification: {template.MfSpecification}");
                message.AppendLine(
                    $"Valid from: {template.ValidFrom?.ToString("yyyy-MM-dd") ?? "-"}");
                message.AppendLine(
                    $"Valid to: {template.ValidTo?.ToString("yyyy-MM-dd") ?? "-"}");
                message.AppendLine();
                message.AppendLine("Tables:");

                AuditTableMetadataResponse[] tables =
                    template.Tables ?? Array.Empty<AuditTableMetadataResponse>();

                foreach (AuditTableMetadataResponse table in tables)
                {
                    message.AppendLine(
                        $"{table.TableErpId} – {table.NameSk} " +
                        $"({table.NumberOfColumns} columns, " +
                        $"{table.NumberOfDataColumns} data columns)");
                }

                message.AppendLine();
                //message.AppendLine($"Cached at: {cachePath}");

                message.AppendLine();
                message.AppendLine($"Source: {source}");
                message.AppendLine($"Cache: {cachePath}");

                if (!string.IsNullOrWhiteSpace(apiFailureMessage))
                {
                    message.AppendLine();
                    message.AppendLine(
                        $"API unavailable: {apiFailureMessage}");
                }

                MessageBox.Show(
                    message.ToString(),
                    "Audit Template Metadata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"Loading the audit template failed...\n\n{exception.Message}",
                    "Audit Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void OnGetTemplatePackage(IRibbonControl control)
        {
            _ = control;

            try
            {
                string json;
                string source;
                string apiFailureMessage = null;
                bool downloadedFromApi = false;

                string cachePath =
                    TemplateMetadataCache.GetPackagePath(
                        PackageTemplateErpId,
                        PackageContractVersion);

                try
                {
                    json = HttpClient
                        .GetStringAsync(TemplatePackageEndpoint)
                        .GetAwaiter()
                        .GetResult();

                    source = "IIS API";
                    downloadedFromApi = true;
                }
                catch (Exception exception)
                    when (exception is HttpRequestException ||
                          exception is TaskCanceledException)
                {
                    apiFailureMessage = exception.Message;

                    json =
                        TemplateMetadataCache.LoadPackage(
                            PackageTemplateErpId,
                            PackageContractVersion);

                    source = "Local cache";
                }

                AuditTemplatePackageResponse package =
                    JsonConvert.DeserializeObject<
                        AuditTemplatePackageResponse>(json);

                if (package == null)
                {
                    throw new InvalidOperationException(
                        "The API returned an empty template package.");
                }

                if (package.ContractVersion != PackageContractVersion)
                {
                    throw new InvalidOperationException(
                        $"Unsupported package contract version " +
                        $"{package.ContractVersion}.");
                }

                if (package.Template == null)
                {
                    throw new InvalidOperationException(
                        "The package does not contain a template.");
                }

                if (package.Template.TemplateErpId != PackageTemplateErpId)
                {
                    throw new InvalidOperationException(
                        $"Expected template {PackageTemplateErpId}, " +
                        $"but received {package.Template.TemplateErpId}.");
                }

                if (downloadedFromApi)
                {
                    cachePath =
                        TemplateMetadataCache.SavePackage(
                            PackageTemplateErpId,
                            PackageContractVersion,
                            json);
                }

                AuditReportTableDefinitionResponse[] tables =
                    package.Template.Tables
                    ?? Array.Empty<AuditReportTableDefinitionResponse>();

                AuditAccountGroupDefinitionResponse[] accountGroups =
                    package.AccountGroups
                    ?? Array.Empty<AuditAccountGroupDefinitionResponse>();

                AuditReportMappingRuleDefinitionResponse[] mappingRules =
                    package.ReportMappingRules
                    ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>();

                int analyticalRuleCount = 0;
                int assetsRuleCount = 0;
                int liabilitiesRuleCount = 0;

                foreach (AuditReportMappingRuleDefinitionResponse rule in mappingRules)
                {
                    if (rule.RequiresAnalyticalMapping)
                    {
                        analyticalRuleCount++;
                    }

                    if (string.Equals(
                            rule.Side,
                            "Assets",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        assetsRuleCount++;
                    }
                    else if (string.Equals(
                                 rule.Side,
                                 "Liabilities",
                                 StringComparison.OrdinalIgnoreCase))
                    {
                        liabilitiesRuleCount++;
                    }
                }

                AuditCalculationDependencyDefinitionResponse[] calculationPlan =
                    package.CalculationPlan
                    ?? Array.Empty<AuditCalculationDependencyDefinitionResponse>();

                int additionCount = 0;
                int subtractionCount = 0;
                int crossTableCount = 0;

                foreach (
                    AuditCalculationDependencyDefinitionResponse dependency
                    in calculationPlan)
                {
                    if (dependency.Coefficient == 1)
                    {
                        additionCount++;
                    }
                    else if (dependency.Coefficient == -1)
                    {
                        subtractionCount++;
                    }

                    if (dependency.TargetTableErpId !=
                        dependency.SourceTableErpId)
                    {
                        crossTableCount++;
                    }
                }

                int totalHeaders = 0;
                int totalRows = 0;

                var message = new StringBuilder();

                message.AppendLine(
                    $"Contract version: {package.ContractVersion}");

                message.AppendLine(
                    $"Generated UTC: " +
                    $"{package.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}");

                message.AppendLine(
                    $"Template ERP ID: {package.Template.TemplateErpId}");

                message.AppendLine(
                    $"Name: {package.Template.Name}");

                message.AppendLine(
                    $"Accounting model: {package.Template.AccountingModel}");

                message.AppendLine();
                message.AppendLine("Tables:");

                foreach (AuditReportTableDefinitionResponse table in tables)
                {
                    int headerCount =
                        table.Headers?.Length ?? 0;

                    int rowCount =
                        table.Rows?.Length ?? 0;

                    totalHeaders += headerCount;
                    totalRows += rowCount;

                    message.AppendLine(
                        $"{table.TableErpId} – {table.NameSk}: " +
                        $"{headerCount} headers, {rowCount} rows");
                }

                message.AppendLine();
                message.AppendLine(
                    $"Totals: {tables.Length} tables, " +
                    $"{totalHeaders} headers, {totalRows} rows");

                message.AppendLine();
                message.AppendLine(
                    $"Account groups: {accountGroups.Length}");

                message.AppendLine(
                    $"Report mapping rules: {mappingRules.Length}");

                message.AppendLine(
                    $"Analytical mapping rules: {analyticalRuleCount}");

                message.AppendLine(
                    $"Rule sides: {assetsRuleCount} Assets, " +
                    $"{liabilitiesRuleCount} Liabilities");

                message.AppendLine();
                message.AppendLine(
                    $"Calculation dependencies: {calculationPlan.Length}");

                message.AppendLine(
                    $"Operations: {additionCount} additions, " +
                    $"{subtractionCount} subtractions");

                message.AppendLine(
                    $"Cross-table dependencies: {crossTableCount}");

                message.AppendLine();
                message.AppendLine($"Source: {source}");
                message.AppendLine($"Cache: {cachePath}");

                if (!string.IsNullOrWhiteSpace(apiFailureMessage))
                {
                    message.AppendLine();
                    message.AppendLine(
                        $"API unavailable: {apiFailureMessage}");
                }

                MessageBox.Show(
                    message.ToString(),
                    "Audit Template Package",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"Loading the audit-template package failed." +
                    $"\n\n{exception.Message}",
                    "Audit Template Package Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}