using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PdfLayoutEngine.V2;
internal static class GroupProcessor
{
    public static void Apply(JsonElement layout, EngineResult result)
    {
        var ordered = result.Records.OrderBy(r => r.PageIndex).ThenByDescending(r => r.OriginY).ToArray();
        foreach (var group in layout.A("groups"))
        {
            var members = new List<ExtractedRecord>(); int sequence = 0; int previousPage = -1; bool incomplete = true;
            string occurrenceId = group.S("id") + ":" + sequence;
            foreach (var record in ordered)
            {
                if (record.SemanticClass == "pageHeader" || record.SemanticClass == "pageFooter") continue;
                if (previousPage >= 0 && record.PageIndex != previousPage && members.Count > 0)
                {
                    // Adjacent physical or printed page numbers alone do not prove group continuity.
                    Finish(null, false); members.Clear(); occurrenceId = group.S("id") + ":" + ++sequence; incomplete = true;
                    PositionedLayoutEngine.Diagnose(result, "groupContinuityUnresolved", occurrenceId + ": no explicit cross-page group continuity evidence.");
                }
                previousPage = record.PageIndex;
                var end = group.GetProperty("endRule"); bool closes = end.S("op") == "match" && PositionedLayoutEngine.Predicate(end.GetProperty("when"), record);
                if (PositionedLayoutEngine.Select(group.GetProperty("memberSelector"), record)) { members.Add(record); record.GroupOccurrences[group.S("id")] = occurrenceId; }
                if (closes)
                {
                    record.GroupOccurrences[group.S("id")] = occurrenceId; Finish(record, !incomplete); members.Clear(); occurrenceId = group.S("id") + ":" + ++sequence; incomplete = false;
                }
            }
            if (members.Count > 0) Finish(null, false);
            void Finish(ExtractedRecord? closing, bool complete)
            {
                foreach (var rule in group.A("inheritance"))
                {
                    if (closing == null || closing.AmbiguousPlacement || !closing.Fields.TryGetValue(rule.S("fieldRef"), out var source) || source.State != "present") continue;
                    foreach (var member in members.Where(m => m != closing && !m.AmbiguousPlacement))
                    {
                        string name = rule.S("fieldRef"); member.Fields.TryGetValue(name, out var existing);
                        if (existing == null || existing.State == "blank" || existing.State == "unresolved" && existing.RawText.Length == 0)
                        {
                            member.Fields[name] = new FieldValue { State = "present", Value = source.Value, Origin = "inherited", RawText = source.RawText, EvidenceRefs = source.EvidenceRefs.ToList(), DerivationInputs = new List<string> { closing.Id + ":" + name } };
                        }
                        else if (existing.State == "present" && existing.Value != source.Value)
                        {
                            existing.State = "ambiguous"; existing.Alternatives = new[] { existing.Value!, source.Value! }.Distinct().ToList(); existing.Value = null;
                            existing.EvidenceRefs.AddRange(source.EvidenceRefs); PositionedLayoutEngine.Diagnose(result, "inheritanceConflict", member.Id + ":" + name + " conflicts with " + closing.Id);
                        }
                    }
                }
                if (!complete && members.Count > 0) PositionedLayoutEngine.Diagnose(result, "incompleteGroup", occurrenceId + " has an unproven start, end or continuity; totals cannot establish original completeness.", "info");
            }
        }
    }
}
