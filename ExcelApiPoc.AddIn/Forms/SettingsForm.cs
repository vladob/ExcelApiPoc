using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using System;
using System.Net.Http;
using System.Windows.Forms;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Forms
{
    internal sealed class SettingsForm : Form
    {
        private readonly Label _apiUrlLabel;
        private readonly Label _uiLanguageLabel;
        private readonly Label _settingsPathLabel;
        private readonly TextBox _apiBaseUrlTextBox;
        private readonly TextBox _apiKeyTextBox;
        private readonly Label _apiKeyLabel;
        private readonly ComboBox _uiLanguageComboBox;
        private readonly CheckBox _roundWholeEurosCheckBox;
        private readonly Button _testConnectionButton;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;
        private readonly Button _editTextsButton;
        private readonly AddInSettings _settings;

        public SettingsForm()
        {
            _settings = SettingsService.Load();
            string language = _settings.UiLanguage;

            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 590;
            Height = 335;

            _apiUrlLabel = new Label();
            _apiUrlLabel.SetBounds(15, 20, 120, 23);

            _apiBaseUrlTextBox = new TextBox();
            _apiBaseUrlTextBox.SetBounds(145, 17, 415, 23);
            _apiKeyLabel = new Label();
            _apiKeyLabel.SetBounds(15, 55, 120, 23);
            _apiKeyTextBox = new TextBox { UseSystemPasswordChar = true };
            _apiKeyTextBox.SetBounds(145, 52, 415, 23);

            _uiLanguageLabel = new Label();
            _uiLanguageLabel.SetBounds(15, 93, 120, 23);

            _uiLanguageComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _uiLanguageComboBox.SetBounds(145, 90, 210, 25);
            _uiLanguageComboBox.Items.Add(new UiLanguageItem("English", SettingsService.DefaultUiLanguage));
            _uiLanguageComboBox.Items.Add(new UiLanguageItem("Slovenčina", SettingsService.SlovakUiLanguage));
            _uiLanguageComboBox.SelectedIndexChanged += UiLanguageComboBox_SelectedIndexChanged;

            _roundWholeEurosCheckBox = new CheckBox
            {
                AutoSize = true,
                Checked = _settings.RoundCalculatedAmountsToWholeEuros
            };
            _roundWholeEurosCheckBox.SetBounds(145, 127, 400, 23);

            _settingsPathLabel = new Label
            {
                AutoEllipsis = true
            };
            _settingsPathLabel.SetBounds(15, 160, 545, 23);

            _testConnectionButton = new Button();
            _testConnectionButton.SetBounds(15, 205, 145, 30);
            _testConnectionButton.Click += TestConnectionButton_Click;

            _editTextsButton = new Button();
            _editTextsButton.SetBounds(165, 205, 200, 30);
            _editTextsButton.Click += (sender, args) =>
            {
                Excel.Application application = (Excel.Application)ExcelDnaUtil.Application;
                Excel.Workbook workbook = application.ActiveWorkbook;
                if (workbook == null || !AccountDetailSnapshot.Exists(workbook))
                {
                    MessageBox.Show(this, "Open an audit workbook with account detail settings first.",
                        "Predefined texts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try { AccountDetailSnapshot.EnsureCurrentAuditor(workbook); }
                catch (Exception exception)
                {
                    MessageBox.Show(this, exception.Message, "Predefined texts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var editor = new AccountDetailTextsForm(workbook)) editor.ShowDialog(this);
            };

            _saveButton = new Button();
            _saveButton.SetBounds(380, 205, 85, 30);
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.SetBounds(475, 205, 85, 30);

            Controls.Add(_apiUrlLabel);
            Controls.Add(_apiBaseUrlTextBox);
            Controls.Add(_apiKeyLabel);
            Controls.Add(_apiKeyTextBox);
            Controls.Add(_uiLanguageLabel);
            Controls.Add(_uiLanguageComboBox);
            Controls.Add(_roundWholeEurosCheckBox);
            Controls.Add(_settingsPathLabel);
            Controls.Add(_testConnectionButton);
            Controls.Add(_editTextsButton);
            Controls.Add(_saveButton);
            Controls.Add(_cancelButton);

            AcceptButton = _saveButton;
            CancelButton = _cancelButton;

            _apiBaseUrlTextBox.Text = _settings.ApiBaseUrl;
            _apiKeyTextBox.Text = _settings.ApiKey;
            SelectLanguage(_settings.UiLanguage);
            ApplyLanguage(language);
        }

        private string SelectedLanguage
        {
            get
            {
                var selected = _uiLanguageComboBox.SelectedItem as UiLanguageItem;
                return selected?.Code ?? SettingsService.DefaultUiLanguage;
            }
        }

        private void SelectLanguage(string language)
        {
            string normalized = SettingsService.NormalizeUiLanguage(language);

            for (int index = 0; index < _uiLanguageComboBox.Items.Count; index++)
            {
                if (_uiLanguageComboBox.Items[index] is UiLanguageItem item && string.Equals(item.Code, normalized, StringComparison.Ordinal))
                {
                    _uiLanguageComboBox.SelectedIndex = index;
                    return;
                }
            }
            _uiLanguageComboBox.SelectedIndex = 0;
        }

        private void UiLanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyLanguage(SelectedLanguage);
        }

        private void ApplyLanguage(string language)
        {
            Text = UiText.Get("Settings.Title", language);
            _apiUrlLabel.Text = UiText.Get("Settings.ApiBaseUrl", language);
            _apiKeyLabel.Text = language == SettingsService.SlovakUiLanguage ? "API kľúč:" : "API key:";
            _uiLanguageLabel.Text = UiText.Get("Settings.UiLanguage", language);
            _roundWholeEurosCheckBox.Text = UiText.Get("Settings.RoundWholeEuros", language);
            _settingsPathLabel.Text = UiText.Format("Settings.SettingsFile", language, SettingsService.SettingsPath);
            _testConnectionButton.Text = UiText.Get("Settings.TestConnection", language);
            _editTextsButton.Text = language == SettingsService.SlovakUiLanguage ? "Upraviť preddefinované texty" : "Edit predefined texts";
            _saveButton.Text = UiText.Get("Common.Save", language);
            _cancelButton.Text = UiText.Get("Common.Cancel", language);
        }

        private void TestConnectionButton_Click(object sender, EventArgs e)
        {
            string language = SelectedLanguage;
            try
            {
                string baseUrl = ValidateAndNormalizeUrl( _apiBaseUrlTextBox.Text, language);
                var healthUri = new Uri( $"{baseUrl}/api/health", UriKind.Absolute);
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string response = client.GetStringAsync(healthUri).GetAwaiter().GetResult();
                    string enteredKey = _apiKeyTextBox.Text.Trim();
                    if (!string.IsNullOrEmpty(enteredKey))
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get,
                            new Uri(baseUrl + "/api/v1/account-detail/texts")))
                        {
                            request.Headers.Add("X-Api-Key", enteredKey);
                            using (HttpResponseMessage authenticated = client.SendAsync(request).GetAwaiter().GetResult())
                                authenticated.EnsureSuccessStatusCode();
                        }
                    }

                    MessageBox.Show(
                        this,
                        UiText.Get("Settings.ConnectionSuccessful", language) +
                        Environment.NewLine +
                        Environment.NewLine +
                        response,
                        UiText.Get("Settings.ConnectionTitle", language),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception exception)
            {
                ErrorDialog.ShowError(
                    this,
                    UiText.Get("Settings.ConnectionTitle", language),
                    UiText.Get("Settings.ConnectionFailed", language),
                    exception,
                    "Settings / Test API connection",
                    "SETTINGS-CONNECTION",
                    null,
                    language);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string language = SelectedLanguage;

            try
            {
                string baseUrl = ValidateAndNormalizeUrl(_apiBaseUrlTextBox.Text, language);

                SettingsService.Save(new AddInSettings
                    {
                        ApiBaseUrl = baseUrl,
                        ApiKey = _apiKeyTextBox.Text.Trim(),
                        UiLanguage = language,
                        RoundCalculatedAmountsToWholeEuros =
                            _roundWholeEurosCheckBox.Checked
                    });

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                ErrorDialog.ShowError(this,
                    UiText.Get("Settings.InvalidTitle", language),
                    exception.Message, exception, "Settings / Save", "SETTINGS-INVALID", null, language);
            }
        }

        private static string ValidateAndNormalizeUrl(string value, string language)
        {
            string normalized = (value ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(normalized, UriKind.Absolute, out Uri uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(UiText.Get("Settings.InvalidUrl", language));
            }
            return normalized;
        }

        private sealed class UiLanguageItem
        {
            public UiLanguageItem(string name, string code)
            {
                Name = name;
                Code = code;
            }

            public string Name { get; }
            public string Code { get; }

            public override string ToString()
            {
                return $"{Name} ({Code})";
            }
        }
    }
}
