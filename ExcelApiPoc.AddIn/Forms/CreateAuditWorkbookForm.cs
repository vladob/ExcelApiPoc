using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
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
        private readonly TextBox _generalLedgerPathTextBox;
        private readonly ComboBox _technicalTypeComboBox;
        private readonly ComboBox _accountingFormatComboBox;
        private readonly TextBox _icoTextBox;
        private readonly TextBox _fiscalYearTextBox;
        private readonly Button _continueButton;
        private readonly Workbook _auditWorkbook;
        private readonly List<string> _journalFilePaths = new List<string>();
        private bool _updatingJournalPathDisplay;
        private readonly string _uiLanguage;

        public CreateAuditWorkbookForm(Workbook auditWorkbook)
        {
            _auditWorkbook = auditWorkbook ??
                throw new ArgumentNullException(nameof(auditWorkbook));
            _uiLanguage = SettingsService.Load().UiLanguage;

            Text = UiText.Get("Create.Title", _uiLanguage);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 690;
            Height = 460;

            // Accounting journal
            AddLabel(UiText.Get("Create.AccountingJournal", _uiLanguage),15,22,145);

            _journalPathTextBox = new TextBox();
            _journalPathTextBox.SetBounds(165, 19, 430, 23);
            _journalPathTextBox.TextChanged += JournalPathTextBox_TextChanged;

            var journalBrowseButton = new Button
            {
                Text = "..."
            };

            journalBrowseButton.SetBounds(605, 18, 45, 25);
            journalBrowseButton.Click += JournalBrowseButton_Click;

            // Entity-specific accounting framework
            AddLabel(UiText.Get("Create.AccountingFramework", _uiLanguage),15,62,145);

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
                Text = UiText.Get("Create.Optional", _uiLanguage),
                AutoSize = true
            };

            accountsOptionalLabel.SetBounds(165, 85, 100, 20);

            // General ledger
            AddLabel(UiText.Get("Create.GeneralLedger", _uiLanguage),15,112,145);

            _generalLedgerPathTextBox = new TextBox();
            _generalLedgerPathTextBox.SetBounds(165,109,430,23);

            var generalLedgerBrowseButton = new Button { Text = "..." };
            generalLedgerBrowseButton.SetBounds(605,108,45,25);
            generalLedgerBrowseButton.Click += GeneralLedgerBrowseButton_Click;

            var generalLedgerOptionalLabel = new Label { Text = UiText.Get("Create.Optional", _uiLanguage), AutoSize = true };
            generalLedgerOptionalLabel.SetBounds(165,135,100,20);

            // Technical file type
            AddLabel(UiText.Get("Create.TechnicalType", _uiLanguage),15,175,145);

            _technicalTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _technicalTypeComboBox.SetBounds(165,172,200,25);
            _technicalTypeComboBox.Items.AddRange(new object[] {"Unknown", "CSV", "XML", "JSON", "PDF", "Excel"});
            _technicalTypeComboBox.SelectedIndex = 0;

            // Accounting-system format
            AddLabel(UiText.Get("Create.AccountingFormat", _uiLanguage),15,215,145);

            _accountingFormatComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _accountingFormatComboBox.SetBounds(165,212,200,25);
            _accountingFormatComboBox.Items.AddRange(
                new object[]
                {
                    "Unknown",
                    "IfoSoft",
                    "IVES",
                    "Softip-MOP",
                    "Urbis",
                    "MkSoft",
                    "Pohoda"
                });
            _accountingFormatComboBox.SelectedIndex = 0;

            // IČO
            AddLabel(UiText.Get("Create.Ico", _uiLanguage),15,255,145);

            _icoTextBox = new TextBox
            {
                MaxLength = 8
            };

            _icoTextBox.SetBounds(165, 252, 200, 23);

            // Fiscal year
            AddLabel(UiText.Get("Create.FiscalYear", _uiLanguage),15,295,145);

            _fiscalYearTextBox = new TextBox
            {
                MaxLength = 4
            };

            _fiscalYearTextBox.SetBounds(165,292,100,23);

            // Bottom buttons
            var settingsButton = new Button
            {
                Text = UiText.Get("Create.Settings", _uiLanguage)
            };

            settingsButton.SetBounds(15, 360, 105, 30);
            settingsButton.Click += SettingsButton_Click;

            var cancelButton = new Button
            {
                Text = UiText.Get("Common.Cancel", _uiLanguage),
                DialogResult = DialogResult.Cancel
            };

            cancelButton.SetBounds(470, 360, 85, 30);

            _continueButton = new Button
            {
                Text = UiText.Get("Create.Continue", _uiLanguage),
                Enabled = false
            };

            _continueButton.SetBounds(565, 360, 85, 30);
            _continueButton.Click += ContinueButton_Click;

            Controls.Add(_journalPathTextBox);
            Controls.Add(journalBrowseButton);
            Controls.Add(_accountsPathTextBox);
            Controls.Add(accountsBrowseButton);
            Controls.Add(accountsOptionalLabel);
            Controls.Add(_generalLedgerPathTextBox);
            Controls.Add(generalLedgerBrowseButton);
            Controls.Add(generalLedgerOptionalLabel);
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
                dialog.Title = UiText.Get("Create.SelectJournal", _uiLanguage);
                dialog.Multiselect = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string[] selectedPaths = dialog.FileNames;
                if (selectedPaths.Length > 1 &&
                    !AreSoftipMopJournalFiles(selectedPaths))
                {
                    MessageBox.Show(
                        UiText.Get("Create.MultipleJournalFiles", _uiLanguage),
                        UiText.Get("Create.SelectJournal", _uiLanguage),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _journalFilePaths.Clear();
                _journalFilePaths.AddRange(selectedPaths);
                UpdateJournalPathDisplay();

                SetBusy(true);
                try
                {
                    ProcessJournalFiles(_journalFilePaths);
                }
                finally
                {
                    SetBusy(false);
                }
            }
        }

        private static bool AreSoftipMopJournalFiles(
            IEnumerable<string> filePaths)
        {
            foreach (string filePath in filePaths)
            {
                if (!AccountingJournalDetectionService.TryDetect(
                        filePath,
                        out JournalDetectionResult detection) ||
                    !string.Equals(
                        detection.AccountingFormat,
                        "Softip-MOP",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateJournalPathDisplay()
        {
            _updatingJournalPathDisplay = true;
            try
            {
                _journalPathTextBox.Text = _journalFilePaths.Count == 1
                    ? _journalFilePaths[0]
                    : _journalFilePaths.Count + " files selected: " +
                        string.Join(
                            "; ",
                            _journalFilePaths.Select(Path.GetFileName));
            }
            finally
            {
                _updatingJournalPathDisplay = false;
            }
        }

        private void AccountsBrowseButton_Click(object sender,EventArgs e)
        {
            using (var dialog = CreateOpenFileDialog())
            {
                dialog.Title = UiText.Get("Create.SelectFramework", _uiLanguage);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _accountsPathTextBox.Text = dialog.FileName;
                }
            }
        }

        private void GeneralLedgerBrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = CreateOpenFileDialog())
            {
                dialog.Title = UiText.Get("Create.SelectGeneralLedger", _uiLanguage);
                if (dialog.ShowDialog() == DialogResult.OK)
                    _generalLedgerPathTextBox.Text = dialog.FileName;
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
            if (_updatingJournalPathDisplay)
            {
                return;
            }

            _journalFilePaths.Clear();
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
            bool workbookPopulationStarted = false;
            var diagnosticInputPaths = new List<string>();

            try
            {
                SetBusy(true);
                List<string> journalFilePaths = GetJournalFilePaths();
                diagnosticInputPaths.AddRange(journalFilePaths);

                if (journalFilePaths.Count == 0 ||
                    journalFilePaths.Any(path => !File.Exists(path)))
                {
                    throw new InvalidOperationException(UiText.Get("Create.InvalidJournal", _uiLanguage));
                }

                string accountsPath = _accountsPathTextBox.Text.Trim();
                string generalLedgerPath = _generalLedgerPathTextBox.Text.Trim();

                if (!string.IsNullOrWhiteSpace(accountsPath))
                    diagnosticInputPaths.Add(accountsPath);
                if (!string.IsNullOrWhiteSpace(generalLedgerPath))
                    diagnosticInputPaths.Add(generalLedgerPath);

                if (!string.IsNullOrWhiteSpace(accountsPath) && !File.Exists(accountsPath))
                {
                    throw new InvalidOperationException(UiText.Get("Create.MissingAccounts", _uiLanguage));
                }
                if (!string.IsNullOrWhiteSpace(generalLedgerPath) && !File.Exists(generalLedgerPath))
                    throw new InvalidOperationException(UiText.Get("Create.MissingLedger", _uiLanguage));


                if (!int.TryParse( _fiscalYearTextBox.Text.Trim(), out int selectedFiscalYear))
                {
                    throw new InvalidOperationException(UiText.Get("Create.InvalidFiscalYear", _uiLanguage));
                }

                string selectedIco = _icoTextBox.Text.Trim();

                var importRequest = new AccountingImportRequest
                {
                    AccountingFormat = _accountingFormatComboBox.Text,
                    JournalFilePath = journalFilePaths[0],
                    GeneralLedgerFilePath = string.IsNullOrWhiteSpace(
                        generalLedgerPath) ? null : generalLedgerPath,
                    ExpectedIco = selectedIco,
                    ExpectedFiscalYear = selectedFiscalYear
                };
                importRequest.JournalFilePaths.AddRange(journalFilePaths);

                AccountingImportPackage importPackage =
                    AccountingImportCoordinator.CreateDefault().Import(importRequest);

                JournalImport journalImport = importPackage.Journal;
                GeneralLedgerImport generalLedgerImport = importPackage.GeneralLedger;
                CalculatedGeneralLedger calculatedGeneralLedger =
                    importPackage.CalculatedGeneralLedger;
                JournalLedgerReconciliationResult canonicalReconciliation =
                    importPackage.JournalLedgerReconciliation;

                if (generalLedgerImport == null && !journalImport.Rows.Any(row => row.RecordKind == JournalRecordKind.Opening))
                {
                    throw new InvalidOperationException(
                        "The accounting journal does not contain opening " +
                        "balance records. Select the corresponding general " +
                        "ledger so that opening balances can be supplied " +
                        "and closing balances validated.");
                }

                AccountingFrameworkImport accountingFrameworkImport = null;
                AccountingFrameworkAccountEnrichmentResult accountingFrameworkEnrichment = null;
                GeneralLedgerReconciliationResult generalLedgerReconciliation = null;

                if (!string.IsNullOrWhiteSpace(accountsPath))
                {
                    if (!string.Equals(importPackage.AccountingFormat, "IfoSoft", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "An entity-specific accounting-framework export " +
                            "is currently supported only for IfoSoft. " +
                            "Leave the Accounting framework field empty for " +
                            "the current IVES, Urbis, and Softip-MOP imports.");
                    }

                    accountingFrameworkImport = new IfoSoftCsvAccountingFrameworkImporter() .Import(accountsPath);

                    if (!string.Equals(accountingFrameworkImport.Ico, journalImport.Ico, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The accounting framework belongs to IČO " +
                            accountingFrameworkImport.Ico +
                            ", but the journal belongs to IČO " +
                            journalImport.Ico +
                            ".");
                    }

                    if (accountingFrameworkImport.FiscalYear != selectedFiscalYear)
                    {
                        throw new InvalidOperationException(
                            "The accounting framework is for fiscal year " +
                            accountingFrameworkImport.FiscalYear +
                            ", but fiscal year " +
                            selectedFiscalYear +
                            " is selected.");
                    }
                }

                DateTime dateFrom = journalImport.Rows.Min(row => row.PostingDate);
                DateTime dateTo = journalImport.Rows.Max(row => row.PostingDate);
                List<AccountSummary> accountSummaries = JournalAccountSummaryBuilder.Build(journalImport);
                int journalReportAccountCount = accountSummaries.Count;

                if (canonicalReconciliation != null)
                {
                    generalLedgerReconciliation = CanonicalReconciliationAccountSummaryAdapter.Apply(canonicalReconciliation, accountSummaries);
                }

                AccountingEntityPackageEnvelope accountingEntityEnvelope =
                    AccountingEntityPackageApiClient.GetEnvelope(
                        journalImport.Ico);

                AuditCalculationPackageSelectionResponse calculationSelection;

                try
                {
                    calculationSelection =
                        AuditCalculationPackageSelectionService.Load(
                            journalImport.Ico,
                            selectedFiscalYear);
                }
                catch (Exception calculationFailure)
                {
                    if (accountingFrameworkImport != null)
                    {
                        accountingFrameworkEnrichment =
                            AccountingFrameworkAccountEnricher.Enrich(
                                accountSummaries,
                                accountingFrameworkImport);
                    }

                    if (generalLedgerImport != null)
                    {
                        GeneralLedgerReconciliationService.ResolveNames(
                            accountSummaries,
                            generalLedgerImport);
                    }

                    workbookPopulationStarted = true;
                    AuditWorkbookWriter.CreateWithoutCalculation(
                        _auditWorkbook,
                        journalImport,
                        accountSummaries,
                        calculatedGeneralLedger,
                        canonicalReconciliation,
                        accountingFrameworkImport,
                        generalLedgerImport,
                        accountingEntityEnvelope,
                        calculationFailure);

                    SetBusy(false);
                    ErrorDialog.ShowError(
                        this,
                        UiText.Get(
                            "Create.CalculationUnavailableTitle",
                            _uiLanguage),
                        UiText.Get(
                            "Create.CalculationUnavailable",
                            _uiLanguage),
                        calculationFailure,
                        "Create Audit Workbook / Calculation package",
                        "CAW-CALCULATION-UNAVAILABLE",
                        diagnosticInputPaths,
                        _uiLanguage);

                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }

                string selectedFrameworkCode =
                    calculationSelection.CalculationPackage.FrameworkCode;

                AccountFrameworkLoadResult frameworkLoad =
                    AccountFrameworkService.Load(
                        selectedFrameworkCode,
                        selectedFiscalYear);
                ApplicableAccountFrameworkResponse framework =
                    frameworkLoad.Framework;
                AccountFrameworkEnrichmentResult enrichmentResult =
                    AccountFrameworkEnricher.Enrich(
                        accountSummaries,
                        framework);

                if (accountingFrameworkImport != null)
                    accountingFrameworkEnrichment = AccountingFrameworkAccountEnricher.Enrich( accountSummaries, accountingFrameworkImport);
                if (generalLedgerImport != null)
                    GeneralLedgerReconciliationService.ResolveNames(accountSummaries, generalLedgerImport);

                decimal totalDebitTurnover = accountSummaries.Sum(account => account.DebitTurnover);
                decimal totalCreditTurnover = accountSummaries.Sum(account => account.CreditTurnover);
                decimal journalDifference = totalDebitTurnover - totalCreditTurnover;
                bool fiscalYearMatches =
                    journalImport.FiscalYear == selectedFiscalYear;

                RegisterUzFinancialReportSelection reportSelection =
                    RegisterUzFinancialReportSelector.Select(
                        accountingEntityEnvelope,
                        selectedFiscalYear,
                        calculationSelection.FinancialStatementId,
                        calculationSelection.FinancialReportId,
                        calculationSelection.RegisterUzTemplateId);

                var reportContext = new AuditReportContext
                    {
                        Ico = journalImport.Ico,
                        FiscalYear = selectedFiscalYear,
                        TemplateErpId = reportSelection.TemplateErpId,
                        FrameworkCode = framework.FrameworkCode,
                        SelectionSource = "RegisterUZ",
                        RegisterUzReportId = reportSelection.RegisterUzReportId.ToString(CultureInfo.InvariantCulture)
                    };

                AuditTemplatePackageLoadResult templatePackageLoad = AuditTemplatePackageService.Load(reportContext);
                AuditTemplatePackageResponse templatePackage = templatePackageLoad.Package;
                AuditReportCalculationResult calculationResult = AuditReportCalculationService.Calculate(accountSummaries, templatePackage);
                AnalyticalMappingData analyticalMapping = AnalyticalMappingBuilder.Build(accountSummaries, templatePackage, calculationResult);
                const int rejectedRecordCount = 0;
                bool frameworkFromCache = string.Equals(frameworkLoad.Source, "Local cache", StringComparison.OrdinalIgnoreCase);
                bool templateFromCache = string.Equals(templatePackageLoad.Source, "Local cache", StringComparison.OrdinalIgnoreCase);

                var message = new System.Text.StringBuilder();

                message.AppendLine($"Company: {journalImport.CompanyName}");
                message.AppendLine($"IČO: {journalImport.Ico}");

                message.AppendLine();
                message.AppendLine($"Posting-date range: {dateFrom:yyyy-MM-dd} – {dateTo:yyyy-MM-dd}");
                message.AppendLine($"Selected fiscal year: {selectedFiscalYear}");
                message.AppendLine($"Fiscal year matches: " + $"{(fiscalYearMatches ? "Yes" : "No")}");

                message.AppendLine();
                message.AppendLine($"Source records: {journalImport.Rows.Count:N0}");
                message.AppendLine($"Rejected records: {rejectedRecordCount:N0}");
                message.AppendLine($"Distinct journal report accounts: {journalReportAccountCount:N0}");

                message.AppendLine();
                message.AppendLine($"Debit turnover: {totalDebitTurnover:N2}");
                message.AppendLine($"Credit turnover: {totalCreditTurnover:N2}");
                message.AppendLine($"Journal difference: {journalDifference:N2}");

                message.AppendLine();
                message.AppendLine($"Framework matched accounts: " + $"{enrichmentResult.MatchedCount:N0}");
                message.AppendLine($"Unmatched accounts: " + $"{enrichmentResult.UnmatchedCount:N0}");

                if (enrichmentResult.InvalidSyntheticCodeCount > 0)
                    message.AppendLine($"Invalid account codes: " + $"{enrichmentResult.InvalidSyntheticCodeCount:N0}");

                if (accountingFrameworkImport != null)
                {
                    message.AppendLine();
                    message.AppendLine($"Accounting-framework rows: {accountingFrameworkImport.Rows.Count:N0}");
                    message.AppendLine($"Accounts matched by exact code: {accountingFrameworkEnrichment.MatchedAccountCount:N0}");
                    message.AppendLine($"Accounts absent from accounting framework: {accountingFrameworkEnrichment.UnmatchedAccountCount:N0}");
                    message.AppendLine($"Normalized duplicate account codes: {accountingFrameworkEnrichment.DuplicateNormalizedAccountCount:N0}");
                }

                if (generalLedgerImport != null)
                {
                    message.AppendLine();
                    message.AppendLine($"General-ledger rows: {generalLedgerImport.Rows.Count:N0}");
                    message.AppendLine($"Journal accounts: {generalLedgerReconciliation.JournalAccountCount:N0}");
                    message.AppendLine($"General-ledger accounts: {generalLedgerReconciliation.LedgerAccountCount:N0}");
                    message.AppendLine($"Matched accounts: {generalLedgerReconciliation.MatchedAccountCount:N0}");
                    message.AppendLine($"Journal-only accounts: {generalLedgerReconciliation.JournalOnlyAccountCount:N0}");
                    message.AppendLine($"Ledger-only accounts: {generalLedgerReconciliation.LedgerOnlyAccountCount:N0}");
                    message.AppendLine($"Reconciled accounts: {generalLedgerReconciliation.ReconciledAccountCount:N0}");
                    message.AppendLine($"Accounts with differences: {generalLedgerReconciliation.DifferentAccountCount:N0}");
                    message.AppendLine($"Debit-turnover difference: {generalLedgerReconciliation.DebitTurnoverDifference:N2}");
                    message.AppendLine($"Credit-turnover difference: {generalLedgerReconciliation.CreditTurnoverDifference:N2}");
                    message.AppendLine($"Closing-balance difference: {generalLedgerReconciliation.ClosingBalanceDifference:N2}");

                    message.AppendLine("Opening balance source: " + (canonicalReconciliation.OpeningBalanceSource == OpeningBalanceSource.Journal ? "Accounting journal" : "General ledger"));
                    message.AppendLine("Journal contains opening records: " + (canonicalReconciliation.JournalContainsOpeningRecords ? "Yes" : "No"));
                    message.AppendLine("Journal contains closing records: " + (canonicalReconciliation.JournalContainsClosingRecords ? "Yes" : "No"));
                }

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
                    enrichmentResult.InvalidSyntheticCodeCount > 0 ||
                    (accountingFrameworkEnrichment != null &&
                     (accountingFrameworkEnrichment.UnmatchedAccountCount > 0 ||
                      accountingFrameworkEnrichment.ConflictingDuplicateAccountCount > 0));
                hasWarning = hasWarning ||
                    (generalLedgerReconciliation != null && !generalLedgerReconciliation.IsReconciled);
                MessageBoxIcon icon = hasWarning
                    ? MessageBoxIcon.Warning
                    : MessageBoxIcon.Information;

                SetBusy(false);
                MessageBox.Show(
                    message.ToString(),
                    UiText.Get("Create.PreflightTitle", _uiLanguage),
                    MessageBoxButtons.OK,
                    icon);
                SetBusy(true);

                workbookPopulationStarted = true;
                var workbook = AuditWorkbookWriter.CreateWorkbook(
                    _auditWorkbook,
                    journalImport,
                    accountSummaries,
                    calculatedGeneralLedger,
                    canonicalReconciliation,
                    frameworkLoad,
                    analyticalMapping,
                    templatePackage,
                    reportContext,
                    templatePackageLoad,
                    reportSelection,
                    accountingEntityEnvelope,
                    accountingFrameworkImport,
                    generalLedgerImport);

                AuditWorkbookRecalculationResult recalculation =
                    AuditWorkbookRecalculationService.Recalculate(workbook);

                SetBusy(false);
                AuditWorkbookRecalculationDialog.Show(recalculation);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                SetBusy(false);

                ErrorDialog.ShowError(
                    this,
                    UiText.Get("Create.Title", _uiLanguage),
                    UiText.Get("Create.Failed", _uiLanguage),
                    exception,
                    "Create Audit Workbook",
                    "CAW-UNEXPECTED",
                    diagnosticInputPaths,
                    _uiLanguage);

                if (workbookPopulationStarted)
                {
                    DialogResult = DialogResult.Abort;
                    Close();
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void DetectJournalInformation(string filePath)
        {
            // Reset values previously detected from another file.
            _accountingFormatComboBox.SelectedItem = "Unknown";

            _icoTextBox.Clear();
            _fiscalYearTextBox.Clear();

            if (!AccountingJournalDetectionService.TryDetect(filePath,out JournalDetectionResult detection))
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

        private void ProcessJournalFiles(IReadOnlyList<string> filePaths)
        {
            bool filesExist = filePaths != null &&
                filePaths.Count > 0 &&
                filePaths.All(File.Exists);

            _continueButton.Enabled = filesExist;
            if (!filesExist)
            {
                return;
            }

            ProcessJournalFile(filePaths[0]);
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            Cursor = busy
                ? Cursors.WaitCursor
                : Cursors.Default;
            System.Windows.Forms.Cursor.Current = Cursor;

            if (busy)
                Update();
        }

        private List<string> GetJournalFilePaths()
        {
            if (_journalFilePaths.Count > 0)
            {
                return new List<string>(_journalFilePaths);
            }

            string path = _journalPathTextBox.Text.Trim();
            return path.Length == 0
                ? new List<string>()
                : new List<string> { path };
        }
    }
}
