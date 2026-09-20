using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    public sealed class SoftipMopPdfGeneralLedgerImporter :
        IGeneralLedgerImporter
    {
        private const string LayoutResourceName =
            "ExcelApiPoc.AccountingImport.PdfLayouts." +
            "SoftipMop.general-ledger.v1.json";

        private static readonly Regex PeriodPattern = new Regex(
            @"\bmesiacu\s+(?<period>\d{6})\s*$",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);

        public bool CanImport(
            string filePath,
            string accountingFormat)
        {
            if (!IsSoftipMop(accountingFormat))
                return false;

            try
            {
                SoftipMopPdfSourceFile.Parse(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public GeneralLedgerImport Import(string filePath)
        {
            SoftipMopPdfSourceFile source =
                SoftipMopPdfSourceFile.Parse(filePath);
            LayoutDefinition layout = LoadLayout();
            LayoutRecognitionResult recognition =
                new ITextLayoutRecognizer().Recognize(
                    source.FilePath,
                    layout);
            int throughMonth = ReadAndValidatePeriod(
                recognition.Document,
                source);

            GeneralLedgerImport result =
                new SoftipMopGeneralLedgerMapper().Map(
                    recognition,
                    source,
                    throughMonth,
                    CalculateSha256(source.FilePath));

            if (!result.ImportReport.IsValid)
                throw CreateValidationException(result.ImportReport);

            return result;
        }

        private static LayoutDefinition LoadLayout()
        {
            Assembly assembly =
                typeof(SoftipMopPdfGeneralLedgerImporter).Assembly;
            using (Stream stream =
                   assembly.GetManifestResourceStream(LayoutResourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        "Embedded Softip-MOP layout '" +
                        LayoutResourceName + "' was not found.");

                LayoutLoadResult result =
                    new LayoutDefinitionLoader().Load(stream);
                if (!result.IsValid)
                    throw new InvalidDataException(
                        "Embedded Softip-MOP layout is invalid: " +
                        string.Join(
                            "; ",
                            result.Messages.Select(
                                message => message.Message)));

                return result.Definition;
            }
        }

        private static int ReadAndValidatePeriod(
            PdfDocument document,
            SoftipMopPdfSourceFile source)
        {
            string[] periods = document.Tokens
                .Select(token => PeriodPattern.Match(token.Text))
                .Where(match => match.Success)
                .Select(match => match.Groups["period"].Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (periods.Length != 1)
                throw new InvalidDataException(
                    "The Softip-MOP PDF must contain exactly one " +
                    "distinct accounting period, but found " +
                    periods.Length + ".");

            int year = int.Parse(
                periods[0].Substring(0, 4),
                CultureInfo.InvariantCulture);
            int month = int.Parse(
                periods[0].Substring(4, 2),
                CultureInfo.InvariantCulture);

            if (year != source.FiscalYear)
                throw new InvalidDataException(
                    "The Softip-MOP PDF period year " + year +
                    " does not match filename year " +
                    source.FiscalYear + ".");
            if (month < 1 || month > 12)
                throw new InvalidDataException(
                    "The Softip-MOP PDF contains invalid accounting " +
                    "month " + month + ".");

            return month;
        }

        private static bool IsSoftipMop(string value)
        {
            return
                string.Equals(
                    value,
                    "Softip-MOP",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    value,
                    "SoftipMop",
                    StringComparison.OrdinalIgnoreCase);
        }

        private static InvalidDataException CreateValidationException(
            ImportReport report)
        {
            ImportDiagnostic firstError = report.Diagnostics.First(
                diagnostic =>
                    diagnostic.Severity ==
                    ImportDiagnosticSeverity.Error);
            return new InvalidDataException(
                "Softip-MOP general-ledger validation failed: " +
                firstError.Message);
        }

        private static string CalculateSha256(string filePath)
        {
            using (Stream stream = File.OpenRead(filePath))
            using (SHA256 sha256 = SHA256.Create())
                return BitConverter
                    .ToString(sha256.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
