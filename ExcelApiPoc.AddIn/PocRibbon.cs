using ExcelDna.Integration.CustomUI;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System.Text;

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
                string json = HttpClient
                    .GetStringAsync(TemplateMetadataEndpoint)
                    .GetAwaiter()
                    .GetResult();

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

                MessageBox.Show(
                    message.ToString(),
                    "Audit Template Metadata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"Downloading the audit template failed.\n\n{exception.Message}",
                    "Audit Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}