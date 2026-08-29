using ExcelApiPoc.AddIn.Models;
using ExcelApiPoc.AddIn.Services;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace ExcelApiPoc.AddIn.Forms
{
    internal sealed class CreateAuditWorkbookForm : Form
    {
        private readonly TextBox _journalPathTextBox;
        private readonly TextBox _accountsPathTextBox;
        private readonly ComboBox _technicalTypeComboBox;
        private readonly ComboBox _accountingFormatComboBox;
        private readonly TextBox _icoTextBox;
        private readonly TextBox _fiscalYearTextBox;
        private readonly Button _continueButton;

        public CreateAuditWorkbookForm()
        {
            Text = "Create Audit Workbook";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 690;
            Height = 410;

            // Accounting journal
            AddLabel("Accounting journal: *",15,22,145);

            _journalPathTextBox = new TextBox();
            _journalPathTextBox.SetBounds(165, 19, 430, 23);
            _journalPathTextBox.TextChanged +=
                JournalPathTextBox_TextChanged;

            var journalBrowseButton = new Button
            {
                Text = "..."
            };

            journalBrowseButton.SetBounds(605, 18, 45, 25);
            journalBrowseButton.Click += JournalBrowseButton_Click;

            // Accounts list
            AddLabel("Accounts list:",15,62,145);

            _accountsPathTextBox = new TextBox();
            _accountsPathTextBox.SetBounds(165, 59, 430, 23);

            var accountsBrowseButton = new Button
            {
                Text = "..."
            };

            accountsBrowseButton.SetBounds(605, 58, 45, 25);
            accountsBrowseButton.Click +=AccountsBrowseButton_Click;

            var accountsOptionalLabel = new Label
            {
                Text = "Optional",
                AutoSize = true
            };

            accountsOptionalLabel.SetBounds(165, 85, 100, 20);

            // Technical file type
            AddLabel("Technical type:",15,125,145);

            _technicalTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _technicalTypeComboBox.SetBounds(165,122,200,25);

            _technicalTypeComboBox.Items.AddRange(new object[] {"Unknown", "CSV", "XML", "JSON", "PDF", "Excel"});

            _technicalTypeComboBox.SelectedIndex = 0;

            // Accounting-system format
            AddLabel("Accounting format:",15,165,145);

            _accountingFormatComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _accountingFormatComboBox.SetBounds(165,162,200,25);

            _accountingFormatComboBox.Items.AddRange(new object[] {"Unknown", "IfoSoft", "MkSoft", "Pohoda"});

            _accountingFormatComboBox.SelectedIndex = 0;

            // IČO
            AddLabel("IČO:",15,205,145);

            _icoTextBox = new TextBox
            {
                MaxLength = 8
            };

            _icoTextBox.SetBounds(165, 202, 200, 23);

            // Fiscal year
            AddLabel("Fiscal year:",15,245,145);

            _fiscalYearTextBox = new TextBox
            {
                MaxLength = 4
            };

            _fiscalYearTextBox.SetBounds(165,242,100,23);

            // Bottom buttons
            var settingsButton = new Button
            {
                Text = "Settings..."
            };

            settingsButton.SetBounds(15, 310, 105, 30);
            settingsButton.Click += SettingsButton_Click;

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            cancelButton.SetBounds(470, 310, 85, 30);

            _continueButton = new Button
            {
                Text = "Continue",
                Enabled = false
            };

            _continueButton.SetBounds(565, 310, 85, 30);
            _continueButton.Click += ContinueButton_Click;

            Controls.Add(_journalPathTextBox);
            Controls.Add(journalBrowseButton);
            Controls.Add(_accountsPathTextBox);
            Controls.Add(accountsBrowseButton);
            Controls.Add(accountsOptionalLabel);
            Controls.Add(_technicalTypeComboBox);
            Controls.Add(_accountingFormatComboBox);
            Controls.Add(_icoTextBox);
            Controls.Add(_fiscalYearTextBox);
            Controls.Add(settingsButton);
            Controls.Add(cancelButton);
            Controls.Add(_continueButton);

            AcceptButton = _continueButton;
            CancelButton = cancelButton;
        }

        private void AddLabel(string text, int left, int top, int width)
        {
            var label = new Label {Text = text, AutoSize = false};
            label.SetBounds(left, top, width, 23);
            Controls.Add(label);
        }

        private void JournalBrowseButton_Click(object sender,EventArgs e)
        {
            using (var dialog = CreateOpenFileDialog())
            {
                dialog.Title = "Select Accounting Journal";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                _journalPathTextBox.Text = dialog.FileName;
                ProcessJournalFile(dialog.FileName);
            }
        }

        private void AccountsBrowseButton_Click(object sender,EventArgs e)
        {
            using (var dialog = CreateOpenFileDialog())
            {
                dialog.Title = "Select Accounts List";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _accountsPathTextBox.Text = dialog.FileName;
                }
            }
        }

        private static OpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter =
                    "Supported files|" +
                    "*.csv;*.xml;*.json;*.pdf;*.xlsx;*.xls|" +
                    "CSV files|*.csv|" +
                    "XML files|*.xml|" +
                    "JSON files|*.json|" +
                    "PDF files|*.pdf|" +
                    "Excel files|*.xlsx;*.xls|" +
                    "All files|*.*"
            };
        }

        private void JournalPathTextBox_TextChanged(object sender,EventArgs e)
        {
            ProcessJournalFile(_journalPathTextBox.Text);
        }

        private void SelectTechnicalType(string path)
        {
            string extension = Path.GetExtension(path) ?.ToLowerInvariant();
            string technicalType;

            switch (extension)
            {
                case ".csv": technicalType = "CSV";
                    break;

                case ".xml": technicalType = "XML";
                    break;

                case ".json": technicalType = "JSON";
                    break;

                case ".pdf": technicalType = "PDF";
                    break;

                case ".xlsx":
                case ".xls": technicalType = "Excel";
                    break;

                default: technicalType = "Unknown";
                    break;
            }
            _technicalTypeComboBox.SelectedItem = technicalType;
        }

        private void SettingsButton_Click(object sender,EventArgs e)
        {
            using (var dialog = new SettingsForm())
            {
                dialog.ShowDialog(this);
            }
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            try
            {
                string journalPath = _journalPathTextBox.Text.Trim();

                if (!File.Exists(journalPath))
                {
                    throw new InvalidOperationException("Select a valid accounting journal file.");
                }

                string accountsPath = _accountsPathTextBox.Text.Trim();

                if (!string.IsNullOrWhiteSpace(accountsPath) && !File.Exists(accountsPath))
                {
                    throw new InvalidOperationException( "The selected accounts-list file does not exist.");
                }

                if (!string.Equals(_accountingFormatComboBox.Text, "IfoSoft", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Preflight validation is currently implemented " + "only for IfoSoft journals.");
                }

                if (!int.TryParse( _fiscalYearTextBox.Text.Trim(), out int selectedFiscalYear))
                {
                    throw new InvalidOperationException("Enter a valid fiscal year.");
                }

                var importer = new IfoSoftCsvJournalImporter();

                if (!importer.CanImport(journalPath,_accountingFormatComboBox.Text))
                {
                    throw new InvalidOperationException("No compatible journal importer was found.");
                }
                JournalImport journalImport =importer.Import(journalPath);
                DateTime dateFrom = journalImport.Rows.Min(row => row.PostingDate);
                DateTime dateTo = journalImport.Rows.Max(row => row.PostingDate);
                List<AccountSummary> accountSummaries = JournalAccountSummaryBuilder.Build(journalImport);
                AccountFrameworkLoadResult frameworkLoad = AccountFrameworkService.Load("GOV_LOCAL", selectedFiscalYear);
                ApplicableAccountFrameworkResponse framework = frameworkLoad.Framework;
                AccountFrameworkEnrichmentResult enrichmentResult = AccountFrameworkEnricher.Enrich(accountSummaries, framework);

                decimal totalDebitTurnover = accountSummaries.Sum(account => account.DebitTurnover);
                decimal totalCreditTurnover = accountSummaries.Sum(account => account.CreditTurnover);
                decimal journalDifference = totalDebitTurnover - totalCreditTurnover;
                bool fiscalYearMatches = dateFrom.Year == selectedFiscalYear && dateTo.Year == selectedFiscalYear;

                var reportContext =
                    new AuditReportContext
                    {
                        Ico = journalImport.Ico,
                        FiscalYear = selectedFiscalYear,
                        TemplateErpId = 690,
                        SelectionSource = "ConfiguredTestFixture",
                        RegisterUzReportId = string.Empty
                    };

                AuditTemplatePackageLoadResult templatePackageLoad = AuditTemplatePackageService.Load(reportContext);
                AuditTemplatePackageResponse templatePackage = templatePackageLoad.Package;
                AuditReportCalculationResult calculationResult = AuditReportCalculationService.Calculate(accountSummaries, templatePackage);
                AnalyticalMappingData analyticalMapping = AnalyticalMappingBuilder.Build(accountSummaries, templatePackage, calculationResult);
                const int rejectedRecordCount = 0;
                bool frameworkFromCache = string.Equals(
                    frameworkLoad.Source,
                    "Local cache",
                    StringComparison.OrdinalIgnoreCase);
                bool templateFromCache = string.Equals(
                    templatePackageLoad.Source,
                    "Local cache",
                    StringComparison.OrdinalIgnoreCase);

                var message = new System.Text.StringBuilder();

                message.AppendLine($"Company: {journalImport.CompanyName}");
                message.AppendLine($"IČO: {journalImport.Ico}");

                message.AppendLine();
                message.AppendLine($"Journal period: {dateFrom:yyyy-MM-dd} – {dateTo:yyyy-MM-dd}");
                message.AppendLine($"Selected fiscal year: {selectedFiscalYear}");
                message.AppendLine($"Fiscal year matches: " + $"{(fiscalYearMatches ? "Yes" : "No")}");

                message.AppendLine();
                message.AppendLine($"Source records: {journalImport.Rows.Count:N0}");
                message.AppendLine($"Rejected records: {rejectedRecordCount:N0}");
                message.AppendLine($"Distinct accounts: {accountSummaries.Count:N0}");

                message.AppendLine();
                message.AppendLine($"Debit turnover: {totalDebitTurnover:N2}");
                message.AppendLine($"Credit turnover: {totalCreditTurnover:N2}");
                message.AppendLine($"Journal difference: {journalDifference:N2}");

                message.AppendLine();
                message.AppendLine($"Framework matched accounts: " + $"{enrichmentResult.MatchedCount:N0}");
                message.AppendLine($"Unmatched accounts: " + $"{enrichmentResult.UnmatchedCount:N0}");

                if (enrichmentResult.InvalidSyntheticCodeCount > 0)
                    message.AppendLine($"Invalid account codes: " + $"{enrichmentResult.InvalidSyntheticCodeCount:N0}");

                message.AppendLine();
                message.AppendLine($"Account framework: " + $"{framework.FrameworkCode} / " + $"{framework.VersionCode}");
                message.AppendLine($"Report template: " + $"{templatePackage.Template.TemplateErpId} / " + $"{templatePackage.Template.Name}");

                if (frameworkFromCache || templateFromCache)
                {
                    message.AppendLine();

                    if (frameworkFromCache && templateFromCache)
                        message.AppendLine("Framework and template loaded from local cache.");
                    else if (frameworkFromCache)
                        message.AppendLine("Framework loaded from local cache.");
                    else
                        message.AppendLine("Template loaded from local cache.");
                }

                bool hasWarning =
                    !fiscalYearMatches ||
                    rejectedRecordCount > 0 ||
                    journalDifference != 0 ||
                    enrichmentResult.UnmatchedCount > 0 ||
                    enrichmentResult.InvalidSyntheticCodeCount > 0;
                MessageBoxIcon icon = hasWarning
                    ? MessageBoxIcon.Warning
                    : MessageBoxIcon.Information;

                MessageBox.Show(message.ToString(), "Accounting Journal Preflight", MessageBoxButtons.OK, icon);

                var workbook = AuditWorkbookWriter.CreateWorkbook(journalImport, accountSummaries, frameworkLoad, analyticalMapping, templatePackage, reportContext, templatePackageLoad);

                AuditWorkbookRecalculationDialog.Show(
                    AuditWorkbookRecalculationService.Recalculate(workbook));

                DialogResult = DialogResult.OK;
                Close();
                workbook.Activate();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Accounting journal processing failed.\n\n" + exception.Message, "Create Audit Workbook", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DetectJournalInformation(string filePath)
        {
            // Reset values previously detected from another file.
            _accountingFormatComboBox.SelectedItem = "Unknown";

            _icoTextBox.Clear();
            _fiscalYearTextBox.Clear();

            if (!IfoSoftCsvJournalDetector.TryDetect(filePath,out JournalDetectionResult detection))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(detection.TechnicalType))
            {
                _technicalTypeComboBox.SelectedItem = detection.TechnicalType;
            }

            if (!string.IsNullOrWhiteSpace(detection.AccountingFormat))
            {
                _accountingFormatComboBox.SelectedItem = detection.AccountingFormat;
            }

            if (!string.IsNullOrWhiteSpace(detection.Ico))
            {
                _icoTextBox.Text = detection.Ico;
            }

            if (detection.FiscalYear.HasValue)
            {
                _fiscalYearTextBox.Text = detection.FiscalYear.Value.ToString( CultureInfo.InvariantCulture);
            }
        }

        private void ProcessJournalFile(string filePath)
        {
            string path = (filePath ?? string.Empty).Trim();
            bool fileExists = File.Exists(path);
            _continueButton.Enabled = fileExists;
            if (!fileExists)
            {
                return;
            }

            SelectTechnicalType(path);
            DetectJournalInformation(path);
        }
    }
}
