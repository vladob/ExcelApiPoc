using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    internal sealed class IvesPdfAccountingFrameworkParser
    {
        private const string LayoutResourceName =
            "ExcelApiPoc.AccountingImport.PdfLayouts.Ives.accounting-framework.v1.json";

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

            LayoutRecognitionResult recognition =
                new ITextLayoutRecognizer().Recognize(
                    fullPath,
                    LoadLayout());

            if (recognition.Diagnostics.Count > 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework PDF layout could not be recognized without diagnostics.");
            }

            var result = new IvesAccountingFrameworkParseResult
            {
                SourceFileName = fileName,
                SourceFilePath = fullPath,
                Ico = metadata.Ico,
                FiscalYear = metadata.FiscalYear,
                SourceRowCount =
                    recognition.RecordDiscovery.Records.Count
            };

            ParseRecords(
                recognition.RecordDiscovery.Records,
                result);

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting-framework PDF contains no recognizable account rows.");
            }

            return result;
        }

        private static void ParseRecords(
            IReadOnlyList<BaselineRecord> records,
            IvesAccountingFrameworkParseResult result)
        {
            int sequence = 0;

            for (int index = 0; index < records.Count; index++)
            {
                BaselineRecord record = records[index];

                if (!LooksLikeAccountRow(record))
                    continue;

                string sourceAccountCode =
                    ReadCompact(record, 40.0, 170.0);

                if (string.IsNullOrWhiteSpace(sourceAccountCode))
                    continue;

                string validFromText =
                    ReadCompact(record, 478.0, 520.0);
                string validToText =
                    ReadCompact(record, 520.0, 560.0);

                result.Rows.Add(new IvesAccountingFrameworkSourceRow
                {
                    SequenceNumber = ++sequence,
                    SourceRowNumber = index + 1,
                    SourceAccountCode = sourceAccountCode,
                    AccountCode =
                        AccountCodeNormalizer.Normalize(
                            sourceAccountCode),
                    AccountName =
                        ReadText(record, 178.0, 330.0),
                    ActivityCode =
                        ReadCompact(record, 338.0, 352.0),
                    Type =
                        ReadCompact(record, 356.0, 371.0),
                    PsFlag =
                        ReadCompact(record, 376.0, 389.0),
                    BuFlag =
                        ReadCompact(record, 393.0, 407.0),
                    PlFlag =
                        ReadCompact(record, 412.0, 425.0),
                    RuFlag =
                        ReadCompact(record, 430.0, 443.0),
                    Currency =
                        ReadCompact(record, 452.0, 474.0),
                    ValidFrom =
                        ParseOptionalDate(
                            validFromText,
                            index + 1,
                            "valid-from date"),
                    ValidTo =
                        ParseOptionalDate(
                            validToText,
                            index + 1,
                            "valid-to date")
                });
            }
        }

        private static bool LooksLikeAccountRow(
            BaselineRecord record)
        {
            string account =
                ReadCompact(record, 40.0, 170.0);
            string activity =
                ReadCompact(record, 338.0, 352.0);
            string type =
                ReadCompact(record, 356.0, 371.0);
            string currency =
                ReadCompact(record, 452.0, 474.0);
            string validFrom =
                ReadCompact(record, 478.0, 520.0);

            if (string.IsNullOrWhiteSpace(account) ||
                account.Length < 3 ||
                !char.IsDigit(account[0]) ||
                !char.IsDigit(account[1]) ||
                !char.IsDigit(account[2]))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(activity) ||
                string.IsNullOrWhiteSpace(type) ||
                string.IsNullOrWhiteSpace(currency))
            {
                return false;
            }

            if (!string.Equals(
                    currency,
                    "EUR",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return ParseDateOrNull(validFrom).HasValue;
        }

        private static DateTime? ParseOptionalDate(
            string value,
            int sourceRowNumber,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            DateTime? parsed = ParseDateOrNull(value);

            if (parsed.HasValue)
                return parsed.Value;

            throw new InvalidDataException(
                "IVES accounting-framework PDF source record " +
                sourceRowNumber +
                " contains invalid " + fieldName +
                " '" + value + "'.");
        }

        private static DateTime? ParseDateOrNull(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            DateTime parsed;
            if (DateTime.TryParseExact(
                    value.Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return parsed.Date;
            }

            return null;
        }

        private static string ReadCompact(
            BaselineRecord record,
            double left,
            double right)
        {
            string value = string.Concat(
                record.SourceTokens
                    .Where(token =>
                        token.Left >= left &&
                        token.Left < right)
                    .OrderBy(token => token.Left)
                    .ThenBy(token => token.Right)
                    .Select(token => Trim(token.Text))
                    .Where(text => text.Length > 0));

            return string.IsNullOrWhiteSpace(value)
                ? null
                : value
                    .Replace(" ", string.Empty)
                    .Replace("\u00A0", string.Empty);
        }

        private static string ReadText(
            BaselineRecord record,
            double left,
            double right)
        {
            var builder = new StringBuilder();

            foreach (PdfTextToken token in
                     record.SourceTokens
                         .Where(token =>
                             token.Left >= left &&
                             token.Left < right)
                         .OrderBy(token => token.Left)
                         .ThenBy(token => token.Right))
            {
                string text = token.Text ?? string.Empty;

                if (builder.Length > 0 &&
                    token.Left > left &&
                    !char.IsWhiteSpace(
                        builder[builder.Length - 1]) &&
                    text.Length > 0 &&
                    !char.IsWhiteSpace(text[0]))
                {
                    builder.Append(' ');
                }

                builder.Append(text);
            }

            string normalized =
                NormalizeWhitespace(builder.ToString());

            return normalized.Length == 0
                ? null
                : normalized;
        }

        private static string NormalizeWhitespace(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            bool pendingSpace = false;

            foreach (char character in value.Trim())
            {
                if (char.IsWhiteSpace(character) ||
                    character == '\u00A0')
                {
                    pendingSpace =
                        builder.Length > 0;
                    continue;
                }

                if (pendingSpace)
                {
                    builder.Append(' ');
                    pendingSpace = false;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }

        private static string Trim(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        private static LayoutDefinition LoadLayout()
        {
            Assembly assembly =
                typeof(IvesPdfAccountingFrameworkParser)
                    .Assembly;

            using (Stream stream =
                   assembly.GetManifestResourceStream(
                       LayoutResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(
                        "Embedded IVES PDF layout '" +
                        LayoutResourceName +
                        "' was not found.");
                }

                LayoutLoadResult result =
                    new LayoutDefinitionLoader()
                        .Load(stream);

                if (!result.IsValid)
                {
                    throw new InvalidDataException(
                        "Embedded IVES accounting-framework PDF layout is invalid: " +
                        string.Join(
                            "; ",
                            result.Messages.Select(
                                message =>
                                    message.Message)));
                }

                return result.Definition;
            }
        }

        private static void ValidateSourceFile(
            string filePath)
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
                    "The IVES accounting-framework PDF file was not found.",
                    filePath);
            }

            if (!string.Equals(
                    Path.GetExtension(filePath),
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "The IVES PDF accounting-framework parser accepts only .pdf files.");
            }
        }
    }
}
