using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Records;

public sealed class BaselineRecordDiscoverer
{
    private readonly BaselineGroupBuilder _groupBuilder = new BaselineGroupBuilder();

    public BaselineRecordDiscoveryResult Discover(
        IEnumerable<SectionDiscoveryResult> sectionResults,
        IBaselineRecordContinuationPolicy? continuationPolicy = null,
        double baselineTolerance = MatchingDefaults.StandardBaselineTolerance)
    {
        if (sectionResults == null) throw new ArgumentNullException(nameof(sectionResults));

        var resultArray = sectionResults.ToArray();
        var diagnostics = resultArray.SelectMany(result => result.Diagnostics).ToList();
        var candidates = resultArray
            .SelectMany(result => result.Sections)
            .OrderBy(section => section.StartPageNumber)
            .ThenBy(section => section.EndPageNumber)
            .SelectMany(section => Groups(section, baselineTolerance))
            .ToArray();
        var records = new List<BaselineRecord>();
        var currentGroups = new List<BaselineGroup>();
        var currentContinuations = new List<RecordContinuationEvidence>();
        Candidate? previous = null;

        foreach (var candidate in candidates)
        {
            var pageDifference = previous == null
                ? 0
                : candidate.Group.PageNumber - previous.Group.PageNumber;
            var mayEvaluate = previous != null &&
                previous.Section.SectionId == candidate.Section.SectionId &&
                pageDifference >= 0 &&
                pageDifference <= 1 &&
                (pageDifference == 0 || previous.Section.MayContinueOnNextPage) &&
                continuationPolicy != null;
            var decision = mayEvaluate
                ? continuationPolicy!.Evaluate(
                    new BaselineRecord(previous!.Section.SectionId, currentGroups, currentContinuations),
                    candidate.Group)
                : null;
            var canContinue = decision?.Status == RecordContinuationDecisionStatus.Continued;

            if (decision?.Status == RecordContinuationDecisionStatus.Ambiguous)
            {
                diagnostics.Add(new RecognitionDiagnostic(
                    RecognitionDiagnosticKind.AmbiguousRecordContinuation,
                    previous!.Section.SectionId,
                    $"{decision.Matches.Count} record-continuation definitions matched between pages " +
                    $"{previous.Group.PageNumber} and {candidate.Group.PageNumber}.",
                    previous.Group.Tokens.Concat(candidate.Group.Tokens)));
            }

            if (!canContinue && currentGroups.Count > 0)
            {
                records.Add(new BaselineRecord(previous!.Section.SectionId, currentGroups, currentContinuations));
                currentGroups.Clear();
                currentContinuations.Clear();
            }

            if (canContinue)
                currentContinuations.Add(decision!.Matches[0]);
            currentGroups.Add(candidate.Group);
            previous = candidate;
        }

        if (currentGroups.Count > 0)
            records.Add(new BaselineRecord(previous!.Section.SectionId, currentGroups, currentContinuations));

        return new BaselineRecordDiscoveryResult(records, diagnostics);
    }

    private IEnumerable<Candidate> Groups(DiscoveredSection section, double baselineTolerance)
    {
        var boundaryTokens = new HashSet<PdfTextToken>(
            (section.StartEvidence?.SourceTokens ?? Array.Empty<PdfTextToken>())
                .Concat(section.EndEvidence?.SourceTokens ?? Array.Empty<PdfTextToken>()));

        return _groupBuilder.Build(section.Tokens, baselineTolerance)
            .Where(group => !group.Tokens.Any(boundaryTokens.Contains))
            .Select(group => new Candidate(section, group));
    }

    private sealed class Candidate
    {
        public Candidate(DiscoveredSection section, BaselineGroup group)
        {
            Section = section;
            Group = group;
        }

        public DiscoveredSection Section { get; }
        public BaselineGroup Group { get; }
    }
}
