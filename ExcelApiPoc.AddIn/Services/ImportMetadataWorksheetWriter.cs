using ExcelApiPoc.AddIn.Models;
using System;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class ImportMetadataWorksheetWriter
    {
        private const string WorksheetName = "__Metadata";
        private const string TableName = "__ImportSources";

        private static readonly string[] Headers =
        {
            "SourceType",
            "SourceFileName",
            "SourceFilePath",
            "SourceFileHash",
            "TechnicalType",
            "AccountingFormat",
            "Ico",
            "CompanyName",
            "DetectedFiscalYear",
            "SelectedFiscalYear",
            "DateFrom",
            "DateTo",
            "ImportedAtUtc",
            "RecordCount",
            "RejectedRecordCount",
            "NormalizedTextFieldCount",
            "FrameworkCode",
            "FrameworkVersionCode",
            "FrameworkFiscalYear",
            "FrameworkRetrievalSource",
            "FrameworkCachePath",
            "FrameworkApiFailureMessage",
            "TemplateContractVersion",
            "TemplateGeneratedAtUtc",
            "TemplateErpId",
            "TemplateName",
            "TemplateMfSpecification",
            "TemplateAccountingModel",
            "TemplateSelectionSource",
            "RegisterUzReportId",
            "TemplateRetrievalSource",
            "TemplateCachePath",
            "TemplateApiFailureMessage",
            "AccountingFrameworkSourceFileName",
            "AccountingFrameworkSourceFilePath",
            "AccountingFrameworkSourceFileHash",
            "AccountingFrameworkIco",
            "AccountingFrameworkFiscalYear",
            "AccountingFrameworkImportedAtUtc",
            "AccountingFrameworkRecordCount",
            "AccountingFrameworkNormalizedTextFieldCount",
            "GeneralLedgerSourceFileName",
            "GeneralLedgerSourceFilePath",
            "GeneralLedgerSourceFileHash",
            "GeneralLedgerIco",
            "GeneralLedgerFiscalYear",
            "GeneralLedgerThroughMonth",
            "GeneralLedgerImportedAtUtc",
            "GeneralLedgerRecordCount",
            "GeneralLedgerNormalizedTextFieldCount",
            "TemplatePackageFrameworkCode",
            "TemplatePackageFrameworkVersionCode",
            "TemplateCalculationConfigurationCode",
            "TemplatePackageApplicableDate"
        };

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            JournalImport journalImport,
            AccountFrameworkLoadResult frameworkLoad,
            AuditTemplatePackageResponse templatePackage,
            AuditReportContext reportContext,
            AuditTemplatePackageLoadResult templatePackageLoad,
            AccountingFrameworkImport accountingFrameworkImport,
            GeneralLedgerImport generalLedgerImport)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook));
            }

            if (journalImport == null)
            {
                throw new ArgumentNullException(nameof(journalImport));
            }

            if (frameworkLoad == null)
            {
                throw new ArgumentNullException(nameof(frameworkLoad));
            }

            if (templatePackage == null || templatePackage.Template == null)
                throw new ArgumentNullException(nameof(templatePackage));

            if (reportContext == null)
                throw new ArgumentNullException(nameof(reportContext));

            if (templatePackageLoad == null)
                throw new ArgumentNullException(nameof(templatePackageLoad));

            Excel.Worksheet lastWorksheet = (Excel.Worksheet)workbook.Worksheets[ workbook.Worksheets.Count];
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets.Add( After: lastWorksheet);

            worksheet.Name = WorksheetName;

            DateTime dateFrom = journalImport.Rows.Min( row => row.PostingDate);
            DateTime dateTo = journalImport.Rows.Max( row => row.PostingDate);

            var values = new object[2, Headers.Length];

            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
            {
                values[0, columnIndex] = Headers[columnIndex];
            }

            values[1, 0] = "AccountingJournal";
            values[1, 1] = journalImport.SourceFileName;
            values[1, 2] = journalImport.SourceFilePath;
            values[1, 3] = journalImport.SourceFileHash;
            values[1, 4] = journalImport.TechnicalType;
            values[1, 5] = journalImport.AccountingFormat;
            values[1, 6] = journalImport.Ico;
            values[1, 7] = journalImport.CompanyName;
            values[1, 8] = journalImport.FiscalYear;
            values[1, 9] = reportContext.FiscalYear;
            values[1, 10] = dateFrom;
            values[1, 11] = dateTo;
            values[1, 12] = journalImport.ImportedAtUtc;
            values[1, 13] = journalImport.Rows.Count;
            values[1, 14] = 0;
            values[1, 15] = journalImport.NormalizedTextFieldCount;
            values[1, 16] = frameworkLoad.Framework.FrameworkCode;
            values[1, 17] = frameworkLoad.Framework.VersionCode;
            values[1, 18] = frameworkLoad.FiscalYear;
            values[1, 19] = frameworkLoad.Source;
            values[1, 20] = frameworkLoad.CachePath;
            values[1, 21] = frameworkLoad.ApiFailureMessage;
            values[1, 22] = templatePackage.ContractVersion;
            values[1, 23] = templatePackage.GeneratedAtUtc;
            values[1, 24] = templatePackage.Template.TemplateErpId;
            values[1, 25] = templatePackage.Template.Name;
            values[1, 26] = templatePackage.Template.MfSpecification;
            values[1, 27] = templatePackage.Template.AccountingModel;
            values[1, 28] = reportContext.SelectionSource;
            values[1, 29] = reportContext.RegisterUzReportId;
            values[1, 30] = templatePackageLoad.Source;
            values[1, 31] = templatePackageLoad.CachePath;
            values[1, 32] = templatePackageLoad.ApiFailureMessage;
            if (accountingFrameworkImport != null)
            {
                values[1, 33] = accountingFrameworkImport.SourceFileName;
                values[1, 34] = accountingFrameworkImport.SourceFilePath;
                values[1, 35] = accountingFrameworkImport.SourceFileHash;
                values[1, 36] = accountingFrameworkImport.Ico;
                values[1, 37] = accountingFrameworkImport.FiscalYear;
                values[1, 38] = accountingFrameworkImport.ImportedAtUtc;
                values[1, 39] = accountingFrameworkImport.Rows.Count;
                values[1, 40] = accountingFrameworkImport.NormalizedTextFieldCount;
            }
            if (generalLedgerImport != null)
            {
                values[1, 41] = generalLedgerImport.SourceFileName;
                values[1, 42] = generalLedgerImport.SourceFilePath;
                values[1, 43] = generalLedgerImport.SourceFileHash;
                values[1, 44] = generalLedgerImport.Ico;
                values[1, 45] = generalLedgerImport.FiscalYear;
                values[1, 46] = generalLedgerImport.ThroughMonth;
                values[1, 47] = generalLedgerImport.ImportedAtUtc;
                values[1, 48] = generalLedgerImport.Rows.Count;
                values[1, 49] = generalLedgerImport.NormalizedTextFieldCount;
            }
            values[1, 50] = templatePackage.FrameworkCode;
            values[1, 51] = templatePackage.FrameworkVersionCode;
            values[1, 52] = templatePackage.CalculationConfigurationCode;
            values[1, 53] = templatePackage.ApplicableDate;

            Excel.Range firstCell = (Excel.Range)worksheet.Cells[1, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[2, Headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            // Preserve identifiers and provenance values exactly.
            int[] textColumns =
            {
                1, 2, 3, 4, 5, 6, 7, 8,
                17, 18, 20, 21, 22,
                26, 27, 28, 29, 30, 31, 32, 33,
                34, 35, 36, 37,
                42, 43, 44, 45,
                51, 52, 53
            };

            foreach (int columnNumber in textColumns)
                ((Excel.Range)worksheet.Cells[2, columnNumber]).NumberFormat = "@";

            tableRange.Value2 = values;

            Excel.Range fiscalYearCell = (Excel.Range)worksheet.Cells[2, 9];
            Excel.Range selectedFiscalYearCell = (Excel.Range)worksheet.Cells[2, 10];
            Excel.Range dateFromCell = (Excel.Range)worksheet.Cells[2, 11];
            Excel.Range dateToCell = (Excel.Range)worksheet.Cells[2, 12];
            Excel.Range importedAtUtcCell = (Excel.Range)worksheet.Cells[2, 13];
            Excel.Range recordCountCell = (Excel.Range)worksheet.Cells[2, 14];
            Excel.Range rejectedCountCell = (Excel.Range)worksheet.Cells[2, 15];
            Excel.Range normalizedCountCell = (Excel.Range)worksheet.Cells[2, 16];
            Excel.Range frameworkFiscalYearCell = (Excel.Range)worksheet.Cells[2, 19];
            Excel.Range templateContractVersionCell = (Excel.Range)worksheet.Cells[2, 23];
            Excel.Range templateGeneratedAtUtcCell = (Excel.Range)worksheet.Cells[2, 24];
            Excel.Range templateErpIdCell = (Excel.Range)worksheet.Cells[2, 25];

            fiscalYearCell.NumberFormat = "0";
            selectedFiscalYearCell.NumberFormat = "0";
            dateFromCell.NumberFormat = "yyyy-mm-dd";
            dateToCell.NumberFormat = "yyyy-mm-dd";
            importedAtUtcCell.NumberFormat = "yyyy-mm-dd hh:mm:ss";
            recordCountCell.NumberFormat = "#,##0";
            rejectedCountCell.NumberFormat = "#,##0";
            normalizedCountCell.NumberFormat = "#,##0";
            frameworkFiscalYearCell.NumberFormat = "0";
            templateContractVersionCell.NumberFormat = "0";
            templateGeneratedAtUtcCell.NumberFormat = "yyyy-mm-dd hh:mm:ss";
            templateErpIdCell.NumberFormat = "0";
            ((Excel.Range)worksheet.Cells[2, 38]).NumberFormat = "0";
            ((Excel.Range)worksheet.Cells[2, 39]).NumberFormat = "yyyy-mm-dd hh:mm:ss";
            ((Excel.Range)worksheet.Cells[2, 40]).NumberFormat = "#,##0";
            ((Excel.Range)worksheet.Cells[2, 41]).NumberFormat = "#,##0";
            ((Excel.Range)worksheet.Cells[2, 46]).NumberFormat = "0";
            ((Excel.Range)worksheet.Cells[2, 47]).NumberFormat = "0";
            ((Excel.Range)worksheet.Cells[2, 48]).NumberFormat = "yyyy-mm-dd hh:mm:ss";
            ((Excel.Range)worksheet.Cells[2, 49]).NumberFormat = "#,##0";
            ((Excel.Range)worksheet.Cells[2, 50]).NumberFormat = "#,##0";
            ((Excel.Range)worksheet.Cells[2, 54]).NumberFormat = "yyyy-mm-dd";

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            AddAccountFrameworkTable(worksheet, frameworkLoad);
            // Users may unhide it for provenance inspection.
            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;

            return worksheet;
        }

        private static void AddAccountFrameworkTable(Excel.Worksheet worksheet,AccountFrameworkLoadResult frameworkLoad)
        {
            ApplicableAccountFrameworkResponse framework = frameworkLoad.Framework;
            AccountDefinitionMetadataResponse[] definitions = framework.Definitions ?? Array.Empty<AccountDefinitionMetadataResponse>();
            AccountRangeMetadataResponse[] ranges = framework.Ranges ?? Array.Empty<AccountRangeMetadataResponse>();
            int syntheticAccountCount = definitions.Count(definition => definition.AccountLevel == 3);
            string[] headers =
            {
                "FrameworkCode",
                "FrameworkName",
                "FrameworkVersionId",
                "VersionCode",
                "FiscalYear",
                "ValidFrom",
                "ValidTo",
                "LegalReference",
                "SourceUrl",
                "SourceSha256",
                "DefinitionCount",
                "SyntheticAccountCount",
                "RangeCount",
                "RetrievalSource",
                "CachePath"
            };

            var values = new object[2, headers.Length];

            for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
            {
                values[0, columnIndex] = headers[columnIndex];
            }

            values[1, 0] = framework.FrameworkCode;
            values[1, 1] = framework.FrameworkName;
            values[1, 2] = framework.FrameworkVersionId;
            values[1, 3] = framework.VersionCode;
            values[1, 4] = frameworkLoad.FiscalYear;
            values[1, 5] = framework.ValidFrom;
            values[1, 6] = framework.ValidTo.HasValue ? (object)framework.ValidTo.Value : null;
            values[1, 7] = framework.LegalReference;
            values[1, 8] = framework.SourceUrl;
            values[1, 9] = framework.SourceSha256;
            values[1, 10] = definitions.Length;
            values[1, 11] = syntheticAccountCount;
            values[1, 12] = ranges.Length;
            values[1, 13] = frameworkLoad.Source;
            values[1, 14] = frameworkLoad.CachePath;

            Excel.Range firstCell = (Excel.Range)worksheet.Cells[5, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[6, headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            int[] textColumns = { 1, 2, 4, 8, 9, 10, 14, 15 };

            foreach (int columnNumber in textColumns)
            {
                Excel.Range cell = (Excel.Range)worksheet.Cells[6, columnNumber];
                cell.NumberFormat = "@";
            }

            tableRange.Value2 = values;

            Excel.Range versionIdCell = (Excel.Range)worksheet.Cells[6, 3];
            Excel.Range fiscalYearCell = (Excel.Range)worksheet.Cells[6, 5];
            Excel.Range validFromCell = (Excel.Range)worksheet.Cells[6, 6];
            Excel.Range validToCell = (Excel.Range)worksheet.Cells[6, 7];
            Excel.Range definitionCountCell = (Excel.Range)worksheet.Cells[6, 11];
            Excel.Range syntheticCountCell = (Excel.Range)worksheet.Cells[6, 12];
            Excel.Range rangeCountCell = (Excel.Range)worksheet.Cells[6, 13];

            versionIdCell.NumberFormat = "0";
            fiscalYearCell.NumberFormat = "0";
            validFromCell.NumberFormat = "yyyy-mm-dd";
            validToCell.NumberFormat = "yyyy-mm-dd";
            definitionCountCell.NumberFormat = "#,##0";
            syntheticCountCell.NumberFormat = "#,##0";
            rangeCountCell.NumberFormat = "#,##0";

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);

            table.Name = "__AccountFramework";
            table.TableStyle = "TableStyleMedium2";
        }
    }
}
