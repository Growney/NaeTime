namespace EventDbLite.Abstractions
{
    public interface ICommandSerializer
    {
        string GetIdentifier(Type eventType);
    }
}
