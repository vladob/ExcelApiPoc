using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditCalculationPackageWorksheetReader
    {
        public static AuditTemplatePackageResponse Read(Excel.Workbook workbook)
        {
            IDictionary<string, object> metadata = AuditWorkbookTableReader.ReadRows(workbook, "__TemplatePackage").Single();
            AuditReportTableDefinitionResponse[] tables = ReadTables(workbook);

            var package = new AuditTemplatePackageResponse
            {
                ContractVersion = AuditWorkbookTableReader.GetInt32(metadata, "ContractVersion"),
                GeneratedAtUtc = AuditWorkbookTableReader.GetDateTime(metadata, "GeneratedAtUtc"),
                Template = new AuditTemplateDefinitionResponse
                {
                    TemplateErpId = AuditWorkbookTableReader.GetInt32(metadata, "TemplateErpId"),
                    Name = AuditWorkbookTableReader.GetString(metadata, "TemplateName"),
                    MfSpecification = AuditWorkbookTableReader.GetString(metadata, "MfSpecification"),
                    ValidFrom = AuditWorkbookTableReader.GetNullableDateTime(metadata, "ValidFrom"),
                    ValidTo = AuditWorkbookTableReader.GetNullableDateTime(metadata, "ValidTo"),
                    AccountingModel = AuditWorkbookTableReader.GetString(metadata, "AccountingModel"),
                    Tables = tables
                },
                AccountGroups = ReadAccountGroups(workbook),
                ReportMappingRules = ReadMappingRules(workbook),
                CalculationPlan = ReadCalculationPlan(workbook)
            };

            if (package.ContractVersion != 3)
                throw new InvalidOperationException($"Unsupported embedded package contract version {package.ContractVersion}.");

            return package;
        }

        private static AuditReportTableDefinitionResponse[] ReadTables(Excel.Workbook workbook)
        {
            IReadOnlyList<IDictionary<string, object>> tableRows = AuditWorkbookTableReader.ReadRows(workbook, "__ReportTables");
            IReadOnlyList<IDictionary<string, object>> headerRows = AuditWorkbookTableReader.ReadRows(workbook, "__ReportHeaders");
            IReadOnlyList<IDictionary<string, object>> reportRows = AuditWorkbookTableReader.ReadRows(workbook, "__ReportRows");
            var result = new List<AuditReportTableDefinitionResponse>();

            foreach (IDictionary<string, object> tableRow in tableRows)
            {
                int tableErpId = AuditWorkbookTableReader.GetInt32(tableRow, "TableErpId");

                AuditReportHeaderDefinitionResponse[] headers = headerRows.Where(row => AuditWorkbookTableReader.GetInt32(row, "TableErpId") == tableErpId).Select(row => new AuditReportHeaderDefinitionResponse
                    {
                        TextSk = AuditWorkbookTableReader.GetString(row, "TextSk"),
                        TextEn = AuditWorkbookTableReader.GetString(row, "TextEn"),
                        RowPosition = AuditWorkbookTableReader.GetInt32(row, "RowPosition"),
                        ColumnPosition = AuditWorkbookTableReader.GetInt32(row, "ColumnPosition"),
                        RowSpan = AuditWorkbookTableReader.GetInt32(row, "RowSpan"),
                        ColumnSpan = AuditWorkbookTableReader.GetInt32(row, "ColumnSpan")
                    }).ToArray();

                AuditReportRowDefinitionResponse[] rows = reportRows.Where(row => AuditWorkbookTableReader.GetInt32(row, "TableErpId") == tableErpId).Select(row => new AuditReportRowDefinitionResponse
                    {
                        RowOrdinal = AuditWorkbookTableReader.GetInt32(row, "RowOrdinal"),
                        RowNumber = AuditWorkbookTableReader.GetNullableInt32(row, "RowNumber"),
                        Designation = AuditWorkbookTableReader.GetString(row, "Designation"),
                        TextSk = AuditWorkbookTableReader.GetString(row, "TextSk"),
                        TextEn = AuditWorkbookTableReader.GetString(row, "TextEn"),
                        IsSumRow = AuditWorkbookTableReader.GetBoolean(row, "IsSumRow"),
                        CategorySk = AuditWorkbookTableReader.GetString(row, "CategorySk"),
                        MappingCaptionSk = AuditWorkbookTableReader.GetString(row, "MappingCaptionSk")
                    }).ToArray();

                result.Add(new AuditReportTableDefinitionResponse
                {
                    TableErpId = tableErpId,
                    TableOrdinal = AuditWorkbookTableReader.GetInt32(tableRow, "TableOrdinal"),
                    NameSk = AuditWorkbookTableReader.GetString(tableRow, "NameSk"),
                    NameEn = AuditWorkbookTableReader.GetString(tableRow, "NameEn"),
                    NumberOfColumns = AuditWorkbookTableReader.GetNullableInt32(tableRow, "NumberOfColumns"),
                    NumberOfDataColumns = AuditWorkbookTableReader.GetNullableInt32(tableRow, "NumberOfDataColumns"),
                    DontHaveRowNumbers = AuditWorkbookTableReader.GetBoolean(tableRow, "DontHaveRowNumbers"),
                    Headers = headers,
                    Rows = rows
                });
            }
            return result.ToArray();
        }

        private static AuditAccountGroupDefinitionResponse[] ReadAccountGroups(Excel.Workbook workbook)
        {
            return AuditWorkbookTableReader.ReadRows(workbook, "__AccountGroups").Select(row => new AuditAccountGroupDefinitionResponse
                {
                    Account = AuditWorkbookTableReader.GetString(row, "Account"),
                    Title = AuditWorkbookTableReader.GetString(row, "Title"),
                    Legend = AuditWorkbookTableReader.GetString(row, "Legend"),
                    AssetsValueSource = AuditWorkbookTableReader.GetString(row, "AssetsValueSource"),
                    LiabilitiesValueSource = AuditWorkbookTableReader.GetString(row, "LiabilitiesValueSource")
                }).ToArray();
        }

        private static AuditReportMappingRuleDefinitionResponse[] ReadMappingRules(Excel.Workbook workbook)
        {
            return AuditWorkbookTableReader.ReadRows(workbook, "__ReportMappingRules").Select(row => new AuditReportMappingRuleDefinitionResponse
                {
                    TableErpId = AuditWorkbookTableReader.GetInt32(row, "TableErpId"),
                    Account3 = AuditWorkbookTableReader.GetString(row, "Account3"),
                    ReportRowNumber = AuditWorkbookTableReader.GetInt32(row, "ReportRowNumber"),
                    AccountTitle = AuditWorkbookTableReader.GetString(row, "AccountTitle"),
                    RequiresAnalyticalMapping = AuditWorkbookTableReader.GetBoolean(row, "RequiresAnalyticalMapping"),
                    IncludeInBrutto = AuditWorkbookTableReader.GetBoolean(row, "IncludeInBrutto"),
                    IncludeInCorrection = AuditWorkbookTableReader.GetBoolean(row, "IncludeInCorrection"),
                    Side = AuditWorkbookTableReader.GetString(row, "Side"),
                    ValueSource = AuditWorkbookTableReader.GetString(row, "ValueSource")
                }).ToArray();
        }

        private static AuditCalculationDependencyDefinitionResponse[] ReadCalculationPlan(Excel.Workbook workbook)
        {
            return AuditWorkbookTableReader.ReadRows(workbook, "__CalculationPlan").Select(row => new AuditCalculationDependencyDefinitionResponse
                {
                    TargetTableErpId = AuditWorkbookTableReader.GetInt32(row, "TargetTableErpId"),
                    TargetRowNumber = AuditWorkbookTableReader.GetInt32(row, "TargetRowNumber"),
                    SourceTableErpId = AuditWorkbookTableReader.GetInt32(row, "SourceTableErpId"),
                    SourceRowNumber = AuditWorkbookTableReader.GetInt32(row, "SourceRowNumber"),
                    Coefficient = AuditWorkbookTableReader.GetInt32(row, "Coefficient"),
                    CalculationLevel = AuditWorkbookTableReader.GetInt32(row, "CalculationLevel")
                }).ToArray();
        }
    }
}
