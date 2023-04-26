using System;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations
{
    public class Command : ICommand
    {
        public Command(uint uIntVal, long longVal, float floatVal, string stringVal, Guid guid)
        {
            UIntVal = uIntVal;
            LongVal = longVal;
            FloatVal = floatVal;
            StringVal = stringVal;
            Guid = guid;
        }

        public uint UIntVal { get; }
        public long LongVal { get; }
        public float FloatVal { get; }
        public string StringVal { get; }
        public Guid Guid { get; }
    }
}
