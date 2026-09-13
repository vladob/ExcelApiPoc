using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
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
        var diagnostics = resultArray.SelectMany(result => result.Diagnostics).ToArray();
        var candidates = resultArray
            .SelectMany(result => result.Sections)
            .OrderBy(section => section.StartPageNumber)
            .ThenBy(section => section.EndPageNumber)
            .SelectMany(section => Groups(section, baselineTolerance))
            .ToArray();
        var records = new List<BaselineRecord>();
        var currentGroups = new List<BaselineGroup>();
        Candidate? previous = null;

        foreach (var candidate in candidates)
        {
            var pageDifference = previous == null
                ? 0
                : candidate.Group.PageNumber - previous.Group.PageNumber;
            var canContinue = previous != null &&
                previous.Section.SectionId == candidate.Section.SectionId &&
                pageDifference >= 0 &&
                pageDifference <= 1 &&
                (pageDifference == 0 || previous.Section.MayContinueOnNextPage) &&
                continuationPolicy != null &&
                continuationPolicy.ContinuesRecord(
                    new BaselineRecord(previous.Section.SectionId, currentGroups),
                    candidate.Group);

            if (!canContinue && currentGroups.Count > 0)
            {
                records.Add(new BaselineRecord(previous!.Section.SectionId, currentGroups));
                currentGroups.Clear();
            }

            currentGroups.Add(candidate.Group);
            previous = candidate;
        }

        if (currentGroups.Count > 0)
            records.Add(new BaselineRecord(previous!.Section.SectionId, currentGroups));

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
