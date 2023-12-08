namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface ILocalApiClientProvider
{
    public bool IsConfigured { get; }
    public bool IsAvailable { get; }
    public DateTime? LastCommunication { get; }

    public IHardwareApiClient? HardwareApiClient { get; }
    public IPilotApiClient? PilotApiClient { get; }

}
