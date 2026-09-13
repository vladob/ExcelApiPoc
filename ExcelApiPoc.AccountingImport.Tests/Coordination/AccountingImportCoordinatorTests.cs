using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Tests.Coordination;

public sealed class AccountingImportCoordinatorTests
{
    [Fact]
    public void Import_SelectsIvesJournalWithoutGeneralLedger()
    {
        AccountingImportPackage result =
            AccountingImportCoordinator
                .CreateDefault()
                .Import(
                    new AccountingImportRequest
                    {
                        AccountingFormat = "IVES",
                        JournalFilePath =
                            GetIvesFixturePath(
                                "U_DENNIK_00322881_2024.xls"),
                        ExpectedIco = "00322881",
                        ExpectedFiscalYear = 2024
                    });

        Assert.Equal("IVES", result.AccountingFormat);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Null(result.ExportStage);
        Assert.NotNull(result.Journal);
        Assert.Equal(2125, result.Journal.Rows.Count);
        Assert.NotNull(result.Journal.ImportReport);
        Assert.True(result.Journal.ImportReport.IsValid);
        Assert.Null(result.GeneralLedger);
        Assert.False(result.HasGeneralLedger);
        Assert.Null(result.JournalLedgerReconciliation);
    }

    [Fact]
    public void Import_LoadsAndReconcilesMatchingIvesJournalAndLedger()
    {
        AccountingImportPackage result =
            AccountingImportCoordinator
                .CreateDefault()
                .Import(
                    new AccountingImportRequest
                    {
                        AccountingFormat = "IVES",
                        JournalFilePath =
                            GetIvesFixturePath(
                                "U_DENNIK_00322881_2024.xls"),
                        GeneralLedgerFilePath =
                            GetIvesFixturePath(
                                "HL_KNIHA_00322881_2024.xls"),
                        ExpectedIco = "00322881",
                        ExpectedFiscalYear = 2024
                    });

        Assert.Equal("IVES", result.AccountingFormat);
        Assert.Equal("00322881", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Null(result.ExportStage);

        Assert.NotNull(result.Journal);
        Assert.Equal(2125, result.Journal.Rows.Count);
        Assert.True(result.Journal.ImportReport.IsValid);

        Assert.NotNull(result.GeneralLedger);
        Assert.Equal(320, result.GeneralLedger.Rows.Count);
        Assert.True(result.GeneralLedger.ImportReport.IsValid);
        Assert.True(result.HasGeneralLedger);

        JournalLedgerReconciliationResult reconciliation =
            Assert.IsType<JournalLedgerReconciliationResult>(
                result.JournalLedgerReconciliation);

        Assert.True(reconciliation.IsReconciled);
        Assert.False(reconciliation.JournalContainsOpeningRecords);
        Assert.False(reconciliation.JournalContainsClosingRecords);
        Assert.Equal(
            OpeningBalanceSource.GeneralLedger,
            reconciliation.OpeningBalanceSource);
        Assert.Equal(264, reconciliation.JournalAccountCount);
        Assert.Equal(320, reconciliation.LedgerAccountCount);
        Assert.Equal(0, reconciliation.JournalOnlyAccountCount);
        Assert.Equal(56, reconciliation.LedgerOnlyAccountCount);
        Assert.Equal(320, reconciliation.ReconciledAccountCount);
        Assert.Equal(0, reconciliation.DifferentAccountCount);
        Assert.Equal(0m, reconciliation.DebitTurnoverDifference);
        Assert.Equal(0m, reconciliation.CreditTurnoverDifference);
        Assert.Equal(0m, reconciliation.ClosingBalanceDifference);
    }

    [Fact]
    public void Import_LoadsMatchingUrbisJournalAndLedger()
    {
        AccountingImportPackage result =
            AccountingImportCoordinator
                .CreateDefault()
                .Import(
                    new AccountingImportRequest
                    {
                        AccountingFormat = "Urbis",
                        JournalFilePath =
                            GetFixturePath(
                                "U_DENNIK_00325791_202412.xlsx"),
                        GeneralLedgerFilePath =
                            GetFixturePath(
                                "HL_KNIHA_00325791_202412.xls"),
                        ExpectedIco = "00325791",
                        ExpectedFiscalYear = 2024
                    });

        Assert.Equal("Urbis", result.AccountingFormat);
        Assert.Equal("00325791", result.Ico);
        Assert.Equal(2024, result.FiscalYear);
        Assert.Equal(12, result.ExportStage);
        Assert.NotNull(result.Journal);
        Assert.NotNull(result.GeneralLedger);
        Assert.True(result.HasGeneralLedger);
        Assert.NotNull(result.JournalLedgerReconciliation);
    }

    [Fact]
    public void Import_AllowsJournalWithoutGeneralLedger()
    {
        AccountingImportPackage result =
            AccountingImportCoordinator
                .CreateDefault()
                .Import(
                    new AccountingImportRequest
                    {
                        AccountingFormat = "Urbis",
                        JournalFilePath =
                            GetFixturePath(
                                "U_DENNIK_00325791_202412.xlsx"),
                        ExpectedIco = "00325791",
                        ExpectedFiscalYear = 2024
                    });

        Assert.NotNull(result.Journal);
        Assert.Null(result.GeneralLedger);
        Assert.False(result.HasGeneralLedger);
        Assert.Null(result.JournalLedgerReconciliation);
    }

    [Fact]
    public void Import_AggregatesSoftipMopMonthlyJournalsAndUsesAccountingPeriod()
    {
        var request = new AccountingImportRequest
        {
            AccountingFormat = "Softip-MOP",
            ExpectedIco = "31715362",
            ExpectedFiscalYear = 2025
        };
        request.JournalFilePaths.Add(
            GetSoftipMopFixturePath("Omida dennik 01 2025.xlsx"));
        request.JournalFilePaths.Add(
            GetSoftipMopFixturePath("Omida dennik 12 2025.xlsx"));

        AccountingImportPackage result =
            AccountingImportCoordinator.CreateDefault().Import(request);

        Assert.Equal("Softip-MOP", result.AccountingFormat);
        Assert.Equal("31715362", result.Ico);
        Assert.Equal(2025, result.FiscalYear);
        Assert.Equal(118787, result.Journal.Rows.Count);
        Assert.Contains(
            result.Journal.Rows,
            row => row.PostingDate.Year == 2026);
        Assert.Contains(
            result.Journal.ImportReport.Diagnostics,
            diagnostic =>
                diagnostic.Code == "SOFTIP_MOP_ICO_SUPPLIED_BY_REQUEST" &&
                diagnostic.Severity == ImportDiagnosticSeverity.Warning);
        Assert.Null(result.GeneralLedger);
    }

    [Fact]
    public void Import_RejectsMultipleJournalFilesForExistingSingleFileFormat()
    {
        var request = new AccountingImportRequest
        {
            AccountingFormat = "IVES",
            ExpectedIco = "00322881",
            ExpectedFiscalYear = 2024
        };
        request.JournalFilePaths.Add(
            GetIvesFixturePath("U_DENNIK_00322881_2024.xls"));
        request.JournalFilePaths.Add(
            GetIvesFixturePath("U_DENNIK_00322881_2024.xls"));

        InvalidDataException exception = Assert.Throws<InvalidDataException>(
            () => AccountingImportCoordinator.CreateDefault().Import(request));

        Assert.Contains("supports exactly one", exception.Message);
    }

    [Fact]
    public void Import_PrefersJournalFilePathsWhenBothRequestShapesArePresent()
    {
        var request = new AccountingImportRequest
        {
            AccountingFormat = "IVES",
            JournalFilePath = "missing-file.xls",
            ExpectedIco = "00322881",
            ExpectedFiscalYear = 2024
        };
        request.JournalFilePaths.Add(
            GetIvesFixturePath("U_DENNIK_00322881_2024.xls"));

        AccountingImportPackage result =
            AccountingImportCoordinator.CreateDefault().Import(request);

        Assert.Equal(2125, result.Journal.Rows.Count);
    }

    [Fact]
    public void Import_RejectsUnexpectedIco()
    {
        InvalidDataException exception =
            Assert.Throws<InvalidDataException>(
                () =>
                    AccountingImportCoordinator
                        .CreateDefault()
                        .Import(
                            new AccountingImportRequest
                            {
                                AccountingFormat = "Urbis",
                                JournalFilePath =
                                    GetFixturePath(
                                        "U_DENNIK_00325791_202412.xlsx"),
                                ExpectedIco = "12345678",
                                ExpectedFiscalYear = 2024
                            }));

        Assert.Contains(
            "IČO '12345678' was requested",
            exception.Message);
    }

    [Fact]
    public void Import_RejectsUnexpectedFiscalYear()
    {
        InvalidDataException exception =
            Assert.Throws<InvalidDataException>(
                () =>
                    AccountingImportCoordinator
                        .CreateDefault()
                        .Import(
                            new AccountingImportRequest
                            {
                                AccountingFormat = "Urbis",
                                JournalFilePath =
                                    GetFixturePath(
                                        "U_DENNIK_00325791_202412.xlsx"),
                                ExpectedIco = "00325791",
                                ExpectedFiscalYear = 2025
                            }));

        Assert.Contains(
            "fiscal year 2025 was requested",
            exception.Message);
    }

    [Fact]
    public void Import_RejectsUnrecognizedFormat()
    {
        InvalidDataException exception =
            Assert.Throws<InvalidDataException>(
                () =>
                    AccountingImportCoordinator
                        .CreateDefault()
                        .Import(
                            new AccountingImportRequest
                            {
                                AccountingFormat = "Unknown",
                                JournalFilePath =
                                    GetFixturePath(
                                        "U_DENNIK_00325791_202412.xlsx"),
                                ExpectedIco = "00325791",
                                ExpectedFiscalYear = 2024
                            }));

        Assert.Contains(
            "No registered importer recognizes",
            exception.Message);
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Urbis",
            fileName);
    }

    private static string GetIvesFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Ives",
            fileName);
    }

    private static string GetSoftipMopFixturePath(string fileName)
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            fileName);
    }

    [Fact]
    public void Import_DoesNotUseExportStageForCompatibility()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc-" +
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        string copiedLedgerPath = Path.Combine(
            directory,
            "HL_KNIHA_00325791_202414.xls");

        File.Copy(
            GetFixturePath(
                "HL_KNIHA_00325791_202412.xls"),
            copiedLedgerPath);

        try
        {
            AccountingImportPackage result =
                AccountingImportCoordinator
                    .CreateDefault()
                    .Import(
                        new AccountingImportRequest
                        {
                            AccountingFormat = "Urbis",
                            JournalFilePath =
                                GetFixturePath(
                                    "U_DENNIK_00325791_202412.xlsx"),
                            GeneralLedgerFilePath =
                                copiedLedgerPath,
                            ExpectedIco = "00325791",
                            ExpectedFiscalYear = 2024
                        });

            Assert.Equal(12, result.Journal.ExportStage);
            Assert.Equal(
                14,
                result.GeneralLedger.ExportStage);
        }
        finally
        {
            if (File.Exists(copiedLedgerPath))
            {
                File.Delete(copiedLedgerPath);
            }

            if (Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
    }
}
