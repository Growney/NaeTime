namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface ILocalApiClientProvider : IApiClientProvider
{
    public string Description { get; }

    public IHardwareApiClient HardwareApiClient { get; }
    public IPilotApiClient PilotApiClient { get; }
    public IFlyingSessionApiClient FlyingSessionApiClient { get; }
    public ITrackApiClient TrackApiClient { get; }


}
