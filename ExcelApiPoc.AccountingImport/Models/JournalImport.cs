using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models
{
    public sealed class JournalImport
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileHash { get; set; }
        public string TechnicalType { get; set; }
        public string AccountingFormat { get; set; }
        public string Ico { get; set; }
        public string CompanyName { get; set; }
        public int FiscalYear { get; set; }
        public DateTime ImportedAtUtc { get; set; }
        public int NormalizedTextFieldCount { get; set; }
        public List<JournalRow> Rows { get; } = new List<JournalRow>();
        public int? ExportStage { get; set; }
    }
}