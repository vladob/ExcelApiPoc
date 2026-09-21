using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesXmlAccountingFrameworkParser : IIvesAccountingFrameworkSourceParser
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
                       StringComparison.OrdinalIgnoreCase) &&
                   Path.GetFileName(filePath).StartsWith(
                       "UCT_ROZVRH_",
                       StringComparison.OrdinalIgnoreCase);
        }

        public IvesAccountingFrameworkParseResult Parse(string filePath)
        {
            ValidateSourceFile(filePath);

            string fullPath = Path.GetFullPath(filePath);
            string fileName = Path.GetFileName(filePath);

            AccountingFileNameMetadata metadata;
            if (!AccountingFileNameMetadataParser.TryParse(
                    fileName,
                    out metadata) ||
                metadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingFramework)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' does not identify an IVES accounting framework with IČO and fiscal year.");
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
                    "The IVES accounting-framework XML file is not valid XML.",
                    exception);
            }

            XElement root = document.Root;
            if (root == null ||
                root.Name != CrystalNamespace + "CrystalReport")
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework XML file is not a Crystal Reports detail export.");
            }

            var result = new IvesAccountingFrameworkParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear
            };

            int sourceRowNumber = 0;
            int sequenceNumber = 0;

            foreach (XElement details in
                     root.Descendants(CrystalNamespace + "Details"))
            {
                sourceRowNumber++;

                Dictionary<string, string> fields =
                    ReadFieldsByFieldName(details);

                if (!fields.ContainsKey("{@au}") &&
                    !fields.ContainsKey("{uctRozvrh.Nazov_uctu}"))
                {
                    continue;
                }

                string sourceAccountCode = Field(fields, "{@au}");

                if (string.IsNullOrWhiteSpace(sourceAccountCode))
                {
                    throw new InvalidDataException(
                        "IVES accounting-framework XML detail " +
                        sourceRowNumber +
                        " contains no account code.");
                }

                result.Rows.Add(new IvesAccountingFrameworkSourceRow
                {
                    SequenceNumber = ++sequenceNumber,
                    SourceRowNumber = sourceRowNumber,
                    SourceAccountCode = sourceAccountCode,
                    AccountCode =
                        AccountCodeNormalizer.Normalize(sourceAccountCode),
                    AccountName =
                        Field(fields, "{uctRozvrh.Nazov_uctu}"),
                    ActivityCode =
                        Field(fields, "{uctRozvrh.Druh_cinnosti}"),
                    Type =
                        Field(fields, "{uctRozvrh.Typ}"),
                    PsFlag =
                        Field(fields, "{@PS}"),
                    BuFlag =
                        Field(fields, "{@BU}"),
                    RuFlag =
                        Field(fields, "{@ru}"),
                    PlFlag =
                        Field(fields, "{@PL}"),
                    Currency =
                        Field(fields, "{uctRozvrh.Kod_meny}"),
                    ValidFrom = ParseOptionalDate(
                        Field(fields, "{uctRozvrh.Platnost_od}"),
                        sourceRowNumber,
                        "valid-from date"),
                    ValidTo = ParseOptionalDate(
                        Field(fields, "{uctRozvrh.platnost_do}"),
                        sourceRowNumber,
                        "valid-to date")
                });
            }

            result.SourceRowCount = sourceRowNumber;

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework XML file contains no account details.");
            }

            return result;
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
            XElement value =
                field.Element(CrystalNamespace + "Value");

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

        private static DateTime? ParseOptionalDate(
            string value,
            int sourceRowNumber,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out DateTime parsed))
            {
                return parsed.Date;
            }

            throw new InvalidDataException(
                "IVES accounting-framework XML detail " +
                sourceRowNumber +
                " contains invalid " + fieldName + " '" +
                value + "'.");
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
                    "The IVES accounting-framework XML file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".xml",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES XML accounting-framework parser accepts only .xml files.");
            }
        }
    }
}
