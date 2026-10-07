using System;
using System.IO;
using System.Linq;
using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using PdfLayoutEngine.Simple;

namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    /// <summary>The AddIn and release tests share this entry point and embedded catalogue.</summary>
    public static class StagedImportRuntime
    {
        private static readonly Lazy<string> Catalogue = new Lazy<string>(ExtractCatalogue);
        public static string CatalogueDirectory => Catalogue.Value;

        public static string ProducerDirectory(string producer)
        {
            string folder;
            switch ((producer ?? "").ToUpperInvariant())
            {
                case "IFOSOFT": folder = "IfoSoft"; break;
                case "IVES": folder = "Ives"; break;
                case "URBIS": folder = "Urbis"; break;
                case "SOFTIP-MOP": folder = "SoftipMop"; break;
                case "KROS OMEGA": case "OMEGA": folder = "Omega"; break;
                default: throw new NotSupportedException("No staged layouts are available for '" + producer + "'.");
            }
            return Path.Combine(CatalogueDirectory, folder, "Compact");
        }

        private static string ExtractCatalogue()
        {
            const string prefix = "StagedLayouts/";
            var assembly = typeof(StagedImportRuntime).Assembly;
            var resources = assembly.GetManifestResourceNames().Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            if (resources.Length == 0) throw new InvalidDataException("The import assembly contains no staged layouts.");
            string root = Path.Combine(Path.GetTempPath(), "ExcelApiPoc", "layouts-" + Guid.NewGuid().ToString("N"));
            try
            {
                foreach (string name in resources)
                {
                    string target = Path.Combine(root, name.Substring(prefix.Length).Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    using (var input = assembly.GetManifestResourceStream(name))
                    using (var output = File.Create(target)) input.CopyTo(output);
                }
                LayoutFiles.Validate(root);
                AppDomain.CurrentDomain.ProcessExit += (sender, args) => { try { Directory.Delete(root, true); } catch { } };
                return root;
            }
            catch { if (Directory.Exists(root)) Directory.Delete(root, true); throw; }
        }

        public static bool TryDetectJournal(string path, out JournalDetectionResult detection)
        {
            detection = null;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
            try
            {
                using (var importer = new CompactLayoutImporter(path, CatalogueDirectory))
                {
                    var result = importer.Examine(ImportLevel.Identify);
                    if (result.Status != "completed" || result.Category != "AJ") return false;
                    result.Identifiers.TryGetValue("cin", out string ico);
                    result.Identifiers.TryGetValue("fiscalYear", out string year);
                    detection = new JournalDetectionResult
                    {
                        AccountingFormat = string.Equals(result.Producer, "Ives", StringComparison.OrdinalIgnoreCase) ? "IVES" : result.Producer,
                        TechnicalType = new[] { ".xls", ".xlsx" }.Contains(Path.GetExtension(path).ToLowerInvariant()) ? "Excel" : result.Format,
                        Ico = ico,
                        FiscalYear = int.TryParse(year, out int parsed) && parsed >= 1900 ? (int?)parsed : null
                    };
                    return true;
                }
            }
            // A producer must be selected explicitly when content cannot distinguish it.
            catch (AmbiguousLayoutException) { return false; }
            catch (NotSupportedException) { return false; }
        }

        public static T Import<T>(string path, string producer, string category) where T : class
        {
            using (var importer = new CompactLayoutImporter(path, ProducerDirectory(producer)))
            {
                var result = importer.Examine(ImportLevel.Normalize);
                if (result.Status != "completed" || result.CompletedLevel != 5 || result.Category != category || !(importer.Canonical is T canonical))
                    throw new InvalidDataException(Path.GetFileName(path) + ": staged " + category + " import " + result.Status + ". " + string.Join("; ", result.Issues.Select(i => i.Code + ": " + i.Message)));
                var report = new ImportReport { AccountingFormat = producer, ImportType = category, SourceFileName = Path.GetFileName(path) };
                report.Diagnostics.Add(new ImportDiagnostic { Code = "STAGED_LAYOUT", Severity = ImportDiagnosticSeverity.Information, Message = "Staged engine: " + result.LayoutId });
                foreach (var issue in result.Issues)
                    report.Diagnostics.Add(new ImportDiagnostic { Code = issue.Code, Message = issue.Message, Severity = issue.Severity == "error" ? ImportDiagnosticSeverity.Error : issue.Severity == "warning" ? ImportDiagnosticSeverity.Warning : ImportDiagnosticSeverity.Information });
                if (canonical is JournalImport journal) { journal.ImportReport = report; journal.AccountingFormat = producer; }
                if (canonical is GeneralLedgerImport ledger) { ledger.ImportReport = report; ledger.AccountingFormat = producer; }
                if (canonical is AccountingFrameworkImport framework) { framework.ImportReport = report; framework.AccountingFormat = producer; }
                return canonical;
            }
        }
    }
}
