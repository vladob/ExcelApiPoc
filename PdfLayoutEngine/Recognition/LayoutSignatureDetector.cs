using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Recognition;

/// <summary>
/// Requires every signature rule of a layout to match. Returns no match or
/// multiple matches explicitly; it never guesses from a filename.
/// </summary>
public sealed class LayoutSignatureDetector
{
    private readonly TokenRuleMatcher _matcher = new TokenRuleMatcher();

    public IReadOnlyList<LayoutDefinition> Detect(
        PdfDocument document,
        IEnumerable<LayoutDefinition> definitions)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        if (definitions == null) throw new ArgumentNullException(nameof(definitions));
        return definitions.Where(definition => definition.Rules.Count > 0 &&
            definition.Rules.All(rule =>
            {
                var match = _matcher.Match(rule, document.Tokens, definition.Defaults);
                return match.Status == RuleMatchStatus.Matched;
            })).ToArray();
    }
}
