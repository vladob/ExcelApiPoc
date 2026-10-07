using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Simple;

namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    public sealed class ImportMetadataCandidate
    {
        public string Path { get; set; }
        public string Category { get; set; }
        public string Ico { get; set; }
        public int? FiscalYear { get; set; }
        public string Producer { get; set; }
    }

    public static class ImportMetadataResolver
    {
        // Caller supplies AJ(s), GL, AF in that order. Resolve each value independently.
        public static ImportMetadataCandidate Resolve(IEnumerable<ImportMetadataCandidate> source)
        {
            var candidates = source.ToArray();
            var result = new ImportMetadataCandidate();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(result.Ico) && !string.IsNullOrWhiteSpace(candidate.Ico)) result.Ico = candidate.Ico;
                if (!result.FiscalYear.HasValue && candidate.FiscalYear > 0) result.FiscalYear = candidate.FiscalYear;
                if (string.IsNullOrWhiteSpace(result.Producer)) result.Producer = candidate.Producer;
            }
            foreach (var candidate in candidates)
                if (AccountingFileNameMetadataParser.TryParse(candidate.Path, out var name))
                {
                    if (string.IsNullOrWhiteSpace(result.Ico)) result.Ico = name.Ico;
                    if (!result.FiscalYear.HasValue) result.FiscalYear = name.FiscalYear;
                }
            return result;
        }

        public static ImportMetadataCandidate Read(string path, string category, string producer = null)
        {
            var candidate = new ImportMetadataCandidate { Path = path, Category = category };
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return candidate;
            try
            {
                var catalogue = string.IsNullOrWhiteSpace(producer) || producer == "Unknown"
                    ? StagedImportRuntime.CatalogueDirectory : StagedImportRuntime.ProducerDirectory(producer);
                using (var importer = new CompactLayoutImporter(path, catalogue))
                {
                    var result = importer.Examine(ImportLevel.Identify);
                    if (result.CompletedLevel < 2 || result.Category != category) return candidate;
                    result.Identifiers.TryGetValue("cin", out var ico);
                    result.Identifiers.TryGetValue("fiscalYear", out var year);
                    candidate.Ico = ico;
                    candidate.FiscalYear = int.TryParse(year, out var parsed) && parsed >= 1900 ? (int?)parsed : null;
                    candidate.Producer = result.Producer == "Ives" ? "IVES" : result.Producer;
                }
            }
            catch (AmbiguousLayoutException) { /* Producer selection is required; filenames still supply metadata. */ }
            catch (NotSupportedException) { }
            return candidate;
        }
    }
}
