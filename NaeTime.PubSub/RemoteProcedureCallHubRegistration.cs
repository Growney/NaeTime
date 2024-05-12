namespace NaeTime.PubSub;
internal class RemoteProcedureCallHubRegistration
{
    public RemoteProcedureCallHubRegistration(Type hubType, RemoteProcedureCallHubLifetime lifetime, object? instance)
    {
        HubType = hubType ?? throw new ArgumentNullException(nameof(hubType));
        Lifetime = lifetime;
        Instance = instance;
    }
    public Type HubType { get; }
    public RemoteProcedureCallHubLifetime Lifetime { get; }
    public object? Instance { get; }
}
