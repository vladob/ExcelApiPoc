using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Tests.Coordination;

public sealed class AccountingImportCoordinatorTests
{
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