using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Services.SoftipMop;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;

namespace ExcelApiPoc.AccountingImport.Tests;

public sealed class SoftipMopPdfGeneralLedgerImporterTests
{
    [Fact]
    public void Imports_recognized_account_and_maps_all_source_values()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "SoftipMopPdfImporterTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(
            directory,
            "HL_KNIHA_31715362_2025(2).pdf");

        try
        {
            CreatePdf(path);
            var importer = new SoftipMopPdfGeneralLedgerImporter();

            Assert.True(importer.CanImport(path, "Softip-MOP"));
            var result = importer.Import(path);

            Assert.Equal("Softip-MOP", result.AccountingFormat);
            Assert.Equal("PDF", result.TechnicalType);
            Assert.Equal("31715362", result.Ico);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Equal(12, result.ThroughMonth);
            Assert.Equal("12/2025", result.PeriodHeader);
            Assert.Equal(64, result.SourceFileHash.Length);

            var row = Assert.Single(result.Rows);
            Assert.Equal(1, row.SequenceNumber);
            Assert.Equal(1, row.SourceRecordNumber);
            Assert.Equal("011", row.SyntheticCode);
            Assert.Equal("10", row.AnalyticalCode);
            Assert.Equal("01110", row.AccountCode);
            Assert.Equal("ACCOUNT NAME", row.AccountName);
            Assert.Equal(0m, row.OpeningDebit);
            Assert.Equal(100m, row.OpeningCredit);
            Assert.Equal(10m, row.PeriodDebitTurnover);
            Assert.Equal(5m, row.PeriodCreditTurnover);
            Assert.Equal(120m, row.AnnualDebitTurnover);
            Assert.Equal(20m, row.AnnualCreditTurnover);
            Assert.Equal(0m, row.ClosingDebit);
            Assert.Equal(0m, row.ClosingCredit);

            Assert.True(result.ImportReport.IsValid);
            Assert.Equal(1, result.ImportReport.RecordCounts["account-row"]);
            Assert.Equal(1, result.ImportReport.RecordCounts["LogicalRecords"]);
            Assert.Equal(8, result.ImportReport.RecordCounts["Fields"]);
            var validation = Assert.Single(
                result.ImportReport.ValidationResults);
            Assert.True(validation.IsValid);
            Assert.Equal(
                "SOFTIP_MOP.ACCOUNT.BALANCE",
                validation.Code);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Default_coordinator_routes_softip_pdf_general_ledger()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "SoftipMopPdfCoordinatorTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string ledgerPath = Path.Combine(
            directory,
            "HL_KNIHA_31715362_2025.pdf");

        try
        {
            CreatePdf(ledgerPath);

            AccountingImportPackage result =
                AccountingImportCoordinator
                    .CreateDefault()
                    .Import(
                        new AccountingImportRequest
                        {
                            AccountingFormat = "Softip-MOP",
                            JournalFilePaths = new[]
                            {
                                GetFixturePath(
                                    "Omida dennik 01 2025.xlsx")
                            },
                            GeneralLedgerFilePath = ledgerPath,
                            ExpectedIco = "31715362",
                            ExpectedFiscalYear = 2025
                        });

            Assert.Equal("Softip-MOP", result.AccountingFormat);
            Assert.NotNull(result.Journal);
            Assert.NotNull(result.GeneralLedger);
            Assert.Single(result.GeneralLedger.Rows);
            Assert.True(result.HasGeneralLedger);
            Assert.NotNull(result.JournalLedgerReconciliation);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            fileName);
    }

    private static void CreatePdf(string path)
    {
        using var writer = new PdfWriter(
            path,
            new WriterProperties().SetCompressionLevel(0));
        using var pdf = new PdfDocument(writer);
        var page = pdf.AddNewPage();
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var canvas = new PdfCanvas(page);

        Add(canvas, font,
            "Hlavna kniha k uctovnemu mesiacu 202512",
            270, 550);
        AddRight(canvas, font, "Dal", 661.5f, 500);

        Add(canvas, font, "01110", 22.5f, 450);
        Add(canvas, font, "ACCOUNT NAME", 81f, 450);
        AddRight(canvas, font, "- 100,00", 372f, 450);
        AddRight(canvas, font, "10,00", 452f, 450);
        AddRight(canvas, font, "5,00", 532f, 450);
        AddRight(canvas, font, "120,00", 611f, 450);
        AddRight(canvas, font, "20,00", 691f, 450);
        AddRight(canvas, font, "0,00", 770f, 450);

        Add(canvas, font, "© Softip a.s.", 22.5f, 50);
    }

    private static void AddRight(
        PdfCanvas canvas,
        PdfFont font,
        string text,
        float right,
        float baseline)
    {
        Add(
            canvas,
            font,
            text,
            right - font.GetWidth(text, 10),
            baseline);
    }

    private static void Add(
        PdfCanvas canvas,
        PdfFont font,
        string text,
        float left,
        float baseline)
    {
        canvas.BeginText()
            .SetFontAndSize(font, 10)
            .MoveText(left, baseline)
            .ShowText(text)
            .EndText();
    }
}
