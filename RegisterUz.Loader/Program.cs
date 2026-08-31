using System.IO.Compression;
using System.Globalization;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RegisterUz.Client;
using RegisterUz.Core;
using RegisterUz.Persistence.SqlServer;
using RegisterUz.Sync;

bool isChangeFeed = args.Length > 0 &&
                    string.Equals(args[0], "changes", StringComparison.OrdinalIgnoreCase);
bool isChangeProcessing = args.Length > 0 &&
                          string.Equals(args[0], "process-changes", StringComparison.OrdinalIgnoreCase);
bool isChangeSynchronization = args.Length > 0 &&
                               string.Equals(args[0], "sync-changes", StringComparison.OrdinalIgnoreCase);
if ((!isChangeFeed && !isChangeProcessing && !isChangeSynchronization && args.Length != 1) ||
    (isChangeFeed && args.Length is < 2 or > 4) ||
    (isChangeProcessing && args.Length is < 1 or > 3) ||
    (isChangeSynchronization && args.Length is < 2 or > 7))
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  RegisterUz.Loader <IČO>");
    Console.Error.WriteLine("  RegisterUz.Loader changes <initial-since-utc> [page-size] [max-pages-per-feed]");
    Console.Error.WriteLine("  RegisterUz.Loader process-changes [observation-batch] [entity-batch]");
    Console.Error.WriteLine("  RegisterUz.Loader sync-changes <initial-since-utc> [page-size] [max-pages-per-feed] [observation-batch] [entity-batch] [max-processing-passes]");
    Console.Error.WriteLine("Example: RegisterUz.Loader changes 2026-08-30T18:00:00Z 100 1");
    return 2;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.AddUserSecrets(
    Assembly.GetExecutingAssembly(),
    optional: true);
string? connectionString = builder.Configuration.GetConnectionString("RegisterUZ");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine(
        "ConnectionStrings:RegisterUZ is not configured. " +
        "Use appsettings.json, user secrets, or ConnectionStrings__RegisterUZ.");
    return 3;
}

string baseAddress = builder.Configuration["RegisterUz:BaseAddress"]
                     ?? RegisterUzApiClient.DefaultBaseAddress;

using var handler = new HttpClientHandler
{
    AutomaticDecompression =
        DecompressionMethods.GZip |
        DecompressionMethods.Deflate |
        DecompressionMethods.Brotli
};
using var httpClient = new HttpClient(handler)
{
    BaseAddress = new Uri(baseAddress, UriKind.Absolute),
    Timeout = TimeSpan.FromMinutes(5)
};

var client = new RegisterUzApiClient(httpClient);
var repository = new SqlRegisterUzPackageRepository(connectionString);
var loader = new RegisterUzAccountingEntityLoader(client, repository);

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

try
{
    if (isChangeFeed)
    {
        if (!DateTimeOffset.TryParse(
                args[1],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset sinceOffset))
        {
            Console.Error.WriteLine("initial-since-utc is not a valid ISO 8601 timestamp.");
            return 5;
        }

        int pageSize = args.Length >= 3 && int.TryParse(args[2], out int parsedPageSize)
            ? parsedPageSize
            : 100;
        int maxPages = args.Length >= 4 && int.TryParse(args[3], out int parsedMaxPages)
            ? parsedMaxPages
            : 1;
        if (pageSize is < 1 or > 10_000 || maxPages < 1)
        {
            Console.Error.WriteLine("page-size must be 1-10000 and max-pages-per-feed must be positive.");
            return 5;
        }

        DateTime initialSinceUtc = TruncateToSecond(sinceOffset.UtcDateTime);
        var changeFeedRepository = new SqlRegisterUzChangeFeedRepository(connectionString);
        var collector = new RegisterUzChangeFeedCollector(client, changeFeedRepository);
        RegisterUzObjectType[] objectTypes =
        [
            RegisterUzObjectType.AccountingEntity,
            RegisterUzObjectType.FinancialStatement,
            RegisterUzObjectType.FinancialReport,
            RegisterUzObjectType.AnnualReport
        ];

        foreach (RegisterUzObjectType objectType in objectTypes)
        {
            RegisterUzChangeFeedResult feed = await collector.CollectAsync(
                objectType, initialSinceUtc, pageSize, maxPages, cancellation.Token);
            Console.WriteLine(
                $"{feed.ObjectType}: run {feed.SyncRunId}; pages {feed.PagesRetrieved}; " +
                $"IDs {feed.ObservedIdCount}; " +
                (feed.FeedCompleted
                    ? "feed completed"
                    : $"resume after ID {feed.ContinueAfterId}"));
        }

        return 0;
    }

    if (isChangeProcessing)
    {
        int observationBatch = args.Length >= 2 && int.TryParse(args[1], out int parsedObservationBatch)
            ? parsedObservationBatch
            : 25;
        int entityBatch = args.Length >= 3 && int.TryParse(args[2], out int parsedEntityBatch)
            ? parsedEntityBatch
            : 1;
        if (observationBatch is < 0 or > 10_000 || entityBatch is < 0 or > 10_000)
        {
            Console.Error.WriteLine("observation-batch and entity-batch must be between 0 and 10000.");
            return 5;
        }

        var processingRepository = new SqlRegisterUzChangeProcessingRepository(connectionString);
        var processor = new RegisterUzChangeProcessor(client, processingRepository, loader);
        RegisterUzChangeProcessingResult registerUzChangeresult = await processor.ProcessAsync(
            observationBatch,
            entityBatch,
            TimeSpan.FromMinutes(15),
            cancellation.Token);
        Console.WriteLine(
            $"Observations: claimed {registerUzChangeresult.ObservationsClaimed}; " +
            $"resolved {registerUzChangeresult.ObservationsResolved}; failed {registerUzChangeresult.ObservationsFailed}");
        Console.WriteLine(
            $"Entities: claimed {registerUzChangeresult.EntitiesClaimed}; " +
            $"refreshed {registerUzChangeresult.EntitiesRefreshed}; failed {registerUzChangeresult.EntitiesFailed}");
        return registerUzChangeresult.ObservationsFailed == 0 && registerUzChangeresult.EntitiesFailed == 0 ? 0 : 1;
    }

    if (isChangeSynchronization)
    {
        if (!DateTimeOffset.TryParse(
                args[1],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTimeOffset sinceOffset))
        {
            Console.Error.WriteLine("initial-since-utc is not a valid ISO 8601 timestamp.");
            return 5;
        }

        int pageSize = ParseOptionalInt(args, 2, 100);
        int maxPages = ParseOptionalInt(args, 3, 1);
        int observationBatch = ParseOptionalInt(args, 4, 100);
        int entityBatch = ParseOptionalInt(args, 5, 10);
        int maxProcessingPasses = ParseOptionalInt(args, 6, 100);
        if (pageSize is < 1 or > 10_000 ||
            maxPages < 1 ||
            observationBatch is < 1 or > 10_000 ||
            entityBatch is < 1 or > 10_000 ||
            maxProcessingPasses < 1)
        {
            Console.Error.WriteLine(
                "page-size and batch sizes must be 1-10000; page and processing-pass limits must be positive.");
            return 5;
        }

        var changeFeedRepository = new SqlRegisterUzChangeFeedRepository(connectionString);
        var processingRepository = new SqlRegisterUzChangeProcessingRepository(connectionString);
        var collector = new RegisterUzChangeFeedCollector(client, changeFeedRepository);
        var processor = new RegisterUzChangeProcessor(client, processingRepository, loader);
        var orchestrator = new RegisterUzChangeOrchestrator(collector, processor);

        RegisterUzChangeOrchestrationResult orchestrationResult = await orchestrator.RunAsync(
            TruncateToSecond(sinceOffset.UtcDateTime),
            pageSize,
            maxPages,
            observationBatch,
            entityBatch,
            maxProcessingPasses,
            TimeSpan.FromMinutes(15),
            cancellation.Token);

        foreach (RegisterUzChangeFeedResult feed in orchestrationResult.Feeds)
        {
            Console.WriteLine(
                $"{feed.ObjectType}: run {feed.SyncRunId}; pages {feed.PagesRetrieved}; " +
                $"IDs {feed.ObservedIdCount}; " +
                (feed.FeedCompleted
                    ? "feed completed"
                    : $"feed paused after ID {feed.ContinueAfterId}"));
        }

        Console.WriteLine(
            $"Feeds: completed {orchestrationResult.CompletedFeedCount}; paused {orchestrationResult.PausedFeedCount}");
        Console.WriteLine(
            $"Processing: passes {orchestrationResult.ProcessingPasses}; " +
            $"observations resolved {orchestrationResult.ObservationsResolved}; " +
            $"entities refreshed {orchestrationResult.EntitiesRefreshed}; " +
            $"failures {orchestrationResult.ObservationFailures + orchestrationResult.EntityFailures}; " +
            (orchestrationResult.WorkDrained
                ? "work drained"
                : orchestrationResult.ProcessingPassLimitReached
                    ? "work remains after pass limit"
                    : "stopped after failure"));

        return orchestrationResult.HasFailures ? 1 : orchestrationResult.ProcessingPassLimitReached ? 6 : 0;
    }

    var result = await loader.LoadByIcoAsync(args[0], cancellation.Token);
    Console.WriteLine($"RegisterUZ load completed for IČO {result.Ico}.");
    Console.WriteLine($"Entity: {result.RegisterUzEntityId}");
    Console.WriteLine($"Financial statements: {result.FinancialStatementCount}");
    Console.WriteLine($"Annual reports: {result.AnnualReportCount}");
    Console.WriteLine($"Financial reports: {result.FinancialReportCount}");
    Console.WriteLine($"Templates: {result.TemplateCount}");
    Console.WriteLine($"Catalog observations: {result.Catalogs.ObservationCount}");
    Console.WriteLine($"Catalog changes: inserted {result.Catalogs.InsertedCount}, " +
                      $"updated {result.Catalogs.UpdatedCount}, removed {result.Catalogs.RemovedCount}");
    Console.WriteLine($"Catalog changes requiring review: {result.Catalogs.ReviewRequiredCount}");
    Console.WriteLine($"Sync run: {result.SyncRunId}");
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("RegisterUZ load was cancelled.");
    return 4;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static DateTime TruncateToSecond(DateTime value) =>
    new(value.Ticks - value.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);

static int ParseOptionalInt(string[] values, int index, int defaultValue) =>
    values.Length > index && int.TryParse(values[index], out int parsed)
        ? parsed
        : defaultValue;
