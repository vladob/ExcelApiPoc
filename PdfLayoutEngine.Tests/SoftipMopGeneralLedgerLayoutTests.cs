using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Records;

namespace PdfLayoutEngine.Tests;

public sealed class SoftipMopGeneralLedgerLayoutTests
{
    [Fact]
    public void Real_layout_definition_is_structurally_valid()
    {
        var result = Load();

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Messages.Select(message => message.Message)));
        var definition = Assert.IsType<LayoutDefinition>(result.Definition);
        Assert.Equal("softip-mop-general-ledger", definition.Id);
        Assert.Equal(new[] { "page-body" }, definition.Sections.Select(section => section.Id));
        Assert.Equal(
            new[]
            {
                "account-row",
                "subtotal-row",
                "report-total-row",
                "summary-row",
                "signature-row"
            },
            definition.RecordRules.Select(rule => rule.Id));
        Assert.Single(definition.RecordContinuations);
        Assert.Equal(
            new[]
            {
                "account-row",
                "subtotal-row",
                "report-total-row",
                "summary-row",
                "signature-row"
            },
            definition.RecordFields.Select(fieldSet => fieldSet.RecordRuleId));
        AssertFieldIds(definition, "account-row",
            "key", "label", "value-1", "value-2", "value-3", "value-4", "value-5", "value-6");
        AssertFieldIds(definition, "subtotal-row",
            "label", "key", "value-1", "value-2", "value-3", "value-4", "value-5", "value-6");
        AssertFieldIds(definition, "report-total-row",
            "label", "value-1", "value-2", "value-3", "value-4", "value-5", "value-6");
        AssertFieldIds(definition, "summary-row",
            "label", "value-1", "value-2", "value-3", "value-4", "value-5", "value-6");
        AssertFieldIds(definition, "signature-row",
            "label-1", "label-2", "label-3");
    }

    [Fact]
    public void Record_rules_classify_supported_shapes_without_catching_unknown_summary()
    {
        var definition = Assert.IsType<LayoutDefinition>(Load().Definition);
        var matcher = new BaselineRecordRuleMatcher();
        var account = Record(Group(1, 100,
            Token(1, "01110", 22.5, 48),
            Token(1, "ACCOUNT NAME", 81, 300),
            Token(1, "0,00", 350, 372),
            Token(1, "0,00", 420, 452),
            Token(1, "0,00", 509, 532),
            Token(1, "0,00", 576, 611),
            Token(1, "0,00", 668, 691),
            Token(1, "0,00", 735, 770)));
        var subtotalLabel = Group(8, 100,
            Token(8, "Medzisúčet za  :", 42.7, 103.7),
            Token(8, "512", 112.5, 125.8));
        var subtotalValues = Group(9, 500,
            Token(9, "0,00", 349.6, 371.8),
            Token(9, "100,00", 421, 452.1),
            Token(9, "0,00", 509.3, 531.6),
            Token(9, "1 211,00", 575.5, 611.1),
            Token(9, "0,00", 668.3, 690.6),
            Token(9, "1 211,00", 734.5, 770.1));
        var continuation = new RecordContinuationEvidence(
            "values-continued-on-next-page",
            subtotalLabel,
            subtotalValues,
            Array.Empty<MatchCriterionEvidence>());
        var subtotal = new BaselineRecord(
            "page-body",
            new[] { subtotalLabel, subtotalValues },
            new[] { continuation });
        var signature = Record(Group(13, 400,
            Token(13, "Vypracoval", 30, 69.6),
            Token(13, "Schválil", 212.25, 239.8),
            Token(13, "Dátum", 383.25, 406.8)));
        var reportTotal = SevenTokenRecord(12, "Spolu", 22.5);
        var summary = SevenTokenRecord(13, "Hospodársky výsledok", 132.7);
        var unsupportedSummary = Record(Group(13, 450,
            Token(13, "Unknown summary", 132, 200),
            Token(13, "0,00", 735, 770)));

        AssertMatch("account-row", matcher.Match(account, definition.RecordRules));
        AssertMatch("subtotal-row", matcher.Match(subtotal, definition.RecordRules));
        AssertMatch("report-total-row", matcher.Match(reportTotal, definition.RecordRules));
        AssertMatch("summary-row", matcher.Match(summary, definition.RecordRules));
        AssertMatch("signature-row", matcher.Match(signature, definition.RecordRules));
        Assert.Equal(
            RuleMatchStatus.Unmatched,
            matcher.Match(unsupportedSummary, definition.RecordRules).Status);

        AssertFields(definition, "account-row", account, 8);
        AssertFields(definition, "subtotal-row", subtotal, 8);
        AssertFields(definition, "report-total-row", reportTotal, 7);
        AssertFields(definition, "summary-row", summary, 7);
        AssertFields(definition, "signature-row", signature, 3);
    }

    private static void AssertFieldIds(
        LayoutDefinition definition,
        string recordRuleId,
        params string[] expectedIds)
    {
        var fieldSet = Assert.Single(
            definition.RecordFields,
            item => item.RecordRuleId == recordRuleId);
        Assert.Equal(expectedIds, fieldSet.Fields.Select(field => field.Id));
    }

    private static void AssertFields(
        LayoutDefinition definition,
        string recordRuleId,
        BaselineRecord record,
        int expectedCount)
    {
        var fieldSet = Assert.Single(
            definition.RecordFields,
            item => item.RecordRuleId == recordRuleId);
        var fields = new BaselineRecordFieldMatcher().Match(
            record,
            fieldSet,
            definition.Defaults);
        Assert.Equal(expectedCount, fields.Count);
        Assert.All(fields, field => Assert.Equal(RuleMatchStatus.Matched, field.Status));
        Assert.All(fields, field => Assert.Single(field.Value!.Evidence.SourceTokens));
    }

    private static void AssertMatch(string ruleId, BaselineRecordRuleMatchResult result)
    {
        Assert.Equal(RuleMatchStatus.Matched, result.Status);
        Assert.Equal(ruleId, Assert.Single(result.Matches).RuleId);
    }

    private static LayoutLoadResult Load()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            "general-ledger.v1.json");
        using var stream = File.OpenRead(path);
        return new LayoutDefinitionLoader().Load(stream);
    }

    private static BaselineRecord Record(BaselineGroup group) =>
        new BaselineRecord("page-body", new[] { group });

    private static BaselineRecord SevenTokenRecord(int page, string label, double left) =>
        Record(Group(page, 450,
            Token(page, label, left, 300),
            Token(page, "0,00", 350, 372),
            Token(page, "0,00", 420, 452),
            Token(page, "0,00", 509, 532),
            Token(page, "0,00", 576, 611),
            Token(page, "0,00", 668, 691),
            Token(page, "0,00", 735, 770)));

    private static BaselineGroup Group(
        int page,
        double baseline,
        params PdfTextToken[] tokens) =>
        new BaselineGroup(page, tokens.Select(token =>
            new PdfTextToken(
                token.PageNumber,
                token.OriginalText,
                token.Left,
                token.Right,
                baseline,
                token.IsBold,
                token.IsItalic)));

    private static PdfTextToken Token(
        int page,
        string text,
        double left,
        double right) =>
        new PdfTextToken(page, text, left, right, 0, false, false);
}
