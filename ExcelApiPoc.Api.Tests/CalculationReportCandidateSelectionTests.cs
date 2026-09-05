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
