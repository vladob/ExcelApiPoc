using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Diagnostics;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Recognition;

namespace PdfLayoutEngine.Sections;

public sealed class SectionDiscoverer
{
    private readonly TokenRuleMatcher _matcher = new TokenRuleMatcher();

    public SectionDiscoveryResult Discover(
        SectionDefinition section,
        IEnumerable<RecognitionRuleDefinition> rules,
        PdfDocument document,
        MatchingDefaults? defaults = null)
    {
        if (section == null) throw new ArgumentNullException(nameof(section));
        if (rules == null) throw new ArgumentNullException(nameof(rules));
        if (document == null) throw new ArgumentNullException(nameof(document));

        var rulesById = rules.ToDictionary(rule => rule.Id, StringComparer.Ordinal);
        var startRule = ResolveRule(section.StartRuleId, rulesById);
        var endRule = ResolveRule(section.EndRuleId, rulesById);
        var startMatches = Match(startRule, document, defaults);
        var endMatches = Match(endRule, document, defaults);

        return section.Scope == SectionScope.PerPage
            ? DiscoverPerPage(section, document, startRule, startMatches, endRule, endMatches)
            : DiscoverDocument(section, document, startRule, startMatches, endRule, endMatches);
    }

    private static RecognitionRuleDefinition? ResolveRule(
        string? ruleId,
        IReadOnlyDictionary<string, RecognitionRuleDefinition> rules) =>
        ruleId == null
            ? null
            : rules.TryGetValue(ruleId, out var rule)
                ? rule
                : throw new ArgumentException($"Unknown boundary rule id '{ruleId}'.", nameof(rules));

    private RuleMatchResult? Match(
        RecognitionRuleDefinition? rule,
        PdfDocument document,
        MatchingDefaults? defaults) =>
        rule == null ? null : _matcher.Match(rule, document.Tokens, defaults);

    private static SectionDiscoveryResult DiscoverDocument(
        SectionDefinition definition,
        PdfDocument document,
        RecognitionRuleDefinition? startRule,
        RuleMatchResult? startResult,
        RecognitionRuleDefinition? endRule,
        RuleMatchResult? endResult)
    {
        var diagnostics = new List<RecognitionDiagnostic>();
        var start = SingleBoundary(definition.Id, startRule, startResult, null, diagnostics);
        var end = SingleBoundary(definition.Id, endRule, endResult, null, diagnostics);
        if (diagnostics.Count > 0)
            return Result(definition.Id, Array.Empty<DiscoveredSection>(), diagnostics);

        var ordered = Order(document.Tokens);
        if (!TrySlice(ordered, start, end, out var tokens))
        {
            diagnostics.Add(InvalidOrder(definition.Id, start!, end!));
            return Result(definition.Id, Array.Empty<DiscoveredSection>(), diagnostics);
        }

        if (tokens.Length == 0)
            return new SectionDiscoveryResult(definition.Id, SectionDiscoveryStatus.Unmatched, Array.Empty<DiscoveredSection>(), diagnostics);

        var instance = new DiscoveredSection(
            definition.Id,
            tokens[0].PageNumber,
            tokens[tokens.Length - 1].PageNumber,
            tokens,
            start,
            end,
            definition.MayContinueOnNextPage);
        return new SectionDiscoveryResult(definition.Id, SectionDiscoveryStatus.Discovered, new[] { instance }, diagnostics);
    }

    private static SectionDiscoveryResult DiscoverPerPage(
        SectionDefinition definition,
        PdfDocument document,
        RecognitionRuleDefinition? startRule,
        RuleMatchResult? startResult,
        RecognitionRuleDefinition? endRule,
        RuleMatchResult? endResult)
    {
        var diagnostics = new List<RecognitionDiagnostic>();
        var sections = new List<DiscoveredSection>();
        foreach (var page in document.Pages)
        {
            var start = SingleBoundary(definition.Id, startRule, startResult, page.PageNumber, diagnostics);
            var end = SingleBoundary(definition.Id, endRule, endResult, page.PageNumber, diagnostics);
            if ((startRule != null && start == null) || (endRule != null && end == null)) continue;

            var ordered = Order(page.Tokens);
            if (!TrySlice(ordered, start, end, out var tokens))
            {
                diagnostics.Add(InvalidOrder(definition.Id, start!, end!));
                continue;
            }
            if (tokens.Length > 0)
                sections.Add(new DiscoveredSection(
                    definition.Id,
                    page.PageNumber,
                    page.PageNumber,
                    tokens,
                    start,
                    end,
                    definition.MayContinueOnNextPage));
        }

        return Result(definition.Id, sections, diagnostics);
    }

    private static RecognitionEvidence? SingleBoundary(
        string sectionId,
        RecognitionRuleDefinition? rule,
        RuleMatchResult? result,
        int? pageNumber,
        ICollection<RecognitionDiagnostic> diagnostics)
    {
        if (rule == null) return null;
        var matches = result!.Matches
            .Where(match => !pageNumber.HasValue || match.SourceTokens[0].PageNumber == pageNumber.Value)
            .ToArray();
        var location = pageNumber.HasValue ? $" on page {pageNumber.Value}" : string.Empty;
        if (matches.Length == 1) return matches[0];

        var kind = matches.Length == 0
            ? RecognitionDiagnosticKind.UnmatchedSectionBoundary
            : RecognitionDiagnosticKind.AmbiguousSectionBoundary;
        diagnostics.Add(new RecognitionDiagnostic(
            kind,
            rule.Id,
            matches.Length == 0
                ? $"Section '{sectionId}' boundary rule '{rule.Id}' did not match{location}."
                : $"Section '{sectionId}' boundary rule '{rule.Id}' matched {matches.Length} tokens{location}.",
            matches.SelectMany(match => match.SourceTokens)));
        return null;
    }

    private static bool TrySlice(
        PdfTextToken[] ordered,
        RecognitionEvidence? start,
        RecognitionEvidence? end,
        out PdfTextToken[] tokens)
    {
        var startIndex = start == null ? 0 : Array.IndexOf(ordered, start.SourceTokens[0]);
        var endIndex = end == null ? ordered.Length - 1 : Array.IndexOf(ordered, end.SourceTokens[0]);
        if (startIndex < 0 || endIndex < startIndex)
        {
            tokens = Array.Empty<PdfTextToken>();
            return false;
        }

        tokens = ordered.Skip(startIndex).Take(endIndex - startIndex + 1).ToArray();
        return true;
    }

    private static PdfTextToken[] Order(IEnumerable<PdfTextToken> tokens) =>
        tokens.OrderBy(token => token.PageNumber)
            .ThenByDescending(token => token.Baseline)
            .ThenBy(token => token.Left)
            .ToArray();

    private static RecognitionDiagnostic InvalidOrder(
        string sectionId,
        RecognitionEvidence start,
        RecognitionEvidence end) =>
        new RecognitionDiagnostic(
            RecognitionDiagnosticKind.InvalidSectionBoundaryOrder,
            sectionId,
            $"Section '{sectionId}' ends before it starts.",
            start.SourceTokens.Concat(end.SourceTokens));

    private static SectionDiscoveryResult Result(
        string sectionId,
        IEnumerable<DiscoveredSection> sections,
        IReadOnlyCollection<RecognitionDiagnostic> diagnostics)
    {
        var status = diagnostics.Any(item => item.Kind == RecognitionDiagnosticKind.AmbiguousSectionBoundary || item.Kind == RecognitionDiagnosticKind.InvalidSectionBoundaryOrder)
            ? SectionDiscoveryStatus.Ambiguous
            : diagnostics.Any(item => item.Kind == RecognitionDiagnosticKind.UnmatchedSectionBoundary)
                ? SectionDiscoveryStatus.Unmatched
                : SectionDiscoveryStatus.Discovered;
        return new SectionDiscoveryResult(sectionId, status, sections, diagnostics);
    }
}
