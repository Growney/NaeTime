using System;
using System.Runtime.Serialization;

namespace EventSourcingCore.Store.Abstractions
{
    public class ConcurrencyException : Exception
    {
        public long? ExpectedVersion { get; }
        public long? ActualVersion { get; }

        public ConcurrencyException(long? expectedVersion, long? actualVersion)
            : this(expectedVersion, actualVersion, null)
        {
        }
        public ConcurrencyException(long? expectedVersion, long? actualVersion, Exception innerException)
            : base(string.Empty, innerException)
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
        }
    }
}
