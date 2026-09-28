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
        private static readonly Regex Account = new Regex(@"^\d{3}[\p{L}\d.]*$");
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
                var root = Read(filePath).Root;
                var name = Name.Match(Path.GetFileName(filePath));
                if (root == null || root.Name != "uctovny_vykaz" || Value(root, "typDoc") != "UCT_VETA" ||
                    root.Element("vety") == null || root.Element("sucetKontrola") == null ||
                    Value(root.Element("identifikacia")?.Element("identifikator"), "ico") != name.Groups["ico"].Value ||
                    Value(root.Element("obdobie"), "rok") != name.Groups["year"].Value) return false;
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
                throw new InvalidDataException("Expected an IfoSoft UCT_VETA journal XML with matching IČO and year.");
            XElement root = Read(filePath).Root;
            var records = root.Element("vety").Elements("veta").ToArray();
            if (records.Length == 0 || records.Length % 2 != 0 || records.Length > 1_000_000 ||
                root.Element("vety").Elements().Count() != records.Length)
                throw new InvalidDataException("IfoSoft XML must contain complete paired journal entries.");
            var result = new JournalImport
            {
                SourceFileName = Path.GetFileName(filePath), SourceFilePath = Path.GetFullPath(filePath),
                SourceFileHash = Hash(filePath), TechnicalType = "XML", AccountingFormat = "IfoSoft",
                Ico = detection.Ico, CompanyName = detection.CompanyName, FiscalYear = detection.FiscalYear.Value,
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = new ImportReport { AccountingFormat = "IfoSoft", ImportType = "AccountingJournal",
                    SourceFileName = Path.GetFileName(filePath) }
            };
            for (int i = 0; i < records.Length; i += 2)
            {
                XElement debit = records[i], credit = records[i + 1];
                string location = "XML records " + (i + 1) + " and " + (i + 2);
                decimal md = Amount(Value(debit, "md"), location), dal = Amount(Value(credit, "dal"), location);
                if (Value(debit, "dal").Length != 0 || Value(credit, "md").Length != 0 ||
                    Value(debit, "md").Length == 0 || Value(credit, "dal").Length == 0 || md != dal ||
                    Value(debit, "ucDok") != Value(credit, "ucDok") ||
                    Value(debit, "ucPripDat") != Value(credit, "ucPripDat") ||
                    Value(debit, "rok") != Value(credit, "rok") || Value(debit, "mes") != Value(credit, "mes"))
                    throw new InvalidDataException(location + ": debit and credit records do not form one entry.");
                string debitAccount = Value(debit, "ucSuv") + Value(debit, "ucAnl");
                string creditAccount = Value(credit, "ucSuv") + Value(credit, "ucAnl");
                if (!Account.IsMatch(debitAccount) || !Account.IsMatch(creditAccount))
                    throw new InvalidDataException(location + ": invalid account code.");
                if (!DateTime.TryParseExact(Value(debit, "ucPripDat"), "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime date) || date.Year != result.FiscalYear ||
                    Value(debit, "rok") != result.FiscalYear.ToString(CultureInfo.InvariantCulture) ||
                    Value(debit, "mes") != date.Month.ToString("00", CultureInfo.InvariantCulture))
                    throw new InvalidDataException(location + ": accounting date differs from the XML fiscal period.");
                XElement debitBudget = debit.Element("rozpocCis"), creditBudget = credit.Element("rozpocCis");
                var row = new JournalRow
                {
                    SequenceNumber = i / 2 + 1, SourceRecordNumber = i + 1, SourceLocation = location,
                    PostingDate = date, DocumentNumber = Value(debit, "ucDok"),
                    Description = Value(debit, "ucDokText"),
                    DebitAccount = debitAccount, CreditAccount = creditAccount,
                    DebitAmount = md, CreditAmount = dal,
                    DebitSection = Value(debitBudget, "funkcKlasif"), DebitItem = Value(debitBudget, "ekonKlasif"),
                    DebitFundingSource = Value(debitBudget, "akciaCis"),
                    CreditSection = Value(creditBudget, "funkcKlasif"), CreditItem = Value(creditBudget, "ekonKlasif"),
                    CreditFundingSource = Value(creditBudget, "akciaCis"),
                    RecordKind = date.Month == 1 && date.Day == 1 &&
                        (debitAccount == "701" || creditAccount == "701") ? JournalRecordKind.Opening :
                        date.Month == 12 && date.Day == 31 &&
                        (debitAccount == "702" || creditAccount == "702" ||
                         debitAccount == "710" || creditAccount == "710") ? JournalRecordKind.Closing : JournalRecordKind.Normal
                };
                result.Rows.Add(row);
            }
            // IfoSoft's sucetKontrola includes the sum of debit amounts and the count of source records.
            decimal control = Amount(Value(root, "sucetKontrola"), "XML control total");
            decimal total = result.Rows.Sum(row => row.DebitAmount.Value);
            if (total != result.Rows.Sum(row => row.CreditAmount.Value) || control != total + records.Length)
                throw new InvalidDataException("IfoSoft XML control total does not agree with the journal entries.");
            result.ImportReport.RecordCounts["JournalRows"] = result.Rows.Count;
            return result;
        }

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

        private static XDocument Read(string path)
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using (var reader = XmlReader.Create(path, settings)) return XDocument.Load(reader);
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
