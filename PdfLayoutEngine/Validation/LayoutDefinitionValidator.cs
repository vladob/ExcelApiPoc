using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;

namespace PdfLayoutEngine.Validation;

public sealed class LayoutDefinitionValidator
{
    public IReadOnlyList<ValidationMessage> Validate(LayoutDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        var messages = new List<ValidationMessage>();

        if (definition.SchemaVersion != LayoutDefinition.CurrentSchemaVersion)
            Error("$.schemaVersion", $"Unsupported schema version {definition.SchemaVersion}; expected {LayoutDefinition.CurrentSchemaVersion}.");
        Required(definition.Id, "$.id");
        PositiveFinite(definition.Defaults?.HorizontalTolerance, "$.defaults.horizontalTolerance");
        PositiveFinite(definition.Defaults?.BaselineTolerance, "$.defaults.baselineTolerance");

        var sections = definition.Sections ?? new List<SectionDefinition>();
        var rules = definition.Rules ?? new List<RecognitionRuleDefinition>();
        DuplicateIds(sections.Select(section => section.Id), "$.sections", "section");
        DuplicateIds(rules.Select(rule => rule.Id), "$.rules", "rule");

        var sectionIds = new HashSet<string>(sections.Select(section => section.Id), StringComparer.Ordinal);
        var ruleIds = new HashSet<string>(rules.Select(rule => rule.Id), StringComparer.Ordinal);
        for (var index = 0; index < sections.Count; index++)
        {
            var section = sections[index];
            Required(section.Id, $"$.sections[{index}].id");
            Reference(section.StartRuleId, ruleIds, $"$.sections[{index}].startRuleId", "rule");
            Reference(section.EndRuleId, ruleIds, $"$.sections[{index}].endRuleId", "rule");
        }

        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            var path = $"$.rules[{index}]";
            Required(rule.Id, path + ".id");
            Reference(rule.SectionId, sectionIds, path + ".sectionId", "section");
            if (rule.Text == string.Empty) Error(path + ".text", "Text cannot be empty when specified.");
            if (rule.Horizontal != null)
            {
                OptionalFinite(rule.Horizontal.Position, path + ".horizontal.position");
                OptionalPositiveFinite(rule.Horizontal.Tolerance, path + ".horizontal.tolerance");
            }
            OptionalFinite(rule.Baseline, path + ".baseline");
            OptionalPositiveFinite(rule.BaselineTolerance, path + ".baselineTolerance");
            if (rule.Text == null && rule.Horizontal == null && !rule.Baseline.HasValue && !rule.IsBold.HasValue && !rule.IsItalic.HasValue)
                Error(path, "At least one matching criterion is required.");
        }

        return messages;

        void Error(string path, string message) => messages.Add(new ValidationMessage(ValidationSeverity.Error, path, message));
        void Required(string? value, string path)
        {
            if (string.IsNullOrWhiteSpace(value)) Error(path, "A value is required.");
        }
        void PositiveFinite(double? value, string path)
        {
            if (!value.HasValue || !IsFinite(value.Value) || value.Value < 0) Error(path, "A finite, non-negative value is required.");
        }
        void OptionalFinite(double? value, string path)
        {
            if (value.HasValue && !IsFinite(value.Value)) Error(path, "The value must be finite.");
        }
        void OptionalPositiveFinite(double? value, string path)
        {
            if (value.HasValue && (!IsFinite(value.Value) || value.Value < 0)) Error(path, "The value must be finite and non-negative.");
        }
        void Reference(string? value, HashSet<string> ids, string path, string kind)
        {
            if (!string.IsNullOrWhiteSpace(value) && !ids.Contains(value)) Error(path, $"Unknown {kind} id '{value}'.");
        }
        void DuplicateIds(IEnumerable<string> ids, string path, string kind)
        {
            foreach (var id in ids.Where(id => !string.IsNullOrWhiteSpace(id)).GroupBy(id => id, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key))
                Error(path, $"Duplicate {kind} id '{id}'.");
        }
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
