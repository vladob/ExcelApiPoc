using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using AccountBalanceVerification.Core;
using AccountBalanceVerification.Import;
using ExcelApiPoc.MkSoft.Client;
namespace AccountBalanceVerification.AddIn
{
    internal sealed class ConnectionSettings
    {
        public string ApiBaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
    }
    internal sealed class CreateWorkbookForm : Form
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AccountBalanceVerification", "settings.json");
        private readonly TextBox url = new TextBox(), key = new TextBox { UseSystemPasswordChar = true }, client = new TextBox(), year = new TextBox { Text = DateTime.Today.Year.ToString() }, entity = new TextBox(), ico = new TextBox();
        private readonly TextBox journal = new TextBox(), ledger = new TextBox();
        private readonly ComboBox framework = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox source = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, producer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox empty = new CheckBox { Text = "Zahrnúť syntetické účty bez zostatkov a obratov", AutoSize = true };
        private readonly Button create = new Button { Text = "Vytvoriť", AutoSize = true };
        private bool busy;
        public string CreatedWorkbookName { get; private set; }
        public CreateWorkbookForm()
        {
            Text = "Inventarizácia"; Width = 730; Height = 590; StartPosition = FormStartPosition.CenterScreen;
            source.Items.AddRange(new object[] { "MkSoft", "Súbory" }); source.SelectedIndex = 0;
            producer.Items.AddRange(new object[] { "IfoSoft", "Urbis", "IVES", "OMEGA", "Softip-MOP" }); producer.SelectedIndex = 0;
            framework.Items.AddRange(new object[] { "PROFIT", "NONPROFIT", "GOV_LOCAL" }); framework.SelectedIndex = 0;
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoScroll = true, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            Controls.Add(grid);
            Add(grid, "Zdroj", source); Add(grid, "API URL", url); Add(grid, "API kľúč", key);
            Add(grid, "MkSoft klient ID", client); Add(grid, "Účtovný rok", year); Add(grid, "Názov účtovnej jednotky", entity); Add(grid, "IČO", ico);
            Add(grid, "Účtovná osnova (súbory)", framework); Add(grid, "Účtovný systém", producer); Add(grid, "Účtovný denník", journal, true); Add(grid, "Hlavná kniha", ledger, true);
            grid.Controls.Add(empty, 1, grid.RowCount++); grid.Controls.Add(create, 1, grid.RowCount++);
            create.Click += Create;
            FormClosing += (s, e) => { if (busy) e.Cancel = true; };
            if (File.Exists(SettingsPath))
            {
                var settings = JsonConvert.DeserializeObject<ConnectionSettings>(File.ReadAllText(SettingsPath));
                if (settings != null) { url.Text = settings.ApiBaseUrl; key.Text = settings.ApiKey; }
            }
        }
        private static void Add(TableLayoutPanel grid, string label, Control control, bool browse = false)
        {
            int row = grid.RowCount++;
            grid.Controls.Add(new Label { Text = label, AutoSize = true }, 0, row);
            control.Dock = DockStyle.Top; grid.Controls.Add(control, 1, row);
            if (browse)
            {
                var button = new Button { Text = "…", Width = 32 };
                button.Click += (s, e) => { using (var dialog = new OpenFileDialog()) if (dialog.ShowDialog() == DialogResult.OK) control.Text = dialog.FileName; };
                grid.Controls.Add(button, 2, row);
            }
        }
        private async void Create(object sender, EventArgs e)
        {
            if (!int.TryParse(year.Text, out int fiscalYear) || fiscalYear < 1900 || fiscalYear > 9999) { MessageBox.Show("Zadajte platný rok."); return; }
            busy = true; Controls[0].Enabled = false; UseWaitCursor = true;
            try
            {
                VerificationPackage package;
                if (source.SelectedIndex == 0)
                {
                    if (!int.TryParse(client.Text, out int clientId) || clientId <= 0) throw new InvalidOperationException("Zadajte platné ID klienta.");
                    if (string.IsNullOrWhiteSpace(key.Text)) throw new InvalidOperationException("Zadajte API kľúč.");
                    var api = new MkSoftClient(Http, url.Text.Trim(), key.Text.Trim());
                    package = await api.GetAsync(clientId, fiscalYear, false, CancellationToken.None);
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                    File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(new ConnectionSettings { ApiBaseUrl = url.Text.Trim(), ApiKey = key.Text.Trim() }, Formatting.Indented));
                }
                else
                {
                    string selectedProducer = producer.Text, journalPath = journal.Text.Trim(), ledgerPath = ledger.Text.Trim(), selectedIco = ico.Text.Trim();
                    if (selectedIco.Length == 0) throw new InvalidOperationException("Zadajte IČO.");
                    string selectedFramework = framework.Text;
                    var api = new MkSoftClient(Http, url.Text.Trim(), key.Text.Trim());
                    package = await Task.Run(() => FileSource.Read(selectedProducer, journalPath, ledgerPath, selectedIco, fiscalYear));
                    await api.ApplyFrameworkAsync(package, selectedFramework, CancellationToken.None);
                }
                if (!string.IsNullOrWhiteSpace(entity.Text)) package.EntityName = entity.Text.Trim();
                if (!string.IsNullOrWhiteSpace(ico.Text)) package.Ico = ico.Text.Trim();
                // The continuation runs on the WinForms/Excel UI thread; no COM calls run in Task.Run.
                CreatedWorkbookName = WorkbookWriter.Create(package, empty.Checked);
                busy = false; Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { busy = false; Controls[0].Enabled = true; UseWaitCursor = false; }
        }
    }
}
