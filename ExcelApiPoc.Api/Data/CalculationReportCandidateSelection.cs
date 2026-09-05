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

        if (candidates.Count > 1)
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

        CalculationReportCandidate candidate = candidates[0];

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
