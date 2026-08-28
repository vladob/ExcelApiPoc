using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookRecalculationService
    {
        public static AuditWorkbookRecalculationResult Recalculate(Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            AuditTemplatePackageResponse package = AuditCalculationPackageWorksheetReader.Read(workbook);
            IReadOnlyList<AccountSummary> accounts = AccountWorksheetReader.Read(workbook);
            AnalyticalMappingSelectionReadResult mappingSelections = AnalyticalMappingWorksheetReader.Read(workbook);
            AuditReportCalculationResult calculation = AuditReportCalculationService.Calculate(accounts, package, mappingSelections.Selections);

            AuditReportCalculationWorksheetWriter.Write(workbook, package, calculation);

            return new AuditWorkbookRecalculationResult
            {
                Calculation = calculation,
                MappingSelections = mappingSelections,
                AccountCount = accounts.Count,
                MappingRuleCount = package.ReportMappingRules?.Length ?? 0,
                CalculationDependencyCount = package.CalculationPlan?.Length ?? 0
            };
        }
    }
}
