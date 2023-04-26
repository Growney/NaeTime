using System.Collections.Generic;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface IGlobalPrecursorRegistry
    {
        IEnumerable<ICommandPrecursor> GetOrderedPrecursors();

        void RegisterClass<T>() where T : ICommandPrecursor;
        void RegisterObject<T>(T instance) where T : ICommandPrecursor;
    }
}
