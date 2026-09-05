using ExcelApiPoc.Api.Models;
using ExcelApiPoc.Api.Models.AccountingEntities;

namespace ExcelApiPoc.Api.Data;

public sealed record AuditCalculationPackageCandidate(
    FinancialStatementDto Statement,
    FinancialReportDto Report,
    int TemplateErpId,
    AuditTemplatePackageV2 Package);

public static class AuditCalculationPackageSelectionPolicy
{
    public static bool IsUnsupported(AuditTemplatePackageV2ResolutionFailure failure) =>
        failure is AuditTemplatePackageV2ResolutionFailure.TemplateNotFound or
            AuditTemplatePackageV2ResolutionFailure.TemplateNotApplicable or
            AuditTemplatePackageV2ResolutionFailure.AssociationNotFound;

    public static bool IsAuditedPeriod(FinancialStatementDto statement, int fiscalYear) =>
        statement.PeriodFrom == $"{fiscalYear:0000}-01" &&
        statement.PeriodTo == $"{fiscalYear:0000}-12";

    public static AuditCalculationPackageCandidate SelectExactlyOne(
        IReadOnlyList<AuditCalculationPackageCandidate> candidates,
        string ico,
        int fiscalYear)
    {
        if (candidates.Count == 0)
            throw new AuditCalculationPackageSelectionException(
                $"No calculation-capable RegisterUZ financial report was found for IČO {ico} and fiscal year {fiscalYear}.");

        if (candidates.Count > 1)
            throw new AuditCalculationPackageAmbiguousException(
                $"Multiple calculation-capable RegisterUZ financial reports were found for IČO {ico} and fiscal year {fiscalYear}: " +
                string.Join(", ", candidates.Select(candidate =>
                    $"report {candidate.Report.Id} / template {candidate.TemplateErpId}")) + ".");

        return candidates[0];
    }
}
