using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services
{
    internal enum UrbisDocumentKind
    {
        AccountingJournal,
        GeneralLedger
    }

    internal sealed class UrbisSourceFile
    {
        private static readonly Regex FileNamePattern =
            new Regex(
                @"^(?<document>U_DENNIK|HL_KNIHA)_" +
                @"(?<ico>\d{8})_" +
                @"(?<year>\d{4})" +
                @"(?<stage>\d{2})?" +
                @"(?<extension>\.xls|\.xlsx)$",
                RegexOptions.Compiled |
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);

        public string FilePath { get; private set; }

        public string FileName { get; private set; }

        public UrbisDocumentKind DocumentKind { get; private set; }

        public string Ico { get; private set; }

        public int FiscalYear { get; private set; }

        public int? ExportStage { get; private set; }

        public string Extension { get; private set; }

        public static UrbisSourceFile Parse(
            string filePath,
            UrbisDocumentKind expectedDocumentKind)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "An Urbis source file is required.",
                    nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "The Urbis source file was not found.",
                    filePath);
            }

            string fileName = Path.GetFileName(filePath);
            Match match = FileNamePattern.Match(fileName);

            if (!match.Success)
            {
                throw new InvalidDataException(
                    "The Urbis filename '" + fileName + "' is invalid. " +
                    "Expected 'U_DENNIK_<IČO>_<YYYY>[<stage>].xls[x]' or " +
                    "'HL_KNIHA_<IČO>_<YYYY>[<stage>].xls[x]'.");
            }

            UrbisDocumentKind detectedDocumentKind =
                ParseDocumentKind(match.Groups["document"].Value);

            if (detectedDocumentKind != expectedDocumentKind)
            {
                throw new InvalidDataException(
                    "The Urbis file '" + fileName + "' is a " +
                    Describe(detectedDocumentKind) +
                    ", but a " +
                    Describe(expectedDocumentKind) +
                    " was expected.");
            }

            int fiscalYear = int.Parse(match.Groups["year"].Value);
            int? v = match.Groups["stage"].Success ? int.Parse(match.Groups["stage"].Value) : (int?)null;
            int? exportStage = v;

            if (fiscalYear < 1900 || fiscalYear > 9999)
            {
                throw new InvalidDataException(
                    "The Urbis filename '" + fileName +
                    "' contains invalid fiscal year " + fiscalYear + ".");
            }

            return new UrbisSourceFile
            {
                FilePath = Path.GetFullPath(filePath),
                FileName = fileName,
                DocumentKind = detectedDocumentKind,
                Ico = match.Groups["ico"].Value,
                FiscalYear = fiscalYear,
                ExportStage = exportStage,
                Extension = match.Groups["extension"].Value.ToLowerInvariant()
            };
        }

        private static UrbisDocumentKind ParseDocumentKind(string value)
        {
            return string.Equals(
                value,
                "U_DENNIK",
                StringComparison.OrdinalIgnoreCase)
                ? UrbisDocumentKind.AccountingJournal
                : UrbisDocumentKind.GeneralLedger;
        }

        private static string Describe(UrbisDocumentKind documentKind)
        {
            return documentKind == UrbisDocumentKind.AccountingJournal
                ? "accounting journal"
                : "general ledger";
        }
    }
}