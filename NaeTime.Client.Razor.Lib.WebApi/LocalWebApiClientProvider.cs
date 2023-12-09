using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class LocalWebApiClientProvider : WebApiClientProviderBase, ILocalApiClientProvider
{
    public IHardwareApiClient HardwareApiClient { get; }
    public IPilotApiClient PilotApiClient { get; }

    public LocalWebApiClientProvider(LocalWebApiConfiguration configuration) : base(configuration)
    {
        HardwareApiClient = new WebApiHardwareClient(HttpClientProvider);
        PilotApiClient = new WebApiPilotClient(HttpClientProvider);
    }

}
