using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Ives;
using ExcelApiPoc.AccountingImport.Services.SoftipMop;
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

        public AccountingImportCoordinator(
            IEnumerable<IJournalImporter> journalImporters,
            IEnumerable<IGeneralLedgerImporter> generalLedgerImporters)
        {
            if (journalImporters == null)
                throw new ArgumentNullException(nameof(journalImporters));

            if (generalLedgerImporters == null)
                throw new ArgumentNullException(nameof(generalLedgerImporters));

            this.journalImporters = journalImporters.ToList();
            this.generalLedgerImporters = generalLedgerImporters.ToList();

            if (this.journalImporters.Count == 0)
            {
                throw new ArgumentException(
                    "At least one journal importer is required.",
                    nameof(journalImporters));
            }
        }

        private readonly bool useStagedEngine;

        private AccountingImportCoordinator()
        {
            journalImporters = Array.Empty<IJournalImporter>();
            generalLedgerImporters = Array.Empty<IGeneralLedgerImporter>();
            useStagedEngine = true;
        }

        public static AccountingImportCoordinator CreateDefault() => new AccountingImportCoordinator();

        public AccountingImportPackage Import(AccountingImportRequest request)
        {
            ValidateRequest(request);

            IReadOnlyList<string> journalFilePaths = ResolveJournalFilePaths(request);
            JournalImport journal = ImportJournal(request, journalFilePaths);

            if (useStagedEngine)
            {
                if (AccountingFileNameMetadataParser.TryParse(journal.SourceFileName, out var name))
                {
                    if (string.IsNullOrWhiteSpace(journal.Ico)) journal.Ico = name.Ico;
                    if (journal.FiscalYear == 0) journal.FiscalYear = name.FiscalYear;
                    if (!journal.ExportStage.HasValue) journal.ExportStage = name.ExportStage;
                }
                AdmitMissingSoftipMopIco(journal, request);
                if (string.IsNullOrWhiteSpace(journal.Ico)) journal.Ico = request.ExpectedIco;
                if (journal.FiscalYear == 0) journal.FiscalYear = request.ExpectedFiscalYear;
            }
            ValidateJournal(journal, request, !useStagedEngine);

            CalculatedGeneralLedger calculatedGeneralLedger =
                CalculatedGeneralLedgerBuilder.Build(journal);

            GeneralLedgerImport generalLedger = null;
            JournalLedgerReconciliationResult reconciliation = null;

            if (!string.IsNullOrWhiteSpace(request.GeneralLedgerFilePath))
            {
                if (useStagedEngine)
                    generalLedger = Layouts.StagedImportRuntime.Import<GeneralLedgerImport>(
                        request.GeneralLedgerFilePath, request.AccountingFormat, "GL", request.ExpectedFiscalYear);
                else
                {
                    var ledgerImporter = SelectExactlyOne(generalLedgerImporters,
                        importer => importer.CanImport(request.GeneralLedgerFilePath, request.AccountingFormat),
                        "general-ledger", request.GeneralLedgerFilePath, request.AccountingFormat);
                    generalLedger = ledgerImporter.Import(request.GeneralLedgerFilePath);
                }

                if (useStagedEngine)
                {
                    if (AccountingFileNameMetadataParser.TryParse(generalLedger.SourceFileName, out var name))
                    {
                        if (string.IsNullOrWhiteSpace(generalLedger.Ico)) generalLedger.Ico = name.Ico;
                        if (generalLedger.FiscalYear == 0) generalLedger.FiscalYear = name.FiscalYear;
                        if (!generalLedger.ExportStage.HasValue) generalLedger.ExportStage = name.ExportStage;
                    }
                    if (string.IsNullOrWhiteSpace(generalLedger.Ico)) generalLedger.Ico = request.ExpectedIco;
                    if (generalLedger.FiscalYear == 0) generalLedger.FiscalYear = request.ExpectedFiscalYear;
                }
                ValidateGeneralLedger(generalLedger, journal, request, !useStagedEngine);

                reconciliation = JournalLedgerReconciliationService.Reconcile(
                    journal,
                    generalLedger);
            }

            return new AccountingImportPackage
            {
                AccountingFormat = journal.AccountingFormat,
                Ico = journal.Ico,
                FiscalYear = journal.FiscalYear,
                ExportStage = journal.ExportStage,
                Journal = journal,
                GeneralLedger = generalLedger,
                CalculatedGeneralLedger = calculatedGeneralLedger,
                JournalLedgerReconciliation = reconciliation
            };
        }

        private JournalImport ImportJournal(
            AccountingImportRequest request,
            IReadOnlyList<string> journalFilePaths)
        {
            if (IsSoftipMop(request.AccountingFormat) && journalFilePaths.All(p => Path.GetExtension(p).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)))
            {
                return new SoftipMopMonthlyJournalImporter()
                    .Import(journalFilePaths, useStagedEngine ? Layouts.StagedImportRuntime.ProducerDirectory("Softip-MOP") : null);
            }

            if (journalFilePaths.Count > 1)
                return ImportCombinedJournals(request, journalFilePaths);

            string journalFilePath = journalFilePaths[0];
            if (useStagedEngine)
                return Layouts.StagedImportRuntime.Import<JournalImport>(journalFilePath, request.AccountingFormat, "AJ", request.ExpectedFiscalYear);
            IJournalImporter journalImporter = SelectExactlyOne(
                journalImporters,
                importer => importer.CanImport(
                    journalFilePath,
                    request.AccountingFormat),
                "accounting-journal",
                journalFilePath,
                request.AccountingFormat);

            return journalImporter.Import(journalFilePath);
        }

        private JournalImport ImportCombinedJournals(AccountingImportRequest request, IReadOnlyList<string> paths)
        {
            var result = new JournalImport
            {
                SourceFileName = paths.Count + " accounting journal files",
                AccountingFormat = request.AccountingFormat,
                Ico = request.ExpectedIco,
                FiscalYear = request.ExpectedFiscalYear,
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = new ImportReport { AccountingFormat = request.AccountingFormat,
                    ImportType = "AccountingJournal", SourceFileName = paths.Count + " accounting journal files" }
            };
            var hashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fullPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                if (!fullPaths.Add(Path.GetFullPath(path)))
                    throw new InvalidDataException("The same journal file was selected more than once: " + Path.GetFileName(path));
                string hash;
                using (var sha = System.Security.Cryptography.SHA256.Create())
                using (var input = File.OpenRead(path))
                    hash = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "");
                if (!hashes.Add(hash))
                    throw new InvalidDataException("Duplicate journal file content: " + Path.GetFileName(path));
                // Import through the same single-file path, including identity/year and
                // printed controls. Never merge unvalidated source rows.
                var part = Import(new AccountingImportRequest
                {
                    AccountingFormat = request.AccountingFormat, JournalFilePath = path,
                    ExpectedIco = request.ExpectedIco, ExpectedFiscalYear = request.ExpectedFiscalYear
                }).Journal;
                JournalImportCapacity.EnsureCanAppend(result.Rows.Count, part.Rows.Count, part.SourceFileName);
                result.TechnicalType = result.TechnicalType == null ? part.TechnicalType :
                    result.TechnicalType == part.TechnicalType ? result.TechnicalType : "Mixed";
                if (result.CompanyName == null) result.CompanyName = part.CompanyName;
                result.NormalizedTextFieldCount += part.NormalizedTextFieldCount;
                foreach (var row in part.Rows)
                {
                    row.SourceFields["SourceFile"] = part.SourceFileName;
                    row.SourceFields["SourceFileHash"] = hash;
                    row.SourceFields["SourceFilePath"] = Path.GetFullPath(path);
                    row.SourceLocation = part.SourceFileName + ", " + row.SourceLocation;
                    row.SequenceNumber = result.Rows.Count + 1;
                    result.Rows.Add(row);
                }
                if (part.ImportReport != null)
                {
                    foreach (var count in part.ImportReport.RecordCounts)
                    {
                        result.ImportReport.RecordCounts.TryGetValue(count.Key, out int existing);
                        result.ImportReport.RecordCounts[count.Key] = existing + count.Value;
                    }
                    result.ImportReport.ValidationResults.AddRange(part.ImportReport.ValidationResults);
                    foreach (var diagnostic in part.ImportReport.Diagnostics)
                    {
                        if (diagnostic.Source == null) diagnostic.Source = new SourceProvenance { SourceFileName = part.SourceFileName };
                        result.ImportReport.Diagnostics.Add(diagnostic);
                    }
                }
            }
            using (var sha = System.Security.Cryptography.SHA256.Create())
                result.SourceFileHash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(
                    string.Join("\n", hashes.OrderBy(h => h, StringComparer.Ordinal))))).Replace("-", "");
            result.ImportReport.RecordCounts["SourceFiles"] = paths.Count;
            result.ImportReport.RecordCounts["Transactions"] = result.Rows.Count;
            return result;
        }

        private static TImporter SelectExactlyOne<TImporter>(
            IEnumerable<TImporter> importers,
            Func<TImporter, bool> canImport,
            string documentDescription,
            string filePath,
            string accountingFormat)
        {
            List<TImporter> matches = importers.Where(canImport).ToList();

            if (matches.Count == 0)
            {
                throw new InvalidDataException(
                    "No registered importer recognizes the " +
                    documentDescription +
                    " file '" + Path.GetFileName(filePath) +
                    "' as accounting format '" + accountingFormat + "'.");
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    "More than one registered importer recognizes the " +
                    documentDescription +
                    " file '" + Path.GetFileName(filePath) +
                    "' as accounting format '" + accountingFormat +
                    "'. Importer selection is ambiguous.");
            }

            return matches[0];
        }

        private static void ValidateRequest(AccountingImportRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.AccountingFormat))
            {
                throw new ArgumentException(
                    "An accounting format is required.",
                    nameof(request));
            }

            IReadOnlyList<string> journalFilePaths = ResolveJournalFilePaths(request);

            if (journalFilePaths.Count == 0)
            {
                throw new ArgumentException(
                    "An accounting-journal file is required.",
                    nameof(request));
            }

            if (journalFilePaths.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "An accounting-journal file path is empty.",
                    nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.ExpectedIco))
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

        private static void ValidateJournal(
            JournalImport journal,
            AccountingImportRequest request, bool useFilename = true)
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

            if (useFilename) ReconcileJournalFileNameMetadata(journal);
            AdmitMissingSoftipMopIco(journal, request);

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

            JournalDateExceptionService.Apply(
                journal,
                request.ExpectedFiscalYear);
        }

        private static void ReconcileJournalFileNameMetadata(
            JournalImport journal)
        {
            if (!AccountingFileNameMetadataParser.TryParse(
                    journal.SourceFileName,
                    out AccountingFileNameMetadata metadata))
            {
                return;
            }

            if (metadata.DocumentKind !=
                AccountingSourceDocumentKind.AccountingJournal)
            {
                throw new InvalidDataException(
                    "Filename '" + journal.SourceFileName +
                    "' does not identify an accounting journal.");
            }

            journal.Ico = AccountingFileNameMetadataParser.ResolveIco(
                journal.SourceFileName,
                journal.Ico,
                metadata);
            journal.FiscalYear =
                AccountingFileNameMetadataParser.ResolveFiscalYear(
                    journal.SourceFileName,
                    journal.FiscalYear,
                    metadata);

            if (!journal.ExportStage.HasValue)
                journal.ExportStage = metadata.ExportStage;
        }

        private static void ReconcileGeneralLedgerFileNameMetadata(
            GeneralLedgerImport ledger)
        {
            if (!AccountingFileNameMetadataParser.TryParse(
                    ledger.SourceFileName,
                    out AccountingFileNameMetadata metadata))
            {
                return;
            }

            if (metadata.DocumentKind !=
                AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "Filename '" + ledger.SourceFileName +
                    "' does not identify a general ledger.");
            }

            ledger.Ico = AccountingFileNameMetadataParser.ResolveIco(
                ledger.SourceFileName,
                ledger.Ico,
                metadata);
            ledger.FiscalYear =
                AccountingFileNameMetadataParser.ResolveFiscalYear(
                    ledger.SourceFileName,
                    ledger.FiscalYear,
                    metadata);

            if (!ledger.ExportStage.HasValue)
                ledger.ExportStage = metadata.ExportStage;
        }

        private static void AdmitMissingSoftipMopIco(
            JournalImport journal,
            AccountingImportRequest request)
        {
            if (!IsSoftipMop(request.AccountingFormat) ||
                !string.IsNullOrWhiteSpace(journal.Ico))
            {
                return;
            }

            journal.Ico = request.ExpectedIco;
            journal.ImportReport?.Diagnostics.Add(
                new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP_ICO_SUPPLIED_BY_REQUEST",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = "The Softip-MOP accounting journal does not contain IČO. " +
                        "IČO '" + request.ExpectedIco +
                        "' was supplied by the import request and could not be verified from the source files.",
                    Source = new SourceProvenance
                    {
                        SourceFileName = journal.SourceFileName,
                        RecordSet = "ImportRequest"
                    }
                });
        }

        private static IReadOnlyList<string> ResolveJournalFilePaths(
            AccountingImportRequest request)
        {
            if (request.JournalFilePaths != null &&
                request.JournalFilePaths.Count > 0)
            {
                return request.JournalFilePaths;
            }

            if (string.IsNullOrWhiteSpace(request.JournalFilePath))
                return Array.Empty<string>();

            return new[] { request.JournalFilePath };
        }

        private static bool IsSoftipMop(string accountingFormat)
        {
            return string.Equals(
                accountingFormat,
                "Softip-MOP",
                StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateGeneralLedger(
            GeneralLedgerImport ledger,
            JournalImport journal,
            AccountingImportRequest request, bool useFilename = true)
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

            if (useFilename) ReconcileGeneralLedgerFileNameMetadata(ledger);

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
                    journal.Ico + "' and '" + ledger.Ico + "'.");
            }

            if (ledger.FiscalYear != journal.FiscalYear)
            {
                throw new InvalidDataException(
                    "The accounting journal and general ledger " +
                    "belong to different fiscal years: " +
                    journal.FiscalYear + " and " +
                    ledger.FiscalYear + ".");
            }
        }

        private static void ValidateFormat(
            string documentDescription,
            string actual,
            string expected)
        {
            if (!string.Equals(
                    actual,
                    expected,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The imported " + documentDescription +
                    " reports accounting format '" + actual +
                    "', but format '" + expected + "' was requested.");
            }
        }

        private static void ValidateIco(
            string documentDescription,
            string fileName,
            string actual,
            string expected)
        {
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The " + documentDescription +
                    " file '" + fileName +
                    "' belongs to IČO '" + actual +
                    "', but IČO '" + expected + "' was requested.");
            }
        }

        private static void ValidateFiscalYear(
            string documentDescription,
            string fileName,
            int actual,
            int expected)
        {
            if (actual != expected)
            {
                throw new InvalidDataException(
                    "The " + documentDescription +
                    " file '" + fileName +
                    "' belongs to fiscal year " + actual +
                    ", but fiscal year " + expected +
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
