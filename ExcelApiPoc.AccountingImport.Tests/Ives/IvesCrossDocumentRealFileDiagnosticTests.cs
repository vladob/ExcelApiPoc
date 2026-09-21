using ExcelApiPoc.AccountingImport.Services.Common;
using ExcelApiPoc.AccountingImport.Services.Ives;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesCrossDocumentRealFileDiagnosticTests
{
    private readonly ITestOutputHelper output;

    public IvesCrossDocumentRealFileDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Diagnose_real_journal_ledger_and_framework_relationships()
    {
        string? journalPath = PathFromEnvironment(
            "IVES_JOURNAL_XML_TEST_FILE");
        string? ledgerPath = PathFromEnvironment(
            "IVES_GENERAL_LEDGER_XML_TEST_FILE");
        string? frameworkPath = PathFromEnvironment(
            "IVES_ACCOUNTING_FRAMEWORK_XML_TEST_FILE");

        if (journalPath == null ||
            ledgerPath == null ||
            frameworkPath == null)
        {
            output.WriteLine(
                "The IVES journal, general-ledger and accounting-framework " +
                "XML test-file variables must all be set.");
            return;
        }

        IvesJournalParseResult journal =
            new IvesXmlJournalParser().Parse(journalPath);
        IvesGeneralLedgerParseResult ledger =
            new IvesXmlGeneralLedgerParser().Parse(ledgerPath);
        IvesAccountingFrameworkParseResult framework =
            new IvesXmlAccountingFrameworkParser().Parse(frameworkPath);

        Assert.Equal(journal.Ico, ledger.Ico);
        Assert.Equal(journal.Ico, framework.Ico);
        Assert.Equal(journal.FiscalYear, ledger.FiscalYear);
        Assert.Equal(journal.FiscalYear, framework.FiscalYear);

        HashSet<string> journalAccounts = AccountSet(
            journal.TransactionRows
                .SelectMany(row => new[]
                {
                    row.DebitCompositeAccount,
                    row.CreditCompositeAccount
                }));

        var ledgerAccountsByActivity =
            ledger.Activities
                .SelectMany(
                    activity => activity.AccountRows.Select(
                        row => new
                        {
                            Activity = activity.Name ?? string.Empty,
                            Account = NormalizeAccount(row.AccountCode),
                            Row = row
                        }))
                .Where(item => item.Account.Length > 0)
                .ToList();

        HashSet<string> ledgerAccounts =
            ledgerAccountsByActivity
                .Select(item => item.Account)
                .ToHashSet(StringComparer.Ordinal);

        HashSet<string> frameworkAccounts = AccountSet(
            framework.Rows.Select(row => row.AccountCode));

        string[] journalNotInFramework =
            journalAccounts.Except(
                frameworkAccounts,
                StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        string[] ledgerNotInFramework =
            ledgerAccounts.Except(
                frameworkAccounts,
                StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        string[] journalNotInLedger =
            journalAccounts.Except(
                ledgerAccounts,
                StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        string[] ledgerNotInJournal =
            ledgerAccounts.Except(
                journalAccounts,
                StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        var duplicateLedgerAccountsAcrossActivities =
            ledgerAccountsByActivity
                .GroupBy(
                    item => item.Account,
                    StringComparer.Ordinal)
                .Select(group => new
                {
                    Account = group.Key,
                    Activities = group
                        .Select(item => item.Activity)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(
                            value => value,
                            StringComparer.Ordinal)
                        .ToArray()
                })
                .Where(item => item.Activities.Length > 1)
                .OrderBy(
                    item => item.Account,
                    StringComparer.Ordinal)
                .ToArray();

        decimal journalDebitTurnover =
            journal.TransactionRows
                .Where(row =>
                    !string.IsNullOrWhiteSpace(
                        row.DebitCompositeAccount))
                .Sum(row => row.Amount.GetValueOrDefault());

        decimal journalCreditTurnover =
            journal.TransactionRows
                .Where(row =>
                    !string.IsNullOrWhiteSpace(
                        row.CreditCompositeAccount))
                .Sum(row => row.Amount.GetValueOrDefault());

        decimal ledgerDebitTurnover =
            ledgerAccountsByActivity.Sum(
                item => item.Row.DebitTurnover.GetValueOrDefault());

        decimal ledgerCreditTurnover =
            ledgerAccountsByActivity.Sum(
                item => item.Row.CreditTurnover.GetValueOrDefault());

        output.WriteLine(
            "IVES AJ / GL / AF cross-document diagnostic completed.");
        output.WriteLine("IČO                 : " + journal.Ico);
        output.WriteLine("Fiscal year         : " + journal.FiscalYear);
        output.WriteLine(
            "Journal transactions: " +
            journal.TransactionRows.Count);
        output.WriteLine(
            "GL activities        : " +
            ledger.Activities.Count);
        output.WriteLine(
            "GL account rows      : " +
            ledgerAccountsByActivity.Count);
        output.WriteLine(
            "GL unique accounts   : " +
            ledgerAccounts.Count);
        output.WriteLine(
            "AF rows               : " +
            framework.Rows.Count);
        output.WriteLine(
            "AF unique accounts    : " +
            frameworkAccounts.Count);
        output.WriteLine(
            "AJ unique accounts    : " +
            journalAccounts.Count);
        output.WriteLine(
            "AJ not in AF          : " +
            journalNotInFramework.Length);
        output.WriteLine(
            "GL not in AF          : " +
            ledgerNotInFramework.Length);
        output.WriteLine(
            "AJ not in GL          : " +
            journalNotInLedger.Length);
        output.WriteLine(
            "GL not in AJ          : " +
            ledgerNotInJournal.Length);
        output.WriteLine(
            "GL accounts in >1 activity: " +
            duplicateLedgerAccountsAcrossActivities.Length);
        output.WriteLine(
            "AJ debit turnover     : " +
            journalDebitTurnover);
        output.WriteLine(
            "GL debit turnover     : " +
            ledgerDebitTurnover);
        output.WriteLine(
            "AJ credit turnover    : " +
            journalCreditTurnover);
        output.WriteLine(
            "GL credit turnover    : " +
            ledgerCreditTurnover);

        WriteSamples(
            "AJ not in AF",
            journalNotInFramework);
        WriteSamples(
            "GL not in AF",
            ledgerNotInFramework);
        WriteSamples(
            "AJ not in GL",
            journalNotInLedger);
        WriteSamples(
            "GL not in AJ",
            ledgerNotInJournal);

        foreach (var duplicate in
                 duplicateLedgerAccountsAcrossActivities.Take(20))
        {
            output.WriteLine(
                "  GL multi-activity " +
                duplicate.Account +
                " => " +
                string.Join(
                    " | ",
                    duplicate.Activities));
        }

        Assert.Equal(
            journalDebitTurnover,
            ledgerDebitTurnover);
        Assert.Equal(
            journalCreditTurnover,
            ledgerCreditTurnover);
    }

    private void WriteSamples(
        string label,
        IReadOnlyList<string> values)
    {
        foreach (string value in values.Take(20))
            output.WriteLine("  " + label + " : " + value);
    }

    private static HashSet<string> AccountSet(
        IEnumerable<string?> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeAccount(value!))
            .Where(value => value.Length > 0)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string NormalizeAccount(string value)
    {
        return AccountCodeNormalizer.Normalize(value);
    }

    private string? PathFromEnvironment(string variableName)
    {
        string? path =
            Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(variableName + " is not set.");
            return null;
        }

        Assert.True(
            File.Exists(path),
            variableName +
            " does not point to an existing file: " +
            path);

        return path;
    }
}
