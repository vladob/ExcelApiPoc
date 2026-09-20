using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.Common
{
    public enum AccountingSourceDocumentKind
    {
        AccountingJournal,
        AccountingFramework,
        GeneralLedger
    }

    public sealed class AccountingFileNameMetadata
    {
        public string FileName { get; internal set; }
        public AccountingSourceDocumentKind DocumentKind { get; internal set; }
        public string Ico { get; internal set; }
        public int FiscalYear { get; internal set; }
        public int? ExportStage { get; internal set; }
    }

    public static class AccountingFileNameMetadataParser
    {
        private static readonly Regex FileNamePattern =
            new Regex(
                @"^(?<document>U_DENNIK|UCT_ROZVRH|HL_KNIHA)_" +
                @"(?<ico>\d{8})_" +
                @"(?<year>\d{4})" +
                @"(?<stage>\d{2})?$",
                RegexOptions.Compiled |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);

        public static bool TryParse(
            string filePathOrName,
            out AccountingFileNameMetadata metadata)
        {
            metadata = null;

            if (string.IsNullOrWhiteSpace(filePathOrName))
                return false;

            string fileName = Path.GetFileName(filePathOrName);
            string stem = Path.GetFileNameWithoutExtension(fileName);
            Match match = FileNamePattern.Match(stem);

            if (!match.Success)
                return false;

            int fiscalYear = int.Parse(
                match.Groups["year"].Value,
                CultureInfo.InvariantCulture);

            if (fiscalYear < 1900 || fiscalYear > 9999)
                return false;

            int? exportStage = match.Groups["stage"].Success
                ? int.Parse(
                    match.Groups["stage"].Value,
                    CultureInfo.InvariantCulture)
                : (int?)null;

            metadata = new AccountingFileNameMetadata
            {
                FileName = fileName,
                DocumentKind = ParseDocumentKind(
                    match.Groups["document"].Value),
                Ico = match.Groups["ico"].Value,
                FiscalYear = fiscalYear,
                ExportStage = exportStage
            };

            return true;
        }

        public static string ResolveIco(
            string fileName,
            string contentIco,
            AccountingFileNameMetadata fileNameMetadata)
        {
            string normalizedContent =
                string.IsNullOrWhiteSpace(contentIco)
                    ? null
                    : contentIco.Trim();

            string fileNameIco = fileNameMetadata?.Ico;

            if (!string.IsNullOrWhiteSpace(normalizedContent) &&
                !string.IsNullOrWhiteSpace(fileNameIco) &&
                !string.Equals(
                    normalizedContent,
                    fileNameIco,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' identifies IČO '" + fileNameIco +
                    "', but file contents identify IČO '" +
                    normalizedContent + "'.");
            }

            return normalizedContent ?? fileNameIco;
        }

        public static int ResolveFiscalYear(
            string fileName,
            int contentFiscalYear,
            AccountingFileNameMetadata fileNameMetadata)
        {
            int? contentYear = contentFiscalYear > 0
                ? contentFiscalYear
                : (int?)null;
            int? fileNameYear = fileNameMetadata?.FiscalYear;

            if (contentYear.HasValue &&
                fileNameYear.HasValue &&
                contentYear.Value != fileNameYear.Value)
            {
                throw new InvalidDataException(
                    "Filename '" + fileName +
                    "' identifies fiscal year " + fileNameYear.Value +
                    ", but file contents identify fiscal year " +
                    contentYear.Value + ".");
            }

            return contentYear ?? fileNameYear ?? 0;
        }

        private static AccountingSourceDocumentKind ParseDocumentKind(
            string value)
        {
            if (string.Equals(
                    value,
                    "U_DENNIK",
                    StringComparison.OrdinalIgnoreCase))
            {
                return AccountingSourceDocumentKind.AccountingJournal;
            }

            if (string.Equals(
                    value,
                    "UCT_ROZVRH",
                    StringComparison.OrdinalIgnoreCase))
            {
                return AccountingSourceDocumentKind.AccountingFramework;
            }

            return AccountingSourceDocumentKind.GeneralLedger;
        }
    }
}
