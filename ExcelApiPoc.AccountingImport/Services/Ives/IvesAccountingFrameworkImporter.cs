using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Common;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ExcelApiPoc.AccountingImport.Services.Ives
{
    public sealed class IvesAccountingFrameworkImporter
    {
        public bool CanImport(string filePath, string accountingFormat)
        {
            return string.Equals(
                       accountingFormat,
                       "IVES",
                       StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(filePath) &&
                   Path.GetFileName(filePath).StartsWith(
                       "UCT_ROZVRH_",
                       StringComparison.OrdinalIgnoreCase) &&
                   IvesAccountingFrameworkParserDispatcher.CanParse(filePath);
        }

        public AccountingFrameworkImport Import(string filePath)
        {
            IIvesAccountingFrameworkSourceParser parser =
                IvesAccountingFrameworkParserDispatcher.Select(filePath);

            IvesAccountingFrameworkParseResult source =
                parser.Parse(filePath);

            AccountingFileNameMetadata fileNameMetadata = null;
            if (AccountingFileNameMetadataParser.TryParse(
                    source.SourceFileName,
                    out AccountingFileNameMetadata parsedMetadata))
            {
                if (parsedMetadata.DocumentKind !=
                    AccountingSourceDocumentKind.AccountingFramework)
                {
                    throw new InvalidDataException(
                        "Filename '" + source.SourceFileName +
                        "' does not identify an accounting framework.");
                }

                fileNameMetadata = parsedMetadata;
            }

            var result = new AccountingFrameworkImport
            {
                SourceFileName = source.SourceFileName,
                SourceFilePath = source.SourceFilePath,
                SourceFileHash = CalculateSha256(source.SourceFilePath),
                TechnicalType = parser.TechnicalType,
                AccountingFormat = "IVES",
                Ico = AccountingFileNameMetadataParser.ResolveIco(
                    source.SourceFileName,
                    source.Ico,
                    fileNameMetadata),
                FiscalYear =
                    AccountingFileNameMetadataParser.ResolveFiscalYear(
                        source.SourceFileName,
                        source.FiscalYear,
                        fileNameMetadata),
                ExportStage = fileNameMetadata?.ExportStage,
                ImportedAtUtc = DateTime.UtcNow
            };

            foreach (IvesAccountingFrameworkSourceRow sourceRow in source.Rows)
                result.Rows.Add(MapCanonicalRow(sourceRow));

            if (result.Rows.Count == 0)
            {
                throw new InvalidDataException(
                    "The IVES accounting framework does not contain any account records.");
            }

            return result;
        }

        private static AccountingFrameworkRow MapCanonicalRow(
            IvesAccountingFrameworkSourceRow sourceRow)
        {
            string accountCode =
                AccountCodeNormalizer.Normalize(sourceRow.AccountCode);

            if (accountCode.Length < 3)
            {
                throw new InvalidDataException(
                    "IVES accounting-framework source row " +
                    sourceRow.SourceRowNumber +
                    " contains invalid account code '" +
                    sourceRow.SourceAccountCode + "'.");
            }

            string syntheticCode = accountCode.Substring(0, 3);
            string analyticalCode = accountCode.Length == 3
                ? string.Empty
                : accountCode.Substring(3);

            return new AccountingFrameworkRow
            {
                SequenceNumber = sourceRow.SequenceNumber,
                SourceRecordNumber = sourceRow.SourceRowNumber,
                SourceSyntheticCode = syntheticCode,
                SourceAnalyticalCode = analyticalCode,
                SyntheticCode = syntheticCode,
                AnalyticalCode = analyticalCode,
                AccountCode = accountCode,
                AccountName = sourceRow.AccountName,
                Type = sourceRow.Type,
                ActivityCode = sourceRow.ActivityCode,
                PsFlag = sourceRow.PsFlag,
                BuFlag = sourceRow.BuFlag,
                RuFlag = sourceRow.RuFlag,
                PlFlag = sourceRow.PlFlag,
                Currency = sourceRow.Currency,
                ValidFrom = sourceRow.ValidFrom,
                ValidTo = sourceRow.ValidTo,
                RowKind = analyticalCode.Length == 0
                    ? AccountingFrameworkRowKind.SyntheticAccount
                    : AccountingFrameworkRowKind.AnalyticalAccount
            };
        }

        private static string CalculateSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToString(
                    sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
            }
        }
    }
}
