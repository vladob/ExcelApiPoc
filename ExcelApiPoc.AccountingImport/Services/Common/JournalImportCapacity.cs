using System;
using System.Globalization;
using System.IO;

namespace ExcelApiPoc.AccountingImport.Services.Common
{
    public static class JournalImportCapacity
    {
        public const int ExcelWorksheetRows = 1_048_576;
        public const int JournalHeaderRow = 4;
        public const int MaximumJournalRows = ExcelWorksheetRows - JournalHeaderRow;

        public static void EnsureCanAppend(
            int existingRows,
            int additionalRows,
            string sourceDescription)
        {
            if (existingRows < 0)
                throw new ArgumentOutOfRangeException(nameof(existingRows));
            if (additionalRows < 0)
                throw new ArgumentOutOfRangeException(nameof(additionalRows));

            long requestedRows = (long)existingRows + additionalRows;
            if (requestedRows <= MaximumJournalRows) return;

            throw new InvalidDataException(
                "The canonical accounting journal would contain " +
                requestedRows.ToString("N0", CultureInfo.InvariantCulture) +
                " rows after adding " +
                (string.IsNullOrWhiteSpace(sourceDescription)
                    ? "the selected source"
                    : sourceDescription) +
                ". The audit-workbook journal worksheet supports at most " +
                MaximumJournalRows.ToString("N0", CultureInfo.InvariantCulture) +
                " data rows.");
        }
    }
}
