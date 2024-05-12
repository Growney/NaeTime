namespace NaeTime.PubSub;
internal class EventHubRegistration
{
    public EventHubRegistration(Type hubType, EventHubLifetime lifetime, object? instance)
    {
        HubType = hubType ?? throw new ArgumentNullException(nameof(hubType));
        Lifetime = lifetime;
        Instance = instance;
    }

    public Type HubType { get; }
    public EventHubLifetime Lifetime { get; }
    public object? Instance { get; }
}
