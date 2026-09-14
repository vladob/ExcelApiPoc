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
