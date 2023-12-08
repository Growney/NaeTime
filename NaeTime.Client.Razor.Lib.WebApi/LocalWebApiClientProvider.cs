namespace NaeTime.Client.Razor.Lib.Web;
internal class LocalWebApiClientProvider : ILocalApiClientProvider
{
    public bool IsConfigured => throw new NotImplementedException();

    public bool IsAvailable => throw new NotImplementedException();

    public DateTime? LastCommunication => throw new NotImplementedException();

    public IHardwareApiClient? HardwareApiClient => throw new NotImplementedException();

    public IPilotApiClient? PilotApiClient => throw new NotImplementedException();



}
