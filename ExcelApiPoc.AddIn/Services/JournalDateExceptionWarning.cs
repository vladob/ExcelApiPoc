using ExcelApiPoc.AccountingImport.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class JournalDateExceptionWarning
    {
        public static void Show(JournalImport journalImport)
        {
            if (journalImport == null)
                return;

            JournalRow[] exceptionalRows = journalImport.Rows
                .Where(row => row.DateExceptionResolution.HasValue)
                .ToArray();

            if (exceptionalRows.Length == 0)
                return;

            DateTime firstDate = exceptionalRows.Min(row => row.PostingDate);
            DateTime lastDate = exceptionalRows.Max(row => row.PostingDate);
            string language = SettingsService.NormalizeUiLanguage(
                SettingsService.Load().UiLanguage);

            bool slovak = string.Equals(
                language,
                SettingsService.SlovakUiLanguage,
                StringComparison.Ordinal);

            string title = slovak
                ? "Dátumy mimo účtovného roka"
                : "Dates outside fiscal year";

            string message = slovak
                ? exceptionalRows.Length.ToString("N0") +
                  " riadkov účtovného denníka obsahuje dátum mimo účtovného roka " +
                  journalImport.FiscalYear +
                  ".\r\n\r\nRiadky boli importované, ale automaticky vylúčené z výpočtu. " +
                  "Skontrolujte ich v hárku Accounting Journal. Pôvodné dátumy zostali zachované; " +
                  "audítor ich môže neskôr potvrdiť alebo zadať opravený dátum.\r\n\r\n" +
                  "Rozsah problémových dátumov: " +
                  firstDate.ToString("yyyy-MM-dd") + " – " +
                  lastDate.ToString("yyyy-MM-dd") + "."
                : exceptionalRows.Length.ToString("N0") +
                  " accounting-journal row(s) contain posting dates outside fiscal year " +
                  journalImport.FiscalYear +
                  ".\r\n\r\nThe rows were imported but automatically excluded from calculation. " +
                  "Review them in the Accounting Journal worksheet. The original dates are preserved; " +
                  "the auditor can later accept the original date or enter a corrected date.\r\n\r\n" +
                  "Problematic date range: " +
                  firstDate.ToString("yyyy-MM-dd") + " – " +
                  lastDate.ToString("yyyy-MM-dd") + ".";

            MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
