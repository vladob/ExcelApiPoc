using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookWorksheetLayout
    {
        private const string LegacyAccountWorksheetName = "Accounts";
        private const string AccountWorksheetName = "Account Summary";
        private const string CalculationResultsTableName = "CalculatedReportRows";
        private const string GeneralLedgerComparisonTableName =
            "GeneralLedgerComparisonRows";

        private static readonly string[] WorksheetOrder =
        {
            "Accounting Framework",
            "Accounting Journal",
            AccountWorksheetName,
            "General Ledger",
            "GL Comparison",
            "Analytical Mapping",
            "Calculation Results",
            "Multi-year Income Statement",
            "Multi-year Balance Sheet",
            "Multi-year Balance & Income",
            "RegisterUZ Attachments",
            "RegisterUZ Reports",
            "No Calculation Report"
        };

        private static readonly string[] GeneralLedgerDifferenceColumns =
        {
            "OpeningDebitDifference",
            "OpeningCreditDifference",
            "DebitTurnoverDifference",
            "CreditTurnoverDifference",
            "ClosingBalanceDifference"
        };

        private static readonly HashSet<string> AccountingEvidenceWorksheets =
            new HashSet<string>(
                new[]
                {
                    "Accounting Framework",
                    "Accounting Journal",
                    AccountWorksheetName,
                    "General Ledger",
                    "GL Comparison"
                },
                StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> AuditWorkWorksheets =
            new HashSet<string>(
                new[]
                {
                    "Analytical Mapping",
                    "Calculation Results",
                    "Multi-year Income Statement",
                    "Multi-year Balance Sheet",
                    "Multi-year Balance & Income"
                },
                StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> RegisterUzEvidenceWorksheets =
            new HashSet<string>(
                new[]
                {
                    "RegisterUZ Attachments",
                    "RegisterUZ Reports"
                },
                StringComparer.OrdinalIgnoreCase);

        private static readonly int AccountingEvidenceColor =
            Rgb(169, 208, 142);
        private static readonly int AuditWorkColor =
            Rgb(155, 194, 230);
        private static readonly int RegisterUzEvidenceColor =
            Rgb(244, 176, 132);
        private static readonly int ExceptionalColor =
            Rgb(224, 102, 102);
        private static readonly int DiagnosticRedColor =
            Rgb(255, 199, 206);
        private static readonly int DiagnosticYellowColor =
            Rgb(255, 235, 156);
        private static readonly int ComparisonColumnColor =
            Rgb(252, 228, 214);

        public static void Apply(Excel.Workbook workbook)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));

            RenameAccountWorksheet(workbook);

            Excel.Worksheet previous = null;

            foreach (string worksheetName in WorksheetOrder)
            {
                Excel.Worksheet worksheet =
                    FindWorksheet(workbook, worksheetName);

                if (worksheet == null)
                    continue;

                ApplyColor(worksheet);

                if (previous == null)
                {
                    if (worksheet.Index != 1)
                        worksheet.Move(Before: workbook.Worksheets[1]);
                }
                else if (worksheet.Index != previous.Index + 1)
                {
                    worksheet.Move(After: previous);
                }

                previous = worksheet;
            }

            PlaceMultiYearWorksheets(workbook);
            ApplyDiagnosticConditionalFormatting(workbook);
        }

        private static void PlaceMultiYearWorksheets(
            Excel.Workbook workbook)
        {
            Excel.Worksheet anchor =
                FindWorksheet(workbook, "RegisterUZ Attachments") ??
                FindWorksheet(workbook, "RegisterUZ Reports") ??
                FindWorksheet(workbook, "No Calculation Report");

            if (anchor == null)
                return;

            Excel.Worksheet[] multiYearWorksheets =
                workbook.Worksheets
                    .Cast<Excel.Worksheet>()
                    .Where(worksheet => worksheet.Name.StartsWith(
                        "Multi-year ",
                        StringComparison.OrdinalIgnoreCase))
                    .OrderBy(worksheet => worksheet.Name)
                    .ToArray();

            foreach (Excel.Worksheet worksheet in multiYearWorksheets)
            {
                worksheet.Tab.Color = AuditWorkColor;
                worksheet.Move(Before: anchor);
            }
        }

        public static void ApplyAuditWorkColor(
            Excel.Worksheet worksheet)
        {
            if (worksheet == null)
                throw new ArgumentNullException(nameof(worksheet));

            worksheet.Tab.Color = AuditWorkColor;
        }

        public static void ApplyRegisterUzEvidenceColor(
            Excel.Worksheet worksheet)
        {
            if (worksheet == null)
                throw new ArgumentNullException(nameof(worksheet));

            worksheet.Tab.Color = RegisterUzEvidenceColor;
        }

        private static void RenameAccountWorksheet(Excel.Workbook workbook)
        {
            Excel.Worksheet legacy =
                FindWorksheet(workbook, LegacyAccountWorksheetName);

            if (legacy == null ||
                FindWorksheet(workbook, AccountWorksheetName) != null)
            {
                return;
            }

            legacy.Name = AccountWorksheetName;
        }

        private static void ApplyColor(Excel.Worksheet worksheet)
        {
            if (AccountingEvidenceWorksheets.Contains(worksheet.Name))
            {
                worksheet.Tab.Color = AccountingEvidenceColor;
                return;
            }

            if (AuditWorkWorksheets.Contains(worksheet.Name))
            {
                worksheet.Tab.Color = AuditWorkColor;
                return;
            }

            if (RegisterUzEvidenceWorksheets.Contains(worksheet.Name))
            {
                worksheet.Tab.Color = RegisterUzEvidenceColor;
                return;
            }

            if (string.Equals(
                    worksheet.Name,
                    "No Calculation Report",
                    StringComparison.OrdinalIgnoreCase))
            {
                worksheet.Tab.Color = ExceptionalColor;
            }
        }

        private static void ApplyDiagnosticConditionalFormatting(
            Excel.Workbook workbook)
        {
            Excel.ListObject calculationResults =
                FindTable(workbook, CalculationResultsTableName);

            if (calculationResults?.DataBodyRange != null)
            {
                ApplyDifferenceFormatting(
                    calculationResults,
                    "Difference1",
                    "Difference3");
            }

            Excel.ListObject generalLedgerComparison =
                FindTable(workbook, GeneralLedgerComparisonTableName);

            if (generalLedgerComparison?.DataBodyRange != null)
            {
                ApplyGeneralLedgerDifferenceColumnFormatting(
                    generalLedgerComparison);
                ApplyGeneralLedgerStatusFormatting(
                    generalLedgerComparison);
            }
        }

        private static void ApplyDifferenceFormatting(
            Excel.ListObject table,
            string firstColumnName,
            string lastColumnName)
        {
            Excel.Range first =
                table.ListColumns[firstColumnName].DataBodyRange;
            Excel.Range last =
                table.ListColumns[lastColumnName].DataBodyRange;

            if (first == null || last == null)
                return;

            Excel.Worksheet worksheet = (Excel.Worksheet)table.Parent;
            Excel.Range target =
                worksheet.Range[
                    first.Cells[1, 1],
                    last.Cells[last.Rows.Count, 1]];

            target.FormatConditions.Delete();

            Excel.FormatCondition blankCondition =
                (Excel.FormatCondition)target.FormatConditions.Add(
                    Excel.XlFormatConditionType.xlBlanksCondition);
            blankCondition.StopIfTrue = true;

            Excel.FormatCondition differenceCondition =
                (Excel.FormatCondition)target.FormatConditions.Add(
                    Excel.XlFormatConditionType.xlCellValue,
                    Excel.XlFormatConditionOperator.xlNotEqual,
                    "0");

            differenceCondition.Interior.Color = DiagnosticRedColor;
            differenceCondition.Font.Bold = true;
        }

        private static void ApplyGeneralLedgerDifferenceColumnFormatting(
            Excel.ListObject table)
        {
            foreach (string columnName in GeneralLedgerDifferenceColumns)
            {
                Excel.Range target =
                    table.ListColumns[columnName].DataBodyRange;

                if (target == null)
                    continue;

                target.Interior.Color = ComparisonColumnColor;
                target.Font.Bold = true;
            }
        }

        private static void ApplyGeneralLedgerStatusFormatting(
            Excel.ListObject table)
        {
            Excel.Range status = table.ListColumns["Status"].DataBodyRange;

            if (status == null)
                return;

            status.FormatConditions.Delete();

            AddStatusCondition(
                status,
                "Different",
                DiagnosticRedColor);
            AddStatusCondition(
                status,
                "Different; opening unavailable",
                DiagnosticRedColor);
            AddStatusCondition(
                status,
                "Journal only",
                DiagnosticYellowColor);
            AddStatusCondition(
                status,
                "General-ledger only",
                DiagnosticYellowColor);
        }

        private static void AddStatusCondition(
            Excel.Range target,
            string status,
            int fillColor)
        {
            Excel.FormatCondition condition =
                (Excel.FormatCondition)target.FormatConditions.Add(
                    Excel.XlFormatConditionType.xlCellValue,
                    Excel.XlFormatConditionOperator.xlEqual,
                    "=\"" + status.Replace("\"", "\"\"") + "\"");

            condition.Interior.Color = fillColor;
            condition.Font.Bold = true;
        }

        private static Excel.ListObject FindTable(
            Excel.Workbook workbook,
            string tableName)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                foreach (Excel.ListObject table in worksheet.ListObjects)
                {
                    if (string.Equals(
                            table.Name,
                            tableName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return table;
                    }
                }
            }

            return null;
        }

        private static Excel.Worksheet FindWorksheet(
            Excel.Workbook workbook,
            string worksheetName)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                if (string.Equals(
                        worksheet.Name,
                        worksheetName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return worksheet;
                }
            }

            return null;
        }

        private static int Rgb(int red, int green, int blue)
        {
            return red | (green << 8) | (blue << 16);
        }
    }
}
