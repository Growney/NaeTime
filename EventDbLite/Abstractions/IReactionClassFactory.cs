using EventDbLite.Reactions;

namespace EventDbLite.Abstractions;
public interface IReactionClassFactory
{
    ReactionClassContainer<T> Create<T>();
}
