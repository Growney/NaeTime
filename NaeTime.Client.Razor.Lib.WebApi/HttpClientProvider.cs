namespace NaeTime.Client.Razor.Lib.WebApi;
internal class HttpClientProvider
{
    private Func<Task<HttpClient?>> _clientProvider;

    public HttpClientProvider(Func<Task<HttpClient?>> clientProvider)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
    }

    public Task<HttpClient?> GetHttpClientAsync() => _clientProvider();
}
