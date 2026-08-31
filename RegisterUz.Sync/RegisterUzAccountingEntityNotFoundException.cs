namespace RegisterUz.Sync;

public sealed class RegisterUzAccountingEntityNotFoundException
    : InvalidOperationException
{
    public RegisterUzAccountingEntityNotFoundException(string ico)
        : base($"No RegisterUZ accounting entity was found for IČO {ico}.")
    {
        Ico = ico;
    }

    public string Ico { get; }
}