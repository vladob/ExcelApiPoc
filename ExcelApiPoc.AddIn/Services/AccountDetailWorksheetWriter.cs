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
            try { AuditNavigationWorksheet.EnsureWorkbookSignificance(workbook); }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Could not initialize the workbook significance on Navigation: " + exception.Message, exception);
            }

            var journal = AuditWorkbookTableReader.ReadRows(workbook, "JournalRows");
            var groups = new SortedDictionary<string, List<object[]>>(StringComparer.Ordinal);
            foreach (IDictionary<string, object> row in journal)
            {
                AddSide(groups, row, "DebitAccount", "DebitAmount", true);
                AddSide(groups, row, "CreditAccount", "CreditAmount", false);
            }
            int created = 0;
            Excel.Worksheet previousSheet = (Excel.Worksheet)workbook.ActiveSheet;
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
                    catch (Exception exception)
                    {
                        try
                        {
                            bool alerts = workbook.Application.DisplayAlerts;
                            try { workbook.Application.DisplayAlerts = false; sheet.Delete(); }
                            finally { workbook.Application.DisplayAlerts = alerts; }
                        }
                        catch { /* Keep the original rendering error. */ }
                        throw new InvalidOperationException("Account " + group.Key + ": " + exception.Message, exception);
                    }
                }
            }
            finally
            {
                try { previousSheet.Activate(); }
                finally { AuditNavigationWorksheet.Refresh(workbook); }
            }
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
            string step = "writing the account detail table";
            try
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
                    Excel.Range dataRange = sheet.Range[sheet.Cells[headerRow + 1, 1],
                        sheet.Cells[headerRow + entries.Count, columns.Count]];
                    foreach (int column in new[] { 1, 2, 3 })
                        ((Excel.Range)dataRange.Columns[column]).NumberFormat = "@";
                    ((Excel.Range)dataRange.Columns[5]).NumberFormat = "#,##0.00";
                    ((Excel.Range)dataRange.Columns[6]).NumberFormat = "#,##0.00";
                    ((Excel.Range)dataRange.Columns[7]).NumberFormat = "yyyy-mm-dd";
                    var data = new object[entries.Count, columns.Count];
                    for (int r = 0; r < entries.Count; r++)
                        for (int c = 0; c < columns.Count; c++) data[r, c] = entries[r][c];
                    dataRange.Value2 = data;
                }
                Excel.Range range = sheet.Range[sheet.Cells[headerRow, 1], sheet.Cells[headerRow + entries.Count, columns.Count]];
                Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
                    range, Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
                table.Name = "AccountDetail_" + account;
                table.TableStyle = (string)definition["detailTable"]["style"];

                step = "creating the worksheet significance name";
                ((Excel.Range)sheet.Cells[1, 4]).Value2 = "Worksheet implementation significance:";
                ((Excel.Range)sheet.Cells[1, 4]).HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                Excel.Range significance = (Excel.Range)sheet.Cells[1, 5];
                significance.Value2 = 0;
                significance.NumberFormat = "#,##0.00 \"€\"";
                sheet.Names.Add(Name: "_WS_significance", RefersTo: "='" + sheet.Name.Replace("'", "''") + "'!$E$1");
                step = "freezing the account header";
                sheet.Activate();
                Excel.Window window = workbook.Application.ActiveWindow;
                window.FreezePanes = false;
                window.SplitColumn = 0;
                window.SplitRow = titleRow;
                window.FreezePanes = true;

                step = "adding the amount conditional formatting";
                if (entries.Count > 0)
                {
                    Excel.Range amounts = sheet.Range[sheet.Cells[headerRow + 1, 5],
                        sheet.Cells[headerRow + entries.Count, 6]];
                    string threshold = "=IF(_WS_significance=0,_WB_significance,_WS_significance)";
                    Excel.FormatCondition rule = (Excel.FormatCondition)amounts.FormatConditions.Add(
                        Excel.XlFormatConditionType.xlCellValue,
                        Excel.XlFormatConditionOperator.xlGreaterEqual, threshold);
                    step = "styling the amount conditional formatting";
                    rule.Font.Bold = true;
                    rule.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 211, 217));
                }

                step = "adding predefined text choices";
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
                step = "setting the print area";
                sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                sheet.PageSetup.PrintArea = sheet.Range[sheet.Cells[(int)definition["print"]["startRow"], 1],
                    sheet.Cells[textRow, 8]].Address;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed while " + step + ": " + exception.Message, exception);
            }
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
