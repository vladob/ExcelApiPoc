using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Forms
{
    internal sealed class AccountDetailTextsForm : Form
    {
        private readonly Excel.Workbook _workbook;
        private readonly AccountDetailTextSettings _original;
        private readonly BindingList<TextEdit> _texts;
        private readonly BindingList<DefaultEdit> _defaults;
        private readonly DataGridView _textGrid;
        private readonly DataGridView _defaultGrid;

        public AccountDetailTextsForm(Excel.Workbook workbook)
        {
            _workbook = workbook;
            _original = AccountDetailSnapshot.ReadTexts(workbook);
            _texts = new BindingList<TextEdit>(_original.Texts.Select(t => new TextEdit {
                TextId = t.TextId, CategoryCode = t.CategoryCode, TextSk = t.TextSk, SortOrder = t.SortOrder }).ToList());
            _defaults = new BindingList<DefaultEdit>(_original.Defaults.Select(d => new DefaultEdit {
                Account = d.Account, CategoryCode = d.CategoryCode, TextId = d.TextId }).ToList());
            Text = "Edit predefined texts";
            Width = 1100;
            Height = 680;
            StartPosition = FormStartPosition.CenterScreen;
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var textTab = new TabPage("Predefined texts");
            var defaultTab = new TabPage("Account defaults");
            _textGrid = Grid(_texts, nameof(TextEdit.TextId), nameof(TextEdit.CategoryCode),
                nameof(TextEdit.TextSk), nameof(TextEdit.SortOrder));
            _defaultGrid = Grid(_defaults, nameof(DefaultEdit.Account),
                nameof(DefaultEdit.CategoryCode), nameof(DefaultEdit.TextId));
            _textGrid.Columns[nameof(TextEdit.TextId)].ReadOnly = true;
            _textGrid.Columns[nameof(TextEdit.TextSk)].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _defaultGrid.Columns[nameof(DefaultEdit.Account)].Width = 100;
            textTab.Controls.Add(_textGrid);
            defaultTab.Controls.Add(_defaultGrid);
            tabs.TabPages.Add(textTab);
            tabs.TabPages.Add(defaultTab);
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft };
            var save = new Button { Text = "Save changes", Width = 120 };
            var discard = new Button { Text = "Discard changes", Width = 130, DialogResult = DialogResult.Cancel };
            var remove = new Button { Text = "Delete selected row", Width = 145 };
            save.Click += Save;
            remove.Click += (sender, args) =>
            {
                DataGridView grid = tabs.SelectedIndex == 0 ? _textGrid : _defaultGrid;
                if (grid.CurrentRow != null && !grid.CurrentRow.IsNewRow) grid.Rows.Remove(grid.CurrentRow);
            };
            bottom.Controls.Add(discard);
            bottom.Controls.Add(save);
            bottom.Controls.Add(remove);
            Controls.Add(tabs);
            Controls.Add(bottom);
            CancelButton = discard;
        }

        private static DataGridView Grid<T>(BindingList<T> source, params string[] properties)
        {
            var grid = new DataGridView { Dock = DockStyle.Fill,
                AutoGenerateColumns = false, AllowUserToAddRows = true,
                AllowUserToDeleteRows = true, RowHeadersVisible = false };
            foreach (string property in properties)
                grid.Columns.Add(new DataGridViewTextBoxColumn {
                    Name = property, DataPropertyName = property, HeaderText = property });
            grid.DataSource = source;
            return grid;
        }

        private void Save(object sender, EventArgs e)
        {
            try
            {
                _textGrid.EndEdit();
                _defaultGrid.EndEdit();
                ValidateEdits();
                var originalDefaults = _original.Defaults.ToDictionary(x => Key(x.Account, x.CategoryCode));
                var editedDefaults = _defaults.ToDictionary(x => Key(x.Account, x.CategoryCode));
                foreach (AccountDetailDefault old in _original.Defaults)
                    if (!editedDefaults.ContainsKey(Key(old.Account, old.CategoryCode)))
                        AccountDetailSettingsApiClient.DeleteDefault(old.Account, old.CategoryCode);

                var originalTexts = _original.Texts.ToDictionary(x => Key(x.CategoryCode, x.TextId));
                var editedTexts = _texts.Where(x => x.TextId > 0).ToDictionary(x => Key(x.CategoryCode, x.TextId));
                foreach (AccountDetailText old in _original.Texts)
                    if (!editedTexts.ContainsKey(Key(old.CategoryCode, old.TextId)))
                        AccountDetailSettingsApiClient.DeleteText(old.CategoryCode, old.TextId);
                foreach (TextEdit edit in _texts)
                {
                    if (edit.TextId == 0)
                        edit.TextId = AccountDetailSettingsApiClient.CreateText(edit.CategoryCode, edit.TextSk, edit.SortOrder).TextId;
                    else
                    {
                        AccountDetailText old = originalTexts[Key(edit.CategoryCode, edit.TextId)];
                        if (old.TextSk != edit.TextSk || old.SortOrder != edit.SortOrder)
                            AccountDetailSettingsApiClient.UpdateText(edit.CategoryCode, edit.TextId, edit.TextSk, edit.SortOrder);
                    }
                }
                foreach (DefaultEdit edit in _defaults)
                    if (!originalDefaults.TryGetValue(Key(edit.Account, edit.CategoryCode), out AccountDetailDefault old) || old.TextId != edit.TextId)
                        AccountDetailSettingsApiClient.SetDefault(edit.Account, edit.CategoryCode, edit.TextId);

                // Read back only after an explicit edit, so server-assigned IDs and the workbook snapshot agree.
                AccountDetailSnapshot.ReplaceTexts(_workbook, AccountDetailSettingsApiClient.GetTexts());
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                // Some operations may already have succeeded. Align the local snapshot with the server.
                try { AccountDetailSnapshot.ReplaceTexts(_workbook, AccountDetailSettingsApiClient.GetTexts()); }
                catch { /* Preserve the original error for the auditor. */ }
                MessageBox.Show(this, exception.Message + "\nReopen the editor to review the current server values.",
                    "Predefined texts", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void ValidateEdits()
        {
            var categories = new HashSet<string>(_original.Categories.Select(x => x.Code));
            foreach (TextEdit text in _texts)
                if (!categories.Contains(text.CategoryCode) || string.IsNullOrWhiteSpace(text.TextSk) ||
                    text.TextSk.Length > 500 || text.SortOrder <= 0)
                    throw new InvalidOperationException("Each text requires a valid category, 1–500 characters, and a positive sort order.");
            foreach (TextEdit text in _texts.Where(x => x.TextId > 0))
                if (!_original.Texts.Any(x => x.CategoryCode == text.CategoryCode && x.TextId == text.TextId))
                    throw new InvalidOperationException("An existing text's category and ID cannot be changed. Delete it and add a new text.");
            if (_texts.Where(x => x.TextId > 0).GroupBy(x => Key(x.CategoryCode, x.TextId)).Any(x => x.Count() > 1))
                throw new InvalidOperationException("A text cannot appear twice.");
            foreach (DefaultEdit item in _defaults)
                if (item.Account == null || item.Account.Length != 3 || !item.Account.All(char.IsDigit) ||
                    !categories.Contains(item.CategoryCode) || item.TextId <= 0 ||
                    !_texts.Any(t => t.CategoryCode == item.CategoryCode && t.TextId == item.TextId))
                    throw new InvalidOperationException("Each default needs a three digit account and an existing text ID from its category.");
            if (_defaults.GroupBy(x => Key(x.Account, x.CategoryCode)).Any(x => x.Count() > 1))
                throw new InvalidOperationException("An account can have only one default per category.");
        }

        private static string Key(string first, object second) => first + "|" + second;

        private sealed class TextEdit
        {
            public TextEdit() { }
            public int TextId { get; set; }
            public string CategoryCode { get; set; }
            public string TextSk { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class DefaultEdit
        {
            public DefaultEdit() { }
            public string Account { get; set; }
            public string CategoryCode { get; set; }
            public int TextId { get; set; }
        }
    }
}
