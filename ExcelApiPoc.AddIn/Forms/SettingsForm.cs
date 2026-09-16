using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExcelApiPoc.AddIn.Forms
{
    internal sealed class SettingsForm : Form
    {
        private readonly Label _apiUrlLabel;
        private readonly Label _uiLanguageLabel;
        private readonly Label _settingsPathLabel;
        private readonly TextBox _apiBaseUrlTextBox;
        private readonly ComboBox _uiLanguageComboBox;
        private readonly Button _testConnectionButton;
        private readonly Button _saveButton;
        private readonly Button _cancelButton;
        private AddInSettings _settings;

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
            Height = 245;

            _apiUrlLabel = new Label();
            _apiUrlLabel.SetBounds(15, 20, 120, 23);

            _apiBaseUrlTextBox = new TextBox();
            _apiBaseUrlTextBox.SetBounds(145, 17, 415, 23);

            _uiLanguageLabel = new Label();
            _uiLanguageLabel.SetBounds(15, 58, 120, 23);

            _uiLanguageComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _uiLanguageComboBox.SetBounds(145, 55, 210, 25);
            _uiLanguageComboBox.Items.Add(
                new UiLanguageItem("English", SettingsService.DefaultUiLanguage));
            _uiLanguageComboBox.Items.Add(
                new UiLanguageItem("Slovenčina", SettingsService.SlovakUiLanguage));
            _uiLanguageComboBox.SelectedIndexChanged +=
                UiLanguageComboBox_SelectedIndexChanged;

            _settingsPathLabel = new Label
            {
                AutoEllipsis = true
            };
            _settingsPathLabel.SetBounds(15, 95, 545, 23);

            _testConnectionButton = new Button();
            _testConnectionButton.SetBounds(15, 140, 145, 30);
            _testConnectionButton.Click += TestConnectionButton_Click;

            _saveButton = new Button();
            _saveButton.SetBounds(380, 140, 85, 30);
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new Button
            {
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.SetBounds(475, 140, 85, 30);

            Controls.Add(_apiUrlLabel);
            Controls.Add(_apiBaseUrlTextBox);
            Controls.Add(_uiLanguageLabel);
            Controls.Add(_uiLanguageComboBox);
            Controls.Add(_settingsPathLabel);
            Controls.Add(_testConnectionButton);
            Controls.Add(_saveButton);
            Controls.Add(_cancelButton);

            AcceptButton = _saveButton;
            CancelButton = _cancelButton;

            _apiBaseUrlTextBox.Text = _settings.ApiBaseUrl;
            SelectLanguage(_settings.UiLanguage);
            ApplyLanguage(language);
        }

        private string SelectedLanguage
        {
            get
            {
                var selected =
                    _uiLanguageComboBox.SelectedItem as UiLanguageItem;

                return selected?.Code ??
                    SettingsService.DefaultUiLanguage;
            }
        }

        private void SelectLanguage(string language)
        {
            string normalized =
                SettingsService.NormalizeUiLanguage(language);

            for (int index = 0;
                 index < _uiLanguageComboBox.Items.Count;
                 index++)
            {
                var item =
                    _uiLanguageComboBox.Items[index] as UiLanguageItem;

                if (item != null &&
                    string.Equals(
                        item.Code,
                        normalized,
                        StringComparison.Ordinal))
                {
                    _uiLanguageComboBox.SelectedIndex = index;
                    return;
                }
            }

            _uiLanguageComboBox.SelectedIndex = 0;
        }

        private void UiLanguageComboBox_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ApplyLanguage(SelectedLanguage);
        }

        private void ApplyLanguage(string language)
        {
            Text = UiText.Get("Settings.Title", language);
            _apiUrlLabel.Text =
                UiText.Get("Settings.ApiBaseUrl", language);
            _uiLanguageLabel.Text =
                UiText.Get("Settings.UiLanguage", language);
            _settingsPathLabel.Text =
                UiText.Format(
                    "Settings.SettingsFile",
                    language,
                    SettingsService.SettingsPath);
            _testConnectionButton.Text =
                UiText.Get("Settings.TestConnection", language);
            _saveButton.Text =
                UiText.Get("Common.Save", language);
            _cancelButton.Text =
                UiText.Get("Common.Cancel", language);
        }

        private void TestConnectionButton_Click(
            object sender,
            EventArgs e)
        {
            string language = SelectedLanguage;

            try
            {
                string baseUrl =
                    ValidateAndNormalizeUrl(
                        _apiBaseUrlTextBox.Text,
                        language);

                var healthUri =
                    new Uri(
                        $"{baseUrl}/api/health",
                        UriKind.Absolute);

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string response =
                        client.GetStringAsync(healthUri)
                            .GetAwaiter()
                            .GetResult();

                    MessageBox.Show(
                        this,
                        UiText.Get(
                            "Settings.ConnectionSuccessful",
                            language) +
                        Environment.NewLine +
                        Environment.NewLine +
                        response,
                        UiText.Get(
                            "Settings.ConnectionTitle",
                            language),
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
                string baseUrl =
                    ValidateAndNormalizeUrl(
                        _apiBaseUrlTextBox.Text,
                        language);

                SettingsService.Save(
                    new AddInSettings
                    {
                        ApiBaseUrl = baseUrl,
                        UiLanguage = language
                    });

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                ErrorDialog.ShowError(
                    this,
                    UiText.Get("Settings.InvalidTitle", language),
                    exception.Message,
                    exception,
                    "Settings / Save",
                    "SETTINGS-INVALID",
                    null,
                    language);
            }
        }

        private static string ValidateAndNormalizeUrl(
            string value,
            string language)
        {
            string normalized =
                (value ?? string.Empty).Trim().TrimEnd('/');

            if (!Uri.TryCreate(
                    normalized,
                    UriKind.Absolute,
                    out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    UiText.Get("Settings.InvalidUrl", language));
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

    internal sealed class ErrorDialog : Form
    {
        private const int CollapsedHeight = 315;
        private const int ExpandedHeight = 590;

        private readonly RichTextBox _messageBox;
        private readonly Label _detailsLabel;
        private readonly TextBox _detailsBox;
        private readonly Button _detailsButton;
        private readonly Button _copyButton;
        private readonly Button _closeButton;
        private readonly string _language;
        private readonly string _copyText;
        private bool _detailsVisible;

        private ErrorDialog(
            string title,
            string userMessage,
            string technicalDetails,
            string language)
        {
            _language =
                SettingsService.NormalizeUiLanguage(language);
            _copyText =
                userMessage +
                Environment.NewLine +
                Environment.NewLine +
                technicalDetails;

            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            Width = 760;
            Height = CollapsedHeight;

            _messageBox = new RichTextBox
            {
                ReadOnly = true,
                DetectUrls = false,
                BorderStyle = BorderStyle.FixedSingle,
                TabStop = false
            };
            _messageBox.SetBounds(15, 15, 710, 145);
            _messageBox.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            _messageBox.Rtf =
                BuildMessageRtf(
                    title,
                    userMessage);

            _detailsLabel = new Label
            {
                Text = UiText.Get(
                    "Error.TechnicalDetails",
                    _language),
                Visible = false
            };
            _detailsLabel.SetBounds(15, 175, 200, 22);

            _detailsBox = new TextBox
            {
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Visible = false,
                Text = technicalDetails
            };
            _detailsBox.SetBounds(15, 200, 710, 290);
            _detailsBox.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;

            _detailsButton = new Button
            {
                Text = UiText.Get(
                    "Error.ShowDetails",
                    _language)
            };
            _detailsButton.SetBounds(15, 220, 130, 30);
            _detailsButton.Anchor =
                AnchorStyles.Left |
                AnchorStyles.Bottom;
            _detailsButton.Click += DetailsButton_Click;

            _copyButton = new Button
            {
                Text = UiText.Get("Common.Copy", _language)
            };
            _copyButton.SetBounds(485, 220, 145, 30);
            _copyButton.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Bottom;
            _copyButton.Click += CopyButton_Click;

            _closeButton = new Button
            {
                Text = UiText.Get("Common.Close", _language),
                DialogResult = DialogResult.OK
            };
            _closeButton.SetBounds(640, 220, 85, 30);
            _closeButton.Anchor =
                AnchorStyles.Right |
                AnchorStyles.Bottom;

            Controls.Add(_messageBox);
            Controls.Add(_detailsLabel);
            Controls.Add(_detailsBox);
            Controls.Add(_detailsButton);
            Controls.Add(_copyButton);
            Controls.Add(_closeButton);

            AcceptButton = _closeButton;
            CancelButton = _closeButton;

            Resize += (sender, e) => LayoutControls();
            LayoutControls();
        }

        private static string BuildMessageRtf(
            string title,
            string userMessage)
        {
            return
                @"{\rtf1\ansi\deff0{\fonttbl{\f0 Segoe UI;}}" +
                @"\fs18\b " +
                EscapeRtf(title) +
                @"\b0\par\par " +
                EscapeRtf(userMessage) +
                "}";
        }

        private static string EscapeRtf(string value)
        {
            var builder = new StringBuilder();

            foreach (char character in value ?? string.Empty)
            {
                switch (character)
                {
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    case '{':
                        builder.Append(@"\{");
                        break;
                    case '}':
                        builder.Append(@"\}");
                        break;
                    case '\r':
                        break;
                    case '\n':
                        builder.Append(@"\par ");
                        break;
                    default:
                        if (character <= 0x7f)
                        {
                            builder.Append(character);
                        }
                        else
                        {
                            builder.Append(@"\u");
                            builder.Append((short)character);
                            builder.Append('?');
                        }
                        break;
                }
            }

            return builder.ToString();
        }

        public static void ShowError(
            IWin32Window owner,
            string title,
            string userMessage,
            Exception exception,
            string operation,
            string errorCode,
            IEnumerable<string> inputFilePaths,
            string language = null)
        {
            string effectiveLanguage =
                language ??
                SettingsService.Load().UiLanguage;

            ClassifyKnownCreateWorkbookError(
                ref userMessage,
                exception,
                ref operation,
                ref errorCode,
                effectiveLanguage);

            string details =
                ErrorDiagnosticBuilder.Build(
                    exception,
                    operation,
                    errorCode,
                    inputFilePaths,
                    effectiveLanguage);

            using (var dialog =
                new ErrorDialog(
                    title,
                    userMessage,
                    details,
                    effectiveLanguage))
            {
                if (owner == null)
                    dialog.ShowDialog();
                else
                    dialog.ShowDialog(owner);
            }
        }

        private static void ClassifyKnownCreateWorkbookError(
            ref string userMessage,
            Exception exception,
            ref string operation,
            ref string errorCode,
            string language)
        {
            if (!string.Equals(
                    errorCode,
                    "CAW-UNEXPECTED",
                    StringComparison.Ordinal) ||
                !(exception is InvalidDataException) ||
                string.IsNullOrWhiteSpace(exception.Message) ||
                exception.Message.IndexOf(
                    "No registered importer recognizes the accounting-journal file",
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            string accountingFormat = "selected";
            Match formatMatch = Regex.Match(
                exception.Message,
                @"accounting format '([^']+)'",
                RegexOptions.IgnoreCase);

            if (formatMatch.Success)
                accountingFormat = formatMatch.Groups[1].Value;

            string normalizedLanguage =
                SettingsService.NormalizeUiLanguage(language);

            if (string.Equals(
                    normalizedLanguage,
                    SettingsService.SlovakUiLanguage,
                    StringComparison.Ordinal))
            {
                userMessage =
                    "Vybraný účtovný denník nebol rozpoznaný ako formát " +
                    accountingFormat +
                    ". Vyberte správny účtovný systém alebo použite export " +
                    "účtovného denníka v podporovanom formáte " +
                    accountingFormat +
                    ".";
            }
            else
            {
                userMessage =
                    "The selected accounting journal is not recognized as " +
                    accountingFormat +
                    " format. Select the correct Accounting format, or " +
                    "provide an accounting-journal export in a supported " +
                    accountingFormat +
                    " format.";
            }

            operation =
                "Create Audit Workbook / Accounting journal import";
            errorCode = "CAW-IMPORT-JOURNAL-FORMAT";
        }

        private void DetailsButton_ClickOld(object sender, EventArgs e)
        {
            _detailsVisible = !_detailsVisible;

            SuspendLayout();
            try
            {
                Height = _detailsVisible
                    ? ExpandedHeight
                    : CollapsedHeight;

                _detailsLabel.Visible = _detailsVisible;
                _detailsBox.Visible = _detailsVisible;

                _detailsButton.Text =
                    UiText.Get(
                        _detailsVisible
                            ? "Error.HideDetails"
                            : "Error.ShowDetails",
                        _language);

                _detailsButton.Top =
                    _detailsVisible
                        ? ClientSize.Height - 50
                        : 220;

                _copyButton.Top = _detailsButton.Top;
                _closeButton.Top = _detailsButton.Top;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void DetailsButton_Click(object sender, EventArgs e)
        {
            _detailsVisible = !_detailsVisible;

            SuspendLayout();
            try
            {
                Height = _detailsVisible ? ExpandedHeight : CollapsedHeight;
                _detailsLabel.Visible = _detailsVisible;
                _detailsBox.Visible = _detailsVisible;
                _detailsButton.Text = UiText.Get(_detailsVisible ? "Error.HideDetails" : "Error.ShowDetails", _language);
                LayoutControls();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void LayoutControls()
        {
            const int margin = 15;
            const int buttonHeight = 30;
            const int buttonGap = 10;

            int buttonTop = ClientSize.Height - margin - buttonHeight;

            _detailsButton.SetBounds(margin, buttonTop, 130, buttonHeight);
            _closeButton.SetBounds(ClientSize.Width - margin - 85, buttonTop, 85, buttonHeight);
            _copyButton.SetBounds(_closeButton.Left - buttonGap - 145, buttonTop, 145, buttonHeight);
            _messageBox.Width = ClientSize.Width - (2 * margin);

            if (_detailsVisible)
            {
                const int detailsTop = 200;

                _detailsBox.SetBounds(margin, detailsTop, ClientSize.Width - (2 * margin), Math.Max(60, buttonTop - buttonGap - detailsTop));
            }
        }

        private void CopyButton_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                Clipboard.SetText(_copyText);
            }
            catch
            {
                // Clipboard can temporarily be unavailable.
            }
        }
    }

    internal static class ErrorDiagnosticBuilder
    {
        private static readonly Regex CredentialRegex =
            new Regex(
                @"(?i)\b(api[-_ ]?key|access[-_ ]?token|token|authorization|password|secret)\b\s*[:=]\s*[^\s,;]+",
                RegexOptions.Compiled);

        private static readonly Regex QuerySecretRegex =
            new Regex(
                @"(?i)([?&](?:api[-_]?key|access_token|token|password|secret)=)[^&\s]+",
                RegexOptions.Compiled);

        private static readonly Regex SourcePathRegex =
            new Regex(
                @"\s+in\s+[A-Za-z]:\\[^\r\n]*?(?::line\s+\d+)?(?=\r?$)",
                RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex AbsolutePathRegex =
            new Regex(
                @"(?i)\b[A-Z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*([^\\/:*?""<>|\r\n]+)",
                RegexOptions.Compiled);

        public static string Build(
            Exception exception,
            string operation,
            string errorCode,
            IEnumerable<string> inputFilePaths,
            string language)
        {
            var paths =
                new List<string>(
                    inputFilePaths ??
                    Array.Empty<string>());

            var builder = new StringBuilder();

            builder.AppendLine(
                $"{UiText.Get("Error.Timestamp", language)}: " +
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
            builder.AppendLine(
                $"{UiText.Get("Error.Operation", language)}: " +
                $"{operation ?? string.Empty}");
            builder.AppendLine(
                $"{UiText.Get("Error.Code", language)}: " +
                $"{errorCode ?? string.Empty}");

            if (exception != null)
            {
                builder.AppendLine(
                    $"{UiText.Get("Error.Exception", language)}: " +
                    exception.GetType().FullName);
                builder.AppendLine(
                    $"{UiText.Get("Error.Message", language)}: " +
                    Sanitize(exception.Message, paths));
            }

            if (paths.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine(
                    UiText.Get("Error.InputFiles", language) + ":");

                foreach (string path in paths)
                {
                    AppendFileMetadata(
                        builder,
                        path,
                        language);
                }
            }

            string exceptionDetails =
                BuildExceptionDetails(exception);

            if (!string.IsNullOrWhiteSpace(exceptionDetails))
            {
                builder.AppendLine();
                builder.AppendLine(
                    UiText.Get(
                        "Error.TechnicalDetails",
                        language) + ":");
                builder.AppendLine(
                    Sanitize(exceptionDetails, paths));
            }

            return builder.ToString().TrimEnd();
        }

        private static void AppendFileMetadata(
            StringBuilder builder,
            string path,
            string language)
        {
            string fileName =
                string.IsNullOrWhiteSpace(path)
                    ? "(none)"
                    : Path.GetFileName(path);

            string size = "n/a";
            string sha256 = "n/a";

            try
            {
                if (!string.IsNullOrWhiteSpace(path) &&
                    File.Exists(path))
                {
                    var info = new FileInfo(path);
                    size =
                        $"{info.Length:N0} bytes";
                    sha256 = ComputeSha256(path);
                }
            }
            catch (Exception metadataException)
            {
                sha256 =
                    "unavailable (" +
                    metadataException.GetType().Name +
                    ")";
            }

            builder.AppendLine(
                $"  {UiText.Get("Error.Name", language)}: {fileName}");
            builder.AppendLine(
                $"  {UiText.Get("Error.Size", language)}: {size}");
            builder.AppendLine(
                $"  {UiText.Get("Error.Sha256", language)}: {sha256}");
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream =
                new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
            {
                byte[] hash = sha.ComputeHash(stream);
                var builder = new StringBuilder(hash.Length * 2);

                foreach (byte value in hash)
                    builder.Append(value.ToString("X2"));

                return builder.ToString();
            }
        }

        private static string BuildExceptionDetails(
            Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var builder = new StringBuilder();
            Exception current = exception;
            int depth = 0;

            while (current != null && depth < 8)
            {
                if (depth > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine(
                        $"Inner exception {depth}:");
                }

                builder.AppendLine(
                    current.GetType().FullName);
                builder.AppendLine(current.Message);

                if (!string.IsNullOrWhiteSpace(current.StackTrace))
                    builder.AppendLine(current.StackTrace);

                current = current.InnerException;
                depth++;
            }

            return builder.ToString().TrimEnd();
        }

        private static string Sanitize(
            string value,
            IReadOnlyList<string> inputPaths)
        {
            string sanitized = value ?? string.Empty;

            foreach (string path in inputPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                sanitized =
                    sanitized.Replace(
                        path,
                        Path.GetFileName(path));
            }

            sanitized =
                CredentialRegex.Replace(
                    sanitized,
                    "$1=[REDACTED]");

            sanitized =
                QuerySecretRegex.Replace(
                    sanitized,
                    "$1[REDACTED]");

            sanitized =
                SourcePathRegex.Replace(
                    sanitized,
                    string.Empty);

            sanitized =
                AbsolutePathRegex.Replace(
                    sanitized,
                    "[path]\\$1");

            return sanitized;
        }
    }
}
