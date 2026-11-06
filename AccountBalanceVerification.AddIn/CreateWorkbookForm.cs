using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AccountBalanceVerification.Core;
using AccountBalanceVerification.Import;
using ExcelApiPoc.MkSoft.Client;
using Excel = Microsoft.Office.Interop.Excel;
namespace AccountBalanceVerification.AddIn
{
    internal sealed class CreateWorkbookForm : Form
    {
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        private readonly Excel.Workbook workbook;
        private ConnectionSettings settings;
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly TextBox mkYear = new TextBox { Text = DateTime.Today.Year.ToString() }, client = new TextBox(), mkIco = new TextBox(), mkEntity = new TextBox();
        private readonly TextBox journal = new TextBox(), chart = new TextBox(), ledger = new TextBox(), fileYear = new TextBox(), fileIco = new TextBox(), fileEntity = new TextBox();
        private readonly ComboBox producer = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label mkStatus = new Label { AutoSize = true }, fileStatus = new Label { AutoSize = true };
        private readonly Button create = new Button { Text = "Vytvoriť", AutoSize = true }, preferences = new Button { Text = "Nastavenia", AutoSize = true };
        private readonly Dictionary<Control, string> automatic = new Dictionary<Control, string>();
        private bool busy;
        private MkSoftEntity resolved;
        private string metadataSignature;

        public CreateWorkbookForm(Excel.Workbook workbook)
        {
            this.workbook = workbook ?? throw new ArgumentNullException(nameof(workbook));
            settings = ConnectionSettings.Load();
            Text = "Inventarizácia"; Width = 800; Height = 520; MinimumSize = new System.Drawing.Size(700, 500); StartPosition = FormStartPosition.CenterScreen;
            producer.Items.AddRange(new object[] { "IfoSoft", "Urbis", "IVES", "OMEGA", "Softip-MOP" });
            var mkPage = new TabPage("MkSoft"); var filePage = new TabPage("Súbory");
            tabs.TabPages.Add(mkPage); tabs.TabPages.Add(filePage); Controls.Add(tabs);
            var mk = NewGrid(); mkPage.Controls.Add(mk);
            Add(mk, "Účtovný rok", mkYear); Add(mk, "MkSoft klient ID", client); Add(mk, "IČO", mkIco);
            Add(mk, "Názov účtovnej jednotky", mkEntity);
            var lookup = new Button { Text = "Načítať údaje", AutoSize = true };
            Add(mk, "", lookup); Add(mk, "", mkStatus);
            lookup.Click += async (s, e) => await RunAsync(LoadEntityAsync);
            mkYear.TextChanged += (s, e) => InvalidateEntity(); client.TextChanged += (s, e) => InvalidateEntity();
            var files = NewGrid(); filePage.Controls.Add(files);
            AddFile(files, "Účtovný denník", journal); AddFile(files, "Účtový rozvrh", chart); AddFile(files, "Hlavná kniha", ledger);
            Add(files, "Účtovný systém", producer); Add(files, "Účtovný rok", fileYear); Add(files, "IČO", fileIco);
            Add(files, "Názov účtovnej jednotky", fileEntity);
            Add(files, "", fileStatus);
            producer.SelectionChangeCommitted += async (s, e) => await DetectAsync();
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(12) };
            buttons.Controls.Add(create); buttons.Controls.Add(preferences); Controls.Add(buttons);
            create.Click += Create;
            preferences.Click += (s, e) => {
                using (var dialog = new SettingsForm(settings))
                    if (dialog.ShowDialog(this) == DialogResult.OK) { settings = dialog.Settings; InvalidateEntity(); }
            };
            FormClosing += (s, e) => { if (busy) e.Cancel = true; };
        }
        internal static TableLayoutPanel NewGrid()
        {
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoScroll = true, Padding = new Padding(12) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            return grid;
        }
        internal static void Add(TableLayoutPanel grid, string label, Control control)
        {
            int row = grid.RowCount++;
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 6, 3, 6) }, 0, row);
            control.Dock = DockStyle.Top; grid.Controls.Add(control, 1, row);
        }
        private void AddFile(TableLayoutPanel grid, string label, TextBox control)
        {
            Add(grid, label, control);
            var browse = new Button { Text = "…", Width = 32 };
            grid.Controls.Add(browse, 2, grid.RowCount - 1);
            browse.Click += async (s, e) => {
                using (var dialog = new OpenFileDialog())
                    if (dialog.ShowDialog(this) == DialogResult.OK) { control.Text = dialog.FileName; await DetectAsync(); }
            };
            control.Leave += async (s, e) => await DetectAsync();
        }
        private void InvalidateEntity()
        {
            if (resolved != null)
            {
                if (mkIco.Text == resolved.Ico) mkIco.Clear();
                if (mkEntity.Text == resolved.EntityName) mkEntity.Clear();
            }
            resolved = null; mkStatus.Text = "Zadajte rok a klient ID alebo IČO, potom načítajte údaje.";
        }
        private void SetBusy(bool value)
        {
            busy = value; tabs.Enabled = !value; create.Enabled = !value; preferences.Enabled = !value; UseWaitCursor = value;
        }
        private async Task RunAsync(Func<Task> action)
        {
            if (busy) return;
            SetBusy(true);
            try { await action(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { if (!IsDisposed) SetBusy(false); }
        }
        private MkSoftClient Api()
        {
            if (string.IsNullOrWhiteSpace(settings.ApiBaseUrl) || string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new InvalidOperationException("V Nastaveniach zadajte API URL a API kľúč.");
            return new MkSoftClient(Http, settings.ApiBaseUrl, settings.ApiKey);
        }
        private static int Year(TextBox box)
        {
            if (!int.TryParse(box.Text.Trim(), out var year) || year < 1900 || year > 9999) throw new InvalidOperationException("Zadajte platný účtovný rok.");
            return year;
        }
        private static void CheckIco(string ico)
        {
            if (ico.Length != 8 || ico.Any(c => c < '0' || c > '9')) throw new InvalidOperationException("IČO musí obsahovať 8 číslic.");
        }
        private async Task LoadEntityAsync()
        {
            resolved = null;
            int year = Year(mkYear); int? clientId = null;
            if (!string.IsNullOrWhiteSpace(client.Text))
            {
                if (!int.TryParse(client.Text.Trim(), out var id) || id <= 0) throw new InvalidOperationException("Zadajte platné ID klienta.");
                clientId = id;
            }
            string ico = mkIco.Text.Trim();
            if (!string.IsNullOrEmpty(ico)) CheckIco(ico);
            if (!clientId.HasValue && ico.Length == 0) throw new InvalidOperationException("Zadajte MkSoft klient ID alebo IČO.");
            var matches = await Api().FindEntitiesAsync(year, clientId, ico, CancellationToken.None);
            if (matches.Count == 0) throw new InvalidOperationException("Pre zadaný rok a identifikátory sa nenašla účtovná jednotka.");
            if (matches.Count != 1) throw new InvalidOperationException("IČO má viac účtovných databáz. Zadajte MkSoft klient ID: " + string.Join(", ", matches.Select(x => x.ClientErpId)));
            var found = matches[0];
            client.Text = found.ClientErpId.ToString(); mkIco.Text = found.Ico; mkEntity.Text = found.EntityName;
            resolved = found;
            mkStatus.Text = "Údaje načítané. Skontrolujte ich a kliknite na Vytvoriť.";
        }
        private void Fill(Control control, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (string.IsNullOrWhiteSpace(control.Text) || (automatic.TryGetValue(control, out var previous) && control.Text == previous))
            { control.Text = value; automatic[control] = control.Text; }
        }
        private async Task DetectAsync()
        {
            if (busy) return;
            string j = journal.Text.Trim(), c = chart.Text.Trim(), l = ledger.Text.Trim(), p = producer.Text;
            string signature = j + "|" + c + "|" + l + "|" + p;
            if (signature == metadataSignature || (j + c + l).Length == 0) return;
            await RunAsync(async () => {
                var metadata = await Task.Run(() => FileMetadata.Read(j, c, l, p));
                Fill(fileIco, metadata.Ico); Fill(fileYear, metadata.FiscalYear?.ToString()); Fill(fileEntity, metadata.EntityName); Fill(producer, metadata.Producer);
                fileStatus.Text = metadata.Warnings.Count == 0 ? "Údaje načítané zo súborov. Skontrolujte ich pred vytvorením zošita." : string.Join(Environment.NewLine, metadata.Warnings.Distinct());
                metadataSignature = j + "|" + c + "|" + l + "|" + producer.Text;
            });
        }
        private async void Create(object sender, EventArgs e)
        {
            bool populationStarted = false;
            await RunAsync(async () => {
                try
                {
                    VerificationPackage package;
                    string selectedIco, selectedName;
                    if (tabs.SelectedIndex == 0)
                    {
                        if (resolved == null) { await LoadEntityAsync(); return; } // Review before creation.
                        selectedIco = mkIco.Text.Trim(); selectedName = mkEntity.Text.Trim();
                        CheckIco(selectedIco);
                        package = await Api().GetAsync(resolved.ClientErpId, Year(mkYear), false, CancellationToken.None);
                    }
                    else
                    {
                        int year = Year(fileYear); selectedIco = fileIco.Text.Trim(); CheckIco(selectedIco);
                        selectedName = fileEntity.Text.Trim();
                        string p = producer.Text, j = journal.Text.Trim(), l = ledger.Text.Trim(), c = chart.Text.Trim();
                        if (p.Length == 0) throw new InvalidOperationException("Vyberte účtovný systém.");
                        package = await Task.Run(() => FileSource.Read(p, j, l, selectedIco, year, c));
                    }
                    package.Ico = selectedIco;
                    if (!string.IsNullOrWhiteSpace(selectedName)) package.EntityName = selectedName;
                    populationStarted = true;
                    WorkbookWriter.Populate(workbook, package, settings.IncludeEmptySyntheticAccounts);
                    busy = false; DialogResult = DialogResult.OK; Close();
                }
                catch
                {
                    if (populationStarted) { busy = false; DialogResult = DialogResult.Abort; Close(); }
                    throw;
                }
            });
        }
    }
}
