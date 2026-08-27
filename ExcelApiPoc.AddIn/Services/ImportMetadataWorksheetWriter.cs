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
            "FiscalYear",
            "DateFrom",
            "DateTo",
            "ImportedAtUtc",
            "RecordCount",
            "NormalizedTextFieldCount"
        };

        public static Excel.Worksheet AddWorksheet(Excel.Workbook workbook,JournalImport journalImport)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook));
            }

            if (journalImport == null)
            {
                throw new ArgumentNullException(nameof(journalImport));
            }

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
            values[1, 9] = dateFrom;
            values[1, 10] = dateTo;
            values[1, 11] = journalImport.ImportedAtUtc;
            values[1, 12] = journalImport.Rows.Count;
            values[1, 13] = journalImport.NormalizedTextFieldCount;

            Excel.Range firstCell = (Excel.Range)worksheet.Cells[1, 1];
            Excel.Range lastCell = (Excel.Range)worksheet.Cells[2, Headers.Length];
            Excel.Range tableRange = worksheet.Range[firstCell, lastCell];

            // Preserve identifiers and provenance values exactly.
            Excel.Range textRange = worksheet.Range["A2:H2"];
            textRange.NumberFormat = "@";
            tableRange.Value2 = values;

            Excel.Range fiscalYearCell = (Excel.Range)worksheet.Cells[2, 9];
            Excel.Range dateFromCell = (Excel.Range)worksheet.Cells[2, 10];
            Excel.Range dateToCell = (Excel.Range)worksheet.Cells[2, 11];
            Excel.Range importedAtUtcCell = (Excel.Range)worksheet.Cells[2, 12];
            Excel.Range recordCountCell = (Excel.Range)worksheet.Cells[2, 13];
            Excel.Range normalizedCountCell = (Excel.Range)worksheet.Cells[2, 14];

            fiscalYearCell.NumberFormat = "0";
            dateFromCell.NumberFormat = "yyyy-mm-dd";
            dateToCell.NumberFormat = "yyyy-mm-dd";
            importedAtUtcCell.NumberFormat = "yyyy-mm-dd hh:mm:ss";
            recordCountCell.NumberFormat = "#,##0";
            normalizedCountCell.NumberFormat = "#,##0";

            Excel.ListObject table = worksheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = TableName;
            table.TableStyle = "TableStyleMedium2";
            // Users may unhide it for provenance inspection.
            worksheet.Visible = Excel.XlSheetVisibility.xlSheetHidden;

            return worksheet;
        }
    }
}