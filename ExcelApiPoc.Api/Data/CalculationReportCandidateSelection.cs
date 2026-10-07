namespace ExcelApiPoc.Api.Data;

public enum CalculationPackageSelectionFailure
{
    NoCalculationReport,
    MultipleCalculationReports,
    CalculationNotImplemented
}

public sealed class CalculationPackageSelectionException : Exception
{
    public CalculationPackageSelectionException(
        CalculationPackageSelectionFailure failure,
        string message)
        : base(message)
    {
        Failure = failure;
    }

    public CalculationPackageSelectionFailure Failure { get; }
}

public static class CalculationReportCandidateSelection
{
    public static CalculationReportCandidate SelectExactlyOne(
        IReadOnlyList<CalculationReportCandidate> candidates,
        string ico,
        int fiscalYear)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        if (candidates.Count == 0)
        {
            throw new CalculationPackageSelectionException(
                CalculationPackageSelectionFailure.NoCalculationReport,
                $"No mapped calculation financial report was found in RegisterUZ " +
                $"for IČO '{ico}' and fiscal year {fiscalYear}.");
        }

        CalculationReportCandidate? selected = candidates.Count == 1 ? candidates[0] : null;
        if (selected is null && candidates.Select(candidate =>
                (candidate.RegisterUzTemplateId, candidate.AccountFrameworkId,
                 candidate.FrameworkCode)).Distinct().Count() == 1)
        {
            // Two ordinary filings of the same report can coexist. A unique
            // approved statement identifies the report to use for calculation.
            // Equal or missing approval dates still need an explicit decision.
            var approved = candidates.Where(candidate => candidate.ApprovalDate.HasValue).ToArray();
            if (approved.Length == 1)
                selected = approved[0];
            else if (candidates.All(candidate => candidate.ContentMatchesFirstCandidate))
            {
                // Equal values at every table/row/column make these filings
                // interchangeable for calculation. Use submission date solely
                // to select the source identity; auditor-attachment dates do
                // not indicate a new financial report.
                selected = candidates.OrderByDescending(candidate => candidate.SubmissionDate)
                    .ThenByDescending(candidate => candidate.FinancialReportId)
                    .First();
            }
        }

        if (selected is null)
        {
            string reportIds = string.Join(
                ", ",
                candidates
                    .Select(candidate => candidate.FinancialReportId)
                    .OrderBy(id => id));

            throw new CalculationPackageSelectionException(
                CalculationPackageSelectionFailure.MultipleCalculationReports,
                $"Multiple mapped calculation financial reports were found in " +
                $"RegisterUZ for IČO '{ico}' and fiscal year {fiscalYear}: " +
                $"{reportIds}. Selection is ambiguous.");
        }

        CalculationReportCandidate candidate = selected;

        if (!candidate.CalculationImplemented)
        {
            throw new CalculationPackageSelectionException(
                CalculationPackageSelectionFailure.CalculationNotImplemented,
                $"RegisterUZ template {candidate.RegisterUzTemplateId} was found " +
                $"for IČO '{ico}' and fiscal year {fiscalYear}, but its " +
                $"calculation is not implemented and approved for production.");
        }

        return candidate;
    }
}
