using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountDetailWorksheetWriter
    {
        public static int CreateMissing(Excel.Workbook workbook)
        {
            if (!AccountDetailSnapshot.Exists(workbook))
                throw new InvalidOperationException("This workbook has no account detail settings snapshot. Create a new audit workbook first.");
            AccountDetailLayout layout = AccountDetailSnapshot.ReadLayout(workbook);
            AccountDetailTextSettings texts = AccountDetailSnapshot.ReadTexts(workbook);
            JObject definition = layout.Definition;
            if (layout.VersionNo != 1 || (int?)definition["schemaVersion"] != 1)
                throw new InvalidOperationException("Unsupported account detail layout version.");
            int titleRow = (int)definition["titleRow"];
            int headerRow = (int)definition["detailTable"]["headerRow"];
            int gapRows = (int)definition["textSection"]["gapRows"];
            JArray columns = (JArray)definition["detailTable"]["columns"];
            if (titleRow < 4 || headerRow <= titleRow || columns.Count != 8)
                throw new InvalidOperationException("The account detail layout has unsupported row or column positions.");

            var journal = AuditWorkbookTableReader.ReadRows(workbook, "JournalRows");
            var groups = new SortedDictionary<string, List<object[]>>(StringComparer.Ordinal);
            foreach (IDictionary<string, object> row in journal)
            {
                AddSide(groups, row, "DebitAccount", "DebitAmount", true);
                AddSide(groups, row, "CreditAccount", "CreditAmount", false);
            }
            int created = 0;
            try
            {
                foreach (KeyValuePair<string, List<object[]>> group in groups)
                {
                    if (Find(workbook, group.Key) != null) continue;
                    if ((long)headerRow + group.Value.Count + gapRows + texts.Categories.Count + 2 > 1048576)
                        throw new InvalidOperationException("Account " + group.Key + " exceeds the Excel worksheet row limit.");
                    Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                    sheet.Name = group.Key;
                    try
                    {
                        Render(workbook, sheet, group.Key, group.Value, definition, texts, columns, titleRow, headerRow, gapRows);
                        AuditNavigationWorksheet.AddReturnLink(sheet);
                        created++;
                    }
                    catch
                    {
                        bool alerts = workbook.Application.DisplayAlerts;
                        try { workbook.Application.DisplayAlerts = false; sheet.Delete(); }
                        finally { workbook.Application.DisplayAlerts = alerts; }
                        throw;
                    }
                }
            }
            finally { AuditNavigationWorksheet.Refresh(workbook); }
            return created;
        }

        private static void AddSide(SortedDictionary<string, List<object[]>> groups,
            IDictionary<string, object> row, string accountColumn, string amountColumn, bool debit)
        {
            string account = AuditWorkbookTableReader.GetString(row, accountColumn).Trim();
            if (account.Length < 3 || !account.Take(3).All(char.IsDigit)) return;
            string synthetic = account.Substring(0, 3);
            if (!groups.TryGetValue(synthetic, out List<object[]> entries))
                groups[synthetic] = entries = new List<object[]>();
            object amount = row[amountColumn];
            entries.Add(new object[] {
                AuditWorkbookTableReader.GetString(row, "DocumentNumber"), synthetic, account,
                AuditWorkbookTableReader.GetString(row, "Description"),
                debit ? amount : null, debit ? null : amount,
                AuditWorkbookTableReader.GetDateTime(row, "PostingDate").ToOADate(), null
            });
        }

        private static void Render(Excel.Workbook workbook, Excel.Worksheet sheet, string account,
            List<object[]> entries, JObject definition, AccountDetailTextSettings texts,
            JArray columns, int titleRow, int headerRow, int gapRows)
        {
            ((Excel.Range)sheet.Cells[titleRow, 1]).Value2 = "Účet " + account;
            ((Excel.Range)sheet.Cells[titleRow, 1]).Font.Bold = true;
            ((Excel.Range)sheet.Cells[(int)definition["countRow"], 1]).Value2 = entries.Count;
            for (int c = 0; c < columns.Count; c++)
            {
                ((Excel.Range)sheet.Cells[headerRow, c + 1]).Value2 = (string)columns[c]["captionSk"];
                ((Excel.Range)sheet.Columns[c + 1]).ColumnWidth = (double)columns[c]["width"];
            }
            if (entries.Count > 0)
            {
                var data = new object[entries.Count, columns.Count];
                for (int r = 0; r < entries.Count; r++)
                    for (int c = 0; c < columns.Count; c++) data[r, c] = entries[r][c];
                sheet.Range[sheet.Cells[headerRow + 1, 1], sheet.Cells[headerRow + entries.Count, columns.Count]].Value2 = data;
            }
            Excel.Range range = sheet.Range[sheet.Cells[headerRow, 1], sheet.Cells[headerRow + entries.Count, columns.Count]];
            Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
                range, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = "AccountDetail_" + account;
            table.TableStyle = (string)definition["detailTable"]["style"];
            sheet.Range[sheet.Cells[headerRow + 1, 5], sheet.Cells[headerRow + entries.Count, 6]].NumberFormat = "#,##0.00";
            sheet.Range[sheet.Cells[headerRow + 1, 7], sheet.Cells[headerRow + entries.Count, 7]].NumberFormat = "dd.mm.yyyy";

            int textRow = headerRow + entries.Count + gapRows + 1;
            foreach (JToken categoryToken in (JArray)definition["textSection"]["categoryOrder"])
            {
                string code = (string)categoryToken;
                AccountDetailCategory category = texts.Categories.FirstOrDefault(x => x.Code == code);
                if (category == null) continue;
                ((Excel.Range)sheet.Cells[textRow, 1]).Value2 = category.DisplayNameSk;
                ((Excel.Range)sheet.Cells[textRow, 1]).Font.Bold = true;
                Excel.Range cell = (Excel.Range)sheet.Cells[textRow, 4];
                AccountDetailDefault selection = texts.Defaults.FirstOrDefault(x => x.Account == account && x.CategoryCode == code);
                AccountDetailText initial = texts.Texts.FirstOrDefault(x => x.CategoryCode == code && x.TextId == selection?.TextId);
                if (initial != null) cell.Value2 = initial.TextSk;
                // Excel accepts typed values outside the suggestion list when ShowError is false.
                string listName = EnsureChoiceRange(workbook, texts, code);
                if (listName != null)
                {
                    cell.Validation.Add(Excel.XlDVType.xlValidateList, Excel.XlDVAlertStyle.xlValidAlertInformation,
                        Excel.XlFormatConditionOperator.xlBetween, "=" + listName, Type.Missing);
                    cell.Validation.InCellDropdown = true;
                    cell.Validation.ShowError = false;
                }
                textRow++;
            }
            ((Excel.Range)sheet.Columns[4]).ColumnWidth = 75;
            sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            sheet.PageSetup.PrintArea = sheet.Range[sheet.Cells[(int)definition["print"]["startRow"], 1],
                sheet.Cells[textRow, 8]].Address;
        }

        private static string EnsureChoiceRange(Excel.Workbook workbook, AccountDetailTextSettings texts, string code)
        {
            Excel.Worksheet sheet = AccountDetailSnapshot.Find(workbook);
            var list = texts.Texts.Where(x => x.CategoryCode == code).OrderBy(x => x.SortOrder).ToList();
            if (list.Count == 0) return null;
            string name = "AccountDetailChoices_" + code;
            int column = 20 + texts.Categories.FindIndex(x => x.Code == code);
            for (int i = 0; i < list.Count; i++) ((Excel.Range)sheet.Cells[i + 2, column]).Value2 = list[i].TextSk;
            foreach (Excel.Name existing in workbook.Names)
                if (existing.Name == name) return name;
            workbook.Names.Add(name, "='" + sheet.Name + "'!" +
                sheet.Range[sheet.Cells[2, column], sheet.Cells[list.Count + 1, column]].Address);
            return name;
        }

        private static Excel.Worksheet Find(Excel.Workbook workbook, string name)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (string.Equals(sheet.Name, name, StringComparison.OrdinalIgnoreCase)) return sheet;
            return null;
        }
    }
}
