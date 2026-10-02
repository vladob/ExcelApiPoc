using ExcelApiPoc.Api.Data;
using Xunit;

namespace ExcelApiPoc.Api.Tests;

public sealed class CalculationReportCandidateSelectionTests
{
    [Fact]
    public void SelectExactlyOne_ReturnsEnabledCandidate()
    {
        CalculationReportCandidate expected = Candidate(
            reportId: 9538193,
            templateId: 690,
            implemented: true);

        CalculationReportCandidate actual =
            CalculationReportCandidateSelection.SelectExactlyOne(
                new[] { expected },
                "00322792",
                2024);

        Assert.Same(expected, actual);
    }

    [Fact]
    public void SelectExactlyOne_RejectsMissingCandidate()
    {
        CalculationPackageSelectionException exception =
            Assert.Throws<CalculationPackageSelectionException>(() =>
                CalculationReportCandidateSelection.SelectExactlyOne(
                    Array.Empty<CalculationReportCandidate>(),
                    "00322792",
                    2024));

        Assert.Equal(
            CalculationPackageSelectionFailure.NoCalculationReport,
            exception.Failure);
    }

    [Fact]
    public void SelectExactlyOne_RejectsAmbiguousCandidates()
    {
        CalculationPackageSelectionException exception =
            Assert.Throws<CalculationPackageSelectionException>(() =>
                CalculationReportCandidateSelection.SelectExactlyOne(
                    new[]
                    {
                        Candidate(10, 690, true),
                        Candidate(20, 690, true)
                    },
                    "00322792",
                    2024));

        Assert.Equal(
            CalculationPackageSelectionFailure.MultipleCalculationReports,
            exception.Failure);
        Assert.Contains("10, 20", exception.Message);
    }

    [Fact]
    public void SelectExactlyOne_Prefers_the_only_approved_statement_of_the_same_template()
    {
        CalculationReportCandidate earlier = Candidate(9393823, 690, true) with
        {
            FinancialStatementId = 6223160,
            AccountFrameworkId = 1,
            FrameworkCode = "GOV_LOCAL"
        };
        CalculationReportCandidate approved = Candidate(9443513, 690, true) with
        {
            FinancialStatementId = 6266919,
            AccountFrameworkId = 1,
            FrameworkCode = "GOV_LOCAL",
            ApprovalDate = new DateTime(2025, 12, 7)
        };

        Assert.Same(approved, CalculationReportCandidateSelection.SelectExactlyOne(
            new[] { earlier, approved }, "00323292", 2024));
    }

    [Fact]
    public void SelectExactlyOne_Does_not_select_between_two_approved_statements()
    {
        var candidates = new[]
        {
            Candidate(10, 690, true) with { ApprovalDate = new DateTime(2025, 3, 1) },
            Candidate(20, 690, true) with { ApprovalDate = new DateTime(2025, 12, 7) }
        };
        var exception = Assert.Throws<CalculationPackageSelectionException>(() =>
            CalculationReportCandidateSelection.SelectExactlyOne(candidates, "00323292", 2024));
        Assert.Equal(CalculationPackageSelectionFailure.MultipleCalculationReports, exception.Failure);
    }

    [Fact]
    public void SelectExactlyOne_Does_not_use_approval_to_choose_a_different_template()
    {
        var candidates = new[]
        {
            Candidate(10, 690, true),
            Candidate(20, 699, true) with { ApprovalDate = new DateTime(2025, 12, 7) }
        };
        var exception = Assert.Throws<CalculationPackageSelectionException>(() =>
            CalculationReportCandidateSelection.SelectExactlyOne(candidates, "00323292", 2024));
        Assert.Equal(CalculationPackageSelectionFailure.MultipleCalculationReports, exception.Failure);
    }

    [Fact]
    public void SelectExactlyOne_RejectsPlannedTemplate()
    {
        CalculationPackageSelectionException exception =
            Assert.Throws<CalculationPackageSelectionException>(() =>
                CalculationReportCandidateSelection.SelectExactlyOne(
                    new[] { Candidate(30, 699, false) },
                    "12345678",
                    2024));

        Assert.Equal(
            CalculationPackageSelectionFailure.CalculationNotImplemented,
            exception.Failure);
        Assert.Contains("699", exception.Message);
    }

    [Fact]
    public void Identical_reports_use_submission_date_instead_of_report_id()
    {
        var earlier = Candidate(9443513, 690, true) with
        {
            SubmissionDate = new DateTime(2025, 3, 1),
            ContentMatchesFirstCandidate = true
        };
        var later = Candidate(9393823, 690, true) with
        {
            SubmissionDate = new DateTime(2025, 3, 15),
            ContentMatchesFirstCandidate = true
        };
        Assert.Same(later, CalculationReportCandidateSelection.SelectExactlyOne(
            new[] { earlier, later }, "00323292", 2024));
    }

    [Fact]
    public void A_changed_report_still_requires_selection_even_if_submitted_later()
    {
        var candidates = new[]
        {
            Candidate(9393823, 690, true) with
            {
                SubmissionDate = new DateTime(2025, 3, 1), ContentMatchesFirstCandidate = true
            },
            Candidate(9443513, 690, true) with
            {
                SubmissionDate = new DateTime(2025, 3, 15), ContentMatchesFirstCandidate = false
            }
        };
        var exception = Assert.Throws<CalculationPackageSelectionException>(() =>
            CalculationReportCandidateSelection.SelectExactlyOne(candidates, "00323292", 2024));
        Assert.Equal(CalculationPackageSelectionFailure.MultipleCalculationReports, exception.Failure);
    }

    [Fact]
    public void Matching_content_does_not_make_different_templates_interchangeable()
    {
        var candidates = new[]
        {
            Candidate(10, 690, true) with { ContentMatchesFirstCandidate = true },
            Candidate(20, 699, true) with { ContentMatchesFirstCandidate = true }
        };
        Assert.Throws<CalculationPackageSelectionException>(() =>
            CalculationReportCandidateSelection.SelectExactlyOne(candidates, "00323292", 2024));
    }

    private static CalculationReportCandidate Candidate(
        long reportId,
        int templateId,
        bool implemented)
    {
        return new CalculationReportCandidate
        {
            FinancialReportId = reportId,
            RegisterUzTemplateId = templateId,
            CalculationImplemented = implemented
        };
    }
}
