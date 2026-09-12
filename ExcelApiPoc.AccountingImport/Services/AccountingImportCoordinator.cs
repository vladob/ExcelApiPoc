using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Urbis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExcelApiPoc.AccountingImport.Services
{
    public sealed class AccountingImportCoordinator
    {
        private readonly IReadOnlyList<IJournalImporter> journalImporters;

        private readonly IReadOnlyList<IGeneralLedgerImporter> generalLedgerImporters;

        public AccountingImportCoordinator(IEnumerable<IJournalImporter> journalImporters, IEnumerable<IGeneralLedgerImporter> generalLedgerImporters)
        {
            if (journalImporters == null)
            {
                throw new ArgumentNullException(
                    nameof(journalImporters));
            }

            if (generalLedgerImporters == null)
            {
                throw new ArgumentNullException(
                    nameof(generalLedgerImporters));
            }

            this.journalImporters =
                journalImporters.ToList();

            this.generalLedgerImporters =
                generalLedgerImporters.ToList();

            if (this.journalImporters.Count == 0)
            {
                throw new ArgumentException(
                    "At least one journal importer is required.",
                    nameof(journalImporters));
            }
        }

        public static AccountingImportCoordinator CreateDefault()
        {
            return new AccountingImportCoordinator(
                new IJournalImporter[]
                {
                    new IfoSoftCsvJournalImporter(),
                    new UrbisExcelJournalImporter()
                },
                new IGeneralLedgerImporter[]
                {
                    new IfoSoftCsvGeneralLedgerImporter(),
                    new UrbisExcelGeneralLedgerImporter()
                });
        }

        public AccountingImportPackage Import(AccountingImportRequest request)
        {
            ValidateRequest(request);

            IJournalImporter journalImporter = SelectExactlyOne(journalImporters, importer => importer.CanImport( request.JournalFilePath, request.AccountingFormat),
                    "accounting-journal", request.JournalFilePath, request.AccountingFormat);

            JournalImport journal = journalImporter.Import(request.JournalFilePath);

            ValidateJournal(journal, request);

            GeneralLedgerImport generalLedger = null;

            JournalLedgerReconciliationResult reconciliation = null;

            if (!string.IsNullOrWhiteSpace(
                    request.GeneralLedgerFilePath))
            {
                IGeneralLedgerImporter ledgerImporter =
                    SelectExactlyOne(
                        generalLedgerImporters,
                        importer => importer.CanImport(
                            request.GeneralLedgerFilePath,
                            request.AccountingFormat),
                        "general-ledger",
                        request.GeneralLedgerFilePath,
                        request.AccountingFormat);

                generalLedger =
                    ledgerImporter.Import(
                        request.GeneralLedgerFilePath);

                ValidateGeneralLedger(
                    generalLedger,
                    journal,
                    request);

                reconciliation =
                    JournalLedgerReconciliationService.Reconcile(
                        journal,
                        generalLedger);
            }

            return new AccountingImportPackage
            {
                AccountingFormat =
                    journal.AccountingFormat,
                Ico = journal.Ico,
                FiscalYear = journal.FiscalYear,
                ExportStage = journal.ExportStage,
                Journal = journal,
                GeneralLedger = generalLedger,
                JournalLedgerReconciliation = reconciliation
            };
        }

        private static TImporter SelectExactlyOne<TImporter>(IEnumerable<TImporter> importers, Func<TImporter, bool> canImport, string documentDescription, string filePath,  string accountingFormat)
        {
            List<TImporter> matches =
                importers.Where(canImport).ToList();

            if (matches.Count == 0)
            {
                throw new InvalidDataException(
                    "No registered importer recognizes the " +
                    documentDescription +
                    " file '" +
                    Path.GetFileName(filePath) +
                    "' as accounting format '" +
                    accountingFormat +
                    "'.");
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    "More than one registered importer recognizes " +
                    "the " +
                    documentDescription +
                    " file '" +
                    Path.GetFileName(filePath) +
                    "' as accounting format '" +
                    accountingFormat +
                    "'. Importer selection is ambiguous.");
            }

            return matches[0];
        }

        private static void ValidateRequest(AccountingImportRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(
                    request.AccountingFormat))
            {
                throw new ArgumentException(
                    "An accounting format is required.",
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(
                    request.JournalFilePath))
            {
                throw new ArgumentException(
                    "An accounting-journal file is required.",
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(
                    request.ExpectedIco))
            {
                throw new ArgumentException(
                    "The expected IČO is required.",
                    nameof(request));
            }

            if (request.ExpectedFiscalYear < 1900 ||
                request.ExpectedFiscalYear > 9999)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request),
                    "The expected fiscal year is invalid.");
            }
        }

        private static void ValidateJournal(JournalImport journal, AccountingImportRequest request)
        {
            if (journal == null)
            {
                throw new InvalidDataException(
                    "The journal importer returned no result.");
            }

            ValidateFormat(
                "accounting journal",
                journal.AccountingFormat,
                request.AccountingFormat);

            ValidateIco(
                "accounting journal",
                journal.SourceFileName,
                journal.Ico,
                request.ExpectedIco);

            ValidateFiscalYear(
                "accounting journal",
                journal.SourceFileName,
                journal.FiscalYear,
                request.ExpectedFiscalYear);

            JournalRow wrongYearRow =
                journal.Rows.FirstOrDefault(
                    row =>
                        row.PostingDate.Year !=
                        request.ExpectedFiscalYear);

            if (wrongYearRow != null)
            {
                throw new InvalidDataException(
                    "Accounting journal '" +
                    journal.SourceFileName +
                    "' contains a posting dated " +
                    wrongYearRow.PostingDate.ToString(
                        "yyyy-MM-dd") +
                    ", outside fiscal year " +
                    request.ExpectedFiscalYear +
                    ".");
            }
        }

        private static void ValidateGeneralLedger(GeneralLedgerImport ledger, JournalImport journal, AccountingImportRequest request)
        {
            if (ledger == null)
            {
                throw new InvalidDataException(
                    "The general-ledger importer returned no result.");
            }

            ValidateFormat(
                "general ledger",
                ledger.AccountingFormat,
                request.AccountingFormat);

            ValidateIco(
                "general ledger",
                ledger.SourceFileName,
                ledger.Ico,
                request.ExpectedIco);

            ValidateFiscalYear(
                "general ledger",
                ledger.SourceFileName,
                ledger.FiscalYear,
                request.ExpectedFiscalYear);

            if (!string.Equals(
                    ledger.Ico,
                    journal.Ico,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The accounting journal and general ledger " +
                    "belong to different entities: IČO '" +
                    journal.Ico +
                    "' and '" +
                    ledger.Ico +
                    "'.");
            }

            if (ledger.FiscalYear != journal.FiscalYear)
            {
                throw new InvalidDataException(
                    "The accounting journal and general ledger " +
                    "belong to different fiscal years: " +
                    journal.FiscalYear +
                    " and " +
                    ledger.FiscalYear +
                    ".");
            }
        }

        private static void ValidateFormat(string documentDescription, string actual, string expected)
        {
            if (!string.Equals(
                    actual,
                    expected,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The imported " +
                    documentDescription +
                    " reports accounting format '" +
                    actual +
                    "', but format '" +
                    expected +
                    "' was requested.");
            }
        }

        private static void ValidateIco(string documentDescription, string fileName, string actual, string expected)
        {
            if (!string.Equals(
                    actual,
                    expected,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The " +
                    documentDescription +
                    " file '" +
                    fileName +
                    "' belongs to IČO '" +
                    actual +
                    "', but IČO '" +
                    expected +
                    "' was requested.");
            }
        }

        private static void ValidateFiscalYear(string documentDescription, string fileName, int actual, int expected)
        {
            if (actual != expected)
            {
                throw new InvalidDataException(
                    "The " +
                    documentDescription +
                    " file '" +
                    fileName +
                    "' belongs to fiscal year " +
                    actual +
                    ", but fiscal year " +
                    expected +
                    " was requested.");
            }
        }

        private static string FormatStage(int? stage)
        {
            return stage.HasValue
                ? stage.Value.ToString()
                : "not specified";
        }
    }
}