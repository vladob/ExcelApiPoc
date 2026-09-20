using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.Common;

namespace ExcelApiPoc.AccountingImport.Tests.Common;

public sealed class JournalDateExceptionServiceTests
{
    [Fact]
    public void Apply_MarksOutOfYearRowsExcludedAndAddsWarning()
    {
        var journal = new JournalImport
        {
            SourceFileName = "U_DENNIK_00322792_202414.CSV",
            FiscalYear = 2024,
            ImportReport = new ImportReport()
        };
        journal.Rows.Add(new JournalRow
        {
            SequenceNumber = 1,
            PostingDate = new DateTime(2024, 12, 31)
        });
        journal.Rows.Add(new JournalRow
        {
            SequenceNumber = 2,
            PostingDate = new DateTime(3025, 3, 12)
        });

        int count = JournalDateExceptionService.Apply(journal, 2024);

        Assert.Equal(1, count);
        Assert.Null(journal.Rows[0].DateExceptionResolution);
        Assert.Equal(
            JournalDateExceptionResolution.Excluded,
            journal.Rows[1].DateExceptionResolution);
        Assert.Null(journal.Rows[1].CorrectedPostingDate);
        Assert.False(journal.Rows[1].UsedForReportCalculation);

        ImportDiagnostic warning = Assert.Single(
            journal.ImportReport.Diagnostics,
            diagnostic =>
                diagnostic.Code == JournalDateExceptionService.DiagnosticCode);
        Assert.Equal(ImportDiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("3025-03-12", warning.Message);
    }

    [Fact]
    public void Apply_DoesNotOverwriteExistingAuditorResolution()
    {
        var journal = new JournalImport
        {
            FiscalYear = 2024
        };
        journal.Rows.Add(new JournalRow
        {
            PostingDate = new DateTime(2025, 1, 3),
            DateExceptionResolution =
                JournalDateExceptionResolution.OriginalIncluded
        });

        JournalDateExceptionService.Apply(journal, 2024);

        Assert.Equal(
            JournalDateExceptionResolution.OriginalIncluded,
            journal.Rows[0].DateExceptionResolution);
        Assert.True(journal.Rows[0].UsedForReportCalculation);
    }

    [Fact]
    public void ModifiedIncluded_PreservesOriginalAndUsesCorrectedEffectiveDate()
    {
        var row = new JournalRow
        {
            PostingDate = new DateTime(3025, 3, 12),
            DateExceptionResolution =
                JournalDateExceptionResolution.ModifiedIncluded,
            CorrectedPostingDate = new DateTime(2025, 3, 12)
        };

        Assert.Equal(new DateTime(3025, 3, 12), row.PostingDate);
        Assert.Equal(new DateTime(2025, 3, 12), row.EffectivePostingDate);
        Assert.True(row.UsedForReportCalculation);
    }

    [Fact]
    public void CalculatedGeneralLedger_ExcludesDateExceptionRows()
    {
        var journal = new JournalImport
        {
            Ico = "00322792",
            FiscalYear = 2024
        };
        journal.Rows.Add(new JournalRow
        {
            PostingDate = new DateTime(2024, 1, 1),
            RecordKind = JournalRecordKind.Opening,
            DebitAccount = "221",
            DebitAmount = 100m,
            CreditAccount = "701",
            CreditAmount = 100m
        });
        journal.Rows.Add(new JournalRow
        {
            PostingDate = new DateTime(3025, 3, 12),
            DateExceptionResolution = JournalDateExceptionResolution.Excluded,
            RecordKind = JournalRecordKind.Normal,
            DebitAccount = "221",
            DebitAmount = 999m,
            CreditAccount = "321",
            CreditAmount = 999m
        });

        CalculatedGeneralLedger result =
            CalculatedGeneralLedgerBuilder.Build(journal);

        CalculatedGeneralLedgerRow bank = Assert.Single(
            result.Rows,
            row => row.AccountCode == "221");
        Assert.Equal(100m, bank.OpeningDebit);
        Assert.Equal(0m, bank.DebitTurnover);
        Assert.DoesNotContain(result.Rows, row => row.AccountCode == "321");
    }
}
