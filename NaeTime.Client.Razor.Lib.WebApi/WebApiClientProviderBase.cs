using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.WebApi.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal abstract class WebApiClientProviderBase : IApiClientProvider
{
    private readonly IWebApiConfiguration _configuration;
    private HttpClient? _httpClient;
    protected HttpClientProvider HttpClientProvider { get; }
    public WebApiClientProviderBase(IWebApiConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        HttpClientProvider = new HttpClientProvider(CreateHttpClient);
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
    public DateTime? LastCommunication => throw new NotImplementedException();

    public Task<bool> IsEnabledAsync(CancellationToken token) => _configuration.IsEnabledAsync();

    public Task<bool> IsValidAsync(CancellationToken token) => _configuration.IsCurrentConfigurationValidAsync();

    public async Task<bool> TryConnectionAsync(CancellationToken token)
    {
        var client = await HttpClientProvider.GetHttpClientAsync();

        if (client == null)
        {
            return false;
        }
        try
        {
            var response = await client.GetAsync(GetTestPath());

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    protected virtual string GetTestPath() => "configuration/available";
}
