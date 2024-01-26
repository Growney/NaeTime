using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Lib.WebApi;
internal class LocalWebApiClientProvider : WebApiClientProviderBase, ILocalApiClientProvider
{
    public string Description => "Local Web Server";
    public IHardwareApiClient HardwareApiClient { get; }
    public IPilotApiClient PilotApiClient { get; }
    public IFlyingSessionApiClient FlyingSessionApiClient { get; }
    public ITrackApiClient TrackApiClient { get; }

    public LocalWebApiClientProvider(LocalWebApiConfiguration configuration) : base(configuration)
    {
        HardwareApiClient = new WebApiHardwareClient(HttpClientProvider);
        PilotApiClient = new WebApiPilotClient(HttpClientProvider);
        FlyingSessionApiClient = new WebApiFlyingSessionClient(HttpClientProvider);
        TrackApiClient = new WebApiTrackClient(HttpClientProvider);
    }

}
