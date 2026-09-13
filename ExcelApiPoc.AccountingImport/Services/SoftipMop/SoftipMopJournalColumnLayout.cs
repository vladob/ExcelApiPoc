using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    internal sealed class SoftipMopJournalColumnLayout
    {
        public int AccountingPeriod { get; private set; }
        public int DocumentType { get; private set; }
        public int DocumentNumber { get; private set; }
        public int Account { get; private set; }
        public int PostingDate { get; private set; }
        public int Description { get; private set; }
        public int DebitAmount { get; private set; }
        public int CreditAmount { get; private set; }
        public int? CostCenter { get; private set; }

        public static bool TryDiscover(
            IReadOnlyList<string> headers,
            out SoftipMopJournalColumnLayout layout,
            out string error)
        {
            layout = null;
            error = null;

            var columns = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < headers.Count; index++)
            {
                string normalized = NormalizeHeader(headers[index]);
                if (normalized.Length == 0) continue;

                if (columns.ContainsKey(normalized))
                {
                    error = "duplicate column '" + headers[index] + "'.";
                    return false;
                }

                columns.Add(normalized, index);
            }

            string[] required =
            {
                "uctmes", "dd", "cislodokladu", "ucet", "datuct",
                "popiszapisu", "madat", "dal"
            };

            foreach (string name in required)
            {
                if (!columns.ContainsKey(name))
                {
                    error = "required column '" + DisplayName(name) + "' is missing.";
                    return false;
                }
            }

            layout = new SoftipMopJournalColumnLayout
            {
                AccountingPeriod = columns["uctmes"],
                DocumentType = columns["dd"],
                DocumentNumber = columns["cislodokladu"],
                Account = columns["ucet"],
                PostingDate = columns["datuct"],
                Description = columns["popiszapisu"],
                DebitAmount = columns["madat"],
                CreditAmount = columns["dal"],
                CostCenter = columns.TryGetValue("stredisko", out int costCenter)
                    ? (int?)costCenter
                    : null
            };
            return true;
        }

        private static string NormalizeHeader(string value)
        {
            string source = (value ?? string.Empty).Trim().ToLowerInvariant();
            string decomposed = source.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(decomposed.Length);

            foreach (char character in decomposed)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(character)) result.Append(character);
            }

            return result.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string DisplayName(string normalized)
        {
            switch (normalized)
            {
                case "uctmes": return "Účt.mes.";
                case "cislodokladu": return "Číslo dokladu";
                case "ucet": return "Účet";
                case "datuct": return "Dát.účt.";
                case "popiszapisu": return "Popis zápisu";
                case "madat": return "Má dať";
                case "dal": return "DAL";
                default: return normalized;
            }
        }
    }
}
