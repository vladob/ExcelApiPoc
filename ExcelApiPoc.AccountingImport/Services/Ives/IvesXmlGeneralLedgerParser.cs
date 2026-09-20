using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXmlGeneralLedgerParser
    {
        private static readonly XNamespace CrystalNamespace =
            "urn:crystal-reports:schemas:report-detail";

        public IvesGeneralLedgerParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            AccountingFileNameMetadata metadata;
            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out metadata) ||
                metadata.DocumentKind != AccountingSourceDocumentKind.GeneralLedger)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' does not identify an IVES general ledger with IČO and fiscal year.");
            }

            XDocument document;
            try
            {
                document = XDocument.Load(fullPath, LoadOptions.None);
            }
            catch (Exception exception)
                when (exception is System.Xml.XmlException ||
                      exception is InvalidOperationException)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger XML file is not valid XML.",
                    exception);
            }

            XElement root = document.Root;
            if (root == null ||
                root.Name != CrystalNamespace + "CrystalReport")
            {
                throw new InvalidDataException(
                    "The IVES general-ledger XML file is not a Crystal Reports detail export.");
            }

            var result = new IvesGeneralLedgerParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                PeriodStart = new DateTime(metadata.FiscalYear, 1, 1),
                PeriodEnd = new DateTime(metadata.FiscalYear, 12, 31)
            };

            int sourceRecordNumber = 0;
            int accountSequence = 0;
            int documentSequence = 0;
            int syntheticSequence = 0;
            int totalSequence = 0;

            foreach (XElement activityGroup in FindActivityGroups(root))
            {
                var activity = new IvesGeneralLedgerActivity
                {
                    Name = ReadNamedField(activityGroup, "cinnost1"),
                    Currency = ReadNamedField(activityGroup, "Kodmeny1")
                };

                if (string.IsNullOrWhiteSpace(activity.Name))
                {
                    throw new InvalidDataException(
                        "An IVES general-ledger XML activity has no recognizable name.");
                }

                result.Activities.Add(activity);

                foreach (XElement details in
                         activityGroup.Descendants(CrystalNamespace + "Details"))
                {
                    Dictionary<string, string> fields =
                        ReadFieldsByFieldName(details);

                    if (!fields.ContainsKey("{uctHlavKniha.Datum_vytv_pohybu}") &&
                        !fields.ContainsKey("{uctHlavKniha.Cislo_dokladu}") &&
                        !fields.ContainsKey("{uctHlavKniha.aU_Text}"))
                    {
                        continue;
                    }

                    int source = ++sourceRecordNumber;
                    activity.DocumentRows.Add(new IvesGeneralLedgerSourceRow
                    {
                        SequenceNumber = ++documentSequence,
                        SourceRowNumber = source,
                        Kind = IvesGeneralLedgerRowKind.Document,
                        DocumentDate = ParseDate(
                            Field(fields, "{uctHlavKniha.Datum_vytv_pohybu}"),
                            source),
                        DocumentNumber =
                            Field(fields, "{uctHlavKniha.Cislo_dokladu}"),
                        AccountCode =
                            Field(fields, "{uctHlavKniha.aU_Text}"),
                        Text =
                            Field(fields, "{@NazovUc}"),
                        DebitTurnover = ParseDecimal(
                            Field(fields, "{@MD}"),
                            source,
                            "debit turnover"),
                        CreditTurnover = ParseDecimal(
                            Field(fields, "{@DAL}"),
                            source,
                            "credit turnover")
                    });
                }

                foreach (XElement footer in
                         activityGroup.Descendants(
                             CrystalNamespace + "GroupFooter"))
                {
                    Dictionary<string, XElement> named =
                        ReadFieldsByName(footer);

                    if (IsAnalyticalSummary(named))
                    {
                        int source = ++sourceRecordNumber;
                        activity.AccountRows.Add(
                            new IvesGeneralLedgerSourceRow
                            {
                                SequenceNumber = ++accountSequence,
                                SourceRowNumber = source,
                                Kind = IvesGeneralLedgerRowKind.Account,
                                AccountCode = ReadValue(named["aUText6"]),
                                Text = ReadValue(named["Field58"]),
                                OpeningBalance = ParseDecimal(
                                    ReadValue(named["Field164"]),
                                    source,
                                    "opening balance"),
                                DebitTurnover = ParseDecimal(
                                    ReadValue(named["Field56"]),
                                    source,
                                    "debit turnover"),
                                CreditTurnover = ParseDecimal(
                                    ReadValue(named["Field57"]),
                                    source,
                                    "credit turnover"),
                                ClosingBalance = ParseDecimal(
                                    ReadValue(named["Field55"]),
                                    source,
                                    "closing balance")
                            });
                        continue;
                    }

                    if (IsSyntheticSummary(named))
                    {
                        int source = ++sourceRecordNumber;
                        activity.SyntheticSummaryRows.Add(
                            new IvesGeneralLedgerSourceRow
                            {
                                SequenceNumber = ++syntheticSequence,
                                SourceRowNumber = source,
                                Kind = IvesGeneralLedgerRowKind.SyntheticSubtotal,
                                AccountCode = ExtractSyntheticCode(
                                    ReadValue(named["Field60"])),
                                OpeningBalance = ParseDecimal(
                                    ReadValue(named["Field47"]),
                                    source,
                                    "opening balance"),
                                DebitTurnover = ParseDecimal(
                                    ReadValue(named["Field7"]),
                                    source,
                                    "debit turnover"),
                                CreditTurnover = ParseDecimal(
                                    ReadValue(named["Field8"]),
                                    source,
                                    "credit turnover"),
                                ClosingBalance = ParseDecimal(
                                    ReadValue(named["Field59"]),
                                    source,
                                    "closing balance")
                            });
                        continue;
                    }

                    if (IsReportTotal(named))
                    {
                        int source = ++sourceRecordNumber;
                        activity.ReportTotalRows.Add(
                            new IvesGeneralLedgerSourceRow
                            {
                                SequenceNumber = ++totalSequence,
                                SourceRowNumber = source,
                                Kind = IvesGeneralLedgerRowKind.ReportTotal,
                                OpeningBalance = ParseDecimal(
                                    ReadValue(named["pocstavCelk1"]),
                                    source,
                                    "opening balance"),
                                DebitTurnover = ParseDecimal(
                                    ReadValue(named["Field156"]),
                                    source,
                                    "debit turnover"),
                                CreditTurnover = ParseDecimal(
                                    ReadValue(named["Field157"]),
                                    source,
                                    "credit turnover"),
                                ClosingBalance = ParseDecimal(
                                    ReadValue(named["Field158"]),
                                    source,
                                    "closing balance")
                            });
                    }
                }
            }

            result.SourceRowCount = sourceRecordNumber;

            if (result.Activities.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES general-ledger XML file contains no recognizable activities.");
            }

            if (result.Activities.All(activity =>
                    activity.AccountRows.Count == 0 &&
                    activity.DocumentRows.Count == 0))
            {
                throw new InvalidDataException(
                    "The IVES general-ledger XML file contains no ledger records.");
            }

            return result;
        }

        private static IEnumerable<XElement> FindActivityGroups(XElement root)
        {
            return root
                .Descendants(CrystalNamespace + "Group")
                .Where(group =>
                    string.Equals(
                        (string)group.Attribute("Level"),
                        "2",
                        StringComparison.Ordinal) &&
                    group
                        .Element(CrystalNamespace + "GroupHeader")
                        ?.Descendants(CrystalNamespace + "Field")
                        .Any(field =>
                            string.Equals(
                                (string)field.Attribute("Name"),
                                "cinnost1",
                                StringComparison.Ordinal)) == true);
        }

        private static string ReadNamedField(
            XElement group,
            string name)
        {
            XElement header = group.Element(
                CrystalNamespace + "GroupHeader");

            XElement field = header?
                .Descendants(CrystalNamespace + "Field")
                .FirstOrDefault(item =>
                    string.Equals(
                        (string)item.Attribute("Name"),
                        name,
                        StringComparison.Ordinal));

            return field == null ? null : ReadValue(field);
        }

        private static Dictionary<string, string> ReadFieldsByFieldName(
            XElement source)
        {
            return source
                .Descendants(CrystalNamespace + "Field")
                .Where(field => field.Attribute("FieldName") != null)
                .GroupBy(
                    field => (string)field.Attribute("FieldName"),
                    StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => ReadValue(group.First()),
                    StringComparer.Ordinal);
        }

        private static Dictionary<string, XElement> ReadFieldsByName(
            XElement source)
        {
            return source
                .Descendants(CrystalNamespace + "Field")
                .Where(field => field.Attribute("Name") != null)
                .GroupBy(
                    field => (string)field.Attribute("Name"),
                    StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.Ordinal);
        }

        private static bool IsAnalyticalSummary(
            IDictionary<string, XElement> fields)
        {
            return HasAll(
                fields,
                "aUText6",
                "Field58",
                "Field55",
                "Field57",
                "Field56",
                "Field164");
        }

        private static bool IsSyntheticSummary(
            IDictionary<string, XElement> fields)
        {
            return HasAll(
                fields,
                "Field7",
                "Field8",
                "Field4",
                "Field59",
                "Field60",
                "Field47") &&
                string.Equals(
                    ReadValue(fields["Field4"]),
                    "SU",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReportTotal(
            IDictionary<string, XElement> fields)
        {
            return HasAll(
                fields,
                "Field156",
                "Field157",
                "Field158",
                "pocstavCelk1");
        }

        private static bool HasAll(
            IDictionary<string, XElement> fields,
            params string[] names)
        {
            foreach (string name in names)
            {
                if (!fields.ContainsKey(name))
                    return false;
            }

            return true;
        }

        private static string Field(
            IDictionary<string, string> fields,
            string name)
        {
            return fields.TryGetValue(name, out string value)
                ? value
                : null;
        }

        private static string ReadValue(XElement field)
        {
            XElement value = field.Element(CrystalNamespace + "Value");
            if (value != null)
                return Normalize(value.Value);

            XElement formatted =
                field.Element(CrystalNamespace + "FormattedValue");

            return formatted == null
                ? null
                : Normalize(formatted.Value);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static DateTime ParseDate(
            string value,
            int sourceRecordNumber)
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
                "IVES general-ledger XML record " +
                sourceRecordNumber +
                " contains invalid posting date '" +
                (value ?? string.Empty) + "'.");
        }

        private static decimal ParseDecimal(
            string value,
            int sourceRecordNumber,
            string field)
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
                "IVES general-ledger XML record " +
                sourceRecordNumber +
                " contains invalid " + field + " '" +
                (value ?? string.Empty) + "'.");
        }

        private static string ExtractSyntheticCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            int separator = value.IndexOf('.');
            if (separator < 0)
                separator = value.IndexOf('=');
            if (separator < 0)
                separator = value.Length;

            return value.Substring(0, separator).Trim();
        }

        private static void ValidateSourceFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "A source file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The IVES general-ledger XML file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".xml",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES XML general-ledger parser accepts only .xml files.");
            }
        }
    }
}
