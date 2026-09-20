using System;
using System.Collections.Generic;
using System.Linq;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Grouping;

public sealed class BaselineGroupBuilder
{
    public IReadOnlyList<BaselineGroup> Build(
        PdfDocument document,
        double baselineTolerance = MatchingDefaults.StandardBaselineTolerance)
    {
        if (document == null) throw new ArgumentNullException(nameof(document));
        return Build(document.Tokens, baselineTolerance);
    }

    public IReadOnlyList<BaselineGroup> Build(
        IEnumerable<PdfTextToken> tokens,
        double baselineTolerance = MatchingDefaults.StandardBaselineTolerance)
    {
        if (tokens == null) throw new ArgumentNullException(nameof(tokens));
        if (double.IsNaN(baselineTolerance) || double.IsInfinity(baselineTolerance) || baselineTolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(baselineTolerance), "Tolerance must be finite and non-negative.");

        var groups = new List<BaselineGroup>();
        foreach (var pageTokens in tokens.GroupBy(token => token.PageNumber).OrderBy(page => page.Key))
        {
            var ordered = pageTokens
                .OrderByDescending(token => token.Baseline)
                .ThenBy(token => token.Left)
                .ToArray();

            var currentTokens = new List<PdfTextToken>();
            double highestBaseline = 0;

            foreach (var token in ordered)
            {
                if (currentTokens.Count == 0)
                {
                    highestBaseline = token.Baseline;
                }
                else if (highestBaseline - token.Baseline > baselineTolerance)
                {
                    groups.Add(new BaselineGroup(pageTokens.Key, currentTokens));
                    currentTokens.Clear();
                    highestBaseline = token.Baseline;
                }

                currentTokens.Add(token);
            }

            if (currentTokens.Count > 0)
                groups.Add(new BaselineGroup(pageTokens.Key, currentTokens));
        }

        return groups.AsReadOnly();
    }
}
