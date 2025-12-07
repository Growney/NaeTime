using EventDbLite.Reactions.Abstractions;

namespace EventDbLite.Abstractions;
public interface IReactionClassFactory
{
    IReactionClassContainer<T> Create<T>()
        where T : class;
}
