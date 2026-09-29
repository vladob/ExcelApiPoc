using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.IfoSoft;
using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Tests.Common;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Tests.IfoSoft;

public sealed class IfoSoftCsvJournalImporterTests
{
    [Fact]
    public void Imports_ladomirov_2025_csv_with_undated_entry_excluded()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_CSV_JOURNAL_LADOMIROV_2025_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_CSV_JOURNAL_LADOMIROV_2025_TEST_FILE to U_DENNIK_00323195_2025.CSV.");
        JournalImport import = new IfoSoftCsvJournalImporter().Import(path!);
        Assert.Equal("00323195", import.Ico);
        Assert.Equal(2025, import.FiscalYear);
        Assert.Equal(2142, import.Rows.Count);
        Assert.Equal(1638050.39m, import.Rows.Sum(row => row.DebitAmount ?? 0m));
        Assert.Equal(1638050.39m, import.Rows.Sum(row => row.CreditAmount ?? 0m));
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_CSV_MISSING_POSTING_DATE" &&
            d.Message.Contains("1 CSV journal row"));
        JournalRow undated = Assert.Single(import.Rows.Where(row => row.PostingDate == DateTime.MinValue));
        Assert.Equal(351, undated.SourceStartLineNumber);
        Assert.Equal("221", undated.DebitAccount);
        Assert.Equal("261", undated.CreditAccount);
        Assert.Equal(1266m, undated.DebitAmount);
        Assert.Equal(1266m, undated.CreditAmount);
        Assert.Equal(JournalDateExceptionResolution.Excluded, undated.DateExceptionResolution);
        Assert.False(undated.UsedForReportCalculation);
        Assert.Equal(11, JournalDateExceptionService.Apply(import, 2025));
    }

    private static readonly string[] Headers =
    [
        "DD", "CisloD", "Datum", "Popis operacie", "Ucet_MD",
        "Pol_MD", "Zdr_MD", "Str_MD", "Zak_MD", "Suma_MD",
        "Ucet_D", "Odd_D", "Pol_D", "Zdr_D", "Str_D", "Zak_D",
        "Suma_D"
    ];

    [Fact]
    public void Detector_ReturnsIfoSoftMetadata()
    {
        using var file = CreateValidJournal();

        bool detected = IfoSoftCsvJournalDetector.TryDetect(
            file.Path,
            out JournalDetectionResult result);

        Assert.True(detected);
        Assert.Equal("CSV", result.TechnicalType);
        Assert.Equal("IfoSoft", result.AccountingFormat);
        Assert.Equal("00325791", result.Ico);
        Assert.Equal("Mesto Sobrance", result.CompanyName);
        Assert.Equal(2024, result.FiscalYear);
    }

    [Fact]
    public void Import_MapsRowsAndPreservesRecordSemantics()
    {
        using var file = CreateValidJournal();

        JournalImport result = new IfoSoftCsvJournalImporter().Import(file.Path);

        Assert.Equal("00325791", result.Ico);
        Assert.Equal("Mesto Sobrance", result.CompanyName);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(3, result.Rows.Count);

        JournalRow opening = result.Rows[0];
        Assert.Equal(JournalRecordKind.Opening, opening.RecordKind);
        Assert.True(opening.UsedForReportCalculation);
        Assert.Equal("701", opening.DebitAccount);
        Assert.Equal(100.25m, opening.DebitAmount);
        Assert.Equal("22101", opening.CreditAccount);
        Assert.Equal(100.25m, opening.CreditAmount);
        Assert.Equal(4, opening.SourceStartLineNumber);
        Assert.Equal("Line 4", opening.SourceLocation);

        JournalRow normal = result.Rows[1];
        Assert.Equal(JournalRecordKind.Normal, normal.RecordKind);
        Assert.Equal("B2/1", normal.DocumentNumber);
        Assert.Equal("51813", normal.DebitAccount);
        Assert.Equal("32102", normal.CreditAccount);
        Assert.Equal(66m, normal.DebitAmount);
        Assert.Equal(66m, normal.CreditAmount);

        JournalRow closing = result.Rows[2];
        Assert.Equal(JournalRecordKind.Closing, closing.RecordKind);
        Assert.False(closing.UsedForReportCalculation);
    }

    [Fact]
    public void Import_ReportsHeaderPositionWhenStructureIsInvalid()
    {
        string[] invalidHeaders = (string[])Headers.Clone();
        invalidHeaders[4] = "WrongAccount";
        using var file = new TemporaryCsvFile(
        [
            Quote("00325791 Mesto Sobrance"),
            Quote("Uctovny dennik"),
            Csv(invalidHeaders),
            JournalRowLine("ID", "1", "02.01.2024", "Movement", "51813", "10,00", "32102", "10,00")
        ]);

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => new IfoSoftCsvJournalImporter().Import(file.Path));

        Assert.Contains("not a recognized IfoSoft accounting journal", exception.Message);
    }

    [Fact]
    public void Import_Kamienka_export_with_timestamp_header_and_filename_ico()
    {
        string? path = Environment.GetEnvironmentVariable("IFOSOFT_CSV_KAMIENKA_JOURNAL_TEST_FILE");
        Assert.True(!string.IsNullOrWhiteSpace(path) && File.Exists(path),
            "Set IFOSOFT_CSV_KAMIENKA_JOURNAL_TEST_FILE to U_DENNIK_00323110_2024.CSV.");
        Assert.True(IfoSoftCsvJournalDetector.TryDetect(path!, out JournalDetectionResult detection));
        Assert.Equal("00323110", detection.Ico);
        JournalImport import = new IfoSoftCsvJournalImporter().Import(path!);
        Assert.Equal("00323110", import.Ico);
        Assert.Equal("OBEC KAMIENKA", import.CompanyName);
        Assert.Equal(2024, import.FiscalYear);
        Assert.Contains(import.ImportReport.Diagnostics, d => d.Code == "IFOSOFT_CSV_ICO_FROM_FILENAME");
        Assert.NotEmpty(import.Rows);
    }

    private static TemporaryCsvFile CreateValidJournal()
    {
        return new TemporaryCsvFile(
        [
            Quote("00325791 Mesto Sobrance"),
            Quote("Uctovny dennik"),
            Csv(Headers),
            JournalRowLine("ID", "1", "01.01.2024", "Opening", "701", "100,25", "22101", "100,25"),
            JournalRowLine("BV", "B2/1", "02.01.2024", "Service", "51813", "66,00", "32102", "66,00"),
            JournalRowLine("ID", "999999", "31.12.2024", "Uzatvorenie účtovných kníh", "702", "166,25", "710", "166,25")
        ]);
    }

    private static string JournalRowLine(
        string documentType,
        string documentNumber,
        string date,
        string description,
        string debitAccount,
        string debitAmount,
        string creditAccount,
        string creditAmount)
    {
        return Csv(
            documentType, documentNumber, date, description, debitAccount,
            "", "", "", "", debitAmount, creditAccount, "", "", "",
            "", "", creditAmount);
    }

    private static string Csv(params string[] fields)
    {
        return string.Join(";", fields.Select(Quote));
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
