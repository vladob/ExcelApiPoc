using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.Sections;

namespace PdfLayoutEngine.Recognition;

public sealed class LayoutRecognizer
{
    private readonly SectionDiscoverer _sectionDiscoverer = new SectionDiscoverer();
    private readonly BaselineRecordDiscoverer _recordDiscoverer = new BaselineRecordDiscoverer();
    private readonly BaselineRecordRuleMatcher _recordMatcher = new BaselineRecordRuleMatcher();
    private readonly BaselineRecordFieldMatcher _fieldMatcher = new BaselineRecordFieldMatcher();

    public LayoutRecognitionResult Recognize(
        PdfDocument document,
        LayoutDefinition layout)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (layout == null) throw new ArgumentNullException(nameof(layout));

        var sections = layout.Sections
            .Select(section => _sectionDiscoverer.Discover(
                section,
                layout.Rules,
                document,
                layout.Defaults))
            .ToArray();
        var continuationPolicy = layout.RecordContinuations.Count == 0
            ? null
            : new DefinitionRecordContinuationPolicy(layout.RecordContinuations);
        var recordDiscovery = _recordDiscoverer.Discover(
            sections,
            continuationPolicy,
            layout.Defaults.BaselineTolerance);
        var fieldSets = layout.RecordFields.ToDictionary(
            fieldSet => fieldSet.RecordRuleId,
            StringComparer.Ordinal);
        var records = recordDiscovery.Records
            .Select(record => Recognize(record, layout, fieldSets))
            .ToArray();

        return new LayoutRecognitionResult(
            document,
            sections,
            recordDiscovery,
            records);
    }

    private RecognizedRecord Recognize(
        BaselineRecord record,
        LayoutDefinition layout,
        IReadOnlyDictionary<string, RecordFieldSetDefinition> fieldSets)
    {
        var classification = _recordMatcher.Match(record, layout.RecordRules);
        if (classification.Status != RuleMatchStatus.Matched)
            return new RecognizedRecord(
                record,
                classification,
                Array.Empty<RecordFieldMatchResult>());

        var ruleId = classification.Matches[0].RuleId;
        var fields = fieldSets.TryGetValue(ruleId, out var fieldSet)
            ? _fieldMatcher.Match(record, fieldSet, layout.Defaults)
            : Array.Empty<RecordFieldMatchResult>();
        return new RecognizedRecord(record, classification, fields);
    }
}
