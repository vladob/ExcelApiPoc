using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Models.Reporting;
using ExcelApiPoc.AccountingImport.Services.Common;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExcelApiPoc.AccountingImport.Services.SoftipMop
{
    internal sealed class SoftipMopGeneralLedgerMapper
    {
        private const decimal AmountTolerance = 0.01m;

        public GeneralLedgerImport Map(
            LayoutRecognitionResult recognition,
            SoftipMopPdfSourceFile source,
            int throughMonth,
            string sourceFileHash)
        {
            if (recognition == null) throw new ArgumentNullException(nameof(recognition));
            if (source == null) throw new ArgumentNullException(nameof(source));

            EnsureCompleteRecognition(recognition);

            var report = new ImportReport
            {
                AccountingFormat = "Softip-MOP",
                ImportType = "GeneralLedger",
                SourceFileName = source.FileName
            };
            AddRecordCounts(recognition, report);

            var result = new GeneralLedgerImport
            {
                SourceFileName = source.FileName,
                SourceFilePath = source.FilePath,
                SourceFileHash = sourceFileHash,
                TechnicalType = "PDF",
                AccountingFormat = "Softip-MOP",
                Ico = source.Ico,
                CompanyName = string.Empty,
                FiscalYear = source.FiscalYear,
                ThroughMonth = throughMonth,
                PeriodHeader =
                    throughMonth.ToString(CultureInfo.InvariantCulture) +
                    "/" +
                    source.FiscalYear.ToString(CultureInfo.InvariantCulture),
                ImportedAtUtc = DateTime.UtcNow,
                ImportReport = report
            };

            var accountCodes = new HashSet<string>(StringComparer.Ordinal);
            int sequenceNumber = 0;
            for (int recordIndex = 0;
                 recordIndex < recognition.Records.Count;
                 recordIndex++)
            {
                RecognizedRecord record = recognition.Records[recordIndex];
                if (!string.Equals(
                        record.RuleId,
                        "account-row",
                        StringComparison.Ordinal))
                    continue;

                var fields = record.Fields.ToDictionary(
                    field => field.FieldId,
                    StringComparer.Ordinal);
                string accountCode = AccountCodeNormalizer.Normalize(
                    Text(fields, "key", record));
                string accountName = Text(fields, "label", record).Trim();
                if (accountCode.Length < 3)
                    throw Error(record, "Account code '" + accountCode +
                        "' contains fewer than three characters.");
                if (!accountCodes.Add(accountCode))
                    throw Error(record, "Duplicate account code '" +
                        accountCode + "'.");

                decimal opening = Amount(fields, "value-1", record);
                decimal periodDebit = Amount(fields, "value-2", record);
                decimal periodCredit = Amount(fields, "value-3", record);
                decimal annualDebit = Amount(fields, "value-4", record);
                decimal annualCredit = Amount(fields, "value-5", record);
                decimal closing = Amount(fields, "value-6", record);

                sequenceNumber++;
                result.Rows.Add(new GeneralLedgerRow
                {
                    SequenceNumber = sequenceNumber,
                    SourceRecordNumber = recordIndex + 1,
                    SyntheticCode = accountCode.Substring(0, 3),
                    AnalyticalCode = accountCode.Length > 3
                        ? accountCode.Substring(3)
                        : string.Empty,
                    AccountCode = accountCode,
                    AccountName = accountName,
                    OpeningDebit = opening > 0m ? opening : 0m,
                    OpeningCredit = opening < 0m ? -opening : 0m,
                    PeriodDebitTurnover = periodDebit,
                    PeriodCreditTurnover = periodCredit,
                    AnnualDebitTurnover = annualDebit,
                    AnnualCreditTurnover = annualCredit,
                    ClosingDebit = closing > 0m ? closing : 0m,
                    ClosingCredit = closing < 0m ? -closing : 0m,
                    Plan = 0m
                });

                AddBalanceValidation(
                    report,
                    source,
                    record,
                    accountCode,
                    opening + annualDebit - annualCredit,
                    closing,
                    recordIndex + 1);
            }

            if (result.Rows.Count == 0)
                throw new InvalidDataException(
                    "The Softip-MOP general ledger does not contain any account records.");

            return result;
        }

        private static void EnsureCompleteRecognition(
            LayoutRecognitionResult recognition)
        {
            if (recognition.Diagnostics.Count != 0)
                throw new InvalidDataException(
                    "Softip-MOP PDF recognition produced " +
                    recognition.Diagnostics.Count + " diagnostic(s).");

            RecognizedRecord invalidRecord = recognition.Records.FirstOrDefault(
                record => record.Status != RuleMatchStatus.Matched);
            if (invalidRecord != null)
                throw Error(
                    invalidRecord,
                    "Record classification is " +
                    invalidRecord.Status + ".");

            foreach (RecognizedRecord record in recognition.Records)
            {
                RecordFieldMatchResult invalidField = record.Fields.FirstOrDefault(
                    field => field.Status != RuleMatchStatus.Matched);
                if (invalidField != null)
                    throw Error(
                        record,
                        "Field '" + invalidField.FieldId +
                        "' recognition is " + invalidField.Status + ".");
            }
        }

        private static string Text(
            IDictionary<string, RecordFieldMatchResult> fields,
            string fieldId,
            RecognizedRecord record)
        {
            RecordFieldMatchResult field;
            if (!fields.TryGetValue(fieldId, out field) || field.Value == null)
                throw Error(record, "Required field '" + fieldId + "' is missing.");
            return field.Value.Value;
        }

        private static decimal Amount(
            IDictionary<string, RecordFieldMatchResult> fields,
            string fieldId,
            RecognizedRecord record)
        {
            return SoftipMopAmountParser.Parse(
                Text(fields, fieldId, record),
                Location(record) + ", field '" + fieldId + "'");
        }

        private static void AddRecordCounts(
            LayoutRecognitionResult recognition,
            ImportReport report)
        {
            foreach (IGrouping<string, RecognizedRecord> group in
                     recognition.Records
                         .Where(record => record.RuleId != null)
                         .GroupBy(record => record.RuleId))
                report.RecordCounts[group.Key] = group.Count();

            report.RecordCounts["LogicalRecords"] = recognition.Records.Count;
            report.RecordCounts["Fields"] =
                recognition.Records.Sum(record => record.Fields.Count);
        }

        private static void AddBalanceValidation(
            ImportReport report,
            SoftipMopPdfSourceFile source,
            RecognizedRecord record,
            string accountCode,
            decimal expected,
            decimal actual,
            int sourceRecordNumber)
        {
            decimal difference = actual - expected;
            bool valid = Math.Abs(difference) <= AmountTolerance;
            var provenance = new SourceProvenance
            {
                SourceFileName = source.FileName,
                WorksheetName =
                    "PDF pages " +
                    record.SourceRecord.StartPageNumber + "-" +
                    record.SourceRecord.EndPageNumber,
                RecordSet = "Accounts",
                SequenceNumber = sourceRecordNumber,
                SourceRowNumber = sourceRecordNumber
            };
            const string description =
                "Closing balance equals opening balance plus " +
                "annual debit turnover minus annual credit turnover.";

            report.ValidationResults.Add(new ImportValidationResult
            {
                Code = "SOFTIP_MOP.ACCOUNT.BALANCE",
                Scope = accountCode,
                Description = description,
                IsValid = valid,
                ExpectedAmount = expected,
                ActualAmount = actual,
                Difference = difference,
                Tolerance = AmountTolerance,
                Source = provenance
            });
            if (!valid)
                report.Diagnostics.Add(new ImportDiagnostic
                {
                    Code = "SOFTIP_MOP.ACCOUNT.BALANCE",
                    Severity = ImportDiagnosticSeverity.Error,
                    Message =
                        description + " Account '" + accountCode +
                        "', expected " + expected +
                        ", actual " + actual +
                        ", difference " + difference + ".",
                    Source = provenance
                });
        }

        private static InvalidDataException Error(
            RecognizedRecord record,
            string message)
        {
            return new InvalidDataException(
                Location(record) + ": " + message);
        }

        private static string Location(RecognizedRecord record)
        {
            return
                "PDF pages " +
                record.SourceRecord.StartPageNumber + "-" +
                record.SourceRecord.EndPageNumber +
                ", record '" + record.SourceRecord.Text + "'";
        }
    }
}
