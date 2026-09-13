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
            new[] { "account-row", "subtotal-row", "signature-row" },
            definition.RecordRules.Select(rule => rule.Id));
        Assert.Single(definition.RecordContinuations);
    }

    [Fact]
    public void Initial_record_rules_classify_only_the_three_supported_shapes()
    {
        var definition = Assert.IsType<LayoutDefinition>(Load().Definition);
        var matcher = new BaselineRecordRuleMatcher();
        var account = Record(Group(1, 100,
            Token(1, "01110", 22.5, 48),
            Token(1, "ACCOUNT NAME", 52, 300),
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
            Token(13, "Vypracoval", 132, 190),
            Token(13, "Schválil", 350, 400),
            Token(13, "Dátum", 650, 700)));
        var unsupportedSummary = Record(Group(13, 450,
            Token(13, "Summary", 132, 200),
            Token(13, "0,00", 735, 770)));

        AssertMatch("account-row", matcher.Match(account, definition.RecordRules));
        AssertMatch("subtotal-row", matcher.Match(subtotal, definition.RecordRules));
        AssertMatch("signature-row", matcher.Match(signature, definition.RecordRules));
        Assert.Equal(
            RuleMatchStatus.Unmatched,
            matcher.Match(unsupportedSummary, definition.RecordRules).Status);
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
