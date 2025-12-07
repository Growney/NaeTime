namespace EventDbLite.Reactions.Abstractions;
public interface IReactionClassContainer<T> : IDisposable
    where T : class
{
    public T Instance { get; }
}
