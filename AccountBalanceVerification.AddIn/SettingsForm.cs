using System;
using System.IO;
using System.Linq;
using AccountBalanceVerification.Core;
using System.Windows.Forms;
using Newtonsoft.Json;
namespace AccountBalanceVerification.AddIn
{
    internal sealed class ConnectionSettings
    {
        public string ApiBaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public bool IncludeEmptySyntheticAccounts { get; set; }
        public string[] SignatureRoles { get; set; } = PrintPlan.DefaultRoles.ToArray();
        private static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccountBalanceVerification", "settings.json");
        public static ConnectionSettings Load() => File.Exists(SettingsPath)
            ? JsonConvert.DeserializeObject<ConnectionSettings>(File.ReadAllText(SettingsPath)) ?? new ConnectionSettings()
            : new ConnectionSettings();
        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
    internal sealed class SettingsForm : Form
    {
        public ConnectionSettings Settings { get; private set; }
        public SettingsForm(ConnectionSettings current)
        {
            Text = "Nastavenia"; Width = 680; Height = 390; StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            var url = new TextBox { Text = current.ApiBaseUrl };
            var key = new TextBox { Text = current.ApiKey, UseSystemPasswordChar = true };
            var empty = new CheckBox { Text = "Zahrnúť syntetické účty bez zostatkov a obratov", AutoSize = true, Checked = current.IncludeEmptySyntheticAccounts };
            var roles = new TextBox { Multiline = true, Height = 110, ScrollBars = ScrollBars.Vertical, Text = string.Join(Environment.NewLine, current.SignatureRoles ?? PrintPlan.DefaultRoles) };
            var grid = CreateWorkbookForm.NewGrid(); Controls.Add(grid);
            CreateWorkbookForm.Add(grid, "API URL", url); CreateWorkbookForm.Add(grid, "API kľúč", key);
            CreateWorkbookForm.Add(grid, "", empty);
            CreateWorkbookForm.Add(grid, "Funkcie (jedna na riadok)", roles);
            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(12) };
            var save = new Button { Text = "Uložiť", AutoSize = true };
            var cancel = new Button { Text = "Zrušiť", DialogResult = DialogResult.Cancel, AutoSize = true };
            buttons.Controls.Add(save); buttons.Controls.Add(cancel); Controls.Add(buttons);
            AcceptButton = save; CancelButton = cancel;
            save.Click += (s, e) => {
                if (!string.IsNullOrWhiteSpace(url.Text) && (!Uri.TryCreate(url.Text.Trim(), UriKind.Absolute, out var address) || (address.Scheme != "http" && address.Scheme != "https")))
                { MessageBox.Show(this, "Zadajte platnú HTTP(S) adresu API.", Text); return; }
                var settings = new ConnectionSettings { ApiBaseUrl = url.Text.Trim().TrimEnd('/'), ApiKey = key.Text.Trim(), IncludeEmptySyntheticAccounts = empty.Checked, SignatureRoles = roles.Lines.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct().ToArray() };
                try { settings.Save(); Settings = settings; DialogResult = DialogResult.OK; Close(); }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
        }
    }
}
