namespace EventDbLite.Exceptions;

public class ConcurrencyException : Exception
{
    public long ExpectedVersion { get; }
    public long ActualVersion { get; }

    public ConcurrencyException(long expectedVersion, long actualVersion)
        : base($"Concurrency conflict: expected version {expectedVersion}, but actual version is {actualVersion}.")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
