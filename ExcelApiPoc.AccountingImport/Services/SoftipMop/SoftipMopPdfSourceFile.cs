using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    internal sealed class SoftipMopPdfSourceFile
    {
        private static readonly Regex FileNamePattern = new Regex(
            @"^HL_KNIHA_(?<ico>\d{8})_(?<year>\d{4})(?:\s*\(\d+\))?\.pdf$",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);

        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public string Ico { get; private set; }
        public int FiscalYear { get; private set; }

        public static SoftipMopPdfSourceFile Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException(
                    "A Softip-MOP PDF source file is required.",
                    nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException(
                    "The Softip-MOP PDF source file was not found.",
                    filePath);

            string fileName = Path.GetFileName(filePath);
            Match match = FileNamePattern.Match(fileName);
            if (!match.Success)
                throw new InvalidDataException(
                    "The Softip-MOP filename '" + fileName +
                    "' is invalid. Expected " +
                    "'HL_KNIHA_<IČO>_<YYYY>.pdf'.");

            int fiscalYear = int.Parse(match.Groups["year"].Value);
            if (fiscalYear < 1900 || fiscalYear > 9999)
                throw new InvalidDataException(
                    "The Softip-MOP filename '" + fileName +
                    "' contains invalid fiscal year " + fiscalYear + ".");

            return new SoftipMopPdfSourceFile
            {
                FilePath = Path.GetFullPath(filePath),
                FileName = fileName,
                Ico = match.Groups["ico"].Value,
                FiscalYear = fiscalYear
            };
        }
    }
}
