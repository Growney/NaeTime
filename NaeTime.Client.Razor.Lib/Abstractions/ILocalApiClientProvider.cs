namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface ILocalApiClientProvider : IApiClientProvider
{
    public IHardwareApiClient HardwareApiClient { get; }
    public IPilotApiClient PilotApiClient { get; }


}
