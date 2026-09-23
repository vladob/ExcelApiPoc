using System;
using System.Collections.Generic;
using System.Globalization;
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

        private static readonly string[] CalculationValueColumns =
        {
            "CalculatedValue1",
            "CalculatedValue2",
            "CalculatedValue3"
        };

        private static readonly string[] RegisterUzValueColumns =
        {
            "RegisterUzValue1",
            "RegisterUzValue2",
            "RegisterUzValue3"
        };

        private static readonly string[] CalculationDifferenceColumns =
        {
            "Difference1",
            "Difference2",
            "Difference3"
        };

        private static readonly string[] GeneralLedgerAmountColumns =
        {
            "CalculatedOpeningDebit",
            "CalculatedOpeningCredit",
            "CalculatedDebitTurnover",
            "CalculatedCreditTurnover",
            "CalculatedClosingDebit",
            "CalculatedClosingCredit",
            "GeneralLedgerOpeningDebit",
            "GeneralLedgerOpeningCredit",
            "GeneralLedgerDebitTurnover",
            "GeneralLedgerCreditTurnover",
            "GeneralLedgerClosingDebit",
            "GeneralLedgerClosingCredit"
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

            Excel.Worksheet previous = FindWorksheet(workbook, "Navigation");

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

            if (SettingsService.Load().RoundCalculatedAmountsToWholeEuros)
                ApplyWholeEuroRounding(workbook);

            ApplyDiagnosticConditionalFormatting(workbook);
        }

        private static void ApplyWholeEuroRounding(Excel.Workbook workbook)
        {
            Excel.ListObject calculationResults =
                FindTable(workbook, CalculationResultsTableName);

            if (calculationResults?.DataBodyRange != null)
                RoundCalculationResults(calculationResults);

            Excel.ListObject generalLedgerComparison =
                FindTable(workbook, GeneralLedgerComparisonTableName);

            if (generalLedgerComparison?.DataBodyRange != null)
                RoundGeneralLedgerComparison(generalLedgerComparison);
        }

        private static void RoundCalculationResults(Excel.ListObject table)
        {
            int rowCount = table.DataBodyRange.Rows.Count;

            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                for (int slotIndex = 0; slotIndex < 3; slotIndex++)
                {
                    Excel.Range calculatedCell =
                        (Excel.Range)table.ListColumns[
                            CalculationValueColumns[slotIndex]].DataBodyRange.Cells[
                                rowIndex,
                                1];
                    Excel.Range registerUzCell =
                        (Excel.Range)table.ListColumns[
                            RegisterUzValueColumns[slotIndex]].DataBodyRange.Cells[
                                rowIndex,
                                1];
                    Excel.Range differenceCell =
                        (Excel.Range)table.ListColumns[
                            CalculationDifferenceColumns[slotIndex]].DataBodyRange.Cells[
                                rowIndex,
                                1];

                    decimal? calculated = RoundCell(calculatedCell);
                    decimal? registerUz = RoundCell(registerUzCell);

                    if (registerUz.HasValue && calculated.HasValue)
                    {
                        differenceCell.Value2 =
                            (double)(calculated.Value - registerUz.Value);
                    }
                    else
                    {
                        differenceCell.ClearContents();
                    }
                }
            }
        }

        private static void RoundGeneralLedgerComparison(Excel.ListObject table)
        {
            int rowCount = table.DataBodyRange.Rows.Count;

            foreach (string columnName in GeneralLedgerAmountColumns)
            {
                Excel.Range column = table.ListColumns[columnName].DataBodyRange;

                if (column == null)
                    continue;

                for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                    RoundCell((Excel.Range)column.Cells[rowIndex, 1]);
            }

            for (int rowIndex = 1; rowIndex <= rowCount; rowIndex++)
            {
                decimal? calculatedOpeningDebit = ReadDecimal(table, "CalculatedOpeningDebit", rowIndex);
                decimal? calculatedOpeningCredit = ReadDecimal(table, "CalculatedOpeningCredit", rowIndex);
                decimal? calculatedDebitTurnover = ReadDecimal(table, "CalculatedDebitTurnover", rowIndex);
                decimal? calculatedCreditTurnover = ReadDecimal(table, "CalculatedCreditTurnover", rowIndex);
                decimal? calculatedClosingDebit = ReadDecimal(table, "CalculatedClosingDebit", rowIndex);
                decimal? calculatedClosingCredit = ReadDecimal(table, "CalculatedClosingCredit", rowIndex);
                decimal? ledgerOpeningDebit = ReadDecimal(table, "GeneralLedgerOpeningDebit", rowIndex);
                decimal? ledgerOpeningCredit = ReadDecimal(table, "GeneralLedgerOpeningCredit", rowIndex);
                decimal? ledgerDebitTurnover = ReadDecimal(table, "GeneralLedgerDebitTurnover", rowIndex);
                decimal? ledgerCreditTurnover = ReadDecimal(table, "GeneralLedgerCreditTurnover", rowIndex);
                decimal? ledgerClosingDebit = ReadDecimal(table, "GeneralLedgerClosingDebit", rowIndex);
                decimal? ledgerClosingCredit = ReadDecimal(table, "GeneralLedgerClosingCredit", rowIndex);

                string status = Convert.ToString(
                    ((Excel.Range)table.ListColumns["Status"].DataBodyRange.Cells[
                        rowIndex,
                        1]).Value2,
                    CultureInfo.InvariantCulture);

                if (string.Equals(status, "Journal only", StringComparison.Ordinal) ||
                    string.Equals(status, "General-ledger only", StringComparison.Ordinal) ||
                    string.Equals(status, "No imported general ledger", StringComparison.Ordinal))
                {
                    continue;
                }

                WriteDifference(
                    table,
                    "DebitTurnoverDifference",
                    rowIndex,
                    calculatedDebitTurnover,
                    ledgerDebitTurnover);
                WriteDifference(
                    table,
                    "CreditTurnoverDifference",
                    rowIndex,
                    calculatedCreditTurnover,
                    ledgerCreditTurnover);

                bool openingAvailable = ReadBoolean(
                    table,
                    "OpeningAvailableFromJournal",
                    rowIndex);

                if (!openingAvailable)
                {
                    ClearCell(table, "OpeningDebitDifference", rowIndex);
                    ClearCell(table, "OpeningCreditDifference", rowIndex);
                    ClearCell(table, "CalculatedNetClosingBalance", rowIndex);
                    ClearCell(table, "GeneralLedgerNetClosingBalance", rowIndex);
                    ClearCell(table, "ClosingBalanceDifference", rowIndex);

                    bool turnoverMatches =
                        DifferenceIsZero(table, "DebitTurnoverDifference", rowIndex) &&
                        DifferenceIsZero(table, "CreditTurnoverDifference", rowIndex);

                    WriteText(
                        table,
                        "Status",
                        rowIndex,
                        turnoverMatches
                            ? "Turnover reconciled; opening unavailable"
                            : "Different; opening unavailable");
                    continue;
                }

                WriteDifference(
                    table,
                    "OpeningDebitDifference",
                    rowIndex,
                    calculatedOpeningDebit,
                    ledgerOpeningDebit);
                WriteDifference(
                    table,
                    "OpeningCreditDifference",
                    rowIndex,
                    calculatedOpeningCredit,
                    ledgerOpeningCredit);

                decimal? calculatedNetClosing =
                    Subtract(calculatedClosingDebit, calculatedClosingCredit);
                decimal? ledgerNetClosing =
                    Subtract(ledgerClosingDebit, ledgerClosingCredit);

                WriteDecimal(
                    table,
                    "CalculatedNetClosingBalance",
                    rowIndex,
                    calculatedNetClosing);
                WriteDecimal(
                    table,
                    "GeneralLedgerNetClosingBalance",
                    rowIndex,
                    ledgerNetClosing);
                WriteDifference(
                    table,
                    "ClosingBalanceDifference",
                    rowIndex,
                    calculatedNetClosing,
                    ledgerNetClosing);

                bool reconciled =
                    DifferenceIsZero(table, "OpeningDebitDifference", rowIndex) &&
                    DifferenceIsZero(table, "OpeningCreditDifference", rowIndex) &&
                    DifferenceIsZero(table, "DebitTurnoverDifference", rowIndex) &&
                    DifferenceIsZero(table, "CreditTurnoverDifference", rowIndex) &&
                    DifferenceIsZero(table, "ClosingBalanceDifference", rowIndex);

                WriteText(
                    table,
                    "Status",
                    rowIndex,
                    reconciled ? "Reconciled" : "Different");
            }
        }

        private static decimal? RoundCell(Excel.Range cell)
        {
            decimal? value = ReadNullableDecimal(cell.Value2);

            if (!value.HasValue)
                return null;

            decimal rounded = Math.Round(
                value.Value,
                0,
                MidpointRounding.AwayFromZero);
            cell.Value2 = (double)rounded;
            return rounded;
        }

        private static decimal? ReadDecimal(
            Excel.ListObject table,
            string columnName,
            int rowIndex)
        {
            Excel.Range cell =
                (Excel.Range)table.ListColumns[columnName].DataBodyRange.Cells[
                    rowIndex,
                    1];
            return ReadNullableDecimal(cell.Value2);
        }

        private static decimal? ReadNullableDecimal(object value)
        {
            if (value == null)
                return null;

            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private static bool ReadBoolean(
            Excel.ListObject table,
            string columnName,
            int rowIndex)
        {
            object value =
                ((Excel.Range)table.ListColumns[columnName].DataBodyRange.Cells[
                    rowIndex,
                    1]).Value2;
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        private static decimal? Subtract(decimal? left, decimal? right)
        {
            if (!left.HasValue || !right.HasValue)
                return null;

            return left.Value - right.Value;
        }

        private static void WriteDifference(
            Excel.ListObject table,
            string columnName,
            int rowIndex,
            decimal? left,
            decimal? right)
        {
            WriteDecimal(
                table,
                columnName,
                rowIndex,
                Subtract(left, right));
        }

        private static void WriteDecimal(
            Excel.ListObject table,
            string columnName,
            int rowIndex,
            decimal? value)
        {
            Excel.Range cell =
                (Excel.Range)table.ListColumns[columnName].DataBodyRange.Cells[
                    rowIndex,
                    1];

            if (value.HasValue)
                cell.Value2 = (double)value.Value;
            else
                cell.ClearContents();
        }

        private static void ClearCell(
            Excel.ListObject table,
            string columnName,
            int rowIndex)
        {
            ((Excel.Range)table.ListColumns[columnName].DataBodyRange.Cells[
                rowIndex,
                1]).ClearContents();
        }

        private static void WriteText(
            Excel.ListObject table,
            string columnName,
            int rowIndex,
            string value)
        {
            ((Excel.Range)table.ListColumns[columnName].DataBodyRange.Cells[
                rowIndex,
                1]).Value2 = value;
        }

        private static bool DifferenceIsZero(
            Excel.ListObject table,
            string columnName,
            int rowIndex)
        {
            decimal? value = ReadDecimal(table, columnName, rowIndex);
            return value.HasValue && value.Value == 0m;
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
