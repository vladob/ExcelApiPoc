using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ExcelApiPoc.AccountingImport.Services.IfoSoft
{
    /// <summary>Reads the paired accounting entries in an IfoSoft UCT_VETA XML export.</summary>
    public sealed class IfoSoftXmlJournalImporter : IJournalImporter
    {
        static IfoSoftXmlJournalImporter()
        {
            // IfoSoft declares windows-1250 in its XML header. .NET 8 needs the
            // code-page provider registered before XmlReader opens the file.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static readonly Regex Name = new Regex(@"^U_DENNIK_(?<ico>\d{8})_(?<year>\d{4})\.xml$", RegexOptions.IgnoreCase);
        private static readonly Regex Account = new Regex(@"^\d{3}[\p{L}\d.]*(?:-[\p{L}\d.]+)*$");
        private static readonly CultureInfo Sk = CultureInfo.GetCultureInfo("sk-SK");

        public bool CanImport(string filePath, string accountingFormat) =>
            string.Equals(accountingFormat, "IfoSoft", StringComparison.OrdinalIgnoreCase) &&
            TryDetect(filePath, out JournalDetectionResult _);

        public static bool TryDetect(string filePath, out JournalDetectionResult detection)
        {
            detection = null;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) ||
                !Name.IsMatch(Path.GetFileName(filePath))) return false;
            try
            {
                var root = Read(filePath, out _).Root;
                var name = Name.Match(Path.GetFileName(filePath));
                if (root == null || root.Name != "uctovny_vykaz" || Value(root, "typDoc") != "UCT_VETA" ||
                    root.Element("vety") == null || root.Element("sucetKontrola") == null ||
                    Value(root.Element("identifikacia")?.Element("identifikator"), "ico") != name.Groups["ico"].Value) return false;
                detection = new JournalDetectionResult
                {
                    TechnicalType = "XML", AccountingFormat = "IfoSoft", Ico = name.Groups["ico"].Value,
                    FiscalYear = int.Parse(name.Groups["year"].Value, CultureInfo.InvariantCulture),
                    CompanyName = Value(root.Element("identifikacia")?.Element("identifikator"), "nazov")
                };
                return true;
            }
            catch (Exception ex) when (ex is XmlException || ex is IOException || ex is InvalidDataException || ex is ArgumentException)
            {
                return false;
            }
        }

        public JournalImport Import(string filePath)
        {
            if (!TryDetect(filePath, out JournalDetectionResult detection))
                throw new InvalidDataException("Expected an IfoSoft UCT_VETA journal XML with matching IČO.");
            XElement root = Read(filePath, out int repairedTextCharacters).Root;
            var records = root.Element("vety").Elements("veta").ToArray();
            if (records.Length == 0 || records.Length > 1_000_000 ||
                root.Element("vety").Elements().Count() != records.Length)
                throw new InvalidDataException("IfoSoft XML contains no entries, too many entries, or unexpected elements.");
            var result = new JournalImport
            {
                SourceFileName = Path.GetFileName(filePath), SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = Hash(filePath), TechnicalType = "XML", AccountingFormat = "IfoSoft",
                Ico = detection.Ico, CompanyName = detection.CompanyName, FiscalYear = detection.FiscalYear.Value,
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = new ImportReport { AccountingFormat = "IfoSoft", ImportType = "AccountingJournal",
                    SourceFileName = Path.GetFileName(filePath) }
            };
            if (repairedTextCharacters > 0)
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_XML_DESCRIPTION_NORMALIZED",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = repairedTextCharacters + " invalid XML characters in journal descriptions were normalized; " +
                        "check the affected descriptions in the source export."
                });
            string exportYear = Value(root.Element("obdobie"), "rok");
            if (exportYear != result.FiscalYear.ToString(CultureInfo.InvariantCulture))
            {
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_XML_EXPORT_PERIOD_DIFFERS",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = "XML export period year " + exportYear + " differs from the filename's fiscal year " +
                        result.FiscalYear + "; every entry date and year will be validated."
                });
            }
            int nextYearEntries = 0;
            for (int i = 0; i < records.Length; i++)
            {
                JournalRow row = ReadRecord(records[i], i + 1, result.FiscalYear);
                if (row.DebitAmount.HasValue && i + 1 < records.Length)
                {
                    // Preserve the compact entry representation when two adjacent
                    // source records clearly identify the same posting.
                    XElement next = records[i + 1];
                    if (Value(next, "dal").Length > 0 && Value(next, "md").Length == 0 &&
                        Value(records[i], "ucDok") == Value(next, "ucDok") &&
                        Value(records[i], "ucPripDat") == Value(next, "ucPripDat") &&
                        Value(records[i], "rok") == Value(next, "rok") &&
                        Value(records[i], "mes") == Value(next, "mes"))
                    {
                        JournalRow credit = ReadRecord(next, i + 2, result.FiscalYear);
                        if (row.DebitAmount == credit.CreditAmount)
                        {
                            row.CreditAccount = credit.CreditAccount;
                            row.CreditAmount = credit.CreditAmount;
                            row.CreditSection = credit.CreditSection;
                            row.CreditItem = credit.CreditItem;
                            row.CreditFundingSource = credit.CreditFundingSource;
                            row.TextNormalizationApplied |= credit.TextNormalizationApplied;
                            row.SourceLocation = "XML records " + (i + 1) + " and " + (i + 2);
                            i++;
                        }
                    }
                }
                row.SequenceNumber = result.Rows.Count + 1;
                if (row.PostingDate.Year != result.FiscalYear) nextYearEntries++;
                row.RecordKind = Classify(row);
                if (row.TextNormalizationApplied) result.NormalizedTextFieldCount++;
                result.Rows.Add(row);
            }
            // IfoSoft's sucetKontrola includes the sum of debit amounts and the count of source records.
            decimal control = Amount(Value(root, "sucetKontrola"), "XML control total");
            decimal total = result.Rows.Sum(row => row.DebitAmount ?? 0m);
            if (total != result.Rows.Sum(row => row.CreditAmount ?? 0m) || control != total + records.Length)
                throw new InvalidDataException("IfoSoft XML control total does not agree with the journal entries.");
            if (nextYearEntries > 0)
                result.ImportReport.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "IFOSOFT_XML_NEXT_YEAR_POSTINGS",
                    Severity = ImportDiagnosticSeverity.Warning,
                    Message = nextYearEntries + " entries in the " + result.FiscalYear +
                        " journal have January " + (result.FiscalYear + 1) + " posting dates; their source dates were preserved."
                });
            result.ImportReport.RecordCounts["JournalRows"] = result.Rows.Count;
            return result;
        }

        private static JournalRow ReadRecord(XElement record, int sourceNumber, int year)
        {
            string location = "XML record " + sourceNumber;
            string debitText = Value(record, "md"), creditText = Value(record, "dal");
            if ((debitText.Length == 0) == (creditText.Length == 0))
                throw new InvalidDataException(location + ": exactly one amount side is required.");
            string rawAccount = Value(record, "ucSuv") + Value(record, "ucAnl");
            string account = Regex.Replace(rawAccount, @"\s+", string.Empty);
            if (!Account.IsMatch(account)) throw new InvalidDataException(location + ": invalid account code '" + account + "'.");
            if (!DateTime.TryParseExact(Value(record, "ucPripDat"), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date) ||
                (date.Year != year && !(date.Year == year + 1 && date.Month == 1)) ||
                Value(record, "rok") != date.Year.ToString(CultureInfo.InvariantCulture) ||
                Value(record, "mes") != date.Month.ToString("00", CultureInfo.InvariantCulture))
                throw new InvalidDataException(location + ": accounting date differs from the filename's fiscal year.");
            XElement budget = record.Element("rozpocCis");
            var row = new JournalRow
            {
                SourceRecordNumber = sourceNumber, SourceLocation = location,
                PostingDate = date, DocumentNumber = Value(record, "ucDok"), Description = Value(record, "ucDokText"),
                TextNormalizationApplied = account != rawAccount
            };
            if (debitText.Length > 0)
            {
                row.DebitAccount = account; row.DebitAmount = Amount(debitText, location);
                row.DebitSection = Value(budget, "funkcKlasif"); row.DebitItem = Value(budget, "ekonKlasif");
                row.DebitFundingSource = Value(budget, "akciaCis");
            }
            else
            {
                row.CreditAccount = account; row.CreditAmount = Amount(creditText, location);
                row.CreditSection = Value(budget, "funkcKlasif"); row.CreditItem = Value(budget, "ekonKlasif");
                row.CreditFundingSource = Value(budget, "akciaCis");
            }
            return row;
        }

        private static JournalRecordKind Classify(JournalRow row) =>
            row.PostingDate.Month == 1 && row.PostingDate.Day == 1 &&
                (row.DebitAccount == "701" || row.CreditAccount == "701") ? JournalRecordKind.Opening :
            row.PostingDate.Month == 12 && row.PostingDate.Day == 31 &&
                (row.DebitAccount == "702" || row.CreditAccount == "702" ||
                 row.DebitAccount == "710" || row.CreditAccount == "710") ? JournalRecordKind.Closing : JournalRecordKind.Normal;

        private static string Value(XElement parent, string child) =>
            ((string)parent?.Element(child) ?? string.Empty).Trim();

        private static decimal Amount(string text, string location)
        {
            if (text.Length == 0) return 0m;
            if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    Sk, out decimal amount))
                throw new InvalidDataException(location + ": invalid monetary amount '" + text + "'.");
            return amount;
        }

        private static XDocument Read(string path, out int repairedTextCharacters)
        {
            repairedTextCharacters = 0;
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            try
            {
                using (var reader = XmlReader.Create(path, settings)) return XDocument.Load(reader);
            }
            catch (XmlException)
            {
                // Repair malformed text content only; never alter structural
                // markup, amounts, accounts, or dates.
                string xml = File.ReadAllText(path, Encoding.GetEncoding(1250));
                int count = 0;
                string repaired = Regex.Replace(xml, @"<ucDokText>([^<]*)</ucDokText>", match =>
                {
                    string value = match.Groups[1].Value;
                    value = Regex.Replace(value, @"&(?!amp;|lt;|gt;|quot;|apos;|#\d+;|#x[0-9a-fA-F]+;)", _ =>
                    {
                        count++;
                        return "&amp;";
                    });
                    value = Regex.Replace(value, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", _ =>
                    {
                        count++;
                        return "\uFFFD";
                    });
                    return "<ucDokText>" + value + "</ucDokText>";
                });
                if (count == 0) throw;
                using (var reader = XmlReader.Create(new StringReader(repaired), settings))
                {
                    var document = XDocument.Load(reader);
                    repairedTextCharacters = count;
                    return document;
                }
            }
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
