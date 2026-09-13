using ExcelApiPoc.AccountingImport.Services.Ives;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ExcelApiPoc.AccountingImport.Tests.Ives
{
    public sealed class IvesExcelJournalParserTests
    {
        [Theory]
        [InlineData("U_DENNIK_00322881_2022.xls", 2022, 3376, 30, 18, 24, 26, 28, 1579)]
        [InlineData("U_DENNIK_00322881_2023.xls", 2023, 3361, 34, 16, 25, 29, 31, 1575)]
        [InlineData("U_DENNIK_00322881_2024.xls", 2024, 4500, 30, 18, 24, 26, 28, 2125)]
        public void Parse_DiscoversEachPhysicalLayoutAndMetadata(
            string fileName,
            int fiscalYear,
            int sourceRows,
            int columns,
            int creditColumn,
            int amountColumn,
            int currencyColumn,
            int textColumn,
            int transactions)
        {
            IvesJournalParseResult result = Parse(fileName);

            Assert.Equal("00322881", result.Ico);
            Assert.Equal(fiscalYear, result.FiscalYear);
            Assert.Equal(new DateTime(fiscalYear, 1, 1), result.PeriodStart);
            Assert.Equal(new DateTime(fiscalYear, 12, 31), result.PeriodEnd);
            Assert.Equal(sourceRows, result.SourceRowCount);
            Assert.Equal("Sheet1", result.WorksheetName);

            Assert.Equal(columns, result.Layout.ColumnCount);
            Assert.Equal(12, result.Layout.HeaderRowNumber);
            Assert.Equal(2, result.Layout.DateColumn + 1);
            Assert.Equal(5, result.Layout.DocumentNumberColumn + 1);
            Assert.Equal(9, result.Layout.DebitAccountColumn + 1);
            Assert.Equal(creditColumn, result.Layout.CreditAccountColumn + 1);
            Assert.Equal(amountColumn, result.Layout.AmountColumn + 1);
            Assert.Equal(currencyColumn, result.Layout.CurrencyColumn + 1);
            Assert.Equal(textColumn, result.Layout.TextColumn + 1);

            Assert.Equal(transactions, result.TransactionRows.Count);
            Assert.Equal(transactions, result.ModuleRows.Count);
            Assert.Single(result.ReportTotalRows);
            Assert.Empty(result.UnclassifiedRows);
        }

        [Theory]
        [InlineData("U_DENNIK_00322881_2022.xls", 374, 104, 0)]
        [InlineData("U_DENNIK_00322881_2023.xls", 321, 107, 5)]
        [InlineData("U_DENNIK_00322881_2024.xls", 722, 146, 0)]
        public void Parse_PreservesOneSidedAccountsAndSignedAmounts(
            string fileName,
            int missingDebit,
            int missingCredit,
            int negativeAmounts)
        {
            IvesJournalParseResult result = Parse(fileName);

            Assert.Equal(missingDebit, result.TransactionRows.Count(row =>
                string.IsNullOrWhiteSpace(row.DebitCompositeAccount)));
            Assert.Equal(missingCredit, result.TransactionRows.Count(row =>
                string.IsNullOrWhiteSpace(row.CreditCompositeAccount)));
            Assert.Equal(negativeAmounts, result.TransactionRows.Count(row => row.Amount < 0m));
            Assert.DoesNotContain(result.TransactionRows, row =>
                string.IsNullOrWhiteSpace(row.DebitCompositeAccount) &&
                string.IsNullOrWhiteSpace(row.CreditCompositeAccount));
        }

        [Theory]
        [InlineData("U_DENNIK_00322881_2022.xls", "1500577.07")]
        [InlineData("U_DENNIK_00322881_2023.xls", "531194.94")]
        [InlineData("U_DENNIK_00322881_2024.xls", "2114932.51")]
        public void Parse_RetainsEachReportedFooterAmount(
            string fileName,
            string expectedFirstAmount)
        {
            IvesJournalParseResult result = Parse(fileName);
            decimal expected = decimal.Parse(expectedFirstAmount,
                System.Globalization.CultureInfo.InvariantCulture);

            Assert.Equal(expected, result.ReportTotalRows.Single().ReportedAmounts[0]);
            Assert.All(result.TransactionRows, row => Assert.Equal("€", row.Currency));

            if (fileName.Contains("2023"))
            {
                Assert.Equal(new[] { expected, expected },
                    result.ReportTotalRows.Single().ReportedAmounts);
            }
        }

        [Fact]
        public void Parse_PairsEveryTransactionWithTheFollowingModuleRow()
        {
            IvesJournalParseResult result = Parse("U_DENNIK_00322881_2024.xls");

            Assert.Equal(Enumerable.Range(1, result.TransactionRows.Count),
                result.TransactionRows.Select(row => row.SequenceNumber));
            Assert.Equal(Enumerable.Range(1, result.ModuleRows.Count),
                result.ModuleRows.Select(row => row.SequenceNumber));

            foreach (IvesJournalSourceRow transaction in result.TransactionRows)
            {
                Assert.Equal(transaction.SourceRowNumber + 1,
                    transaction.RelatedSourceRowNumber);
                IvesJournalSourceRow module = result.ModuleRows.Single(row =>
                    row.SourceRowNumber == transaction.RelatedSourceRowNumber);
                Assert.Equal(transaction.SourceRowNumber, module.RelatedSourceRowNumber);
                Assert.Equal(transaction.Module, module.Module);
                Assert.False(string.IsNullOrWhiteSpace(module.Module));
            }
        }

        [Fact]
        public void Parse_ExtractsRepresentativeTransactionWithoutCanonicalMapping()
        {
            IvesJournalParseResult result = Parse("U_DENNIK_00322881_2024.xls");
            IvesJournalSourceRow row = result.TransactionRows[0];

            Assert.Equal(IvesJournalRowKind.Transaction, row.Kind);
            Assert.Equal(new DateTime(2024, 1, 2), row.PostingDate);
            Assert.False(string.IsNullOrWhiteSpace(row.DocumentNumber));
            Assert.True(row.Amount.HasValue);
            Assert.Equal("€", row.Currency);
            Assert.False(string.IsNullOrWhiteSpace(row.Module));
            Assert.Contains('.', row.DebitCompositeAccount ?? row.CreditCompositeAccount);
        }

        private static IvesJournalParseResult Parse(string fileName)
        {
            return new IvesExcelJournalParser().Parse(GetFixturePath(fileName));
        }

        private static string GetFixturePath(string fileName)
        {
            return Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "Ives",
                fileName);
        }
    }
}
