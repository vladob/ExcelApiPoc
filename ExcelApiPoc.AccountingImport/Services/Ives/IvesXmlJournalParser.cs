using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXmlJournalParser : IIvesJournalSourceParser
    {
        private static readonly XNamespace CrystalNamespace =
            "urn:crystal-reports:schemas:report-detail";

        public string TechnicalType => "XML";

        public bool CanParse(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) &&
                   string.Equals(
                       Path.GetExtension(filePath),
                       ".xml",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesJournalParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            var result = new IvesJournalParseResult
            {
                SourceFileName = Path.GetFileName(filePath),
                SourceFilePath = fullPath
            };

            AccountingFileNameMetadata metadata = null;
            if (AccountingFileNameMetadataParser.TryParse(
                    result.SourceFileName,
                    out AccountingFileNameMetadata parsedMetadata))
            {
                if (parsedMetadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingJournal)
                {
                    throw new InvalidDataException(
                        "Filename '" + result.SourceFileName +
                        "' does not identify an accounting journal.");
                }

                metadata = parsedMetadata;
                result.Ico = metadata.Ico;
                result.FiscalYear = metadata.FiscalYear;
                result.PeriodStart = new DateTime(metadata.FiscalYear, 1, 1);
                result.PeriodEnd = new DateTime(metadata.FiscalYear, 12, 31);
            }

            XDocument document;
            try
            {
                document = XDocument.Load(
                    fullPath,
                    LoadOptions.None);
            }
            catch (Exception exception)
                when (exception is System.Xml.XmlException ||
                      exception is InvalidOperationException)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal XML file is not valid XML.",
                    exception);
            }

            XElement root = document.Root;
            if (root == null ||
                root.Name != CrystalNamespace + "CrystalReport")
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal XML file is not a Crystal Reports detail export.");
            }

            int sequence = 0;
            foreach (XElement details in
                     root.Descendants(CrystalNamespace + "Details"))
            {
                var fields = details
                    .Descendants(CrystalNamespace + "Field")
                    .Where(field => field.Attribute("FieldName") != null)
                    .GroupBy(
                        field => (string)field.Attribute("FieldName"),
                        StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => ReadValue(group.First()),
                        StringComparer.Ordinal);

                if (!fields.ContainsKey("{uctDennik.Datum_vytv_pohybu}") &&
                    !fields.ContainsKey("{uctDennik.Cislo_dokladu}") &&
                    !fields.ContainsKey("{uctDennik.Obnos}"))
                {
                    continue;
                }

                int recordNumber = ++sequence;
                var row = new IvesJournalSourceRow
                {
                    SequenceNumber = recordNumber,
                    SourceRowNumber = recordNumber,
                    Kind = IvesJournalRowKind.Transaction,
                    PostingDate = ParseDate(
                        Field(fields, "{uctDennik.Datum_vytv_pohybu}"),
                        recordNumber),
                    DocumentNumber =
                        Field(fields, "{uctDennik.Cislo_dokladu}"),
                    DebitCompositeAccount =
                        Field(fields, "{uctDennik.aU_Text_MD}"),
                    CreditCompositeAccount =
                        Field(fields, "{uctDennik.aU_Text_Dal}"),
                    Amount = ParseDecimal(
                        Field(fields, "{uctDennik.Obnos}"),
                        recordNumber),
                    Currency =
                        Field(fields, "{uctDennik.Symbol_meny}"),
                    Text =
                        Field(fields, "{uctDennik.text_operacie}"),
                    Module =
                        Field(fields, "{uctDennik.Priznak_modulu}"),
                    SourceLocation = result.SourceFileName +
                        ", XML detail " + recordNumber
                };

                result.TransactionRows.Add(row);
            }

            result.SourceRowCount = result.TransactionRows.Count;

            if (result.TransactionRows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal XML file contains no transaction details.");
            }

            if (metadata == null)
            {
                int fiscalYear = ResolveFiscalYearFromTransactions(result);
                result.FiscalYear = fiscalYear;
                result.PeriodStart = new DateTime(fiscalYear, 1, 1);
                result.PeriodEnd = new DateTime(fiscalYear, 12, 31);
            }

            decimal[] footerAmounts = root
                .Descendants(CrystalNamespace + "Field")
                .Where(field =>
                    string.Equals(
                        (string)field.Attribute("Name"),
                        "SumofObnos1",
                        StringComparison.Ordinal) &&
                    string.Equals(
                        (string)field.Attribute("FieldName"),
                        "Sum ({uctDennik.Obnos}, {uctDennik.Kod_meny})",
                        StringComparison.Ordinal))
                .Select(field => ParseFooterDecimal(ReadValue(field)))
                .ToArray();

            if (footerAmounts.Length > 0)
            {
                var total = new IvesJournalSourceRow
                {
                    SequenceNumber = 1,
                    SourceRowNumber = result.TransactionRows.Count + 1,
                    Kind = IvesJournalRowKind.ReportTotal,
                    SourceLocation = result.SourceFileName +
                        ", XML group footers (" +
                        footerAmounts.Length + ")"
                };
                total.ReportedAmounts.Add(footerAmounts.Sum());
                result.ReportTotalRows.Add(total);
            }

            return result;
        }

        private static string ReadValue(XElement field)
        {
            XElement value = field.Element(CrystalNamespace + "Value");
            if (value != null)
                return Normalize(value.Value);

            XElement formatted =
                field.Element(CrystalNamespace + "FormattedValue");
            return formatted == null ? null : Normalize(formatted.Value);
        }

        private static string Field(
            System.Collections.Generic.IDictionary<string, string> fields,
            string name)
        {
            return fields.TryGetValue(name, out string value)
                ? value
                : null;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private static DateTime ParseDate(string value, int recordNumber)
        {
            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTime result))
            {
                return result.Date;
            }

            throw new InvalidDataException(
                "IVES accounting-journal XML detail " +
                recordNumber +
                " contains invalid posting date '" +
                (value ?? string.Empty) + "'.");
        }

        private static decimal ParseDecimal(string value, int recordNumber)
        {
            if (decimal.TryParse(
                    value,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                return result;
            }

            throw new InvalidDataException(
                "IVES accounting-journal XML detail " +
                recordNumber +
                " contains invalid amount '" +
                (value ?? string.Empty) + "'.");
        }

        private static decimal ParseFooterDecimal(string value)
        {
            if (decimal.TryParse(
                    value,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal result))
            {
                return result;
            }

            throw new InvalidDataException(
                "The IVES accounting-journal XML report contains an invalid group total '" +
                (value ?? string.Empty) + "'.");
        }

        private static int ResolveFiscalYearFromTransactions(
            IvesJournalParseResult result)
        {
            int fiscalYear = result.TransactionRows[0].PostingDate.Value.Year;

            if (result.TransactionRows.Any(
                    row => row.PostingDate.Value.Year != fiscalYear))
            {
                throw new InvalidDataException(
                    "The IVES accounting-journal XML file contains transactions from more than one fiscal year and its filename does not identify the fiscal year.");
            }

            return fiscalYear;
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A source file is required.", nameof(filePath));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES accounting-journal XML file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".xml",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES XML journal parser accepts only .xml files.");
            }
        }
    }
}
