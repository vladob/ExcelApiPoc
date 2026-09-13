using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Recognition;

public sealed class TokenRuleMatcher
{
    public RuleMatchResult Match(
        RecognitionRuleDefinition rule,
        IEnumerable<PdfTextToken> tokens,
        MatchingDefaults? defaults = null)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));

        defaults = defaults ?? new MatchingDefaults();
        var tokenArray = tokens.ToArray();
        var lastPage = tokenArray.Length == 0 ? 0 : tokenArray.Max(token => token.PageNumber);
        var matches = tokenArray
            .Where(token => IsInPageScope(rule.PageScope, token.PageNumber, lastPage))
            .Select(token => Evaluate(rule, token, defaults))
            .Where(evidence => evidence.Criteria.All(criterion => criterion.IsMatch))
            .ToArray();

        var status = matches.Length == 0
            ? RuleMatchStatus.Unmatched
            : matches.Length == 1 || rule.AllowMultipleMatches || rule.PageScope == PageScope.Repeated
                ? RuleMatchStatus.Matched
                : RuleMatchStatus.Ambiguous;

        return new RuleMatchResult(rule.Id, status, Array.AsReadOnly(matches));
    }

    private static RecognitionEvidence Evaluate(
        RecognitionRuleDefinition rule,
        PdfTextToken token,
        MatchingDefaults defaults)
    {
        var criteria = new List<MatchCriterionEvidence>();
        if (rule.Text != null)
        {
            var expected = TextNormalizer.Normalize(rule.Text);
            var comparison = rule.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var matched = rule.TextMatch == TextMatchMode.Equals
                ? string.Equals(token.Text, expected, comparison)
                : rule.TextMatch == TextMatchMode.Contains
                    ? token.Text.IndexOf(expected, comparison) >= 0
                    : Regex.IsMatch(token.Text, expected, rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            criteria.Add(new MatchCriterionEvidence("Text", matched, expected, token.Text));
        }

        if (rule.Horizontal != null)
        {
            var actual = rule.Horizontal.Anchor == HorizontalAnchor.Left
                ? token.Left
                : rule.Horizontal.Anchor == HorizontalAnchor.Right
                    ? token.Right
                    : (token.Left + token.Right) / 2d;
            var tolerance = rule.Horizontal.Tolerance ?? defaults.HorizontalTolerance;
            criteria.Add(Coordinate(rule.Horizontal.Anchor.ToString(), rule.Horizontal.Position, actual, tolerance));
        }

        if (rule.Baseline.HasValue)
        {
            var tolerance = rule.BaselineTolerance ?? defaults.BaselineTolerance;
            criteria.Add(Coordinate("Baseline", rule.Baseline.Value, token.Baseline, tolerance));
        }

        if (rule.IsBold.HasValue)
            criteria.Add(new MatchCriterionEvidence("Bold", token.IsBold == rule.IsBold.Value, rule.IsBold.Value.ToString(), token.IsBold.ToString()));
        if (rule.IsItalic.HasValue)
            criteria.Add(new MatchCriterionEvidence("Italic", token.IsItalic == rule.IsItalic.Value, rule.IsItalic.Value.ToString(), token.IsItalic.ToString()));

        return new RecognitionEvidence(rule.Id, new[] { token }, "Token evaluated against rule criteria.", criteria);
    }

    private static MatchCriterionEvidence Coordinate(string name, double expected, double actual, double tolerance) =>
        new MatchCriterionEvidence(
            name,
            Math.Abs(actual - expected) <= tolerance,
            expected.ToString("R", CultureInfo.InvariantCulture),
            actual.ToString("R", CultureInfo.InvariantCulture),
            tolerance);

    private static bool IsInPageScope(PageScope scope, int pageNumber, int lastPage) =>
        scope == PageScope.Any ||
        scope == PageScope.Repeated ||
        scope == PageScope.First && pageNumber == 1 ||
        scope == PageScope.Last && pageNumber == lastPage;
}
