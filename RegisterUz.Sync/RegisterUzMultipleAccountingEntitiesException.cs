namespace RegisterUz.Sync;

public sealed class RegisterUzMultipleAccountingEntitiesException
    : Exception
{
    public RegisterUzMultipleAccountingEntitiesException(
        string ico,
        IReadOnlyCollection<long> registerUzIds)
        : base(
            $"There are multiple accounting entities with IČO {ico}. " +
            "This functionality is not supported in this version!")
    {
        Ico = ico;
        RegisterUzIds = registerUzIds;
    }

    public string Ico { get; }

    public IReadOnlyCollection<long> RegisterUzIds { get; }
}