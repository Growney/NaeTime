namespace NaeTime.PubSub.Abstractions;
internal class EventHubRegistration
{
    public EventHubRegistration(Type hubType, HubLifetime lifetime, object? instance)
    {
        HubType = hubType ?? throw new ArgumentNullException(nameof(hubType));
        Lifetime = lifetime;
        Instance = instance;
    }

    public Type HubType { get; }
    public HubLifetime Lifetime { get; }
    public object? Instance { get; }
}
