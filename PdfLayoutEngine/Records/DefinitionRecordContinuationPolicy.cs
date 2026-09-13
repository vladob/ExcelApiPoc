using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Records;

public sealed class DefinitionRecordContinuationPolicy : IBaselineRecordContinuationPolicy
{
    private readonly IReadOnlyList<RecordContinuationDefinition> _definitions;

    public DefinitionRecordContinuationPolicy(IEnumerable<RecordContinuationDefinition> definitions)
    {
        _definitions = Array.AsReadOnly(
            definitions?.ToArray() ?? throw new ArgumentNullException(nameof(definitions)));
    }

    public RecordContinuationDecision Evaluate(BaselineRecord currentRecord, BaselineGroup nextGroup)
    {
        if (currentRecord == null) throw new ArgumentNullException(nameof(currentRecord));
        if (nextGroup == null) throw new ArgumentNullException(nameof(nextGroup));

        var previousGroup = currentRecord.Groups[currentRecord.Groups.Count - 1];
        var matches = _definitions
            .Where(definition => definition.SectionId == currentRecord.SectionId)
            .Select(definition => Evaluate(definition, previousGroup, nextGroup))
            .Where(evidence => evidence.Criteria.All(criterion => criterion.IsMatch))
            .ToArray();
        var status = matches.Length == 0
            ? RecordContinuationDecisionStatus.NotContinued
            : matches.Length == 1
                ? RecordContinuationDecisionStatus.Continued
                : RecordContinuationDecisionStatus.Ambiguous;
        return new RecordContinuationDecision(status, matches);
    }

    private static RecordContinuationEvidence Evaluate(
        RecordContinuationDefinition definition,
        BaselineGroup previous,
        BaselineGroup next)
    {
        var criteria = new List<MatchCriterionEvidence>();
        if (definition.AcrossPageBoundaryOnly)
        {
            var difference = next.PageNumber - previous.PageNumber;
            criteria.Add(new MatchCriterionEvidence(
                "AdjacentPageBoundary",
                difference == 1,
                "1",
                difference.ToString(CultureInfo.InvariantCulture)));
        }

        AddConditions(criteria, "Previous", definition.Previous, previous);
        AddConditions(criteria, "Next", definition.Next, next);
        return new RecordContinuationEvidence(definition.Id, previous, next, criteria);
    }

    private static void AddConditions(
        ICollection<MatchCriterionEvidence> criteria,
        string prefix,
        BaselineGroupConditionDefinition conditions,
        BaselineGroup group)
    {
        AddMinimum(criteria, prefix + "LeftAtLeast", conditions.LeftAtLeast, group.Left);
        AddMaximum(criteria, prefix + "LeftAtMost", conditions.LeftAtMost, group.Left);
        AddMinimum(criteria, prefix + "RightAtLeast", conditions.RightAtLeast, group.Right);
        AddMaximum(criteria, prefix + "RightAtMost", conditions.RightAtMost, group.Right);
    }

    private static void AddMinimum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        double? expected,
        double actual)
    {
        if (expected.HasValue)
            criteria.Add(Criterion(name, actual >= expected.Value, expected.Value, actual));
    }

    private static void AddMaximum(
        ICollection<MatchCriterionEvidence> criteria,
        string name,
        double? expected,
        double actual)
    {
        if (expected.HasValue)
            criteria.Add(Criterion(name, actual <= expected.Value, expected.Value, actual));
    }

    private static MatchCriterionEvidence Criterion(
        string name,
        bool isMatch,
        double expected,
        double actual) =>
        new MatchCriterionEvidence(
            name,
            isMatch,
            expected.ToString("R", CultureInfo.InvariantCulture),
            actual.ToString("R", CultureInfo.InvariantCulture));
}
