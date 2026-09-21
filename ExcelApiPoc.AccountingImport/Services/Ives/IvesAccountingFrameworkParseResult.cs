using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesAccountingFrameworkParseResult
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string Ico { get; set; }
        public int FiscalYear { get; set; }
        public int SourceRowCount { get; set; }

        public List<IvesAccountingFrameworkSourceRow> Rows { get; } =
            new List<IvesAccountingFrameworkSourceRow>();
    }

    internal sealed class IvesAccountingFrameworkSourceRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRowNumber { get; set; }

        public string SourceAccountCode { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }

        public string ActivityCode { get; set; }
        public string Type { get; set; }
        public string PsFlag { get; set; }
        public string BuFlag { get; set; }
        public string RuFlag { get; set; }
        public string PlFlag { get; set; }
        public string Currency { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
