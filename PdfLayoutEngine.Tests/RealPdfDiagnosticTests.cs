using PdfLayoutEngine.IText.Extraction;
using Xunit.Abstractions;
using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.Recognition;
using PdfLayoutEngine.Sections;
using PdfLayoutEngine.Records;
using PdfLayoutEngine.IText.Recognition;

namespace PdfLayoutEngine.Tests;

public sealed class RealPdfDiagnosticTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void Extract_real_general_ledger()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var extractor = new ITextPdfTokenExtractor();
        var document = extractor.Extract(pdfPath);

        _output.WriteLine(
            $"Pages: {document.Pages.Count}");

        _output.WriteLine(
            $"Tokens: {document.Tokens.Count}");

        foreach (var page in document.Pages)
        {
            _output.WriteLine(
                $"Page {page.PageNumber}: {page.Tokens.Count} tokens");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "Page\tText\tLeft\tRight\tBaseline\tBold\tItalic");

        foreach (var token in document.Pages[0].Tokens)
        {
            _output.WriteLine(
                $"{token.PageNumber}\t" +
                $"{token.Text}\t" +
                $"{token.Left:F3}\t" +
                $"{token.Right:F3}\t" +
                $"{token.Baseline:F3}\t" +
                $"{token.IsBold}\t" +
                $"{token.IsItalic}");
        }

        Assert.Equal(13, document.Pages.Count);
        Assert.All(
            document.Pages,
            page => Assert.NotEmpty(page.Tokens));

        Assert.Contains(
            document.Tokens,
            token => token.Left != Math.Truncate(token.Left));
    }

    [Fact]
    public void Group_real_general_ledger_by_baseline()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);
        var groups = new BaselineGroupBuilder().Build(document);

        _output.WriteLine($"Pages: {document.Pages.Count}");
        _output.WriteLine($"Tokens: {document.Tokens.Count}");
        _output.WriteLine($"Baseline groups: {groups.Count}");
        _output.WriteLine("");

        foreach (var page in document.Pages)
        {
            var pageGroups = groups
                .Where(group => group.PageNumber == page.PageNumber)
                .ToArray();

            _output.WriteLine(
                $"Page {page.PageNumber}: " +
                $"{page.Tokens.Count} tokens, " +
                $"{pageGroups.Length} groups");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "Page\tBaseline\tLeft\tRight\tTokens\tText");

        foreach (var group in groups.Where(group => group.PageNumber == 1))
        {
            _output.WriteLine(
                $"{group.PageNumber}\t" +
                $"{group.Baseline:F3}\t" +
                $"{group.Left:F3}\t" +
                $"{group.Right:F3}\t" +
                $"{group.Tokens.Count}\t" +
                $"{group.Text}");
        }

        var tableHeaderGroup = Assert.Single(groups, group =>
                group.PageNumber == 1 &&
                group.Text.Contains("Účet") &&
                group.Text.Contains("Názov účtu") &&
                group.Text.Contains("Počiatočný zostatok"));

        Assert.Contains(
            tableHeaderGroup.Tokens,
            token => token.Text == "Účet");

        Assert.Contains(
            tableHeaderGroup.Tokens,
            token => token.Text == "Názov účtu");

        Assert.InRange(
            tableHeaderGroup.Baseline,
            519.8,
            520.6);

        Assert.All(
            groups,
            group => Assert.All(
                group.Tokens,
                token => Assert.Equal(
                    group.PageNumber,
                    token.PageNumber)));

        Assert.All(
            groups,
            group =>
            {
                var highest = group.Tokens.Max(
                    token => token.Baseline);

                var lowest = group.Tokens.Min(
                    token => token.Baseline);

                Assert.True(
                    highest - lowest <= 4,
                    $"Group on page {group.PageNumber} " +
                    $"has baseline span {highest - lowest:F3}.");
            });
    }

    [Fact]
    public void Match_real_general_ledger_tokens()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);
        var matcher = new TokenRuleMatcher();

        var reportTitleRule = new RecognitionRuleDefinition
        {
            Id = "report-title",
            Text = "Hlavná kniha k účtovnému mesiacu",
            TextMatch = TextMatchMode.Contains,
            IsBold = true,
            PageScope = PageScope.First
        };

        var repeatedTitleRule = new RecognitionRuleDefinition
        {
            Id = "repeated-report-title",
            Text = "Hlavná kniha k účtovnému mesiacu",
            TextMatch = TextMatchMode.Contains,
            IsBold = true,
            PageScope = PageScope.Repeated
        };

        var repeatedTitles = matcher.Match(
            repeatedTitleRule,
            document.Tokens);

        _output.WriteLine(
            $"Repeated titles: {repeatedTitles.Status}, " +
            $"matches: {repeatedTitles.Matches.Count}");

        Assert.Equal(
            RuleMatchStatus.Matched,
            repeatedTitles.Status);

        Assert.Equal(
            document.Pages.Count,
            repeatedTitles.Matches.Count);

        var repeatedTitlePages = repeatedTitles.Matches
            .SelectMany(match => match.SourceTokens)
            .Select(token => token.PageNumber)
            .OrderBy(page => page)
            .ToArray();

        Assert.Equal(
            Enumerable.Range(1, document.Pages.Count),
            repeatedTitlePages);

        var reportTitle = matcher.Match(
            reportTitleRule,
            document.Tokens);

        _output.WriteLine(
            $"Report title: {reportTitle.Status}, " +
            $"matches: {reportTitle.Matches.Count}");

        Assert.Equal(
            RuleMatchStatus.Matched,
            reportTitle.Status);

        var reportTitleEvidence = Assert.Single(
            reportTitle.Matches);

        var reportTitleToken = Assert.Single(
            reportTitleEvidence.SourceTokens);

        _output.WriteLine(
            $"  Page {reportTitleToken.PageNumber}, " +
            $"baseline {reportTitleToken.Baseline:F3}, " +
            $"text: {reportTitleToken.Text}");

        Assert.Equal(1, reportTitleToken.PageNumber);
        Assert.True(reportTitleToken.IsBold);
        Assert.All(
            reportTitleEvidence.Criteria,
            criterion => Assert.True(criterion.IsMatch));

        var missingRule = new RecognitionRuleDefinition
        {
            Id = "missing-text",
            Text = "This text does not exist in the PDF"
        };

        var missing = matcher.Match(
            missingRule,
            document.Tokens);

        _output.WriteLine(
            $"Missing text: {missing.Status}, " +
            $"matches: {missing.Matches.Count}");

        Assert.Equal(
            RuleMatchStatus.Unmatched,
            missing.Status);

        Assert.Empty(missing.Matches);

        var zeroRule = new RecognitionRuleDefinition
        {
            Id = "zero-value",
            Text = @"^\s*0,00\s*$",
            TextMatch = TextMatchMode.RegularExpression
        };

        var zeroValues = matcher.Match(
            zeroRule,
            document.Tokens);

        _output.WriteLine(
            $"Zero value: {zeroValues.Status}, " +
            $"matches: {zeroValues.Matches.Count}");

        Assert.Equal(
            RuleMatchStatus.Ambiguous,
            zeroValues.Status);

        Assert.True(zeroValues.Matches.Count > 1);

        var closingValuesRule = new RecognitionRuleDefinition
        {
            Id = "closing-value",
            Text = @"^\s*-?\s*\d[\d ]*,\d{2}\s*$",
            TextMatch = TextMatchMode.RegularExpression,
            Horizontal = new HorizontalMatchDefinition
            {
                Anchor = HorizontalAnchor.Right,
                Position = 770,
                Tolerance = 5
            },
            AllowMultipleMatches = true
        };

        var closingValues = matcher.Match(
            closingValuesRule,
            document.Tokens);

        _output.WriteLine(
            $"Closing values: {closingValues.Status}, " +
            $"matches: {closingValues.Matches.Count}");

        Assert.Equal(
            RuleMatchStatus.Matched,
            closingValues.Status);

        Assert.True(closingValues.Matches.Count > 1);

        var pagesWithClosingValues = closingValues.Matches
            .SelectMany(match => match.SourceTokens)
            .Select(token => token.PageNumber)
            .Distinct()
            .OrderBy(page => page)
            .ToArray();

        _output.WriteLine(
            "Pages with closing values: " +
            string.Join(", ", pagesWithClosingValues));

        Assert.True(
            pagesWithClosingValues.Length > 1,
            "Expected closing-column values on multiple pages.");

        foreach (var evidence in closingValues.Matches)
        {
            var token = Assert.Single(evidence.SourceTokens);

            Assert.InRange(
                token.Right,
                765,
                775);

            Assert.All(
                evidence.Criteria,
                criterion => Assert.True(criterion.IsMatch));
        }

        _output.WriteLine("");
        _output.WriteLine(
            "First ten closing-column matches:");

        foreach (var evidence in closingValues.Matches.Take(10))
        {
            var token = evidence.SourceTokens[0];

            _output.WriteLine(
                $"Page {token.PageNumber}, " +
                $"baseline {token.Baseline:F3}, " +
                $"right {token.Right:F3}, " +
                $"text: {token.Text}");
        }
    }

    [Fact]
    public void Discover_real_general_ledger_sections()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);
        var discoverer = new SectionDiscoverer();

        var rules = new[]
        {
        new RecognitionRuleDefinition
        {
            Id = "header-start",
            Text = "Hlavná kniha k účtovnému mesiacu",
            TextMatch = TextMatchMode.Contains,
            IsBold = true,
            PageScope = PageScope.Repeated
        },
        new RecognitionRuleDefinition
        {
            Id = "header-end",
            Text = @"^\s*Dal\s*$",
            TextMatch = TextMatchMode.RegularExpression,
            Horizontal = new HorizontalMatchDefinition
            {
                Anchor = HorizontalAnchor.Right,
                Position = 661.5,
                Tolerance = 5
            },
            PageScope = PageScope.Repeated
        },
        new RecognitionRuleDefinition
        {
            Id = "footer-start",
            Text = "© Softip a.s.",
            TextMatch = TextMatchMode.Contains,
            PageScope = PageScope.Repeated
        },
        new RecognitionRuleDefinition
        {
            Id = "footer-end",
            Text = "Autor:",
            TextMatch = TextMatchMode.Contains,
            PageScope = PageScope.Repeated
        },
        new RecognitionRuleDefinition
        {
            Id = "document-start",
            Text = "Názov účtu",
            PageScope = PageScope.First
        },
        new RecognitionRuleDefinition
        {
            Id = "document-end",
            Text = "© Softip a.s.",
            TextMatch = TextMatchMode.Contains,
            PageScope = PageScope.Last
        }
    };

        var repeatedHeader = discoverer.Discover(
            new SectionDefinition
            {
                Id = "page-header",
                StartRuleId = "header-start",
                EndRuleId = "header-end",
                Scope = SectionScope.PerPage
            },
            rules,
            document);

        var repeatedFooter = discoverer.Discover(
            new SectionDefinition
            {
                Id = "page-footer",
                StartRuleId = "footer-start",
                EndRuleId = "footer-end",
                Scope = SectionScope.PerPage
            },
            rules,
            document);

        var pageBodies = discoverer.Discover(
            new SectionDefinition
            {
                Id = "page-body",
                StartRuleId = "header-end",
                EndRuleId = "footer-start",
                Scope = SectionScope.PerPage
            },
            rules,
            document);

        var documentContent = discoverer.Discover(
            new SectionDefinition
            {
                Id = "document-content",
                StartRuleId = "document-start",
                EndRuleId = "document-end",
                Scope = SectionScope.Document,
                MayContinueOnNextPage = true
            },
            rules,
            document);

        WriteResult("Repeated headers", repeatedHeader);
        WriteResult("Repeated footers", repeatedFooter);
        WriteResult("Page bodies", pageBodies);
        WriteResult("Document content", documentContent);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            repeatedHeader.Status);

        Assert.Equal(
            document.Pages.Count,
            repeatedHeader.Sections.Count);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            repeatedFooter.Status);

        Assert.Equal(
            document.Pages.Count,
            repeatedFooter.Sections.Count);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            pageBodies.Status);

        Assert.Equal(
            document.Pages.Count,
            pageBodies.Sections.Count);

        var completeDocumentSection = Assert.Single(
            documentContent.Sections);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            documentContent.Status);

        Assert.Equal(1, completeDocumentSection.StartPageNumber);

        Assert.Equal(
            document.Pages.Count,
            completeDocumentSection.EndPageNumber);

        Assert.NotNull(completeDocumentSection.StartEvidence);
        Assert.NotNull(completeDocumentSection.EndEvidence);

        Assert.All(
            repeatedHeader.Sections,
            section =>
            {
                Assert.Equal(
                    section.StartPageNumber,
                    section.EndPageNumber);

                Assert.NotNull(section.StartEvidence);
                Assert.NotNull(section.EndEvidence);
            });

        Assert.All(
            repeatedFooter.Sections,
            section =>
            {
                Assert.Equal(
                    section.StartPageNumber,
                    section.EndPageNumber);

                Assert.NotNull(section.StartEvidence);
                Assert.NotNull(section.EndEvidence);
            });

        Assert.All(
            pageBodies.Sections,
            section =>
            {
                Assert.Equal(
                    section.StartPageNumber,
                    section.EndPageNumber);

                Assert.NotEmpty(section.Tokens);
            });

        void WriteResult(
            string name,
            SectionDiscoveryResult result)
        {
            _output.WriteLine(
                $"{name}: {result.Status}, " +
                $"sections: {result.Sections.Count}, " +
                $"diagnostics: {result.Diagnostics.Count}");

            foreach (var section in result.Sections)
            {
                _output.WriteLine(
                    $"  Pages {section.StartPageNumber}-" +
                    $"{section.EndPageNumber}, " +
                    $"tokens: {section.Tokens.Count}");
            }

            foreach (var diagnostic in result.Diagnostics)
            {
                _output.WriteLine(
                    $"  {diagnostic.Kind}: " +
                    $"{diagnostic.Message}");
            }
        }
    }

    [Fact]
    public void Discover_real_general_ledger_physical_records()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);

        var rules = new[]
        {
        new RecognitionRuleDefinition
        {
            Id = "header-end",
            Text = @"^\s*Dal\s*$",
            TextMatch = TextMatchMode.RegularExpression,
            Horizontal = new HorizontalMatchDefinition
            {
                Anchor = HorizontalAnchor.Right,
                Position = 661.5,
                Tolerance = 5
            },
            PageScope = PageScope.Repeated
        },
        new RecognitionRuleDefinition
        {
            Id = "footer-start",
            Text = "© Softip a.s.",
            TextMatch = TextMatchMode.Contains,
            PageScope = PageScope.Repeated
        }
    };

        var pageBodies = new SectionDiscoverer().Discover(
            new SectionDefinition
            {
                Id = "page-body",
                StartRuleId = "header-end",
                EndRuleId = "footer-start",
                Scope = SectionScope.PerPage,
                MayContinueOnNextPage = true
            },
            rules,
            document);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            pageBodies.Status);

        Assert.Equal(
            document.Pages.Count,
            pageBodies.Sections.Count);

        var result = new BaselineRecordDiscoverer().Discover(
            [pageBodies]);

        _output.WriteLine($"Pages: {document.Pages.Count}");
        _output.WriteLine(
            $"Physical records: {result.Records.Count}");
        _output.WriteLine(
            $"Diagnostics: {result.Diagnostics.Count}");
        _output.WriteLine("");

        Assert.Empty(result.Diagnostics);
        Assert.NotEmpty(result.Records);

        // Without a continuation policy, every physical record must
        // contain exactly one baseline group.
        Assert.All(
            result.Records,
            record => Assert.Single(record.Groups));

        foreach (var page in document.Pages)
        {
            var pageRecords = result.Records
                .Where(record =>
                    record.StartPageNumber == page.PageNumber)
                .ToArray();

            Assert.NotEmpty(pageRecords);

            var first = pageRecords[0];
            var last = pageRecords[^1];

            _output.WriteLine(
                $"Page {page.PageNumber}: " +
                $"{pageRecords.Length} records");

            _output.WriteLine(
                $"  First: baseline " +
                $"{first.Groups[0].Baseline:F3}, " +
                $"tokens {first.SourceTokens.Count}, " +
                $"text: {first.Text}");

            _output.WriteLine(
                $"  Last:  baseline " +
                $"{last.Groups[0].Baseline:F3}, " +
                $"tokens {last.SourceTokens.Count}, " +
                $"text: {last.Text}");
        }

        // The discoverer excludes the complete baseline groups that
        // contain the header-end and footer-start anchor tokens.
        var boundaryTokens = pageBodies.Sections
            .SelectMany(section =>
                (section.StartEvidence?.SourceTokens
                    ?? [])
                .Concat(
                    section.EndEvidence?.SourceTokens
                    ?? []))
            .ToHashSet();

        var recordTokens = result.Records
            .SelectMany(record => record.SourceTokens)
            .ToArray();

        Assert.DoesNotContain(
            recordTokens,
            token => boundaryTokens.Contains(token));

        Assert.DoesNotContain(
            result.Records,
            record => record.Text.Contains("© Softip a.s."));

        _output.WriteLine("");
        _output.WriteLine("Page transitions:");

        for (var pageNumber = 1;
             pageNumber < document.Pages.Count;
             pageNumber++)
        {
            var previousPageLast = result.Records.Last(
                record =>
                    record.EndPageNumber == pageNumber);

            var nextPageFirst = result.Records.First(
                record =>
                    record.StartPageNumber == pageNumber + 1);

            _output.WriteLine(
                $"Page {pageNumber} -> {pageNumber + 1}");

            _output.WriteLine(
                $"  Previous last: " +
                $"baseline {previousPageLast.Groups[^1].Baseline:F3}, " +
                $"left {previousPageLast.Groups[^1].Left:F3}, " +
                $"right {previousPageLast.Groups[^1].Right:F3}, " +
                $"tokens {previousPageLast.SourceTokens.Count}, " +
                $"text: {previousPageLast.Text}");

            _output.WriteLine(
                $"  Next first:    " +
                $"baseline {nextPageFirst.Groups[0].Baseline:F3}, " +
                $"left {nextPageFirst.Groups[0].Left:F3}, " +
                $"right {nextPageFirst.Groups[0].Right:F3}, " +
                $"tokens {nextPageFirst.SourceTokens.Count}, " +
                $"text: {nextPageFirst.Text}");

            if (pageNumber == 8)
            {
                _output.WriteLine("  Previous tokens:");

                foreach (var token in previousPageLast.SourceTokens)
                {
                    _output.WriteLine(
                        $"    left {token.Left:F3}, " +
                        $"right {token.Right:F3}, " +
                        $"text: {token.Text}");
                }

                _output.WriteLine("  Next tokens:");

                foreach (var token in nextPageFirst.SourceTokens)
                {
                    _output.WriteLine(
                        $"    left {token.Left:F3}, " +
                        $"right {token.Right:F3}, " +
                        $"text: {token.Text}");
                }
            }
        }

        var continuationDefinition =
            new RecordContinuationDefinition
            {
                Id = "values-continued-on-next-page",
                SectionId = "page-body",
                AcrossPageBoundaryOnly = true,
                Previous = new BaselineGroupConditionDefinition
                {
                    RightAtMost = 340
                },
                Next = new BaselineGroupConditionDefinition
                {
                    LeftAtLeast = 340
                }
            };

        var continuationPolicy =
            new DefinitionRecordContinuationPolicy(
                new[] { continuationDefinition });

        var logicalResult =
            new BaselineRecordDiscoverer().Discover(
                new[] { pageBodies },
                continuationPolicy);

        _output.WriteLine("");
        _output.WriteLine(
            $"Physical records: {result.Records.Count}");

        _output.WriteLine(
            $"Logical records: {logicalResult.Records.Count}");

        _output.WriteLine(
            $"Continuation diagnostics: " +
            $"{logicalResult.Diagnostics.Count}");

        Assert.Equal(349, result.Records.Count);
        Assert.Equal(348, logicalResult.Records.Count);
        Assert.Empty(logicalResult.Diagnostics);

        var joinedRecord = Assert.Single(
            logicalResult.Records,
            record => record.CrossesPageBoundary);

        Assert.Equal(8, joinedRecord.StartPageNumber);
        Assert.Equal(9, joinedRecord.EndPageNumber);
        Assert.Equal(2, joinedRecord.Groups.Count);

        var continuationEvidence = Assert.Single(
            joinedRecord.Continuations);

        Assert.Equal(
            "values-continued-on-next-page",
            continuationEvidence.DefinitionId);

        Assert.All(
            continuationEvidence.Criteria,
            criterion => Assert.True(criterion.IsMatch));

        _output.WriteLine("");
        _output.WriteLine(
            $"Joined record: pages " +
            $"{joinedRecord.StartPageNumber}-" +
            $"{joinedRecord.EndPageNumber}");

        _output.WriteLine(
            $"  Text: {joinedRecord.Text}");

        foreach (var criterion in continuationEvidence.Criteria)
        {
            _output.WriteLine(
                $"  {criterion.Criterion}: " +
                $"expected {criterion.Expected}, " +
                $"actual {criterion.Actual}, " +
                $"matched {criterion.IsMatch}");
        }

        var recordMatcher = new BaselineRecordRuleMatcher();

        var continuedRecordRule =
            new RecordRecognitionRuleDefinition
            {
                Id = "continued-wide-record",
                SectionId = "page-body",
                MinimumGroupCount = 2,
                MinimumTokenCount = 8,
                RightAtLeast = 760,
                CrossesPageBoundary = true
            };

        var matchedResult = recordMatcher.Match(
            joinedRecord,
            new[] { continuedRecordRule });

        _output.WriteLine("");
        _output.WriteLine(
            $"Joined-record classification: " +
            $"{matchedResult.Status}");

        Assert.Equal(
            RuleMatchStatus.Matched,
            matchedResult.Status);

        var matchedEvidence = Assert.Single(
            matchedResult.Matches);

        Assert.Same(
            joinedRecord,
            matchedEvidence.SourceRecord);

        Assert.Equal(
            2,
            matchedEvidence.SourceGroups.Count);

        Assert.Equal(
            8,
            matchedEvidence.SourceTokens.Count);

        Assert.Single(
            matchedEvidence.SourceRecord.Continuations);

        Assert.All(
            matchedEvidence.Criteria,
            criterion => Assert.True(criterion.IsMatch));

        foreach (var criterion in matchedEvidence.Criteria)
        {
            _output.WriteLine(
                $"  {criterion.Criterion}: " +
                $"expected {criterion.Expected}, " +
                $"actual {criterion.Actual}, " +
                $"matched {criterion.IsMatch}");
        }

        // The final record does not satisfy the continued-record rule.
        var unsupportedRecord = logicalResult.Records[^1];

        var unmatchedResult = recordMatcher.Match(
            unsupportedRecord,
            new[] { continuedRecordRule });

        _output.WriteLine("");
        _output.WriteLine(
            $"Unsupported-record classification: " +
            $"{unmatchedResult.Status}");

        _output.WriteLine(
            $"  Text: {unsupportedRecord.Text}");

        Assert.Equal(
            RuleMatchStatus.Unmatched,
            unmatchedResult.Status);

        Assert.Empty(unmatchedResult.Matches);

        // Deliberately overlapping generic rules must be reported as ambiguous.
        var ordinaryRecord = logicalResult.Records[0];

        var overlappingRules = new[]
        {
    new RecordRecognitionRuleDefinition
    {
        Id = "many-token-record",
        SectionId = "page-body",
        MinimumTokenCount = 8
    },
    new RecordRecognitionRuleDefinition
    {
        Id = "single-wide-group",
        SectionId = "page-body",
        MaximumGroupCount = 1,
        RightAtLeast = 760,
        CrossesPageBoundary = false
    }
};

        var ambiguousResult = recordMatcher.Match(
            ordinaryRecord,
            overlappingRules);

        _output.WriteLine("");
        _output.WriteLine(
            $"Overlapping-rule classification: " +
            $"{ambiguousResult.Status}");

        _output.WriteLine(
            $"  Text: {ordinaryRecord.Text}");

        foreach (var match in ambiguousResult.Matches)
        {
            _output.WriteLine(
                $"  Matching rule: {match.RuleId}");
        }

        Assert.Equal(
            RuleMatchStatus.Ambiguous,
            ambiguousResult.Status);

        Assert.Equal(
            2,
            ambiguousResult.Matches.Count);

        Assert.Equal(
            new[] { "many-token-record", "single-wide-group" },
            ambiguousResult.Matches.Select(match => match.RuleId));

        Assert.All(
            ambiguousResult.Matches,
            match => Assert.Same(
                ordinaryRecord,
                match.SourceRecord));

    }

    [Fact]
    public void Classify_real_general_ledger_from_json_layout()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            "general-ledger.v1.json");

        Assert.True(
            File.Exists(layoutPath),
            $"Layout file does not exist: {layoutPath}");

        LayoutLoadResult loadResult;

        using (var stream = File.OpenRead(layoutPath))
        {
            loadResult =
                new LayoutDefinitionLoader().Load(stream);
        }

        Assert.True(
            loadResult.IsValid,
            string.Join(
                Environment.NewLine,
                loadResult.Messages.Select(
                    message =>
                        $"{message.Path}: {message.Message}")));

        var layout = Assert.IsType<LayoutDefinition>(
            loadResult.Definition);

        var document =
            new ITextPdfTokenExtractor().Extract(pdfPath);

        var bodyDefinition = Assert.Single(
            layout.Sections,
            section => section.Id == "page-body");

        var bodySections =
            new SectionDiscoverer().Discover(
                bodyDefinition,
                layout.Rules,
                document,
                layout.Defaults);

        Assert.Equal(
            SectionDiscoveryStatus.Discovered,
            bodySections.Status);

        Assert.Equal(
            document.Pages.Count,
            bodySections.Sections.Count);

        var continuationPolicy =
            new DefinitionRecordContinuationPolicy(
                layout.RecordContinuations);

        var logicalResult =
            new BaselineRecordDiscoverer().Discover(
                new[] { bodySections },
                continuationPolicy,
                layout.Defaults.BaselineTolerance);

        Assert.Empty(logicalResult.Diagnostics);
        Assert.Equal(348, logicalResult.Records.Count);

        var matcher = new BaselineRecordRuleMatcher();

        var classifications = logicalResult.Records
            .Select(record => new
            {
                Record = record,
                Result = matcher.Match(
                    record,
                    layout.RecordRules)
            })
            .ToArray();

        _output.WriteLine(
            $"Layout: {layout.Id}");

        _output.WriteLine(
            $"Logical records: {classifications.Length}");

        _output.WriteLine("");

        foreach (var rule in layout.RecordRules)
        {
            var count = classifications.Count(item =>
                item.Result.Status == RuleMatchStatus.Matched &&
                item.Result.Matches[0].RuleId == rule.Id);

            _output.WriteLine(
                $"{rule.Id}: {count}");
        }

        var matched = classifications
            .Where(item =>
                item.Result.Status == RuleMatchStatus.Matched)
            .ToArray();

        var unmatched = classifications
            .Where(item =>
                item.Result.Status == RuleMatchStatus.Unmatched)
            .ToArray();

        var ambiguous = classifications
            .Where(item =>
                item.Result.Status == RuleMatchStatus.Ambiguous)
            .ToArray();

        _output.WriteLine("");
        _output.WriteLine($"Matched: {matched.Length}");
        _output.WriteLine($"Unmatched: {unmatched.Length}");
        _output.WriteLine($"Ambiguous: {ambiguous.Length}");

        _output.WriteLine("");
        _output.WriteLine("Unmatched records:");

        foreach (var item in unmatched)
        {
            WriteRecord(item.Record);
        }

        _output.WriteLine("");
        _output.WriteLine("Ambiguous records:");

        foreach (var item in ambiguous)
        {
            WriteRecord(item.Record);

            foreach (var match in item.Result.Matches)
            {
                _output.WriteLine(
                    $"    Matching rule: {match.RuleId}");
            }
        }

        Assert.Equal(
            classifications.Length,
            matched.Length +
            unmatched.Length +
            ambiguous.Length);

        // The initial rules are deliberately incomplete, but they
        // should not overlap.
        Assert.Empty(ambiguous);

        var continuedRecord = Assert.Single(
            logicalResult.Records,
            record => record.CrossesPageBoundary);

        var continuedClassification =
            matcher.Match(
                continuedRecord,
                layout.RecordRules);

        Assert.Equal(
            RuleMatchStatus.Matched,
            continuedClassification.Status);

        Assert.Equal(
            "subtotal-row",
            Assert.Single(
                continuedClassification.Matches).RuleId);

        void WriteRecord(BaselineRecord record)
        {
            _output.WriteLine(
                $"  Pages {record.StartPageNumber}-" +
                $"{record.EndPageNumber}, " +
                $"groups {record.Groups.Count}, " +
                $"tokens {record.SourceTokens.Count}, " +
                $"left {record.Left:F3}, " +
                $"right {record.Right:F3}");

            _output.WriteLine(
                $"    {record.Text}");
        }
    }

    [Fact]
    public void Recognize_real_general_ledger_account_fields_from_json_layout()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            "general-ledger.v1.json");

        using var layoutStream = File.OpenRead(layoutPath);
        var loadResult = new LayoutDefinitionLoader().Load(layoutStream);

        Assert.True(
            loadResult.IsValid,
            string.Join(
                Environment.NewLine,
                loadResult.Messages.Select(message => message.Message)));

        var layout = Assert.IsType<LayoutDefinition>(
            loadResult.Definition);

        var document = new ITextPdfTokenExtractor().Extract(pdfPath);

        var sectionDiscoverer = new SectionDiscoverer();
        var sectionResults = layout.Sections
            .Select(section => sectionDiscoverer.Discover(
                section,
                layout.Rules,
                document,
                layout.Defaults))
            .ToArray();

        Assert.All(
            sectionResults,
            result => Assert.Equal(
                SectionDiscoveryStatus.Discovered,
                result.Status));

        var continuationPolicy =
            new DefinitionRecordContinuationPolicy(
                layout.RecordContinuations);

        var recordResult =
            new BaselineRecordDiscoverer().Discover(
                sectionResults,
                continuationPolicy,
                layout.Defaults.BaselineTolerance);

        Assert.Empty(recordResult.Diagnostics);
        Assert.Equal(348, recordResult.Records.Count);

        var recordMatcher = new BaselineRecordRuleMatcher();

        var classifiedRecords = recordResult.Records
            .Select(record => new
            {
                Record = record,
                Match = recordMatcher.Match(
                    record,
                    layout.RecordRules)
            })
            .ToArray();

        var recordsByRule = classifiedRecords
    .Where(item =>
        item.Match.Status == RuleMatchStatus.Matched &&
        item.Match.Matches.Count == 1)
    .GroupBy(item => item.Match.Matches[0].RuleId)
    .ToDictionary(
        group => group.Key,
        group => group.Select(item => item.Record).ToArray());

        Assert.Equal(93, recordsByRule["subtotal-row"].Length);
        Assert.Single(recordsByRule["report-total-row"]);
        Assert.Equal(5, recordsByRule["summary-row"].Length);
        Assert.Single(recordsByRule["signature-row"]);

        WriteRecordShape(
            "subtotal-row",
            recordsByRule["subtotal-row"]);

        WriteRecordShape(
            "report-total-row",
            recordsByRule["report-total-row"]);

        WriteRecordShape(
            "summary-row",
            recordsByRule["summary-row"]);

        WriteRecordShape(
            "signature-row",
            recordsByRule["signature-row"]);

        var accountRecords = classifiedRecords
            .Where(item =>
                item.Match.Status == RuleMatchStatus.Matched &&
                item.Match.Matches.Count == 1 &&
                item.Match.Matches[0].RuleId == "account-row")
            .Select(item => item.Record)
            .ToArray();

        Assert.Equal(248, accountRecords.Length);

        var accountTokenCounts = accountRecords
            .GroupBy(record => record.SourceTokens.Count)
            .OrderBy(group => group.Key)
            .Select(group => new
            {
                TokenCount = group.Key,
                RecordCount = group.Count()
            })
            .ToArray();

                _output.WriteLine("");
                _output.WriteLine("Account-row token shapes:");

                foreach (var shape in accountTokenCounts)
                {
                    _output.WriteLine(
                        $"  {shape.TokenCount} tokens: " +
                        $"{shape.RecordCount} records");
                }

                Assert.Single(accountTokenCounts);
                Assert.Equal(8, accountTokenCounts[0].TokenCount);
                Assert.Equal(248, accountTokenCounts[0].RecordCount);

                var accountParts = accountRecords
                    .Select(record => new
                    {
                        Record = record,
                        Key = record.SourceTokens[0],
                        Label = record.SourceTokens[1],
                        Values = record.SourceTokens.Skip(2).ToArray()
                    })
                    .ToArray();

                Assert.All(
                    accountParts,
                    account =>
                    {
                        var key = account.Key.Text.Trim();

                        Assert.Equal(5, key.Length);
                        Assert.All(key, character => Assert.True(char.IsDigit(character)));

                        Assert.False(
                            string.IsNullOrWhiteSpace(account.Label.Text));

                        Assert.Equal(6, account.Values.Length);
                    });

                _output.WriteLine("");
                _output.WriteLine("Account key coordinates:");
                _output.WriteLine(
                    $"  Left:  " +
                    $"{accountParts.Min(item => item.Key.Left):F3} - " +
                    $"{accountParts.Max(item => item.Key.Left):F3}");
                _output.WriteLine(
                    $"  Right: " +
                    $"{accountParts.Min(item => item.Key.Right):F3} - " +
                    $"{accountParts.Max(item => item.Key.Right):F3}");

                _output.WriteLine("");
                _output.WriteLine("Account label coordinates:");
                _output.WriteLine(
                    $"  Left:  " +
                    $"{accountParts.Min(item => item.Label.Left):F3} - " +
                    $"{accountParts.Max(item => item.Label.Left):F3}");
                _output.WriteLine(
                    $"  Right: " +
                    $"{accountParts.Min(item => item.Label.Right):F3} - " +
                    $"{accountParts.Max(item => item.Label.Right):F3}");

                var keyLabelGaps = accountParts
                    .Select(item => item.Label.Left - item.Key.Right)
                    .ToArray();

                var labelValueGaps = accountParts
                    .Select(item => item.Values[0].Left - item.Label.Right)
                    .ToArray();

                _output.WriteLine("");
                _output.WriteLine("Horizontal separation:");
                _output.WriteLine(
                    $"  Key -> label gap: " +
                    $"{keyLabelGaps.Min():F3} - " +
                    $"{keyLabelGaps.Max():F3}");
                _output.WriteLine(
                    $"  Label -> value-1 gap: " +
                    $"{labelValueGaps.Min():F3} - " +
                    $"{labelValueGaps.Max():F3}");

                Assert.All(
                    accountParts,
                    item =>
                    {
                        Assert.True(
                            item.Key.Right < item.Label.Left,
                            $"Key and label overlap on page " +
                            $"{item.Record.StartPageNumber}: {item.Record.Text}");

                        Assert.True(
                            item.Label.Right < item.Values[0].Left,
                            $"Label and value-1 overlap on page " +
                            $"{item.Record.StartPageNumber}: {item.Record.Text}");
                    });

                _output.WriteLine("");
                _output.WriteLine("Coordinate extremes:");

                var coordinateExtremes = new[]
                {
            accountParts.MinBy(item => item.Key.Left)!,
            accountParts.MaxBy(item => item.Key.Right)!,
            accountParts.MinBy(item => item.Label.Left)!,
            accountParts.MaxBy(item => item.Label.Right)!
        }
        .DistinctBy(item => item.Record)
        .ToArray();

        foreach (var item in coordinateExtremes)
        {
            _output.WriteLine(
                $"  Page {item.Record.StartPageNumber}: " +
                $"key '{item.Key.Text.Trim()}' " +
                $"[{item.Key.Left:F3}, {item.Key.Right:F3}], " +
                $"label '{item.Label.Text.Trim()}' " +
                $"[{item.Label.Left:F3}, {item.Label.Right:F3}]");
        }

        var accountFieldSet = Assert.Single(
            layout.RecordFields,
            fieldSet => fieldSet.RecordRuleId == "account-row");

        Assert.Equal(8, accountFieldSet.Fields.Count);

        var fieldMatcher = new BaselineRecordFieldMatcher();

        var recognizedAccounts = accountRecords
            .Select(record => new
            {
                Record = record,
                Fields = fieldMatcher.Match(
                    record,
                    accountFieldSet,
                    layout.Defaults)
            })
            .ToArray();

        var allFields = recognizedAccounts
            .SelectMany(account => account.Fields)
            .ToArray();

        var matched = allFields.Count(
            field => field.Status == RuleMatchStatus.Matched);

        var unmatched = allFields.Count(
            field => field.Status == RuleMatchStatus.Unmatched);

        var ambiguous = allFields.Count(
            field => field.Status == RuleMatchStatus.Ambiguous);

        var fieldSetsByRuleId = layout.RecordFields
    .ToDictionary(
        fieldSet => fieldSet.RecordRuleId,
        StringComparer.Ordinal);

        var recognizedFields = classifiedRecords
            .Where(item =>
                item.Match.Status == RuleMatchStatus.Matched &&
                item.Match.Matches.Count == 1)
            .SelectMany(item =>
            {
                var ruleId = item.Match.Matches[0].RuleId;
                var fieldSet = fieldSetsByRuleId[ruleId];

                return fieldMatcher
                    .Match(
                        item.Record,
                        fieldSet,
                        layout.Defaults)
                    .Select(field => new
                    {
                        RuleId = ruleId,
                        Record = item.Record,
                        Field = field
                    });
            })
            .ToArray();

        var allMatchedFieldCount = recognizedFields.Count(
            item => item.Field.Status == RuleMatchStatus.Matched);

        var allUnmatchedFieldCount = recognizedFields.Count(
            item => item.Field.Status == RuleMatchStatus.Unmatched);

        var allAmbiguousFieldCount = recognizedFields.Count(
            item => item.Field.Status == RuleMatchStatus.Ambiguous);

        _output.WriteLine("");
        _output.WriteLine("Complete logical-record field recognition:");
        _output.WriteLine(
            $"  Logical records: {classifiedRecords.Length}");
        _output.WriteLine(
            $"  Field results: {recognizedFields.Length}");
        _output.WriteLine(
            $"  Matched: {allMatchedFieldCount}");
        _output.WriteLine(
            $"  Unmatched: {allUnmatchedFieldCount}");
        _output.WriteLine(
            $"  Ambiguous: {allAmbiguousFieldCount}");

        var expectedFieldCounts = new Dictionary<string, int>
        {
            ["account-row"] = 248 * 8,
            ["subtotal-row"] = 93 * 8,
            ["report-total-row"] = 1 * 7,
            ["summary-row"] = 5 * 7,
            ["signature-row"] = 1 * 3
        };

        _output.WriteLine("");
        _output.WriteLine("Field results by classification:");

        foreach (var expected in expectedFieldCounts)
        {
            var fields = recognizedFields
                .Where(item => item.RuleId == expected.Key)
                .ToArray();

            var matchedFields = fields.Count(
                item => item.Field.Status == RuleMatchStatus.Matched);

            _output.WriteLine(
                $"  {expected.Key}: " +
                $"{matchedFields}/{expected.Value} matched");

            Assert.Equal(expected.Value, fields.Length);
            Assert.Equal(expected.Value, matchedFields);
        }

        Assert.Equal(348, classifiedRecords.Length);
        Assert.Equal(2773, recognizedFields.Length);
        Assert.Equal(2773, allMatchedFieldCount);
        Assert.Equal(0, allUnmatchedFieldCount);
        Assert.Equal(0, allAmbiguousFieldCount);

        Assert.All(
            recognizedFields,
            item =>
            {
                Assert.NotNull(item.Field.Value);

                var evidence = item.Field.Value!.Evidence;

                Assert.Single(evidence.SourceTokens);
                Assert.All(
                    evidence.Criteria,
                    criterion => Assert.True(criterion.IsMatch));
            });

        var continuedSubtotal = Assert.Single(
    recordsByRule["subtotal-row"],
    record => record.CrossesPageBoundary);

        var subtotalFieldSet =
            fieldSetsByRuleId["subtotal-row"];

        var continuedSubtotalFields = fieldMatcher
            .Match(
                continuedSubtotal,
                subtotalFieldSet,
                layout.Defaults)
            .ToDictionary(
                field => field.FieldId,
                StringComparer.Ordinal);

        _output.WriteLine("");
        _output.WriteLine("Continued subtotal field provenance:");

        foreach (var field in continuedSubtotalFields.Values)
        {
            Assert.Equal(
                RuleMatchStatus.Matched,
                field.Status);

            var token = Assert.Single(
                field.Value!.Evidence.SourceTokens);

            _output.WriteLine(
                $"  {field.FieldId}: " +
                $"page {token.PageNumber}, " +
                $"left {token.Left:F3}, " +
                $"right {token.Right:F3}, " +
                $"text: '{token.Text}'");
        }

        foreach (var fieldId in new[] { "label", "key" })
        {
            var token = Assert.Single(
                continuedSubtotalFields[fieldId]
                    .Value!
                    .Evidence
                    .SourceTokens);

            Assert.Equal(8, token.PageNumber);
        }

        foreach (var fieldId in new[]
        {
    "value-1",
    "value-2",
    "value-3",
    "value-4",
    "value-5",
    "value-6"
})
        {
            var token = Assert.Single(
                continuedSubtotalFields[fieldId]
                    .Value!
                    .Evidence
                    .SourceTokens);

            Assert.Equal(9, token.PageNumber);
        }



        Assert.Equal(248 * 8, allFields.Length);
        Assert.Equal(1984, matched);
        Assert.Equal(0, unmatched);
        Assert.Equal(0, ambiguous);

        _output.WriteLine($"Account records: {accountRecords.Length}");
        _output.WriteLine($"Field definitions: {accountFieldSet.Fields.Count}");
        _output.WriteLine($"Field results: {allFields.Length}");
        _output.WriteLine($"Matched: {matched}");
        _output.WriteLine($"Unmatched: {unmatched}");
        _output.WriteLine($"Ambiguous: {ambiguous}");
        _output.WriteLine("");

        Assert.Equal(248 * 8, allFields.Length);
        Assert.Equal(1984, matched);
        Assert.Equal(0, unmatched);
        Assert.Equal(0, ambiguous);

        foreach (var fieldDefinition in accountFieldSet.Fields)
        {
            var fieldResults = allFields
                .Where(field => field.FieldId == fieldDefinition.Id)
                .ToArray();

            _output.WriteLine(
                $"{fieldDefinition.Id}: " +
                $"{fieldResults.Count(field => field.Status == RuleMatchStatus.Matched)} matched");

            Assert.Equal(248, fieldResults.Length);

            Assert.All(
                fieldResults,
                field => Assert.Equal(
                    RuleMatchStatus.Matched,
                    field.Status));
        }

        Assert.All(
            allFields,
            field =>
            {
                Assert.NotNull(field.Value);

                var evidence = field.Value!.Evidence;
                Assert.Single(evidence.SourceTokens);
                Assert.All(
                    evidence.Criteria,
                    criterion => Assert.True(criterion.IsMatch));
            });

        _output.WriteLine("");
        _output.WriteLine("First and last three account records:");

        var samples = recognizedAccounts
            .Take(3)
            .Concat(recognizedAccounts.TakeLast(3))
            .ToArray();

        foreach (var account in samples)
        {
            _output.WriteLine(
                $"Pages {account.Record.StartPageNumber}-" +
                $"{account.Record.EndPageNumber}: " +
                account.Record.Text);

            foreach (var field in account.Fields)
            {
                var value = field.Value!;
                var token = Assert.Single(
                    value.Evidence.SourceTokens);

                _output.WriteLine(
                    $"  {field.FieldId}: " +
                    $"'{value.Value}', " +
                    $"page {token.PageNumber}, " +
                    $"baseline {token.Baseline:F3}, " +
                    $"right {token.Right:F3}");
            }
        }
    }

    [Fact]
    public void Recognize_real_general_ledger_through_itext_facade()
    {
        var pdfPath = Environment.GetEnvironmentVariable(
            "PDF_LAYOUT_TEST_FILE");

        Assert.False(
            string.IsNullOrWhiteSpace(pdfPath),
            "Set the PDF_LAYOUT_TEST_FILE environment variable.");

        Assert.True(
            File.Exists(pdfPath),
            $"PDF file does not exist: {pdfPath}");

        var layoutPath = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "SoftipMop",
            "general-ledger.v1.json");

        using var layoutStream = File.OpenRead(layoutPath);
        var loadResult = new LayoutDefinitionLoader()
            .Load(layoutStream);

        Assert.True(
            loadResult.IsValid,
            string.Join(
                Environment.NewLine,
                loadResult.Messages.Select(
                    message => message.Message)));

        var layout = Assert.IsType<LayoutDefinition>(
            loadResult.Definition);

        var result = new ITextLayoutRecognizer()
            .Recognize(pdfPath, layout);

        var matchedRecords = result.Records
            .Where(record =>
                record.Status == RuleMatchStatus.Matched)
            .ToArray();

        var unmatchedRecords = result.Records
            .Where(record =>
                record.Status == RuleMatchStatus.Unmatched)
            .ToArray();

        var ambiguousRecords = result.Records
            .Where(record =>
                record.Status == RuleMatchStatus.Ambiguous)
            .ToArray();

        var fields = result.Records
            .SelectMany(record => record.Fields)
            .ToArray();

        var matchedFields = fields.Count(
            field => field.Status == RuleMatchStatus.Matched);

        var unmatchedFields = fields.Count(
            field => field.Status == RuleMatchStatus.Unmatched);

        var ambiguousFields = fields.Count(
            field => field.Status == RuleMatchStatus.Ambiguous);

        _output.WriteLine(
            $"Layout: {layout.Id}");
        _output.WriteLine(
            $"Pages: {result.Document.Pages.Count}");
        _output.WriteLine(
            $"Sections: {result.Sections.Sum(section => section.Sections.Count)}");
        _output.WriteLine(
            $"Logical records: {result.Records.Count}");
        _output.WriteLine(
            $"Matched records: {matchedRecords.Length}");
        _output.WriteLine(
            $"Unmatched records: {unmatchedRecords.Length}");
        _output.WriteLine(
            $"Ambiguous records: {ambiguousRecords.Length}");
        _output.WriteLine(
            $"Field results: {fields.Length}");
        _output.WriteLine(
            $"Matched fields: {matchedFields}");
        _output.WriteLine(
            $"Unmatched fields: {unmatchedFields}");
        _output.WriteLine(
            $"Ambiguous fields: {ambiguousFields}");
        _output.WriteLine(
            $"Diagnostics: {result.Diagnostics.Count}");

        Assert.Equal(13, result.Document.Pages.Count);
        Assert.Equal(13, result.Sections.Sum(
            section => section.Sections.Count));

        Assert.Equal(348, result.Records.Count);
        Assert.Equal(348, matchedRecords.Length);
        Assert.Empty(unmatchedRecords);
        Assert.Empty(ambiguousRecords);

        Assert.Equal(2773, fields.Length);
        Assert.Equal(2773, matchedFields);
        Assert.Equal(0, unmatchedFields);
        Assert.Equal(0, ambiguousFields);

        Assert.Empty(result.Diagnostics);

        var continuedRecord = Assert.Single(
            result.Records,
            record => record.SourceRecord.CrossesPageBoundary);

        Assert.Equal(
            "subtotal-row",
            continuedRecord.RuleId);

        Assert.Single(
            continuedRecord.SourceRecord.Continuations);

        Assert.Equal(
            8,
            continuedRecord.SourceRecord.StartPageNumber);

        Assert.Equal(
            9,
            continuedRecord.SourceRecord.EndPageNumber);

        var continuedFields = continuedRecord.Fields
            .ToDictionary(
                field => field.FieldId,
                StringComparer.Ordinal);

        Assert.Equal(8, continuedFields.Count);

        _output.WriteLine("");
        _output.WriteLine(
            "Continued record field provenance:");

        foreach (var field in continuedRecord.Fields)
        {
            Assert.Equal(
                RuleMatchStatus.Matched,
                field.Status);

            var token = Assert.Single(
                field.Value!.Evidence.SourceTokens);

            _output.WriteLine(
                $"  {field.FieldId}: " +
                $"page {token.PageNumber}, " +
                $"text: '{token.Text}'");
        }

        foreach (var fieldId in new[] { "label", "key" })
        {
            var token = Assert.Single(
                continuedFields[fieldId]
                    .Value!
                    .Evidence
                    .SourceTokens);

            Assert.Equal(8, token.PageNumber);
        }

        foreach (var fieldId in new[]
        {
        "value-1",
        "value-2",
        "value-3",
        "value-4",
        "value-5",
        "value-6"
    })
        {
            var token = Assert.Single(
                continuedFields[fieldId]
                    .Value!
                    .Evidence
                    .SourceTokens);

            Assert.Equal(9, token.PageNumber);
        }
    }

    private void WriteRecordShape(
    string ruleId,
    IReadOnlyList<BaselineRecord> records)
    {
        _output.WriteLine("");
        _output.WriteLine(
            $"{ruleId}: {records.Count} records");

        var shapes = records
            .GroupBy(record => new
            {
                GroupCount = record.Groups.Count,
                TokenCount = record.SourceTokens.Count,
                record.CrossesPageBoundary
            })
            .OrderBy(shape => shape.Key.GroupCount)
            .ThenBy(shape => shape.Key.TokenCount)
            .ThenBy(shape => shape.Key.CrossesPageBoundary)
            .ToArray();

        _output.WriteLine("  Shapes:");

        foreach (var shape in shapes)
        {
            _output.WriteLine(
                $"    groups {shape.Key.GroupCount}, " +
                $"tokens {shape.Key.TokenCount}, " +
                $"crosses page {shape.Key.CrossesPageBoundary}: " +
                $"{shape.Count()} records");
        }

        var maximumTokenCount = records.Max(
            record => record.SourceTokens.Count);

        _output.WriteLine("  Token positions:");

        for (var index = 0; index < maximumTokenCount; index++)
        {
            var tokens = records
                .Where(record => record.SourceTokens.Count > index)
                .Select(record => record.SourceTokens[index])
                .ToArray();

            var samples = tokens
                .Select(token => token.Text.Trim())
                .Distinct()
                .Take(3)
                .ToArray();

            _output.WriteLine(
                $"    Token {index + 1}: " +
                $"count {tokens.Length}, " +
                $"left {tokens.Min(token => token.Left):F3}-" +
                $"{tokens.Max(token => token.Left):F3}, " +
                $"right {tokens.Min(token => token.Right):F3}-" +
                $"{tokens.Max(token => token.Right):F3}");

            _output.WriteLine(
                $"      Samples: " +
                string.Join(" | ", samples));
        }

        var pageSpanningRecords = records
            .Where(record => record.CrossesPageBoundary)
            .ToArray();

        if (pageSpanningRecords.Length == 0)
            return;

        _output.WriteLine("  Page-spanning records:");

        foreach (var record in pageSpanningRecords)
        {
            _output.WriteLine(
                $"    Pages {record.StartPageNumber}-" +
                $"{record.EndPageNumber}: {record.Text}");

            foreach (var token in record.SourceTokens)
            {
                _output.WriteLine(
                    $"      Page {token.PageNumber}, " +
                    $"left {token.Left:F3}, " +
                    $"right {token.Right:F3}, " +
                    $"text: '{token.Text}'");
            }
        }
    }
}