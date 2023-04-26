using System;

namespace EventSourcingCore.CommandHandler.Abstractions
{
    public abstract class PrecursorAttribute : Attribute
    {
        public abstract ICommandPrecursor GetPrecursor();
    }
}
