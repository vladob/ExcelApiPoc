using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
namespace AccountBalanceVerification.Import
{
    public sealed class FileMetadata
    {
        public string Ico { get; set; }
        public int? FiscalYear { get; set; }
        public string EntityName { get; set; }
        public string Producer { get; set; }
        public List<string> Warnings { get; } = new List<string>();

        public static FileMetadata Read(string journal, string chart, string ledger, string producer)
        {
            var result = new FileMetadata();
            var names = new List<AccountingFileNameMetadata>();
            var paths = new[] { journal, ledger, chart };
            var categories = new[] { "AJ", "GL", "AF" };
            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (string.IsNullOrWhiteSpace(path)) continue;
                // Browser download suffixes are not part of the accounting filename.
                var normalized = Regex.Replace(Path.GetFileNameWithoutExtension(path), @"\s*\(\d+\)$", "") + Path.GetExtension(path);
                if (AccountingFileNameMetadataParser.TryParse(normalized, out var name)) names.Add(name);
                if (!File.Exists(path)) { result.Warnings.Add("Súbor neexistuje: " + Path.GetFileName(path)); continue; }
                try
                {
                    var catalogue = string.IsNullOrWhiteSpace(producer) ? StagedImportRuntime.CatalogueDirectory : StagedImportRuntime.ProducerDirectory(producer);
                    using (var importer = new CompactLayoutImporter(path, catalogue))
                    {
                        var data = importer.Examine(ImportLevel.Identify);
                        if (data.CompletedLevel < 2 || data.Category != categories[i])
                        { result.Warnings.Add("Nerozpoznaný typ súboru: " + Path.GetFileName(path)); continue; }
                        data.Identifiers.TryGetValue("cin", out var ico);
                        data.Identifiers.TryGetValue("fiscalYear", out var year);
                        data.Identifiers.TryGetValue("AccountingEntityName", out var entity);
                        if (string.IsNullOrWhiteSpace(entity)) data.Identifiers.TryGetValue("entityName", out entity);
                        result.Ico = Merge(result.Ico, ico, "IČO", result.Warnings);
                        if (int.TryParse(year, out var y) && y >= 1900 && y <= 9999)
                        {
                            if (result.FiscalYear.HasValue && result.FiscalYear != y) result.Warnings.Add("Súbory obsahujú rozdielne účtovné roky.");
                            else result.FiscalYear = y;
                        }
                        result.EntityName = Merge(result.EntityName, entity, "názov účtovnej jednotky", result.Warnings);
                        result.Producer = Merge(result.Producer, data.Producer == "Ives" ? "IVES" : data.Producer == "KROS OMEGA" || data.Producer == "Omega" ? "OMEGA" : data.Producer, "účtovný systém", result.Warnings);
                    }
                }
                catch (Exception ex) { result.Warnings.Add(Path.GetFileName(path) + ": " + ex.Message); }
            }
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(result.Ico)) result.Ico = name.Ico;
                else if (result.Ico != name.Ico) result.Warnings.Add("IČO v názve súboru nesúhlasí s načítanými údajmi: " + name.FileName);
                if (!result.FiscalYear.HasValue) result.FiscalYear = name.FiscalYear;
                else if (result.FiscalYear != name.FiscalYear) result.Warnings.Add("Rok v názve súboru nesúhlasí s načítanými údajmi: " + name.FileName);
            }
            return result;
        }
        private static string Merge(string current, string next, string label, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(next)) return current;
            next = next.Trim();
            if (string.IsNullOrWhiteSpace(current)) return next;
            if (current != next) warnings.Add("Súbory obsahujú rozdielne údaje: " + label + ".");
            return current;
        }
    }
}
