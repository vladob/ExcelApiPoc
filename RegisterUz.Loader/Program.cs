using System.IO.Compression;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RegisterUz.Client;
using RegisterUz.Persistence.SqlServer;
using RegisterUz.Sync;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: RegisterUz.Loader <IČO>");
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
