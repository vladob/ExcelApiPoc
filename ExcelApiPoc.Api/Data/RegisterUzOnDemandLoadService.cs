using RegisterUz.Client;
using RegisterUz.Persistence.SqlServer;
using RegisterUz.Sync;

namespace ExcelApiPoc.Api.Data;

public sealed class RegisterUzOnDemandLoadService
{
    private readonly IConfiguration _configuration;

    public RegisterUzOnDemandLoadService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task LoadByIcoAsync(
        string ico,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ico);

        string connectionString =
            _configuration.GetConnectionString("RegisterUz")
            ?? throw new InvalidOperationException(
                "Connection string 'RegisterUz' is not configured.");

        string baseAddress =
            _configuration["RegisterUz:BaseAddress"]
            ?? RegisterUzApiClient.DefaultBaseAddress;

        using var handler = new HttpClientHandler
        {
            AutomaticDecompression =
                System.Net.DecompressionMethods.GZip |
                System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.Brotli
        };

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                baseAddress,
                UriKind.Absolute),
            Timeout = TimeSpan.FromMinutes(5)
        };

        var client =
            new RegisterUzApiClient(httpClient);

        var repository =
            new SqlRegisterUzPackageRepository(
                connectionString);

        var loader =
            new RegisterUzAccountingEntityLoader(
                client,
                repository);

        await loader.LoadByIcoAsync(
            ico,
            cancellationToken);
    }
}
