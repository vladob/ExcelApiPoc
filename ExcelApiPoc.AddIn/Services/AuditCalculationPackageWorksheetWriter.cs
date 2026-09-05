using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditCalculationPackageWorksheetWriter
    {
        private const string WorksheetName = "__Calculation";

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook, AuditTemplatePackageResponse package, AuditReportContext reportContext, AuditTemplatePackageLoadResult packageLoad)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            if (package == null)
                throw new ArgumentNullException(nameof(package));

            if (package.Template == null)
                throw new InvalidOperationException("The calculation package does not contain a template.");

            if (reportContext == null)
                throw new ArgumentNullException(nameof(reportContext));

            if (packageLoad == null)
                throw new ArgumentNullException(nameof(packageLoad));

            Excel.Worksheet lastWorksheet = (Excel.Worksheet)workbook.Worksheets[workbook.Worksheets.Count];
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets.Add(After: lastWorksheet);

            worksheet.Name = WorksheetName;

            int nextRow = 1;

            AddPackageTable(worksheet, ref nextRow, package, reportContext, packageLoad);
            AddReportTablesTable(worksheet, ref nextRow, package);
            AddReportHeadersTable(worksheet, ref nextRow, package);
            AddReportRowsTable(worksheet, ref nextRow, package);
            AddAccountGroupsTable(worksheet, ref nextRow, package);
            AddMappingRulesTable(worksheet, ref nextRow, package);
            AddCalculationPlanTable(worksheet, ref nextRow, package);

            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;
            return worksheet;
        }

        private static void AddPackageTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package, AuditReportContext reportContext, AuditTemplatePackageLoadResult packageLoad)
        {
            string[] headers =
            {
                "ContractVersion", "GeneratedAtUtc", "TemplateErpId",
                "TemplateName", "MfSpecification", "ValidFrom", "ValidTo",
                "AccountingModel", "Ico", "FiscalYear", "SelectionSource",
                "RegisterUzReportId", "RetrievalSource", "CachePath",
                "TemplateFrameworkVersionId", "AccountFrameworkId",
                "FrameworkCode", "AccountFrameworkVersionId", "FrameworkVersionCode",
                "CalculationConfigurationVersionId", "CalculationConfigurationCode", "ApplicableDate"
            };

            var rows = new List<object[]>
            {
                new object[]
                {
                    package.ContractVersion,
                    package.GeneratedAtUtc,
                    package.Template.TemplateErpId,
                    package.Template.Name,
                    package.Template.MfSpecification,
                    package.Template.ValidFrom,
                    package.Template.ValidTo,
                    package.Template.AccountingModel,
                    reportContext.Ico,
                    reportContext.FiscalYear,
                    reportContext.SelectionSource,
                    reportContext.RegisterUzReportId,
                    packageLoad.Source,
                    packageLoad.CachePath,
                    package.TemplateFrameworkVersionId,
                    package.AccountFrameworkId,
                    package.FrameworkCode,
                    package.AccountFrameworkVersionId,
                    package.FrameworkVersionCode,
                    package.CalculationConfigurationVersionId,
                    package.CalculationConfigurationCode,
                    package.ApplicableDate
                }
            };
            AddTable(worksheet, ref nextRow, "__TemplatePackage", headers, rows, new[] { 4, 5, 8, 9, 11, 12, 13, 14, 17, 19, 21, 22 });
        }

        private static void AddReportTablesTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "TableErpId", "TableOrdinal", "NameSk", "NameEn",
                "NumberOfColumns", "NumberOfDataColumns", "DontHaveRowNumbers"
            };
            var rows = new List<object[]>();

            foreach (AuditReportTableDefinitionResponse table in package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
            {
                rows.Add(new object[]
                {
                    table.TableErpId,
                    table.TableOrdinal,
                    table.NameSk,
                    table.NameEn,
                    table.NumberOfColumns,
                    table.NumberOfDataColumns,
                    table.DontHaveRowNumbers
                });
            }
            AddTable(worksheet, ref nextRow, "__ReportTables", headers, rows, new[] { 3, 4 });
        }

        private static void AddReportHeadersTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "TableErpId", "TextSk", "TextEn", "RowPosition",
                "ColumnPosition", "RowSpan", "ColumnSpan"
            };
            var rows = new List<object[]>();

            foreach (AuditReportTableDefinitionResponse table in package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
            {
                foreach (AuditReportHeaderDefinitionResponse header in table.Headers ?? Array.Empty<AuditReportHeaderDefinitionResponse>())
                {
                    rows.Add(new object[]
                    {
                        table.TableErpId,
                        header.TextSk,
                        header.TextEn,
                        header.RowPosition,
                        header.ColumnPosition,
                        header.RowSpan,
                        header.ColumnSpan
                    });
                }
            }
            AddTable(worksheet, ref nextRow, "__ReportHeaders", headers, rows, new[] { 2, 3 });
        }

        private static void AddReportRowsTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "TableErpId", "RowOrdinal", "RowNumber", "Designation",
                "TextSk", "TextEn", "IsSumRow", "CategorySk", "MappingCaptionSk"
            };

            var rows = new List<object[]>();

            foreach (AuditReportTableDefinitionResponse table in package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
            {
                foreach (AuditReportRowDefinitionResponse row in table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                {
                    rows.Add(new object[]
                    {
                        table.TableErpId,
                        row.RowOrdinal,
                        row.RowNumber,
                        row.Designation,
                        row.TextSk,
                        row.TextEn,
                        row.IsSumRow,
                        row.CategorySk,
                        row.MappingCaptionSk
                    });
                }
            }
            AddTable(worksheet, ref nextRow, "__ReportRows", headers, rows, new[] { 4, 5, 6, 8, 9 });
        }

        private static void AddAccountGroupsTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "Account", "Title", "Legend", "AssetsValueSource",
                "LiabilitiesValueSource"
            };
            var rows = new List<object[]>();

            foreach (AuditAccountGroupDefinitionResponse accountGroup in package.AccountGroups ?? Array.Empty<AuditAccountGroupDefinitionResponse>())
            {
                rows.Add(new object[]
                {
                    accountGroup.Account,
                    accountGroup.Title,
                    accountGroup.Legend,
                    accountGroup.AssetsValueSource,
                    accountGroup.LiabilitiesValueSource
                });
            }
            AddTable(worksheet, ref nextRow, "__AccountGroups", headers, rows, new[] { 1, 2, 3, 4, 5 });
        }

        private static void AddMappingRulesTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "TableErpId", "Account3", "ReportRowNumber", "AccountTitle",
                "RequiresAnalyticalMapping", "IncludeInBrutto",
                "IncludeInCorrection", "Side", "ValueSource"
            };
            var rows = new List<object[]>();

            foreach (AuditReportMappingRuleDefinitionResponse rule in package.ReportMappingRules ?? Array.Empty<AuditReportMappingRuleDefinitionResponse>())
            {
                rows.Add(new object[]
                {
                    rule.TableErpId,
                    rule.Account3,
                    rule.ReportRowNumber,
                    rule.AccountTitle,
                    rule.RequiresAnalyticalMapping,
                    rule.IncludeInBrutto,
                    rule.IncludeInCorrection,
                    rule.Side,
                    rule.ValueSource
                });
            }
            AddTable(worksheet, ref nextRow, "__ReportMappingRules", headers, rows, new[] { 2, 4, 8, 9 });
        }

        private static void AddCalculationPlanTable(Excel.Worksheet worksheet, ref int nextRow, AuditTemplatePackageResponse package)
        {
            string[] headers =
            {
                "TargetTableErpId", "TargetRowNumber", "SourceTableErpId",
                "SourceRowNumber", "Coefficient", "CalculationLevel"
            };
            var rows = new List<object[]>();

            foreach (AuditCalculationDependencyDefinitionResponse dependency in package.CalculationPlan ?? Array.Empty<AuditCalculationDependencyDefinitionResponse>())
            {
                rows.Add(new object[]
                {
                    dependency.TargetTableErpId,
                    dependency.TargetRowNumber,
                    dependency.SourceTableErpId,
                    dependency.SourceRowNumber,
                    dependency.Coefficient,
                    dependency.CalculationLevel
                });
            }
            AddTable(worksheet, ref nextRow, "__CalculationPlan", headers, rows, new int[0]);
        }

        private static void AddTable(Excel.Worksheet worksheet, ref int nextRow, string tableName, string[] headers, IReadOnlyList<object[]> rows, IEnumerable<int> textColumns)
        {
            var values = new object[rows.Count + 1, headers.Length];
            for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                values[0, columnIndex] = headers[columnIndex];

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                if (rows[rowIndex].Length != headers.Length)
                    throw new InvalidOperationException($"Table {tableName} contains an invalid row.");

                for (int columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                    values[rowIndex + 1, columnIndex] = rows[rowIndex][columnIndex];
            }

            int lastRow = nextRow + rows.Count;
            Excel.Range firstCell = (Excel.Range)worksheet.Cells[nextRow, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[lastRow, headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            if (rows.Count > 0)
            {
                foreach (int columnNumber in textColumns)
                {
                    Excel.Range firstDataCell = (Excel.Range)worksheet.Cells[nextRow + 1, columnNumber];
                    Excel.Range lastDataCell = (Excel.Range)worksheet.Cells[lastRow, columnNumber];
                    worksheet.Range[firstDataCell, lastDataCell].NumberFormat = "@";
                }
            }

            tableRange.Value2 = values;
            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = tableName;
            table.TableStyle = "TableStyleMedium2";
            nextRow = lastRow + 3;
        }
    }
}
