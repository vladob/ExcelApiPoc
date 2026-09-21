using System;
using System.Collections.Generic;

namespace ExcelApiPoc.AccountingImport.Models
{
    public enum AccountingFrameworkRowKind
    {
        Empty,
        GroupHeading,
        SyntheticAccount,
        AnalyticalAccount
    }

    public sealed class AccountingFrameworkImport
    {
        public string SourceFileName { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileHash { get; set; }
        public string TechnicalType { get; set; }
        public string AccountingFormat { get; set; }
        public string Ico { get; set; }
        public string CompanyName { get; set; }
        public int FiscalYear { get; set; }
        public int? ExportStage { get; set; }
        public DateTime ImportedAtUtc { get; set; }
        public int NormalizedTextFieldCount { get; set; }

        public List<AccountingFrameworkRow> Rows { get; } =
            new List<AccountingFrameworkRow>();
    }

    public sealed class AccountingFrameworkRow
    {
        public int SequenceNumber { get; set; }
        public int SourceRecordNumber { get; set; }

        public string SourceSyntheticCode { get; set; }
        public string SourceAnalyticalCode { get; set; }

        public string SyntheticCode { get; set; }
        public string AnalyticalCode { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string Type { get; set; }

        public string SubsidiaryFlag { get; set; }
        public string TaxFlag { get; set; }
        public string BalanceFlag { get; set; }
        public string VatFlag { get; set; }

        public string ActivityCode { get; set; }
        public string PsFlag { get; set; }
        public string BuFlag { get; set; }
        public string RuFlag { get; set; }
        public string PlFlag { get; set; }
        public string Currency { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public AccountingFrameworkRowKind RowKind { get; set; }
    }
}
