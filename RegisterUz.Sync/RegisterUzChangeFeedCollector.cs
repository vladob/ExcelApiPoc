using RegisterUz.Core;

namespace RegisterUz.Sync;

public sealed class RegisterUzChangeFeedCollector
{
    private readonly IRegisterUzClient _client;
    private readonly IRegisterUzChangeFeedRepository _repository;

    public RegisterUzChangeFeedCollector(
        IRegisterUzClient client,
        IRegisterUzChangeFeedRepository repository)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RegisterUzChangeFeedResult> CollectAsync(
        RegisterUzObjectType objectType,
        DateTime initialChangedSinceUtc,
        int pageSize,
        int maxPages,
        CancellationToken cancellationToken = default)
    {
        if (initialChangedSinceUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Initial changed-since timestamp must be UTC.", nameof(initialChangedSinceUtc));
        if (pageSize is < 1 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (maxPages < 1)
            throw new ArgumentOutOfRangeException(nameof(maxPages));

        RegisterUzChangeFeedSession session = await _repository.BeginOrResumeAsync(
            objectType, initialChangedSinceUtc, pageSize, cancellationToken);
        int pagesRetrieved = 0;
        long observedIdCount = 0;
        long? continueAfterId = session.ContinueAfterId;

        try
        {
            while (pagesRetrieved < maxPages)
            {
                RegisterUzChangeFeedPage page = await _client.GetChangedIdsAsync(
                    objectType,
                    session.ChangedSinceUtc,
                    continueAfterId,
                    session.PageSize,
                    cancellationToken);
                ValidatePage(page, session, continueAfterId);

                RegisterUzChangeFeedPageSaveResult saved =
                    await _repository.SavePageAsync(session, page, cancellationToken);
                pagesRetrieved++;
                observedIdCount += page.Document.Value.Ids.LongLength;
                continueAfterId = saved.ContinueAfterId;

                if (saved.FeedCompleted)
                {
                    return new RegisterUzChangeFeedResult(
                        objectType, session.SyncRunId, session.ChangedSinceUtc,
                        session.ScanStartedAtUtc, pagesRetrieved, observedIdCount,
                        true, null);
                }
            }

            await _repository.CompleteBoundedRunAsync(
                session, pagesRetrieved, observedIdCount, cancellationToken);
            return new RegisterUzChangeFeedResult(
                objectType, session.SyncRunId, session.ChangedSinceUtc,
                session.ScanStartedAtUtc, pagesRetrieved, observedIdCount,
                false, continueAfterId);
        }
        catch (OperationCanceledException)
        {
            await _repository.CancelRunAsync(session, CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await _repository.FailRunAsync(session, exception, CancellationToken.None);
            throw;
        }
    }

    private static void ValidatePage(
        RegisterUzChangeFeedPage page,
        RegisterUzChangeFeedSession session,
        long? continueAfterId)
    {
        if (page.ObjectType != session.ObjectType ||
            page.ChangedSinceUtc != session.ChangedSinceUtc ||
            page.ContinueAfterId != continueAfterId ||
            page.PageSize != session.PageSize)
            throw new InvalidOperationException("RegisterUZ change-feed page does not match its active session.");

        long[] ids = page.Document.Value.Ids;
        if (page.Document.Value.HasMoreIds && ids.Length == 0)
            throw new InvalidOperationException("RegisterUZ indicated another page but returned no IDs.");

        long previous = continueAfterId ?? 0;
        foreach (long id in ids)
        {
            if (id <= previous)
                throw new InvalidOperationException("RegisterUZ change-feed IDs are not strictly increasing.");
            previous = id;
        }
    }
}
