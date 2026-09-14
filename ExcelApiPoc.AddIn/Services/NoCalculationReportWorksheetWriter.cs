using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AddIn.Models;
using System;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class NoCalculationReportWorksheetWriter
    {
        private const string WorksheetName = "No Calculation Report";

        public static Excel.Worksheet AddWorksheet(
            Excel.Workbook workbook,
            JournalImport journalImport,
            AccountingFrameworkImport accountingFrameworkImport,
            GeneralLedgerImport generalLedgerImport,
            Exception calculationFailure)
        {
            if (workbook == null)
                throw new ArgumentNullException(nameof(workbook));
            if (journalImport == null)
                throw new ArgumentNullException(nameof(journalImport));
            if (calculationFailure == null)
                throw new ArgumentNullException(nameof(calculationFailure));

            Excel.Worksheet worksheet =
                (Excel.Worksheet)workbook.Worksheets.Add(
                    After: workbook.Worksheets[workbook.Worksheets.Count]);
            worksheet.Name = WorksheetName;

            worksheet.Cells[1, 1] = "Calculation report unavailable";
            Excel.Range title = worksheet.Range["A1:B1"];
            title.Merge();
            title.Font.Bold = true;
            title.Font.Size = 16;
            title.Interior.Color = 10092543;

            WriteValue(worksheet, 3, "Status", "Source data imported successfully");
            WriteValue(worksheet, 4, "Calculation", "Not available");
            WriteValue(worksheet, 5, "Reason", calculationFailure.Message);
            WriteValue(
                worksheet,
                6,
                "Exception type",
                calculationFailure.GetType().FullName);

            WriteValue(worksheet, 8, "Accounting format", journalImport.AccountingFormat);
            WriteValue(worksheet, 9, "IČO", journalImport.Ico);
            WriteValue(worksheet, 10, "Fiscal year", journalImport.FiscalYear);

            WriteValue(
                worksheet,
                12,
                "Accounting journal",
                journalImport.SourceFileName);
            WriteValue(
                worksheet,
                13,
                "Journal rows",
                journalImport.Rows.Count);

            WriteValue(
                worksheet,
                15,
                "General ledger",
                generalLedgerImport == null
                    ? "Not provided"
                    : generalLedgerImport.SourceFileName);
            WriteValue(
                worksheet,
                16,
                "General-ledger rows",
                generalLedgerImport == null
                    ? 0
                    : generalLedgerImport.Rows.Count);

            WriteValue(
                worksheet,
                18,
                "Accounting framework",
                accountingFrameworkImport == null
                    ? "Not provided"
                    : accountingFrameworkImport.SourceFileName);
            WriteValue(
                worksheet,
                19,
                "Accounting-framework rows",
                accountingFrameworkImport == null
                    ? 0
                    : accountingFrameworkImport.Rows.Count);

            worksheet.Columns[1].ColumnWidth = 28;
            worksheet.Columns[2].ColumnWidth = 100;
            worksheet.Columns[2].WrapText = true;
            worksheet.Rows.AutoFit();
            worksheet.Activate();

            return worksheet;
        }

        private static void WriteValue(
            Excel.Worksheet worksheet,
            int row,
            string label,
            object value)
        {
            Excel.Range labelCell = (Excel.Range)worksheet.Cells[row, 1];
            Excel.Range valueCell = (Excel.Range)worksheet.Cells[row, 2];

            labelCell.Value2 = label;
            labelCell.Font.Bold = true;
            valueCell.Value2 = value;
        }
    }
}
