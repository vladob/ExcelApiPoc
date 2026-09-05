using ExcelApiPoc.Api.Data;
using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;

namespace ExcelApiPoc.Api.Tests;

public sealed class AuditCalculationPackageSelectionPolicyTests
{
    [Fact]
    public void IsAuditedPeriod_UsesRequestedFiscalYear()
    {
        Assert.True(AuditCalculationPackageSelectionPolicy.IsAuditedPeriod(Statement(2024), 2024));
        Assert.False(AuditCalculationPackageSelectionPolicy.IsAuditedPeriod(Statement(2025), 2024));
    }

    [Fact]
    public void SelectExactlyOne_DoesNotUseLegalFormAsSelector()
    {
        AuditCalculationPackageCandidate candidate = Candidate(10, 690);
        Assert.Same(candidate, AuditCalculationPackageSelectionPolicy.SelectExactlyOne(
            new[] { candidate }, "00322792", 2024));
    }

    [Fact]
    public void SelectExactlyOne_ReportsUnsupportedConfigurationClearly()
    {
        Assert.Throws<AuditCalculationPackageSelectionException>(() =>
            AuditCalculationPackageSelectionPolicy.SelectExactlyOne(
                Array.Empty<AuditCalculationPackageCandidate>(), "00322792", 2024));
    }

    [Fact]
    public void SelectExactlyOne_RejectsAmbiguousReports()
    {
        Assert.Throws<AuditCalculationPackageAmbiguousException>(() =>
            AuditCalculationPackageSelectionPolicy.SelectExactlyOne(
                new[] { Candidate(10, 690), Candidate(11, 690) }, "00322792", 2024));
    }

    [Theory]
    [InlineData(AuditTemplatePackageV2ResolutionFailure.TemplateNotFound)]
    [InlineData(AuditTemplatePackageV2ResolutionFailure.TemplateNotApplicable)]
    [InlineData(AuditTemplatePackageV2ResolutionFailure.AssociationNotFound)]
    public void IsUnsupported_ExcludesUnconfiguredSupportingTemplates(
        AuditTemplatePackageV2ResolutionFailure failure)
    {
        Assert.True(AuditCalculationPackageSelectionPolicy.IsUnsupported(failure));
        Assert.False(AuditCalculationPackageSelectionPolicy.IsUnsupported(
            AuditTemplatePackageV2ResolutionFailure.MultipleAssociations));
    }

    private static FinancialStatementDto Statement(int year) => new()
    {
        Id = year,
        PeriodFrom = $"{year}-01",
        PeriodTo = $"{year}-12",
        FinancialReports = Array.Empty<FinancialReportDto>()
    };

    private static AuditCalculationPackageCandidate Candidate(long reportId, int templateId) => new(
        Statement(2024),
        new FinancialReportDto
        {
            Id = reportId,
            TemplateId = templateId,
            Attachments = Array.Empty<FinancialReportAttachmentDto>(),
            Tables = Array.Empty<FinancialReportTableDto>()
        },
        templateId,
        new AuditTemplatePackageV2());
}
