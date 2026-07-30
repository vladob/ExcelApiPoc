using ExcelDna.Integration.CustomUI;
using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ExcelApiPoc.AddIn
{
    [ComVisible(true)]
    public class PocRibbon : ExcelRibbon
    {
//        private const string HealthEndpoint = "https://localhost:7238/api/health";
        private const string HealthEndpoint = "http://localhost:5080/api/health";

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
    }
}