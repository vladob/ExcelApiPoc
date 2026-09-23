using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountDetailSnapshot
    {
        private const string SheetName = "_AccountDetailSettings";

        public static void Create(Excel.Workbook workbook)
        {
            AccountDetailTextSettings texts = AccountDetailSettingsApiClient.GetTexts();
            AccountDetailLayout layout = AccountDetailSettingsApiClient.GetLayout();
            if (texts?.Categories == null || texts.Texts == null || texts.Defaults == null || layout?.Definition == null)
                throw new InvalidOperationException("The account detail settings response is incomplete.");

            Excel.Worksheet sheet = (Excel.Worksheet)workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
            sheet.Name = SheetName;
            WriteTable(sheet, 1, "AccountDetailCategories", new[] { "Code", "DisplayNameSk", "SortOrder" },
                texts.Categories.Select(x => new object[] { x.Code, x.DisplayNameSk, x.SortOrder }).ToArray());
            WriteTable(sheet, 5, "AccountDetailTexts", new[] { "TextId", "CategoryCode", "TextSk", "SortOrder" },
                texts.Texts.Select(x => new object[] { x.TextId, x.CategoryCode, x.TextSk, x.SortOrder }).ToArray());
            WriteTable(sheet, 11, "AccountDetailDefaults", new[] { "Account", "CategoryCode", "TextId" },
                texts.Defaults.Select(x => new object[] { x.Account, x.CategoryCode, x.TextId }).ToArray());
            ((Excel.Range)sheet.Cells[1, 16]).Value2 = layout.VersionNo;
            ((Excel.Range)sheet.Cells[2, 16]).Value2 = layout.Definition.ToString();
            ((Excel.Range)sheet.Cells[3, 16]).Value2 = KeyFingerprint(SettingsService.Load().ApiKey);
            sheet.Visible = Excel.XlSheetVisibility.xlSheetVeryHidden;
        }

        public static bool Exists(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == SheetName) return true;
            return false;
        }

        public static AccountDetailLayout ReadLayout(Excel.Workbook workbook)
        {
            Excel.Worksheet sheet = Find(workbook);
            return new AccountDetailLayout
            {
                VersionNo = Convert.ToInt32(((Excel.Range)sheet.Cells[1, 16]).Value2),
                Definition = Newtonsoft.Json.Linq.JObject.Parse((string)((Excel.Range)sheet.Cells[2, 16]).Value2)
            };
        }

        public static AccountDetailTextSettings ReadTexts(Excel.Workbook workbook)
        {
            var categories = AuditWorkbookTableReader.ReadRows(workbook, "AccountDetailCategories")
                .Select(r => new AccountDetailCategory { Code = AuditWorkbookTableReader.GetString(r, "Code"),
                    DisplayNameSk = AuditWorkbookTableReader.GetString(r, "DisplayNameSk"), SortOrder = AuditWorkbookTableReader.GetInt32(r, "SortOrder") }).ToList();
            var texts = AuditWorkbookTableReader.ReadRows(workbook, "AccountDetailTexts")
                .Select(r => new AccountDetailText { TextId = AuditWorkbookTableReader.GetInt32(r, "TextId"),
                    CategoryCode = AuditWorkbookTableReader.GetString(r, "CategoryCode"),
                    TextSk = AuditWorkbookTableReader.GetString(r, "TextSk"), SortOrder = AuditWorkbookTableReader.GetInt32(r, "SortOrder") }).ToList();
            var defaults = AuditWorkbookTableReader.ReadRows(workbook, "AccountDetailDefaults")
                .Select(r => new AccountDetailDefault { Account = AuditWorkbookTableReader.GetString(r, "Account"),
                    CategoryCode = AuditWorkbookTableReader.GetString(r, "CategoryCode"), TextId = AuditWorkbookTableReader.GetInt32(r, "TextId") }).ToList();
            return new AccountDetailTextSettings { Categories = categories, Texts = texts, Defaults = defaults };
        }

        public static void ReplaceTexts(Excel.Workbook workbook, AccountDetailTextSettings settings)
        {
            Excel.Worksheet sheet = Find(workbook);
            foreach (string tableName in new[] { "AccountDetailCategories", "AccountDetailTexts", "AccountDetailDefaults" })
            {
                Excel.ListObject table = sheet.ListObjects[tableName];
                table.Delete();
            }
            sheet.Range["A1:N2000"].ClearContents();
            WriteTable(sheet, 1, "AccountDetailCategories", new[] { "Code", "DisplayNameSk", "SortOrder" },
                settings.Categories.Select(x => new object[] { x.Code, x.DisplayNameSk, x.SortOrder }).ToArray());
            WriteTable(sheet, 5, "AccountDetailTexts", new[] { "TextId", "CategoryCode", "TextSk", "SortOrder" },
                settings.Texts.Select(x => new object[] { x.TextId, x.CategoryCode, x.TextSk, x.SortOrder }).ToArray());
            WriteTable(sheet, 11, "AccountDetailDefaults", new[] { "Account", "CategoryCode", "TextId" },
                settings.Defaults.Select(x => new object[] { x.Account, x.CategoryCode, x.TextId }).ToArray());
            foreach (AccountDetailCategory category in settings.Categories)
            {
                int column = 20 + settings.Categories.IndexOf(category);
                sheet.Range[sheet.Cells[2, column], sheet.Cells[1000, column]].ClearContents();
                var choices = settings.Texts.Where(x => x.CategoryCode == category.Code).OrderBy(x => x.SortOrder).ToList();
                for (int i = 0; i < choices.Count; i++) ((Excel.Range)sheet.Cells[i + 2, column]).Value2 = choices[i].TextSk;
                string name = "AccountDetailChoices_" + category.Code;
                foreach (Excel.Name existing in workbook.Names)
                    if (existing.Name == name)
                    {
                        existing.RefersTo = "='" + sheet.Name + "'!" + sheet.Range[
                            sheet.Cells[2, column], sheet.Cells[Math.Max(2, choices.Count + 1), column]].Address;
                        break;
                    }
            }
        }

        public static Excel.Worksheet Find(Excel.Workbook workbook)
        {
            foreach (Excel.Worksheet sheet in workbook.Worksheets)
                if (sheet.Name == SheetName) return sheet;
            throw new InvalidOperationException("This workbook has no account detail settings snapshot.");
        }

        public static void EnsureCurrentAuditor(Excel.Workbook workbook)
        {
            string saved = Convert.ToString(((Excel.Range)Find(workbook).Cells[3, 16]).Value2);
            if (!string.Equals(saved, KeyFingerprint(SettingsService.Load().ApiKey), StringComparison.Ordinal))
                throw new InvalidOperationException("The current API key belongs to a different auditor than this workbook's predefined texts.");
        }

        private static string KeyFingerprint(string key)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes((key ?? "").Trim().ToUpperInvariant()))).Replace("-", "");
        }

        private static void WriteTable(Excel.Worksheet sheet, int column, string name, string[] headers, object[][] rows)
        {
            for (int index = 0; index < headers.Length; index++) ((Excel.Range)sheet.Cells[1, column + index]).Value2 = headers[index];
            for (int row = 0; row < rows.Length; row++)
                for (int index = 0; index < headers.Length; index++) ((Excel.Range)sheet.Cells[row + 2, column + index]).Value2 = rows[row][index];
            Excel.Range range = sheet.Range[sheet.Cells[1, column], sheet.Cells[rows.Length + 1, column + headers.Length - 1]];
            Excel.ListObject table = sheet.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange, range,
                Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
            table.Name = name;
        }
    }
}
