using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Services.MkSoft
{
    public sealed class MkSoftWorkbookInspection
    {
        public string SourceFileName { get; set; }
        public string WorksheetName { get; set; }
        public MkSoftWorkbookKind Kind { get; set; }
        public int FieldCount { get; set; }
        public int DataRowCount { get; set; }
        public IReadOnlyList<string> Headers { get; set; }
    }
}
