using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace ExcelApiPoc.AddIn.Forms
{
    internal sealed class SettingsForm : Form
    {
        private readonly TextBox _apiBaseUrlTextBox;
        private readonly Button _testConnectionButton;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;

        public SettingsForm()
        {
            Text = "Excel API PoC Settings";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 570;
            Height = 190;

            var apiUrlLabel = new Label
            {
                Text = "API base URL:"
            };
            apiUrlLabel.SetBounds(15, 20, 100, 23);

            _apiBaseUrlTextBox = new TextBox();
            _apiBaseUrlTextBox.SetBounds(120, 17, 420, 23);

            var settingsPathLabel = new Label
            {
                Text = $"Settings file: {SettingsService.SettingsPath}",
                AutoEllipsis = true
            };
            settingsPathLabel.SetBounds(15, 55, 525, 23);

            _testConnectionButton = new Button
            {
                Text = "Test connection"
            };
            _testConnectionButton.SetBounds(15, 95, 125, 30);
            _testConnectionButton.Click += TestConnectionButton_Click;

            _saveButton = new Button
            {
                Text = "Save"
            };
            _saveButton.SetBounds(360, 95, 85, 30);
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.SetBounds(455, 95, 85, 30);

            Controls.Add(apiUrlLabel);
            Controls.Add(_apiBaseUrlTextBox);
            Controls.Add(settingsPathLabel);
            Controls.Add(_testConnectionButton);
            Controls.Add(_saveButton);
            Controls.Add(_cancelButton);

            AcceptButton = _saveButton;
            CancelButton = _cancelButton;

            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            AddInSettings settings = SettingsService.Load();
            _apiBaseUrlTextBox.Text = settings.ApiBaseUrl;
        }

        private void TestConnectionButton_Click(object sender,EventArgs e)
        {
            try
            {
                string baseUrl = ValidateAndNormalizeUrl( _apiBaseUrlTextBox.Text);
                var healthUri = new Uri( $"{baseUrl}/api/health", UriKind.Absolute);
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string response = client.GetStringAsync(healthUri).GetAwaiter().GetResult();
                    MessageBox.Show($"Connection successful.\n\n{response}", "API Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Connection failed.\n\n{exception.Message}", "API Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender,EventArgs e)
        {
            try
            {
                string baseUrl = ValidateAndNormalizeUrl(_apiBaseUrlTextBox.Text);
                SettingsService.Save(new AddInSettings{ApiBaseUrl = baseUrl});
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message,"Invalid Settings",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }

        private static string ValidateAndNormalizeUrl(string value)
        {
            string normalized =(value ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(normalized,UriKind.Absolute,out Uri uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException("Enter a valid HTTP or HTTPS API address.");
            }
            return normalized;
        }
    }
}