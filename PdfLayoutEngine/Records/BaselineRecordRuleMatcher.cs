using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordRuleMatcher
{
    public BaselineRecordRuleMatchResult Match(
        BaselineRecord record,
        IEnumerable<RecordRecognitionRuleDefinition> rules)
    {
        if (record == null) throw new ArgumentNullException(nameof(record));
        if (rules == null) throw new ArgumentNullException(nameof(rules));

        var matches = rules
            .Select(rule => Evaluate(rule, record))
            .Where(evidence => evidence.Criteria.All(criterion => criterion.IsMatch))
            .ToArray();
        var status = matches.Length == 0
            ? RuleMatchStatus.Unmatched
            : matches.Length == 1
                ? RuleMatchStatus.Matched
                : RuleMatchStatus.Ambiguous;
        return new BaselineRecordRuleMatchResult(status, matches);
    }

    private static BaselineRecordRecognitionEvidence Evaluate(
        RecordRecognitionRuleDefinition rule,
        BaselineRecord record)
    {
        var criteria = new List<MatchCriterionEvidence>();
        if (rule.SectionId != null)
            criteria.Add(Text("Section", rule.SectionId == record.SectionId, rule.SectionId, record.SectionId));
        if (rule.Text != null)
        {
            var expected = TextNormalizer.Normalize(rule.Text);
            var comparison = rule.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var matched = rule.TextMatch == TextMatchMode.Equals
                ? string.Equals(record.Text, expected, comparison)
                : rule.TextMatch == TextMatchMode.Contains
                    ? record.Text.IndexOf(expected, comparison) >= 0
                    : Regex.IsMatch(record.Text, expected, rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            criteria.Add(Text("Text", matched, expected, record.Text));
        }

        AddMinimum(criteria, "GroupCountMinimum", rule.MinimumGroupCount, record.Groups.Count);
        AddMaximum(criteria, "GroupCountMaximum", rule.MaximumGroupCount, record.Groups.Count);
        AddMinimum(criteria, "TokenCountMinimum", rule.MinimumTokenCount, record.SourceTokens.Count);
        AddMaximum(criteria, "TokenCountMaximum", rule.MaximumTokenCount, record.SourceTokens.Count);
        AddMinimum(criteria, "LeftAtLeast", rule.LeftAtLeast, record.Left);
        AddMaximum(criteria, "LeftAtMost", rule.LeftAtMost, record.Left);
        AddMinimum(criteria, "RightAtLeast", rule.RightAtLeast, record.Right);
        AddMaximum(criteria, "RightAtMost", rule.RightAtMost, record.Right);
        if (rule.CrossesPageBoundary.HasValue)
            criteria.Add(Text(
                "CrossesPageBoundary",
                rule.CrossesPageBoundary.Value == record.CrossesPageBoundary,
                rule.CrossesPageBoundary.Value.ToString(),
                record.CrossesPageBoundary.ToString()));

        return new BaselineRecordRecognitionEvidence(rule.Id, record, criteria);
    }

    private static MatchCriterionEvidence Text(
        string name,
        bool isMatch,
        string expected,
        string actual) =>
        new MatchCriterionEvidence(name, isMatch, expected, actual);

    private static void AddMinimum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        int? expected,
        int actual)
    {
        if (expected.HasValue)
            criteria.Add(Text(name, actual >= expected.Value, expected.Value.ToString(CultureInfo.InvariantCulture), actual.ToString(CultureInfo.InvariantCulture)));
    }

    private static void AddMaximum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        int? expected,
        int actual)
    {
        if (expected.HasValue)
            criteria.Add(Text(name, actual <= expected.Value, expected.Value.ToString(CultureInfo.InvariantCulture), actual.ToString(CultureInfo.InvariantCulture)));
    }

    private static void AddMinimum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        double? expected,
        double actual)
    {
        if (expected.HasValue)
            criteria.Add(Text(name, actual >= expected.Value, Number(expected.Value), Number(actual)));
    }

    private static void AddMaximum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        double? expected,
        double actual)
    {
        if (expected.HasValue)
            criteria.Add(Text(name, actual <= expected.Value, Number(expected.Value), Number(actual)));
    }

    private static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
