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
//        private const string HealthEndpoint = "https://localhost:7238/api/health";
        private const string HealthEndpoint = "http://localhost:5080/api/health";

        private const string TemplateMetadataEndpoint = "http://localhost:5080/api/templates/690/metadata";

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

    }
}