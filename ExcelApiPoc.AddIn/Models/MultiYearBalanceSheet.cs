using System.Collections.Generic;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class MultiYearBalanceSheet
    {
        public AccountingEntityDto Entity { get; set; }

        public string TemplateName { get; set; }

        public IReadOnlyList<int> FiscalYears { get; set; }

        public IReadOnlyList<MultiYearBalanceSheetRow> Rows { get; set; }
    }

    internal sealed class MultiYearBalanceSheetRow
    {
        public string Designation { get; set; }

        public string Description { get; set; }

        public int RowNumber { get; set; }

        public bool IsSumRow { get; set; }

        public int? HasData { get; set; }

        public IReadOnlyDictionary<int, decimal> ValuesByFiscalYear { get; set; }
    }
}
