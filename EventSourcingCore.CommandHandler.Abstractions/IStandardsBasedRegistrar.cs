using System;
using System.Collections.Generic;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public interface IStandardsBasedRegistrar
    {
        IEnumerable<ICommandHandler> CreateClassHandlers(Type createFor);
        IEnumerable<ICommandHandler> CreateClassHandlers<T>();
        IEnumerable<ICommandHandler> CreateObjectHandlers(object instance);
    }
}