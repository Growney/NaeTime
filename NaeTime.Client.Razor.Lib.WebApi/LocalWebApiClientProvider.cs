using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class LocalWebApiClientProvider : ILocalApiClientProvider
{

    public DateTime? LastCommunication => throw new NotImplementedException();

    public IHardwareApiClient HardwareApiClient { get; }
    public IPilotApiClient PilotApiClient { get; }

    private HttpClient? _httpClient;
    private readonly LocalWebApiConfiguration _configuration;
    private readonly HttpClientProvider _httpClientProvider;

    public LocalWebApiClientProvider(LocalWebApiConfiguration configuration)
    {
        _configuration = configuration;
        _httpClientProvider = new HttpClientProvider(CreateHttpClient);

        HardwareApiClient = new WebApiHardwareClient(_httpClientProvider);
        PilotApiClient = new WebApiPilotClient(_httpClientProvider);
    }

    private async Task<HttpClient?> CreateHttpClient()
    {
        var address = await _configuration.GetAddressAsync();

        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        if (_httpClient == null)
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(address)
            };
        }

        return _httpClient;
    }

    public async Task<bool> TryConnectionAsync(CancellationToken token)
    {
        var client = await _httpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            return false;
        }
        try
        {
            var response = await client.GetAsync("configuration/available");

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> IsConfiguredAsync(CancellationToken token) => _configuration.IsCurrentConfigurationValidAsync();
}
