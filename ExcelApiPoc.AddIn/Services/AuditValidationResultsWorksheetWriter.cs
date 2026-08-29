using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditValidationResultsWorksheetWriter
    {
        private const string WorksheetName = "__Metadata";
        private const string TableName = "__ValidationResults";
        private const int HeaderRow = 9;

        private static readonly string[] Headers =
        {
            "Category", "CheckCode", "Status", "Actual", "Expected",
            "Message", "CheckedAtUtc"
        };

        public static void Write(
            Excel.Workbook workbook,
            IReadOnlyList<AccountSummary> accounts,
            AnalyticalMappingSelectionReadResult mappingSelections,
            AuditTemplatePackageResponse package,
            AuditReportCalculationResult calculation)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));
            if (accounts == null) throw new ArgumentNullException(nameof(accounts));
            if (mappingSelections == null) throw new ArgumentNullException(nameof(mappingSelections));
            if (package == null || package.Template == null)
                throw new ArgumentNullException(nameof(package));
            if (calculation == null) throw new ArgumentNullException(nameof(calculation));

            IReadOnlyList<IDictionary<string, object>> importRows =
                AuditWorkbookTableReader.ReadRows(workbook, "__ImportSources");

            if (importRows.Count != 1)
                throw new InvalidOperationException("Table '__ImportSources' must contain exactly one row.");

            IDictionary<string, object> import = importRows[0];
            DateTime dateFrom = AuditWorkbookTableReader.GetDateTime(import, "DateFrom");
            DateTime dateTo = AuditWorkbookTableReader.GetDateTime(import, "DateTo");
            int fiscalYear = AuditWorkbookTableReader.GetInt32(import, "SelectedFiscalYear");
            int rejectedRecords = AuditWorkbookTableReader.GetInt32(import, "RejectedRecordCount");
            decimal debitTurnover = accounts.Sum(account => account.DebitTurnover);
            decimal creditTurnover = accounts.Sum(account => account.CreditTurnover);
            decimal journalDifference = debitTurnover - creditTurnover;
            int frameworkMatchedAccounts = accounts.Count(account =>
                !string.IsNullOrWhiteSpace(account.FrameworkAccountCode));
            int unresolvedAccounts = mappingSelections.UnresolvedAccountCodes.Count;
            int pendingRequirements = calculation.AnalyticalRequirements.Count;
            int unmappedAccounts = calculation.UnmappedAccounts.Count;
            int expectedReportRows =
                (package.Template.Tables ?? Array.Empty<AuditReportTableDefinitionResponse>())
                .Sum(table =>
                    (table.Rows ?? Array.Empty<AuditReportRowDefinitionResponse>())
                    .Count(row => row.RowNumber.HasValue));
            bool fiscalYearMatches =
                dateFrom.Year == fiscalYear && dateTo.Year == fiscalYear;
            DateTime checkedAtUtc = DateTime.UtcNow;

            ValidationRow[] rows =
            {
                Check(
                    "Journal", "FiscalYearMatches", fiscalYearMatches,
                    $"{dateFrom:yyyy-MM-dd} – {dateTo:yyyy-MM-dd}",
                    fiscalYear.ToString(CultureInfo.InvariantCulture),
                    fiscalYearMatches ? string.Empty :
                        "Journal dates extend outside the selected fiscal year.",
                    checkedAtUtc),
                Check(
                    "Journal", "RejectedRecords", rejectedRecords == 0,
                    rejectedRecords.ToString(CultureInfo.InvariantCulture), "0",
                    rejectedRecords == 0 ? string.Empty :
                        "One or more source records were rejected.",
                    checkedAtUtc),
                Check(
                    "Journal", "DebitEqualsCredit", journalDifference == 0,
                    journalDifference.ToString("0.00", CultureInfo.InvariantCulture), "0.00",
                    journalDifference == 0 ? string.Empty :
                        "Debit and credit turnover are not equal.",
                    checkedAtUtc),
                Check(
                    "Accounts", "FrameworkAccountsMatched",
                    frameworkMatchedAccounts == accounts.Count,
                    frameworkMatchedAccounts.ToString(CultureInfo.InvariantCulture),
                    accounts.Count.ToString(CultureInfo.InvariantCulture),
                    frameworkMatchedAccounts == accounts.Count ? string.Empty :
                        "One or more accounts were not matched to the account framework.",
                    checkedAtUtc),
                Check(
                    "Mapping", "UnresolvedAnalyticalAccounts", unresolvedAccounts == 0,
                    unresolvedAccounts.ToString(CultureInfo.InvariantCulture), "0",
                    unresolvedAccounts == 0 ? string.Empty :
                        "Analytical mapping is still required.",
                    checkedAtUtc),
                Check(
                    "Report", "CalculatedReportRows",
                    calculation.Rows.Count == expectedReportRows,
                    calculation.Rows.Count.ToString(CultureInfo.InvariantCulture),
                    expectedReportRows.ToString(CultureInfo.InvariantCulture),
                    calculation.Rows.Count == expectedReportRows ? string.Empty :
                        "The number of calculated rows does not match the report template.",
                    checkedAtUtc),
                Check(
                    "Report", "PendingAnalyticalRequirements", pendingRequirements == 0,
                    pendingRequirements.ToString(CultureInfo.InvariantCulture), "0",
                    pendingRequirements == 0 ? string.Empty :
                        "Report rows still have pending analytical requirements.",
                    checkedAtUtc),
                Check(
                    "Report", "UnmappedAccounts", unmappedAccounts == 0,
                    unmappedAccounts.ToString(CultureInfo.InvariantCulture), "0",
                    unmappedAccounts == 0 ? string.Empty :
                        "One or more nonzero accounts are not covered by report mapping rules.",
                    checkedAtUtc),
                Check(
                    "Report", "CalculationComplete", calculation.IsComplete,
                    calculation.IsComplete ? "Yes" : "No", "Yes",
                    calculation.IsComplete ? string.Empty :
                        "The report calculation requires additional analytical mapping.",
                    checkedAtUtc)
            };

            WriteTable(FindMetadataWorksheet(workbook), rows);
        }

        private static ValidationRow Check(
            string category,
            string checkCode,
            bool passed,
            string actual,
            string expected,
            string message,
            DateTime checkedAtUtc)
        {
            return new ValidationRow
            {
                Category = category,
                CheckCode = checkCode,
                Status = passed ? "Passed" : "Warning",
                Actual = actual,
                Expected = expected,
                Message = message,
                CheckedAtUtc = checkedAtUtc
            };
        }

        private static void WriteTable(Excel.Worksheet worksheet, IReadOnlyList<ValidationRow> rows)
        {
            var values = new object[rows.Count + 1, Headers.Length];
            var dataValues = new object[rows.Count, Headers.Length];

            for (int columnIndex = 0; columnIndex < Headers.Length; columnIndex++)
                values[0, columnIndex] = Headers[columnIndex];

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                ValidationRow row = rows[rowIndex];
                values[rowIndex + 1, 0] = row.Category;
                values[rowIndex + 1, 1] = row.CheckCode;
                values[rowIndex + 1, 2] = row.Status;
                values[rowIndex + 1, 3] = row.Actual;
                values[rowIndex + 1, 4] = row.Expected;
                values[rowIndex + 1, 5] = row.Message;
                values[rowIndex + 1, 6] = row.CheckedAtUtc;

                dataValues[rowIndex, 0] = row.Category;
                dataValues[rowIndex, 1] = row.CheckCode;
                dataValues[rowIndex, 2] = row.Status;
                dataValues[rowIndex, 3] = row.Actual;
                dataValues[rowIndex, 4] = row.Expected;
                dataValues[rowIndex, 5] = row.Message;
                dataValues[rowIndex, 6] = row.CheckedAtUtc;
            }

            Excel.ListObject table = FindTable(worksheet);

            if (table == null)
            {
                Excel.Range firstCell = (Excel.Range)worksheet.Cells[HeaderRow, 1];
                Excel.Range lastCell = (Excel.Range)worksheet.Cells[
                    HeaderRow + rows.Count,
                    Headers.Length];
                Excel.Range range = worksheet.Range[firstCell, lastCell];
                range.Value2 = values;
                table = worksheet.ListObjects.Add(
                    Excel.XlListObjectSourceType.xlSrcRange,
                    range,
                    Type.Missing,
                    Excel.XlYesNoGuess.xlYes,
                    Type.Missing);
                table.Name = TableName;
                table.TableStyle = "TableStyleMedium2";
                ((Excel.Range)table.DataBodyRange.Columns[7]).NumberFormat =
                    "yyyy-mm-dd hh:mm:ss";
            }
            else
            {
                if (table.Range.Rows.Count != rows.Count + 1 ||
                    table.Range.Columns.Count != Headers.Length)
                {
                    throw new InvalidOperationException(
                        $"Table '{TableName}' has an unexpected shape.");
                }

                for (int columnIndex = 1; columnIndex <= Headers.Length; columnIndex++)
                {
                    if (!string.Equals(
                        table.ListColumns[columnIndex].Name,
                        Headers[columnIndex - 1],
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Table '{TableName}' has an unexpected column layout.");
                    }
                }

                Excel.Range dataBodyRange = table.DataBodyRange;

                if (dataBodyRange == null)
                {
                    throw new InvalidOperationException(
                        $"Table '{TableName}' does not contain validation rows.");
                }

                dataBodyRange.Value2 = dataValues;
            }
        }

        private static Excel.Worksheet FindMetadataWorksheet(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet worksheet in workbook.Worksheets)
            {
                if (string.Equals(
                    worksheet.Name,
                    WorksheetName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return worksheet;
                }
            }

            throw new InvalidOperationException(
                $"The workbook does not contain worksheet '{WorksheetName}'.");
        }

        private static Excel.ListObject FindTable(Excel.Worksheet worksheet)
        {
            foreach (Excel.ListObject table in worksheet.ListObjects)
            {
                if (string.Equals(table.Name, TableName, StringComparison.OrdinalIgnoreCase))
                    return table;
            }

            return null;
        }

        private sealed class ValidationRow
        {
            public string Category { get; set; }
            public string CheckCode { get; set; }
            public string Status { get; set; }
            public string Actual { get; set; }
            public string Expected { get; set; }
            public string Message { get; set; }
            public DateTime CheckedAtUtc { get; set; }
        }
    }
}
